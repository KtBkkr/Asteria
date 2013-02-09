using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using AsteriaLibrary.Shared;
using AsteriaWorldServer.Messages;
using AsteriaLibrary.Messages;
using AsteriaLibrary.Entities;
using AsteriaWorldServer.PlayerCache;
using AsteriaLibrary.Data;

namespace AsteriaWorldServer
{
    // Message Destinations:
    // If type is direct, message is sent to character with dest ID.
    // If type is channel, message is sent to channel with dest ID.

    public class Channel
    {
        #region Fields
        public int Id;
        public string Name;
        public int Type; // 0=Player, 1=Guild, 2=Group
        public string Motd;
        public string Password;
        public int Owner;
        #endregion

        #region Constructors
        public Channel(int id, string name, int type, int owner)
        {
            this.Id = id;
            this.Type = type;
            this.Name = name;
            this.Owner = owner;
        }
        #endregion
    }

    public class ChatMessage
    {
        #region Fields
        public Character Sender;
        public ChatType Type;
        public int Destination;
        public string Message;
        #endregion

        #region Constructors
        public ChatMessage(Character sender, ChatType type, int dest, string message)
        {
            this.Sender = sender;
            this.Type = type;
            this.Destination = dest;
            this.Message = message;
        }

        public ChatMessage(ChatType type, int dest, string message)
        {
            this.Sender = null;
            this.Type = type;
            this.Destination = dest;
            this.Message = message;
        }
        #endregion
    }

    /// <summary>
    /// Handles chat messages and dispatches any commands to the command handler.
    /// </summary>
    public class ChatProcessor
    {
        #region Fields
        private ServerContext context;
        private CommandHandler commandHandler;
        private Dictionary<int, Channel> channels;
        #endregion

        #region Properties
        #endregion

        #region Constructors
        public ChatProcessor(ServerContext context)
        {
            this.context = context;
            this.commandHandler = new CommandHandler(context);
            this.channels = new Dictionary<int, Channel>();
        }
        #endregion

        #region Methods

        #region Channels
        private Channel GetChannelByName(string name)
        {
            foreach (Channel c in channels.Values)
            {
                if (c.Name == name)
                    return c;
            }
            return null;
        }
        #endregion

        public void ProcessMessage(ClientToServerMessage msg)
        {
            Character character = context.Mpt.GetCharacterByCharacterId(msg.CharacterId);
            if (character != null)
            {
                try
                {
                    ChatType type = (ChatType)Convert.ToInt32(msg.Data.Split('|')[0]);
                    int dest = Convert.ToInt32(msg.Data.Split('|')[1]);
                    string text = msg.GameData;
                    ChatMessage message = new ChatMessage(character, type, dest, text);

                    if (text.StartsWith("/"))
                        HandleCommand(message);
                    else
                        HandleChat(message);
                }
                catch (Exception e)
                {
                    // Client possibly tampered with chat protocol.
                    Logger.Output(this, "Chat process error: {0}, {1}.", e.Message, e.StackTrace);
                }
            }
            else
            {
                // Invalid char ID should never get this far.. but log it anyway.
                Logger.Output(this, "Chat process found unknown character ID {0}.", msg.CharacterId);
            }
        }

        public void HandleCommand(ChatMessage msg)
        {
            string message = msg.Message;
            if (string.IsNullOrWhiteSpace(message))
                return;

            string command;
            string[] args;
            if (message.Split(' ').Length > 1)
            {
                // command with args
                command = message.Substring(1, message.IndexOf(' ') - 1);
                args = CommandHandler.Parse(message.Substring(message.IndexOf(' ')));
            }
            else
            {
                // standalone command
                command = message.Substring(1, message.Length - 1);
                args = new string[0];
            }

            if (string.IsNullOrWhiteSpace(command))
                return;

            Character sender = msg.Sender;
            Logger.Output(this, "Received command from ({0}){1}. Command: \"{2}\".", sender.CharacterId, sender.Name, message);

            switch (command)
            {
                case "createchannel":
                    string name = args[0];
                    string password = null;
                    if (args.Length > 2)
                        password = args[1];

                    Channel c = context.Dal.CreateChannel(name, 0, password, sender.CharacterId);
                    if (c != null)
                    {
                        channels.Add(c.Id, c);
                        SendServerMessage("Channel created sucessfully!", sender.CharacterId);
                        HandleCommand(new ChatMessage(sender, ChatType.All, 0, "/joinchannel " + name));
                        // TODO: [LOW] should we use goto case here instead of "fake" a command?
                    }
                    else
                        SendServerMessage("Channel could not be created!", sender.CharacterId);

                    break;

                case "joinchannel":
                    name = args[0];
                    c = GetChannelByName(name);
                    if (c != null)
                    {
                        sender.Channels.Add(c.Id);
                        SendServerMessage(String.Format("You've joined the channel {0}.", name), sender.CharacterId);
                    }
                    else
                        SendServerMessage("Channel could not be found!", sender.CharacterId);

                    break;

                case "leavechannel":
                    name = args[0];
                    c = GetChannelByName(name);
                    if (c != null)
                    {
                        sender.Channels.Remove(c.Id);
                        SendServerMessage(String.Format("You've left the channel {0}.", name), sender.CharacterId);
                    }
                    else
                        SendServerMessage("Channel could not be found!", sender.CharacterId);

                    break;

                case "tell":
                case "say":
                case "msg":
                case "w":
                    SendServerMessage("Error 0, that should be handled by the client!", sender.CharacterId);
                    break;

                default:
                    // Non a chat related command, send it to command handler.
                    commandHandler.HandleCommand(sender, command, args);
                    break;
            }
        }

