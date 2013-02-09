using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AsteriaLibrary.Messages;
using AsteriaLibrary.Math;
using AsteriaLibrary.Entities;

namespace AsteriaWorldServer.Zones
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
        private int width;
        private int height;

        private List<Entity> entities = new List<Entity>();
        private List<Character> characters = new List<Character>();
        private DateTime lastActive;
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
        /// Returns the zones user defined name (for debugging purposes only).
        /// </summary>
        public string Name
        {
            get { return name; }
            set { name = value; }
        }

        /// <summary>
        /// Returns the zones width.
        /// </summary>
        public int Width { get { return width; } }

        /// <summary>
        /// Returns the zones height.
        /// </summary>
        public int Height { get { return height; } }

        /// <summary>
        /// Returns a list of all entities (characters + entities) inside the zone.
        /// </summary>
        public List<Entity> AllEntities
        {
            get
            {
                lock (zoneLock)
                {
                    List<Entity> list = new List<Entity>();

                    foreach (Character character in characters)
                        list.Add((Entity)character);

                    foreach (Entity entity in entities)
                        list.Add(entity);

                    return list;
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
        /// Returns a list of all entities inside the zone.
        /// </summary>
        public List<Entity> Entities
        {
            get
            {
                lock (zoneLock)
                    return new List<Entity>(entities);
            }
        }

        /// <summary>
        /// Returns the DateTime that a character was last inside the zone.
        /// </summary>
        public DateTime LastActive
        {
            get { return lastActive; }
        }

        /// <summary>
        /// Returns true if the zone is active (contains at least one character entity).
        /// </summary>
        public bool IsActive
        {
            get { return characters.Count > 0; }
        }
        #endregion

        #region Constructors
        public Zone()
        {
        }
        #endregion

        #region Methods
        /// <summary>
        /// Initializes a new zone instance.
        /// </summary>
        public virtual void Initialize(int id, string name, int width, int height)
        {
            lock (zoneLock)
            {
                this.id = id;
                this.name = name;
                this.width = width;
                this.height = height;
                this.lastActive = DateTime.Now;
            }
        }

        public virtual void Update()
        {
            if (IsActive)
                lastActive = DateTime.Now;
        }

        /// <summary>
        /// Checks if the supplied position is within the zone width and height.
        /// </summary>
        public bool IsInsideZone(ref Point position)
        {
            return position.X >= 0 && position.X <= width
                && position.Y >= 0 && position.Y >= height;
        }

        /// <summary>
        /// Updates the zone data.
        /// </summary>
        public virtual void ChangeZone(string name, int width, int height)
        {
            lock (zoneLock)
            {
                if (this.width == width && this.height == height)
                    return;

                this.name = name;
                this.width = width;
                this.height = height;
            }
        }

        #region Entity Management
        /// <summary>
        /// Adds an entity to the zone.
        /// </summary>
        /// <param name="entity"></param>
        public virtual void AddEntity(Entity entity)
        {
            lock (zoneLock)
            {
                if (entity.GetType() == typeof(Character))
                    characters.Add((Character)entity);
                else
                    entities.Add(entity);
            }
        }

        /// <summary>
        /// Removes an entity from the zone.
        /// </summary>
        public virtual bool RemoveEntity(Entity entity)
        {
            lock (zoneLock)
            {
                bool found;

                if (entity.GetType() == typeof(Character))
                    found = characters.Remove((Character)entity);
                else
                    found = entities.Remove(entity);

                return found;
            }
        }

        /// <summary>
        /// Removes an entity from the zone.
        /// </summary>
        public virtual bool RemoveEntity(int id)
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
        public virtual void Clear()
        {
            lock (zoneLock)
            {
                characters.Clear();
                entities.Clear();
            }
        }

        /// <summary>
        /// Gets an entity from the zone.
        /// </summary>
        public virtual Entity GetEntity(int id)
        {
            lock (zoneLock)
            {
                foreach (Character character in characters)
                {
                    if (character.Id == id)
                        return (Entity)character;
                }

                foreach (Entity entity in entities)
                {
                    if (entity.Id == id)
                        return entity;
                }
                return null;
            }
        }
        #endregion

        #endregion
    }
}
