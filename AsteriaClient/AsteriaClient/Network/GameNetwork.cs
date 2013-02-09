using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AsteriaClient.Interface.Controls;
using AsteriaClient.Zones;
using AsteriaLibrary.Client;
using AsteriaLibrary.Entities;
using AsteriaLibrary.Messages;
using AsteriaLibrary.Shared;
using Lidgren.Network;
using Microsoft.Xna.Framework;
using AsteriaClient.Sprites;

namespace AsteriaClient.Network
{
    public class GameNetwork
    {
        #region Fields
        private Context context;
        private ZoneManager zoneManager;

        private WorldConnection connection;
        private WorldConnection.WorldConnectionState lastState;
        private Character playerCharacter;
        #endregion

        #region Properties
        /// <summary>
        /// Gets the current world connection state.
        /// </summary>
        public WorldConnection.WorldConnectionState State
        {
            get { return connection.State; }
        }

        /// <summary>
        /// Gets the current player character if in game.
        /// </summary>
        public Character PlayerCharacter
        {
            get { return playerCharacter; }
        }
        #endregion

        #region Constructors
        public GameNetwork(Context context)
        {
            this.context = context;
            this.zoneManager = context.ZoneManager;
            this.lastState = WorldConnection.WorldConnectionState.Disconnected;
            this.connection = new WorldConnection(context.Protocol);
        }
        #endregion

        #region Methods
        public void Update(GameTime gameTime)
        {
            if (connection.State == WorldConnection.WorldConnectionState.InGame)
                context.Gui.UpdateCharacterInfo();
        }

        public void ConnectToWorld(string host, int port, int account, string secret)
        {
            connection.StateChanged += new WorldConnection.StateChangeHandler(HandleStateChanged);
            connection.WorldMessageReceived += new WorldClientMsgEvent(HandleMessageReceived);
            connection.CharManagementMessageReceived += new WorldClientMsgEvent(HandleCharMngtMessageReceived);
            connection.AccountId = account;

            // TODO: [HIGH] use the account id and secret from the login server.
            connection.ConnectToWorld(host, port, secret);
        }

        private void HandleStateChanged(WorldConnection.WorldConnectionState state)
        {
            if (connection.State == WorldConnection.WorldConnectionState.Disconnected)
            {
                if (!string.IsNullOrWhiteSpace(connection.DisconnectMessage))
                    Logger.Output(this, connection.DisconnectMessage);

                HandleCleanup();
            }
            else if (connection.State == WorldConnection.WorldConnectionState.Connected)
            {
            }
            else if (connection.State == WorldConnection.WorldConnectionState.CharacterManagement)
            {
                FillCharacterList();
                context.Gui.ShowCharSelect();

                if (lastState == WorldConnection.WorldConnectionState.InGame)
                {
                    HandleCleanup();
                    context.Gui.ClearCharacterInfo();
                }
            }
            else if (connection.State == WorldConnection.WorldConnectionState.InGame)
            {
                context.Gui.HideCharSelect();
                playerCharacter = new Character(connection.WorldClient.OwnCharacter);
            }
            lastState = state;
        }

        /// <summary>
        /// Perform any cleanup needed before the next character is started.
        /// </summary>
        private void HandleCleanup()
        {
            zoneManager.RemoveAllZones();
            playerCharacter = null;
        }

        #region Message Handling
        private void HandleCharMngtMessageReceived(MessageType messageType)
        {
            // These messages are retreived in WorldClient in order to set charlist/charid/char format string..
            // TODO: [LOW] find a better way to handle these MessageReceived events, possibly passing the message itself instead of just the type.
            if (messageType != MessageType.S2C_CharacterList && messageType != MessageType.S2C_StartSuccess)
            {
                ServerToClientMessage wm = connection.GetMessage(messageType);
                Logger.Output(this, "Character management message received: {0}", messageType);
                ServerToClientMessage.FreeSafe(wm);
            }
            else if (messageType == MessageType.S2C_CharacterList)
            {
                FillCharacterList();
            }
        }

