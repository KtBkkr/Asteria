using System.Collections.Generic;
using System.Linq;
using System.Text;
using AsteriaClient.Interface.Controls;
using Microsoft.Xna.Framework;

namespace AsteriaClient.Interface
{
    /// <summary>
    /// Handles in game interface.
    /// </summary>
    class Gui
    {
        #region Variables
        public Manager manager;

        private GuiConsole console;
        private Window test;
        #endregion

        #region Properties
        public GuiConsole Console
        {
            get { return console; }
        }
        #endregion

        #region Constructors
        public Gui(Game1 game)
        {
            manager = new Manager(game);
            manager.SkinDirectory = @"Content\Skins\";
            manager.SetSkin("Default");
            manager.Initialize();

            InitInterface();
        }
        #endregion

        #region Methods
        private void InitInterface()
        {
            console = new GuiConsole(manager, false);

            test = new Window(manager);
            test.Init();
            manager.Add(test);
        }

        public void Update(GameTime gameTime)
        {
            manager.Update(gameTime);
        }

        public void Draw(GameTime gameTime)
        {
            //manager.Draw(gameTime);
        }
        #endregion
    }
}
