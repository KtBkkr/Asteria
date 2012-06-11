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

namespace AsteriaClient.Network
{
    public class GameNetwork
    {
        #region Fields
        private Context context;

        private WorldConnection connection;
        private Character playerCharacter;
        private Dictionary<int, Entity> worldEntities;
        private ZoneManager zoneManager;
        #endregion

        #region Properties
        public Character PlayerCharacter
        {
            get { return playerCharacter; }
        }

        public ZoneManager ZoneManager
        {
            get { return zoneManager; }
        }

        public Dictionary<int, Entity> WorldEntities
        {
            get { return worldEntities; }
        }
        #endregion

        #region Constructors
        public GameNetwork(Context context)
        {
            this.context = context;

            worldEntities = new Dictionary<int, Entity>();
            zoneManager = new ZoneManager();
        }
        #endregion

        #region Methods
        public void Update(GameTime gameTime)
        {

        }

        public void ConnectToWorld(string host, int port, int account, string secret)
        {
            connection = new WorldConnection(host, port, context.Protocol);
            connection.StateChanged += new WorldConnection.StateChangeHandler(HandleStateChanged);
            connection.WorldMessageReceived += new WorldClientMsgEvent(HandleMessageReceived);
            connection.CharManagementMessageReceived += new WorldClientMsgEvent(HandleCharMngtMessageReceived);
            connection.AccountId = account;

            // TODO: [HIGH] use the account id and secret from the login server.
            connection.ConnectToWorld(secret);
        }

        private void HandleStateChanged(WorldConnection.WorldConnectionState state)
        {
            if (connection.State == WorldConnection.WorldConnectionState.Disconnected)
            {
                if (connection.DisconnectMessage != null && connection.DisconnectMessage.Length > 0)
                    Logger.Output(this, connection.DisconnectMessage);

                // TODO: reset the network here to allow for reconnecting and/or connect to other servers.
            }
            else if (connection.State == WorldConnection.WorldConnectionState.Connected)
            {
            }
            else if (connection.State == WorldConnection.WorldConnectionState.CharacterManagement)
            {
                FillCharacterList();
            }
            else if (connection.State == WorldConnection.WorldConnectionState.InGame)
            {
                playerCharacter = new Character(connection.WorldClient.OwnCharacter);
            }
        }

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
                    HandleZoneMessage(wm);
                    break;

                case MessageType.S2C_PlayerLoggedOut:
                    HandlePlayerLogout(wm);
                    break;

                case MessageType.S2C_CharacterLoggedOut:
                    HandleCharacterLogout(wm);
                    break;

                default:
                    Logger.Output(this, "Non-Zone message received: {0}", messageType);
                    if (messageType == MessageType.S2C_ChatMessage)
                        HandleChatMessage(wm);
                    break;
            }
            ServerToClientMessage.FreeSafe(wm);
        }

        private void HandleZoneMessage(ServerToClientMessage wm)
        {
            // Check action and act
            PlayerAction a = (PlayerAction)wm.Code;
            switch (a)
            {
                case PlayerAction.AddEntity:
                case PlayerAction.MoveEntity:
                case PlayerAction.RemoveEntity:

                case PlayerAction.AddZone:
                    HandleAddZone(wm);
                    break;

                case PlayerAction.RemoveZone:
                    HandleRemoveZone(wm);
                    break;

                case PlayerAction.Teleport:

                case PlayerAction.InvalidAction:
                    break;
            }
            Logger.Output(this, "HandleZoneMessage() type {0}", a);
        }

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
                    int width = Convert.ToInt32(zoneInfo[2]);
                    int height = Convert.ToInt32(zoneInfo[3]);
                    zoneManager.AddZone(id, zoneInfo[1], width, height);
                    Logger.Output(this, "HandleAddZone() Zone (Id:{0}, Name:{1}, Size:{2}x{3}", id, zoneInfo[1], width, height);
                }
                else if (key == "C")
                {
                }
                else if (key == "E")
                {
                    Entity entity = new Entity(value);
                    worldEntities.Add(entity.Id, entity);
                    Logger.Output(this, "HandleAddZone() Entity (Id:{0}, Type:{1}, Pos:{2}", entity.Id, entity.TypeId, entity.Position);
                }
            }
        }

        private void HandleRemoveZone(ServerToClientMessage wm)
        {
            string[] zones = wm.Data.Split(':');
            for (int i = 0; i < zones.Length; i++)
            {
                // Make sure the zone exists.
                Zone zone = zoneManager.GetZone(Convert.ToInt32(zones[i]));
                if (zone == null)
                    continue;

                // Compile entities then remove them.
                List<int> removeEntities = new List<int>();
                foreach (KeyValuePair<int, Entity> e in worldEntities)
                {
                    if (e.Value.Zone == zone.Id)
                        removeEntities.Add(e.Key);
                }

                foreach (int e in removeEntities)
                    worldEntities.Remove(e);

                // Finally remove the zone.
                zoneManager.RemoveZone(zone.Id);
            }
        }
        #endregion

        private void HandlePlayerLogout(ServerToClientMessage wm)
        {
        }

        private void HandleCharacterLogout(ServerToClientMessage wm)
        {
            worldEntities.Clear();
            zoneManager.RemoveAllZones();
        }

        private void HandleChatMessage(ServerToClientMessage wm)
        {
            int channel = wm.Code;
            string message = wm.Data;
            context.Gui.Console.MessageBuffer.Add(new ConsoleMessage(message, 2));
        }

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

        #region Character Management
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
                ClientToServerMessage pm = new ClientToServerMessage();
                pm.MessageType = MessageType.C2S_CharacterLogoutRequest;
                pm.Action = (int)PlayerAction.None;
                SendMessage(pm);
            }
        }

        public void Quit()
        {
            if (connection.State == WorldConnection.WorldConnectionState.InGame)
            {
                ClientToServerMessage pm = new ClientToServerMessage();
                pm.MessageType = MessageType.C2S_PlayerLogoutRequest;
                pm.Action = (int)PlayerAction.None;
                SendMessage(pm);
            }
        }

        public void SendMessage(ClientToServerMessage pm)
        {
            if (connection.WorldClient != null)
            {
                Logger.Output(this, "Sending standalone message: {0}", pm.MessageType);
                pm.DeliveryMethod = NetDeliveryMethod.ReliableUnordered;
                pm.AccountId = connection.AccountId;
                pm.CharacterId = connection.CharacterId;
                connection.WorldClient.SendMessage(pm);
            }
        }
        #endregion
    }
}
