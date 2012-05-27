using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AsteriaLibrary.Messages;
using Lidgren.Network;
using AsteriaLibrary.Shared;

namespace AsteriaLibrary.Client
{
    public delegate void WorldClientEvent();
    public delegate void WorldClientMsgEvent(MessageType messageType);

    /// <summary>
    /// Basic communication component, handles all communication towards an asteria world server.
    /// </summary>
    public class WorldClient : WorldClientBase
    {
        #region Fields
        public event WorldClientEvent WorldConnected;
        public event WorldClientEvent WorldDisconnected;
        public event WorldClientMsgEvent WorldMessageReceived;

        private int accountId = -1;
        private int characterId = -1;
        private string characterList = null;
        private int timeoutMilliseconds;
        #endregion

        #region Properties
        /// <summary>
        /// Returns a string that contains characters, done by the WSE
        /// Note this list is null until a successful auth.
        /// </summary>
        public string CharacterList { get { return characterList; } }

        /// <summary>
        /// Gets the account id as returned from the world server.
        /// </summary>
        public int AccountId
        {
            get { return accountId; }
            set { accountId = value; }
        }

        /// <summary>
        /// Gets the character id after selection.
        /// </summary>
        public int CharacterId
        {
            get { return characterId; }
            set { characterId = value; }
        }

        /// <summary>
        /// ServerToClientMessage received from server on StartCharacter.
        /// </summary>
        public string OwnCharacter { get; set; }
        #endregion

        #region Constructors
        public WorldClient(string protocolVersion) : this(5, protocolVersion) { }

        public WorldClient(int timeoutSeconds, string protocolVersion)
            : base(protocolVersion)
        {
            this.timeoutMilliseconds = timeoutSeconds * 1000;
        }
        #endregion

        #region Methods

        /// <summary>
        /// Authenticates a connection to the world server
        /// </summary>
        /// <param name="secretkey"></param>
        /// <returns>True if sent.</returns>
        public bool Authenticate(string secretkey)
        {
            if (!IsConnected)
                throw new InvalidOperationException("Can't connect to world in disconnected state!");

            characterList = null;

            ClientToServerMessage pm = ClientToServerMessage.CreateMessageSafe();
            pm.AccountId = accountId;
            pm.MessageType = MessageType.C2S_Authenticate;
            pm.Data = secretkey;
            pm.DeliveryMethod = NetDeliveryMethod.ReliableOrdered;

            return SendMessage(pm);
        }

        /// <summary>
        /// Requests a character list.
        /// </summary>
        /// <returns>True if sent.</returns>
        public bool GetCharacterList()
        {
            characterList = null;

            ClientToServerMessage pm = ClientToServerMessage.CreateMessageSafe();
            pm.AccountId = AccountId;
            pm.MessageType = MessageType.C2S_GetCharacterList;
            pm.DeliveryMethod = NetDeliveryMethod.ReliableOrdered;

            return SendMessage(pm);
        }

        #region Overrides
        protected override void OnConnected()
        {
#if DEBUG
            Logger.Output(this, "OnConnected");
#endif
            if (WorldConnected != null)
                WorldConnected();
        }

        protected override void OnDisconnected()
        {
#if DEBUG
            Logger.Output(this, "OnDisconnected");
#endif
            if (WorldDisconnected != null)
                WorldDisconnected();
        }

        protected override void OnMessageReceived(MessageType messageType)
        {
            if (messageType == MessageType.S2C_CharacterList)
            {
                ServerToClientMessage msg = GetMessage(MessageType.S2C_CharacterList);
                if (msg != null)
                {
                    accountId = msg.Code;
                    characterList = msg.Data;
                }
                ServerToClientMessage.FreeSafe(msg);
            }
            else if (messageType == MessageType.S2C_StartSuccess)
            {
                ServerToClientMessage msg = GetMessage(MessageType.S2C_StartSuccess);
                if (msg != null)
                {
                    characterId = msg.Code;
                    OwnCharacter = msg.Data;
                }
                ServerToClientMessage.FreeSafe(msg);
            }

            if (WorldMessageReceived != null)
                WorldMessageReceived(messageType);
        }
        #endregion

        #endregion
    }
}
