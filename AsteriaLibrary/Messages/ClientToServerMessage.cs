using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Lidgren.Network;
using AsteriaLibrary.Data;

namespace AsteriaLibrary.Messages
{
    public class ClientToServerMessage : BaseClientMessage
    {
        #region Fields
        public int CharacterId;
        public int Action;
        public string GameData;
        public NetConnection Sender;

        private static ObjectPoolSafe<ClientToServerMessage> safePool = new ObjectPoolSafe<ClientToServerMessage>();
        #endregion

        #region Methods
        public static new ClientToServerMessage CreateMessageSafe() { return safePool.Get(); }
        public static void FreeSafe(ClientToServerMessage msg) { safePool.Free(msg); }
        #endregion
    }
}
