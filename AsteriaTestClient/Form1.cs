using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Windows.Forms;
using AsteriaLibrary.Client;
using AsteriaLibrary.Entities;
using AsteriaLibrary.Messages;
using AsteriaLibrary.Shared;
using Lidgren.Network;

namespace AsteriaClient
{
    public partial class Form1 : Form
    {
        #region Variables
        static string protocolVersion = "0.1";

        private Logger logger;
        private WorldConnection conn;
        private Character playerCharacter;
        private Dictionary<int, Entity> worldEntities;
        #endregion

        #region Constructors
        public Form1()
        {
            InitializeComponent();

            logger = new Logger("AsteriaClient.log");
            Logger.MessageReceived += new LoggerMsgEvent(ToLog);

            conn = new WorldConnection("127.0.0.1", 5961, protocolVersion);
            conn.AccountId = 2;

            conn.StateChanged += new WorldConnection.StateChangeHandler(HandleStateChanged);
            conn.WorldMessageReceived += new WorldClientMsgEvent(HandleMessageReceived);
            conn.CharManagementMessageReceived += new WorldClientMsgEvent(HandleCharMngtMessageReceived);

            fillCharacterList = new FillCharacterListDelegate(FillCharacterList);
            displayMessage = new DisplayWorldMessageDelegate(DisplayWorldMessage);
            logMessage = new LogMessageDelegate(ToLog);
            stateChange = new StateChangedDelegate(HandleStateChanged);

            conn.ConnectToWorld("admin_testing");

            worldEntities = new Dictionary<int, Entity>();
        }
        #endregion

        #region Events
        delegate void FillCharacterListDelegate();
        private FillCharacterListDelegate fillCharacterList;

        delegate void DisplayWorldMessageDelegate(ServerToClientMessage wm);
        private DisplayWorldMessageDelegate displayMessage;

        delegate void LogMessageDelegate(string msg);
        private LogMessageDelegate logMessage;

        private delegate void StateChangedDelegate(WorldConnection.WorldConnectionState state);
        private StateChangedDelegate stateChange;
        #endregion

        #region Helpers
        /// <summary>
        /// Updates on screen stats from the game.PlayerCharacter instance.
        /// </summary>
        private void UpdateCharacterStats()
        {
            if (this.InvokeRequired)
            {
                this.Invoke((MethodInvoker)UpdateCharacterStats);
            }
            else
            {
                lblName.Text = playerCharacter.Name;
                lblClass.Text = playerCharacter.GetPropertyValue("class");
                lblPosition.Text = playerCharacter.Position.ToOutput();
                lblRotation.Text = playerCharacter.Rotation.ToString();
                lblType.Text = playerCharacter.TypeId.ToString();

                lblHealth.Text = playerCharacter.GetAttributeValue("health").ToString();
                lblMana.Text = playerCharacter.GetAttributeValue("mana").ToString();
                lblGold.Text = playerCharacter.Gold.ToString();
                lblLevel.Text = playerCharacter.GetAttributeValue("level").ToString();
            }
        }

        /// <summary>
        /// Just dumps the messages into the list.
        /// </summary>
        /// <param name="wm"></param>
        private void DisplayWorldMessage(ServerToClientMessage wm)
        {
            if (this.InvokeRequired)
            {
                this.Invoke(displayMessage, wm);
            }
            else
            {
                try
                {
                    switch (wm.MessageType)
                    {
                        case MessageType.S2C_ZoneMessage:
                            PlayerAction action = (PlayerAction)wm.Code;
                            switch (action)
                            {
                                case PlayerAction.InvalidMove:
                                    Logger.Output(this, "InvalidMove Id: {0}", BitConverter.ToInt32(wm.Buffer, 0));
                                    break;

                                default:
                                    Logger.Output(this, "ZoneMessage action: {0}", action);
                                    break;
                            }
                            break;

                        default:
                            Logger.Output(this, "Unhandled message type: {0}, code: {1}, data: '{2}'", wm.MessageType, wm.Code, wm.Data);
                            break;
                    }

                    UpdateCharacterStats();
                }
                catch (Exception ex)
                {
                    Debug.WriteLine(String.Format("DisplayWorldMessag Exception: {0}", ex.Message));
                }
            }
        }

        /// <summary>
        /// Fills the list box with player characters created on the connected world server.
        /// </summary>
        /// <returns></returns>
        private void FillCharacterList()
        {
            if (this.InvokeRequired)
            {
                this.Invoke(fillCharacterList);
            }
            else
            {
                lstCharacters.Rows.Clear();

                foreach (Character achar in conn.CharacterList)
                {
                    if (achar.AccountId > 0 && achar.CharacterId > 0)
                        lstCharacters.Rows.Add(achar.CharacterId, achar.Name, achar.GetAttributeValue("level"), achar.GetPropertyValue("class"));
                }
            }
        }

        private int GetSelectedCharacterId()
        {
            int id = -1;
            if (lstCharacters.SelectedRows.Count > 0 && lstCharacters.SelectedRows[0].Cells.Count > 0)
                id = (int)lstCharacters.SelectedRows[0].Cells[0].Value;

            return id;
        }

