using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AsteriaClient.Interface.Controls
{
    public class SideBarPanel : Container
    {
        #region Constructors
        public SideBarPanel(Manager manager)
            : base(manager)
        {
            CanFocus = false;
            Passive = true;
            Width = 64;
            Height = 64;
        }
        #endregion

        #region Methods
        public override void Init()
        {
            base.Init();
        }
        #endregion
    }
}
