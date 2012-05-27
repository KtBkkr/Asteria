using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using Microsoft.Xna.Framework.Content;
using System.IO;
using System.Windows.Forms;

namespace AsteriaClient.Interface.Controls
{
    public class SkinXmlDocument : XmlDocument { }

    public class SkinReader : ContentTypeReader<SkinXmlDocument>
    {
        #region Methods
        protected override SkinXmlDocument Read(ContentReader input, SkinXmlDocument existingInstance)
        {
            if (existingInstance == null)
            {
                SkinXmlDocument doc = new SkinXmlDocument();
                doc.LoadXml(input.ReadString());
                return doc;
            }
            else
                existingInstance.LoadXml(input.ReadString());

            return existingInstance;
        }
        #endregion
    }

    public class CursorReader : ContentTypeReader<Cursor>
    {
        #region Methods
        protected override Cursor Read(ContentReader input, Cursor existingInstance)
        {
            if (existingInstance == null)
            {
                int count = input.ReadInt32();
                byte[] data = input.ReadBytes(count);

                string path = Path.GetTempFileName();
                File.WriteAllBytes(path, data);

                IntPtr handle = NativeMethods.LoadCursor(path);
                Cursor cur = new Cursor(handle);
                File.Delete(path);

                return cur;
            }
            else
            {
            }
            return existingInstance;
        }
        #endregion

    }   
}
