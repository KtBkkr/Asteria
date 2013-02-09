using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;

namespace AsteriaClient.Sprites
{
    public class Relay : Sprite
    {
        #region Fields
        float energyFade = 1f;

        protected int length;
        protected int width;
        #endregion

        #region Constructors
        public Relay(Texture2D texture, Vector2 position, float rotation, int length)
            : base(position, texture)
        {
            this.rotation = rotation;
            this.length = length;
            this.width = 3;
        }
        #endregion

        #region Methods
        public void PassEnergy()
        {
            energyFade = 0f;
        }

        public override void Update(GameTime gameTime, Context context)
        {
            if (energyFade < 1f)
                energyFade += 0.05f;

            base.Update(gameTime, context);
        }

        public void Draw(SpriteBatch spriteBatch, Rectangle visibleArea)
        {
            Color fadecolor = new Color(1 - energyFade, 1 - energyFade, 1);
//#if DEBUG
//            fadecolor = new Color(1, 0.5f, 1 - energyFade);
//#endif

            Vector2 size = new Vector2(width, length);
            base.Draw(spriteBatch, size, fadecolor, Config.GetFloat("buildingRelayLayer"), visibleArea);
        }
        #endregion
    }
}
