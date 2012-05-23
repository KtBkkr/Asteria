using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using AsteriaLibrary.Messages;

namespace AsteriaLibrary.Serialization
{
    public sealed class ServerToServerMessageSerializer : BaseSerializer<ServerToServerMessage>
    {
        #region Methods
        public override byte[] Serialize(ServerToServerMessage msg)
        {
            ms.Position = 0;
            SerializeMessage(msg);
            return ms.ToArray();
        }

        public byte[] Serialize(List<ServerToServerMessage> messages)
        {
            ms.Position = 0;
            foreach (ServerToServerMessage msg in messages)
            {
                SerializeMessage(msg);
                ServerToServerMessage.FreeSafe(msg);
            }
            return ms.ToArray();
        }

        public override ServerToServerMessage Deserialize(byte[] buffer)
        {
            ServerToServerMessage msg;
            using (MemoryStream memoryStream = new MemoryStream(buffer))
            {
                using (BinaryReader reader = new BinaryReader(memoryStream))
                {
                    msg = DeserializeMessage(reader);
                }
            }
            return msg;
        }

        public ServerToServerMessage[] Deserialize(byte[] buffer, int count)
        {
            ServerToServerMessage[] result = new ServerToServerMessage[count];
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

        private ServerToServerMessage DeserializeMessage(BinaryReader reader)
        {
            ServerToServerMessage msg = ServerToServerMessage.CreateMessageSafe();
            msg.MessageType = (MessageType)reader.ReadInt32();
            msg.Data = reader.ReadString();
            msg.Code = reader.ReadInt32();
            return msg;
        }

        private void SerializeMessage(ServerToServerMessage msg)
        {
            writer.Write((int)msg.MessageType);
            writer.Write(msg.Data == null ? string.Empty : msg.Data);
            writer.Write(msg.Code);
        }
        #endregion
    }
}
