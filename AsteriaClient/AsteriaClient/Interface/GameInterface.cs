using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AsteriaClient.Interface.Controls;
using Microsoft.Xna.Framework;
using Controls = AsteriaClient.Interface.Controls;
using AsteriaLibrary.Shared;
using AsteriaLibrary.Messages;
using AsteriaLibrary.Entities;
using AsteriaClient.Zones;

namespace AsteriaClient.Interface
{
    /// <summary>
    /// Handles in game interface.
    /// </summary>
    public class GameInterface
    {
        #region Variables
        private Context context;
        public Manager manager;

        #region Controls
        private BottomBar botbar;
        private BottomBarPanel chatPanel;
        private BottomBarPanel infoPanel;
        private Controls.Console console;
        private Panel panel;

        private PauseMenu pauseMenu;

        private Panel charsPanel;
        public ListBox charsList;
        private TextBox createName;
        private Button createButton;
        private Button deleteButton;
        private Button startButton;

        private Label name;
        private Label zone;
        private Label entityListLabel;
        private ListBox entityList;
        private Label zoneListLabel;
        private ListBox zoneList;

        private Button logoutButton;
        private Button quitButton;
        private Button debugButton;

        private Window debugWindow;
        private Controls.Console debugConsole;
        #endregion

        #endregion

        #region Properties
        public Controls.Console Console
        {
            get { return console; }
        }
        #endregion

        #region Events
        #endregion

        #region Constructors
        public GameInterface(Context context)
        {
            this.context = context;

            manager = new Manager(context.Game);
            manager.SkinDirectory = @"Content\Skins\";
            manager.SetSkin("Default");
            manager.Initialize();
        }
        #endregion

        #region Methods
        public void Update(GameTime gameTime)
        {
            manager.Update(gameTime);
        }

        public void Draw(GameTime gameTime)
        {
            //manager.Draw(gameTime);
        }

        /// <summary>
        /// Initializes the gui and controls.
        /// </summary>
        public void InitInterface()
        {
            botbar = new BottomBar(manager);
            botbar.Init();
            botbar.Height = 200;
            botbar.Width = manager.ScreenWidth;
            botbar.Top = manager.ScreenHeight - botbar.Height;
            manager.Add(botbar);

            pauseMenu = new PauseMenu(context, manager);
            pauseMenu.Init();
            pauseMenu.Hide();
            manager.Add(pauseMenu);

            chatPanel = new BottomBarPanel(manager);
            chatPanel.Init();
            chatPanel.Width = 600;
            chatPanel.Left = 10;
            chatPanel.Top = 10;
            chatPanel.Height = botbar.Height - 20;
            botbar.Add(chatPanel);

            console = new Controls.Console(manager);
            console.Init();
            console.Left = 4;
            console.Width = chatPanel.Width - 8;
            console.Height = chatPanel.Height - 4;
            console.Anchor = Anchors.All;
            console.Channels.Add(new ConsoleChannel(0, "System", Color.DarkOrange, false));
            console.Channels.Add(new ConsoleChannel(1, "User", Color.White, false));
            console.Channels.Add(new ConsoleChannel(2, "General", Color.LightBlue, false));
            console.Channels.Add(new ConsoleChannel(3, "Debug", Color.White, false));
            console.ChannelsVisible = false;
            console.MessageFormat = ConsoleMessageFormats.All;
            console.MessageSent += new ConsoleMessageEventHandler(MessageSent);
            console.SelectedChannel = 1;
            chatPanel.Add(console);

            infoPanel = new BottomBarPanel(manager);
            infoPanel.Init();
            infoPanel.Width = manager.ScreenWidth - chatPanel.Width - 30;
            infoPanel.Left = chatPanel.Width + 20;
            infoPanel.Top = 10;
            infoPanel.Height = botbar.Height - 20;
            botbar.Add(infoPanel);

            panel = new Panel(manager);
            panel.Init();
            panel.Left = 4;
            panel.Width = infoPanel.Width - 8;
            panel.Height = infoPanel.Height - 4;
            panel.Anchor = Anchors.All;
            panel.Color = new Color(80, 80, 80);
            panel.BevelColor = Color.Black;
            panel.BevelBorder = BevelBorder.All;
            panel.BevelStyle = BevelStyle.Flat;
            infoPanel.Add(panel);

            InitCharWindow();
            InitCharInfo();
            InitDebugWindow();
        }

        #region Utilities
        public Button CreateButton(Control parent, int left, int top, int width, string text, Controls.EventHandler eventHandler)
        {
            Button button = new Button(manager);

            button.Init();
            button.Parent = parent;

            button.Left = left;
            button.Top = top;
            button.Width = width;
            button.Click += eventHandler;

            button.Text = text;

            return button;
        }
        #endregion

