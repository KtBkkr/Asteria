using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using AsteriaLibrary.Data;
using AsteriaLibrary.Entities;
using AsteriaLibrary.Messages;
using AsteriaLibrary.Shared;
using AsteriaWorldServer.PlayerCache;
using Lidgren.Network;

namespace AsteriaWorldServer.Messages
{
    /// <summary>
    /// Maintains the turn queues.
    /// </summary>
    sealed class TurnManager
    {
        #region Fields
        private QueueManager queueMngr;
        private Stopwatch timer;
        private GameProcessor game;
        private ServerContext context;
        private int turnNumber;
        private volatile bool isRunning;
        private float elapsedTimeSinceLastTurn = 0;
        private float lastTurnEndTime = 0;
        private System.Timers.Timer clock;
        #endregion

        #region Constructors
        /// <summary>
        /// Creates a new TurnManager instance.
        /// </summary>
        /// <param name="context"></param>
        public TurnManager(ServerContext context)
        {
            this.context = context;
            this.game = context.GameProcessor;
            this.queueMngr = QueueManager.Singletone;
            this.timer = new Stopwatch();

            // Create a server timer for executing turns.
            double clockCycleDuration = (double)context.TurnDuration;
            clockCycleDuration *= 0.9; // TODO: [LOW] thislooks like an ugly hack, reconsider implementing dynamic clock frequency balancing.
            clock = new System.Timers.Timer(clockCycleDuration);
            clock.Elapsed += new System.Timers.ElapsedEventHandler(clock_Elapsed);
        }

        /// <summary>
        /// Main worker function executed onan implicitly  created threadpool thread and invoked from the clock server timer instance.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void clock_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            if (isRunning)
            {
                Logger.Output(this, "Dropping current clock cyclesince turn {0} is still in progress!", turnNumber);
                return;
            }
            else
                isRunning = true;

            Queue<ClientToServerMessage> requests;
            try
            {
                float turnStartTime = (float)timer.Elapsed.TotalMilliseconds;
                float elapsedTimeInsideTurn;

                // Signal WSE a new turn, this invoked OnNewTurn() where
                // the WSE calculates all the timebased game logic.
                game.NewTurn(turnNumber, elapsedTimeSinceLastTurn);

                // Swap turns
                requests = queueMngr.SwapTurnQueue();

                // Run actions
                int turnMessages = 0;
                while (requests.Count > 0)
                {
                    ClientToServerMessage msg = requests.Dequeue();
                    game.ProcessMessage(msg);
                    turnMessages++;
                }

                // Character logout handling
                LinkedNode<MasterPlayerRecord> node = context.Mpt.First;
                while (node != null)
                {
                    Character c = (Character)node.Value.pCharacter;
                    if (c != null)
                    {
                        // Check if player issued a logout request.
                        if (node.Value.LogoutCharacterRequested && !node.Value.LogoutCharacterGranted)
                        {
                            node.Value.LogoutCharacterGranted = game.IsLogoutAllowed(c.CharacterId);
                            Logger.Output(this, "Logout requested for character: {0}, WSE logout result: {1}", c.CharacterId, node.Value.LogoutCharacterGranted);

                            if (c.Sender.Status == NetConnectionStatus.Connected)
                            {
                                // TODO: [MID] when a player logs out we need to disconnect the client.
                                ServerToClientMessage wm = ServerToClientMessage.CreateMessageSafe(c.Sender);
                                MessageFormatter.CreateCharacterLogoutMessage(node.Value, turnNumber, wm);
                                QueueManager.WorldMessageQueueReadWrite = wm;
                            }
                        }

                        // Remove the client from the world.
                        if (node.Value.LogoutCharacterGranted && node.Value.State != ClientState.CharacterLoggingOut)
                        {
                            // TODO: [MID] the RemoveEntity message seems to be sent to the player who is logging out..
                            game.LogoutCharacter(c.CharacterId);
                            Logger.Output(this, "Logout granted for character: {0}, connection state: {1}!", c.CharacterId, c.Sender.Status);
                            node.Value.State = ClientState.CharacterLoggingOut;
                        }
                    }
                    node = node.Next;
                }

                // End turn, update stats.
                float turnEndMilis = (float)timer.Elapsed.TotalMilliseconds;
                elapsedTimeSinceLastTurn = turnEndMilis - lastTurnEndTime;
                elapsedTimeInsideTurn = turnEndMilis - turnStartTime;
                lastTurnEndTime = turnEndMilis;
#if DEBUG
                if (turnMessages > 0)
                    Logger.Output(this, "Turn: {0}, messages: {1}, turn duration: {2:0.00}, since last turn: {3:0.000}", turnNumber, turnMessages, elapsedTimeInsideTurn, elapsedTimeSinceLastTurn);
#endif
            }
            catch (Exception ex)
            {
                Logger.Output(this, "Turn processing exception, turn: {0}, message: {1}, stacktrace: {2}", turnNumber, ex.Message, ex.StackTrace);
            }
            finally
            {
                turnNumber++;
                isRunning = false;
            }
        }

        /// <summary>
        /// Stops the turn manager.
        /// </summary>
        public void Stop()
        {
            Logger.Output(this, "Stopping turn manager clock..");
            clock.Stop();
            timer.Stop();
        }

        /// <summary>
        /// Starts the turn manager.
        /// </summary>
        public void Start()
        {
            Logger.Output(this, "Starting turn manager clock..");
            clock.Start();
            timer.Start();
        }
        #endregion
    }
}
