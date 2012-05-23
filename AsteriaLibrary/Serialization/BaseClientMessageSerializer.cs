using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using AsteriaLibrary.Messages;

namespace AsteriaLibrary.Serialization
{
    public class BaseClientMessageSerializer : BaseSerializer<BaseClientMessage>
    {
        #region Methods
        public override byte[] Serialize(BaseClientMessage msg)
        {
            ms.Position = 0;
            writer.Write(msg.AccountId);
            writer.Write((int)msg.MessageType);
            writer.Write(msg.Data == null ? string.Empty : msg.Data);
            return ms.ToArray();
        }

        public override BaseClientMessage Deserialize(byte[] buffer)
        {
            BaseClientMessage msg = BaseClientMessage.CreateMessageSafe();
            using (MemoryStream memoryStream = new MemoryStream(buffer))
            {
                using (BinaryReader reader = new BinaryReader(memoryStream))
                {
                    msg.AccountId = reader.ReadInt32();
                    msg.MessageType = (MessageType)reader.ReadInt32();
                    msg.Data = reader.ReadString();
                    return msg;
                }
            }
        }
        #endregion
    }
}
