using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AsteriaClient
{
    public class CharacterMgtEventArgs : EventArgs
    {
        #region Fields
        public int Id;
        public string Name;
        #endregion

        #region Constructors
        public CharacterMgtEventArgs() { }

        public CharacterMgtEventArgs(int id, string name)
        {
            Id = id;
            Name = name;
        }
        #endregion
    }
}
