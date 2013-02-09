using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace AsteriaClient.Sprites.Shots
{
    public class Beam : Sprite
    {
        #region Fields
        protected int age;
        protected int lifetime;

        protected int length;
        protected int width;

        protected float dieFade;
        protected float dieFadeRate;
        protected bool dead;

        protected Color color;
        protected Vector2 targetPosition;
        #endregion

        #region Properties
        public int Width
        {
            get { return width; }
        }

        public int Length
        {
            get { return length; }
        }

        public bool IsDead
        {
            get { return dead; }
        }
        #endregion

        #region Constructors
        public Beam(Texture2D texture, Vector2 pos, Vector2 target, float rotation, Color color, int width, int length)
            : base(pos, texture)
        {
            this.rotation = rotation;
            this.color = color;
            this.lifetime = 10;
            this.width = width;
            this.length = length;
            this.targetPosition = target;

            this.dieFade = 0f;
            this.dieFadeRate = 0.005f;
            this.dead = false;
        }
        #endregion

        #region Methods
        public override void Update(GameTime gameTime, Context context)
        {
            if (age >= lifetime)
                Killing();
            else
                age++;

            base.Update(gameTime, context);
        }

        public void Killing()
        {
            if (dieFade >= 1f)
                dead = true;

            dieFade += dieFadeRate;
        }

        public void UpdateTracking(Vector2 pos, Vector2 target, float rotation, int length)
        {
            this.position = pos;
            this.targetPosition = target;
            this.rotation = rotation;
            this.length = length;
        }

        public override void Draw(SpriteBatch spriteBatch, float layerDepth, Rectangle visibleArea)
        {
            Vector3 fade = color.ToVector3();
            Color fadeColor = new Color(fade.X, fade.Y, fade.Z, 1 - dieFade);

            Vector2 size = new Vector2(width, length);
            base.Draw(spriteBatch, size, fadeColor, layerDepth, visibleArea);
        }
        #endregion
    }
}
