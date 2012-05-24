using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Input;

namespace AsteriaClient.Interface.Controls
{
    #region Enums
    [Flags]
    public enum InputMethods
    {
        None = 0x00,
        Keyboard = 0x01,
        Mouse = 0x02,
        All = Keyboard | Mouse
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

    #region Structs
    public struct InputOffset
    {
        public int X;
        public int Y;
        public float RatioX;
        public float RatioY;

        public InputOffset(int x, int y, float rx, float ry)
        {
            X = x;
            Y = y;
            RatioX = rx;
            RatioY = ry;
        }
    }
    #endregion

    #region Classes
    public class InputSystem : Disposable
    {
        #region Classes
        private class InputKey
        {
            public Keys Key = Keys.None;
            public bool Pressed = false;
            public double Countdown = RepeatDelay;
        }

        private class InputMouseButton
        {
            public MouseButton Button = MouseButton.None;
            public bool Pressed = false;
            public double Countdown = RepeatDelay;

            public InputMouseButton()
            {
            }

            public InputMouseButton(MouseButton button)
            {
                Button = button;
            }
        }
        #endregion
    }
    #endregion
}
