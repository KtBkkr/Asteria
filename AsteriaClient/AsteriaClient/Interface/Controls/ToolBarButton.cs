using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

namespace AsteriaClient.Interface.Controls
{
    public class ToolBarButton : Button
    {
        #region Constructors
        public ToolBarButton(Manager manager)
            : base(manager)
        {
            CanFocus = false;
            Text = "";
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
            Skin = new SkinControl(Manager.Skin.Controls["ToolBarButton"]);
        }

        protected override void DrawControl(Renderer renderer, Rectangle rect, GameTime gameTime)
        {
            base.DrawControl(renderer, rect, gameTime);
        }
        #endregion
    }
}
