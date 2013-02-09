using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using AsteriaLibrary.Messages;
using AsteriaLibrary.Serialization;
using Lidgren.Network;
using AsteriaLibrary.Shared;

namespace AsteriaLibrary.Client
{
    /// <summary>
    /// Abstract base asteria world server communication component. Handles low level communication with asteria world server.
    /// A client application does not use this class directly, instead the WorldClient or a derived class is used.
    /// Note: this class is not thread safe, actually multiple instances should never be created of this class.
    /// </summary>
    public abstract class WorldClientBase : IDisposable
    {
        #region Fields
        private bool isRunning;
        private bool isConnected;

        private Thread networkLoop;
        private Thread deserializerLoop;

        private Dictionary<MessageType, List<ServerToClientMessage>> messages;

        private Queue<NetIncomingMessage> receivingQ;
        private Queue<ClientToServerMessage> sendingQ;
        private AutoResetEvent newDataArrivedEvent;

        protected NetClient client;

        protected ClientToServerMessageSerializer serializer;
        protected ServerToClientMessageSerializer deserializer;

        private string protocolVersion;

        private const int connection_timeout = 1000;
        #endregion

        #region Properties
        /// <summary>
        /// Returns the connection status.
        /// </summary>
        public bool IsConnected { get { return isConnected; } }
        #endregion

        #region Constructors
        public WorldClientBase(string protocolVersion)
        {
            this.protocolVersion = protocolVersion;

            // Net configuration
            NetPeerConfiguration cfg = new NetPeerConfiguration("Asteria");
            cfg.MaximumConnections = 1;
            cfg.ReceiveBufferSize = 4095;
            cfg.MaximumTransmissionUnit = 4095;
#if DEBUG
            try
            {
                cfg.EnableMessageType(NetIncomingMessageType.VerboseDebugMessage);
            }
            catch
            {
                Logger.Output(this, "Running in debug mode but using release Lidgren library version, Lidgren debug messages disabled!");
            }
#endif
            client = new NetClient(cfg);
            client.Start();

            // Create objects needed.
            sendingQ = new Queue<ClientToServerMessage>(64);
            receivingQ = new Queue<NetIncomingMessage>(128);
            newDataArrivedEvent = new AutoResetEvent(false);

            messages = new Dictionary<MessageType, List<ServerToClientMessage>>();
            serializer = new ClientToServerMessageSerializer();
            deserializer = new ServerToClientMessageSerializer();

            isRunning = true;
        }
        #endregion

        #region Methods

        /// <summary>
        /// Attempts establishing a network connection.
        /// This is a physical connection and no logical data except asteria hail data is sent.
        /// If the asteria client is already connected (even to a different host) ho connection attempt is made and teh function immediately returns false.
        /// Note that this method times out after 5 seconds and returns false if no connection was established.
        /// </summary>
        /// <param name="hostAddress"></param>
        /// <param name="hostPort"></param>
        /// <returns>True if connected else false.</returns>
        public bool Connect(string hostAddress, int hostPort)
        {
            if (isConnected)
                return false;

            IPAddress addr = null;
            if (!IPAddress.TryParse(hostAddress, out addr))
            {
                IPHostEntry he = Dns.GetHostEntry(hostAddress);
                foreach (IPAddress ip in he.AddressList)
                {
                    if (ip.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork)
                    {
                        addr = ip;
                        break;
                    }
                }
            }
            IPEndPoint endPoint = new IPEndPoint(addr, hostPort);

            return Connect(endPoint);
        }

