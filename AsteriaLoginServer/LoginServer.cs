using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Net;
using System.Data;
using Lidgren.Network;
using MySql.Data.MySqlClient;
using AsteriaLibrary.Shared;
using AsteriaLibrary.Serialization;
using AsteriaLibrary.Messages;

namespace AsteriaLoginServer
{
    sealed class LoginServer
    {
        #region Fields
        private bool isRunning;
        private bool isClosing;
        private Thread serverThread;

        private NetPeerConfiguration configClients;
        private NetPeerConfiguration configWorlds;
        private NetServer server;

        private string protocolVersion;
        private Dictionary<string, string> config;
        private DateTime lastSaved = DateTime.Now;

        private Dictionary<int, WorldServerInfo> registeredWorlds;
        private Dictionary<int, WorldConnection> worldConnections;
        private string worldInfoString;

        private MySqlCommand cmdGetAccount;
        private MySqlCommand cmdSetAccount;
        private MySqlCommand cmdUpdateWorld;

        private BaseServerMessageSerializer serializer = new BaseServerMessageSerializer();
        private BaseClientMessageSerializer deserializer = new BaseClientMessageSerializer();

        private enum AccountStatus
        {
            New = 0,        // New account not yet verified. Not playable.
            Verified = 1,   // Normal account state. Can login and play.
            InGame = 2,     // Already logged in and playing inside a world.
            GmLocked = 3,   // Locked by a GM. Can't login until lock is manually cleared.
            Kicked = 4,     // Kicked and can't login until date is reached.
            Banned = 5,     // Permanently locked. No login is possible.
        }
        #endregion

        #region Properties
        public bool IsClosing
        {
            get { return isClosing; }
        }
        #endregion

        #region Constructors
        public LoginServer()
        {
            config = new Dictionary<string, string>();

            registeredWorlds = new Dictionary<int, WorldServerInfo>();
            worldConnections = new Dictionary<int, WorldConnection>();
            worldInfoString = "None";

            isClosing = false;
        }

        ~LoginServer()
        {
            if ((DateTime.Now - lastSaved).TotalSeconds > 5)
                SaveWorldsToDatabase();
        }
        #endregion

