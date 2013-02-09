using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AsteriaClient.Interface.Controls;
using Microsoft.Xna.Framework;

namespace AsteriaClient.Interface
{
    public class PauseMenu : Panel
    {
        #region Variables
        Context context;

        Panel pnlPause2 = null;
        Dictionary<string, Button> btnPause = null;

        private string[] MenuOptions = new string[]
        {
            "Settings",
            "Keybinds",
            "Debug",
            "Leave Game"
        };
        #endregion

        #region Properties
        #endregion

        #region Constructors
        public PauseMenu(Context context, Manager manager)
            : base(manager)
        {
            this.context = context;

            Passive = true;
            Height = context.Game.Window.ClientBounds.Height;
            Width = context.Game.Window.ClientBounds.Width;
            MinimumHeight = context.Game.Window.ClientBounds.Height;
            MinimumWidth = context.Game.Window.ClientBounds.Width;
            Color = new Color(0f, 0f, 0f, 0.3f);
            Anchor = Anchors.All;
            Visible = false;
            CanFocus = true;
            StayOnTop = true;

            pnlPause2 = new Panel(manager);
            pnlPause2.Init();
            pnlPause2.Parent = this;
            pnlPause2.CanFocus = false;
            pnlPause2.Passive = true;
            pnlPause2.StayOnTop = true;

            btnPause = new Dictionary<string, Button>();

            for (int i = 0; i < MenuOptions.Length; i++)
            {
                int left = 8;
                int top = 8 + i * 25;
                int width = 152;

                btnPause.Add(MenuOptions[i], context.Gui.CreateButton(pnlPause2, left, top,
                    width, MenuOptions[i], new Controls.EventHandler(btnPauseMenu_Click)));

                pnlPause2.Width = width + 16;
                pnlPause2.Height = top + 32;
            }

            pnlPause2.Left = (context.Game.Window.ClientBounds.Width / 2) - (pnlPause2.Width / 2);
            pnlPause2.Top = (context.Game.Window.ClientBounds.Height / 2) - (pnlPause2.Height / 2);
        }
        #endregion

        #region Methods
        private void btnPauseMenu_Click(object sender, Controls.EventArgs e)
        {
            if (sender == btnPause["Settings"])
            {
                // TODO
            }
            else if (sender == btnPause["Keybinds"])
            {
                // TODO
            }
            else if (sender == btnPause["Debug"])
            {
                context.Gui.ShowDebugWindow();
                context.Gui.HidePauseMenu();
            }
            else if (sender == btnPause["Leave Game"])
            {
                context.Game.Exit();
            }
        }

        public override void Init()
        {
            base.Init();
        }

        protected internal override void Update(GameTime gameTime)
        {
            base.Update(gameTime);
        }
        #endregion
    }
}
