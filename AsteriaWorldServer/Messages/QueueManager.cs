using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using AsteriaLibrary.Messages;
using Lidgren.Network;

namespace AsteriaWorldServer.Messages
{
    /// <summary>
    /// Manages all server queues.
    /// All static members are guaranteed to be thread safe and no locking is needed.
    /// </summary>
    public sealed class QueueManager
    {
        #region Fields
        private bool isInitialized;
        private QueueSample sample;
        private Queue<ClientToServerMessage> swapQueuePointer;

        private static readonly QueueManager instance = new QueueManager();

        /// <summary>
        /// Locker used to synchronise the CurrentTurnQueue access.
        /// </summary>
        private static object locker = new object();

        /// <summary>
        /// The main queue for reading and processing action requests.
        /// This queue must be accessed only inside a lock on the <see cref="locker"/> object.
        /// Note that this queue is accessed only for processing, writting is forbidden.
        /// </summary>
        private static Queue<ClientToServerMessage> currentTurnReadQueue;

        /// <summary>
        /// The main queue for storing deserialized action requests.
        /// This queue must be accessed only inside a lock on the <see cref="locker"/> object.
        /// </summary>
        private static Queue<ClientToServerMessage> currentTurnWriteQueue;

        /// <summary>
        /// Queue for storing raw (not deserialized) client messages.
        /// This queue must be accessed only inside a lock on the queue object.
        /// </summary>
        private static Queue<NetIncomingMessage> rawNetworkQueue;

        /// <summary>
        /// Queue for inter server communication.
        /// This queue must be accessed only inside a lock on the queue object.
        /// </summary>
        private static Queue<ServerToServerMessage> interServerQueue;

        /// <summary>
        /// Queue for storing character management stage messages.
        /// This queue must be accessed only inside a lock on the queue object.
        /// </summary>
        private static Queue<ClientToServerMessage> charMngtQueue;

        /// <summary>
        /// Queue for storing private, team, group, zone chat messages.
        /// This queue must be accessed only inside a lock on the queue object.
        /// </summary>
        private static Queue<ClientToServerMessage> chatQueue;

        /// <summary>
        /// Queue containing outgoing World Server messages.
        /// This queue must be accessed only inside a lock on the queue object.
        /// </summary>
        private static Queue<ServerToClientMessage> worldMessageQueue;

        /// <summary>
        /// Set to signaled state by the QueueManager when new data becomes available in the World Message queue.
        /// </summary>
        public static AutoResetEvent WorldMessageDataArrived = new AutoResetEvent(false);
        /// <summary>
        /// Set to signaled state by the QueueManager when new data becomes available in the Character Management queue.
        /// </summary>
        public static AutoResetEvent CharMngtQueueDataArrived = new AutoResetEvent(false);
        /// <summary>
        /// Set to signaled state by the QueueManager when new data becomes available in the Chat queue.
        /// </summary>
        public static AutoResetEvent ChatQueueDataArrived = new AutoResetEvent(false);
        /// <summary>
        /// Set to signaled state by the QueueManager when new data becomes available in the InterServer queue.
        /// </summary>
        public static AutoResetEvent InterServerQueueDataArrived = new AutoResetEvent(false);
        /// <summary>
        /// Set to signaled state by the QueueManager when new data becomes available in the Network queue.
        /// </summary>
        public static AutoResetEvent NetQueueDataArrived = new AutoResetEvent(false);
        #endregion

        #region Properties
        /// <summary>
        /// Returns singletone Queuemanager instance.
        /// </summary>
        public static QueueManager Singletone
        {
            get { return instance; }
        }

        /// <summary>
        /// Reads/Writes to the server queue.
        /// </summary>
        public static ServerToClientMessage WorldMessageQueueReadWrite
        {
            get
            {
                if (worldMessageQueue.Count > 0)
                {
                    lock (worldMessageQueue)
                    {
                        if (worldMessageQueue.Count > 0)
                            return worldMessageQueue.Dequeue();
                    }
                }
                return null;
            }
            set
            {
                lock (worldMessageQueue)
                {
                    ServerToClientMessage wm = value;
                    worldMessageQueue.Enqueue(wm);
                    Debug.Assert(wm.Sender != null);
                }
                WorldMessageDataArrived.Set();
            }
        }

