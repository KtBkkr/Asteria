using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AsteriaClient.Interface;
using AsteriaClient.Network;
using AsteriaClient.Zones;

namespace AsteriaClient
{
    public class Context
    {
        #region Fields
        // Current version
        public string Protocol;

        // Contains any relevant graphics/mouse settings.
        public Game1 Game;

        // Handles mouse/keyboard input.
        public InputManager Input;

        // Handles user interface.
        public GameInterface Gui;

        // Handles communication with the server.
        public GameNetwork Network;

        // Handles zone and entity information.
        public ZoneManager ZoneManager;

        // Handles world/entity drawing and information.
        public WorldManager WorldManager;
        #endregion
    }
}
