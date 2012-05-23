using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using AsteriaLibrary.Shared;

namespace AsteriaWorldServer
{
    /// <summary>
    /// Base class for spanning multiple worker threads executing the same task.
    /// </summary>
    abstract class ThreadedComponent
    {
        /// <summary>
        /// Helper class to make thread manipulation easier.
        /// </summary>
        protected sealed class WorkerThread
        {
            public Thread Thread;
            public bool IsRunning;
        }

        #region Fields
        protected object locker;
        private List<WorkerThread> workerThreads;
        #endregion

        #region Constructors
        public ThreadedComponent()
        {
            locker = new object();
            workerThreads = new List<WorkerThread>();
        }
        #endregion

        #region Methods
        /// <summary>
        /// Spans the background worker threads.
        /// </summary>
        /// <param name="initialThreadCount">Number of threads that execute the worker method.</param>
        public void Start(int initialThreadCount)
        {
            Logger.Output(this, "Starting {0} threads..", ThreadName);
            ConfigureRunningThreads(initialThreadCount);
            Logger.Output(this, "{0} worker threads started, thread count: {1}!", ThreadName, initialThreadCount);
        }

        /// <summary>
        /// Stop the worker threads.
        /// </summary>
        public void Stop()
        {
            Logger.Output(this, "{0} stop request received..", ThreadName);
            foreach (WorkerThread wt in workerThreads)
                wt.IsRunning = false;
            foreach (WorkerThread wt in workerThreads)
                wt.Thread.Join(1000);
            Logger.Output(this, "All {0} worker threads stopped!", ThreadName);
        }

        /// <summary>
        /// Configured background thread count.
        /// </summary>
        /// <param name="numberOfThreads"></param>
        public void ConfigureRunningThreads(int numberOfThreads)
        {
            lock (locker)
            {
                int difference = numberOfThreads - workerThreads.Count;
                Logger.Output(this, "Reconfiguring {0} threads count, old value: {1}, new value: {2}.", ThreadName, workerThreads.Count, numberOfThreads);
                try
                {
                    for (int i = 0; i < difference; i++)
                    {
                        // Start or create threads?
                        if (difference > 0)
                        {
                            WorkerThread wt = new WorkerThread();
                            wt.Thread = new Thread(new ParameterizedThreadStart(Worker));
                            wt.Thread.Name = ThreadName + workerThreads.Count.ToString();
                            wt.IsRunning = true;
                            workerThreads.Add(wt);
                            wt.Thread.Start(wt);
                        }
                        else
                        {
                            WorkerThread wt = workerThreads[workerThreads.Count - 1];
                            workerThreads.RemoveAt(workerThreads.Count - 1);
                            wt.IsRunning = false;
                            wt.Thread.Join(1000);
                        }
                    }
                    Logger.Output(this, "Reconfiguration successful!");
                }
                catch (Exception ex)
                {
                    Logger.Output(this, "Reconfiguration exception: {0}, stack trace: {1}.", ex.Message, ex.StackTrace);
                }
            }
        }

        /// <summary>
        /// The thread worker function.
        /// </summary>
        /// <param name="parameter"></param>
        abstract protected void Worker(object parameter);

        /// <summary>
        /// Returns the thread names assigned to threads.
        /// </summary>
        abstract protected string ThreadName { get; }
        #endregion
    }
}
