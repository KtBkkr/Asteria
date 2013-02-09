using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using AsteriaLibrary.Data;
using AsteriaLibrary.Entities;
using AsteriaLibrary.Math;
using AsteriaLibrary.Messages;
using AsteriaLibrary.Serialization;
using AsteriaLibrary.Shared;
using AsteriaWorldServer.Messages;
using AsteriaWorldServer.PlayerCache;
using AsteriaWorldServer.Zones;
using Lidgren.Network;

namespace AsteriaWorldServer
{
    /// <summary>
    /// Abstract game rule processor. This class must be extended by every WSE.
    /// The concrete implementation is responsible for the game logic.
    /// </summary>
    public class GameProcessor
    {
        #region Fields
        private ServerContext context;
        private MasterPlayerTable mpt;
        private ServerToClientMessageSerializer serializer;

        private ZoneManager zoneManager;
        
        private int turnNumber;
        private static int lastEntityID;
        #endregion

        #region Constructors
        /// <summary>
        /// Creates a new GameProcessor instance.
        /// Used for processing in world messages and handling in world updating such as entities.
        /// </summary>
        public GameProcessor(ServerContext context)
        {
            this.context = context;
            this.mpt = context.Mpt;
            this.serializer = new ServerToClientMessageSerializer();

            // TODO: [HIGH] this could cause issues if the server crashes before the latest entity ID
            // could be saved.. possibly update this value in the database every time it's generated.
            GameProcessor.lastEntityID = Convert.ToInt32(context.ServerConfig["entity_id"]);

            // Save our own instance of the ZoneManager.
            zoneManager = context.ZoneManager;
        }
        #endregion

        #region Methods
        /// <summary>
        /// Generates a unique entity ID which stays unique across the whole WS lifetime.
        /// </summary>
        /// <returns></returns>
        public static int GenerateEntityID()
        {
            return Interlocked.Increment(ref lastEntityID);
        }

        /// <summary>
        /// Invoked by the WS before each new turn starts.
        /// </summary>
        /// <param name="currentTurnNumber"></param>
        /// <param name="milliseconds"></param>
        public void NewTurn(int currentTurnNumber, float milliseconds)
        {
            turnNumber = currentTurnNumber;
            OnNewTurn(milliseconds);
            DispatchZoneMessages();
        }

        /// <summary>
        /// Invoked every server turn start. Game specific implementation which is time dependant here.
        /// </summary>
        /// <param name="elapsedMilliseconds">Number of milliseconds passed since last turn.</param>
        private void OnNewTurn(float elapsedMilliseconds)
        {
            float elapsedSeconds = elapsedMilliseconds / 1000;
            zoneManager.Update();

            foreach (Zone z in zoneManager.Zones)
            {
                // TODO: [HIGH] we need a way to prioritize entities
                // ie. energy station first to calculate energy for this turn before other buildings subtract from it
                
                foreach (Entity entity in z.Entities)
                {
                    if (entity is EnergyStation)
                    {
                        EnergyStation station = entity as EnergyStation;
                        if (station.Timer >= station.Rate)
                        {
                            if (station.CurrentEnergy >= station.TotalEnergy)
                                continue;

                            if ((station.CurrentEnergy + station.Damage) > station.TotalEnergy)
                                station.CurrentEnergy = station.TotalEnergy;
                            else
                                station.CurrentEnergy += station.Damage;

                            station.Timer = 0;
                        }
                        else
                            station.Timer += elapsedSeconds;
                    }
                    else if (entity is EnergyRelay)
                    {
                        // TODO
                    }
                    else if (entity is MineralMiner)
                    {
                        MineralMiner miner = (entity as MineralMiner);
                        if (miner.Timer >= miner.Rate)
                        {
                            Asteroid asteroid = GetEntityInRange(miner, 10, miner.Range) as Asteroid;
                            if (asteroid != null)
                            {
                                // Asteroid is dry..
                                if (asteroid.CurrentMinerals <= 0)
                                    continue;

                                int minerals = asteroid.CurrentMinerals;
                                if (DamageEntity(asteroid, miner, miner.Damage))
                                {
                                    if (miner.Owner != -1)
                                    {
                                        // TODO: [HIGH] if the zone is loaded but the player is not online this won't work..
                                        // we need a way to give the player minerals no matter what

                                        // TODO: [HIGH] we also need a way to tell a player about a gold update.
                                        Character c = context.Mpt.GetCharacterByCharacterId(miner.Owner);
                                        if (c != null)
                                            c.Gold += minerals;
                                    }
                                }
                                miner.Timer = 0;
                            }
                        }
                        else
                            miner.Timer += elapsedSeconds;
                    }
                    else if (entity is BasicLaser)
                    {
                        // TODO
                    }
                    else if (entity is PulseLaser)
                    {
                        // TODO
                    }
                    else if (entity is TacticalLaser)
                    {
                        // TODO
                    }
                    else if (entity is MissileLauncher)
                    {
                        // TODO
                    }
                    else if (entity is Asteroid)
                    {
                        // TODO
                    }
                    else if (entity is Unit)
                    {
                        // TODO
                    }
                }
            }
        }

