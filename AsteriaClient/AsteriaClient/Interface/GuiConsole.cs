using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AsteriaClient.Interface.Controls;
using Microsoft.Xna.Framework;
using Controls = AsteriaClient.Interface.Controls;

namespace AsteriaClient.Interface
{
    class GuiConsole
    {
        #region Variables
        private Manager manager;
        private TabControl tabControl;
        private Dictionary<TabPage, Controls.Console> consoles;
        private ChannelList channels;
        #endregion

        #region Properties
        /// <summary>
        /// Returns the currently selected tab.
        /// </summary>
        public TabPage CurrentTab
        {
            get { return tabControl.SelectedPage; }
        }

        /// <summary>
        /// Returns the console of the current tab.
        /// </summary>
        public Controls.Console CurrentConsole
        {
            get { return tabControl.SelectedPage.GetControl("console") as Controls.Console; }
        }

        /// <summary>
        /// Returns the channel list.
        /// </summary>
        public ChannelList Channels
        {
            get { return channels; }
        }
        #endregion

        #region Constructors
        public GuiConsole(Manager manager, bool debug)
        {
            // TODO: [MID] create a more modular console with custom methods to manage it.
            // TODO: [MID] rewrite how channels work ie. make it possible for hidden channels.
            // TODO: [LOW] implement input history with up arrow to send previous messages.

            this.manager = manager;

            tabControl = new TabControl(manager);
            consoles = new Dictionary<TabPage, Controls.Console>();
            channels = new ChannelList();

            tabControl.Init();
            tabControl.Top = 200;
            tabControl.Left = 200;
            tabControl.Height = 220;
            tabControl.Width = 400;
            tabControl.Movable = true;
            tabControl.Resizable = true;
            tabControl.MinimumHeight = 96;
            tabControl.MinimumWidth = 160;
            manager.Add(tabControl);

            AddChannel(0, "System", Color.DarkOrange, false);
            AddChannel(1, "User", Color.White, false);
            AddChannel(2, "General", Color.LightBlue, true);

            AddConsole("Global", 2, ConsoleMessageFormats.All, MessageSent, true, "Welcome to Asteria!");
            AddConsole("Debug", 1, ConsoleMessageFormats.ChannelName, MessageSent, false);
        }
        #endregion

        #region Methods
        /// <summary>
        /// Iterates through the tab pages to find the one with the specified name.
        /// </summary>
        public TabPage FindConsoleTab(string name)
        {
            foreach (TabPage page in tabControl.TabPages)
            {
                if (page.Name.ToLower() == name.ToLower())
                    return page;
            }
            return null;
        }

        /// <summary>
        /// Creates a tab page and embeds a console with the specified parameters.
        /// </summary>
        public void AddConsole(string name, byte selected, ConsoleMessageFormats format,
            ConsoleMessageEventHandler handler, bool showChannels, string motd)
        {
            // Create tab as usual.
            AddConsole(name, selected, format, handler, showChannels);

            // Add specified message of the day.
            TabPage tab = FindConsoleTab(name);
            if (tab != null)
                consoles[tab].MessageBuffer.Add(new ConsoleMessage(motd, 0));
        }

        /// <summary>
        /// Creates a tab page and embeds a console with the specified parameters.
        /// </summary>
        public void AddConsole(string name, byte selected, ConsoleMessageFormats format,
            ConsoleMessageEventHandler handler, bool showChannels)
        {
            // Create a new tab page and a new console.
            TabPage tab = tabControl.AddPage(name);
            tab.Name = name;
            //tab.Name = name;
            consoles.Add(tab, new Controls.Console(manager));

            consoles[tab].Init();
            consoles[tab].Name = "console";

            // Setup the console inside the tab page.
            tab.Add(consoles[tab]);
            consoles[tab].Width = tab.ClientWidth;
            consoles[tab].Height = tab.ClientHeight;
            consoles[tab].Anchor = Anchors.All;
            consoles[tab].Channels = channels;
            consoles[tab].SelectedChannel = selected;
            consoles[tab].ChannelsVisible = showChannels;
            consoles[tab].MessageFormat = format;
            consoles[tab].MessageSent += handler;
        }

        /// <summary>
        /// Removes the named tab and console.
        /// </summary>
        public void RemoveConsole(string name)
        {
            TabPage page = FindConsoleTab(name);
            if (page != null)
                tabControl.RemovePage(page, true);
        }

        /// <summary>
        /// Adds the specified channel to the list.
        /// </summary>
        public void AddChannel(byte index, string name, Color color, bool visible)
        {
            // TODO: [LOW] create a better way to manage channel indexes.
            channels.Add(new ConsoleChannel(index, name, color, visible));
        }

        /// <summary>
        /// Removes the specified channel from the list.
        /// </summary>
        public void RemoveChannel(string name)
        {
            // TODO: [LOW] find a cleaner way to do this.
            ConsoleChannel remove = null;
            foreach (ConsoleChannel chan in channels)
            {
                if (chan.Name.ToLower() == name.ToLower())
                    remove = chan;
            }

            if (remove != null)
                channels.Remove(remove);
        }

        /// <summary>
        /// Adds the message to the specified console.
        /// </summary>
        public void AddMessage(byte channel, string console, string message)
        {
            TabPage tab = FindConsoleTab(console);
            if (tab != null)
                consoles[tab].MessageBuffer.Add(new ConsoleMessage(message, channel));
        }

        /// <summary>
        /// Adds the message to the specified console.
        /// </summary>
        public void AddMessage(byte channel, string console, string message, params object[] args)
        {
            string msg = String.Format(message, args);
            TabPage tab = FindConsoleTab(console);
            if (tab != null)
                consoles[tab].MessageBuffer.Add(new ConsoleMessage(msg, channel));
        }

        /// <summary>
        /// Adds the message to the current console.
        /// </summary>
        public void AddMessage(byte channel, string message, params object[] args)
        {
            string msg = String.Format(message, args);
            CurrentConsole.MessageBuffer.Add(new ConsoleMessage(msg, channel));
        }

        /// <summary>
        /// Adds the message to the current console.
        /// </summary>
        public void AddMessage(byte channel, string message)
        {
            CurrentConsole.MessageBuffer.Add(new ConsoleMessage(message, channel));
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
                        CurrentConsole.MessageBuffer.Clear();
                        break;

                    case "channels":
                        string chans = "Channels: ";
                        foreach (ConsoleChannel chan in channels)
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
                                AddChannel(index, name, color, visible);
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
        }
        #endregion
    }
}
