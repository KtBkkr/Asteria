using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AsteriaClient.Interface.Controls
{
    public class ContextMenu : MenuBase
    {
        #region Variables
        private long timer = 0;
        private Control sender = null;
        #endregion

        #region Properties
        protected internal Control Sender
        {
            get { return sender; }
            set { sender = value; }
        }
        #endregion

        #region Constructors
        public ContextMenu(Manager manager)
            : base(manager)
        {
            
        }
        #endregion
    }
}
