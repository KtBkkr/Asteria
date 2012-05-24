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
            Visible = false;
            Detached = true;
            StayOnBack = true;

            Manager.Input.MouseDown += new MouseEventHandler(Input_MouseDown);
        }
        #endregion

        #region Destructors
        protected override void Dispose(bool disposing)
        {
            if (disposing)
                Manager.Input.MouseDown -= Input_MouseDown;

            base.Dispose(disposing);
        }
        #endregion

        #region Methods
        public override void Init()
        {
            base.Init();
        }

        protected internal override void InitSkin()
        {
            base.InitSkin();
            Skin = new SkinControl(Manager.Skin.Controls["ContextMenu"]);
        }
        #endregion
    }
}