        /// <summary>
        /// Processes individual messages from connected clients.
        /// </summary>
        /// <param name="msg"></param>
        public void ProcessMessage(ClientToServerMessage msg)
        {
            // First get the character.
            MasterPlayerRecord mpr = mpt.GetByCharacterId(msg.CharacterId);
            Character c = mpr.pCharacter as Character;

            // If character exists store action.
            if (c != null)
            {
                if (msg.MessageType == MessageType.C2S_PlayerAction)
                {
                    try
                    {
                        // Queues message for execution inside OnNewTurn.
                        PlayerAction action = (PlayerAction)msg.Action;
                        if (action == PlayerAction.Teleport)
                        {
                            string[] teleportData = msg.Data.Split(':');
                            int zoneId = Convert.ToInt32(teleportData[0]);
                            int x = Convert.ToInt32(teleportData[1]);
                            int y = Convert.ToInt32(teleportData[2]);
                            Zone zone = zoneManager.GetZone(zoneId);
                            if (zone != null && zone.Id != c.Zone)
                            {
                                // Zone is found handle the teleport.
                                Teleport(c, zone, x, y);
                            }
                        }
                        else if (action == PlayerAction.AddBuilding)
                        {
                            string[] buildingData = msg.Data.Split(':');
                            int typeId = Convert.ToInt32(buildingData[0]);
                            int x = Convert.ToInt32(buildingData[1]);
                            int y = Convert.ToInt32(buildingData[2]);
                            if (AddEntity(typeId, c.Zone, x, y, c.CharacterId))
                            {
                            }
                            else
                            {
                            }
                        }
                        else if (action == PlayerAction.RemoveBuilding)
                        {
                            string[] removeList = msg.Data.Split(':');
                            foreach (string remove in removeList)
                            {
                                int id = Convert.ToInt32(remove);
                                Entity entity = zoneManager.GetEntity(id);
                                if (entity != null)
                                {
                                    if (entity.Owner == c.CharacterId)
                                    {
                                        if (RemoveEntity(id))
                                        {
                                            c.Gold += ((Unit)entity).Cost;
                                        }
                                        else
                                        {
                                        }
                                    }
                                }
                            }
                        }
                        else
                        {
                            // Invalid action codes are not to be ignored, they represent either an outdated or malicious client!
                            Logger.Output(this, "ProcessMessage for character: {0}-'{1}' with invalid action code: {2}, ignoring message!", c.CharacterId, c.Name, msg.Action);
                            mpt.Disconnect(c.Sender, "Protocol error", 10000);
                        }
                    }
                    catch (Exception ex)
                    {
                        Logger.Output(this, "Action message exception: {0}, {1}", ex.Message, ex.StackTrace);

                        ServerToClientMessage wm = ServerToClientMessage.CreateMessageSafe(c.Sender);
                        MessageFormatter.CreateInvalidActionMessage((PlayerAction)msg.Action, "An unknown action error has happened.", wm);
                        c.MessageBuffer.Add(wm);
                    }
                }
                else if (msg.MessageType == MessageType.C2S_PlayerLogoutRequest)
                {
                    mpr.LogoutCharacterRequested = true;
                    mpr.LogoutClientRequested = true;
                }
                else if (msg.MessageType == MessageType.C2S_CharacterLogoutRequest)
                {
                    mpr.LogoutCharacterRequested = true;
                    mpr.LogoutClientRequested = false;
                }
                else
                    Logger.Output(this, "ProcessMessage for character: {0}-'{1}' with invalid message type: {2}, ignoring message!", c.Id, c.Name, msg.MessageType);
            }
            else
                Logger.Output(this, "ProcessMessage for unknown character: {0}, type: {1}, action: {2}, sender: {3}", msg.CharacterId, msg.MessageType, msg.Action, msg.Sender);
        }

