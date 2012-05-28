using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AsteriaClient.Interface.Controls
{
    public class BottomBarPanel : Container
    {
        #region Constructors
        public BottomBarPanel(Manager manager)
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
