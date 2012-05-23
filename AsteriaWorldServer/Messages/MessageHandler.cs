using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AsteriaLibrary.Entities;
using AsteriaLibrary.Messages;
using AsteriaLibrary.Serialization;
using AsteriaLibrary.Shared;
using AsteriaWorldServer.PlayerCache;
using Lidgren.Network;

namespace AsteriaWorldServer.Messages
{
    /// <summary>
    /// Processes all messages received by the NetworkServer.
    /// It's primary task is to deserialize incoming messages.
    /// The messages also get checked for the ClientState and MessageType combination.
    /// Incoming messages are eventually sorted and queued into corresponding task queues or directly passed to the CSM.
    /// Note that only T2 level threads are executing inside this class instance, there can be any number of T2 level threads at any time executing.
    /// </summary>
    sealed class MessageHandler : ThreadedComponent
    {
        #region Fields
        private NetServer netServer;
        private ServerContext context;
        #endregion

        #region Properties
        protected override string ThreadName
        {
            get { return "MessageHandler"; }
        }
        #endregion

        #region Constructors
        /// <summary>
        /// Creates a new MessageHandler instance.
        /// </summary>
        /// <param name="context"></param>
        /// <param name="netServer"></param>
        public MessageHandler(ServerContext context, NetServer netServer)
        {
            this.netServer = netServer;
            this.context = context;
        }
        #endregion

        #region Methods
        protected override void Worker(object parameter)
        {
            Logger.Output(this, "Worker thread starting..");

            // Grab worker thread and init.
            WorkerThread wt = (WorkerThread)parameter;
            if (wt == null)
                throw new InvalidOperationException("Invalid worker parameter.");

            ClientToServerMessageSerializer deserializer = new ClientToServerMessageSerializer();
            ClientToServerMessage msg = null;

            NetIncomingMessage receivedMsg = null;
            NetConnection sender = null;

            byte[] bytesIn = new byte[netServer.Configuration.ReceiveBufferSize];

            Logger.Output(this, "Worker thread: {0} started!", wt.Thread.Name);

            int waitTime = context.TurnDuration / 2;

            // Loop until the server stops and all messages are handled.
            do
            {
                receivedMsg = QueueManager.NetworkQueueReadWrite;

                if (receivedMsg == null)
                {
                    if (!wt.IsRunning)
                        break;
                    else
                        QueueManager.NetQueueDataArrived.WaitOne(waitTime);
                }
                else
                {
                    // Deserialize the message.
                    sender = (NetConnection)receivedMsg.SenderConnection;

                    if (sender != null)
                    {
                        receivedMsg.ReadBytes(bytesIn, 0, receivedMsg.LengthBytes);

                        msg = deserializer.Deserialize(bytesIn);

                        if (msg != null)
                        {
#if DEBUG
                            MasterPlayerRecord mpr = context.Mpt.GetByEndPoint(sender.RemoteEndpoint);
                            if (mpr.State == ClientState.InWorld)
                                Logger.Output(this, "Deserialized message: {0}->{1} from: {2}.", msg.MessageType, (PlayerAction)msg.Action, sender.RemoteEndpoint);
                            else
                                Logger.Output(this, "Deserialized message: {0}->{1} from: {2}.", msg.MessageType, msg.Action, sender.RemoteEndpoint);
#endif
                            msg.Sender = sender;
                            HandleClientToServerMessage(msg);
                        }
                        else
                            context.Mpt.Disconnect(sender, "Null message!", 5000);
                    }
                    else
                        Logger.Output(this, "Encounted network buffer without an attached sender!");
                }

            } while (true);

            Logger.Output(this, "Worker thread: {0} exited!", wt.Thread.Name);
        }