        /// <summary>
        /// Sends all buffered zone messages to clients.
        /// </summary>
        private void DispatchZoneMessages()
        {
            if (mpt.Count == 0)
                return;

            // Interate through all characters and serialize buffered messages.
            int counter = 0;
            LinkedNode<MasterPlayerRecord> node = mpt.First;
            while (node != null)
            {
                Character c = node.Value.pCharacter as Character;
                if (c != null)
                {
                    // Check for disconnects since the lidgren disconnect
                    // will be detected only after he ping timeouts.
                    if (c.Sender.Status == NetConnectionStatus.Connected)
                    {
                        // Only if we have something to send.
                        if (c.MessageBuffer.Count > 0)
                        {
                            // Pack all individual messages into a container
                            // message or send the single message as is.
                            ServerToClientMessage wm;
                            if (c.MessageBuffer.Count > 1)
                            {
                                wm = ServerToClientMessage.CreateMessageSafe(c.Sender);
#if DEBUG
                                if (c.Sender == null)
                                    throw (new Exception("Character has NULL sender, this will crash the MessageSender."));
#endif
                                wm.MessageType = MessageType.S2C_Container;
                                wm.Code = c.MessageBuffer.Count;
                                wm.Buffer = serializer.Serialize(c.MessageBuffer);
                                counter++;
                            }
                            else
                            {
                                wm = c.MessageBuffer[0];
                                wm.Sender = c.Sender;
                                counter++;
                            }

                            // Queue for sending and clear buffer
                            wm.TurnNumber = turnNumber;
                            QueueManager.WorldMessageQueueReadWrite = wm;
                            c.MessageBuffer.Clear();
                        }
                    }
                    else
                        node.Value.LogoutClientRequested = true;
                }
                node = node.Next;
            }
        }

        #region Login/Logout Management
        /// <summary>
        /// TODO: check multiple conditions to verify if logout is allowed like current game state, is the character attacked, etc.
        /// </summary>
        public bool IsLogoutAllowed(int characterId)
        {
            return true;
        }

        /// <summary>
        /// Invoked immediately before the players character is placed into the world.
        /// At this stage the character is fully loaded and the player has already received all the needed character data.
        /// </summary>
        /// <param name="character"></param>
        public void StartCharacter(Character character)
        {
            try
            {
                // Find the character zone
                Zone zone = context.ZoneManager.GetZone(character.Zone);
                List<Zone> zones = new List<Zone>();
                zones.Add(zone);

                Logger.Output(this, "StartCharacter: '{0}' ({1}), zone: {2} ({3})", character.Name, character.CharacterId, zone.Name, zone.Id);
#if DEBUG
                if (zone == null)
                    throw (new Exception("Sending empty zone message. The player always starts in one zone. This shouldn't be possible at all."));
#endif
                ServerToClientMessage wm = ServerToClientMessage.CreateMessageSafe(character.Sender);
                MessageFormatter.CreateZoneSyncMessage(zones, wm);
                QueueManager.WorldMessageQueueReadWrite = wm;

                // Add entity -> all zone players.
                ServerToClientMessage wm2 = ServerToClientMessage.CreateMessageSafe(character.Sender);
                MessageFormatter.CreateAddEntityToZoneMessage(character, wm2);
                wm2.TurnNumber = turnNumber;
                zoneManager.AddMessageToZone(zone, wm2);
                ServerToClientMessage.FreeSafe(wm2);

                // Finally add to zone.
                context.ZoneManager.AddEntity(character);
            }
            catch (Exception ex)
            {
                Logger.Output(this, "StartCharacter() exception, character id: {0}, message: {1}, stack strace: {2}", character.Id, ex.Message, ex.StackTrace);
            }
        }

