using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

namespace AsteriaClient.Interface.Controls
{
    public class ClipBox : Control
    {
        #region Constructors
        public ClipBox(Manager manager)
            : base(manager)
        {
            Color = Color.Transparent;
            BackColor = Color.Transparent;
            CanFocus = false;
            Passive = true;
        }
        #endregion

        #region Methods
        protected override void DrawControl(Renderer renderer, Rectangle rect, GameTime gameTime)
        {
            base.DrawControl(renderer, rect, gameTime);
        }
        #endregion
    }
}
