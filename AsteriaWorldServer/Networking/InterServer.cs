using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using AsteriaLibrary.Messages;
using AsteriaLibrary.Serialization;
using AsteriaLibrary.Shared;
using AsteriaWorldServer.Messages;
using Lidgren.Network;

namespace AsteriaWorldServer.Networking
{
    /// <summary>
    /// Disconnect Handler
    /// </summary>
    /// <param name="ep"></param>
    public delegate void InterDisconnectHandler(IPEndPoint ep);

    /// <summary>
    /// Low level networking handling.
    /// </summary>
    sealed class InterServer : ThreadedComponent
    {
        #region Fields
        private NetServer netServer;
        private string protocolVersion;
        private ServerContext context;
        #endregion

        #region Properties
        protected override string ThreadName
        {
            get { return "InterServer"; }
        }
        #endregion

        #region Constructors
        public InterServer(ServerContext _context, NetServer _netServer)
        {
            this.context = _context;
            this.netServer = _netServer;

            protocolVersion = context.ServerConfig["protocol_version"];
        }
        #endregion

        #region Methods
        protected override void Worker(object parameter)
        {
            Logger.Output(this, "InterServer worker thread starting..");
            netServer.Start();

            // Grab the workerthread and init.
            WorkerThread wt = (WorkerThread)parameter;

            BaseServerMessageSerializer serializer = new BaseServerMessageSerializer();
            if (wt == null)
                throw new InvalidOperationException("InterServer invalid worker parameter.");

            AutoResetEvent waitDataSignal = netServer.MessageReceivedEvent;

            NetConnection sender;
            NetIncomingMessage receivedMsg;

            byte[] bytesIn = new byte[4096];

            Logger.Output(this, "InterServer worker thread: {0} started!", wt.Thread.Name);

            ServerToServerMessageSerializer deserializer = new ServerToServerMessageSerializer();
            ServerToServerMessage msg = null;

            // loop until server stop sand all messages are sent.
            do
            {
                if ((receivedMsg = netServer.ReadMessage()) != null)
                {
                    sender = receivedMsg.SenderConnection;
                    switch (receivedMsg.MessageType)
                    {
                        case NetIncomingMessageType.VerboseDebugMessage:

                            string debug = receivedMsg.ReadString();
                            Logger.Output(this, "VerboseDebugMessage: {0}", debug);
                            break;

                        case NetIncomingMessageType.ConnectionApproval:

                            string remote_protocol_version = receivedMsg.ReadString();
                            if (remote_protocol_version != protocolVersion)
                            {
                                Logger.Output(this, "Server rejected: {0}. Using version: {1}", sender.RemoteEndpoint, remote_protocol_version);
                                sender.Deny("Connection rejected. Login version: " + protocolVersion);
                            }
                            sender.Approve();
                            break;

                        case NetIncomingMessageType.StatusChanged:

                            if (sender.Status == NetConnectionStatus.Connected)
                                Logger.Output(this, "LoginServer connected: {0}", sender.RemoteEndpoint.ToString());
                            else if (sender.Status == NetConnectionStatus.Disconnected)
                                Logger.Output(this, "LoginServer disconnected: {0}", sender.RemoteEndpoint.ToString());
                            break;

                        case NetIncomingMessageType.Data:

                            receivedMsg.ReadBytes(bytesIn, 0, receivedMsg.LengthBytes);
                            msg = deserializer.Deserialize(bytesIn);

                            if (msg != null)
                            {
#if DEBUG
                                Logger.Output(this, "Deserialized message: {0} from: {1}", msg.MessageType, sender.RemoteEndpoint);
#endif
                                msg.Sender = sender;
                                QueueManager.InterServerQueueReadWrite = msg;
                            }
                            break;
                    }
                }
                else
                    waitDataSignal.WaitOne(150);

            } while (wt.IsRunning);

            Logger.Output(this, "InterServer worker thread: {0} exited!", wt.Thread.Name);
        }

        public string[] DumpStats()
        {
            string[] statistics = new string[]{
                "--------------------------------------------------",
                "                STAT SUMMARY",
                "--------------------------------------------------",
                "Bytes received :" + netServer.Statistics.ReceivedBytes,
                "Bytes sent     :" + netServer.Statistics.SentBytes,
                "Packet received:" + netServer.Statistics.ReceivedPackets,
                "Packet sent    :" + netServer.Statistics.SentPackets,
                "--------------------------------------------------"};
            return statistics;
        }
        #endregion
    }
}
