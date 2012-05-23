using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using AsteriaLibrary.Messages;
using AsteriaLibrary.Serialization;
using AsteriaLibrary.Shared;
using Lidgren.Network;

namespace AsteriaLoginServer
{
    class WorldConnection
    {
        #region Fields
        private bool isRunning;
        private Thread clientThread;

        private NetPeerConfiguration config;
        private NetClient client;
        private WorldServerInfo World;
        private string protocolVersion;

        private AutoResetEvent stopSignal = new AutoResetEvent(false);

        private ServerToServerMessageSerializer serializer = new ServerToServerMessageSerializer();
        #endregion

        #region Constructors
        public WorldConnection(NetPeerConfiguration config, WorldServerInfo world, string protocolVersion)
        {
            this.World = world;
            this.config = config;
            this.protocolVersion = protocolVersion;

            client = new NetClient(config);

            clientThread = new Thread(new ThreadStart(Update));
            clientThread.Name = "WSL" + World.Id;
            clientThread.IsBackground = true;
        }
        #endregion

        #region Method
        public void Start()
        {
            isRunning = true;
            clientThread.Start();
        }

        public void Stop()
        {
            isRunning = false;
            stopSignal.Set();
            clientThread.Join(3000);
        }

        public void SendMessage(ServerToServerMessage msg)
        {
            byte[] bytes = serializer.Serialize(msg);
            NetDeliveryMethod method = msg.DeliveryMethod;
            ServerToServerMessage.Free(msg);

            NetOutgoingMessage dispatchBuffer = client.CreateMessage(bytes.Length);
            dispatchBuffer.Write(bytes);
            client.SendMessage(dispatchBuffer, method);
        }

        /// <summary>
        /// Connects to the server, verifies uptime and updates world server info.
        /// </summary>
        private void Update()
        {
            int SleepTime = 30000 + World.Id * 1000;

            // Update world info via inter server connection.
            Logger.Output(this, "Started scanning world server ({0}).", World.Name);

            try
            {
                client.Start();

                NetIncomingMessage receivedMsg = null;
                DateTime waitReply;
                AutoResetEvent waitSignal;

                waitSignal = client.MessageReceivedEvent;

                while (isRunning)
                {
                    if (client.ServerConnection == null || client.ServerConnection.Status != NetConnectionStatus.Connected)
                    {
                        NetOutgoingMessage sendBuffer = client.CreateMessage();
                        sendBuffer.Write(protocolVersion);
                        client.Connect(World.InterAddress, sendBuffer);
                    }
                    else
                    {
                        ServerToServerMessage msg = ServerToServerMessage.CreateMessageSafe();
                        msg.MessageType = MessageType.L2S_GetStatus;
                        msg.DeliveryMethod = NetDeliveryMethod.ReliableUnordered;
                        SendMessage(msg);
                    }

                    waitReply = DateTime.Now;

                    while ((DateTime.Now - waitReply).TotalMilliseconds < 5000)
                    {
                        if ((receivedMsg = client.ReadMessage()) != null)
                        {
                            switch (receivedMsg.MessageType)
                            {
                                case NetIncomingMessageType.StatusChanged:

                                    NetConnectionStatus status = (NetConnectionStatus)receivedMsg.ReadByte();
                                    if (status == NetConnectionStatus.Connected)
                                    {
                                        ServerToServerMessage msg = ServerToServerMessage.CreateMessageSafe();
                                        msg.MessageType = MessageType.L2S_GetStatus;
                                        msg.DeliveryMethod = NetDeliveryMethod.ReliableUnordered;
                                        SendMessage(msg);

                                        Logger.Output(this, "World connected: {0} ({1}).", World.Name, World.InterAddress.ToString());
                                    }
                                    else if (status == NetConnectionStatus.Disconnected)
                                        Logger.Output(this, "World disconnected: {0} ({1}).", World.Name, World.InterAddress.ToString());
                                    break;

                                case NetIncomingMessageType.Data:

                                    byte[] byteBuffer = new byte[receivedMsg.LengthBytes];
                                    receivedMsg.ReadBytes(byteBuffer, 0, receivedMsg.LengthBytes);
                                    ServerToServerMessage msg2 = serializer.Deserialize(byteBuffer);

                                    if (msg2 != null && msg2.Data.Length > 0 && msg2.MessageType == MessageType.S2L_SendStatus)
                                    {
                                        string[] data = msg2.Data.Split(':');
                                        if (data.Length != 8)
                                        {
                                            Logger.Output(this, "Invalid world status response.");
                                            break;
                                        }

                                        lock (World)
                                        {
                                            int conv;
                                            if (int.TryParse(data[0], out conv))
                                                World.Id = conv;

                                            World.Name = data[1];

                                            string ipOrHost = data[2];
                                            IPAddress ipAdd;

                                            if (!IPAddress.TryParse(ipOrHost, out ipAdd))
                                            {
                                                try
                                                {
                                                    IPHostEntry iph = Dns.GetHostEntry(ipOrHost);
                                                    ipAdd = iph.AddressList[0];
                                                }
                                                catch
                                                {
                                                    Logger.Output(this, "Unable to resolve world client address: {0} ({1}).", World.Name, ipOrHost);
                                                    continue;
                                                }
                                            }

                                            if (int.TryParse(data[3], out conv))
                                                World.ClientAddress = new IPEndPoint(ipAdd, conv);

                                            ipOrHost = data[4];
                                            if (!IPAddress.TryParse(ipOrHost, out ipAdd))
                                            {
                                                try
                                                {
                                                    IPHostEntry iph = Dns.GetHostEntry(ipOrHost);
                                                    ipAdd = iph.AddressList[0];
                                                }
                                                catch
                                                {
                                                    Logger.Output(this, "Unable to resolve world inter address: {0} ({1}).", World.Name, ipOrHost);
                                                    continue;
                                                }
                                            }

                                            if (int.TryParse(data[5], out conv))
                                                World.InterAddress = new IPEndPoint(ipAdd, conv);

                                            if (int.TryParse(data[6], out conv))
                                                World.Online = conv;

                                            if (int.TryParse(data[7], out conv))
                                                World.Allowed = conv;

                                            World.IsOnline = DateTime.Now;
                                            Logger.Output(this, "World Info: {0} (I-{1}, C-{2}) Users: {3}, Max: {4}.", World.Name, World.InterAddress.ToString(), World.ClientAddress.ToString(), World.Online, World.Allowed);
                                        }
                                    }
                                    break;
                            }
                        }
                        else
                        {
                            waitSignal.WaitOne(250);
                        }
                    }

                    //Thread.Sleep(SleepTime);
                    stopSignal.WaitOne(SleepTime);
                }

                client.Shutdown("");
            }
            catch (Exception ex)
            {
                Logger.Output(this, "Server exception: {0}, {1}", ex.Message, ex.StackTrace);
            }
            Logger.Output(this, "Stopped scanning world server ({0})", World.Name);
        }
        #endregion
    }
}
