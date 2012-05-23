using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AsteriaClient.Interface.Controls
{
    #region Enums
    [Flags]
    public enum InputMethods
    {
        None = 0x00,
        Keyboard = 0x01,
        Mouse = 0x02,
        GamePad = 0x04,
        All = Keyboard | Mouse | GamePad
    }

    public enum MouseButton
    {
        None = 0,
        Left,
        Right,
        Middle,
        XButton1,
        XButton2
    }
    #endregion
}
