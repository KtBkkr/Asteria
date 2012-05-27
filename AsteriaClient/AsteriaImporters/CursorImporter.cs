using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content.Pipeline;
using Microsoft.Xna.Framework.Content.Pipeline.Serialization.Compiler;
using System.IO;

namespace AsteriaImporters
{
    public class CursorFile
    {
        public byte[] Data = null;
    }

    [ContentImporter(".cur", DisplayName = "Cursor - Asteria")]
    class CursorImporter : ContentImporter<CursorFile>
    {
        #region Methods
        public override CursorFile Import(string filename, ContentImporterContext context)
        {
            CursorFile cur = new CursorFile();
            cur.Data = File.ReadAllBytes(filename);
            return cur;
        }
        #endregion
    }

    [ContentTypeWriter]
    class CursorWriter : ContentTypeWriter<CursorFile>
    {
        #region Methods
        protected override void Write(ContentWriter output, CursorFile value)
        {
            output.Write(value.Data.Length);
            output.Write(value.Data);
        }

        public override string GetRuntimeType(TargetPlatform targetPlatform)
        {
            return typeof(CursorFile).AssemblyQualifiedName;
        }

        public override string GetRuntimeReader(TargetPlatform targetPlatform)
        {
            return "AsteriaClient.Interface.Controls.CursorReader, AsteriaClient";
        } 
        #endregion
    }
}
