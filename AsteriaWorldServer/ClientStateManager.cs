using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using AsteriaLibrary.Shared;
using AsteriaWorldServer.PlayerCache;

namespace AsteriaWorldServer
{
    /// <summary>
    /// Keeps track of connected clients, checks client states and handles Login/Logout and character management.
    /// The ClientStateManager periodically checks for disconnected clients and removes them from the player cache.
    /// Note that this class is not reading from queues or processing messages, instea it is more of a helper used by
    /// the MessageHandler, NetworkServer and LowPriorityManager instances.
    /// </summary>
    sealed class ClientStateManager : ThreadedComponent
    {
        #region Fields
        private ServerContext context;
        #endregion

        #region Properties
        protected override string ThreadName
        {
            get { return "CSM"; }
        }
        #endregion

        #region Constructors
        public ClientStateManager(ServerContext context)
        {
            this.context = context;
        }
        #endregion

        #region Methods
        protected override void Worker(object parameter)
        {
            Logger.Output(this, "CSM loop starting..");

            // Grab the worker thread and init
            WorkerThread wt = (WorkerThread)parameter;
            if (wt == null)
                throw new InvalidOperationException("Invalid CSM parameter.");

            Logger.Output(this, "CSM loop started!");

            // Loop until the server stops
            do
            {
                // TODO: [LOW] inactive players should be disconnected (configurable).
                context.Mpt.Cleanup();

                // TODO: [MID] loop through the InWorld characters and save them periodically (configurable).

                Thread.Sleep(500);

            } while (wt.IsRunning);

            Logger.Output(this, "Worker thread: {0} exited!", wt.Thread.Name);
        }
        #endregion
    }
}
