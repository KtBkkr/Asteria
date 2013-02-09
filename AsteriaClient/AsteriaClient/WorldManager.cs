using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using AsteriaClient.Zones;
using AsteriaLibrary.Entities;
using AsteriaClient.Sprites;
using Microsoft.Xna.Framework.Graphics;

namespace AsteriaClient
{
    public class WorldManager
    {
        #region Fields
        private Context context;
        private ZoneManager zoneManager;
        #endregion

        #region Properties
        #endregion

        #region Constructors
        public WorldManager(Context context)
        {
            this.context = context;
            this.zoneManager = context.ZoneManager;
        }
        #endregion

        #region Methods
        public void Update(GameTime gameTime, Context context)
        {
            List<Character> characters = new List<Character>();

            foreach (Zone zone in zoneManager.Zones)
            {
                foreach (Entity entity in zone.AllEntities)
                {
                    if (entity is Character)
                    {
                        characters.Add((Character)entity);
                    }
                    else
                    {
                        if (entity is EnergyStation)
                        {
                            if (entity.Tag != null)
                                (entity.Tag as SpriteEnergyStation).Update(gameTime, context);
                        }
                        else if (entity is EnergyRelay)
                        {
                            if (entity.Tag != null)
                                (entity.Tag as SpriteEnergyRelay).Update(gameTime, context);
                        }
                        else if (entity is MineralMiner)
                        {
                            if (entity.Tag != null)
                                (entity.Tag as SpriteMineralMiner).Update(gameTime, context);
                        }
                        else if (entity is BasicLaser)
                        {
                        }
                        else if (entity is PulseLaser)
                        {
                        }
                        else if (entity is TacticalLaser)
                        {
                        }
                        else if (entity is Asteroid)
                        {
                            if (entity.Tag != null)
                                (entity.Tag as SpriteAsteroid).Update(gameTime, context);
                        }
                        else if (entity is Unit)
                        {
                            if (entity.Tag != null)
                                (entity.Tag as SpriteUnit).Update(gameTime, context);
                        }
                        else
                        {
                            if (entity.Tag != null)
                                (entity.Tag as Sprite).Update(gameTime, context);
                        }
                    }
                }
            }
        }

        public void Draw(SpriteBatch spriteBatch, Rectangle visibleArea)
        {
            foreach (Zone zone in zoneManager.Zones)
            {
                foreach (Entity entity in zone.AllEntities)
                {
                    if (entity is EnergyStation)
                    {
                        if (entity.Tag != null)
                            (entity.Tag as SpriteEnergyStation).Draw(spriteBatch, visibleArea);
                    }
                    else if (entity is EnergyRelay)
                    {
                        if (entity.Tag != null)
                            (entity.Tag as SpriteEnergyRelay).Draw(spriteBatch, visibleArea);
                    }
                    else if (entity is MineralMiner)
                    {
                        if (entity.Tag != null)
                            (entity.Tag as SpriteMineralMiner).Draw(spriteBatch, visibleArea);
                    }
                    else if (entity is BasicLaser)
                    {
                    }
                    else if (entity is PulseLaser)
                    {
                    }
                    else if (entity is TacticalLaser)
                    {
                    }
                    else if (entity is Asteroid)
                    {
                        if (entity.Tag != null)
                            (entity.Tag as SpriteAsteroid).Draw(spriteBatch, visibleArea);
                    }
                    else if (entity is Unit)
                    {
                        if (entity.Tag != null)
                            (entity.Tag as SpriteUnit).Draw(spriteBatch, visibleArea);
                    }
                    else
                    {
                        if (entity.Tag != null)
                            (entity.Tag as Sprite).Draw(spriteBatch, visibleArea);
                    }
                }
            }
        }

        public void FireShot(Entity from, Entity to)
        {
            if (from is MineralMiner)
            {
                if (from.Tag != null && to is Asteroid)
                {
                    Vector2 target = new Vector2(to.Position.X, to.Position.Y);
                    if (to.Tag != null)
                        target = (to.Tag as SpriteAsteroid).Center;

                    (from.Tag as SpriteMineralMiner).FireShot(target);
                }
            }
            else if (from is BasicLaser)
            {
            }
            else if (from is PulseLaser)
            {
            }
            else if (from is TacticalLaser)
            {
            }
        }
        #endregion
    }
}
