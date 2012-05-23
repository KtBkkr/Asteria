using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;
using AsteriaLibrary.Shared;
using AsteriaLibrary.Client;
using AsteriaLibrary.Entities;
using AsteriaLibrary.Messages;
using Neoforce = TomShane.Neoforce.Controls;
using AsteriaClient.Interface;

namespace AsteriaClient
{
    delegate void FillCharacterListDelegate();
    delegate void DisplayWorldMessageDelegate();
    delegate void LogMessageDelegate();
    delegate void StateChangedDelegate();

    /// <summary>
    /// This is the main type for your game
    /// </summary>
    public class Game1 : Microsoft.Xna.Framework.Game
    {
        #region Variables
        private GraphicsDeviceManager graphics;
        private SpriteBatch spriteBatch;

        private int afps = 0;
        private int fps = 0;
        private double et = 0;
        public static long Frames = 0;

        private Texture2D background;
        private Gui gameInterface;

        private string protocolVersion;
        private Logger logger;

        private FillCharacterListDelegate fillCharacterList;
        private DisplayWorldMessageDelegate displayMessage;
        private LogMessageDelegate logMessage;
        private StateChangedDelegate stateChanged;

        private WorldConnection connection;
        private Character playerCharacter;
        private Dictionary<int, Entity> worldEntities;

        private SpriteFont font;
        #endregion

        #region Constructors
        public Game1()
        {
            graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
        }
        #endregion

        #region Methods
        /// <summary>
        /// Allows the game to perform any initialization it needs to before starting to run.
        /// This is where it can query for any required services and load any non-graphic
        /// related content.  Calling base.Initialize will enumerate through any components
        /// and initialize them as well.
        /// </summary>
        protected override void Initialize()
        {
            Window.Title = "Asteria";
            IsMouseVisible = true;

            graphics.PreferredBackBufferWidth = 1024;
            graphics.PreferredBackBufferHeight = 768;
            graphics.IsFullScreen = false;

            graphics.ApplyChanges();

            gameInterface = new Gui(this);

            protocolVersion = "0.1";
            logger = new Logger("Asteria.log");
            Logger.MessageReceived += new LoggerMsgEvent(ToLog);

            connection = new WorldConnection("127.0.0.1", 5961, protocolVersion);
            connection.StateChanged += new WorldConnection.StateChangeHandler(HandleStateChanged);
            connection.WorldMessageReceived += new WorldClientMsgEvent(HandleMessageReceived);
            connection.CharManagementMessageReceived += new WorldClientMsgEvent(HandleCharMngtMessageReceived);
            connection.AccountId = 2;

            //connection.ConnectToWorld("admin_testing");

            worldEntities = new Dictionary<int, Entity>();

            base.Initialize();
        }

        /// <summary>
        /// LoadContent will be called once per game and is the place to load
        /// all of your content.
        /// </summary>
        protected override void LoadContent()
        {
            // Create a new SpriteBatch, which can be used to draw textures.
            spriteBatch = new SpriteBatch(GraphicsDevice);

            background = Content.Load<Texture2D>("Background2");
        }

        /// <summary>
        /// UnloadContent will be called once per game and is the place to unload
        /// all content.
        /// </summary>
        protected override void UnloadContent()
        {
            // TODO: Unload any non ContentManager content here
        }

        /// <summary>
        /// Allows the game to run logic such as updating the world,
        /// checking for collisions, gathering input, and playing audio.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Update(GameTime gameTime)
        {
            // Allows the game to exit.
            if (Keyboard.GetState().IsKeyDown(Keys.Escape))
                this.Exit();

            gameInterface.Update(gameTime);

            base.Update(gameTime);
        }

        /// <summary>
        /// This is called when the game should draw itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.Black);

            gameInterface.manager.BeginDraw(gameTime);

            gameInterface.Draw(gameTime);

            spriteBatch.Begin();
            spriteBatch.Draw(background, new Rectangle(0, 0, graphics.PreferredBackBufferWidth, graphics.PreferredBackBufferHeight), Color.White);
            spriteBatch.End();

            gameInterface.manager.EndDraw();

            base.Draw(gameTime);
        }

        /// <summary>
        /// Adds the log message to the console queue.
        /// </summary>
        /// <param name="message"></param>
        private void ToLog(string message)
        {
            gameInterface.Console.AddMessage(0, "debug", message);
        }

        private void HandleStateChanged(WorldConnection.WorldConnectionState state)
        {
            if (connection.State == WorldConnection.WorldConnectionState.Disconnected)
            {
                if (connection.DisconnectMessage != null && connection.DisconnectMessage.Length > 0)
                    Logger.Output(this, connection.DisconnectMessage);

                //Close();
            }
            else if (connection.State == WorldConnection.WorldConnectionState.Connected)
            {
            }
            else if (connection.State == WorldConnection.WorldConnectionState.CharacterManagement)
            {
                //FillCharacterList();
            }
            else if (connection.State == WorldConnection.WorldConnectionState.InGame)
            {
                //StartGame();
            }
        }

        private void HandleCharMngtMessageReceived(MessageType messageType)
        {
            // These messages are retreived in WorldClient in order to set charlist/charid/char format string..
            // TODO: [LOW] find a better way to handle these MessageReceived events, possibly passing the message itself instead of just the type.
            if (messageType != MessageType.S2C_CharacterList && messageType != MessageType.S2C_StartSuccess)
            {
                ServerToClientMessage wm = connection.GetMessage(messageType);
                Logger.Output(this, "Character management message received: {0}", messageType);
                ServerToClientMessage.FreeSafe(wm);
            }
            else if (messageType == MessageType.S2C_CharacterList)
            {
                //FillCharacterList();
            }
        }

        private void HandleMessageReceived(MessageType messageType)
        {
            ServerToClientMessage wm = connection.GetMessage(messageType);

            if (messageType == MessageType.S2C_ZoneMessage)
            {
                if (wm != null)
                {
                    HandleZoneMessage(wm);
                    //DisplayWorldMessage(wm);
                }
            }
            else
            {
                Logger.Output(this, "Non-Zone message received: {0}", messageType);
            }
            ServerToClientMessage.FreeSafe(wm);
        }

        private void HandleZoneMessage(ServerToClientMessage wm)
        {
            Entity e;

            // Check action and act
            PlayerAction a = (PlayerAction)wm.Code;
            switch (a)
            {
                case PlayerAction.AddEntity:
                case PlayerAction.MoveEntity:
                case PlayerAction.RemoveEntity:

                case PlayerAction.AddZone:
                case PlayerAction.RemoveZone:

                case PlayerAction.Attack:
                case PlayerAction.Damage:
                case PlayerAction.Move:
                case PlayerAction.Pickup:
                case PlayerAction.Drop:
                case PlayerAction.Use:
                case PlayerAction.EquipmentChange:
                case PlayerAction.EquipmentSync:
                case PlayerAction.Teleport:
                case PlayerAction.PlayerDied:
                case PlayerAction.InventoryChange:
                case PlayerAction.InventorySync:

                case PlayerAction.InvalidAction:
                case PlayerAction.InvalidMove:
                case PlayerAction.InvalidTarget:
                    break;
            }
            Logger.Output(this, "HandleZoneMessage() type {0}", a);
        }
        #endregion
    }
}
