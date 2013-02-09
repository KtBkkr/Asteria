using System;
using System.Collections.Generic;
using System.Linq;
using AsteriaLibrary.Entities;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace AsteriaClient.Sprites
{
    public class Sprite
    {
        #region Variables
        protected Texture2D texture;
        protected Entity entity;

        protected Vector2 position;
        protected Vector2 origin;
        protected float rotation;

        static int viewAreaExpansion = 100;
        #endregion

        #region Properties
        public Texture2D Texture
        {
            get { return texture; }
        }

        public Vector2 Position
        {
            get { return position; }
        }

        public Vector2 Origin
        {
            get { return origin; }
        }

        public Vector2 Center
        {
            get { return (position - origin); }
        }

        public float Rotation
        {
            get { return rotation; }
            set { rotation = value; }
        }

        public Rectangle CollisionRect
        {
            get { return new Rectangle((int)position.X, (int)position.Y, texture.Width, texture.Height); }
        }
        #endregion

        #region Constructors
        /// <summary>
        /// Creates a new sprite instance linked to an entity.
        /// </summary>
        public Sprite(Entity entity, Texture2D texture)
        {
            this.texture = texture;
            this.entity = entity;

            this.position = new Vector2(entity.Position.X, entity.Position.Y);

            if (texture != null)
                this.origin = new Vector2(texture.Width / 2, texture.Height / 2);

            this.rotation = MathHelper.ToRadians(0);
        }

        /// <summary>
        /// Creates a new sprite instance not linked to an entity.
        /// </summary>
        public Sprite(Vector2 position, Texture2D texture)
        {
            this.texture = texture;
            this.entity = null;

            this.position = position;

            if (texture != null)
                this.origin = new Vector2(texture.Width / 2, texture.Height / 2);

            this.rotation = MathHelper.ToRadians(0);
        }
        #endregion

        #region Methods
        private Rectangle AdjustViewArea(Rectangle visibleArea)
        {
            Rectangle newArea = new Rectangle(
                visibleArea.X - viewAreaExpansion,
                visibleArea.Y - viewAreaExpansion,
                visibleArea.Width + (viewAreaExpansion * 2),
                visibleArea.Height + (viewAreaExpansion * 2));

            return newArea;
        }

        public virtual void Update(GameTime gameTime, Context context)
        {
            if (entity != null)
            {
                position.X = entity.Position.X;
                position.Y = entity.Position.Y;
            }
        }

        public virtual void Draw(SpriteBatch spriteBatch, Rectangle visibleArea)
        {
            visibleArea = AdjustViewArea(visibleArea);
            if (visibleArea.Intersects(this.CollisionRect))
                spriteBatch.Draw(texture, Center, null, Color.White, rotation, origin, 1.0f, SpriteEffects.None, 0);
        }
        public virtual void Draw(SpriteBatch spriteBatch, float layerDepth, Rectangle visibleArea)
        {
            visibleArea = AdjustViewArea(visibleArea);
            if (visibleArea.Intersects(this.CollisionRect))
                spriteBatch.Draw(texture, Center, null, Color.White, rotation, origin, 1.0f, SpriteEffects.None, layerDepth);
        }

        public virtual void Draw(SpriteBatch spriteBatch, Color color, Rectangle visibleArea)
        {
            visibleArea = AdjustViewArea(visibleArea);
            if (visibleArea.Intersects(this.CollisionRect))
                spriteBatch.Draw(texture, Center, null, color, rotation, origin, 1.0f, SpriteEffects.None, 0);
        }

        public virtual void Draw(SpriteBatch spriteBatch, Color color, float layerDepth, Rectangle visibleArea)
        {
            visibleArea = AdjustViewArea(visibleArea);
            if (visibleArea.Intersects(this.CollisionRect))
                spriteBatch.Draw(texture, Center, null, color, rotation, origin, 1.0f, SpriteEffects.None, layerDepth);
        }

        public virtual void Draw(SpriteBatch spriteBatch, Color color, float layerDepth)
        {
            spriteBatch.Draw(texture, Center, null, color, rotation, origin, 1.0f, SpriteEffects.None, layerDepth);
        }

        public virtual void Draw(SpriteBatch spriteBatch, Vector2 size, Color color, float layerDepth, Rectangle visibleArea)
        {
            Rectangle drawRectangle = new Rectangle((int)Center.X, (int)Center.Y, (int)size.X, (int)size.Y);

            visibleArea = AdjustViewArea(visibleArea);
            if (visibleArea.Intersects(this.CollisionRect))
                spriteBatch.Draw(texture, drawRectangle, null, color, rotation, origin, SpriteEffects.None, layerDepth);
        }
        #endregion
    }
}
