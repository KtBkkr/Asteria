using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AsteriaLibrary.Messages;
using System.IO;

namespace AsteriaLibrary.Serialization
{
    /// <summary>
    /// Serializer for ClientToServerMessage instances.
    /// Supports serializing byte arrays and deserializing from byte arrays.
    /// The fields are serialized/deserialized in this order: AccountId, CharacterId, Action, Data, GameData
    /// </summary>
    public sealed class ClientToServerMessageSerializer : BaseSerializer<ClientToServerMessage>
    {
        #region Constructors
        public ClientToServerMessageSerializer() : base() { }
        #endregion

        #region Methods
        /// <summary>
        /// Serializes a ClientToServerMessage instance into a byte array.
        /// </summary>
        /// <param name="msg"></param>
        /// <returns></returns>
        public override byte[] Serialize(ClientToServerMessage msg)
        {
            ms.Position = 0;
            writer.Write(msg.AccountId);
            writer.Write((int)msg.MessageType);
            writer.Write(msg.CharacterId);
            writer.Write(msg.Action);
            writer.Write(msg.Data == null ? string.Empty : msg.Data);
            writer.Write(msg.GameData == null ? string.Empty : msg.GameData);
            return ms.ToArray();
        }

        /// <summary>
        /// Deserializes a byte array into a ClientToServerMessage instance.
        /// Note that after using ClientToServerMessage instance it should be returned to the thread safe object pool with ClientToServerMessage.FreeSafe().
        /// </summary>
        /// <param name="buffer"></param>
        /// <returns></returns>
        public override ClientToServerMessage Deserialize(byte[] buffer)
        {
            ClientToServerMessage msg = ClientToServerMessage.CreateMessageSafe();
            using (MemoryStream memoryStream = new MemoryStream(buffer))
            {
                using (BinaryReader reader = new BinaryReader(memoryStream))
                {
                    msg.AccountId = reader.ReadInt32();
                    msg.MessageType = (MessageType)reader.ReadInt32();
                    msg.CharacterId = reader.ReadInt32();
                    msg.Action = reader.ReadInt32();
                    msg.Data = reader.ReadString();
                    msg.GameData = reader.ReadString();
                    return msg;
                }
            }
        }
        #endregion
    }
}