        /// <summary>
        /// Removes the players character from the managed characters collection.
        /// This is not a request processing, the character has already been granted the logout and we just do the cleanup here.
        /// </summary>
        /// <param name="characterId"></param>
        public void LogoutCharacter(int characterId)
        {
            Logger.Output(this, "LogoutCharacter() invoked for characterId: {0}!", characterId);

            // Get character and invoke saving.
            Character c = mpt.GetCharacterByCharacterId(characterId);
            if (c != null)
            {
                Logger.Output(this, "LogoutCharacter() invoking background save..");
                InvokeBackgroundCharacterSave(c);
            }

            // Remove character from the zone.
            zoneManager.RemoveEntity(c.Id);

            // Dispatch the message to the zone.
            ServerToClientMessage wm = ServerToClientMessage.CreateMessageSafe(c.Sender);
            MessageFormatter.CreateRemoveEntityFromZoneMessage(c, wm);
            wm.TurnNumber = turnNumber;
            zoneManager.AddMessageToZone(c.Zone, wm);
            ServerToClientMessage.FreeSafe(wm);
        }

        /// <summary>
        /// Invokes the IDalProvider.SaveCompleteCharacter on a background thread.
        /// </summary>
        /// <param name="character">The Character instance to be persisted to the DB.</param>
        private void InvokeBackgroundCharacterSave(Character character)
        {
            ThreadPool.QueueUserWorkItem(new WaitCallback(BackgroundSave), character);
        }

        /// <summary>
        /// Saves a character. Executed on a Threadpool thread.
        /// </summary>
        /// <param name="characterInstance"></param>
        private void BackgroundSave(object characterInstance)
        {
            Character c = (Character)characterInstance;
            context.Dal.SaveCompleteCharacter(c);
        }
        #endregion

        #region Characters
        public void Teleport(Character c, Zone newZone, int x, int y)
        {
            Zone oldZone = zoneManager.GetZone(c.Zone);

            // Remove the character from the current zone (local).
            zoneManager.RemoveEntity(c.Id);

            // Remove the character from the current zone (remote).
            ServerToClientMessage wm = ServerToClientMessage.CreateMessageSafe();
            MessageFormatter.CreateRemoveEntityFromZoneMessage(c, wm);
            zoneManager.AddMessageToZone(oldZone.Id, wm);
            ServerToClientMessage.FreeSafe(wm);

            // Inform the player of the teleport and pass the new zone data.
            wm = ServerToClientMessage.CreateMessageSafe();
            MessageFormatter.CreateTeleportMessage(newZone, wm);
            c.MessageBuffer.Add(ServerToClientMessage.Copy(wm, c.Sender));
            ServerToClientMessage.FreeSafe(wm);

            // Inform the new zone of the new character (remote).
            wm = ServerToClientMessage.CreateMessageSafe();
            MessageFormatter.CreateAddEntityToZoneMessage(c, wm);
            zoneManager.AddMessageToZone(newZone.Id, wm);
            ServerToClientMessage.FreeSafe(wm);

            // Add character to the new zone (local).
            c.Zone = newZone.Id;
            zoneManager.AddEntity(c);

            // TODO: [LOW] update the characters position (remote).
            c.Position = new Point(x, y);
        }
        #endregion

