using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AsteriaLibrary.Shared;

namespace AsteriaLibrary.Client
{
    /// <summary>
    /// Wrapper around LoginClient to streamline the client connection process.
    /// The logical order of invoking methods is:
    /// Login() to connect to the login server,
    /// WorldServers to get the world server list,
    /// SelectWorld to get the secret phrase and prepare login
    /// </summary>
    public sealed class LoginConnection : IDisposable
    {
        public enum LoginConnectionState
        {
            Disconnected,
            ConnectedToLogin,
            WorldListReceived
        }

        #region Delegates / Events
        public delegate void StateChangeHandler(LoginConnectionState state);

        /// <summary>
        /// Fired when the state changes.
        /// </summary>
        public event StateChangeHandler StateChanged;
        #endregion

        #region Fields
        private LoginClient lc;
        private List<WorldServerInfo> worldList = null;
        private LoginConnectionState state = LoginConnectionState.Disconnected;

        private string host;
        private int port;

        private string protocolVersion;
        private int accountId = -1;
        private string disconnectMessage;
        #endregion

        #region Properties
        public string DisconnectMessage { get { return disconnectMessage; } }

        /// <summary>
        /// Returns the accountId after a successful login.
        /// </summary>
        public int AccountId { get { return accountId; } }

        /// <summary>
        /// Returns the current connection state.
        /// </summary>
        public LoginConnectionState State
        {
            get { return state; }
            private set
            {
                state = value;
                if (StateChanged != null)
                    StateChanged(state);
            }
        }

        /// <summary>
        /// Returns the world list with connection info or null if not connected to the login server.
        /// </summary>
        public List<WorldServerInfo> WorldServers { get { return worldList; } }
        #endregion

        #region Constructors
        public LoginConnection(string host, int port, string protocolVersion)
        {
            this.host = host;
            this.port = port;
            this.protocolVersion = protocolVersion;
        }
        #endregion

        #region Methods
        /// <summary>
        /// Login to the login server.
        /// </summary>
        /// <param name="username"></param>
        /// <param name="password"></param>
        /// <returns></returns>
        public bool Login(string username, string password)
        {
            if (lc != null)
                lc.Dispose();

            lc = new LoginClient(5, protocolVersion);

            lc.LoginDisconnected += new LoginClientEvent(HandleDisconnectEvent);
            lc.LoginConnected += new LoginClientEvent(HandleConnectEvent);

            if (lc.Connect(host, port))
            {
                State = LoginConnectionState.ConnectedToLogin;

                if (lc.Login(username, password))
                {
                    State = LoginConnectionState.WorldListReceived;
                    worldList = lc.WorldServers;
                    accountId = lc.AccountId;
                }
                else
                {
                    disconnectMessage = "Unable to login.";
                    State = LoginConnectionState.Disconnected;

                    lc.Dispose();
                }
            }
            else
            {
                disconnectMessage = "Unabe to connect.";
                State = LoginConnectionState.Disconnected;

                lc.Dispose();
            }

            return (state == LoginConnectionState.WorldListReceived);
        }

        /// <summary>
        /// Gets the secret key for a world server.
        /// </summary>
        /// <param name="worldId"></param>
        /// <returns></returns>
        public string SelectWorld(int worldId)
        {
            if (state == LoginConnectionState.WorldListReceived)
            {
                WorldServerInfo wsi = worldList.Find(c => c.Id == worldId);
                return lc.SelectWorld(wsi);
            }
            else
                throw new InvalidOperationException("Invalid state: " + state);
        }

        /// <summary>
        /// Called on disconnect.
        /// </summary>
        private void HandleDisconnectEvent() { }

        /// <summary>
        /// Called on connect.
        /// </summary>
        private void HandleConnectEvent() { }

        #region IDisposable Members
        public void Dispose()
        {
            try
            {
                if (lc != null)
                    lc.Dispose();

                lc = null;
            }
            finally
            {
                State = LoginConnectionState.Disconnected;
            }
        }
        #endregion

        #endregion
    }
}
