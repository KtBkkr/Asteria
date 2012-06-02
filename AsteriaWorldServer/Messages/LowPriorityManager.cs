using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using AsteriaLibrary.Messages;
using AsteriaLibrary.Serialization;
using AsteriaLibrary.Shared;
using AsteriaWorldServer.Messages;
using AsteriaWorldServer.PlayerCache;
using Lidgren.Network;
using AsteriaLibrary.Entities;

namespace AsteriaWorldServer.Messages
{
    /// <summary>
    /// Processes player messages from low priority queues: Server Queue, CharMngt Queue, and Chat Queue.
    /// Only T3 level threads are executing inside this class instance. For a majority of the messages DB
    /// access is needed and a thread can therefore be blocked considerably longer than in other scenerios.
    /// Therefore the number of executing T3 level threads should be carefully tuned. The default is one but
    /// this is hardly optimal for world servers with larger number of players
    /// Note that all messages are already preprocessed at the T2 level, thismeans that the messages are
    /// deserialized and authenticated.
    /// </summary>
    sealed class LowPriorityManager : ThreadedComponent
    {
        #region Fields
        private ServerContext context;
        private DalProvider dal;

        ServerToServerMessageSerializer serializer = new ServerToServerMessageSerializer();
        byte[] bytesOut;
        #endregion

        #region Properties
        protected override string ThreadName
        {
            get { return "LowPrioProc"; }
        }
        #endregion

        #region Constructors
        public LowPriorityManager(ServerContext context, DalProvider dal)
            : base()
        {
            this.context = context;
            this.dal = dal;
        }
        #endregion

        #region Methods
        protected override void Worker(object parameter)
        {
            Logger.Record(this, "Worker thread starting..");
            
            // Grab the workerthread and init
            WorkerThread wt = (WorkerThread)parameter;
            if (wt == null)
                throw new InvalidOperationException("Invalid Worker Parameter.");

            Logger.Output(this, "Worker thread: {0} started!", wt.Thread.Name);

            // WaitHandle the thread waits on
            WaitHandle[] wHandles = new WaitHandle[] {
                QueueManager.ChatQueueDataArrived,
                QueueManager.CharMngtQueueDataArrived,
                QueueManager.InterServerQueueDataArrived };

            ClientToServerMessage msg = null;
            ServerToServerMessage msg2 = null;

            do
            {
                // Check for exit conditions
                if (!wt.IsRunning)
                    break;

                bool isIdle = true;

                // Chat Messages
                msg = QueueManager.ChatQueueReadWrite;
                if (msg != null)
                {
                    HandleChat(msg);
                    isIdle = false;
                }

                // Char Management Messages
                msg = QueueManager.CharMngtQueueReadWrite;
                if (msg != null)
                {
                    HandleCharacterManagement(msg);
                    isIdle = false;
                }

                // Inter Server Messages
                msg2 = QueueManager.InterServerQueueReadWrite;
                if (msg2 != null)
                {
                    HandleInterServerMessage(msg2);
                    isIdle = false;
                }

                if (isIdle)
                    AutoResetEvent.WaitAny(wHandles, 150);

            } while (true);

            Logger.Output(this, "Worker thread: {0} exited!", wt.Thread.Name);
        }

        private void HandleChat(ClientToServerMessage msg)
        {
            Character character = (Character)context.Mpt.GetByCharacterId(msg.CharacterId).pCharacter;
            int channel = Convert.ToInt32(msg.Data.Split('|')[0]);
            int dest = Convert.ToInt32(msg.Data.Split('|')[1]);
            string message = msg.GameData;
            context.GameProcessor.ProcessChatMessage(character, channel, dest, message);
        }