        #region Entities
        /// <summary>
        /// Creates an entity in the database, adds it to the world and informs the zone.
        /// </summary>
        /// <param name="entityData"></param>
        public bool AddEntity(int type, int zone, int x, int y, int owner)
        {
            // Format the entity creation data.
            string entityData = String.Format("{0}:{1}:{2},{3}:{4}", type, zone, x, y, owner);

            // Attempt to create the new entity in the db.
            Entity entity = context.Dal.CreateEntity(entityData);
            if (entity != null)
            {
                // Initialize and add it to the zone manager.
                if (type == (int)EntityType.Unit)
                {
                    Unit unit = new Unit();
                    unit.Id = entity.Id;
                    context.Dal.LoadEntity(unit);
                    unit.LoadData();
                    zoneManager.AddEntity(unit);
                    Logger.Output(this, "AddEntity() created new Unit.");
                }
                else if (type == (int)EntityType.EnergyStation)
                {
                    EnergyStation station = new EnergyStation();
                    station.Id = entity.Id;
                    context.Dal.LoadEntity(station);
                    station.LoadData();

                    // We need to generate connections.
                    UpdateConnections(station);

                    zoneManager.AddEntity(station);
                    Logger.Output(this, "AddEntity() created new Energy Station.");
                }
                else if (type == (int)EntityType.EnergyRelay)
                {
                    EnergyRelay relay = new EnergyRelay();
                    relay.Id = entity.Id;
                    context.Dal.LoadEntity(relay);
                    relay.LoadData();

                    // We need to generate connections.
                    UpdateConnections(relay);

                    zoneManager.AddEntity(relay);
                    Logger.Output(this, "AddEntity() created new Energy Relay.");
                }
                else if (type == (int)EntityType.MineralMiner)
                {
                    MineralMiner miner = new MineralMiner();
                    miner.Id = entity.Id;
                    context.Dal.LoadEntity(miner);
                    miner.LoadData();

                    // TODO [LOW] should we give the miner an initial set of targets?

                    zoneManager.AddEntity(miner);
                    Logger.Output(this, "AddEntity() created new Mineral Miner.");
                }
                else if (type == (int)EntityType.BasicLaser)
                {
                    BasicLaser laser = new BasicLaser();
                    laser.Id = entity.Id;
                    context.Dal.LoadEntity(laser);
                    laser.LoadData();
                    zoneManager.AddEntity(laser);
                    Logger.Output(this, "AddEntity() created new Basic Laser.");
                }
                else if (type == (int)EntityType.PulseLaser)
                {
                    PulseLaser laser = new PulseLaser();
                    laser.Id = entity.Id;
                    context.Dal.LoadEntity(laser);
                    laser.LoadData();
                    zoneManager.AddEntity(laser);
                    Logger.Output(this, "AddEntity() created new Pulse Laser.");
                }
                else if (type == (int)EntityType.TacticalLaser)
                {
                    TacticalLaser laser = new TacticalLaser();
                    laser.Id = entity.Id;
                    context.Dal.LoadEntity(laser);
                    laser.LoadData();
                    zoneManager.AddEntity(laser);
                    Logger.Output(this, "AddEntity() created new Tactical Laser.");
                }
                else if (type == (int)EntityType.MissileLauncher)
                {
                    MissileLauncher launcher = new MissileLauncher();
                    launcher.Id = entity.Id;
                    context.Dal.LoadEntity(launcher);
                    launcher.LoadData();
                    zoneManager.AddEntity(launcher);
                    Logger.Output(this, "AddEntity() created new Missile Launcher.");
                }
                else if (type == (int)EntityType.Asteroid)
                {
                    Asteroid roid = new Asteroid();
                    roid.Id = entity.Id;
                    context.Dal.LoadEntity(roid);
                    roid.LoadData();
                    zoneManager.AddEntity(roid);
                    Logger.Output(this, "AddEntity() created new Asteroid.");
                }

                // Inform the zone of the new entity.
                ServerToClientMessage wm = ServerToClientMessage.CreateMessageSafe();
                MessageFormatter.CreateAddEntityToZoneMessage(zoneManager.GetEntity(entity.Id), wm);
                zoneManager.AddMessageToZone(zone, wm);
                ServerToClientMessage.FreeSafe(wm);

                return true;
            }
            else
                Logger.Output(this, "AddEntity() entity creation failed. ('{0}')", entityData);

            return false;
        }

