using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AsteriaLibrary.Entities;
using AsteriaLibrary.Messages;
using Lidgren.Network;
using AsteriaLibrary.Shared;

namespace AsteriaLibrary.Client
{
    /// <summary>
    /// Wrapper around WorldClient to streamline the client connecting process.
    /// The logical order of invoking methods is:
    /// ConnectToWorld() to establish a connection to the world server,
    /// CharacterList to get a list of characters,
    /// optionally Add/Delete() for character management,
    /// CharacterStart() to start playing in a world.
    /// </summary>
    public class WorldConnection : IDisposable
    {
        public enum WorldConnectionState
        {
            /// <summary>
            /// NotConnected - initial state.
            /// </summary>
            NotConnected,

            /// <summary>
            /// Connect message sent, awaiting reply.
            /// </summary>
            Connecting,

            /// <summary>
            /// Connected to the world server.
            /// </summary>
            Connected,

            /// <summary>
            /// Authenticating to the world server.
            /// </summary>
            Authenticating,

            /// <summary>
            /// Connected to the world server and authenticated (obtained character list)
            /// </summary>
            CharacterManagement,

            /// <summary>
            /// Start char message sent, awaiting reply.
            /// </summary>
            Starting,

            /// <summary>
            /// Connected to the world server and playing a character.
            /// </summary>
            InGame,

            /// <summary>
            /// Disconnected - communication failure.
            /// </summary>
            Disconnected,
        }

        public delegate void StateChangeHandler(WorldConnectionState state);

        #region Fields
        public event StateChangeHandler StateChanged;

        public event WorldClientMsgEvent WorldMessageReceived;
        public event WorldClientMsgEvent CharManagementMessageReceived;

        protected WorldClient worldClient;
        private WorldConnectionState state = WorldConnectionState.NotConnected;

        private List<Character> characterList = null;
        private string host;
        private int port;
        private string protocolVersion;
        private string secretkey;
        private int accountId = -1;
        private int characterId = -1;
        private string disconnectMessage;
        private bool isDispatching;
        #endregion

        #region Properties
        public string DisconnectMessage { get { return disconnectMessage; } }

        /// <summary>
        /// Returns the accountId after successful auth.
        /// </summary>
        public int AccountId
        {
            get { return accountId; }
            set { accountId = value; }
        }

        /// <summary>
        /// Returns the characterId after successful start.
        /// </summary>
        public int CharacterId
        {
            get { return characterId; }
            set { characterId = value; }
        }

        /// <summary>
        /// Returns the current connection state.
        /// </summary>
        public WorldConnectionState State
        {
            get { return state; }
            set
            {
                state = value;

                if (StateChanged != null)
                    StateChanged(state);
            }
        }

        /// <summary>
        /// Returns the WorldClient instance. This property returns the WorldClient only if the client is in a InGame state.
        /// </summary>
        public WorldClient WorldClient
        {
            get
            {
                if (state == WorldConnectionState.InGame)
                    return worldClient;
                else
                    return null;
            }
        }

        /// <summary>
        /// Returns the last retreived character data.
        /// </summary>
        public List<Character> CharacterList { get { return characterList; } }
        #endregion

        #region Constructors
        public WorldConnection(string host, int port, string protocolVersion)
        {
            this.host = host;
            this.port = port;
            this.protocolVersion = protocolVersion;
        }
        #endregion

        #region Methods
        /// <summary>
        /// Connects to the world server.
        /// TODO: [LOW] make the connection process more aware.
        /// </summary>
        /// <param name="secretkey"></param>
        /// <returns></returns>
        public bool ConnectToWorld(string secretkey)
        {
            this.secretkey = secretkey;

            // TODO: [LOW] find a simpler way to set the account id.

            worldClient = new WorldClient(5, protocolVersion);
            worldClient.AccountId = this.accountId;

            worldClient.WorldConnected += new WorldClientEvent(HandleConnectEvent);
            worldClient.WorldDisconnected += new WorldClientEvent(HandleDisconnectEvent);
            worldClient.WorldMessageReceived += new WorldClientMsgEvent(HandleMessageReceivedEvent);

            State = WorldConnectionState.Connecting;

            if (worldClient.Connect(host, port))
            {
                if (worldClient.Authenticate(secretkey))
                {
                    State = WorldConnectionState.Authenticating;
                    return true;
                }
                else
                {
                    disconnectMessage = "Unable to authenticate.";
                    State = WorldConnectionState.Disconnected;
                    worldClient.Dispose();
                }
            }
            else
            {
                disconnectMessage = "Unable to connect.";
                State = WorldConnectionState.Disconnected;
                worldClient.Dispose();
            }
            return false;
        }

        /// <summary>
        /// Creates a new character on the world server.
        /// </summary>
        /// <param name="GameData"></param>
        /// <returns>True if sent.</returns>
        public bool CharacterAdd(string GameData)
        {
            if (state != WorldConnectionState.CharacterManagement)
                return false;

            // Prepare char add request message.
            ClientToServerMessage pm = ClientToServerMessage.CreateMessageSafe();
            pm.MessageType = MessageType.C2S_CreateCharacter;
            pm.AccountId = worldClient.AccountId;
            pm.GameData = GameData;
            pm.DeliveryMethod = NetDeliveryMethod.ReliableUnordered;

            return worldClient.SendMessage(pm);
        }

