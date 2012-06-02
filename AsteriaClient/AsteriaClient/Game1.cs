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
using AsteriaClient.Network;
using System.Threading;

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

        private Logger logger;

        private Texture2D background;

        private Context context;
        private GameInterface gameInterface;
        private GameNetwork gameNetwork;

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
            logger = new Logger("Asteria.log");
            Logger.MessageReceived += new LoggerMsgEvent(ToLog);

            context = new Context();
            context.Protocol = "0.1";
            context.Game = this;

            gameInterface = new GameInterface(context);
            context.Gui = gameInterface;

            gameNetwork = new GameNetwork(context);
            gameNetwork.ConnectToWorld("173.51.135.30", 5961, 1, "admin_testing");
            context.Network = gameNetwork;

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
        #endregion
    }
}
