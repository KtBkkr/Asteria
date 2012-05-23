﻿using System;
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
using AsteriaLibrary.Zones;
using AsteriaWorldServer.Entities;
using AsteriaWorldServer.Messages;
using AsteriaWorldServer.PlayerCache;
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

        private Dictionary<int, CharacterState> charStates = new Dictionary<int, CharacterState>();
        private List<int> removeCharStates = new List<int>();

        private ZoneManager zoneMngr;
        private MoveManager moveMngr;

        private int pickupDistance;
        private int turnNumber;
        private static int lastEntityID;
        #endregion

        #region Constructors
        /// <summary>
        /// Creates a new GameProcessor instance.
        /// The IWorldServerExtension.GameProcessor implementation must return a concrete GameProcessor, thus it is best to create the
        /// concrete instance inside the IWorldServerExtension.Initialize implementation.
        /// </summary>
        /// <param name="context"></param>
        public GameProcessor(ServerContext context)
        {
            this.context = context;
            this.mpt = context.Mpt;
            this.serializer = new ServerToClientMessageSerializer();

            // Save our own instance of the ZoneManager, we could have passed that through the constructor as well.
            zoneMngr = context.ZoneManager;

            // Create MoveManager (handles valid world positions).
            moveMngr = new MoveManager(zoneMngr.ZoneSize * zoneMngr.ZoneCountX, zoneMngr.ZoneSize * zoneMngr.ZoneCountY);
            moveMngr.Initialize();

            string s = DataManager.Singletone.WorldParameters["PickupDistance"];
            pickupDistance = int.Parse(s);
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
        /// Invoked immediately before the players character is placed into the world.
        /// At this stage the character is fully loaded and the player has already received all the needed character data.
        /// </summary>
        /// <param name="character"></param>
        public void StartCharacter(Character character)
        {
            try
            {
                // Calculate the character zone.
                Zone zone = context.ZoneManager.GetZone(character.CurrentZone);

                Logger.Output(this, "StartCharacter: '{0}' ({1}), zone: {2} ({3})", character.Name, character.CharacterId, zone.Name, zone.Id);

                List<Zone> zones = zone.LinkedZones;
                zones.Add(zone);
#if DEBUG
                if (zones.Count == 0)
                    throw (new Exception("Sending empty zone message. The player always starts in one zone. This shouldn't be possible at all."));
#endif
                ServerToClientMessage wm = ServerToClientMessage.CreateMessageSafe(character.Sender);
                MessageFormatter.CreateZoneSyncMessage(zones, wm);
                QueueManager.WorldMessageQueueReadWrite = wm;

                // Add entity -> all zone players.
                ServerToClientMessage wm2 = ServerToClientMessage.CreateMessageSafe(character.Sender);
                MessageFormatter.CreateAddEntityToZoneMessage(character, wm2);
                AddMessageToLinkedZones(wm2, zone.Id);
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
        /// Handles entity movements, fires notifications and takes care of zone changes.
        /// If a WSE wants to change an entities position it must be done through this method.
        /// </summary>
        /// <param name="e"></param>
        /// <param name="newPosition"></param>
        public void MoveEntity(Entity e, ref Point newPosition)
        {
            if (e.Position == newPosition)
                return;

            // Catch zone changes and build notifications.
            Zone oldZone = context.ZoneManager.GetZone(e.CurrentZone);
            Zone newZone = context.ZoneManager.FindZoneContaining(ref newPosition);

            if (newZone == null)
            {
                Logger.Output(this, "MoveEntity() out of world map -> id: {0}, old position: {1}, new position: {2}", e.Id, e.Position, newPosition);
                return;
            }

            e.Position = newPosition;

            if (newZone.Id != e.CurrentZone)
            {
                // Remove entity from old zone and calculate added/removed zones.
                context.ZoneManager.MoveEntity(e, newZone);

                StringBuilder sb = new StringBuilder();
                sb.Append(newZone.Id);
                sb.Append(";");
                sb.Append(newZone.Name);
                sb.Append(";");
                sb.Append(newZone.Min.ToString());
                sb.Append(";");
                sb.Append(newZone.Max.ToString());

                e.SetProperty("zoneinfo", sb.ToString()); // TODO [HIGHEST]: this is WSE implementation specific and can't be in the server framework!

                IList<Zone> v_newzones = newZone.LinkedZones;
                v_newzones.Add(newZone);
                IList<Zone> v_oldzones = oldZone.LinkedZones;
                v_oldzones.Add(oldZone);

                var unchangedZones = v_newzones.Intersect(v_oldzones);
                var removedZones = v_oldzones.Except(unchangedZones);
                var addedZones = v_newzones.Except(unchangedZones);

                // Manually create the EntitySync messages so we notify
                // only the new zones instead of all linked zones.
                if (addedZones.Count() > 0)
                {
                    ServerToClientMessage wm = ServerToClientMessage.CreateMessageSafe();
                    MessageFormatter.CreateAddEntityToZoneMessage(e, wm);
                    wm.TurnNumber = turnNumber;

                    foreach (Zone z in addedZones)
                    {
                        if (z != null)
                            AddMessageToSingleZone(z, wm);
                    }
                    ServerToClientMessage.FreeSafe(wm);
                }

                // Send RemoveEntity messages.
                if (removedZones.Count() > 0)
                {
                    ServerToClientMessage wm = ServerToClientMessage.CreateMessageSafe();
                    MessageFormatter.CreateRemoveEntityFromZoneMessage(e, wm);
                    wm.TurnNumber = turnNumber;

                    foreach (Zone z in removedZones)
                    {
                        if (z != null)
                            AddMessageToSingleZone(z, wm);
                    }
                    ServerToClientMessage.FreeSafe(wm);
                }

                // Place entity in new zone.
                Logger.Output(this, "MoveEntity: '{0}' ({1}), zone: {2} ({3})", e.Name, e.Id, newZone.Name, newZone.Id);

                // Send zone syncs to all zones the character can now see or not.
                if (e.GetType() == typeof(Character))
                {
                    Character character = (Character)e;

                    if (addedZones.Count() > 0)
                    {
                        ServerToClientMessage wm = ServerToClientMessage.CreateMessageSafe(character.Sender);
                        MessageFormatter.CreateZoneSyncMessage(addedZones, wm);
                        QueueManager.WorldMessageQueueReadWrite = wm;
                    }

                    if (removedZones.Count() > 0)
                    {
                        ServerToClientMessage wm2 = ServerToClientMessage.CreateMessageSafe(character.Sender);
                        MessageFormatter.CreateZoneSyncMessage(removedZones, wm2);
                        QueueManager.WorldMessageQueueReadWrite = wm2;
                    }
                }
            }
            else
            {
                //oldZone.OnEntityMoved(e);
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

            // Dispatch the message to whole zone.
            ServerToClientMessage wm = ServerToClientMessage.CreateMessageSafe(c.Sender);
            MessageFormatter.CreateRemoveEntityFromZoneMessage(c, wm);
            AddMessageToLinkedZones(wm, c.CurrentZone);
            ServerToClientMessage.FreeSafe(wm);
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

        #region Abstracts
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
                    if (!charStates.ContainsKey(c.CharacterId))
                        charStates.Add(c.CharacterId, new CharacterState(c.CharacterId));

                    // Queues message for execution inside OnNewTurn.
                    // Note that a real WSE would add considerably more preprocessing here.
                    CharacterState cs = charStates[c.CharacterId];
                    switch (msg.Action)
                    {
                        case (int)PlayerAction.Move:
                            Point moveTarget = (Point)msg.GameData;
                            Zone zone = context.ZoneManager.FindZoneContaining(ref moveTarget);

                            if (zone != null && moveMngr.IsValidPosition(ref moveTarget))
                            {
                                c.SetAttribute("movementoption", int.Parse(msg.Data));
                                cs.SetMove(moveTarget, msg.Data);
                            }
                            else
                            {
                                ServerToClientMessage wm = ServerToClientMessage.CreateMessageSafe(c.Sender);
                                MessageFormatter.CreateInvalidMoveMessage(c, ref moveTarget, wm);
                                c.MessageBuffer.Add(wm);
                            }
                            break;

                        case (int)PlayerAction.Attack:
                            // TODO: [MID] implement
                            break;

                        case (int)PlayerAction.Pickup:
                            ProcessPickupMessage(msg, c);
                            break;

                        case (int)PlayerAction.Drop:
                            // TODO: [MID] implement
                            // 1. Add item to zone
                            // 2. Notify zone about new item
                            // 3. Notify player about inventory change
                            break;

                        case (int)PlayerAction.Use:
                            // TODO: [MID] implement
                            break;

                        case (int)PlayerAction.EquipmentChange:
                            // TODO: [MID] implement
                            // 1. Check if possible
                            // 2. Notify zone about equipment change
                            // 3. Notify player about equipment change and inventory change
                            break;

                        case (int)PlayerAction.Teleport:
                            // TODO: [MID] implement
                            break;

                        // TODO: [WSE DEV] add custom game specific actions.

                        default:
                            // Invalid action codes are not to be ignored, they represent either an outdated or malicious client!
                            Logger.Output(this, "ProcessMessage for character: {0}-'{1}' with invalid action code: {2}, ignoring message!", c.CharacterId, c.Name, msg.Action);
                            mpt.Disconnect(c.Sender, "Protocol error", 10000);
                            break;
                    }
                }
                else if (msg.MessageType == MessageType.C2S_PlayerLogoutRequest)
                {
                    // TODO: [WSE DEV] we must at least set the logout requested flag here.
                    // If applicable the character can be prepared for logout (setting some flags etc).
                    // Note that the TurnManager will at some later point invoke IsLogoutAllowed() to trigger the actual logout.
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
        /// Processes individual chat messages.
        /// </summary>
        /// <param name="sendingCharacter"></param>
        /// <param name="channel"></param>
        /// <param name="destination"></param>
        /// <param name="text"></param>
        public void ProcessChatMessage(Character sendingCharacter, int channel, int destination, string text)
        {
            // TODO: add application defined/private chat messages handling.
        }

        private void ProcessPickupMessage(ClientToServerMessage msg, Character c)
        {
            int entityId;
            if (int.TryParse(msg.GameData, out entityId))
            {
                Entity e = context.ZoneManager.GetEntity(entityId);
                bool isWrongTarget = false;
                string reason = "";

                if (e == null)
                {
                    isWrongTarget = true;
                    reason = "Item not found.";
                }
                else
                {
                    // Check if thats a pickable item at all.
                    if ((e.TypeId > 1000) && (e.TypeId < 3001)) // TODO: those values must be kept in sync with the Entities.xml, maybe using worldParams is better??
                    {
                        Point distance = c.Position - e.Position;
                        if (distance.Length() <= pickupDistance)
                        {
                            try
                            {
                                // Check if possible and add.
                                EntityClassData ecd;
                                InventoryBag bag;
                                int itemAmount = 1;
                                if (InventoryManager.AddItem(c, e.Id, itemAmount, out ecd, out bag))
                                {
                                    // Start saving character to DB.
                                    InvokeBackgroundCharacterSave(c);

                                    // Bag contains now the real characters item bag already updated with
                                    // the amount but we need to send only the changed amount so we need a copy.
                                    InventoryBag newBag = InventoryBag.FromInventoryBag(bag);
                                    newBag.Amount = itemAmount;

                                    // Check if special case for gold.
                                    if (ecd.SlotSize == Size.Zero)
                                    {
                                        if (e.Name == "Gold")
                                        {
                                            // TODO: [HIGH] implement sending gold change to client.
                                            c.Gold += e.Gold;
                                        }
                                        else
                                        {
                                            isWrongTarget = true;
                                            reason = "I can't pickup this item!"; // TODO: [LOW] implement message generator, the text is displayed in the client.
                                        }
                                    }
                                    else
                                    {
                                        // Send cient the inventory layout.
                                        ServerToClientMessage wm = ServerToClientMessage.CreateMessageSafe();
                                        // TODO: [LOW] the framework supports changing multiple items at once but we always send only one. Rethink if the whole inventory logic fits as it is now.
                                        MessageFormatter.CreateInventoryChangeMessage(c, InventoryChangeType.Pickup, new InventoryBag[] { newBag }, wm);
                                        c.MessageBuffer.Add(wm);
                                    }
                                }
                                else
                                {
                                    ServerToClientMessage wm = ServerToClientMessage.CreateMessageSafe();
                                    MessageFormatter.CreateInvalidActionMessage(PlayerAction.Pickup, "Inventory full!", wm);
                                    c.MessageBuffer.Add(wm);
                                }
                            }
                            catch (Exception ex)
                            {
                                Logger.Output(this, "ProcessMessage() msg: 'Pickup', item: {0}, character: {1}, exception: {2}, stacktrace {3}", entityId, c.CharacterId, ex.Message, ex.StackTrace);
                                ServerToClientMessage wm = ServerToClientMessage.CreateMessageSafe();
                                MessageFormatter.CreateInvalidActionMessage(PlayerAction.Pickup, "Unexpected error.", wm);
                                c.MessageBuffer.Add(wm);
                                isWrongTarget = true;
                                reason = "Inventory error!";
                            }
                        }
                        else
                        {
                            isWrongTarget = true;
                            reason = "Item is unreachable!";
                        }
                    }
                    else
                    {
                        isWrongTarget = true;
                        reason = "This can't be picked up!";
                    }
                }

                // Final notification
                if (isWrongTarget)
                {
                    // Notify client that this is not possible.
                    ServerToClientMessage wm = ServerToClientMessage.CreateMessageSafe(c.Sender);
                    MessageFormatter.CreateInvalidTargetMessage(entityId, reason, wm);
                    c.MessageBuffer.Add(wm);
                }
                else
                {
                    // Notify zone that item is gone.
                    using (ServerToClientMessage wm = ServerToClientMessage.CreateMessageSafe())
                    {
                        MessageFormatter.CreateRemoveEntityFromZoneMessage(e, wm);
                        AddMessageToLinkedZones(wm, e.CurrentZone);
                        ServerToClientMessage.FreeSafe(wm);
                    }
                    zoneMngr.RemoveEntity(entityId);
                }
            }
        }

        /// <summary>
        /// TODO: check multiple conditions to verify if logout is allowed like current game state, is the character attacked, etc.
        /// </summary>
        /// <param name="characterId"></param>
        /// <returns></returns>
        public bool IsLogoutAllowed(int characterId)
        {
            return true;
        }

        /// <summary>
        /// Invoked every server turn start. Add ame specific implementation which is time dependant here (movements, bullets, spell duration, buffs, recovery, etc).
        /// </summary>
        /// <param name="elapsedMilliseconds">Number of milliseconds passed since last turn.</param>
        private void OnNewTurn(float elapsedMilliseconds)
        {
            // We act on character states set during message processing.
            foreach (KeyValuePair<int, CharacterState> kvp in charStates)
            {
                CharacterState state = kvp.Value;
                Character c = null;

                // Try to get the character
                MasterPlayerRecord mpr = mpt.GetByCharacterId(kvp.Key);
                bool isCharacterOK = false;
                if (mpr != null)
                {
                    // TODO: [WSE DEV] if wse keeps its own collection of client related objects (like the
                    // charStates collection here) then such a collection must be kept in sync with the clients
                    c = mpr.pCharacter as Character;
                    if (c != null && !mpr.LogoutCharacterGranted && !mpr.LogoutCharacterRequested)
                        isCharacterOK = true;
                }

                // Mark player for removal and skip.
                if (!isCharacterOK)
                {
                    removeCharStates.Add(kvp.Key);
                    continue;
                }

                // Process all character actions.
                if (!c.IsDead)
                {
                    if (state.IsMoving)
                    {
                        // Calculate movement.
                        Point start = c.Position;
                        Point dest = state.MoveTarget;
                        Point newPosition;
                        int rotation = c.Rotation;

                        // TODO: [MID] here we have to take into account all buffing/spell/equip/mount speed factors.
                        //float velocity = (state.Movement == MoveType.Run ? runSpeed : walkSpeed);

                        // TODO: [MID] implement variable movement speeds (ie. destination will always remain the center of a tile, but position can be more accurate.
                        MoveResult mr = moveMngr.MoveTo(ref start, ref dest, elapsedMilliseconds, 1, ref rotation, out newPosition);
                        c.Rotation = rotation;

                        // Update position
                        if (mr != MoveResult.Blocked)
                            MoveEntity(c, ref newPosition);

                        // Stop moving if blocked or arrived
                        if (mr != MoveResult.Moved)
                        {
                            state.Stop();

                            if (mr == MoveResult.BlockedMove || mr == MoveResult.Blocked)
                            {
                                // notify the client that move is blocked, the client app might want to play a sound or similar.
                                ServerToClientMessage wm = ServerToClientMessage.CreateMessageSafe(c.Sender);
                                MessageFormatter.CreateInvalidMoveMessage(c, ref dest, wm);
                                c.MessageBuffer.Add(wm);
                            }
                        }

                        // We must notify the whole zone of this movement.
                        ServerToClientMessage wm2 = ServerToClientMessage.CreateMessageSafe(null);
                        MessageFormatter.CreateMoveEntityMessage(c, wm2);
                        AddMessageToLinkedZones(wm2, c.CurrentZone);
                        ServerToClientMessage.FreeSafe(wm2);
                    }

                    // TODO: [HIGH] process other actions
                }
            }

            // clean up states
            if (removeCharStates.Count > 0)
            {
                foreach (int i in removeCharStates)
                    charStates.Remove(i);

                removeCharStates.Clear();
            }
        }
        #endregion

        #region Helper Functions
        /// <summary>
        /// Stores the message into the buffer for all character entities of the given zone and it's linked zones.
        /// The message is added to a per client based buffer but not sent immediately. To send the buffered messages use DispatchZoneMessages.
        /// The ServerToClientMessage.TurnNumber is set to the current turn number regardless what the ServerToClientMessage.TurnNumber contains.
        /// Note that the passed in message can be immediately returned to the pool since AddZoneMessage creates separate copies of the message for each destination.
        /// </summary>
        /// <param name="wm"></param>
        /// <param name="zoneId"></param>
        protected void AddMessageToLinkedZones(ServerToClientMessage wm, int zoneId)
        {
            // Set the turnhere.
            wm.TurnNumber = turnNumber;

            // Get current zone and send.
            Zone zone = context.ZoneManager.GetZone(zoneId);
            AddMessageToSingleZone(zone, wm);

            // Get neighbors and send if active
            foreach (Zone z in zone.LinkedZones)
            {
                // IZone implementation can return null values.
                if (z != null && z.IsActive)
                    AddMessageToSingleZone(z, wm);
            }
        }

        /// <summary>
        /// Buffers the message to all character entities of a single zone.
        /// Note that the message parameter wm is not used directly, instead a copy is created.
        /// </summary>
        /// <param name="zone"></param>
        /// <param name="wm"></param>
        private void AddMessageToSingleZone(Zone zone, ServerToClientMessage wm)
        {
            foreach (Character c in zone.Characters)
            {
                // The copy is mandatory or we will end up overwritng messages
                // which ar still in use after the serializer invokes FreeSafe!
                c.MessageBuffer.Add(ServerToClientMessage.Copy(wm, c.Sender));
            }
        }

        /// <summary>
        /// Immediately sends the message to all character entities of a single zone.
        /// </summary>
        /// <param name="zone"></param>
        /// <param name="wm"></param>
        private void DispatchToZoneCharacters(Zone zone, ServerToClientMessage wm)
        {
            foreach (Character c in zone.Characters)
            {
                ServerToClientMessage m = ServerToClientMessage.Copy(wm, c.Sender);
#if DEBUG
                if(c.Sender == null)
                    throw (new Exception("Character has NULL sender, this will crash the MessageSender."));
#endif
                QueueManager.WorldMessageQueueReadWrite = m;
            }
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

        /// <summary>
        /// Invokes the IDalProvider.SaveCompleteCharacter on a background thread.
        /// </summary>
        /// <param name="character">The Character instance to be persisted to the DB.</param>
        protected void InvokeBackgroundCharacterSave(Character character)
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

        /// <summary>
        /// Builds a list containing character entities residing inside the given and linked zones.
        /// </summary>
        /// <param name="zoneId"></param>
        /// <returns></returns>
        private List<Character> BuildZoneCharacterList(int zoneId)
        {
            List<Character> allCharacters = new List<Character>();

            // Get current zone characters.
            Zone zone = context.ZoneManager.GetZone(zoneId);

            allCharacters.AddRange(zone.Characters);

            // Get neighbours and send if active.
            foreach (Zone z in zone.LinkedZones)
            {
                if (z != null && z.IsActive)
                    allCharacters.AddRange(z.Characters);
            }

            return allCharacters;
        }
        #endregion

        #endregion
    }
}