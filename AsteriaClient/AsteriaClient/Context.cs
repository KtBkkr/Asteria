using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AsteriaClient.Interface;
using AsteriaClient.Network;

namespace AsteriaClient
{
    public class Context
    {
        #region Fields
        public string Protocol;
        public Game1 Game;
        public GameInterface Gui;
        public GameNetwork Network;
        #endregion
    }
}
