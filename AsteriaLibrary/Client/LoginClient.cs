using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AsteriaLibrary.Messages;
using AsteriaLibrary.Shared;
using Lidgren.Network;

namespace AsteriaLibrary.Client
{
    public delegate void LoginClientEvent();
    public delegate void LoginClientMsgEvent(MessageType messageType, BaseServerMessage message);

    /// <summary>
    /// Handles login into an asteria login server.
    /// After a connection is established the account login is attempted.
    /// On success the list of world servers is retreived from the login server.
    /// </summary>
    public sealed class LoginClient : LoginClientBase
    {
        #region Fields
        public event LoginClientEvent LoginConnected;
        public event LoginClientEvent LoginDisconnected;
        public event LoginClientMsgEvent LoginMessageReceived;

        private int accountId = -1;
        private List<WorldServerInfo> worldServers;
        private int timeoutMilliseconds;
        #endregion

        #region Properties
        /// <summary>
        /// Returns a list of world servers.
        /// Note this list is empty until a successful login.
        /// </summary>
        public List<WorldServerInfo> WorldServers
        {
            get { return worldServers; }
        }

        /// <summary>
        /// Gets the account ID as returned from the login server.
        /// </summary>
        public int AccountId
        {
            get { return accountId; }
        }
        #endregion

        #region Constructors
        public LoginClient(string protocolVersion) : this(5, protocolVersion) { }

        public LoginClient(int timeoutSeconds, string protocolVersion)
            : base(protocolVersion)
        {
            this.timeoutMilliseconds = timeoutSeconds * 1000;
        }
        #endregion

        #region Methods
        /// <summary>
        /// Tries a login with the current account name and password. On success the world server list is filled.
        /// </summary>
        /// <param name="accountName"></param>
        /// <param name="password"></param>
        /// <returns></returns>
        public bool Login(string accountName, string password)
        {
            if (!IsConnected)
                throw new InvalidOperationException("Can't login in disconnected state!");

            worldServers = new List<WorldServerInfo>();

            // Send message: Login
            BaseClientMessage message = BaseClientMessage.CreateMessage();
            message.MessageType = MessageType.C2L_Login;
            message.AccountId = accountId;
            message.Data = accountName.PadRight(20).Substring(0, 20) + LoginClient.Sha512Encrypt(password);

            SendMessage(message);

            BaseServerMessage msg = WaitForMessage(MessageType.L2C_LoginResponse, timeoutMilliseconds);

            if (msg != null && msg.Data.Length > 1)
            {
                this.accountId = msg.Code;
                string[] worlds = msg.Data.Split(new char[] { ';' }, StringSplitOptions.RemoveEmptyEntries);
                foreach (string s in worlds)
                    worldServers.Add(s);

                return true;
            }
            return false;
        }

        public string SelectWorld(WorldServerInfo worldServer)
        {
            if (!IsConnected)
            {
                return null;
                //throw new InvalidOperationException("Can't connect to world in disconnected state!");
            }

            // Send message: SelectWorld
            BaseClientMessage message = BaseClientMessage.CreateMessage();
            message.MessageType = MessageType.C2L_SelectWorld;
            message.AccountId = accountId;
            message.Data = worldServer.Id.ToString();
            message.DeliveryMethod = NetDeliveryMethod.ReliableOrdered;
            SendMessage(message);

            string secretServerKey = "";
            BaseServerMessage msg = WaitForMessage(MessageType.L2C_WorldSelectResponse, timeoutMilliseconds);

            if (msg != null && msg.Data.Length > 0)
                secretServerKey = msg.Data;

            Disconnect();

            return secretServerKey;
        }

        /// <summary>
        /// Encrypts a password using the SHA-512 hashing.
        /// </summary>
        /// <param name="password">The password to get the hash of.</param>
        /// <returns>The hashed password.</returns>
        private static string Sha512Encrypt(string password)
        {
            var encoder = new UTF8Encoding();
            var sha512hasher = new System.Security.Cryptography.SHA512Managed();
            byte[] hashedDataBytes = sha512hasher.ComputeHash(encoder.GetBytes(password));

            var output = new StringBuilder();
            for (int i = 0; i < hashedDataBytes.Length; i++)
                output.Append(hashedDataBytes[i].ToString("X2"));

            return output.ToString();
        }

        #region Overrides
        protected override void OnConnected()
        {
#if DEBUG
            Console.WriteLine("OnConnected");
#endif
            if (LoginConnected != null)
                LoginConnected();
        }

        protected override void OnDisconnected()
        {
#if DEBUG
            Console.WriteLine("OnDisconnected");
#endif
            if (LoginDisconnected != null)
                LoginDisconnected();
        }

        protected override void OnMessageReceived(MessageType messageType, BaseServerMessage message)
        {
#if DEBUG
            Console.WriteLine("OnMessageReceived: {0}", messageType);
#endif
            if (LoginMessageReceived != null)
                LoginMessageReceived(messageType, message);
        }
        #endregion

        #endregion
    }
}
