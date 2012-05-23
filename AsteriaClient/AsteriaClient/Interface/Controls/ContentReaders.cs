using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using Microsoft.Xna.Framework.Content;

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
}
