using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AsteriaWorldServer.PlayerCache
{
    /// <summary>
    /// Character flood prevention record. The FloodEntry is attached to an MPR instance.
    /// </summary>
    public sealed class FloodEntry
    {
        #region Fields
        private int actionFrequency;
        private int chatFrequency;

        public bool IsConnectionAllowed;
        public bool IsFlooder;

        public DateTime NextChatAllowed;
        public DateTime NextActionAllowed;

        public DateTime LastActivity;
        public DateTime LastChat;
        #endregion

        #region Constructors
        public FloodEntry() : this(10, 100) { }

        public FloodEntry(int actionFrequency, int chatFrequency)
        {
            this.actionFrequency = actionFrequency;
            this.chatFrequency = chatFrequency;
            Reset();
        }
        #endregion

        #region Methods
        public void AddConnectionPrevent(int milliseconds)
        {
            NextActionAllowed = DateTime.Now.AddMilliseconds(milliseconds);
            IsConnectionAllowed = false;
            IsFlooder = true;
        }

        // TODO: [HIGH] invoke this method before processing any actions.
        public bool IsChatAllowed()
        {
            LastChat = DateTime.Now;
            if (NextChatAllowed < DateTime.Now)
            {
                NextChatAllowed = DateTime.Now.AddMilliseconds(chatFrequency);
                return true;
            }
            else
                return false; // TODO: [LOW] we could update a counter here to track the number of floods.
        }

        // TODO: [HIGH] invoke this method before processing any chats.
        public bool IsActionAllowed()
        {
            LastActivity = DateTime.Now;
            if (NextActionAllowed < DateTime.Now)
            {
                NextActionAllowed = DateTime.Now.AddMilliseconds(actionFrequency);
                return true;
            }
            else
                return false; // TODO: [LOW] we could update a counter here to track the number of floods.
        }

        public void Reset()
        {
            IsConnectionAllowed = true;
            IsFlooder = false;
            NextActionAllowed = NextChatAllowed = DateTime.MinValue;
        }
        #endregion
    }
}
