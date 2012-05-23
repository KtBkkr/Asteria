using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace AsteriaLibrary.Serialization
{
    public abstract class BaseSerializer<T> where T : new()
    {
        #region Fields
        protected MemoryStream ms;
        protected BinaryWriter writer;

        private Decoder decoder;
        private Encoder encoder;

        public const int BYTE_BUFFER_SIZE = 512;
        public const int MAX_STRING_LEN = 4096 * 8;
        #endregion

        #region Constructors
        public BaseSerializer()
        {
            ms = new MemoryStream(MAX_STRING_LEN);
            writer = new BinaryWriter(ms);
            UTF8Encoding utf8 = new UTF8Encoding();
            decoder = utf8.GetDecoder();
            encoder = utf8.GetEncoder();
        }
        #endregion

        #region Abstract Methods
        abstract public byte[] Serialize(T msg);
        abstract public T Deserialize(byte[] buffer);
        #endregion
    }
}
