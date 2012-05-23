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
    /// Abstract base asteria communication component. Handles low level communication with asteria servers.
    /// Usually a client application does not use this class directly, instead the LoginClient is used to communicate with the login server.
    /// Note: this class is not thread safe.
    /// </summary>
    public abstract class LoginClientBase : IDisposable
    {
        #region Fields
        private Dictionary<MessageType, List<BaseServerMessage>> messages;
        private NetClient client;
        private Thread mainLoop;
        private bool isRunning;
        private bool isConnected;

        protected BaseClientMessageSerializer serializer;
        protected BaseServerMessageSerializer deserializer;

        private string protocalVersion;
        #endregion

        #region Properties
        public bool IsConnected { get { return isConnected; } }
        #endregion

        #region Constructors
        public LoginClientBase(string protocalVersion)
        {
            this.protocalVersion = protocalVersion;

            serializer = new BaseClientMessageSerializer();
            deserializer = new BaseServerMessageSerializer();

            isRunning = true;
            mainLoop = new Thread(new ThreadStart(MessageLoop));
            mainLoop.Name = "MessageLoop";
            mainLoop.IsBackground = true;
            mainLoop.Start();

            // Wait until background thread creates the client.
            while (client == null)
                Thread.Sleep(20);
        }
        #endregion

        #region Methods
        /// <summary>
        /// Attempts establishing a network connection.
        /// This is a physical connection and no logical data except the astera hail data is sent.
        /// If the AsteriaClient is already connected (even to a different host) no connection attempt is made and the funtion immediately returns false.
        /// Note that this method timesout after 5 seconds and returns false if no connection was established.
        /// </summary>
        /// <param name="hostAddress"></param>
        /// <param name="hostPort"></param>
        /// <returns></returns>
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
        /// This is a physical connection and no logical data except the astera hail data is sent.
        /// If the AsteriaClient is already connected (even to a different host) no connection attempt is made and the funtion immediately returns false.
        /// Note that this method timesout after 5 seconds and returns false if no connection was established.
        /// </summary>
        /// <param name="endPoint"></param>
        /// <returns></returns>
        public bool Connect(IPEndPoint endPoint)
        {
            if (isConnected)
                return false;

            NetOutgoingMessage msg = client.CreateMessage();
            msg.Write(protocalVersion);
            client.Connect(endPoint, msg);

            DateTime timeout = DateTime.Now.AddMilliseconds(5000);

            while (!isConnected && DateTime.Now < timeout)
                Thread.Sleep(500);

            if (!isConnected)
                Disconnect();

            return isConnected;
        }

        /// <summary>
        /// Disconnects the client from the server, empties the message queue and resets internal status.
        /// Note that the OnDisconnect is not invoked from this function since onlyuser code invokes Disconnect.
        /// </summary>
        public void Disconnect()
        {
            lock (client)
            {
                isRunning = false;
                client.Disconnect("");
                isConnected = false;

            }
        }

        /// <summary>
        /// Sends an unformatted message to the connected server.
        /// </summary>
        /// <param name="message"></param>
        protected void SendMessage(BaseClientMessage message)
        {
            if (!isConnected)
                throw new InvalidOperationException("Can't send message in disconnected state!");

            byte[] bytes = serializer.Serialize(message);
            lock (client)
            {
                NetOutgoingMessage sendBuffer = client.CreateMessage(bytes.Length);
                sendBuffer.Write(bytes, 0, bytes.Length);
                client.SendMessage(sendBuffer, message.DeliveryMethod);
            }
            BaseClientMessage.Free(message);
        }

        /// <summary>
        /// Gets the message of the given type. The message is removed from the internal queue.
        /// </summary>
        /// <param name="messageType"></param>
        /// <returns></returns>
        public BaseServerMessage GetMessage(MessageType messageType)
        {
            if (!isConnected)
                return null;

            BaseServerMessage msg = null;
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
        /// Waits for a message of the given type and returns the message or null if the timout is reached.
        /// If read, the message is removed from internal queue.
        /// </summary>
        /// <param name="messageType"></param>
        /// <param name="timeoutMilliseconds"></param>
        /// <returns></returns>
        public BaseServerMessage WaitForMessage(MessageType messageType, int timeoutMilliseconds)
        {
            if (!isConnected)
                return null;

            int sleep = 5;
            int sleepFactor = 0;
            BaseServerMessage msg = null;

            DateTime start = DateTime.Now;
            DateTime timeout = start.AddMilliseconds(timeoutMilliseconds);

            while ((DateTime.Now < timeout) && (msg == null))
            {
                msg = GetMessage(messageType);
                if (msg == null)
                {
                    Thread.Sleep(sleep);
                    if (++sleepFactor % 2 == 0)
                    {
                        sleep++;
                        sleepFactor = 0;
                    }
                }
            }

            if (msg == null)
                msg = GetMessage(messageType);

#if DEBUG
            Logger.Output(this, "Waited for {0} message: {1} milliseconds!", messageType, (DateTime.Now - start).TotalMilliseconds);
#endif
            return msg;
        }

        /// <summary>
        /// Main message loop.
        /// Reads data from the server and stores and received message.
        /// </summary>
        private void MessageLoop()
        {
            NetPeerConfiguration cfg = new NetPeerConfiguration("Asteria");
            cfg.MaximumConnections = 1;
            cfg.ReceiveBufferSize = 1024;
            cfg.MaximumTransmissionUnit = 4095;

            client = new NetClient(cfg);
            client.Start();
            messages = new Dictionary<MessageType, List<BaseServerMessage>>();
            NetIncomingMessage receivedMsg;

            while (isRunning)
            {
                lock (client)
                    receivedMsg = client.ReadMessage();

                if (receivedMsg != null)
                {
                    switch (receivedMsg.MessageType)
                    {
                        case NetIncomingMessageType.ConnectionApproval:
#if DEBUG
                            Console.WriteLine("Approval: " + receivedMsg.ReadString());
#endif
                            break;

                        case NetIncomingMessageType.StatusChanged:

                            NetConnectionStatus status = (NetConnectionStatus)receivedMsg.ReadByte();
#if DEBUG
                            Console.WriteLine("Status changed: " + status.ToString());
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

                            byte[] byteBuffer = new byte[receivedMsg.LengthBytes];
                            receivedMsg.ReadBytes(byteBuffer, 0, receivedMsg.LengthBytes);
                            BaseServerMessage msg = deserializer.Deserialize(byteBuffer);
                            if (msg != null)
                            {
                                lock (messages)
                                {
                                    if (!messages.ContainsKey(msg.MessageType))
                                        messages.Add(msg.MessageType, new List<BaseServerMessage>());

                                    messages[msg.MessageType].Add(msg);
                                }
                                OnMessageReceived(msg.MessageType, msg);
                            }
                            break;
                    }
                }
                else
                    client.MessageReceivedEvent.WaitOne(500);
            }
        }

        #region Protected Abstract
        /// <summary>
        /// Invoked after the client gets connected to the server.
        /// </summary>
        protected abstract void OnConnected();

        /// <summary>
        /// Invoked after the client gets disconnected from the server.
        /// </summary>
        protected abstract void OnDisconnected();

        /// <summary>
        /// Invoked after the client receives data from the server.
        /// </summary>
        /// <param name="messageType"></param>
        /// <param name="message"></param>
        protected abstract void OnMessageReceived(MessageType messageType, BaseServerMessage message);
        #endregion

        #region IDisposable Members
        public void Dispose()
        {
            Disconnect();
            Thread.Sleep(50);
            mainLoop.Join(1000);
            messages = null;
            GC.SuppressFinalize(this);
        }
        #endregion

        #endregion
    }
}
