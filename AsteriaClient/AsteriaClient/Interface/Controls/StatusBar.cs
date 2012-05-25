using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

namespace AsteriaClient.Interface.Controls
{
    public class StatusBar : Control
    {
        #region Constructors
        public StatusBar(Manager manager)
            : base(manager)
        {
            Left = 0;
            Top = 0;
            Width = 64;
            Height = 24;
            CanFocus = false;
        }
        #endregion

        #region Methods
        public override void Init()
        {
            base.Init();
        }

        protected internal override void InitSkin()
        {
            base.InitSkin();
            Skin = new SkinControl(Manager.Skin.Controls["StatusBar"]);
        }

        protected override void DrawControl(Renderer renderer, Rectangle rect, GameTime gameTime)
        {
            base.DrawControl(renderer, rect, gameTime);
        }
        #endregion
    }
}
