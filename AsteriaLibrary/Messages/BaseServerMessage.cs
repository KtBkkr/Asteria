using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Lidgren.Network;
using AsteriaLibrary.Data;

namespace AsteriaLibrary.Messages
{
    [Serializable()]
    public class BaseServerMessage
    {
        #region Fields
        public MessageType MessageType;
        public NetDeliveryMethod DeliveryMethod = NetDeliveryMethod.ReliableUnordered;
        public int DeliveryChannel;

        public string Data;
        public int Code;

        private static ObjectPool<BaseServerMessage> pool = new ObjectPool<BaseServerMessage>();
        #endregion

        #region Methods
        public static BaseServerMessage CreateMessage()
        {
            return pool.Get();
        }

        public static void Free(BaseServerMessage msg)
        {
            pool.Free(msg);
        }
        #endregion
    }
}