        #region Methods
        public bool Start()
        {
            bool result = true;

            try
            {
                Logger.Output(this, "Starting login server..");

                // Setup config.
                using (MySqlConnection conn = new MySqlConnection(Config.DatabaseConnectionString))
                {
                    conn.Open();

                    if (conn.State == ConnectionState.Open)
                    {
                        Logger.Output(this, "Database connection verified..");

                        MySqlCommand cmd = conn.CreateCommand();
                        cmd.CommandText = "SELECT * FROM settings";
                        MySqlDataReader dr = cmd.ExecuteReader();

                        // Load config from database
                        while (dr.Read())
                            config.Add(DatabaseHelper.GetString(dr, "name"), DatabaseHelper.GetString(dr, "value"));

                        dr.Close();
                    }
                    else
                    {
                        Logger.Output(this, "Database connection can not be opened!");
                        throw new InvalidOperationException("Database connection can not be opened.");
                    }
                }

                protocolVersion = config["protocol_version"];

                // Check Data
                int maxConnections, clientPort;
                if (!int.TryParse(config["users_allowed"], out maxConnections))
                    throw new InvalidOperationException("Config parameter 'users_allowed' not defined!");
                if (!int.TryParse(config["port"], out clientPort))
                    throw new InvalidOperationException("Config parameter 'port' not defined!");

                // Setup Networking
                configClients = new NetPeerConfiguration("Asteria");
                configClients.Port = clientPort;
                configClients.MaximumConnections = maxConnections;
                configClients.ReceiveBufferSize = 1024;
                configClients.MaximumTransmissionUnit = 4095;

                IPAddress address;
                if (IPAddress.TryParse(config["hostname"], out address))
                {
                    configClients.LocalAddress = address;
                    Logger.Output(this, "Using network interface: {0}:{1}.", configClients.LocalAddress, configClients.Port);
                }
                else
                    Logger.Output(this, "Could not parse 'hostname' param, using: {0}:{1}.", configClients.LocalAddress, configClients.Port);

                configClients.EnableMessageType(NetIncomingMessageType.ConnectionApproval);
                server = new NetServer(configClients);

                configWorlds = new NetPeerConfiguration("InterAsteria");
                configWorlds.MaximumConnections = maxConnections;
                configWorlds.ReceiveBufferSize = 1024;
                configWorlds.MaximumTransmissionUnit = 4095;

                // Setup Database Procedures
                cmdGetAccount = new MySqlCommand();
                cmdGetAccount.CommandText = "SELECT * FROM accounts WHERE username = @Name";
                cmdGetAccount.CommandTimeout = 15;
                cmdGetAccount.Parameters.Add(new MySqlParameter("@Name", MySqlDbType.String, 20));

                cmdSetAccount = new MySqlCommand();
                cmdSetAccount.CommandText = "UPDATE accounts SET status = @Status, last_login = @LastLogin, locked_until = @LockedUntil WHERE username = @Name";
                cmdSetAccount.CommandTimeout = 15;
                cmdSetAccount.Parameters.Add(new MySqlParameter("@Name", MySqlDbType.String, 20));
                cmdSetAccount.Parameters.Add(new MySqlParameter("@Status", MySqlDbType.Int32, 4));
                cmdSetAccount.Parameters.Add(new MySqlParameter("@LastLogin", MySqlDbType.DateTime, 8));
                cmdSetAccount.Parameters.Add(new MySqlParameter("@LockedUntil", MySqlDbType.DateTime, 8));

                cmdUpdateWorld = new MySqlCommand();
                cmdUpdateWorld.CommandText = "UPDATE worlds SET name = @Name, client_host = @ClientHost, client_port = @ClientPort, inter_host = @InterHost, inter_port = @InterPort, online = @Online, users_allowed = @Allowed WHERE id = @Id";
                cmdUpdateWorld.Parameters.Add(new MySqlParameter("@Id", MySqlDbType.Int32, 4));
                cmdUpdateWorld.Parameters.Add(new MySqlParameter("@Name", MySqlDbType.String, 50));
                cmdUpdateWorld.Parameters.Add(new MySqlParameter("@ClientHost", MySqlDbType.String, 50));
                cmdUpdateWorld.Parameters.Add(new MySqlParameter("@ClientPort", MySqlDbType.Int32, 4));
                cmdUpdateWorld.Parameters.Add(new MySqlParameter("@InterHost", MySqlDbType.String, 50));
                cmdUpdateWorld.Parameters.Add(new MySqlParameter("@InterPort", MySqlDbType.Int32, 4));
                cmdUpdateWorld.Parameters.Add(new MySqlParameter("@Online", MySqlDbType.DateTime, 8));
                cmdUpdateWorld.Parameters.Add(new MySqlParameter("@Allowed", MySqlDbType.Int32, 4));

                isRunning = true;

                // Get world server list from database.
                LoadWorldsFromDatabase();

                // Create connection to each world.
                foreach (WorldServerInfo wsi in registeredWorlds.Values)
                {
                    WorldConnection conn = new WorldConnection(configWorlds, wsi, protocolVersion);
                    conn.Start();
                    worldConnections.Add(wsi.Id, conn);
                }

                serverThread = new Thread(new ThreadStart(Update));
                serverThread.Name = "SL";
                serverThread.IsBackground = true;
                serverThread.Start();

                Thread.Sleep(50);
            }
            catch (Exception ex)
            {
                Logger.Output(this, "Server exception: {0}", ex.Message);
                result = false;
            }
            finally
            {
                if (result)
                    Logger.Output(this, "Login server started!");
            }
            return result;
        }

        public void Stop()
        {
            Logger.Output(this, "Stopping login server..");

            try
            {
                isRunning = false;
                serverThread.Join(3000);

                foreach (WorldConnection wc in worldConnections.Values)
                    wc.Stop();
            }
            catch (Exception ex)
            {
                Logger.Output(this, "Server exception: {0}", ex.Message);
            }
            finally
            {
                Logger.Output(this, "Login server stopped!");
                SaveWorldsToDatabase();
            }
        }

