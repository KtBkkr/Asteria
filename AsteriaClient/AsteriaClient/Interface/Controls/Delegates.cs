using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AsteriaClient.Interface.Controls
{
    #region Delegates
    public delegate void EventHandler(object sender, EventArgs e);
    public delegate void MouseEventHandler(object sender, MouseEventArgs e);
    public delegate void KeyEventHandler(object sender, KeyEventArgs e);
    public delegate void MoveEventHandler(object sender, MoveEventArgs e);
    public delegate void ResizeEventHandler(object sender, ResizeEventArgs e);
    #endregion
}