        private void HandleMessageReceived(MessageType messageType)
        {
            ServerToClientMessage wm = connection.GetMessage(messageType);
            if (wm == null)
                return;

            switch (messageType)
            {
                case MessageType.S2C_ZoneMessage:
                    ProcessZoneMessage(wm);
                    break;

                case MessageType.S2C_PlayerLoggedOut:
                case MessageType.S2C_CharacterLoggedOut:
                    break;

                default:
                    Logger.Output(this, "Non-Zone message received: {0}", messageType);
                    if (messageType == MessageType.S2C_ChatMessage)
                        ProcessChatMessage(wm);
                    break;
            }
            ServerToClientMessage.FreeSafe(wm);
        }

        private void ProcessZoneMessage(ServerToClientMessage wm)
        {
            // Check action and act
            PlayerAction a = (PlayerAction)wm.Code;
            switch (a)
            {
                case PlayerAction.AddEntity:
                    HandleAddEntity(wm);
                    break;

                case PlayerAction.MoveEntity:
                    HandleMoveEntity(wm);
                    break;

                case PlayerAction.RemoveEntity:
                    HandleRemoveEntity(wm);
                    break;

                case PlayerAction.Damage:
                    HandleDamage(wm);
                    break;

                case PlayerAction.AddZone:
                    HandleAddZone(wm);
                    break;

                case PlayerAction.RemoveZone:
                    HandleRemoveZone(wm);
                    break;

                case PlayerAction.Teleport:
                    HandleTeleport(wm);
                    break;

                case PlayerAction.InvalidAction:
                    break;
            }
            Logger.Output(this, "HandleZoneMessage() type {0}", a);
        }

        #region Entity Message Handling
        private void HandleAddEntity(ServerToClientMessage wm)
        {
            int entityId = BitConverter.ToInt32(wm.Buffer, 0);
            int typeId = BitConverter.ToInt32(wm.Buffer, 4);

            // Entity already exists. maybe we should delete it instead of return..
            if (zoneManager.GetEntity(entityId) != null)
                return;

            // Format and add entity based on type.
            AddEntityFromFormat(typeId, wm.Data);
        }

        private void HandleRemoveEntity(ServerToClientMessage wm)
        {
            int entityId = BitConverter.ToInt32(wm.Buffer, 0);
            if (zoneManager.GetEntity(entityId) != null)
            {
                zoneManager.RemoveEntity(entityId);
                Logger.Output(this, "HandleRemoveEntity() removing entity ID {0}.", entityId);
            }
            else
                Logger.Output(this, "HandleRemoveEntity() failed to remove entity ID {0}, doesn't exist.", entityId);
        }

        private void HandleMoveEntity(ServerToClientMessage wm)
        {
        }

        private void HandleDamage(ServerToClientMessage wm)
        {
            int entityId = BitConverter.ToInt32(wm.Buffer, 0);
            Entity entity = zoneManager.GetEntity(entityId);
            Entity from = null;
            if (entity != null)
            {
                string[] damageData = wm.Data.Split(':');
                int fromId = -1;
                int damageAmount = int.Parse(damageData[0]);
                if (damageData.Length > 1)
                {
                    fromId = int.Parse(damageData[1]);
                    from = zoneManager.GetEntity(fromId);
                    if (from != null)
                        context.WorldManager.FireShot(from, entity);
                }

                Logger.Output(this, "HandleDamage() Doing {0} damage to entity ID {1}", damageAmount, entityId);
            }
            else
                Logger.Output(this, "HandleDamage() failed to damage entity ID {0}, doesn't exist.", entityId);
        }

