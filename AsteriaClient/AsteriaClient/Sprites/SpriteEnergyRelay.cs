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
    public class SpriteEnergyRelay : SpriteUnit
    {
        #region Fields
        private Texture2D relayTexture;
        private Dictionary<Entity, Relay> connections;
        #endregion

        #region Properties
        public Dictionary<Entity, Relay> Connectons
        {
            get { return connections; }
        }
        #endregion

        #region Constructors
        public SpriteEnergyRelay(EnergyRelay entity, Texture2D texture)
            : base(entity, texture)
        {
            this.relayTexture = TextureManager.Singletone.Get("Beam");
            this.connections = new Dictionary<Entity, Relay>();
        }
        #endregion

        #region Methods
        public override void Update(GameTime gameTime, Context context)
        {
            base.Update(gameTime, context);

            // Add new connection if entity exists.
            foreach (int entityId in ((EnergyRelay)entity).Connections)
            {
                Entity e = context.ZoneManager.GetEntity(entityId);
                if (e != null && !connections.ContainsKey(e))
                    AddConnection(e);
            }

            // Update or remove connections.
            List<Entity> remove = new List<Entity>();
            foreach (KeyValuePair<Entity, Relay> kvp in connections)
            {
                if (((EnergyRelay)entity).Connections.Contains(kvp.Key.Id) && context.ZoneManager.GetEntity(kvp.Key.Id) != null)
                    kvp.Value.Update(gameTime, context);
                else
                    remove.Add(kvp.Key);
            }

            foreach (Entity e in remove)
                connections.Remove(e);
        }

        public override void Draw(SpriteBatch spriteBatch, Rectangle visibleArea)
        {
            foreach (Relay relay in connections.Values)
                relay.Draw(spriteBatch, visibleArea);

            base.Draw(spriteBatch, visibleArea);
        }

        public void AddConnection(Entity target)
        {
            Vector2 targetPos = (target.Tag as Sprite).Center;
            float rotation = BuildingHelper.DirectionToTarget(Center, targetPos);
            int distance = (int)Vector2.Distance(Center, targetPos);
            Relay relay = new Relay(relayTexture, Center, rotation, distance);
            connections.Add(target, relay);
        }
        #endregion
    }
}
