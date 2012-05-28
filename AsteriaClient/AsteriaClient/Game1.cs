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
using AsteriaClient.Interface;
using AsteriaClient.Interface.Controls;

namespace AsteriaClient
{
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
        private GameInterface gameInterface;

        private string protocolVersion;
        private Logger logger;

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

            graphics.PreferredBackBufferWidth = 1440;
            graphics.PreferredBackBufferHeight = 900;
            graphics.IsFullScreen = false;

            graphics.ApplyChanges();

            gameInterface = new GameInterface(this);
            gameInterface.CreateCharacter += new CreateCharacterHandler(CreateCharacter);
            gameInterface.DeleteCharacter += new DeleteCharacterHandler(DeleteCharacter);
            gameInterface.StartCharacter += new StartCharacterHandler(StartCharacter);

            protocolVersion = "0.1";
            logger = new Logger("Asteria.log");
            Logger.MessageReceived += new LoggerMsgEvent(ToLog);

            connection = new WorldConnection("127.0.0.1", 5961, protocolVersion);
            connection.StateChanged += new WorldConnection.StateChangeHandler(HandleStateChanged);
            connection.WorldMessageReceived += new WorldClientMsgEvent(HandleMessageReceived);
            connection.CharManagementMessageReceived += new WorldClientMsgEvent(HandleCharMngtMessageReceived);
            connection.AccountId = 2;

            connection.ConnectToWorld("admin_testing");

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
            gameInterface.Console.MessageBuffer.Add(new ConsoleMessage(message, 3));
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
                FillCharacterList();
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
                FillCharacterList();
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

        /// <summary>
        /// Fills the list box with player characters created on the connected world server.
        /// </summary>
        /// <returns></returns>
        private void FillCharacterList()
        {
            gameInterface.charsList.Items.Clear();

            foreach (Character achar in connection.CharacterList)
            {
                if (achar.AccountId > 0 && achar.CharacterId > 0)
                    gameInterface.charsList.Items.Add(String.Format("ID:{0} - LVL:{1} - CLASS:{2} - NAME:{3}",
                        achar.CharacterId, achar.GetAttributeValue("level"), achar.GetPropertyValue("class"), achar.Name));
                else
                    Logger.Output(this, "Received invalid character list data!");
            }
        }

        private void CreateCharacter(CharacterMgtEventArgs e)
        {
            if (connection.CharacterAdd("1|" + e.Name))
                return;

            Logger.Output(this, "Character could not be created!");
        }

        private void DeleteCharacter(CharacterMgtEventArgs e)
        {
            if (connection.CharacterDelete(e.Id))
                return;

            Logger.Output(this, "Character could not be deleted!");
        }

        private void StartCharacter(CharacterMgtEventArgs e)
        {
            if (connection.CharacterStart(e.Id))
                return;

            Logger.Output(this, "Character could not be started!");
        }
        #endregion
    }
}
