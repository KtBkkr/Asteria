using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

namespace AsteriaClient.Interface.Controls
{
    public class Label : Control
    {
        #region Fields
        private Alignment alignment = Alignment.MiddleLeft;
        private bool ellipsis = true;
        #endregion

        #region Properties
        public virtual Alignment Alignment
        {
            get { return alignment; }
            set { alignment = value; }
        }

        public virtual bool Ellipsis
        {
            get { return ellipsis; }
            set { ellipsis = value; }
        }
        #endregion

        #region Constructors
        public Label(Manager manager)
            : base(manager)
        {
            CanFocus = false;
            Passive = true;
            Width = 64;
            Height = 16;
        }
        #endregion

        #region Methods
        public override void Init()
        {
            base.Init();
        }

        protected override void DrawControl(Renderer renderer, Rectangle rect, GameTime gameTime)
        {
            //base.DrawControl(renderer, rect, gameTime);

            SkinLayer s = new SkinLayer(Skin.Layers[0]);
            s.Text.Alignment = alignment;
            renderer.DrawString(this, s, Text, rect, true, 0, 0, ellipsis);
        }
        #endregion
    }
}
