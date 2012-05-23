using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AsteriaLibrary.Shared;

namespace AsteriaWorldServer
{
    class Program
    {
        class Launcher
        {
            #region Fields
            private Logger logger = new Logger(Config.LogFile);
            #endregion

            #region Constructors
            public Launcher()
            {
                Logger.Output(this, "Creating world server..");
                WorldServer worldServer = new WorldServer();

                if (worldServer.Start())
                {
                    string command;
                    while (!worldServer.IsClosing)
                    {
                        command = Console.ReadLine();
                        worldServer.HandleConsoleCommand(command);
                    }

                    worldServer.PrepareShutdown();
                    worldServer.Shutdown();
                    worldServer.WriteStatistics();
                }
                else
                    Logger.Output(this, "World server couldn't be started!");

                Console.WriteLine("\r\nPress ENTER to close window.");
                Console.ReadLine();
            }
            #endregion

            #region Entry Point
            static void Main(string[] args)
            {
                Launcher launcher = new Launcher();
            }
            #endregion
        }
    }
}
