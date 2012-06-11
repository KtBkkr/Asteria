using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Net;
using System.Threading;
using AsteriaLibrary.Shared;
using AsteriaLibrary.Zones;
using AsteriaWorldServer.Messages;
using AsteriaWorldServer.Networking;
using AsteriaWorldServer.PlayerCache;
using Lidgren.Network;
using MySql.Data.MySqlClient;

namespace AsteriaWorldServer
{
    sealed class WorldServer
    {
        #region Fields
        private bool isClosing;

        private InterServer interServer;
        private NetworkServer networkServer;

        private ClientStateManager csm;
        private MessageHandler msgHandler;
        private MessageSender msgSender;
        private LowPriorityManager lowPrioHandler;
        private TurnManager turnManager;
        private DalProvider dal;

        private ServerContext context;

        private NetServer netServerInter;
        private NetServer netServerClient;

        private Dictionary<string, string> serverConfig;
        public static Stopwatch Timer;
        #endregion

        #region Properties
        public bool IsClosing
        {
            get { return isClosing; }
        }
        #endregion

        #region Constructors
        public WorldServer()
        {
            serverConfig = new Dictionary<string, string>();
            isClosing = false;
        }
        #endregion

        #region Methods
        public bool Start()
        {
            try
            {
                context = new ServerContext();
                context.ServerConfig = serverConfig;

                // Database connection check
                Logger.Output(this, "Checking database connection..");
                context.Css = Config.DatabaseConnectionString;

                using (MySqlConnection conn = new MySqlConnection(Config.DatabaseConnectionString))
                {
                    conn.Open();

                    if (conn.State == ConnectionState.Open)
                    {
                        Logger.Output(this, "Database connection verified..");

                        MySqlCommand cmd = conn.CreateCommand();
                        cmd.CommandText = "SELECT * FROM settings";
                        MySqlDataReader dr = cmd.ExecuteReader();

                        // Load all world settings from the database.
                        while (dr.Read())
                        {
                            string name, value;
                            name = DatabaseHelper.GetString(dr, "name");
                            value = DatabaseHelper.GetString(dr, "value");
                            Logger.Record(this, "Config param: {0}\t= '{1}'", name, value);
                            serverConfig.Add(name, value);
                        }
                        dr.Close();
                    }
                    else
                    {
                        Logger.Output(this, "Database connection can't be opened!");
                        throw new InvalidOperationException("Database connection can't be opened.");
                    }
                }

                Logger.Output(this, "Creating server components..");

                Timer = new Stopwatch();
                Timer.Start();
                Logger.Output(this, "Hi res timer available: {0}, frequency: {1}.", Stopwatch.IsHighResolution, Stopwatch.Frequency);

                int result;
                int.TryParse(serverConfig["turn_duration"], out result);
                context.TurnDuration = result;

                int.TryParse(serverConfig["users_allowed"], out result);
                context.Mpt = new MasterPlayerTable(result, context);

                // Queues
                QueueManager qMngr = QueueManager.Singletone;
                qMngr.CreateQueues(result);

                // Load all WorldData.xml and Entities.xml data
                DataManager dMngr = DataManager.Singletone;

                // Create the zone manager.
                Logger.Output(this, "Creating zone manager..");
                context.ZoneManager = new ZoneManager(context);

                // Now load all static predefined entities.
                Logger.Output(this, "Loading static entities..");
                dMngr.LoadStaticEntities(context.ZoneManager);

                // Create the game processor for handling messages.
                Logger.Output(this, "Creating game processor..");
                context.GameProcessor = new GameProcessor(context);

                // Network server
                Logger.Output(this, "Creating network server..");

                NetPeerConfiguration cfg = new NetPeerConfiguration("Asteria");
                int.TryParse(context.ServerConfig["client_port"], out result);
                cfg.Port = result;

                int.TryParse(context.ServerConfig["users_allowed"], out result);
                cfg.MaximumConnections = result;

                int.TryParse(context.ServerConfig["max_transmissionunit"], out result);
                cfg.MaximumTransmissionUnit = result;

                int.TryParse(context.ServerConfig["client_timeout_seconds"], out result);
                cfg.ConnectionTimeout = result;

                cfg.ReceiveBufferSize = 4095;
                cfg.EnableMessageType(NetIncomingMessageType.ConnectionApproval);
#if DEBUG
                cfg.EnableMessageType(NetIncomingMessageType.VerboseDebugMessage);
#endif

                IPAddress clientAddress;
                if (context.ServerConfig.ContainsKey("client_host") && IPAddress.TryParse(context.ServerConfig["client_host"], out clientAddress))
                {
                    cfg.LocalAddress = clientAddress;
                    Logger.Output(this, "NetworkServer using network interface: {0}:{1}", cfg.LocalAddress, cfg.Port);
                }
                else
                    Logger.Output(this, "Could not parse 'client_host' param, using: {0}:{1}", cfg.LocalAddress, cfg.Port);

                netServerClient = new NetServer(cfg);
                Logger.Output(this, "Networking params client: max connections: {0}, MTU: {1}, connection timeout: {2}", cfg.MaximumConnections, cfg.MaximumTransmissionUnit, cfg.ConnectionTimeout);

                networkServer = new NetworkServer(context, netServerClient);

                int.TryParse(context.ServerConfig["threadpool_size_T1"], out result);
                networkServer.Start(result);

                // Inter server
                Logger.Output(this, "Creating inter server..");

                NetPeerConfiguration cfg2 = new NetPeerConfiguration("InterAsteria");
                int.TryParse(serverConfig["inter_port"], out result);
                cfg2.Port = result;

                int.TryParse(serverConfig["users_allowed"], out result);
                cfg2.MaximumConnections = result;

                int.TryParse(serverConfig["max_transmissionunit"], out result);
                cfg2.MaximumTransmissionUnit = Math.Min(result, 4095);

                int.TryParse(serverConfig["client_timeout_seconds"], out result);
                cfg2.ConnectionTimeout = result;

                cfg2.ReceiveBufferSize = 4095;
                cfg2.EnableMessageType(NetIncomingMessageType.ConnectionApproval);
#if DEBUG
                cfg2.EnableMessageType(NetIncomingMessageType.VerboseDebugMessage);
#endif

                // TODO: [LOW] look into WorldConnection disconnects..
                cfg2.ConnectionTimeout = 120;

                IPAddress interAddress;
                if (serverConfig.ContainsKey("inter_host") && IPAddress.TryParse(serverConfig["inter_host"], out interAddress))
                {
                    cfg2.LocalAddress = interAddress;
                    Logger.Output(this, "InterServer using network interface: {0}:{1}", cfg2.LocalAddress, cfg2.Port);
                }
                else
                    Logger.Output(this, "Could not parse 'inter_host' param, using: {0}:{1}", cfg2.LocalAddress, cfg2.Port);

                netServerInter = new NetServer(cfg2);
                Logger.Output(this, "InterServer params: max connections: {0}, MTU: {1}, ping interval: {2}, timeout delay: {3}", cfg2.MaximumConnections, cfg2.MaximumTransmissionUnit, cfg2.PingInterval, cfg2.ConnectionTimeout);

                interServer = new InterServer(context, netServerInter);

                int.TryParse(serverConfig["threadpool_size_T1"], out result);
                interServer.Start(result);

                // Client state manager
                Logger.Output(this, "Creating client state manager..");
                csm = new ClientStateManager(context);
                csm.Start(1);

                // ClientToServerMessage deserializer
                Logger.Output(this, "Creating message handler..");
                msgHandler = new MessageHandler(context, netServerClient);

                int.TryParse(context.ServerConfig["threadpool_size_T2"], out result);
                msgHandler.Start(result);

                // ClientToServerMessage serializer
                Logger.Output(this, "Creating message sender..");
                msgSender = new MessageSender(context, netServerClient);

                int.TryParse(context.ServerConfig["threadpool_size_T4"], out result);
                msgSender.Start(result);

                // Dal Provider
                Logger.Output(this, "Creating DAL provider..");
                dal = new DalProvider(context);
                context.Dal = dal;

                // Character Management, Inter Server, Chat
                Logger.Output(this, "Creating low priority handler..");
                lowPrioHandler = new LowPriorityManager(context, dal);

                int.TryParse(context.ServerConfig["threadpool_size_T3"], out result);
                lowPrioHandler.Start(result);

                // Turn Manager
                Logger.Output(this, "Creating turn manager..");
                turnManager = new TurnManager(context);
                turnManager.Start();

                Logger.Output(this, "Configuration finished in {0} milliseconds!", Timer.ElapsedMilliseconds);
                Thread.Sleep(500);
                return true;
            }
            catch (Exception ex)
            {
                Logger.Output(this, "Configure exception: {0}, stack trace: {1}", ex.Message, ex.StackTrace);
            }
            return false;
        }

