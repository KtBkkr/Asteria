using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Graphics;
using AsteriaLibrary.Entities;
using Microsoft.Xna.Framework;

namespace AsteriaClient.Sprites
{
    public class SpriteUnit : Sprite
    {
        #region Fields
        protected bool selected;

        protected Texture2D rangeTexture;

        protected float dieFade;
        protected float dieFadeRate;
        protected bool dead;
        #endregion

        #region Properties
        public bool Selected
        {
            get { return selected; }
            set { selected = value; }
        }

        public bool IsDead
        {
            get { return dead; }
        }

        public bool IsDying
        {
            get { return (((Unit)entity).CurrentHealth <= 0); }
        }
        #endregion

        #region Constructors
        public SpriteUnit(Unit entity, Texture2D texture)
            : base(entity, texture)
        {
            this.rangeTexture = TextureManager.Singletone.Get("Range");

            this.dieFade = 0f;
            this.dieFadeRate = 0.005f;
            this.dead = false;
        }
        #endregion

        #region Methods
        public override void Update(GameTime gameTime, Context context)
        {
            base.Update(gameTime, context);
        }

        public void DrawRange(SpriteBatch spriteBatch)
        {
            // TODO: [LOW] get a primitives class working to draw lines and ranges.
            if (selected)
            {
                Rectangle rangeRect = new Rectangle(
                    entity.Position.X + (int)origin.X - ((Unit)entity).Range,
                    entity.Position.Y + (int)origin.Y - ((Unit)entity).Range,
                    ((Unit)entity).Range * 2, ((Unit)entity).Range * 2);

                spriteBatch.Draw(rangeTexture, rangeRect, null, Color.LightBlue, 0, Vector2.Zero, SpriteEffects.None, Config.GetFloat("buildingRangeLayer"));
            }
        }
        #endregion
    }
}