        /// <summary>
        /// Removes a building owned by the character, deletes it from the db and informs the zone.
        /// </summary>
        public bool RemoveEntity(int id)
        {
            Entity entity = zoneManager.GetEntity(id);
            if (entity != null)
            {
                // Remove it from the zone manager.
                zoneManager.RemoveEntity(entity.Id);

                // Inform the zone of the removal
                ServerToClientMessage wm = ServerToClientMessage.CreateMessageSafe();
                MessageFormatter.CreateRemoveEntityFromZoneMessage(entity, wm);
                zoneManager.AddMessageToZone(entity.Zone, wm);
                ServerToClientMessage.FreeSafe(wm);

                // Finally delete it from the database.
                context.Dal.DeleteEntity(entity.Id);
                return true;
            }
            else
                Logger.Output(this, "RemoveEntity() entity ID: {0} doesn't exist.", id);

            return false;
        }

        /// <summary>
        /// Damages (or heals) an entity and informs the zone about it.
        /// </summary>
        public bool DamageEntity(Entity to, int amount)
        {
            return DamageEntity(to, null, amount);
        }

        /// <summary>
        /// Damages (or heals) an entity and informs the zone about it.
        /// </summary>
        public bool DamageEntity(Entity to, Entity from, int amount)
        {
            if (amount == 0)
                return false;

            // Only units have health. (or minerals =)
            if (!(to is Unit) && !(to is Asteroid))
                return false;

            if (to is Asteroid)
            {
                Asteroid asteroid = to as Asteroid;
                int damageAmount = amount;
                if (asteroid.CurrentMinerals <= amount)
                    damageAmount = asteroid.CurrentMinerals;

                if (damageAmount > 0)
                {
                    asteroid.CurrentMinerals -= damageAmount;
                    ServerToClientMessage wm = ServerToClientMessage.CreateMessageSafe();
                    MessageFormatter.CreateDamageEntityMessage(to, from, amount, wm);
                    zoneManager.AddMessageToZone(to.Zone, wm);
                    ServerToClientMessage.FreeSafe(wm);
                }
                return true;
            }
            else
            {
                Unit unit = to as Unit;
                if (unit.CurrentHealth <= amount)
                {
                    // TODO: [HIGH] handle building destruction.
                    context.ChatProcessor.SendMessage(new ChatMessage(ChatType.Zone, to.Zone, "Boom! a unit has been destroyed!"));
                    RemoveEntity(to.Id);
                    return false;
                }
                else
                {
                    unit.CurrentHealth -= amount;
                    ServerToClientMessage wm = ServerToClientMessage.CreateMessageSafe();
                    MessageFormatter.CreateDamageEntityMessage(to, from, amount, wm);
                    zoneManager.AddMessageToZone(to.Zone, wm);
                    ServerToClientMessage.FreeSafe(wm);
                    return true;
                }
            }
        }

        /// <summary>
        /// Returns the first entity within the specified range.
        /// </summary>
        private Entity GetEntityInRange(Entity from, int type, int range)
        {
            foreach (Entity e in zoneManager.GetZone(from.Zone).Entities)
            {
                if (e.TypeId != type)
                    continue;

                if (GetDistance(from.Position, e.Position) <= range)
                    return e;
            }
            return null;
        }

