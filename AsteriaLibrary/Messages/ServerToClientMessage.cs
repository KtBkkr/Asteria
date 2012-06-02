using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;
using Lidgren.Network;
using AsteriaLibrary.Data;

namespace AsteriaLibrary.Messages
{
    /// <summary>
    /// Represents an asteria world server to client message.
    /// This is the only type of messages a world server ever sends to its clients.
    /// </summary>
    public class ServerToClientMessage : BaseServerMessage, System.IDisposable
    {
        #region Fields
        public NetConnection Sender;
        public int TurnNumber;
        public byte[] Buffer;
        public bool IsAllocatedFromPool;
        public object Tag;

        private static ObjectPoolSafe<ServerToClientMessage> safePool = new ObjectPoolSafe<ServerToClientMessage>();
        public bool isDisposed = false;
        #endregion

        #region Constructors
        public ServerToClientMessage()
        {
            isDisposed = false;
        }

        ~ServerToClientMessage()
        {
            if (!isDisposed)
                Dispose();
        }
        #endregion

        #region Methods
        public static ServerToClientMessage CreateMessageSafe()
        {
            ServerToClientMessage wm = safePool.Get();
            Debug.Assert(!wm.IsAllocatedFromPool, "Allocated ServerToClientMessage from pool which is in use!");
            wm.IsAllocatedFromPool = true;
            return wm;
        }

        public static ServerToClientMessage CreateMessageSafe(NetConnection sender)
        {
            ServerToClientMessage wm = safePool.Get();
            Debug.Assert(!wm.IsAllocatedFromPool, "Allocated ServerToClientMessage from pool which is in use!");
            wm.Sender = sender;
            wm.IsAllocatedFromPool = true;
            return wm;
        }

        public static void FreeSafe(ServerToClientMessage msg)
        {
            if (msg.IsAllocatedFromPool)
            {
                msg.IsAllocatedFromPool = false;
                safePool.Free(msg);
            }
        }

        public static ServerToClientMessage Copy(ServerToClientMessage a, NetConnection sender)
        {
            ServerToClientMessage wm = safePool.Get();
            Debug.Assert(!wm.IsAllocatedFromPool, "Allocated ServerToClientMessage from pool which is in use!");

            wm.IsAllocatedFromPool = true;
            wm.Sender = sender;
            wm.MessageType = a.MessageType;
            wm.Buffer = a.Buffer;
            wm.Data = a.Data;
            wm.Code = a.Code;
            wm.TurnNumber = a.TurnNumber;
            wm.DeliveryMethod = a.DeliveryMethod;
            wm.DeliveryChannel = a.DeliveryChannel;

            return wm;
        }

        public void Dispose()
        {
            isDisposed = true;

            Debug.Assert((IsAllocatedFromPool == false), "ServerToClientMessage.Dispose() found non pooled message disposing!");

            if (IsAllocatedFromPool)
                FreeSafe(this);

            GC.SuppressFinalize(this);
        }
        #endregion
    }
}
