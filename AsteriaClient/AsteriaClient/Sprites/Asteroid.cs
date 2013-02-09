using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Graphics;
using AsteriaLibrary.Entities;
using Microsoft.Xna.Framework;

namespace AsteriaClient.Sprites
{
    public class SpriteAsteroid : Sprite
    {
        #region Fields
        protected bool selected;
        #endregion

        #region Properties
        public bool Selected
        {
            get { return selected; }
            set { selected = value; }
        }
        #endregion

        #region Constructors
        public SpriteAsteroid(Asteroid entity, Texture2D texture)
            : base(entity, texture)
        {
        }
        #endregion

        #region Methods
        public override void Update(GameTime gameTime, Context context)
        {
            base.Update(gameTime, context);
        }
        #endregion
    }
}
