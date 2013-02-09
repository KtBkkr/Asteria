using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AsteriaLibrary.Entities;
using AsteriaLibrary.Shared;
using AsteriaWorldServer.Zones;
using AsteriaLibrary.Messages;
using System.Threading;

namespace AsteriaWorldServer.Zones
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
                ThreadPool.QueueUserWorkItem(new WaitCallback(SaveZone), zone);
                RemoveZone(zone.Id);
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

        public bool AddZone(string name, int width, int height)
        {
            lock (zoneMngrLock)
            {
                Zone z = context.Dal.CreateZone(name, width, height);
                if (z != null)
                {
                    zones.Add(z);
                    zone_lookup.Add(z.Id, z);
                    return true;
                }
                return false;
            }
        }

        public bool LoadZone(int zoneId)
        {
            Zone zone = context.Dal.LoadZone(zoneId);
            if (zone != null)
            {
                // Load any entities from the database then add it.
                context.Dal.LoadZoneEntities(zone);
                AddZone(zone);
                return true;
            }
            return false;
        }

        /// <summary>
        /// Saves a zone and its entities (non characters) to the database.
        /// </summary>
        public void SaveZone(object zoneObject)
        {
            Zone zone = zoneObject as Zone;
            context.Dal.SaveZone(zone);

            foreach (Entity e in zone.Entities)
            {
                if (e is EnergyStation)
                {
                    ((EnergyStation)e).PrepareData();
                    context.Dal.SaveEntity((EnergyStation)e);
                }
                else if (e is EnergyRelay)
                {
                    ((EnergyRelay)e).PrepareData();
                    context.Dal.SaveEntity((EnergyRelay)e);
                }
                else if (e is MineralMiner)
                {
                    ((MineralMiner)e).PrepareData();
                    context.Dal.SaveEntity((MineralMiner)e);
                }
                else if (e is BasicLaser)
                {
                    ((BasicLaser)e).PrepareData();
                    context.Dal.SaveEntity((BasicLaser)e);
                }
                else if (e is PulseLaser)
                {
                    ((PulseLaser)e).PrepareData();
                    context.Dal.SaveEntity((PulseLaser)e);
                }
                else if (e is TacticalLaser)
                {
                    ((TacticalLaser)e).PrepareData();
                    context.Dal.SaveEntity((TacticalLaser)e);
                }
                else if (e is MissileLauncher)
                {
                    ((MissileLauncher)e).PrepareData();
                    context.Dal.SaveEntity((MissileLauncher)e);
                }
                else if (e is Asteroid)
                {
                    ((Asteroid)e).PrepareData();
                    context.Dal.SaveEntity((Asteroid)e);
                }
                else if (e is Unit)
                {
                    ((Unit)e).PrepareData();
                    context.Dal.SaveEntity(((Unit)e));
                }
            }
        }

        /// <summary>
        /// Saves all zones into the database.
        /// </summary>
        public void SaveAllZones(object obj)
        {
            int zcount = 0;
            int ecount = 0;
            foreach (Zone zone in zones)
            {
                SaveZone(zone);
                Logger.Output(this, "Saving zone ID:{0} ({1}). W:{2} H:{3}. Entities: {4}.", zone.Id, zone.Name, zone.Width, zone.Height, zone.Entities.Count);
                zcount++;
            }
            Logger.Output(this, "Zone save complete. Zone count: {0}, Entity Count: {1}.", zcount, ecount);
        }

        /// <summary>
        /// Removes a zone from the zone manager but it remains in the database.
        /// </summary>
        public void RemoveZone(int zoneId)
        {
            lock (zoneMngrLock)
            {
                Zone zone = GetZone(zoneId);
                if (zone != null)
                {
                    foreach (Entity e in zone.AllEntities)
                        entity_lookup.Remove(e.Id);

                    zones.Remove(zone);
                    zone_lookup.Remove(zoneId);
                }
            }
        }

        /// <summary>
        /// Deletes a zone and its entities from the database.
        /// NOTE: THIS IS NOT RECOVERABLE. THEY WILL BE GONE FOR GOOD.
        /// </summary>
        public bool DeleteZone(int zoneId)
        {
            lock (zoneMngrLock)
            {
                Zone zone = GetZone(zoneId);
                if (zone != null)
                {
                    foreach (Entity e in zone.Entities)
                    {
                        context.Dal.DeleteEntity(e.Id);
                        entity_lookup.Remove(e.Id);
                    }

                    context.Dal.DeleteZone(zone.Id);
                    zones.Remove(zone);
                    zone_lookup.Remove(zoneId);
                    return true;
                }
                return false;
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

        #region Messages
        /// <summary>
        /// Buffers the message to all character entities of a single zone.
        /// Note that the message parameter wm is not used directly, instead a copy is created.
        /// </summary>
        public void AddMessageToZone(Zone zone, ServerToClientMessage wm)
        {
            if (!zone.IsActive)
                return;

            foreach (Character c in zone.Characters)
            {
                // The copy is mandatory or we will end up overwritng messages
                // which ar still in use after the serializer invokes FreeSafe!
                c.MessageBuffer.Add(ServerToClientMessage.Copy(wm, c.Sender));
            }
        }

        /// <summary>
        /// Buffers the message to all character entities of a single zone.
        /// Note that the message parameter wm is not used directly, instead a copy is created.
        /// </summary>
        public void AddMessageToZone(int zoneId, ServerToClientMessage wm)
        {
            Zone zone = GetZone(zoneId);
            if (zone != null && zone.IsActive)
                AddMessageToZone(zone, wm);
        }

        /// <summary>
        /// Buffers the message to all character entities in every zone.
        /// Note that the message parameter wm is not used directly, instead a copy is created.
        /// </summary>
        public void AddMessageToAllZones(ServerToClientMessage wm)
        {
            foreach (Zone zone in zones)
            {
                if (!zone.IsActive)
                    continue;

                foreach (Character c in zone.Characters)
                {
                    // The copy is mandatory or we will end up overwritng messages
                    // which ar still in use after the serializer invokes FreeSafe!
                    c.MessageBuffer.Add(ServerToClientMessage.Copy(wm, c.Sender));
                }
            }
        }
        #endregion

        #endregion
    }
}