        /// <summary>
        /// Attempts establishing a network connection.
        /// This is a physical connection and no logical data except asteria hail data is sent.
        /// If the asteria client is already connected (even to a different host) ho connection attempt is made and teh function immediately returns false.
        /// Note that this method times out after 5 seconds and returns false if no connection was established.
        /// </summary>
        /// <param name="endPoint"></param>
        /// <returns>True if connected else false.</returns>
        public bool Connect(IPEndPoint endPoint)
        {
            if (isConnected)
                return false;

            // Start receiver worker thread.
            networkLoop = new Thread(new ThreadStart(NetReceiver));
            networkLoop.Name = "NetReceiver";
            networkLoop.IsBackground = true;
            networkLoop.Start();

            // Connect and wait up to 5 seconds for connected.
            NetOutgoingMessage msg = client.CreateMessage();
            msg.Write(protocolVersion);

            client.Connect(endPoint, msg);

            DateTime timeout = DateTime.Now.AddMilliseconds(connection_timeout);

            while (!isConnected && DateTime.Now < timeout)
                Thread.Sleep(500);

            if (isConnected)
            {
                // Start serializer worker thread.
                deserializerLoop = new Thread(new ThreadStart(Deserializer));
                deserializerLoop.Name = "Serializer";
                deserializerLoop.IsBackground = true;
                deserializerLoop.Start();
                // TODO: [NOTE] if the single serializer thread for serializing/deserializing becomes a performance bottleneck,
                // split into multuple serializer and deserializer threads (1 serializer should be always enough while 2 deserializers would easy hit 100 msg/millisecond).
            }
            else
                Disconnect();

            return isConnected;
        }

        /// <summary>
        /// Disconnects the client from the server, empties the message queue and resets internal status.
        /// Note that the OnDisconnect is not invoked from this function since only user code invokes Disconnect.
        /// </summary>
        public void Disconnect()
        {
            client.Disconnect("");
            isConnected = false;
            messages = new Dictionary<MessageType, List<ServerToClientMessage>>();
        }

        public void Stop()
        {
            Disconnect();
            isRunning = false;
            client.Shutdown(null);
        }

        /// <summary>
        /// Sends a message to the connected server. This message will be returned to the message pool after sending so the caller must not free the message.
        /// </summary>
        /// <param name="msg"></param>
        /// <returns></returns>
        public bool SendMessage(ClientToServerMessage msg)
        {
            if (!isConnected)
                return false;

            lock (sendingQ)
            {
                sendingQ.Enqueue(msg);
                newDataArrivedEvent.Set();
            }
            return true;
        }

        /// <summary>
        /// Gets the ServerToClientMessage of the given type if present in the receiving queue.
        /// The message is removed from the internal queue.
        /// </summary>
        /// <param name="messageType"></param>
        /// <returns></returns>
        public ServerToClientMessage GetMessage(MessageType messageType)
        {
            if (!isConnected)
                return null;

            ServerToClientMessage msg = null;
            if (messages.ContainsKey(messageType))
            {
                lock (messages)
                {
                    if (messages[messageType].Count > 0)
                    {
                        msg = messages[messageType][0];
                        messages[messageType].RemoveAt(0);
                    }
                }
            }
            return msg;
        }

        /// <summary>
        /// Gets any ServerToClientMessage type that is in the receiving queue.
        /// TODO: I don't like this function at all, it will change the sort of messages
        /// as some message types are always emptied before others. Need to revisit this and change how messages are received.
        /// </summary>
        /// <returns></returns>
        public MessageType GetQueuedMessageType()
        {
            lock (messages)
            {
                foreach (MessageType check in messages.Keys)
                {
                    if (messages[check].Count > 0)
                    {
                        return check;
                    }
                }
            }
            return MessageType.None;
        }

        /// <summary>
        /// Deletes the ServerToClientMessage of the given type if present in the receiving queue.
        /// </summary>
        /// <param name="messageType"></param>
        /// <returns></returns>
        public bool DeleteMessage(MessageType messageType)
        {
            if (!isConnected)
                return false;

            if (messages.ContainsKey(messageType))
            {
                lock (messages)
                {
                    if (messages[messageType].Count > 0)
                    {
                        messages[messageType].RemoveAt(0);
                        return true;
                    }
                }
            }
            return false;
        }

        /// <summary>
        /// Reads data from the server and stores it in the received messages queue.
        /// </summary>
        private void NetReceiver()
        {
            string data;
            while (isRunning)
            {
                NetIncomingMessage receivedMsg;

                lock (client)
                    receivedMsg = client.ReadMessage();

                if (receivedMsg != null)
                {
                    switch (receivedMsg.MessageType)
                    {
                        case NetIncomingMessageType.VerboseDebugMessage:

                            data = receivedMsg.ReadString();
#if DEBUG
                            Logger.Output(this, "VerboseDebugMessage: {0}", data);
#endif
                            break;

                        case NetIncomingMessageType.StatusChanged:

                            NetConnectionStatus status = (NetConnectionStatus)receivedMsg.ReadByte();
#if DEBUG
                            Logger.Output(this, "StatusChanged: {0}", status.ToString());
#endif
                            if (status == NetConnectionStatus.Connected)
                            {
                                isConnected = true;
                                OnConnected();
                            }
                            else if (status == NetConnectionStatus.Disconnected)
                            {
                                isConnected = false;
                                OnDisconnected();
                            }
                            break;

                        case NetIncomingMessageType.Data:
#if DEBUG
                            Logger.Output(this, "Data arrived bytes: {0}", receivedMsg.LengthBytes.ToString());
#endif
                            lock (receivingQ)
                                receivingQ.Enqueue(receivedMsg);

                            // Signal we have something to process.
                            newDataArrivedEvent.Set();
                            break;
                    }
                }
                else
                    client.MessageReceivedEvent.WaitOne(100);
            }
        }

