using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AsteriaLibrary.Entities;
using AsteriaLibrary.Math;
using AsteriaLibrary.Messages;
using AsteriaLibrary.Serialization;
using AsteriaLibrary.Shared;
using AsteriaWorldServer.PlayerCache;
using Lidgren.Network;

namespace AsteriaWorldServer.Messages
{
    /// <summary>
    /// Sends messages stored in the QueueMaager.WorldMessageQueueReadWrite to connected clients.
    /// Note that only T4 level threads are executing inside this class instance, there can be any number of T4 level threads at any time executing.
    /// </summary>
    sealed class MessageSender : ThreadedComponent
    {
        #region Fields
        private NetServer netServer;
        private ServerContext context;
        #endregion

        #region Properties
        protected override string ThreadName
        {
            get { return "MessageSender"; }
        }
        #endregion

        #region Constructors
        /// <summary>
        /// Creates a new MessageHandler instance.
        /// </summary>
        /// <param name="context"></param>
        /// <param name="netServer"></param>
        public MessageSender(ServerContext context, NetServer netServer)
        {
            this.netServer = netServer;
            this.context = context;
        }
        #endregion

        #region Methods
        protected override void Worker(object parameter)
        {
            Logger.Output(this, "Worker thread starting..");

            // Grab the worker thread and init.
            WorkerThread wt = (WorkerThread)parameter;
            if (wt == null)
                throw new InvalidOperationException("Invalid worker parameter.");

            ServerToClientMessageSerializer serializer = new ServerToClientMessageSerializer();
            byte[] bytesOut;

            Logger.Output(this, "Worker thread: {0} started!", wt.Thread.Name);

            int waitTime = context.TurnDuration / 2;

            // Loop until the server stops and all messages are sent.
            do
            {
                // Get outgoing message, serialize and send.
                ServerToClientMessage wm;
                wm = QueueManager.WorldMessageQueueReadWrite;

                if (wm == null)
                {
                    if (!wt.IsRunning)
                        break;
                    else
                        QueueManager.WorldMessageDataArrived.WaitOne(waitTime);
                }
                else
                {
                    try
                    {
                        // Check clients connection state, serialize and send the message.
                        if (wm.Sender.Status == NetConnectionStatus.Connected)
                        {
                            bytesOut = serializer.Serialize(wm);
                            NetOutgoingMessage sendBuffer = netServer.CreateMessage(bytesOut.Length);
                            sendBuffer.Write(bytesOut);

                            try
                            {
                                wm.Sender.SendMessage(sendBuffer, wm.DeliveryMethod, wm.DeliveryChannel);
                            }
                            catch (Exception ex)
                            {
                                Logger.Output(this, "SendMessage exception: {0}, client status: {1}", ex.Message, wm.Sender.Status);
                            }
#if DEBUG
                            MasterPlayerRecord mpr = context.Mpt.GetByEndPoint(wm.Sender.RemoteEndpoint);
                            Character destCharacter = (Character)mpr.pCharacter;

                            if (wm.MessageType == MessageType.S2C_ZoneMessage)
                            {
                                PlayerAction pa = (PlayerAction)wm.Code;
                                if (pa == PlayerAction.AddEntity)
                                {
                                    Entity e = null;
                                    if (wm.Buffer[4] == 0)
                                        e = new Entity(wm.Data);
                                    else
                                        e = new Character(wm.Data);

                                    Logger.Output(this, "Turn {0}: Sent ZoneMessage ({1}) to {2} -> Entity {3}, Position: {4}, Zone: {5}", wm.TurnNumber, pa, destCharacter.Name, e.Id, e.Position, e.CurrentZone);
                                }
                                else if (pa == PlayerAction.MoveEntity)
                                {
                                    int entityId = BitConverter.ToInt32(wm.Buffer, 0);
                                    Point pos = Point.FromBuffer(ref wm.Buffer, 4);

                                    Logger.Output(this, "Turn {0}: Sent ZoneMessage ({1}) to {2} -> Entity {3}, Position: {4}", wm.TurnNumber, pa, destCharacter.Name, entityId, pos.ToString());
                                }
                                else if (pa == PlayerAction.RemoveEntity)
                                {
                                    int entityId = BitConverter.ToInt32(wm.Buffer, 0);
                                    Logger.Output(this, "Turn {0}: SentZoneMessage ({1}) to {2} -> Entity {3}", wm.TurnNumber, pa, destCharacter.Name, entityId);
                                }
                                else if (pa == PlayerAction.AddZone)
                                {
                                    string zones = "";
                                    string[] data = wm.Data.Split(new string[] { "#Z" }, StringSplitOptions.RemoveEmptyEntries);

                                    foreach (string entry in data)
                                        zones = zones + entry.Substring(0, entry.IndexOf(':')) + ":";

                                    Logger.Output(this, "Turn {0}: Sent ZoneMessage ({1}) to {2} -> Zones: {3}", wm.TurnNumber, pa, destCharacter.Name, zones);
                                }
                                else if (pa == PlayerAction.RemoveZone)
                                {
                                    Logger.Output(this, "Turn {0}: Sent ZoneMessage ({1}) to {2} -> Zones: {3}", wm.TurnNumber, pa, destCharacter.Name, wm.Data);
                                }
                                else if (pa == PlayerAction.InvalidAction)
                                {
                                    string reason = "Unknown";
                                    if (wm.Data != null && wm.Data.Length > 0)
                                        reason = wm.Data;

                                    Logger.Output(this, "Turn {0}: Sent ZoneMessage ({1}) to {2} -> Reason: {3}", wm.TurnNumber, pa, destCharacter.Name, reason);
                                }
                                else
                                    Logger.Output(this, "Turn {0}: Sent ZoneMessage ({1}) to {2} -> Delivery: {3}-{4}", wm.TurnNumber, pa, destCharacter.Name, wm.DeliveryMethod, wm.DeliveryChannel);
                            }
                            else if (wm.MessageType == MessageType.S2C_Container)
                                Logger.Output(this, "Turn {0}: Sent container message to {1}", wm.TurnNumber, wm.Sender.RemoteEndpoint);
                            else
                                Logger.Output(this, "Turn {0}: Sent message: {1} to {2}", wm.TurnNumber, wm.MessageType, wm.Sender.RemoteEndpoint);
#endif
                        }
                    }
                    catch (Exception ex)
                    {
                        Logger.Output(this, "Outgoing message handling exception: {0}, stack trace: {1}", ex.Message, ex.StackTrace);
                    }
                    finally
                    {
                        ServerToClientMessage.FreeSafe(wm);
                    }
                }

            } while (true);

            Logger.Output(this, "Worker thread: {0} exited!", wt.Thread.Name);
        }
        #endregion
    }
}
