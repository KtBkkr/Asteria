using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AsteriaWorldServer
{
    public class EntityData
    {
        #region Fields
        public int TypeId;
        public string Name;
        public string Description;
        public Dictionary<string, int> Attributes = new Dictionary<string, int>();
        public Dictionary<string, string> Properties = new Dictionary<string, string>();
        #endregion

        #region Methods
        public override string ToString()
        {
            return Name + " (id=" + TypeId.ToString() + ")";
        }
        #endregion
    }
}
