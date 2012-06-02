using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AsteriaClient.Interface.Controls;
using Microsoft.Xna.Framework;
using Controls = AsteriaClient.Interface.Controls;
using AsteriaLibrary.Shared;
using AsteriaLibrary.Messages;

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

        private BottomBar botbar;
        private BottomBarPanel chatPanel;
        private BottomBarPanel botpanel2;
        private Controls.Console console;
        private Panel panel;

        private Window charsWindow;
        public ListBox charsList;
        private TextBox createName;
        private Button createButton;
        private Button deleteButton;
        private Button startButton;

        private Label Name;
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

            InitInterface();
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

        private void InitInterface()
        {
            botbar = new BottomBar(manager);
            botbar.Init();
            botbar.Height = 200;
            botbar.Width = manager.ScreenWidth;
            botbar.Top = manager.ScreenHeight - botbar.Height;
            manager.Add(botbar);

            chatPanel = new BottomBarPanel(manager);
            chatPanel.Init();
            chatPanel.Width = 800;
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

            botpanel2 = new BottomBarPanel(manager);
            botpanel2.Init();
            botpanel2.Width = manager.ScreenWidth - chatPanel.Width - 30;
            botpanel2.Left = chatPanel.Width + 20;
            botpanel2.Top = 10;
            botpanel2.Height = botbar.Height - 20;
            botbar.Add(botpanel2);

            panel = new Panel(manager);
            panel.Init();
            panel.Left = 4;
            panel.Width = botpanel2.Width - 8;
            panel.Height = botpanel2.Height - 4;
            panel.Anchor = Anchors.All;
            panel.Color = new Color(80, 80, 80);
            panel.BevelColor = Color.Black;
            panel.BevelBorder = BevelBorder.All;
            panel.BevelStyle = BevelStyle.Flat;
            botpanel2.Add(panel);

            InitCharWindow();
            InitCharInfo();
        }

        private void InitCharWindow()
        {
            charsWindow = new Window(manager);
            charsWindow.Init();
            charsWindow.Width = 600;
            charsWindow.Height = 300;
            charsWindow.Center();
            charsWindow.Resizable = false;
            manager.Add(charsWindow);

            charsList = new ListBox(manager);
            charsList.Init();
            charsList.Left = 4;
            charsList.Top = 4;
            charsList.TextColor = Color.White;
            charsList.Width = charsWindow.Width - 22;
            charsList.Height = charsWindow.Height - 94;
            charsWindow.Add(charsList);

            createName = new TextBox(manager);
            createName.Init();
            createName.Top = charsList.Top + charsList.Height + 4;
            createName.Left = 4;
            createName.Width = charsWindow.Width - 22;
            charsWindow.Add(createName);

            createButton = new Button(manager);
            createButton.Init();
            createButton.Top = createName.Top + createName.Height + 4;
            createButton.Text = "Create Character";
            createButton.Left = 4;
            createButton.Width = 150;
            createButton.Click += new Controls.EventHandler(createButton_Click);
            charsWindow.Add(createButton);

            deleteButton = new Button(manager);
            deleteButton.Init();
            deleteButton.Top = createName.Top + createName.Height + 4;
            deleteButton.Text = "Delete Character";
            deleteButton.Left = createButton.Left + 158;
            deleteButton.Width = 150;
            deleteButton.Click += new Controls.EventHandler(deleteButton_Click);
            charsWindow.Add(deleteButton);

            startButton = new Button(manager);
            startButton.Init();
            startButton.Top = createName.Top + createName.Height + 4;
            startButton.Text = "Start Selected";
            startButton.Left = charsWindow.Width - 168;
            startButton.Width = 150;
            startButton.Click += new Controls.EventHandler(startButton_Click);
            charsWindow.Add(startButton);
        }

        private void InitCharInfo()
        {

        }

        /// <summary>
        /// Adds the message to the current console.
        /// </summary>
        public void AddMessage(byte channel, string message, params object[] args)
        {
            string msg = String.Format(message, args);
            console.MessageBuffer.Add(new ConsoleMessage(msg, channel));
        }

        /// <summary>
        /// Adds the message to the current console.
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

                    default:
                        AddMessage(0, "Invalid command!");
                        break;
                }
            }
            else
            {
                ClientToServerMessage pm = new ClientToServerMessage();
                pm.MessageType = MessageType.C2S_PlayerChat;
                pm.Data = "0|0";
                pm.GameData = message;
                context.Network.SendMessage(pm);
            }
        }

        void createButton_Click(object sender, Controls.EventArgs e)
        {
            // TODO: [LOW] add some checking to make sure it's a valid name ie. no spaces, symbols, etc
            context.Network.CreateCharacter(createName.Text);
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
    }
}