        #region Pause Menu
        /// <summary>
        /// Makes the pause menu visible.
        /// </summary>
        public void ShowPauseMenu()
        {
            pauseMenu.Show();

            if (charsPanel.Visible)
                charsPanel.Hide();
        }

        /// <summary>
        /// Makes the pause menu not visible.
        /// </summary>
        public void HidePauseMenu()
        {
            pauseMenu.Hide();

            if (context.Network.PlayerCharacter == null)
                charsPanel.Show();
        }

        /// <summary>
        /// Sets pause menu visibility opposite what it currently is.
        /// </summary>
        public void TogglePauseMenu()
        {
            if (pauseMenu.Visible)
                HidePauseMenu();
            else
                ShowPauseMenu();
        }
        #endregion

        #region Popup
        public void ShowPopup(string title, string message)
        {
            Window win = new Window(manager);
            win.Init();
            win.Width = 400;
            win.Height = 150;
            win.IconVisible = false;
            win.Text = title;
            manager.Add(win);

            Button button = new Button(manager);
            button.Init();
            button.Text = "OK";
            button.Left = (win.ClientWidth / 2) - (button.Width / 2);
            button.Top = win.ClientHeight - button.Height - 4;
            button.Click += new Controls.EventHandler(PopupClick);
            button.Tag = win;
            win.Add(button);

            Label label = new Label(manager);
            label.Init();
            label.Top = 4;
            label.Left = 4;
            label.Ellipsis = false;
            label.Width = win.ClientWidth - 8;
            label.Height = win.ClientHeight - button.Height - 12;
            label.Text = message;
            win.Add(label);
        }

        private void PopupClick(object sender, Controls.EventArgs e)
        {
            ((sender as Button).Tag as Window).Close();
        }
        #endregion

        #region Character Info
        private void InitCharInfo()
        {
            logoutButton = new Button(manager);
            logoutButton.Init();
            logoutButton.Left = 4;
            logoutButton.Top = panel.Height - logoutButton.Height - 4;
            logoutButton.Text = "Logout";
            logoutButton.Click += new Controls.EventHandler(logoutButton_Click);
            panel.Add(logoutButton);

            quitButton = new Button(manager);
            quitButton.Init();
            quitButton.Left = logoutButton.Left + logoutButton.Width + 4;
            quitButton.Top = logoutButton.Top;
            quitButton.Text = "Quit";
            quitButton.Click += new Controls.EventHandler(quitButton_Click);
            panel.Add(quitButton);

            debugButton = new Button(manager);
            debugButton.Init();
            debugButton.Left = panel.Width - debugButton.Width - 4;
            debugButton.Top = logoutButton.Top;
            debugButton.Text = "Debug";
            debugButton.Click += new Controls.EventHandler(debugButton_Click);
            panel.Add(debugButton);

            name = new Label(manager);
            name.Init();
            name.Top = 4;
            name.Left = 4;
            name.Width = 300;
            name.Text = "Name: ";
            panel.Add(name);

            zone = new Label(manager);
            zone.Init();
            zone.Top = name.Top + name.Height + 4;
            zone.Left = 4;
            zone.Width = 300;
            zone.Text = "Zone: ";
            panel.Add(zone);

            zoneList = new ListBox(manager);
            zoneList.Init();
            zoneList.Width = 250;
            zoneList.Top = name.Top + name.Height + 4;
            zoneList.Left = panel.Width - zoneList.Width - 4;
            zoneList.Height = panel.Height - debugButton.Height - name.Height - 16;
            zoneList.TextColor = Color.LightBlue;
            panel.Add(zoneList);

            zoneListLabel = new Label(manager);
            zoneListLabel.Init();
            zoneListLabel.Top = 4;
            zoneListLabel.Left = zoneList.Left;
            zoneListLabel.Width = 100;
            zoneListLabel.Text = "Current zones:";
            panel.Add(zoneListLabel);

            entityList = new ListBox(manager);
            entityList.Init();
            entityList.Width = 350;
            entityList.Top = name.Top + name.Height + 4;
            entityList.Left = zoneList.Left - entityList.Width - 8;
            entityList.Height = panel.Height - debugButton.Height - name.Height - 16;
            entityList.TextColor = Color.LightBlue;
            panel.Add(entityList);

            entityListLabel = new Label(manager);
            entityListLabel.Init();
            entityListLabel.Top = 4;
            entityListLabel.Left = entityList.Left;
            entityListLabel.Width = 100;
            entityListLabel.Text = "Zone Entities:";
            panel.Add(entityListLabel);
        }

