using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content.Pipeline;
using Microsoft.Xna.Framework.Content.Pipeline.Serialization.Compiler;
using System.Xml;

namespace AsteriaImporters
{
    class SkinXmlDocument : XmlDocument { }

    [ContentImporter(".xml", DisplayName = "Skin - Asteria")]
    class SkinImporter : ContentImporter<SkinXmlDocument>
    {
        #region Methods
        public override SkinXmlDocument Import(string filename, ContentImporterContext context)
        {
            SkinXmlDocument doc = new SkinXmlDocument();
            doc.Load(filename);

            return doc;
        }
        #endregion
    }

    [ContentTypeWriter]
    class SkinWriter : ContentTypeWriter<SkinXmlDocument>
    {
        #region Methods
        protected override void Write(ContentWriter output, SkinXmlDocument value)
        {
            output.Write(value.InnerXml);
        }

        public override string GetRuntimeType(TargetPlatform targetPlatform)
        {
            return typeof(SkinXmlDocument).AssemblyQualifiedName;
        }

        public override string GetRuntimeReader(TargetPlatform targetPlatform)
        {
            return "AsteriaClient.Interface.Controls.SkinReader, AsteriaClient";
        }
        #endregion
    }
}
