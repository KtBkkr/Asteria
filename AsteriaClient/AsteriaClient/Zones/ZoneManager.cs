using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AsteriaLibrary.Entities;

namespace AsteriaClient.Zones
{
    /// <summary>
    /// Provides zones in a 2D top down view on the world.
    /// </summary>
    public class ZoneManager
    {
        #region Fields
        private Context context;
        private object zoneMngrLock = new object();
        private List<Zone> zones = new List<Zone>();
        private SortedList<int, Zone> entity_lookup = new SortedList<int, Zone>();
        private SortedList<int, Zone> zone_lookup = new SortedList<int, Zone>();
        #endregion

        #region Properties
        public List<Zone> Zones { get { return zones; } }
        #endregion

        #region Constructors

        /// <summary>
        /// Creates a new ZoneManager instance.
        /// This is used by the server to manage world zones.
        /// </summary>
        public ZoneManager(Context context)
        {
            this.context = context;
        }
        #endregion

        #region Methods
        public void Update()
        {
            foreach (Zone zone in zones)
            {
                zone.Update();
            }
        }

        #region Zone Management
        public void AddZone(Zone zone)
        {
            lock (zoneMngrLock)
            {
                zones.Add(zone);
                zone_lookup.Add(zone.Id, zone);

                foreach (Entity e in zone.AllEntities)
                {
                    if (entity_lookup.ContainsKey(e.Id))
                        entity_lookup[e.Id] = zone;
                    else
                        entity_lookup.Add(e.Id, zone);
                }
            }
        }

        public void AddZone(int id, string name, int width, int height)
        {
            lock (zoneMngrLock)
            {
                Zone z = new Zone();
                z.Initialize(id, name, width, height);
                zones.Add(z);
                zone_lookup.Add(z.Id, z);
            }
        }

        public void RemoveZone(int id)
        {
            lock (zoneMngrLock)
            {
                if (zone_lookup.ContainsKey(id))
                {
                    Zone zone = GetZone(id);

                    foreach (Entity ent in zone.Entities)
                        entity_lookup.Remove(ent.Id);

                    zones.Remove(zone);
                    zone_lookup.Remove(id);
                }
            }
        }

        /// <summary>
        /// Removes all zones from the zone manager.
        /// </summary>
        public void RemoveAllZones()
        {
            lock (zoneMngrLock)
            {
                zones.Clear();
                entity_lookup.Clear();
                zone_lookup.Clear();
            }
        }

        /// <summary>
        /// Checks if a zone exists in the zone manager.
        /// </summary>
        public bool ZoneExists(int id)
        {
            lock (zoneMngrLock)
                return zone_lookup.ContainsKey(id);
        }

        /// <summary>
        /// Returns a zone by ID if it exists in the manager
        /// </summary>
        public Zone GetZone(int id)
        {
            lock (zoneMngrLock)
            {
                if (zone_lookup.ContainsKey(id))
                    return zone_lookup[id];
                else
                    return null;
            }
        }
        #endregion

        #region Entity Management
        public Entity GetEntity(int id)
        {
            lock (zoneMngrLock)
            {
                if (entity_lookup.ContainsKey(id))
                    return entity_lookup[id].GetEntity(id);
                else
                    return null;
            }
        }

        public bool AddEntity(Entity entity)
        {
            lock (zoneMngrLock)
            {
                // Entity already exists.
                if (GetEntity(entity.Id) != null)
                    return false;

                // Zone doesn't exist.
                Zone zone = GetZone(entity.Zone);
                if (zone == null)
                    return false;

                zone.AddEntity(entity);
                entity.Zone = zone.Id;

                if (entity_lookup.ContainsKey(entity.Id))
                    entity_lookup[entity.Id] = zone;
                else
                    entity_lookup.Add(entity.Id, zone);

                return true;
            }
        }

        public void MoveEntity(Entity entity, Zone newZone)
        {
            lock (zoneMngrLock)
            {
                if (entity_lookup.ContainsKey(entity.Id))
                {
                    Zone z = entity_lookup[entity.Id];
                    if (z != newZone)
                    {
                        z.RemoveEntity(entity);
                        newZone.AddEntity(entity);
                        entity.Zone = newZone.Id;
                        entity_lookup[entity.Id] = newZone;
                    }
                }
                else
                {
                    newZone.AddEntity(entity);
                    entity.Zone = newZone.Id;
                    entity_lookup.Add(entity.Id, newZone);
                }
            }
        }

        public bool RemoveEntity(int id)
        {
            lock (zoneMngrLock)
            {
                if (entity_lookup.ContainsKey(id))
                {
                    bool result = entity_lookup[id].RemoveEntity(id);
                    result &= entity_lookup.Remove(id);

                    return result;
                }
                return false;
            }
        }

        /// <summary>
        /// Removes all entities in the zone manager.
        /// NOTE: THIS REMOVES ALL ENTITIES IN EVERY ZONE
        /// </summary>
        public void RemoveAllEntities()
        {
            lock (zoneMngrLock)
            {
                entity_lookup.Clear();

                foreach (Zone z in zones)
                    z.Clear();
            }
        }

        /// <summary>
        /// Removes all entities in the zone manager except one.
        /// </summary>
        /// <param name="id"></param>
        public void RemoveAllEntities(int id)
        {
            lock (zoneMngrLock)
            {
                Entity save = GetEntity(id);

                entity_lookup.Clear();

                foreach (Zone z in zones)
                    z.Clear();

                if (save != null)
                    AddEntity(save);
            }
        }

        /// <summary>
        /// Removes all entities from a zone.
        /// </summary>
        /// <param name="id"></param>
        public void ClearZone(int id)
        {
            lock (zoneMngrLock)
            {
                Zone zone = GetZone(id);

                foreach (Entity entity in zone.Entities)
                    entity_lookup.Remove(entity.Id);

                zone.Clear();
            }
        }
        #endregion

        #endregion
    }
}