        /// <summary>
        /// Deletes a character from the world server.
        /// Note tht this operation is irreversible unless the WSE takes special measures to backup everything.
        /// </summary>
        /// <param name="characterId"></param>
        /// <returns></returns>
        public bool CharacterDelete(int characterId)
        {
            if (state != WorldConnectionState.CharacterManagement)
                return false;

            ClientToServerMessage pm = ClientToServerMessage.CreateMessageSafe();
            pm.MessageType = MessageType.C2S_DeleteCharacter;
            pm.AccountId = worldClient.AccountId;
            pm.CharacterId = characterId;
            pm.DeliveryMethod = NetDeliveryMethod.ReliableUnordered;

            return worldClient.SendMessage(pm);
        }

        /// <summary>
        /// Starts the chosen character.
        /// </summary>
        /// <param name="characterId"></param>
        /// <returns></returns>
        public bool CharacterStart(int characterId)
        {
            if (state != WorldConnectionState.CharacterManagement)
                return false;

            State = WorldConnectionState.Starting;

            ClientToServerMessage pm = ClientToServerMessage.CreateMessageSafe();
            pm.MessageType = MessageType.C2S_StartCharacter;
            pm.AccountId = worldClient.AccountId;
            pm.CharacterId = characterId;
            pm.DeliveryMethod = NetDeliveryMethod.ReliableUnordered;

            return worldClient.SendMessage(pm);
        }

        /// <summary>
        /// Returns a message from the queue of the selected type.
        /// </summary>
        /// <param name="messageType"></param>
        /// <returns></returns>
        public ServerToClientMessage GetMessage(MessageType messageType)
        {
            return worldClient.GetMessage(messageType);
        }

        /// <summary>
        /// If there are any messages queued up this will return a messageType that is stacked.
        /// </summary>
        /// <returns></returns>
        public MessageType GetQueuedMessageType()
        {
            return worldClient.GetQueuedMessageType();
        }

        /// <summary>
        /// Starts to dispatch incoming messages through the events.
        /// </summary>
        public void StartReceiving()
        {
            isDispatching = true;
            MessageType type;

            while ((type = GetQueuedMessageType()) != MessageType.None)
                HandleMessageReceivedEvent(type);
        }

        /// <summary>
        /// Stops dispatching messages.
        /// </summary>
        public void StopReceiving()
        {
            isDispatching = false;
        }

        private void HandleConnectEvent()
        {
            State = WorldConnectionState.Connected;
        }

        private void HandleDisconnectEvent()
        {
            State = WorldConnectionState.Disconnected;

            characterList = null;

            if (worldClient != null)
            {
                worldClient.WorldDisconnected -= HandleDisconnectEvent;
                worldClient.Dispose();
            }
        }

        private void HandleMessageReceivedEvent(MessageType messageType)
        {
#if DEBUG
            Logger.Output(this, "HandleMessageReceivedEvent() {0}", messageType);
#endif
            if (messageType == MessageType.S2C_CharacterLoggedOut || messageType == MessageType.S2C_PlayerLoggedOut)
            {
                worldClient.DeleteMessage(messageType);
                StopReceiving();
                State = WorldConnectionState.CharacterManagement;
            }
            else if (messageType == MessageType.S2C_CharacterList)
            {
                ParseCharacterListString();
                AccountId = worldClient.AccountId;

                if (State == WorldConnectionState.Authenticating)
                    State = WorldConnectionState.CharacterManagement;

                if(CharManagementMessageReceived != null)
                    CharManagementMessageReceived(messageType);
            }
            else if (messageType == MessageType.S2C_CreateSuccess || messageType == MessageType.S2C_DeleteSuccess)
            {
                worldClient.GetCharacterList();

                if (CharManagementMessageReceived != null)
                    CharManagementMessageReceived(messageType);
            }
            else if (messageType == MessageType.S2C_CreateFailed || messageType == MessageType.S2C_DeleteFailed)
            {
                if (CharManagementMessageReceived != null)
                    CharManagementMessageReceived(messageType);
            }
            else if (messageType == MessageType.S2C_StartSuccess || messageType == MessageType.S2C_StartFailed)
            {
                if (messageType == MessageType.S2C_StartSuccess)
                {
                    CharacterId = worldClient.CharacterId;
                    State = WorldConnectionState.InGame;
                    StartReceiving();
                }
                else
                    State = WorldConnectionState.CharacterManagement;

                if (CharManagementMessageReceived != null)
                    CharManagementMessageReceived(messageType);
            }
            else
            {
                if (isDispatching && WorldMessageReceived != null)
                    WorldMessageReceived(messageType);
            }
        }

        /// <summary>
        /// Parses the char list string and fills the character list.
        /// </summary>
        /// <returns></returns>
        private bool ParseCharacterListString()
        {
            if (characterList != null)
                characterList.Clear();
            else
                characterList = new List<Character>();

            if (worldClient.CharacterList != null && worldClient.CharacterList.Length > 0)
            {
                string[] chars = worldClient.CharacterList.Split('|');

                if (chars.Length == 0)
                    return true;

                foreach (string achar in chars)
                    characterList.Add(new Character(achar));

                return true;
            }
            return false;
        }

        #region IDisposable Members
        public void Dispose()
        {
            try
            {
                if (worldClient != null)
                    worldClient.Dispose();

                worldClient = null;
            }
            finally
            {
                State = WorldConnectionState.Disconnected;
            }
        }
        #endregion

        #endregion
    }
}
