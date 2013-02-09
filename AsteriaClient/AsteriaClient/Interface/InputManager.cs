using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework;

namespace AsteriaClient.Interface
{
    public class InputManager
    {
        #region Fields
        private Context context;

        private KeyboardState keyboardState;
        private KeyboardState lastKeyboardState;

        private MouseState mouseState;
        private MouseState lastMouseState;
        #endregion

        #region Constructors
        public InputManager(Context context)
        {
            this.context = context;
        }
        #endregion

        #region Methods
        public void Update(GameTime gameTime)
        {
            lastKeyboardState = keyboardState;
            lastMouseState = mouseState;

            keyboardState = Keyboard.GetState();
            mouseState = Mouse.GetState();
        }

        /// <summary>
        /// Returns true if the specified key has the specified state.
        /// </summary>
        public bool CheckKeyState(Keys key, KeyState state)
        {
            if (state == KeyState.Down && keyboardState.IsKeyDown(key))
                return true;
            else if (state == KeyState.Up && keyboardState.IsKeyUp(key))
                return true;

            return false;
        }

        /// <summary>
        /// Returns true if all of the supplied keys share the supplied state.
        /// </summary>
        public bool CheckKeyStates(Keys[] keys, KeyState state)
        {
            foreach (Keys key in keys)
            {
                if (state == KeyState.Down && !keyboardState.IsKeyDown(key))
                    return false;
                else if (state == KeyState.Up && !keyboardState.IsKeyUp(key))
                    return false;
            }
            return true;
        }

        public bool CheckKeyPress(Keys key)
        {
            if (keyboardState.IsKeyDown(key) && lastKeyboardState.IsKeyUp(key))
                return true;

            return false;
        }
        #endregion
    }
}