        /// <summary>
        /// Reads/Writes to the server queue.
        /// </summary>
        public static ClientToServerMessage CharMngtQueueReadWrite
        {
            get
            {
                if (charMngtQueue.Count > 0)
                {
                    lock (charMngtQueue)
                    {
                        if (charMngtQueue.Count > 0)
                            return charMngtQueue.Dequeue();
                    }
                }
                return null;
            }
            set
            {
                lock (charMngtQueue)
                {
                    charMngtQueue.Enqueue(value);
                }
                CharMngtQueueDataArrived.Set();
            }
        }

        /// <summary>
        /// Reads/Writes to the server queue.
        /// </summary>
        public static ClientToServerMessage ChatQueueReadWrite
        {
            get
            {
                if (chatQueue.Count > 0)
                {
                    lock (chatQueue)
                    {
                        if (chatQueue.Count > 0)
                            return chatQueue.Dequeue();
                    }
                }
                return null;
            }
            set
            {
                lock (chatQueue)
                {
                    chatQueue.Enqueue(value);
                }
                ChatQueueDataArrived.Set();
            }
        }

        /// <summary>
        /// Reads/Writes to the server queue.
        /// </summary>
        public static ServerToServerMessage InterServerQueueReadWrite
        {
            get
            {
                if (interServerQueue.Count > 0)
                {
                    lock (interServerQueue)
                    {
                        if (interServerQueue.Count > 0)
                            return interServerQueue.Dequeue();
                    }
                }
                return null;
            }
            set
            {
                lock (interServerQueue)
                {
                    interServerQueue.Enqueue(value);
                }
                InterServerQueueDataArrived.Set();
            }
        }

        /// <summary>
        /// Reads/Writes to the network queue.
        /// </summary>
        public static NetIncomingMessage NetworkQueueReadWrite
        {
            get
            {
                if (rawNetworkQueue.Count > 0)
                {
                    lock (rawNetworkQueue)
                    {
                        if (rawNetworkQueue.Count > 0)
                            return rawNetworkQueue.Dequeue();
                    }
                }
                return null;
            }
            set
            {
                lock (rawNetworkQueue)
                {
                    rawNetworkQueue.Enqueue(value);
                }
                NetQueueDataArrived.Set();
            }
        }

        /// <summary>
        /// Write only property for storing new player messages into the turn queue.
        /// </summary>
        public static ClientToServerMessage TurnQueueWrite
        {
            set
            {
                lock (locker)
                {
                    currentTurnWriteQueue.Enqueue(value);
                }
            }
        }
        #endregion

        #region Constructors
        static QueueManager() { }
        private QueueManager() { }
        #endregion

        #region Methods
        /// <summary>
        /// Initializes the queues.
        /// </summary>
        /// <param name="maxConnections"></param>
        public void CreateQueues(int maxConnections)
        {
            if (isInitialized)
                return;

            rawNetworkQueue = new Queue<NetIncomingMessage>(maxConnections * 2);
            currentTurnReadQueue = new Queue<ClientToServerMessage>(maxConnections);
            currentTurnWriteQueue = new Queue<ClientToServerMessage>(maxConnections);
            interServerQueue = new Queue<ServerToServerMessage>(32);
            charMngtQueue = new Queue<ClientToServerMessage>(32);
            chatQueue = new Queue<ClientToServerMessage>(128);
            worldMessageQueue = new Queue<ServerToClientMessage>(maxConnections);

            isInitialized = true;
        }

        /// <summary>
        /// Swaps the active turn queue.
        /// The callee must ensure that the queue returned from the last call is empty.
        /// </summary>
        /// <returns>The queue from which the server reads messages for current turn processing.</returns>
        public Queue<ClientToServerMessage> SwapTurnQueue()
        {
            lock (locker)
            {
                swapQueuePointer = currentTurnWriteQueue;
                currentTurnWriteQueue = currentTurnReadQueue;
                currentTurnReadQueue = swapQueuePointer;
            }
            return swapQueuePointer;
        }

        /// <summary>
        /// Samples current queue sizes.
        /// </summary>
        /// <returns></returns>
        public QueueSample SampleQueueSizes()
        {
            sample.RawQueueSize = rawNetworkQueue.Count;
            lock (locker)
            {
                sample.TurnReadQueueSize = currentTurnReadQueue.Count;
                sample.TurnWriteQueueSize = currentTurnWriteQueue.Count;
            }
            sample.ServerQueueSize = interServerQueue.Count;
            sample.CharMngtQueueSize = charMngtQueue.Count;
            sample.ChatQueueSize = chatQueue.Count;
            sample.WorldMessageQueueSize = worldMessageQueue.Count;
            return sample;
        }
        #endregion
    }
}
