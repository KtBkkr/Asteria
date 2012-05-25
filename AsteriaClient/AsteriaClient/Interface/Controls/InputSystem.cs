using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.GamerServices;

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

        private class InputMouse
        {
            public MouseState State = new MouseState();
            public Point Position = new Point(0, 0);
        }
        #endregion

        #region Consts
        private const int RepeatDelay = 500;
        private const int RepeatRate = 50;
        private float ClickThreshold = 0.5f;
        #endregion

        #region Fields
        private System.Windows.Forms.Form window = null;
        private List<InputKey> keys = new List<InputKey>();
        private List<InputMouseButton> mouseButtons = new List<InputMouseButton>();
        private MouseState mouseState = new MouseState();
        private Manager manager = null;
        private InputOffset inputOffset = new InputOffset(0, 0, 1.0f, 1.0f);
        private InputMethods inputMethods = InputMethods.All;  
        #endregion

        #region Properties
        public virtual InputOffset InputOffset
        {
            get { return inputOffset; }
            set { inputOffset = value; }
        }

        public virtual InputMethods InputMethods
        {
            get { return inputMethods; }
            set { inputMethods = value; }
        }
        #endregion

        #region Events
        public event KeyEventHandler KeyDown;
        public event KeyEventHandler KeyPress;
        public event KeyEventHandler KeyUp;

        public event MouseEventHandler MouseDown;
        public event MouseEventHandler MousePress;
        public event MouseEventHandler MouseUp;
        public event MouseEventHandler MouseMove;
        #endregion

        #region Constructors
        public InputSystem(Manager manager, InputOffset offset)
        {
            this.inputOffset = offset;
            this.manager = manager;
            window = manager.Window;
        }

        public InputSystem(Manager manager) : this(manager, new InputOffset(0, 0, 1.0f, 1.0f)) { }
        #endregion

        #region Methods
        public virtual void Initialize()
        {
            keys.Clear();
            mouseButtons.Clear();

            foreach (string str in Enum.GetNames(typeof(Keys)))
            {
                InputKey key = new InputKey();
                key.Key = (Keys)Enum.Parse(typeof(Keys), str);
                keys.Add(key);
            }

            foreach (string str in Enum.GetNames(typeof(MouseButton)))
            {
                InputMouseButton btn = new InputMouseButton();
                btn.Button = (MouseButton)Enum.Parse(typeof(MouseButton), str);
                mouseButtons.Add(btn);
            }
        }

        public virtual void SendMouseState(MouseState state, GameTime gameTime)
        {
            UpdateMouse(state, gameTime);
        }

        public virtual void SendKeyboardState(KeyboardState state, GameTime gameTime)
        {
            UpdateKeys(state, gameTime);
        }

        public virtual void Update(GameTime gameTime)
        {
            if (manager.UseGuide && Guide.IsVisible) return;

            MouseState ms = Mouse.GetState();
            KeyboardState ks = Keyboard.GetState(PlayerIndex.One);

            if (window.Focused)
            {
                if ((inputMethods & InputMethods.Mouse) == InputMethods.Mouse) UpdateMouse(ms, gameTime);
                if ((inputMethods & InputMethods.Keyboard) == InputMethods.Keyboard) UpdateKeys(ks, gameTime);
            }
        }

        private void UpdateKeys(KeyboardState state, GameTime gameTime)
        {
            KeyEventArgs e = new KeyEventArgs();
            e.Caps = (((ushort)NativeMethods.GetKeyState(0x14)) & 0xffff) != 0;

            foreach (Keys key in state.GetPressedKeys())
            {
                if (key == Keys.LeftAlt || key == Keys.RightAlt) e.Alt = true;
                else if (key == Keys.LeftShift || key == Keys.RightShift) e.Shift = true;
                else if (key == Keys.LeftControl || key == Keys.RightControl) e.Control = true;
            }

            foreach (InputKey key in keys)
            {
                if (key.Key == Keys.LeftAlt || key.Key == Keys.RightAlt ||
                    key.Key == Keys.LeftShift || key.Key == Keys.RightShift ||
                    key.Key == Keys.LeftControl || key.Key == Keys.RightControl)
                {
                    continue;
                }

                bool pressed = state.IsKeyDown(key.Key);
                double ms = gameTime.ElapsedGameTime.TotalMilliseconds;
                if (pressed) key.Countdown -= ms;

                if ((pressed) && (!key.Pressed))
                {
                    key.Pressed = true;
                    e.Key = key.Key;

                    if (KeyDown != null) KeyDown.Invoke(this, e);
                    if (KeyPress != null) KeyPress.Invoke(this, e);
                }
                else if ((!pressed) && (key.Pressed))
                {
                    key.Pressed = false;
                    key.Countdown = RepeatDelay;
                    e.Key = key.Key;

                    if (KeyUp != null) KeyUp.Invoke(this, e);
                }
                else if (key.Pressed && key.Countdown < 0)
                {
                    key.Countdown = RepeatRate;
                    e.Key = key.Key;

                    if (KeyPress != null) KeyPress.Invoke(this, e);
                }
            }
        }

        private Point RecalcPosition(Point pos)
        {
            return new Point((int)((pos.X - InputOffset.X) / InputOffset.RatioX), (int)((pos.Y - InputOffset.Y) / InputOffset.RatioY));
        }

        private void AdjustPosition(ref MouseEventArgs e)
        {
            Rectangle screen = manager.Game.Window.ClientBounds;

            if (e.Position.X < 0) e.Position.X = 0;
            if (e.Position.Y < 0) e.Position.Y = 0;
            if (e.Position.X >= screen.Width) e.Position.X = screen.Width - 1;
            if (e.Position.Y >= screen.Height) e.Position.Y = screen.Height - 1;
        }

        private void BuildMouseEvent(MouseState state, MouseButton button, ref MouseEventArgs e)
        {
            e.State = state;
            e.Button = button;

            e.Position = new Point(state.X, state.Y);
            AdjustPosition(ref e);

            e.Position = RecalcPosition(e.Position);
            e.State = new MouseState(e.Position.X, e.Position.Y, e.State.ScrollWheelValue, e.State.LeftButton, e.State.MiddleButton, e.State.RightButton, e.State.XButton1, e.State.XButton2);

            Point pos = RecalcPosition(new Point(mouseState.X, mouseState.Y));
            e.Difference = new Point(e.Position.X - pos.X, e.Position.Y - pos.Y);
        }

        private void UpdateMouse(MouseState state, GameTime gameTime)
        {
            if ((state.X != mouseState.X) || (state.Y != mouseState.Y))
            {
                MouseEventArgs e = new MouseEventArgs();

                MouseButton btn = MouseButton.None;
                if (state.LeftButton == ButtonState.Pressed) btn = MouseButton.Left;
                else if (state.RightButton == ButtonState.Pressed) btn = MouseButton.Right;
                else if (state.MiddleButton == ButtonState.Pressed) btn = MouseButton.Middle;
                else if (state.XButton1 == ButtonState.Pressed) btn = MouseButton.XButton1;
                else if (state.XButton2 == ButtonState.Pressed) btn = MouseButton.XButton2;

                BuildMouseEvent(state, btn, ref e);
                if (MouseMove != null)
                    MouseMove.Invoke(this, e);
            }

            UpdateButtons(state, gameTime);
            mouseState = state;
        }

        private void UpdateButtons(MouseState state, GameTime gameTime)
        {
            MouseEventArgs e = new MouseEventArgs();
            foreach (InputMouseButton btn in mouseButtons)
            {
                ButtonState bs = ButtonState.Released;

                if (btn.Button == MouseButton.Left) bs = state.LeftButton;
                else if (btn.Button == MouseButton.Right) bs = state.RightButton;
                else if (btn.Button == MouseButton.Middle) bs = state.MiddleButton;
                else if (btn.Button == MouseButton.XButton1) bs = state.XButton1;
                else if (btn.Button == MouseButton.XButton2) bs = state.XButton2;
                else continue;

                bool pressed = (bs == ButtonState.Pressed);
                if (pressed)
                {
                    double ms = gameTime.ElapsedGameTime.TotalMilliseconds;
                    if (pressed) btn.Countdown -= ms;
                }

                if ((pressed) && (!btn.Pressed))
                {
                    btn.Pressed = true;
                    BuildMouseEvent(state, btn.Button, ref e);

                    if (MouseDown != null) MouseDown.Invoke(this, e);
                    if (MousePress != null) MousePress.Invoke(this, e);
                }
                else if ((!pressed) && (btn.Pressed))
                {
                    btn.Pressed = false;
                    btn.Countdown = RepeatDelay;
                    BuildMouseEvent(state, btn.Button, ref e);

                    if (MouseUp != null) MouseUp.Invoke(this, e);
                }
                else if (btn.Pressed && btn.Countdown < 0)
                {
                    e.Button = btn.Button;
                    btn.Countdown = RepeatRate;
                    BuildMouseEvent(state, btn.Button, ref e);

                    if (MousePress != null) MousePress.Invoke(this, e);
                }
            }
        }
        #endregion
    }
    #endregion
}
