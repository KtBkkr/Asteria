using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AsteriaLibrary.Shared;

namespace AsteriaLoginServer
{
    class Launcher
    {
        #region Fields
        private Logger logger = new Logger(Config.LogFile);
        #endregion

        #region Constructors
        public Launcher()
        {
            Logger.Output(this, "Creating login server..");
            LoginServer loginServer = new LoginServer();

            if (loginServer.Start())
            {
                string command;
                while (!loginServer.IsClosing)
                {
                    command = Console.ReadLine();
                    loginServer.HandleConsoleCommand(command);
                }

                loginServer.Stop();
                loginServer.WriteStatistics();
            }
            else
                Logger.Output(this, "Login server couldn't be started!");

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