        public void HandleChat(ChatMessage msg)
        {
            // A character can't talk if it doesn't exist..
            if (msg.Sender == null || string.IsNullOrWhiteSpace(msg.Message))
                return;

            Logger.Output(this, "Received chat from ({0}){1}. T: {2}, D: {3}, M: \"{4}\".", msg.Sender.CharacterId, msg.Sender.Name, msg.Type, msg.Destination, msg.Message);

            if (msg.Type == ChatType.Direct)
            {
                // Make sure dest char exists.
                Character c = context.Mpt.GetCharacterByCharacterId(msg.Destination);
                if (c != null)
                    SendMessage(new ChatMessage(msg.Sender, ChatType.Direct, msg.Destination, msg.Message));
                else
                    SendServerMessage("Character not found.", msg.Sender.CharacterId);
            }
            else if (msg.Type == ChatType.Channel)
            {
                // make sure the channel exists.
                if (channels.ContainsKey(msg.Destination))
                    SendMessage(new ChatMessage(msg.Sender, ChatType.Channel, msg.Destination, msg.Message));
                else
                    SendServerMessage("Channel not found.", msg.Sender.CharacterId);
            }
            else if (msg.Type == ChatType.Zone)
            {
                // Make sure the zone exists.
                if (context.ZoneManager.ZoneExists(msg.Destination))
                    SendMessage(new ChatMessage(msg.Sender, ChatType.Zone, msg.Destination, msg.Message));
                else
                    SendServerMessage("Error sending zone chat, zone not found!");
            }
            else if (msg.Type == ChatType.All)
            {
                SendMessage(new ChatMessage(msg.Sender, ChatType.All, 0, msg.Message));
            }
        }

        public void SendMessage(ChatMessage msg)
        {
            ServerToClientMessage wm = ServerToClientMessage.CreateMessageSafe();
            string from = msg.Sender != null ? msg.Sender.Name : "SERVER";
            string format = String.Format("UNKNOWN: {0}", msg.Message);

            if (msg.Type == ChatType.Direct)
            {
                if (msg.Sender != null)
                {
                    // If it's not from the server, give the sending player one too.
                    string to = context.Mpt.GetCharacterByCharacterId(msg.Destination).Name;
                    format = String.Format("[Me -> {0}]: {1}", to, msg.Message);

                    ServerToClientMessage wm2 = ServerToClientMessage.CreateMessageSafe();
                    MessageFormatter.CreateChatMessage(msg.Type, format, wm2);
                    msg.Sender.MessageBuffer.Add(wm2);
                }

                // The message is for the receiving player.
                format = String.Format("[{0} -> Me]: {1}", from, msg.Message);

                Character c = context.Mpt.GetCharacterByCharacterId(msg.Destination);
                MessageFormatter.CreateChatMessage(msg.Type, format, wm);
                c.MessageBuffer.Add(ServerToClientMessage.Copy(wm, c.Sender));
            }
            else if (msg.Type == ChatType.Channel)
            {
                // The message is going to a specific channel.
                string channel = "CHANNEL";
                if (channels.ContainsKey(msg.Destination))
                    channel = channels[msg.Destination].Name;

                format = String.Format("[{0}] {1}: {2}", channel, from, msg.Message);
                MessageFormatter.CreateChatMessage(msg.Type, format, wm);
                SendChannelMessage(msg.Destination, wm);
            }
            else if (msg.Type == ChatType.Zone)
            {
                // The message is going to everyone in the dest zone.
                format = String.Format("{0}: {1}", msg.Sender.Name, msg.Message);

                MessageFormatter.CreateChatMessage(msg.Type, format, wm);
                context.ZoneManager.AddMessageToZone(msg.Destination, wm);
            }
            else if (msg.Type == ChatType.All)
            {
                // The message is going to everyone on the server.
                // TODO: [MID] create static channels based on ChatType.All dest.
                format = String.Format("{0}: {1}", msg.Sender.Name, msg.Message);

                MessageFormatter.CreateChatMessage(msg.Type, format, wm);
                context.ZoneManager.AddMessageToAllZones(wm);
            }
            ServerToClientMessage.FreeSafe(wm);
        }

        private void SendChannelMessage(int channel, ServerToClientMessage wm)
        {
            LinkedNode<MasterPlayerRecord> node = context.Mpt.First;
            while (node != null)
            {
                Character c = (Character)node.Value.pCharacter;
                if (c.Channels.Contains(channel))
                    c.MessageBuffer.Add(ServerToClientMessage.Copy(wm, c.Sender));

                node = node.Next;
            }
        }

        public void SendServerMessage(string message)
        {
            ChatMessage msg = new ChatMessage(ChatType.All, 0, message);
            SendMessage(msg);
        }

        public void SendServerMessage(string message, int playerId)
        {
            ChatMessage msg = new ChatMessage(ChatType.Direct, playerId, message);
            SendMessage(msg);
        }
        #endregion
    }
}
