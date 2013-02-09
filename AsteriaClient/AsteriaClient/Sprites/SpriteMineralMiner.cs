using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AsteriaLibrary.Entities;
using Microsoft.Xna.Framework.Graphics;
using AsteriaClient.Sprites.Shots;
using Microsoft.Xna.Framework;

namespace AsteriaClient.Sprites
{
    public class SpriteMineralMiner : SpriteUnit
    {
        #region Fields
        private Texture2D shotTexture;
        private List<BeamMining> shotList;
        #endregion

        #region Properties
        public List<BeamMining> Shots
        {
            get { return shotList; }
        }
        #endregion

        #region Constructors
        public SpriteMineralMiner(MineralMiner entity, Texture2D texture)
            : base(entity, texture)
        {
            this.shotList = new List<BeamMining>();
            this.shotTexture = TextureManager.Singletone.Get("Beam");
        }
        #endregion

        #region Methods
        public override void Update(GameTime gameTime, Context context)
        {
            base.Update(gameTime, context);

            List<BeamMining> removeList = new List<BeamMining>();
            foreach (BeamMining beam in shotList)
            {
                beam.Update(gameTime, context);

                if (beam.IsDead)
                    removeList.Add(beam);
            }

            foreach (BeamMining beam in removeList)
                shotList.Remove(beam);
        }

        public override void Draw(SpriteBatch spriteBatch, Rectangle visibleArea)
        {
            foreach (BeamMining beam in shotList)
                beam.Draw(spriteBatch, Config.GetFloat("buildingShotLayer"), visibleArea);

            base.Draw(spriteBatch, visibleArea);
        }

        public void FireShot(Vector2 target)
        {
            float rotation = BuildingHelper.DirectionToTarget(Center, target);
            int distance = (int)Vector2.Distance(Center, target);
            BeamMining beam = new BeamMining(shotTexture, Center, target, rotation, distance);
            shotList.Add(beam);
        }
        #endregion
    }
}
