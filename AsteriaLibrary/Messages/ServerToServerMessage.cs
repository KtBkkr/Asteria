using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;
using Lidgren.Network;
using AsteriaLibrary.Data;

namespace AsteriaLibrary.Messages
{
    public class ServerToServerMessage : BaseServerMessage, System.IDisposable
    {
        #region Fields
        public NetConnection Sender;
        public bool IsAllocatedFromPool;
        public object Tag;

        private static ObjectPoolSafe<ServerToServerMessage> safePool = new ObjectPoolSafe<ServerToServerMessage>();
        private bool isDisposed = false;
        #endregion

        #region Constructors
        public ServerToServerMessage()
        {
            isDisposed = false;
        }

        ~ServerToServerMessage()
        {
            if (!isDisposed)
                Dispose();
        }
        #endregion

        #region Methods
        public static ServerToServerMessage CreateMessageSafe()
        {
            ServerToServerMessage wm = safePool.Get();
            Debug.Assert(!wm.IsAllocatedFromPool, "Allocated ServerToServerMessage from pool which is in use!");
            wm.IsAllocatedFromPool = true;
            return wm;
        }

        public static ServerToServerMessage CreateMessageSafe(NetConnection sender)
        {
            ServerToServerMessage wm = safePool.Get();
            Debug.Assert(!wm.IsAllocatedFromPool, "Allocated ServerToServerMessage from pool which is in use!");
            wm.Sender = sender;
            wm.IsAllocatedFromPool = true;
            return wm;
        }

        public static void FreeSafe(ServerToServerMessage msg)
        {
            if (msg.IsAllocatedFromPool)
            {
                msg.IsAllocatedFromPool = false;
                safePool.Free(msg);
            }
        }

        public static ServerToServerMessage Copy(ServerToServerMessage a, NetConnection sender)
        {
            ServerToServerMessage wm = safePool.Get();
            Debug.Assert(!wm.IsAllocatedFromPool, "Allocated ServerToServerMessage from pool which is in use!");

            wm.IsAllocatedFromPool = true;
            wm.Sender = sender;
            wm.MessageType = a.MessageType;
            wm.Data = a.Data;
            wm.Code = a.Code;

            return wm;
        }

        public void Dispose()
        {
            isDisposed = true;

            Debug.Assert(IsAllocatedFromPool, "ServerToServerMessage.Dispose() found non pooled message disposing!");

            if (IsAllocatedFromPool)
                FreeSafe(this);

            GC.SuppressFinalize(this);
        }
        #endregion
    }
}