        public void ClearCharacterInfo()
        {
            name.Text = "Name: None";
            zone.Text = "Zone: None";
            entityList.Items.Clear();
            zoneList.Items.Clear();
        }

        public void UpdateCharacterInfo()
        {
            Character c = context.Network.PlayerCharacter;
            if (c != null)
            {
                name.Text = "Name: " + c.Name;

                string zoneInfo = "None";
                Zone z = context.ZoneManager.GetZone(c.Zone);
                if (z != null)
                    zoneInfo = String.Format("{0} ({1})", z.Name, z.Id);
                zone.Text = "Zone: " + zoneInfo;

                zoneList.Items.Clear();
                foreach (Zone z2 in context.ZoneManager.Zones)
                {
                    zoneInfo = String.Format("I:{0}, N:'{1}', S:{2}x{3}, E:{4}({5}).", z2.Id, z2.Name, z2.Width, z2.Height, z2.AllEntities.Count, z2.Characters.Count);
                    zoneList.Items.Add(zoneInfo);
                }

                if (z != null)
                {
                    string entityInfo = "None";
                    entityList.Items.Clear();
                    foreach (Entity e in z.AllEntities)
                    {
                        entityInfo = String.Format("I:{0}, T:{1}, N:'{2}', P:{3}, O:{4}", e.Id, e.TypeId, e.Name, e.Position, e.Owner);
                        entityList.Items.Add(entityInfo);
                    }
                }
            }
        }

        void logoutButton_Click(object sender, Controls.EventArgs e)
        {
            context.Network.Logout();
        }

        void quitButton_Click(object sender, Controls.EventArgs e)
        {
            context.Network.Quit();
        }

        void debugButton_Click(object sender, Controls.EventArgs e)
        {
            ShowDebugWindow();
        }
        #endregion

        #region Console
        public void SetConsoleFocus()
        {
            console.InputFocus = true;
        }

        public void SetConsoleFocus(string text)
        {
            console.AddInput(text);
            console.InputFocus = true;
        }

        /// <summary>
        /// Adds the message to the console.
        /// </summary>
        public void AddMessage(byte channel, string message, params object[] args)
        {
            string msg = String.Format(message, args);
            console.MessageBuffer.Add(new ConsoleMessage(msg, channel));
        }

        /// <summary>
        /// Adds the message to the console.
        /// </summary>
        public void AddMessage(byte channel, string message)
        {
            console.MessageBuffer.Add(new ConsoleMessage(message, channel));
        }

        void MessageSent(object sender, ConsoleMessageEventArgs e)
        {
            string message = e.Message.Text;
            if (message.StartsWith("/"))
            {
                e.Message.Send = false;
                e.Message.Channel = 1;
                string[] args = message.Split(' ');
                string command = args[0].Substring(1);
                switch (command)
                {
                    case "clear":
                        console.MessageBuffer.Clear();
                        break;

                    case "popup":
                        ShowPopup("Popup!", message);
                        break;

                    case "channels":
                        string chans = "Channels: ";
                        foreach (ConsoleChannel chan in console.Channels)
                            chans += String.Format("{0}({1}), ", chan.Name.ToString(), chan.Visible);

                        AddMessage(0, chans);
                        break;
                    case "addchannel":
                        if (args.Length == 4)
                        {
                            try
                            {
                                byte index = Convert.ToByte(args[1]);
                                string name = args[2];
                                Color color = Color.White;
                                bool visible = Convert.ToBoolean(args[3]);
                                console.Channels.Add(new ConsoleChannel(index, name, color, visible));
                                AddMessage(0, "Channel created. Index: {0}, Name: {1}, Color: White, Visible: {2}.", index, name, visible);
                            }
                            catch (Exception ex)
                            {
                                AddMessage(0, "Command exception: {0}", ex.Message);
                            }
                        }
                        else
                            AddMessage(0, "Invalid command arguments.");
                        break;

                    case "removechannel":
                        break;

                    case "tell":
                        if (args.Length > 2)
                        {
                            string player = args[1];
                            string mes = "";
                            for (int i = 2; i < args.Length; i++)
                                mes += args[i] + " ";
                            context.Network.SendChatMessage(ChatType.Direct, 5, mes);
                        }
                        break;

                    default:
                        context.Network.SendChatMessage(ChatType.All, 0, message);
                        break;
                }
            }
            else
            {
                context.Network.SendChatMessage(ChatType.All, 0, message);
            }
        }
        #endregion

