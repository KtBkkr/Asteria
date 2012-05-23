using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AsteriaLibrary.Messages;
using AsteriaLibrary.Math;
using AsteriaLibrary.Entities;

namespace AsteriaLibrary.Zones
{
    /// <summary>
    /// Main container for entities.
    /// </summary>
    public class Zone
    {
        #region Fields
        private object zoneLock = new object();
        private int id;
        private string name;
        private List<ServerToClientMessage> messages = new List<ServerToClientMessage>();

        private Point min;
        private Point max;

        internal List<Zone> linkedZones = new List<Zone>(8);

        private List<Entity> objects = new List<Entity>();
        private List<Character> characters = new List<Character>();
        #endregion

        #region Properties
        /// <summary>
        /// Returns the unique zone ID.
        /// </summary>
        public int Id
        {
            get { return id; }
            set { id = value; }
        }

        /// <summary>
        /// Returns true if the zone is active (contains at least one character entity).
        /// </summary>
        public bool IsActive
        {
            get { return characters.Count > 0; }
        }

        /// <summary>
        /// Returns the zones user defined name (for debugging purposes only).
        /// </summary>
        public string Name
        {
            get { return name; }
            set { name = value; }
        }

        /// <summary>
        /// Returns the zones min coordinate.
        /// </summary>
        public Point Min { get { return min; } }

        /// <summary>
        /// Returns the zones max coordinate.
        /// </summary>
        public Point Max { get { return max; } }

        /// <summary>
        /// Returns all neighbouring zones. Read only.
        /// </summary>
        public List<Zone> LinkedZones
        {
            get
            {
                lock (zoneLock)
                    return new List<Zone>(linkedZones);
            }
        }

        /// <summary>
        /// Returns all messages from the current zone.
        /// </summary>
        private List<ServerToClientMessage> Messages
        {
            get
            {
                lock (zoneLock)
                    return messages;
            }
        }

        /// <summary>
        /// Returns all messages from the current zone and all neighbouring zones.
        /// </summary>
        public List<ServerToClientMessage> AllMessages
        {
            get
            {
                List<ServerToClientMessage> tmpMessages = new List<ServerToClientMessage>();
                lock (zoneLock)
                {
                    tmpMessages.AddRange(Messages);

                    foreach (Zone neighbourZone in LinkedZones)
                    {
                        if (neighbourZone != null && neighbourZone.IsActive)
                            tmpMessages.AddRange(neighbourZone.Messages);
                    }
                    return tmpMessages;
                }
            }
        }

        /// <summary>
        /// Returns a list of all entities (characters + objects) inside the zone.
        /// </summary>
        public List<Entity> Entities
        {
            get
            {
                lock (zoneLock)
                {
                    List<Entity> entities = new List<Entity>();

                    foreach (Character character in characters)
                        entities.Add((Entity)character);

                    foreach (Entity entity in objects)
                        entities.Add(entity);

                    return entities;
                }
            }
        }

        /// <summary>
        /// Returns a list of characters inside the zone.
        /// </summary>
        public List<Character> Characters
        {
            get
            {
                lock (zoneLock)
                    return new List<Character>(characters);
            }
        }

        /// <summary>
        /// Returns a list of all objects inside the zone.
        /// </summary>
        public List<Entity> Objects
        {
            get
            {
                lock (zoneLock)
                    return new List<Entity>(objects);
            }
        }
        #endregion

        #region Methods
        /// <summary>
        /// Initializes a new zone instance.
        /// </summary>
        /// <param name="id">Unique zone ID.</param>
        /// <param name="name">Zone name for debugging.</param>
        /// <param name="size">Zone size.</param>
        public void Initialize(int id, string name, Point min, Point max)
        {
            lock (zoneLock)
            {
                this.id = id;
                this.name = name;
                this.min = min;
                this.max = max;
            }
        }

        public bool IsInsideZone(ref Point position)
        {
            return position >= min && position <= max;
        }

        #region Zone Implementation
        /// <summary>
        /// Adds a S2C message to the zone queue.
        /// </summary>
        /// <param name="msg"></param>
        public void AddMessage(ServerToClientMessage msg)
        {
            lock (zoneLock)
                messages.Add(msg);
        }

        /// <summary>
        /// Updates the zone data.
        /// </summary>
        /// <param name="name"></param>
        /// <param name="size"></param>
        public void UpdateZone(string name, Point min, Point max)
        {
            // TODO: possibly not needed?
            lock (zoneLock)
            {
                if (this.min == min && this.max == max)
                    return;

                this.name = name;
                this.min = min;
                this.max = max;
            }
        }

        /// <summary>
        /// Adds an entity to the zone.
        /// </summary>
        /// <param name="entity"></param>
        internal void AddEntity(Entity entity)
        {
            lock (zoneLock)
            {
                if (entity.GetType() == typeof(Character))
                    characters.Add((Character)entity);
                else
                    objects.Add(entity);
            }
        }

        /// <summary>
        /// Removes an entity from the zone.
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        internal bool RemoveEntity(Entity entity)
        {
            lock (zoneLock)
            {
                bool found;

                if (entity.GetType() == typeof(Character))
                    found = characters.Remove((Character)entity);
                else
                    found = objects.Remove(entity);

                return found;
            }
        }

        /// <summary>
        /// Removes an entity from the zone.
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        internal bool RemoveEntity(int id)
        {
            lock (zoneLock)
            {
                Entity found = GetEntity(id);
                if (found != null)
                    return RemoveEntity(found);

                return false;
            }
        }

        /// <summary>
        /// Clears the zone of all entities.
        /// </summary>
        internal void Clear()
        {
            lock (zoneLock)
            {
                characters.Clear();
                objects.Clear();
            }
        }

        /// <summary>
        /// Gets an entity from the zone.
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public Entity GetEntity(int id)
        {
            lock (zoneLock)
            {
                foreach(Character character in characters)
                {
                    if (character.Id == id)
                        return (Entity)character;
                }

                foreach (Entity entity in objects)
                {
                    if (entity.Id == id)
                        return entity;
                }
                return null;
            }
        }

        /// <summary>
        /// Adds a linked zone to the list.
        /// </summary>
        /// <param name="zone"></param>
        public void AddLinkedZone(Zone zone)
        {
            lock (zoneLock)
                linkedZones.Add(zone);
        }

        /// <summary>
        /// Removes a linked zone from the list.
        /// </summary>
        /// <param name="zone"></param>
        public void RemoveLinkedZone(Zone zone)
        {
            lock (zoneLock)
                linkedZones.Remove(zone);
        }
        #endregion

        #endregion
    }
}
