using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using AsteriaLibrary.Serialization;
using AsteriaLibrary.Shared;
using AsteriaWorldServer.Messages;
using AsteriaWorldServer.PlayerCache;
using Lidgren.Network;

namespace AsteriaWorldServer.Networking
{
    /// <summary>
    /// Low level networking handling based on the lidgren library.
    /// This class contains two unrelated parts: receiver and dispatcher.
    /// </summary>
    sealed class NetworkServer : ThreadedComponent
    {
        #region Fields
        private NetServer netServer;
        private ServerContext context;
        private MasterPlayerTable mpt;
        private string protocolVersion;
        #endregion

        #region Properties
        protected override string ThreadName
        {
            get { return "NetworkServer"; }
        }
        #endregion

        #region Constructors
        /// <summary>
        /// Creates a NetworkServer instance.
        /// </summary>
        /// <param name="context">Server context.</param>
        /// <param name="netServer">A NetServer instance.</param>
        public NetworkServer(ServerContext context, NetServer netServer)
        {
            this.netServer = netServer;
            this.context = context;
            this.mpt = context.Mpt;

            protocolVersion = context.ServerConfig["protocol_version"];
        }
        #endregion

        #region Methods
        /// <summary>
        /// A background thread loop, dequeues QueueManager.WorldMessageQueueReadWrite messages, serializes them and sends to clients.
        /// There are usually multiple threads assigned to execute this method.
        /// </summary>
        /// <param name="parameter"></param>
        protected override void Worker(object parameter)
        {
            Logger.Output(this, "NetworkServer worker thread starting..");
            netServer.Start();

            // Grab the worker thread and init
            WorkerThread wt = (WorkerThread)parameter;

            ServerToClientMessageSerializer serializer = new ServerToClientMessageSerializer();
            if (wt == null)
                throw new InvalidOperationException("NetworkServer invalid worker parameter.");

            NetIncomingMessage receivedMessage;
            NetConnection sender;
            AutoResetEvent waitDataSignal = netServer.MessageReceivedEvent;

            Logger.Output(this, "NetworkServer worker thread: {0} started!", wt.Thread.Name);

            // Loop until the server stops and all messages are sent.
            do
            {
                if ((receivedMessage = netServer.ReadMessage()) != null)
                {
                    sender = receivedMessage.SenderConnection;

                    switch (receivedMessage.MessageType)
                    {
                        case NetIncomingMessageType.VerboseDebugMessage:

                            string data = receivedMessage.ReadString();
                            Logger.Output(this, "VerboseDebugMessage: {0}", data);
                            break;

                        case NetIncomingMessageType.ConnectionApproval:

                            string remote_protocol_version = receivedMessage.ReadString();
                            if (remote_protocol_version != protocolVersion)
                            {
                                Logger.Output(this, "Player rejected: {0}", sender.RemoteEndpoint);
                                sender.Deny("Connection rejected. Login version: " + protocolVersion);
                                break;
                            }

                            if (AcceptPlayerConnection(sender))
                                sender.Approve();
                            else
                                sender.Deny("Connection rejected.");

                            break;

                        case NetIncomingMessageType.StatusChanged:

                            if (sender.Status == NetConnectionStatus.Disconnected)
                            {
                                Logger.Output(this, "Client: {0} disconnected!", sender.RemoteEndpoint.ToString());

                                // A client can disconnect because of multiple reasons like network failures, player disconnects, or server disconnect request.
                                MasterPlayerRecord mpr = context.Mpt.GetByEndPoint(sender.RemoteEndpoint);
                                if (mpr != null)
                                {
                                    if (mpr.State == ClientState.InWorld)
                                    {
                                        mpr.LogoutCharacterRequested = true;
                                        mpr.LogoutClientRequested = true;
                                    }
                                    else
                                    {
                                        mpr.LogoutCharacterRequested = true;
                                        mpr.LogoutCharacterGranted = true;
                                        mpr.LogoutClientRequested = true;
                                    }
                                    mpr.State = ClientState.Disconnecting;
                                }
                            }
                            else
                                Logger.Output(this, "Client: {0}, new status: {1}.", sender.RemoteEndpoint.ToString(), sender.Status);

                            break;

                        case NetIncomingMessageType.Data:

                            QueueManager.NetworkQueueReadWrite = receivedMessage;
                            break;
                    }
                }
                else
                    waitDataSignal.WaitOne(150);

            } while (wt.IsRunning);

            Logger.Output(this, "NetworkServer worker thread: {0} exited!", wt.Thread.Name);
        }

        /// <summary>
        /// Creates a new player object.
        /// </summary>
        /// <param name="conn"></param>
        /// <returns></returns>
        private bool AcceptPlayerConnection(NetConnection conn)
        {
            MasterPlayerRecord mpr = mpt.GetByEndPoint(conn.RemoteEndpoint);

            if (mpr != null)
            {
                // Check if already present but not disconnected.
                if (mpr.State != ClientState.Disconnected)
                {
                    Logger.Output(this, "AcceptPlayerConnection from: {0} request denied since the player: {1} is found in cache with state: {2}.", conn.RemoteEndpoint.Address, mpr.AccountId, mpr.State);
                    mpr.FloodEntry.AddConnectionPrevent(5000);
                    return false;
                }

                // Prevent connection flooding from same IP
                if (!mpr.FloodEntry.IsConnectionAllowed && mpr.FloodEntry.NextActionAllowed > DateTime.Now)
                {
                    Logger.Output(this, "AcceptPlayerConnection from: {0} request denied since the player: {1} is a flooder, next action allowed: {2}.", conn.RemoteEndpoint.Address, mpr.AccountId, mpr.FloodEntry.NextActionAllowed);
                    mpr.FloodEntry.AddConnectionPrevent(5000);
                    return false;
                }
            }
            else
            {
                // Brand new IP, just create the MPR.
                mpr = new MasterPlayerRecord(conn);
                mpt.Add(mpr);
            }

            // Everything is okay.
            mpr.State = ClientState.Handshake;
            mpr.FloodEntry.Reset();

            return true;
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
