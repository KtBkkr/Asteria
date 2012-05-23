using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AsteriaWorldServer.Messages
{
    /// <summary>
    /// Represents a snapshot of all server queue sizes.
    /// </summary>
    public struct QueueSample
    {
        /// <summary>
        /// Size of the networking queue.
        /// </summary>
        public int RawQueueSize;

        /// <summary>
        /// Size of the message queue receiving deserialized messages.
        /// </summary>
        public int TurnReadQueueSize;

        /// <summary>
        /// Size of the message queue being processed for the current turn.
        /// </summary>
        public int TurnWriteQueueSize;

        /// <summary>
        /// Size of the lower priority server communication queue.
        /// </summary>
        public int ServerQueueSize;

        /// <summary>
        /// Size of the lower priority character management queue.
        /// </summary>
        public int CharMngtQueueSize;

        /// <summary>
        /// Size of the lower priority chat queue.
        /// </summary>
        public int ChatQueueSize;

        /// <summary>
        /// Size of the outgoing message queue.
        /// </summary>
        public int WorldMessageQueueSize;
    }
}
