using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AsteriaLibrary.Entities;
using AsteriaLibrary.Zones;
using AsteriaLibrary.Shared;

namespace AsteriaWorldServer
{
    /// <summary>
    /// Provides zones in a 2D top down view on the world.
    /// </summary>
    public class ZoneManager
    {
        #region Fields
        private object zoneMngrLock = new object();
        private List<Zone> zones = new List<Zone>();
        private SortedList<int, Zone> entity_lookup = new SortedList<int, Zone>();
        private SortedList<int, Zone> zone_lookup = new SortedList<int, Zone>();

        private static ZoneManager singletone;
        private ServerContext context;
        #endregion

        #region Properties
        public static ZoneManager Singletone { get { return singletone; } }
        public List<Zone> Zones { get { return zones; } }
        #endregion

        #region Constructors

        /// <summary>
        /// Creates a new ZoneManager instance.
        /// This is used by the server to manage world zones.
        /// </summary>
        public ZoneManager(ServerContext context)
        {
            ZoneManager.singletone = this;
            this.context = context;
        }
        #endregion

        #region Methods
        public void Update()
        {
            List<Zone> removeList = new List<Zone>();
            foreach (Zone zone in zones)
            {
                zone.Update();
                if ((DateTime.Now - zone.LastActive).TotalMinutes > 5)
                    removeList.Add(zone);
            }

            foreach (Zone zone in removeList)
            {
                Logger.Output(this, "Zone ID: {0} ({1}) inactive for 5 minutes.. Saving and removing.", zone.Id, zone.Name);
                SaveZone(zone);
                RemoveZone(zone.Id);
            }
        }

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

        public void LoadZone(int zoneId)
        {
            Zone zone = context.Dal.LoadZone(zoneId);
            if (zone != null)
            {
                // Load any entities from the database then add it.
                context.Dal.LoadZoneEntities(zone);
                AddZone(zone);
            }
        }

        /// <summary>
        /// Saves a zone and its entities (non characters) to the database.
        /// </summary>
        /// <param name="zone"></param>
        public void SaveZone(Zone zone)
        {
            context.Dal.SaveZone(zone);

            foreach (Entity e in zone.Entities)
                context.Dal.SaveEntity(e);
        }

        public void SaveAllZones()
        {
            foreach (Zone zone in zones)
            {
                context.Dal.SaveZone(zone);

                foreach (Entity e in zone.Entities)
                    context.Dal.SaveEntity(e);

                Logger.Output(this, "Saving zone ID:{0} ({1}). W:{2} H:{3}. Entities: {4}.", zone.Id, zone.Name, zone.Width, zone.Height, zone.Entities.Count);
            }
        }

        public void RemoveZone(int id)
        {
            lock (zoneMngrLock)
            {
                if (zone_lookup.ContainsKey(id))
                {
                    Zone zone = GetZone(id);

                    foreach (Entity e in zone.AllEntities)
                        entity_lookup.Remove(e.Id);

                    zones.Remove(zone);
                    zone_lookup.Remove(id);
                }
            }
        }

        public bool ZoneExists(int id)
        {
            lock (zoneMngrLock)
                return zone_lookup.ContainsKey(id);
        }

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
        /// Removes all entities from a single zone.
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
