using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Lidgren.Network;
using AsteriaLibrary.Data;

namespace AsteriaLibrary.Messages
{
    public class BaseClientMessage
    {
        #region Fields
        public int AccountId;
        public MessageType MessageType;
        public NetDeliveryMethod DeliveryMethod = NetDeliveryMethod.ReliableUnordered;
        public string Data;

        private static ObjectPool<BaseClientMessage> pool = new ObjectPool<BaseClientMessage>();
        private static ObjectPoolSafe<BaseClientMessage> safePool = new ObjectPoolSafe<BaseClientMessage>();
        #endregion

        #region Methods
        public static BaseClientMessage CreateMessageSafe()
        {
            return safePool.Get();
        }

        public static BaseClientMessage CreateMessage()
        {
            return pool.Get();
        }

        public static void FreeSafe(BaseClientMessage msg)
        {
            safePool.Free(msg);
        }

        public static void Free(BaseClientMessage msg)
        {
            pool.Free(msg);
        }
        #endregion
    }
}