        private void AddEntityFromFormat(int typeId, string entityData)
        {
            if (typeId == (int)EntityType.Player)
            {
                Character character = new Character(entityData);
                zoneManager.AddEntity(character);
                Logger.Output(this, "AddEntity() added new character ID {0}.", character.Id);
            }
            else if (typeId == (int)EntityType.Unit)
            {
                Unit entity = new Unit(entityData);
                SpriteUnit sprite = new SpriteUnit(entity, TextureManager.Singletone.Get("Windmill"));
                entity.Tag = sprite;
                zoneManager.AddEntity(entity);
                Logger.Output(this, "AddEntity() added new unit ID {0}.", entity.Id);
            }
            else if (typeId == (int)EntityType.EnergyStation)
            {
                EnergyStation station = new EnergyStation(entityData);
                SpriteEnergyStation sprite = new SpriteEnergyStation(station, TextureManager.Singletone.Get("EnergyStation"));
                station.Tag = sprite;
                zoneManager.AddEntity(station);
                Logger.Output(this, "AddEntity() added new energy station ID {0}.", station.Id);
            }
            else if (typeId == (int)EntityType.EnergyRelay)
            {
                EnergyRelay relay = new EnergyRelay(entityData);
                SpriteEnergyRelay sprite = new SpriteEnergyRelay(relay, TextureManager.Singletone.Get("EnergyRelay"));
                relay.Tag = sprite;
                zoneManager.AddEntity(relay);
                Logger.Output(this, "AddEntity() added new energy relay ID {0}.", relay.Id);
            }
            else if (typeId == (int)EntityType.MineralMiner)
            {
                MineralMiner miner = new MineralMiner(entityData);
                SpriteMineralMiner sprite = new SpriteMineralMiner(miner, TextureManager.Singletone.Get("MineralMiner"));
                miner.Tag = sprite;
                zoneManager.AddEntity(miner);
                Logger.Output(this, "AddEntity() added new mineral miner ID {0}.", miner.Id);
            }
            else if (typeId == (int)EntityType.BasicLaser)
            {
                BasicLaser laser = new BasicLaser(entityData);
                zoneManager.AddEntity(laser);
                Logger.Output(this, "AddEntity() added new basic laser ID {0}.", laser.Id);
            }
            else if (typeId == (int)EntityType.PulseLaser)
            {
                PulseLaser laser = new PulseLaser(entityData);
                zoneManager.AddEntity(laser);
                Logger.Output(this, "AddEntity() added new pulse laser ID {0}.", laser.Id);
            }
            else if (typeId == (int)EntityType.TacticalLaser)
            {
                TacticalLaser laser = new TacticalLaser(entityData);
                zoneManager.AddEntity(laser);
                Logger.Output(this, "AddEntity() added new tactical laser ID {0}.", laser.Id);
            }
            else if (typeId == (int)EntityType.Asteroid)
            {
                Asteroid asteroid = new Asteroid(entityData);
                SpriteAsteroid sprite = new SpriteAsteroid(asteroid, TextureManager.Singletone.Get("Asteroid1"));
                asteroid.Tag = sprite;
                zoneManager.AddEntity(asteroid);
                Logger.Output(this, "AddEntity() added new asteroid ID {0}.", asteroid.Id);
            }
            else
            {
                Entity entity = new Entity(entityData);
                zoneManager.AddEntity(entity);
                Logger.Output(this, "AddEntity() added unknown entity type ID {0}.", entity.Id);
            }
        }
        #endregion

