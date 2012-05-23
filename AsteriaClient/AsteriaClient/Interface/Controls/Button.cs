using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace AsteriaClient.Interface.Controls
{
    public class Glyph
    {
        #region Fields
        public Texture2D Image = null;
        public SizeMode SizeMode = SizeMode.Stretched;
        public Color Color = Color.White;
        public Point Offset = Point.Zero;
        public Rectangle SourceRect = Rectangle.Empty;
        #endregion

        #region Constructors
        public Glyph(Texture2D image)
        {
            Image = image;
        }

        public Glyph(Texture2D image, Rectangle sourceRect)
            : this(image)
        {
            SourceRect = sourceRect;
        }
        #endregion
    }
}