        private void ToLog(string message)
        {
            if (this.InvokeRequired)
            {
                this.Invoke(logMessage, message);
            }
            else
            {
                logBox.Items.Add(message);
                logBox.SelectedIndex = logBox.Items.Count - 1;
            }
        }
        #endregion

        #region Game Events Handler
        private void HandleStateChanged(WorldConnection.WorldConnectionState state)
        {
            if (this.InvokeRequired)
            {
                this.Invoke(stateChange, state);
            }
            else
            {
                if (conn.State == WorldConnection.WorldConnectionState.Disconnected)
                {
                    groupCharMngt.Hide();
                    groupCharSelect.Hide();
                    groupData.Hide();
                    groupChat.Hide();

                    if (conn.DisconnectMessage != null && conn.DisconnectMessage.Length > 0)
                        ToLog(conn.DisconnectMessage);

                    //Close();
                }
                else if (conn.State == WorldConnection.WorldConnectionState.Connected)
                {
                    groupCharMngt.Show();
                    groupCharSelect.Show();
                    groupData.Hide();
                    groupChat.Hide();
                }
                else if (conn.State == WorldConnection.WorldConnectionState.CharacterManagement)
                {
                    FillCharacterList();
                    groupCharMngt.Show();
                    groupCharSelect.Show();
                    groupData.Hide();
                    groupChat.Hide();
                }
                else if (conn.State == WorldConnection.WorldConnectionState.InGame)
                {
                    groupCharMngt.Hide();
                    groupCharSelect.Hide();
                    groupData.Show();
                    groupChat.Show();

                    StartGame();
                }
            }
        }

        private void HandleCharMngtMessageReceived(MessageType messageType)
        {
            // These messages are retreived in WorldClient in order to set charlist/charid/char format string..
            // TODO: [LOW] find a better way to handle these MessageReceived events, possibly passing the entire message instead of just the type.
            if (messageType != MessageType.S2C_CharacterList && messageType != MessageType.S2C_StartSuccess)
            {
                ServerToClientMessage wm = conn.GetMessage(messageType);
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
            ServerToClientMessage wm = conn.GetMessage(messageType);

            if (messageType == MessageType.S2C_ZoneMessage)
            {
                if (wm != null)
                {
                    HandleZoneMessage(wm);
                    DisplayWorldMessage(wm);
                }
            }
            else
            {
                Logger.Output(this, "Non-Zone message received: {0}", messageType);
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

        private void StartGame()
        {
            playerCharacter = new Character(conn.WorldClient.OwnCharacter);
            UpdateCharacterStats();
        }
        #endregion

        private void btnCreateCharacter_Click(object sender, EventArgs e)
        {
            if (conn.CharacterAdd("1|" + txtName.Text))
                return;

            MessageBox.Show(this, "Character could not be created!", "Error");
        }

        private void btnDeleteCharacter_Click(object sender, EventArgs e)
        {
            int characterId = GetSelectedCharacterId();
            if (characterId > -1)
            {
                if (conn.CharacterDelete(characterId))
                    return;

                MessageBox.Show(this, "Character not deleted!", "Server Error");
            }
            else
                MessageBox.Show(this, "Character not selected!", "Invalid Input");
        }

        private void btnStart_Click(object sender, EventArgs e)
        {
            int characterId = GetSelectedCharacterId();
            if (characterId > -1)
            {
                if (conn.CharacterStart(characterId))
                    return;

                MessageBox.Show(this, "Character not started!", "Server Error");
            }
            else
                MessageBox.Show(this, "No character selected!", "Invalid Input");
        }

        private void lstCharacters_SelectionChanged(object sender, EventArgs e)
        {
            if (lstCharacters.SelectedRows.Count > 0)
            {
                btnStart.Enabled = true;
                btnDeleteCharacter.Enabled = true;
            }
            else
            {
                btnStart.Enabled = false;
                btnDeleteCharacter.Enabled = false;
            }
        }

        private void btnLogout_Click(object sender, EventArgs e)
        {
            if (conn.State == WorldConnection.WorldConnectionState.InGame)
            {
                ClientToServerMessage pm = new ClientToServerMessage();
                pm.MessageType = MessageType.C2S_CharacterLogoutRequest;
                pm.Action = (int)PlayerAction.None;
                pm.AccountId = conn.AccountId;
                pm.CharacterId = conn.CharacterId;
                pm.DeliveryMethod = NetDeliveryMethod.ReliableUnordered;
                conn.WorldClient.SendMessage(pm);
            }
        }

        private void btnQuit_Click(object sender, EventArgs e)
        {
            if (conn.State == WorldConnection.WorldConnectionState.InGame)
            {
                ClientToServerMessage pm = new ClientToServerMessage();
                pm.MessageType = MessageType.C2S_PlayerLogoutRequest;
                pm.Action = (int)PlayerAction.None;
                pm.AccountId = conn.AccountId;
                pm.CharacterId = conn.CharacterId;
                pm.DeliveryMethod = NetDeliveryMethod.ReliableUnordered;
                conn.WorldClient.SendMessage(pm);
            }
        }
    }
}
