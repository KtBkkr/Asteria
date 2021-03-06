﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AsteriaClient.Interface.Controls
{
    public abstract class ButtonBase : Control
    {
        #region Properties
        public override ControlState ControlState
        {
            get
            {
                if (DesignMode) return ControlState.Enabled;
                else if (Suspended) return ControlState.Disabled;
                else
                {
                    if (!Enabled) return ControlState.Disabled;

                    if ((Pressed[(int)MouseButton.Left] && Inside) || (Focused && Pressed[(int)MouseButton.None])) return ControlState.Pressed;
                    else if (Hovered && Inside) return ControlState.Hovered;
                    else if ((Focused && !Inside) || (Hovered && !Inside) || (Focused && !Hovered && Inside)) return ControlState.Focused;
                    else return ControlState.Enabled;
                }
            }
        }  
        #endregion

        #region Constructors
        protected ButtonBase(Manager manager)
            : base(manager)
        {
            SetDefaultSize(72, 24);
            DoubleClicks = false;
        }
        #endregion

        #region Methods
        public override void Init()
        {
            base.Init();
        }

        protected override void OnClick(EventArgs e)
        {
            MouseEventArgs ex = (e is MouseEventArgs) ? (MouseEventArgs)e : new MouseEventArgs();
            if (ex.Button == MouseButton.Left || ex.Button == MouseButton.None)
            {
                base.OnClick(ex);
            }
        }
        #endregion
    }
}
