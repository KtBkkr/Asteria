using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections;
using AsteriaLibrary.Entities;
using AsteriaWorldServer.Zones;
using AsteriaLibrary.Messages;
using AsteriaWorldServer.Messages;
using System.Text.RegularExpressions;
using AsteriaLibrary.Shared;
using System.Threading;

namespace AsteriaWorldServer
{
    public class CommandHandler
    {
        #region Fields
        private ServerContext context;
        #endregion

        #region Constructors
        public CommandHandler(ServerContext context)
        {
            this.context = context;
        }
        #endregion

        #region Methods
        public void HandleCommand(Character sender, string command, string[] args)
        {
            try
            {
                #region Debugging
                if (command == "testparse")
                {
                    string argList = "";
                    foreach (string a in args)
                        argList += a + "|";

                    string message = String.Format("Command \"{0}\", Args \"{1}\"", command, argList.Trim('|'));
                    context.ChatProcessor.SendServerMessage(message, sender.CharacterId);
                }
                #endregion

                #region Zone Management
                if (command == "addzone")
                {
                    if (args.Length > 2)
                    {
                        string name = args[0];
                        int width = Convert.ToInt32(args[1]);
                        int height = Convert.ToInt32(args[2]);
                        if (context.ZoneManager.AddZone(name, width, height))
                            context.ChatProcessor.SendServerMessage(String.Format("Zone '{0}' created sucessfully.", name), sender.CharacterId);
                        else
                            context.ChatProcessor.SendServerMessage("Failed to create zone, name must be taken.", sender.CharacterId);
                    }
                }
                else if (command == "removezone")
                {
                    if (args.Length > 0)
                    {
                        int id = Convert.ToInt32(args[0]);
                        Zone zone = context.ZoneManager.GetZone(id);
                        if (zone != null)
                        {
                            if (context.ZoneManager.DeleteZone(id))
                                context.ChatProcessor.SendServerMessage(String.Format("Zone '{0}' deleted sucessfully.", zone.Name), sender.CharacterId);
                            else
                                context.ChatProcessor.SendServerMessage("Failed to delete zone. Unknown problem.", sender.CharacterId);
                        }
                        else
                            context.ChatProcessor.SendServerMessage("Failed to delete zone. Zone doesn't exist.", sender.CharacterId);
                    }
                }
                else if (command == "editzone")
                {
                    if (args.Length > 3)
                    {
                        int id = Convert.ToInt32(args[0]);
                        Zone zone = context.ZoneManager.GetZone(id);
                        if (zone != null)
                        {
                            string name = args[1];
                            int width = Convert.ToInt32(args[2]);
                            int height = Convert.ToInt32(args[3]);
                            zone.ChangeZone(name, width, height);
                            context.ChatProcessor.SendServerMessage(String.Format("Zone '{0}' updated sucessfully. ('{1}', {2}x{3}).", id, name, width, height), sender.CharacterId);

                            ServerToClientMessage wm = ServerToClientMessage.CreateMessageSafe();
                            MessageFormatter.CreateZoneSyncMessage(new Zone[] { zone }, wm);
                            context.ZoneManager.AddMessageToZone(id, wm);
                            ServerToClientMessage.FreeSafe(wm);
                        }
                        else
                            context.ChatProcessor.SendServerMessage("Failed to edit zone. Zone doesn't exist.", sender.CharacterId);
                    }
                }
                else if (command == "loadzone")
                {
                    if (args.Length > 0)
                    {
                        int id = Convert.ToInt32(args[0]);
                        if (!context.ZoneManager.ZoneExists(id))
                        {
                            Zone zone = context.Dal.LoadZone(id);
                            if (zone != null)
                            {
                                context.ZoneManager.AddZone(zone);
                                context.ChatProcessor.SendServerMessage(String.Format("Zone '{0}' loaded successfully.", zone.Name), sender.CharacterId);
                            }
                            else
                                context.ChatProcessor.SendServerMessage("Failed to load zone. Zone doesn't exist.", sender.CharacterId);
                        }
                        else
                            context.ChatProcessor.SendServerMessage("Failed to load zone. Zone is already loaded.", sender.CharacterId);
                    }
                }
                else if (command == "listzones")
                {
                    context.ChatProcessor.SendServerMessage("Currently loaded zones:", sender.CharacterId);
                    foreach (Zone z in context.ZoneManager.Zones)
                        context.ChatProcessor.SendServerMessage(String.Format("ID:{0}, Name: '{1}', Size: {2}x{3}, Entities: {4}({5}).", z.Id, z.Name, z.Width, z.Height, z.AllEntities.Count, z.Characters.Count), sender.CharacterId);
                }
                else if (command == "savezones")
                {
                    ThreadPool.QueueUserWorkItem(new WaitCallback(context.ZoneManager.SaveAllZones));
                    context.ChatProcessor.SendServerMessage("Started background zone save..", sender.CharacterId);
                }
                #endregion

                #region Entity Management
                if (command == "addentity")
                {
                    if (args.Length > 2)
                    {
                        int type = Convert.ToInt32(args[0]);
                        int x = Convert.ToInt32(args[1]);
                        int y = Convert.ToInt32(args[2]);
                        if (context.GameProcessor.AddEntity(type, sender.Zone, x, y, sender.CharacterId))
                            context.ChatProcessor.SendServerMessage("Entity added successfully.", sender.CharacterId);
                        else
                            context.ChatProcessor.SendServerMessage("Failed to add entity.", sender.CharacterId);
                    }
                }
                else if (command == "removeentity")
                {
                    if (args.Length > 0)
                    {
                        int id = Convert.ToInt32(args[0]);
                        if (context.GameProcessor.RemoveEntity(id))
                            context.ChatProcessor.SendServerMessage("Entity removed successfully.", sender.CharacterId);
                        else
                            context.ChatProcessor.SendServerMessage("Failed to remove entity.", sender.CharacterId);
                    }
                }
                else if (command == "listentities")
                {
                    Zone z = context.ZoneManager.GetZone(sender.Zone);
                    context.ChatProcessor.SendServerMessage("Current zone entities:", sender.CharacterId);
                    foreach (Entity e in z.AllEntities)
                    {
                        string entityInfo = string.Format("ID:{0}, Type:{1}, Name:{2}, Zone:{3}, Pos:{4}, Owner:{5}", e.Id, e.TypeId, e.Name, e.Zone, e.Position, e.Owner);
                        context.ChatProcessor.SendServerMessage(entityInfo, sender.CharacterId);
                    }
                }
                #endregion

                if (command == "teleport")
                {
                    if (args.Length > 2)
                    {
                        int zoneId = Convert.ToInt32(args[0]);
                        int x = Convert.ToInt32(args[1]);
                        int y = Convert.ToInt32(args[2]);
                        if (sender.Zone != zoneId && context.ZoneManager.ZoneExists(zoneId))
                        {
                            Zone newZone = context.ZoneManager.GetZone(zoneId);
                            context.GameProcessor.Teleport(sender, newZone, x, y);
                        }
                        else
                        {
                            //if (context.ZoneManager.LoadZone(zoneId)) { }
                            context.ChatProcessor.SendServerMessage("That zone doesn't exist, isn't loaded, or you're already in it.", sender.CharacterId);
                        }
                    }
                }

                if (command == "help")
                {
                    context.ChatProcessor.SendServerMessage("Zone Management - addzone, editzone, removezone, loadzone, listzones, savezones.", sender.CharacterId);
                    context.ChatProcessor.SendServerMessage("Entity Management - addentity, removeentity, listentities.", sender.CharacterId);
                    context.ChatProcessor.SendServerMessage("Other - teleport, help.", sender.CharacterId);
                }
            }
            catch (Exception e)
            {
                Logger.Output(this, "HandleCommand() exception: {0}, {1}", e.Message, e.StackTrace);
            }
        }

        /// <summary>
        /// Parses a string into arguments.
        /// Anything inside quotes is added as a single argument, quotes can also be escaped.
        /// </summary>
        public static string[] Parse(string message)
        {
            RegexOptions options = RegexOptions.None;
            Regex regex = new Regex(@"((""((?<token>.*?)(?<!\\)"")|(?<token>[\w]+))(\s)*)", options);
            var result = (from Match m in regex.Matches(message)
                          where m.Groups["token"].Success
                          select m.Groups["token"].Value).ToList();

            return result.ToArray();
        }
        #endregion
    }
}
