using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

namespace AsteriaClient.Interface.Controls
{
    public class ToolBarPanel : Control
    {
        #region Constructors
        public ToolBarPanel(Manager manager)
            : base(manager)
        {
            Width = 64;
            Height = 25;
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
            Skin = new SkinControl(Manager.Skin.Controls["ToolBarPanel"]);
        }

        protected override void DrawControl(Renderer renderer, Rectangle rect, GameTime gameTime)
        {
            base.DrawControl(renderer, rect, gameTime);
        }

        protected override void OnResize(ResizeEventArgs e)
        {
            base.OnResize(e);
        }

        protected internal override void Update(GameTime gameTime)
        {
            base.Update(gameTime);
            AlignBars();
        }

        private void AlignBars()
        {
            int[] rx = new int[8];
            int h = 0;
            int rm = -1;

            foreach (Control c in Controls)
            {
                if (c is ToolBar)
                {
                    ToolBar t = c as ToolBar;
                    if (t.FullRow) t.Width = Width;
                    t.Left = rx[t.Row];
                    t.Top = (t.Row * t.Height) + (t.Row > 0 ? 1 : 0);
                    rx[t.Row] += t.Width + 1;

                    if (t.Row > rm)
                    {
                        rm = t.Row;
                        h = t.Top + t.Height + 1;
                    }
                }
            }
            Height = h;
        }
        #endregion
    }
}