        private void Update()
        {
            Logger.Output(this, "Started listening for player connections..");

            try
            {
                List<NetConnection> clients = new List<NetConnection>();
                NetIncomingMessage receivedMsg;
                NetConnection sender;
                AutoResetEvent waitSignal = server.MessageReceivedEvent;

                server.Start();

                while (isRunning)
                {
                    if ((receivedMsg = server.ReadMessage()) != null)
                    {
                        sender = receivedMsg.SenderConnection;
                        switch (receivedMsg.MessageType)
                        {
                            case NetIncomingMessageType.DebugMessage:
                                string debugMessage = receivedMsg.ReadString();
                                Logger.Output(this, "Debug message: {0}", debugMessage);
                                break;

                            case NetIncomingMessageType.VerboseDebugMessage:
                                debugMessage = receivedMsg.ReadString();
                                Logger.Output(this, "Debug message: {0}", debugMessage);
                                break;

                            case NetIncomingMessageType.ConnectionApproval:
                                string remoteProtocalVersion = receivedMsg.ReadString();
                                if (remoteProtocalVersion != protocolVersion)
                                {
                                    Logger.Output(this, "Player rejected: {0}", sender.RemoteEndpoint);
                                    sender.Deny("Connection rejected. Login version: " + protocolVersion);
                                    break;
                                }

                                Logger.Output(this, "Player joined: {0}", sender.RemoteEndpoint);
                                sender.Approve();
                                break;

                            case NetIncomingMessageType.StatusChanged:
                                if (sender.Status == NetConnectionStatus.Disconnected)
                                {
                                    if (clients.Contains(sender))
                                        clients.Remove(sender);

                                    Logger.Output(this, "Player disconnected: {0}", sender.RemoteEndpoint);
                                }
                                break;

                            case NetIncomingMessageType.Data:
                                bool rejectClient = true;
                                string message = "";

                                // Get the client messages
                                byte[] byteBuffer = new byte[receivedMsg.LengthBytes];
                                receivedMsg.ReadBytes(byteBuffer, 0, receivedMsg.LengthBytes);
                                BaseClientMessage msg = deserializer.Deserialize(byteBuffer);

                                if (msg != null)
                                {
                                    switch (msg.MessageType)
                                    {
                                        case MessageType.C2L_Login:
                                            if (!clients.Contains(sender))
                                            {
                                                int accountId;
                                                if (ValidateLogin(msg.Data, out message, out accountId))
                                                {
                                                    sender.Tag = accountId;
                                                    clients.Add(sender);

                                                    BaseServerMessage serverMsg = BaseServerMessage.CreateMessage();
                                                    serverMsg.MessageType = MessageType.L2C_LoginResponse;
                                                    serverMsg.Data = GetWorldString();
                                                    serverMsg.Code = accountId;
                                                    serverMsg.DeliveryMethod = NetDeliveryMethod.ReliableOrdered;
                                                    SendMessage(serverMsg, sender);

                                                    Logger.Output(this, "Player logged in: ID {0} ({1}).", accountId, sender.RemoteEndpoint);
                                                    rejectClient = false;
                                                }
                                                else
                                                    Logger.Output(this, "Player rejected: {0}", message);
                                            }
                                            else
                                                message = "Already logged in";
                                            break;

                                        case MessageType.C2L_SelectWorld:
                                            int worldId;
                                            string token;

                                            // Hacked clients may request a different secret key
                                            // for an ID they didn't login with.
                                            if (msg.AccountId != (int)sender.Tag)
                                            {
                                                message = "Hacking is lame..";
                                                break;
                                            }

                                            if (ParseWorldSelection(msg.Data, out message, out worldId))
                                            {
                                                Random rand = new Random();

                                                token = sender.RemoteEndpoint.Address.ToString();

                                                char add;
                                                for (int i = 0; i < 16; i++)
                                                {
                                                    add = (char)rand.Next(33, 125);
                                                    token += add;
                                                }

                                                // We need to add a _ here because a hacker may wait for a lucky shot
                                                // of the random OTP being a number at the end.
                                                token += "_" + ((int)sender.Tag).ToString();

                                                ServerToServerMessage svrm2 = ServerToServerMessage.CreateMessageSafe();
                                                svrm2.MessageType = MessageType.L2S_SendOneTimePad;
                                                svrm2.Data = token;
                                                svrm2.Code = (int)sender.Tag;
                                                svrm2.DeliveryMethod = NetDeliveryMethod.ReliableOrdered;
                                                worldConnections[worldId].SendMessage(svrm2);

                                                BaseServerMessage svrm = BaseServerMessage.CreateMessage();
                                                svrm.MessageType = MessageType.L2C_WorldSelectResponse;
                                                svrm.Data = token;
                                                svrm.Code = (int)sender.Tag;
                                                svrm.DeliveryMethod = NetDeliveryMethod.ReliableOrdered;
                                                SendMessage(svrm, sender);

                                                Logger.Output(this, "Player entering world: ID {0} ({1}) -> '{2}'!", msg.AccountId, sender.RemoteEndpoint, worldId);
                                                rejectClient = false;
                                            }
                                            else
                                                Logger.Output(this, "Player: ID {0} ({1}) -> could not parse world!", msg.AccountId, msg.Data);
                                            break;
                                    }
                                }
                                else
                                    Logger.Output(this, "Received data which could not be deserialized. Client: {0}", sender.RemoteEndpoint);

                                if (rejectClient)
                                {
                                    if (clients.Contains(sender))
                                        clients.Remove(sender);

                                    sender.Disconnect(String.Format("Player rejected: {0}", message));
                                }
                                break;

                            default:
                                Logger.Output(this, "Unhandled message type. ({0}) {1}", receivedMsg.MessageType, receivedMsg.ToString());
                                break;
                        }
                    }
                    else
                    {
                        UpdateWorldString();

                        if ((DateTime.Now - lastSaved).TotalSeconds > 60)
                            SaveWorldsToDatabase();

                        waitSignal.WaitOne(250);
                    }
                }
                server.Shutdown("");
            }
            catch (Exception ex)
            {
                Logger.Output(this, "Server exception: {0} | {1}", ex.Message, ex.StackTrace);
            }

            Logger.Output(this, "Stopped listening for player connections..");
        }