        /// <summary>
        /// Handles character management messages.
        /// Process: GetCharacterList, DeleteCharacter, CreateCharacter, StartCharacter, and PlayerLogoutRequest.
        /// </summary>
        /// <param name="msg"></param>
        private void HandleCharacterManagement(ClientToServerMessage msg)
        {
            ServerToClientMessage wm = null;
            MasterPlayerRecord mpr = context.Mpt.GetByEndPoint(msg.Sender.RemoteEndpoint);
            if (mpr == null)
            {
                context.Mpt.Disconnect(msg.Sender, "No MPR found!", 5000);
                return;
            }

            switch (msg.MessageType)
            {
                case MessageType.C2S_Authenticate:

                    Logger.Output(this, "Unexpected Authenticate message, account: {0}, secret: {1}..", mpr.AccountId, msg.Data);
                    context.Mpt.Disconnect(msg.Sender, "Unexpected message", 15000);
                    break;

                case MessageType.C2S_GetCharacterList:
                    wm = dal.GetAccountCharacterListMessage(mpr);
                    break;

                case MessageType.C2S_DeleteCharacter:
                    wm = dal.DeleteCharacter(msg);
                    break;

                case MessageType.C2S_CreateCharacter:
                    wm = dal.CreateCharacter(mpr.AccountId, msg);
                    break;

                case MessageType.C2S_StartCharacter:
                    ThreadPool.QueueUserWorkItem(new WaitCallback(dal.LoadPlayerCharacter), new int[] { mpr.AccountId, msg.CharacterId });
                    break;

                case MessageType.C2S_PlayerLogoutRequest:
                    // Since we're in management stage we can immediately allow logout.
                    mpr.LogoutCharacterRequested = true;
                    mpr.LogoutCharacterGranted = true;
                    wm = ServerToClientMessage.CreateMessageSafe(msg.Sender);
                    wm.MessageType = MessageType.S2C_PlayerLoggedOut;
                    break;

                case MessageType.C2S_CharacterLogoutRequest:
                    // Don't do anything as char is not logged in.
                    wm = ServerToClientMessage.CreateMessageSafe(msg.Sender);
                    wm.MessageType = MessageType.S2C_LogoutDenied;
                    break;

                default:
                    context.Mpt.Disconnect(msg.Sender, "Unexpected message.", 15000);
                    break;
            }

            if (wm != null)
            {
#if DEBUG
                if(msg.Sender == null)
                    throw (new Exception("Character has null sender, this will crash the MessageSender."));
#endif
                wm.DeliveryMethod = NetDeliveryMethod.ReliableOrdered;
                wm.DeliveryChannel = 0;
                QueueManager.WorldMessageQueueReadWrite = wm;
            }
        }

        /// <summary>
        /// Handles inter server communication.
        /// </summary>
        /// <param name="msg"></param>
        private void HandleInterServerMessage(ServerToServerMessage msg)
        {
            switch (msg.MessageType)
            {
                case (MessageType.L2S_SendOneTimePad):

                    context.Mpt.AddOneTimePad(msg.Data);
                    Logger.Output(this, "Added OTP: {0}", msg.Data);
                    break;

                case (MessageType.L2S_GetStatus):

                    ServerToServerMessage response = ServerToServerMessage.CreateMessageSafe();
                    response.MessageType = MessageType.S2L_SendStatus;

                    StringBuilder sb = new StringBuilder();
                    sb.Append(context.ServerConfig["id"]);
                    sb.Append(":");
                    sb.Append(context.ServerConfig["name"]);
                    sb.Append(":");
                    sb.Append(context.ServerConfig["client_host"]);
                    sb.Append(":");
                    sb.Append(context.ServerConfig["client_port"]);
                    sb.Append(":");
                    sb.Append(context.ServerConfig["inter_host"]);
                    sb.Append(":");
                    sb.Append(context.ServerConfig["inter_port"]);
                    sb.Append(":");
                    sb.Append(context.Mpt.Count.ToString());
                    sb.Append(":");
                    sb.Append(context.ServerConfig["users_allowed"]);

                    response.Data = sb.ToString();

                    bytesOut = serializer.Serialize(response);

                    NetOutgoingMessage sendBuffer = msg.Sender.Peer.CreateMessage(bytesOut.Length);
                    sendBuffer.Write(bytesOut);

                    try
                    {
                        msg.Sender.SendMessage(sendBuffer, NetDeliveryMethod.ReliableOrdered, 0);
                    }
                    catch (Exception ex)
                    {
                        Logger.Output(this, "SendMessage exception: {0}, client status: {1}", ex.Message, msg.Sender.Status);
                    }
                    break;
            }
        }
        #endregion
    }
}