        /// <summary>
        /// Returns a list of entities in range of the provided entity.
        /// </summary>
        /// <param name="type">Only selects specific type ID, specify -1 for any.</param>
        /// <param name="max">Only selected a max amount, specify -1 for all.</param>
        private List<Entity> GetEntitiesInRange(Entity from, int type, int range, int max)
        {
            List<Entity> entities = new List<Entity>();
            foreach (Entity e in zoneManager.GetZone(from.Zone).Entities)
            {
                if (type != -1 && e.TypeId != type)
                    continue;

                if (GetDistance(from.Position, e.Position) <= range)
                    entities.Add(e);

                if (max != -1 && entities.Count >= max)
                    return entities;
            }
            return entities;
        }

        /// <summary>
        /// Checks if an entity is already connected to the powergrid.
        /// </summary>
        private bool AlreadyConnected(Entity entity)
        {
            // Not connectable..
            // This shouldn't be called in the first place.
            if (!(entity is Structure))
                return true;

            // If it's an energy station/relay it accepts more than one connection.
            if (entity is EnergyStation && (entity as EnergyStation).Connections.Count < (entity as EnergyStation).MaxConnections)
                return false;
            else if (entity is EnergyRelay && (entity as EnergyRelay).Connections.Count < (entity as EnergyRelay).MaxConnections)
                return false;

            // Anything else only accepts one connection.
            foreach (Entity e in zoneManager.GetZone(entity.Zone).Entities)
            {
                if (e is EnergyStation && (e as EnergyStation).Connections.Contains(entity.Id))
                    return true;
                else if (e is EnergyRelay && (e as EnergyRelay).Connections.Contains(entity.Id))
                    return true;
                else
                    continue;
            }
            return false;
        }

        private void UpdateConnections(Entity entity)
        {
            // Not owned or not an energy structure.
            if (entity.Owner == -1 || (!(entity is EnergyStation) && !(entity is EnergyRelay)))
                return;

            List<Entity> range = new List<Entity>();
            if (entity is EnergyStation)
            {
                EnergyStation station = entity as EnergyStation;
                range = GetEntitiesInRange(station, -1, station.Range, -1);
                foreach (Entity cand in range)
                {
                    // Already full on connections.
                    if (station.Connections.Count >= station.MaxConnections)
                        break;

                    // Not owned or not connectable.
                    if (!(cand is Structure) || cand.Owner == -1 || cand.Owner != station.Owner)
                        continue;

                    // Theres already a connection to this station/relay from the other end.
                    if (cand is EnergyStation && (cand as EnergyStation).Connections.Contains(station.Id))
                        continue;
                    else if (cand is EnergyRelay && (cand as EnergyRelay).Connections.Contains(station.Id))
                        continue;
                    else if (AlreadyConnected(cand))
                        continue;

                    // Connect it.
                    station.Connections.Add(cand.Id);
                }
            }
            else if (entity is EnergyRelay)
            {
                EnergyRelay relay = entity as EnergyRelay;
                range = GetEntitiesInRange(relay, -1, relay.Range, -1);
                foreach (Entity cand in range)
                {
                    // Already full on connections.
                    if (relay.Connections.Count >= relay.MaxConnections)
                        break;

                    // Not owned or not connectable.
                    if (!(cand is Structure) || cand.Owner == -1 || cand.Owner != relay.Owner)
                        continue;

                    // Theres already a connection to this station/relay from the other end.
                    if (cand is EnergyStation && (cand as EnergyStation).Connections.Contains(relay.Id))
                        continue;
                    else if (cand is EnergyRelay && (cand as EnergyRelay).Connections.Contains(relay.Id))
                        continue;
                    else if (AlreadyConnected(cand))
                        continue;

                    // Connect it.
                    relay.Connections.Add(cand.Id);
                }
            }
        }
        #endregion

        #region Helpers
        public int GetDistance(Point pos1, Point pos2)
        {
            double part1 = Math.Pow((pos2.X - pos1.X), 2);
            double part2 = Math.Pow((pos2.Y - pos1.Y), 2);
            double underRadical = part1 + part2;

            return (int)Math.Sqrt(underRadical);
        }
        #endregion

        #endregion
    }
}
