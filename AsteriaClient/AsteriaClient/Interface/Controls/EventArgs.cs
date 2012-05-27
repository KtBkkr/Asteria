using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace AsteriaClient.Interface.Controls
{
    public class EventArgs : System.EventArgs
    {
        #region Fields
        public bool Handled = false;
        #endregion

        #region Constructors
        public EventArgs() { }
        #endregion
    }

    public class KeyEventArgs : EventArgs
    {
        #region Fields
        public Keys Key = Keys.None;
        public bool Control = false;
        public bool Shift = false;
        public bool Alt = false;
        public bool Caps = false;
        #endregion

        #region Constructors
        public KeyEventArgs() { }

        public KeyEventArgs(Keys key)
        {
            Key = key;
            Control = false;
            Shift = false;
            Alt = false;
            Caps = false;
        }

        public KeyEventArgs(Keys key, bool control, bool shift, bool alt, bool caps)
        {
            Key = key;
            Control = control;
            Shift = shift;
            Alt = alt;
            Caps = caps;
        }
        #endregion
    }

    public class MouseEventArgs : EventArgs
    {
        #region Fields
        public MouseState State = new MouseState();
        public MouseButton Button = MouseButton.None;
        public Point Position = new Point(0, 0);
        public Point Difference = new Point(0, 0);
        #endregion

        #region Constructors
        public MouseEventArgs() { }

        public MouseEventArgs(MouseState state, MouseButton button, Point position)
        {
            State = state;
            Button = button;
            Position = position;
        }

        public MouseEventArgs(MouseEventArgs e)
        {
            State = e.State;
            Button = e.Button;
            Position = e.Position;
            Difference = e.Difference;
        }
        #endregion
    }

    public class ResizeEventArgs : EventArgs
    {
        #region Fields
        public int Width = 0;
        public int Height = 0;
        public int OldWidth = 0;
        public int OldHeight = 0;
        #endregion

        #region Constructors
        public ResizeEventArgs() { }

        public ResizeEventArgs(int width, int height, int oldWidth, int oldHeight)
        {
            Width = width;
            Height = height;
            OldWidth = oldWidth;
            OldHeight = oldHeight;
        }
        #endregion
    }

    public class MoveEventArgs : EventArgs
    {
        #region Fields
        public int Left = 0;
        public int Top = 0;
        public int OldLeft = 0;
        public int OldTop = 0;
        #endregion

        #region Constructors
        public MoveEventArgs() { }

        public MoveEventArgs(int left, int top, int oldLeft, int oldTop)
        {
            Left = left;
            Top = top;
            OldLeft = oldLeft;
            OldTop = oldTop;
        }
        #endregion
    }

    public class DrawEventArgs : EventArgs
    {
        #region Fields
        public Renderer Renderer = null;
        public Rectangle Rectangle = Rectangle.Empty;
        public GameTime GameTime = null;
        #endregion

        #region Constructors
        public DrawEventArgs() { }

        public DrawEventArgs(Renderer renderer, Rectangle rectangle, GameTime gameTime)
        {
            Renderer = renderer;
            Rectangle = rectangle;
            GameTime = gameTime;
        }
        #endregion
    }

    public class DeviceEventArgs : EventArgs
    {
        #region Fields
        public PreparingDeviceSettingsEventArgs DeviceSettings = null;
        #endregion

        #region Constructors
        public DeviceEventArgs() { }

        public DeviceEventArgs(PreparingDeviceSettingsEventArgs deviceSettings)
        {
            DeviceSettings = deviceSettings;
        }
        #endregion
    }

    public class WindowClosingEventArgs : EventArgs
    {
        #region Fields
        public bool Cancel = false;
        #endregion

        #region Constructors
        public WindowClosingEventArgs() { }
        #endregion
    }

    public class WindowClosedEventArgs : EventArgs
    {
        #region Fields
        public bool Dispose = false;
        #endregion

        #region Constructors
        public WindowClosedEventArgs() { }
        #endregion
    }

    public class ConsoleMessageEventArgs : EventArgs
    {
        #region Fields
        public ConsoleMessage Message;
        #endregion

        #region Constructors
        public ConsoleMessageEventArgs() { }

        public ConsoleMessageEventArgs(ConsoleMessage message)
        {
            Message = message;
        }
        #endregion
    }
}