        #region Debug Window
        private void InitDebugWindow()
        {
            debugWindow = new Window(manager);
            debugWindow.Init();
            debugWindow.Width = 700;
            debugWindow.Height = 400;
            debugWindow.Text = "Debug Console";
            debugWindow.Center();
            debugWindow.Hide();
            manager.Add(debugWindow);

            debugConsole = new Controls.Console(manager);
            debugConsole.Left = 4;
            debugConsole.Top = 4;
            debugConsole.Width = debugWindow.ClientWidth - 8;
            debugConsole.Height = debugWindow.ClientHeight - 8;
            debugConsole.Anchor = Anchors.All;
            debugConsole.AutoScroll = true;
            debugConsole.Channels.Add(new ConsoleChannel(0, "Debug", Color.LightCoral, false));
            debugConsole.ChannelsVisible = false;
            debugConsole.SelectedChannel = 0;
            debugWindow.Add(debugConsole);
        }

        public void ShowDebugWindow()
        {
            debugWindow.Show();
        }

        public void HideDebugWindow()
        {
            debugWindow.Hide();
        }

        /// <summary>
        /// Adds the message to the debug window.
        /// </summary>
        public void AddDebugMessage(string message, params object[] args)
        {
            string msg = String.Format(message, args);
            debugConsole.MessageBuffer.Add(new ConsoleMessage(msg, 0));
        }

        /// <summary>
        /// Adds the message to the debug window.
        /// </summary>
        public void AddDebugMessage(string message)
        {
            debugConsole.MessageBuffer.Add(new ConsoleMessage(message, 0));
        }
        #endregion

        #region Character Select Window
        private void InitCharWindow()
        {
            charsPanel = new Panel(manager);
            charsPanel.Init();
            charsPanel.Width = 600;
            charsPanel.Height = 300;
            charsPanel.Left = (manager.TargetWidth / 2) - charsPanel.Width / 2;
            charsPanel.Top = (manager.TargetHeight / 2) - charsPanel.Height / 2;
            charsPanel.Passive = true;
            charsPanel.StayOnBack = true;
            charsPanel.CanFocus = false;
            manager.Add(charsPanel);

            charsList = new ListBox(manager);
            charsList.Init();
            charsList.Left = 4;
            charsList.Top = 4;
            charsList.TextColor = Color.White;
            charsList.Width = charsPanel.ClientWidth - 8;
            charsList.Height = charsPanel.ClientHeight - 58;
            charsPanel.Add(charsList);

            createName = new TextBox(manager);
            createName.Init();
            createName.Top = charsList.Top + charsList.Height + 4;
            createName.Left = 4;
            createName.Width = charsPanel.Width - 8;
            charsPanel.Add(createName);

            createButton = new Button(manager);
            createButton.Init();
            createButton.Top = createName.Top + createName.Height + 4;
            createButton.Text = "Create Character";
            createButton.Left = 4;
            createButton.Width = 150;
            createButton.Click += new Controls.EventHandler(createButton_Click);
            charsPanel.Add(createButton);

            deleteButton = new Button(manager);
            deleteButton.Init();
            deleteButton.Top = createName.Top + createName.Height + 4;
            deleteButton.Text = "Delete Character";
            deleteButton.Left = createButton.Left + 158;
            deleteButton.Width = 150;
            deleteButton.Click += new Controls.EventHandler(deleteButton_Click);
            charsPanel.Add(deleteButton);

            startButton = new Button(manager);
            startButton.Init();
            startButton.Top = createName.Top + createName.Height + 4;
            startButton.Text = "Start Selected";
            startButton.Left = charsPanel.Width - 154;
            startButton.Width = 150;
            startButton.Click += new Controls.EventHandler(startButton_Click);
            charsPanel.Add(startButton);
        }

        public void HideCharSelect()
        {
            charsPanel.Hide();
        }

        public void ShowCharSelect()
        {
            charsPanel.Show();
        }

        void createButton_Click(object sender, Controls.EventArgs e)
        {
            // TODO: [LOW] add some checking to make sure it's a valid name ie. no spaces, symbols, etc
            if (!string.IsNullOrWhiteSpace(createName.Text))
                context.Network.CreateCharacter(createName.Text);
            else
                AddMessage(0, "Unable to create. Invalid character name!");
        }

        void deleteButton_Click(object sender, Controls.EventArgs e)
        {
            foreach (string item in charsList.Items)
            {
                if (item.Contains(String.Format("NAME:{0}", createName.Text)))
                {
                    int id = Convert.ToInt32(item.Substring(3, (item.IndexOf("-") - 4)));
                    context.Network.DeleteCharacter(id);
                }
            }
        }

        void startButton_Click(object sender, Controls.EventArgs e)
        {
            string item = (string)charsList.Items[charsList.ItemIndex];
            int id = Convert.ToInt32(item.Substring(3, (item.IndexOf("-") - 4)));
            context.Network.StartCharacter(id);
        }
        #endregion

        #endregion
    }
}