        /// <summary>
        /// Checks the client state and dipatches the message to various sub handlers.
        /// </summary>
        /// <param name="msg"></param>
        private void HandleClientToServerMessage(ClientToServerMessage msg)
        {
            // First check if the player is in cache at all.
            // Note that the connection must be there since the NetworkServr
            // explicitly asks the MPT to store the player.
            // If it's not there we have a much bigger problem!
            MasterPlayerRecord mpr = context.Mpt.GetByEndPoint(msg.Sender.RemoteEndpoint);
            if (mpr == null)
            {
                string message = String.Format("No MPR for endpoint: {0} exists!", msg.Sender.RemoteEndpoint.ToString());
                Logger.Output(this, message, msg.Sender.RemoteEndpoint.ToString());
                // TODO: [LOW] wouldn't this disconnect fail due to the very reason we just checked for?
                context.Mpt.Disconnect(msg.Sender, message, 5000);
                return;
            }

            // State machine check
            switch (mpr.State)
            {
                case ClientState.InWorld:
                    HandleInWorldMessage(mpr, msg);
                    break;

                case ClientState.Handshake:
                    if (context.Mpt.AuthenticatePlayer(msg))
                    {
                        // Route to character list message.
                        msg.MessageType = MessageType.C2S_GetCharacterList;
                        HandleCharMngtMessage(mpr, msg);
                    }
                    break;

                case ClientState.CharacterManagement:
                    HandleCharMngtMessage(mpr, msg);
                    break;

                case ClientState.Disconnecting:
                case ClientState.Disconnected:
                    Logger.Output(this, "Message from {0}", mpr.State);
                    break;

                case ClientState.Kicked:
                    // TODO: [LOW] implement additional logic.
                    context.Mpt.Disconnect(msg.Sender, "Player kicked!", 5000);
                    break;

                default:
                    Logger.Output(this, "Unexpected client state: {0}, message type: {1}, from: {2}, account: {3}, disconnecting!", mpr.State, msg.MessageType, msg.Sender.RemoteEndpoint, msg.AccountId);
                    context.Mpt.Disconnect(msg.Sender, "Internal server error!", 5000);
                    break;
            }
        }

        /// <summary>
        /// Handles character management state message dispatching to the turn queue or the CharMngtQueue queue.
        /// </summary>
        /// <param name="mpr"></param>
        /// <param name="msg"></param>
        private void HandleCharMngtMessage(MasterPlayerRecord mpr, ClientToServerMessage msg)
        {
            switch (msg.MessageType)
            {
                case MessageType.C2S_PlayerLogoutRequest:
                case MessageType.C2S_CharacterLogoutRequest:
                case MessageType.C2S_GetCharacterList:
                case MessageType.C2S_CreateCharacter:
                case MessageType.C2S_DeleteCharacter:
                case MessageType.C2S_StartCharacter:
                    QueueManager.CharMngtQueueReadWrite = msg;
                    break;
                    
                default:
                    Logger.Output(this, "Unexpected message type: {0} from: {1}, account: {2}, state: {3}, disconnecting!", msg.MessageType, msg.Sender.RemoteEndpoint, msg.AccountId, mpr.State);
                    context.Mpt.Disconnect(msg.Sender, "Invalid message", 5000);
                    break;
            }
        }

        /// <summary>
        /// Handles in world state message dispatching to either the turn queue or the chat queue.
        /// </summary>
        /// <param name="mpr"></param>
        /// <param name="msg"></param>
        private void HandleInWorldMessage(MasterPlayerRecord mpr, ClientToServerMessage msg)
        {
            if (!mpr.FloodEntry.IsActionAllowed())
            {
                Logger.Output(this, "Flood detect (action): account: {0}, character: {1}-{2}, message type: {3}, action: {4}", mpr.AccountId, mpr.CharacterId, (mpr.pCharacter as Character).Name, msg.MessageType, msg.Action);
                return;
            }

            // Sanity check so malicious clients can't mess up other characters.
            if (mpr.CharacterId != msg.CharacterId)
            {
                Logger.Output(this, "InWorld message with invalid characterId received, type: {0}, msg character: {1}, mpr character: {2}, sender: {3}, disconnecting!", msg.MessageType, msg.CharacterId, mpr.CharacterId, msg.Sender.RemoteEndpoint);
                context.Mpt.Disconnect(msg.Sender, "Invalid message", 5000);
                return;
            }

            // Enqueue message
            switch (msg.MessageType)
            {
                case MessageType.C2S_PlayerAction:
                case MessageType.C2S_PlayerLogoutRequest:
                case MessageType.C2S_CharacterLogoutRequest:
                    QueueManager.TurnQueueWrite = msg;
                    break;

                case MessageType.C2S_PlayerChat:
                    QueueManager.ChatQueueReadWrite = msg;
                    break;

                default:
                    Logger.Output(this, "Invalid InWorld message received, type: {0}, characterId: {1}, sender: {2}, disconnecting!", msg.MessageType, msg.CharacterId, msg.Sender.RemoteEndpoint);
                    context.Mpt.Disconnect(msg.Sender, "Invalid message", 1000);
                    break;
            }
        }
        #endregion
    }
}
