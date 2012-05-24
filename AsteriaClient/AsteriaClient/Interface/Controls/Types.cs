using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AsteriaClient.Interface.Controls
{
    #region Enums
    public enum Message
    {
        Click,
        MouseDown,
        MouseUp,
        MousePress,
        MouseMove,
        MouseOver,
        MouseOut,
        KeyDown,
        KeyUp,
        KeyPress
    }

    public enum ControlState
    {
        Enabled,
        Hovered,
        Pressed,
        Focused,
        Disabled
    }

    public enum Alignment
    {
        None,
        TopLeft,
        TopCenter,
        TopRight,
        MiddleLeft,
        MiddleCenter,
        MiddleRight,
        BottomLeft,
        BottomCenter,
        BottomRight
    }

    [Flags]
    public enum Anchors
    {
        None = 0x00,
        Left = 0x01,
        Top = 0x02,
        Right = 0x04,
        Bottom = 0x08,
        Horizontal = Left | Right,
        Vertical = Top | Bottom,
        All = Left | Top | Right | Bottom
    }

    public enum SizeMode
    {
        Normal,
        Auto,
        Centered,
        Stretched
    }
    #endregion

    #region Structs
    public struct Margins
    {
        public int Left;
        public int Top;
        public int Right;
        public int Bottom;

        public int Vertical { get { return (Top + Bottom); } }
        public int Horizontal { get { return (Left + Right); } }

        public Margins(int left, int top, int right, int bottom)
        {
            Left = left;
            Top = top;
            Right = right;
            Bottom = bottom;
        }
    }

    public struct Size
    {
        public int Width;
        public int Height;

        public static Size Zero { get { return new Size(0, 0); } }

        public Size(int width, int height)
        {
            Width = width;
            Height = height;
        }
    }
    #endregion
}