        private void SendMessage(BaseServerMessage msg, NetConnection conn)
        {
            byte[] bytes = serializer.Serialize(msg);
            NetDeliveryMethod method = msg.DeliveryMethod;
            BaseServerMessage.Free(msg);

            NetOutgoingMessage dispatchBuffer = server.CreateMessage(bytes.Length);
            dispatchBuffer.Write(bytes);
            server.SendMessage(dispatchBuffer, conn, method);
        }

        private void LoadWorldsFromDatabase()
        {
            try
            {
                using (MySqlConnection conn = new MySqlConnection(Config.DatabaseConnectionString))
                {
                    conn.Open();

                    MySqlCommand cmd = conn.CreateCommand();
                    cmd.CommandText = "SELECT id, name, client_host, client_port, inter_host, inter_port, online, users_allowed FROM worlds";
                    MySqlDataReader dr = cmd.ExecuteReader();

                    lock (registeredWorlds)
                    {
                        registeredWorlds.Clear();
                        while (dr.Read())
                        {
                            IPAddress clientIp, interIp;

                            // Read world connection data.
                            int wsiId = DatabaseHelper.GetInt32(dr, "id");
                            string wsiName = DatabaseHelper.GetString(dr, "name");

                            // Get the client address
                            string clientHost = DatabaseHelper.GetString(dr, "client_host");
                            if (!IPAddress.TryParse(clientHost, out clientIp))
                            {
                                try
                                {
                                    IPHostEntry iph = Dns.GetHostEntry(clientHost);
                                    clientIp = iph.AddressList[0];
                                }
                                catch
                                {
                                    Logger.Output(this, "Unable to resolve world: {0} ({1})", wsiName, clientHost);
                                    continue;
                                }
                            }

                            // Get the inter address
                            string interHost = DatabaseHelper.GetString(dr, "inter_host");
                            if (!IPAddress.TryParse(interHost, out interIp))
                            {
                                try
                                {
                                    IPHostEntry iph = Dns.GetHostEntry(interHost);
                                    interIp = iph.AddressList[0];
                                }
                                catch
                                {
                                    Logger.Output(this, "Unable to resolve world: {0} ({1})", wsiName, interHost);
                                    continue;
                                }
                            }

                            // Read world data.
                            IPEndPoint wsiClientAddress = new IPEndPoint(clientIp, DatabaseHelper.GetInt32(dr, "client_port"));
                            IPEndPoint wsiInterAddress = new IPEndPoint(interIp, DatabaseHelper.GetInt32(dr, "inter_port"));
                            DateTime wsiIsOnline = dr.IsDBNull(dr.GetOrdinal("online")) ? DateTime.MinValue : dr.GetDateTime(dr.GetOrdinal("online"));
                            int wsiAllowed = DatabaseHelper.GetInt32(dr, "users_allowed");
                            WorldServerInfo wsi = new WorldServerInfo(wsiId, wsiName, wsiIsOnline, wsiClientAddress, wsiInterAddress, 0, wsiAllowed);

                            registeredWorlds.Add(wsi.Id, wsi);
                        }
                    }

                    dr.Close();
                }

                UpdateWorldString();
                Logger.Output(this, "Worlds loaded from database: {0}", registeredWorlds.Count);
            }
            catch (MySqlException ex)
            {
                Logger.Output(this, "Database exception: {0}", ex.Message);
            }
            catch (Exception ex)
            {
                Logger.Output(this, "Server exception: {0}", ex.Message);
            }
        }

