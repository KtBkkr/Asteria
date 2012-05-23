using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using AsteriaLibrary.Messages;

namespace AsteriaLibrary.Serialization
{
    public sealed class ServerToClientMessageSerializer : BaseSerializer<ServerToClientMessage>
    {
        #region Methods
        public override byte[] Serialize(ServerToClientMessage msg)
        {
            ms.Position = 0;
            SerializeMessage(msg);
            return ms.ToArray();
        }

        public byte[] Serialize(List<ServerToClientMessage> messages)
        {
            ms.Position = 0;
            foreach (ServerToClientMessage msg in messages)
            {
                SerializeMessage(msg);
                ServerToClientMessage.FreeSafe(msg);
            }
            return ms.ToArray();
        }

        public override ServerToClientMessage Deserialize(byte[] buffer)
        {
            ServerToClientMessage msg;
            using (MemoryStream memoryStream = new MemoryStream(buffer))
            {
                using (BinaryReader reader = new BinaryReader(memoryStream))
                {
                    msg = DeserializeMessage(reader);
                }
            }
            return msg;
        }

        public ServerToClientMessage[] Deserialize(byte[] buffer, int count)
        {
            ServerToClientMessage[] result = new ServerToClientMessage[count];
            using (MemoryStream memoryStream = new MemoryStream(buffer))
            {
                using (BinaryReader reader = new BinaryReader(memoryStream))
                {
                    for (int i = 0; i < count; i++)
                        result[i] = DeserializeMessage(reader);
                }
            }
            return result;
        }

        private ServerToClientMessage DeserializeMessage(BinaryReader reader)
        {
            ServerToClientMessage msg = ServerToClientMessage.CreateMessageSafe();
            msg.MessageType = (MessageType)reader.ReadInt32();
            msg.Data = reader.ReadString();
            msg.Code = reader.ReadInt32();
            msg.TurnNumber = reader.ReadInt32();
            int byteCount = reader.ReadInt32();
            if (byteCount > 0)
                msg.Buffer = reader.ReadBytes(byteCount);

            return msg;
        }

        private void SerializeMessage(ServerToClientMessage msg)
        {
            writer.Write((int)msg.MessageType);
            writer.Write(msg.Data == null ? string.Empty : msg.Data);
            writer.Write(msg.Code);
            writer.Write(msg.TurnNumber);
            if (msg.Buffer != null)
            {
                writer.Write(msg.Buffer.Length);
                writer.Write(msg.Buffer);
            }
            else
                writer.Write(0);
        }
        #endregion
    }
}
