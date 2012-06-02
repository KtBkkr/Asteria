using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AsteriaLibrary.Client;
using AsteriaLibrary.Entities;
using AsteriaLibrary.Messages;
using AsteriaLibrary.Shared;
using Lidgren.Network;
using Microsoft.Xna.Framework;
using AsteriaClient.Interface.Controls;

namespace AsteriaClient.Network
{
    public class GameNetwork
    {
        #region Fields
        private Context context;

        private WorldConnection connection;
        private Character playerCharacter;
        private Dictionary<int, Entity> worldEntities;
        #endregion

        #region Constructors
        public GameNetwork(Context context)
        {
            this.context = context;

            worldEntities = new Dictionary<int, Entity>();
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
                //StartGame();
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

            if (messageType == MessageType.S2C_ZoneMessage)
            {
                if (wm != null)
                {
                    HandleZoneMessage(wm);
                    //DisplayWorldMessage(wm);
                }
            }
            else
            {
                Logger.Output(this, "Non-Zone message received: {0}", messageType);
                if (messageType == MessageType.S2C_ChatMessage)
                {
                    HandleChatMessage(wm);
                }
            }
            ServerToClientMessage.FreeSafe(wm);
        }

        private void HandleZoneMessage(ServerToClientMessage wm)
        {
            Entity e;

            // Check action and act
            PlayerAction a = (PlayerAction)wm.Code;
            switch (a)
            {
                case PlayerAction.AddEntity:
                case PlayerAction.MoveEntity:
                case PlayerAction.RemoveEntity:

                case PlayerAction.AddZone:
                case PlayerAction.RemoveZone:

                case PlayerAction.Attack:
                case PlayerAction.Damage:
                case PlayerAction.Move:
                case PlayerAction.Pickup:
                case PlayerAction.Drop:
                case PlayerAction.Use:
                case PlayerAction.EquipmentChange:
                case PlayerAction.EquipmentSync:
                case PlayerAction.Teleport:
                case PlayerAction.PlayerDied:
                case PlayerAction.InventoryChange:
                case PlayerAction.InventorySync:

                case PlayerAction.InvalidAction:
                case PlayerAction.InvalidMove:
                case PlayerAction.InvalidTarget:
                    break;
            }
            Logger.Output(this, "HandleZoneMessage() type {0}", a);
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
                    context.Gui.charsList.Items.Add(String.Format("ID:{0} - LVL:{1} - CLASS:{2} - NAME:{3}",
                        achar.CharacterId, achar.GetAttributeValue("level"), achar.GetPropertyValue("class"), achar.Name));
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

        private void Start()
        {
            playerCharacter = new Character(connection.WorldClient.OwnCharacter);
            //context.Gui.InitCharacterInfo();
        }

        public void Logout()
        {
            if (connection.State == WorldConnection.WorldConnectionState.InGame)
            {
                ClientToServerMessage pm = new ClientToServerMessage();
                pm.MessageType = MessageType.C2S_CharacterLogoutRequest;
                pm.Action = (int)PlayerAction.None;
                pm.AccountId = connection.AccountId;
                pm.CharacterId = connection.CharacterId;
                pm.DeliveryMethod = NetDeliveryMethod.ReliableUnordered;
                connection.WorldClient.SendMessage(pm);
            }
        }

        public void Quit()
        {
            if (connection.State == WorldConnection.WorldConnectionState.InGame)
            {
                ClientToServerMessage pm = new ClientToServerMessage();
                pm.MessageType = MessageType.C2S_PlayerLogoutRequest;
                pm.Action = (int)PlayerAction.None;
                pm.AccountId = connection.AccountId;
                pm.CharacterId = connection.CharacterId;
                pm.DeliveryMethod = NetDeliveryMethod.ReliableUnordered;
                connection.WorldClient.SendMessage(pm);
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