        #region Zone Message Handling
        private void HandleAddZone(ServerToClientMessage wm)
        {
            string[] data = wm.Data.Remove(0, 1).Split('#');
            for (int i = 0; i < data.Length; i++)
            {
                string key = data[i].Substring(0, 1);
                string value = data[i].Substring(1, data[i].Length - 1);
                if (key == "Z")
                {
                    // id:name:width:height
                    string[] zoneInfo = value.Split(':');
                    int id = Convert.ToInt32(zoneInfo[0]);

                    if (zoneManager.ZoneExists(id))
                    {
                        Logger.Output(this, "HandleAddZone() Removing existing zone {0}.", id);
                        zoneManager.RemoveZone(id);
                    }

                    int width = Convert.ToInt32(zoneInfo[2]);
                    int height = Convert.ToInt32(zoneInfo[3]);
                    zoneManager.AddZone(id, zoneInfo[1], width, height);

                    if (id == playerCharacter.Zone)
                    {
                        zoneManager.AddEntity(playerCharacter);
                        Logger.Output(this, "HandleAddZone() Moving player into zone.");
                    }
                    Logger.Output(this, "HandleAddZone() Zone (Id:{0}, Name:{1}, Size:{2}x{3}", id, zoneInfo[1], width, height);
                }
                else if (key == "C")
                {
                    Character character = new Character(value);
                    zoneManager.AddEntity(character);
                    Logger.Output(this, "HandleAddZone() Character (Id:{0}, Name:{1}, Pos:{2}", character.Id, character.Name, character.Position);
                }
                else if (key == "E")
                {
                    int typeId = int.Parse(value.Split('|')[0]);
                    string entityData = value.Split('|')[1];
                    AddEntityFromFormat(typeId, entityData);
                    Logger.Output(this, "HandleAddZone() Entity sent to parser.");
                }
            }
        }

        private void HandleRemoveZone(ServerToClientMessage wm)
        {
            string[] zones = wm.Data.Split(':');
            for (int i = 0; i < zones.Length; i++)
            {
                // Make sure it's an actual ID
                if (string.IsNullOrWhiteSpace(zones[i]))
                    continue;

                // Make sure the zone exists.
                Zone zone = zoneManager.GetZone(Convert.ToInt32(zones[i]));
                if (zone == null)
                    continue;

                // Finally remove the zone.
                zoneManager.RemoveZone(zone.Id);
            }
        }

        private void HandleTeleport(ServerToClientMessage wm)
        {
            string[] data = wm.Data.Remove(0, 1).Split('#');
            for (int i = 0; i < data.Length; i++)
            {
                string key = data[i].Substring(0, 1);
                string value = data[i].Substring(1, data[i].Length - 1);
                if (key == "Z")
                {
                    // id:name:width:height
                    string[] zoneInfo = value.Split(':');
                    int id = Convert.ToInt32(zoneInfo[0]);

                    if (zoneManager.ZoneExists(id))
                    {
                        Logger.Output(this, "HandleTeleport() Removing existing zone {0}.", id);
                        zoneManager.RemoveZone(id);
                    }

                    int width = Convert.ToInt32(zoneInfo[2]);
                    int height = Convert.ToInt32(zoneInfo[3]);
                    Zone zone = new Zone();
                    zone.Initialize(id, zoneInfo[1], width, height);
                    zoneManager.AddZone(zone);

                    int oldZone = playerCharacter.Zone;
                    zoneManager.MoveEntity(playerCharacter, zone);
                    zoneManager.RemoveZone(oldZone);
                    Logger.Output(this, "HandleTeleport() Zone (Id:{0}, Name:{1}, Size:{2}x{3}", id, zoneInfo[1], width, height);
                }
                else if (key == "C")
                {
                    Character character = new Character(value);
                    zoneManager.AddEntity(character);
                    Logger.Output(this, "HandleTeleport() Character (Id:{0}, Name:{1}, Pos:{2}", character.Id, character.Name, character.Position);
                }
                else if (key == "E")
                {
                    int typeId = int.Parse(value.Split('|')[0]);
                    string entityData = value.Split('|')[1];
                    AddEntityFromFormat(typeId, entityData);
                    Logger.Output(this, "HandleTeleport() Entity sent to parser.");
                }
            }
        }
        #endregion

        #region Chat Message Handling
        private void ProcessChatMessage(ServerToClientMessage wm)
        {
            int channel = wm.Code;
            string message = wm.Data;
            context.Gui.AddMessage(2, message);
        }
        #endregion

        #endregion

        #region Character Management
        /// <summary>
        /// Fills the list box with player characters created on the connected world server.
        /// </summary>
        /// <returns></returns>
        private void FillCharacterList()
        {
            context.Gui.charsList.Items.Clear();

            foreach (Character achar in connection.CharacterList)
            {
                if (achar.AccountId > 0 && achar.CharacterId > 0)
                    context.Gui.charsList.Items.Add(String.Format("ID:{0} - NAME:{1}", achar.CharacterId, achar.Name));
                else
                    Logger.Output(this, "Received invalid character list data!");
            }
        }

