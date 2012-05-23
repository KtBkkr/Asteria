using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AsteriaLibrary.Math;
using AsteriaLibrary.Entities;

namespace AsteriaLibrary.Zones
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

        private int zoneSize;
        private int zoneCountX;
        private int zoneCountY;

        private static ZoneManager singletone;
        #endregion

        #region Properties
        public int ZoneSize { get { return zoneSize; } }
        public int ZoneCountX { get { return zoneCountX; } }
        public int ZoneCountY { get { return zoneCountY; } }
        public static ZoneManager Singletone { get { return singletone; } }
        #endregion

        #region Constructors
        /// <summary>
        /// Creates a new empty ZoneManager instance.
        /// This is used by the client to create its local world.
        /// </summary>
        public ZoneManager()
        {
        }

        /// <summary>
        /// Creates a new ZoneManager instance with a set zoneSize and tiling.
        /// This is used by the server to create the world zones.
        /// </summary>
        /// <param name="zoneSize"></param>
        /// <param name="zoneCountX"></param>
        /// <param name="zoneCountY"></param>
        public ZoneManager(int zoneSize, int zoneCountX, int zoneCountY)
        {
            ZoneManager.singletone = this;

            this.zoneSize = zoneSize;
            this.zoneCountX = zoneCountX;
            this.zoneCountY = zoneCountY;

            // Create zones
            for (int y = 0; y < zoneCountY; y++)
            {
                for (int x = 0; x < zoneCountX; x++)
                {
                    int id = x + y * zoneCountX;
                    AddZone(id, id.ToString(), new Point(x * zoneSize, y * zoneSize), new Point(x * zoneSize + zoneSize, y * zoneSize + zoneSize));
                }
            }

            // Calculate neighbors
            for (int y = 0; y < zoneCountY; y++)
            {
                for (int x = 0; x < zoneCountX; x++)
                {
                    int id = x + y * zoneCountX;
                    Zone z = GetZone(id);

                    // Left
                    if (x > 0)
                        z.AddLinkedZone(GetZone(id - 1));

                    // Right
                    if (x < zoneCountX - 1)
                        z.AddLinkedZone(GetZone(id + 1));

                    // Row before neighbor
                    if (y > 0)
                    {
                        // Top
                        z.AddLinkedZone(GetZone(id - zoneCountX));

                        // Top Left
                        if (x > 0)
                            z.AddLinkedZone(GetZone(id - zoneCountX - 1));

                        // Top Right
                        if (x < zoneCountX - 1)
                            z.AddLinkedZone(GetZone(id - zoneCountX + 1));
                    }

                    // Row after neighbor
                    if (y < zoneCountY - 1)
                    {
                        z.AddLinkedZone(GetZone(id + zoneCountX));

                        // Bottom Left
                        if (x > 0)
                            z.AddLinkedZone(GetZone(id + zoneCountX - 1));

                        // Bottom Right
                        if (x < zoneCountX - 1)
                            z.AddLinkedZone(GetZone(id + zoneCountX + 1));
                    }
                }
            }
        }
        #endregion

        #region Methods
        public void AddZone(Zone zone)
        {
            lock (zoneMngrLock)
            {
                zones.Add(zone);
                zone_lookup.Add(zone.Id, zone);
            }
        }

        public void AddZone(int id, string name, Point min, Point max)
        {
            lock (zoneMngrLock)
            {
                Zone z = new Zone();
                z.Initialize(id, name, min, max);
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

        public Zone FindZoneContaining(ref Point position)
        {
            lock (zoneMngrLock)
            {
                foreach (Zone z in zones)
                {
                    if (z.IsInsideZone(ref position))
                        return z;
                }
                return null;
            }
        }

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
                if (GetEntity(entity.Id) != null)
                    return false;

                Point position = Point.Zero;
                entity.GetPosition(out position);

                Zone zone = FindZoneContaining(ref position);

                if (zone == null)
                    return false;

                zone.AddEntity(entity);
                entity.LastZone = entity.CurrentZone;
                entity.CurrentZone = zone.Id;

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
                        entity.LastZone = entity.CurrentZone;
                        entity.CurrentZone = newZone.Id;
                        entity_lookup[entity.Id] = newZone;
                    }
                }
                else
                {
                    newZone.AddEntity(entity);
                    entity.LastZone = entity.CurrentZone;
                    entity.CurrentZone = newZone.Id;
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
                Zone oldZone = GetZone(id);

                Zone newZone = new Zone();
                newZone.Initialize(id, oldZone.Name, oldZone.Min, oldZone.Max);

                foreach (Entity entity in oldZone.Entities)
                    entity_lookup.Remove(entity.Id);

                zones.Remove(oldZone);
                zone_lookup.Remove(id);

                AddZone(newZone);
            }
        }
        #endregion
    }
}
