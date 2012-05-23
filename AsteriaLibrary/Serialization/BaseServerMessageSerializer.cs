using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using AsteriaLibrary.Messages;

namespace AsteriaLibrary.Serialization
{
    public class BaseServerMessageSerializer : BaseSerializer<BaseServerMessage>
    {
        #region Methods
        public override byte[] Serialize(BaseServerMessage msg)
        {
            ms.Position = 0;
            writer.Write((int)msg.MessageType);
            writer.Write(msg.Data);
            writer.Write(msg.Code);
            return ms.ToArray();
        }

        public override BaseServerMessage Deserialize(byte[] buffer)
        {
            BaseServerMessage msg = BaseServerMessage.CreateMessage();
            using (MemoryStream memoryStream = new MemoryStream(buffer))
            {
                using (BinaryReader reader = new BinaryReader(memoryStream))
                {
                    msg.MessageType = (MessageType)reader.ReadInt32();
                    msg.Data = reader.ReadString();
                    msg.Code = reader.ReadInt32();
                    return msg;
                }
            }
        }
        #endregion
    }
}