        public void CreateCharacter(string name)
        {
            if (connection.CharacterAdd("1|" + name))
                return;

            Logger.Output(this, "Character could not be created!");
        }

        public void DeleteCharacter(int id)
        {
            if (connection.CharacterDelete(id))
                return;

            Logger.Output(this, "Character could not be deleted!");
        }

        public void StartCharacter(int id)
        {
            if (connection.CharacterStart(id))
                return;

            Logger.Output(this, "Character could not be started!");
        }
        #endregion

        public void Logout()
        {
            if (connection.State == WorldConnection.WorldConnectionState.InGame)
            {
                ClientToServerMessage pm = CreateMessage();
                pm.MessageType = MessageType.C2S_CharacterLogoutRequest;
                pm.Action = (int)PlayerAction.None;
                SendMessage(pm);
            }
        }

        public void Quit()
        {
            if (connection.State == WorldConnection.WorldConnectionState.Connected ||
                connection.State == WorldConnection.WorldConnectionState.CharacterManagement ||
                connection.State == WorldConnection.WorldConnectionState.InGame)
            {
                ClientToServerMessage pm = CreateMessage();
                pm.MessageType = MessageType.C2S_PlayerLogoutRequest;
                pm.Action = (int)PlayerAction.None;
                SendMessage(pm);
            }
        }

        #region Messages
        /// <summary>
        /// Creates a safe ClientToServerMessage and inserts necessary data.
        /// </summary>
        public ClientToServerMessage CreateMessage()
        {
            ClientToServerMessage pm = ClientToServerMessage.CreateMessageSafe();
            pm.DeliveryMethod = NetDeliveryMethod.ReliableUnordered;
            pm.AccountId = connection.AccountId;
            pm.CharacterId = connection.CharacterId;
            return pm;
        }

        /// <summary>
        /// Sends a ClientToServerMessage to the server.
        /// Note: the message is expected to be formatted properly and inclue necessary data.
        /// </summary>
        public void SendMessage(ClientToServerMessage pm)
        {
            if (connection.State != WorldConnection.WorldConnectionState.Disconnected)
            {
                Logger.Output(this, "Sending standalone message: {0}", pm.MessageType);
                connection.WorldClient.SendMessage(pm);
            }
        }

        /// <summary>
        /// Formats and sends a chat message with the supplied data.
        /// </summary>
        /// <param name="type"></param>
        /// <param name="dest"></param>
        /// <param name="message"></param>
        public void SendChatMessage(ChatType type, int dest, string message)
        {
            if (connection.WorldClient != null && connection.State == WorldConnection.WorldConnectionState.InGame)
            {
                Logger.Output(this, "Sending chat message ({0}|{1}).", type, dest);
                ClientToServerMessage pm = CreateMessage();
                pm.MessageType = MessageType.C2S_PlayerChat;
                pm.Action = (int)PlayerAction.None;
                pm.Data = String.Format("{0}|{1}", (int)type, dest);
                pm.GameData = message;
                connection.WorldClient.SendMessage(pm);
            }
            else
                Logger.Output(this, "Tried to send chat message outside of game. Ignoring..");
        }

        public void SendActionMessage(PlayerAction action, string data, string gameData)
        {
            if (connection.WorldClient != null && connection.State == WorldConnection.WorldConnectionState.InGame)
            {
                Logger.Output(this, "Sending action message: {0} ({1}).", action.ToString(), gameData);
                ClientToServerMessage pm = CreateMessage();
                pm.MessageType = MessageType.C2S_PlayerAction;
                pm.Action = (int)action;
                pm.Data = data;
                pm.GameData = gameData;
                connection.WorldClient.SendMessage(pm);
            }
            else
                Logger.Output(this, "Tried to send action message outside of game. Ignoring..");
        }
        #endregion

        #endregion
    }
}