        public void PrepareShutdown()
        {
            Logger.Output(this, "Preparing for shudown..");

            if (interServer != null)
                interServer.Stop();

            if (networkServer != null)
                networkServer.Stop();

            context.ZoneManager.SaveAllZones();
        }

        /// <summary>
        /// Stops all the server background threads.
        /// </summary>
        public void Shutdown()
        {
            Logger.Output(this, "Stopping background threads..");

            if (msgHandler != null)
                msgHandler.Stop();

            if (lowPrioHandler != null)
                lowPrioHandler.Stop();

            if (turnManager != null)
                turnManager.Stop();

            if (csm != null)
                csm.Stop();

            if (msgSender != null)
                msgSender.Stop();
        }

        /// <summary>
        /// Dumps statistics data.
        /// </summary>
        public void WriteStatistics()
        {
            Console.WriteLine();
            foreach (string s in networkServer.DumpStats())
                Console.WriteLine(s);
        }

        /// <summary>
        /// Handles console commands.
        /// </summary>
        /// <param name="message"></param>
        public void HandleConsoleCommand(string message)
        {
            if (message == "exit")
            {
                isClosing = true;
                return;
            }
            else if (message == "stats")
            {
                WriteStatistics();
                return;
            }
            else
                Console.WriteLine("\r\nUnknown command.\r\n");
        }
        #endregion
    }
}