        /// <summary>
        /// Reads messages from the receiving queue, deserializes them, stores it in the message queue and fires the OnMessageReceived member.
        /// </summary>
        private void Deserializer()
        {
            byte[] byteBuffer;

            while (isRunning)
            {
                // Default to no work but check queue.
                bool hasWork = false;
                NetIncomingMessage receivedMsg = null;

                // Check receiving queue.
                lock (receivingQ)
                {
                    if (receivingQ.Count > 0)
                    {
                        receivedMsg = receivingQ.Dequeue();
                        hasWork = true;
                    }
                }

                // Either do something or block on WaitOne()
                if (hasWork)
                {
                    // Deserialize the message
                    byteBuffer = new byte[receivedMsg.LengthBytes];
                    receivedMsg.ReadBytes(byteBuffer, 0, receivedMsg.LengthBytes);
                    ServerToClientMessage msg = deserializer.Deserialize(byteBuffer);

                    // If message exists add to queue and fire event.
                    if (msg != null)
                    {
                        List<ServerToClientMessage> list;

                        if (msg.MessageType == MessageType.S2C_Container)
                        {
                            ServerToClientMessage[] msgarr = deserializer.Deserialize(msg.Buffer, msg.Code);

                            foreach (ServerToClientMessage onemsg in msgarr)
                            {
                                lock (messages)
                                {
                                    if (!messages.TryGetValue(onemsg.MessageType, out list))
                                    {
                                        list = new List<ServerToClientMessage>();
                                        messages.Add(onemsg.MessageType, list);
                                    }
                                    list.Add(onemsg);
                                }
                                OnMessageReceived(onemsg.MessageType);
                            }
                            ServerToClientMessage.FreeSafe(msg);
                        }
                        else
                        {
                            lock (messages)
                            {
                                if (!messages.TryGetValue(msg.MessageType, out list))
                                {
                                    list = new List<ServerToClientMessage>();
                                    messages.Add(msg.MessageType, list);
                                }
                                list.Add(msg);
                            }
                            OnMessageReceived(msg.MessageType);
                        }
                    }
                }

                // Check sending queue.
                if (!hasWork && sendingQ.Count > 0)
                {
                    hasWork = true;
                    ClientToServerMessage msg = null;

                    lock (sendingQ)
                    {
                        if (sendingQ.Count > 0)
                            msg = sendingQ.Dequeue();
                    }

                    if (msg != null)
                    {
                        if (client.Status == NetPeerStatus.Running)
                        {
                            NetDeliveryMethod how = msg.DeliveryMethod;
                            byteBuffer = serializer.Serialize(msg);
                            BaseClientMessage.FreeSafe(msg);

                            lock (client)
                            {
                                NetOutgoingMessage sendBuffer = client.CreateMessage(byteBuffer.Length);
                                sendBuffer.Write(byteBuffer, 0, byteBuffer.Length);
                                client.SendMessage(sendBuffer, how);
                            }
                        }
                    }
                }

                if (!hasWork)
                    newDataArrivedEvent.WaitOne(100);
            }
        }

        #region Abstract Methods
        protected abstract void OnConnected();
        protected abstract void OnDisconnected();
        protected abstract void OnMessageReceived(MessageType messageType);
        #endregion

        #region IDisposable Members
        public void Dispose()
        {
            isRunning = false;
            if (isConnected)
                Disconnect();

            newDataArrivedEvent.Set();
            if (networkLoop != null)
                networkLoop.Join(500);

            if (deserializerLoop != null)
                deserializerLoop.Join(500);
        }
        #endregion

        #endregion
    }
}