        private void SaveWorldsToDatabase()
        {
            try
            {
                using (MySqlConnection conn = new MySqlConnection(Config.DatabaseConnectionString))
                {
                    conn.Open();

                    lock (registeredWorlds)
                    {
                        foreach (WorldServerInfo wsi in registeredWorlds.Values)
                        {
                            // Write changes to database
                            cmdUpdateWorld.Connection = conn;
                            cmdUpdateWorld.Parameters["@Id"].Value = wsi.Id;
                            cmdUpdateWorld.Parameters["@Name"].Value = wsi.Name;
                            cmdUpdateWorld.Parameters["@ClientHost"].Value = wsi.ClientAddress.Address.ToString();
                            cmdUpdateWorld.Parameters["@ClientPort"].Value = wsi.ClientAddress.Port;
                            cmdUpdateWorld.Parameters["@InterHost"].Value = wsi.InterAddress.Address.ToString();
                            cmdUpdateWorld.Parameters["@InterPort"].Value = wsi.InterAddress.Port;
                            cmdUpdateWorld.Parameters["@Online"].Value = wsi.IsOnline;
                            cmdUpdateWorld.Parameters["@Allowed"].Value = wsi.Allowed;
                            cmdUpdateWorld.ExecuteNonQuery();
                        }

                        lastSaved = DateTime.Now;
                        Logger.Output(this, "Worlds saved to database: {0}", registeredWorlds.Count);
                    }
                }
            }
            catch (MySqlException ex)
            {
                Logger.Output(this, "Database exception: {0}", ex.Message);
            }
            catch (Exception ex)
            {
                Logger.Output(this, "Server exception: {0} | {1}", ex.Message, ex.StackTrace);
            }
        }

        private void UpdateWorldString()
        {
            lock (worldInfoString)
            {
                worldInfoString = "";
                foreach (WorldServerInfo wsi in registeredWorlds.Values)
                {
                    lock (wsi)
                    {
                        worldInfoString += wsi.ToClientString();
                    }
                }
            }
        }

        private string GetWorldString()
        {
            lock (worldInfoString)
            {
                string result = worldInfoString;
                return result;
            }
        }

