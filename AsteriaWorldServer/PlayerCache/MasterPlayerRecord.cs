using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using Lidgren.Network;

namespace AsteriaWorldServer.PlayerCache
{
    public enum ClientState
    {
        /// <summary>
        /// The client is just connected but not logged in.
        /// This state is only valid during the first client message towards the world server where the message type must be Authenticate.
        /// </summary>
        Handshake,

        /// <summary>
        /// The client is an Asteria client and logged in with it's account.
        /// </summary>
        CharacterManagement,

        /// <summary>
        /// The client is running a player character.
        /// </summary>
        InWorld,

        /// <summary>
        /// The client is kicked.
        /// </summary>
        Kicked,

        /// <summary>
        /// The client is logging out of the character and dropping back to character management.
        /// </summary>
        CharacterLoggingOut,

        /// <summary>
        /// The client is disconnecting and some resources associated with the clients connection/account/character might still be in use.
        /// No re-connection are allowed for that client until the state gets changed to Disconnected.
        /// </summary>
        Disconnecting,

        /// <summary>
        /// The client is disconnected, it's character state must be saved (if in game) and resources cleared.
        /// </summary>
        Disconnected
    }

    /// <summary>
    /// An MPR instance holds information about a single player instance.
    /// </summary>
    public class MasterPlayerRecord
    {
        #region Fields
        private FloodEntry floodEntry;
        private int accountId;
        private int characterId;
        private IPEndPoint endPoint;
        private NetConnection sender;

        private bool logoutCharacterRequested;
        private bool logoutCharacterGranted;
        private bool logoutClientRequested;

        private ClientState state;

        public object pCharacter;
        public DateTime LastSaved;
        #endregion

        #region Properties
        public int AccountId
        {
            get { return accountId; }
            set { accountId = value; }
        }

        public int CharacterId
        {
            get { return characterId; }
            set { characterId = value; }
        }

        public NetConnection Sender
        {
            get { return sender; }
        }

        public FloodEntry FloodEntry
        {
            get { return floodEntry; }
        }

        public bool LogoutCharacterRequested
        {
            get { return logoutCharacterRequested; }
            set { logoutCharacterRequested = value; }
        }

        public bool LogoutCharacterGranted
        {
            get { return logoutCharacterGranted; }
            set { logoutCharacterGranted = value; }
        }

        public bool LogoutClientRequested
        {
            get { return logoutClientRequested; }
            set { logoutClientRequested = value; }
        }

        public ClientState State
        {
            get { return state; }
            set { state = value; }
        }
        #endregion

        #region Constructors
        public MasterPlayerRecord(NetConnection sender)
        {
            this.accountId = 0;
            this.endPoint = sender.RemoteEndpoint;
            this.characterId = 0;
            this.floodEntry = new FloodEntry();
            this.sender = sender;
            this.pCharacter = null;
            this.logoutCharacterGranted = false;
            this.logoutCharacterRequested = false;
            this.LastSaved = System.DateTime.MinValue;
            this.state = ClientState.CharacterManagement;
        }
        #endregion
    }
}
