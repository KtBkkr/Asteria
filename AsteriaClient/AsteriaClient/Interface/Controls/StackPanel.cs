using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

namespace AsteriaClient.Interface.Controls
{
    public class StackPanel : Container
    {
        #region Fields
        private Orientation orientation;
        #endregion

        #region Constructors
        public StackPanel(Manager manager, Orientation orientation)
            : base(manager)
        {
            this.orientation = orientation;
            this.Color = Color.Transparent;
        }
        #endregion

        #region Methods
        private void CalcLayout()
        {
            int top = Top;
            int left = Left;

            foreach (Control c in ClientArea.Controls)
            {
                Margins m = c.Margins;
                if (orientation == Orientation.Vertical)
                {
                    top += m.Top;
                    c.Top = top;
                    top += c.Height;
                    top += m.Bottom;
                    c.Left = left;
                }

                if (orientation == Orientation.Horizontal)
                {
                    left += m.Left;
                    c.Left = left;
                    left += c.Width;
                    left += m.Right;
                    c.Top = top;
                }
            }
        }

        protected override void DrawControl(Renderer renderer, Rectangle rect, GameTime gameTime)
        {
            base.DrawControl(renderer, rect, gameTime);
        }

        protected override void OnResize(ResizeEventArgs e)
        {
            CalcLayout();
            base.OnResize(e);
        }
        #endregion
    }
}