        private bool ValidateLogin(string accountString, out string message, out int accountId)
        {
            accountId = -1;
            AccountStatus accountStatus = AccountStatus.New;
            DateTime lastLogin = new DateTime();
            DateTime lockedUntil = new DateTime();

            try
            {
                // Get account name and password.
                string name = accountString.Substring(0, 20).TrimEnd();
                string password = accountString.Substring(20);

                // Search for account in database.
                using (MySqlConnection conn = new MySqlConnection(Config.DatabaseConnectionString))
                {
                    conn.Open();

                    // Get stored password and check.
                    cmdGetAccount.Connection = conn;
                    cmdGetAccount.Parameters["@Name"].Value = name;
                    MySqlDataReader dr = cmdGetAccount.ExecuteReader();

                    if (dr.Read())
                    {
                        string storedPassword = DatabaseHelper.GetString(dr, "password");
                        if (string.Compare(storedPassword, password, StringComparison.OrdinalIgnoreCase) == 0)
                        {
                            // Password is good so check other data.
                            accountId = DatabaseHelper.GetInt32(dr, "id");
                            accountStatus = (AccountStatus)DatabaseHelper.GetInt32(dr, "status");

                            int ordinal = dr.GetOrdinal("last_login");
                            if (!dr.IsDBNull(ordinal))
                                lastLogin = dr.GetDateTime(ordinal);

                            ordinal = dr.GetOrdinal("locked_until");
                            if (!dr.IsDBNull(ordinal))
                                lockedUntil = dr.GetDateTime(ordinal);

                            dr.Close();

                            // Verify account status.
                            if (accountStatus == AccountStatus.GmLocked || accountStatus == AccountStatus.Kicked)
                            {
                                if (lockedUntil == null || lockedUntil <= DateTime.Now)
                                    accountStatus = AccountStatus.Verified;
                            }

                            if (accountStatus == AccountStatus.Verified)
                            {
                                lastLogin = DateTime.Now;
                                cmdSetAccount.Parameters["@LockedUntil"].Value = DBNull.Value;
                            }
                            else
                                cmdSetAccount.Parameters["@LockedUntil"].Value = lockedUntil;

                            // Write changes to database.
                            cmdSetAccount.Connection = conn;
                            cmdSetAccount.Parameters["@Name"].Value = name;
                            cmdSetAccount.Parameters["@Status"].Value = accountStatus;
                            cmdSetAccount.Parameters["@LastLogin"].Value = lastLogin;
                            cmdSetAccount.ExecuteNonQuery();

                            switch (accountStatus)
                            {
                                case AccountStatus.New:
                                    message = "Account not activated.";
                                    return false;

                                case AccountStatus.InGame:
                                    message = "Already in game.";
                                    return false;

                                case AccountStatus.Kicked:
                                    message = "Account kicked.";
                                    return false;

                                case AccountStatus.GmLocked:
                                    message = "Account locked.";
                                    return false;

                                case AccountStatus.Banned:
                                    message = "Account banned.";
                                    return false;

                                case AccountStatus.Verified:
                                    message = "";
                                    return true;
                            }
                        }
                    }

                    message = "Invalid account data.";
                    return false;
                }
            }
            catch (MySqlException ex)
            {
                message = "Database exception.";
                Logger.Output(this, "Database exception: {0}", ex.Message);
            }
            catch (Exception ex)
            {
                message = "Server exception.";
                Logger.Output(this, "Server exception: {0} | {1} | {2}", ex.Message, accountString, ex.StackTrace);
            }
            return false;
        }

        private bool ParseWorldSelection(string worldRequest, out string message, out int worldId)
        {
            if (int.TryParse(worldRequest, out worldId))
            {
                if (!registeredWorlds.ContainsKey(worldId))
                {
                    message = "Invalid world ID.";
                    worldId = -1;
                    return false;
                }
                else
                {
                    message = "";
                    return true;
                }
            }

            message = "Invalid world format.";
            worldId = -1;
            return false;
        }

        public void WriteStatistics()
        {
            // not implemented yet.
        }

        public void HandleConsoleCommand(string message)
        {
            if (message == "exit")
            {
                isClosing = true;
                return;
            }
            else if (message == "help")
            {
                Console.WriteLine("\r\nCommands:");
                Console.WriteLine("exit - Stops the login server.");
                Console.WriteLine("help - Shows this message.\r\n");
                return;
            }

            Console.WriteLine("\r\nUnknown command.\r\n");
        }
        #endregion
    }
}
