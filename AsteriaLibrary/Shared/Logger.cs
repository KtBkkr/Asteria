using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace AsteriaLibrary.Shared
{
    public delegate void LoggerMsgEvent(string message);

    /// <summary>
    /// Simple logging class.
    /// </summary>
    public class Logger
    {
        #region Fields
        public static event LoggerMsgEvent MessageReceived;
        private static string fileName = "Logger.log";
        private static object locker = new object();
        #endregion

        #region Constructors
        public Logger(string fileName)
        {
            Logger.fileName = fileName;
        }
        #endregion

        #region Methods
        /// <summary>
        /// Records the given message to the log file.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="format"></param>
        /// <param name="args"></param>
        public static void Record(object sender, string format, params object[] args)
        {
            string message = String.Format(format, args);
            Record(sender, message);
        }

        /// <summary>
        /// Records the given message to the log file.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="message"></param>
        public static void Record(object sender, string message)
        {
            string entry = String.Format("[{0} - {1}] {2}", DateTime.Now, sender.GetType().Name, message);
            lock (locker)
            {
                using (StreamWriter w = new StreamWriter(fileName, true))
                    w.WriteLine(entry);
            }

            if (MessageReceived != null)
                MessageReceived(entry);
        }

        /// <summary>
        /// Outputs the given message to the console and records it to the log file.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="format"></param>
        /// <param name="args"></param>
        public static void Output(object sender, string format, params object[] args)
        {
            string message = String.Format(format, args);
            Output(sender, message);
        }

        /// <summary>
        /// Outputs the given message to the console and records it to the log file.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="message"></param>
        public static void Output(object sender, string message)
        {
            string entry = String.Format("[{0} - {1}] {2}", DateTime.Now.ToLongTimeString(), sender.GetType().Name, message);
            Console.WriteLine(entry);
            Record(sender, message);
        }

        /// <summary>
        /// Outputs the given message to the console.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="format"></param>
        /// <param name="args"></param>
        public static void Display(object sender, string format, params object[] args)
        {
            string message = String.Format(format, args);
            Display(sender, message);
        }

        /// <summary>
        /// Outputs the given message to the console.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="message"></param>
        public static void Display(object sender, string message)
        {
            string entry = String.Format("[{0} - {1}] {2}", DateTime.Now.ToLongTimeString(), sender.GetType().Name, message);
            Console.WriteLine(entry);

            if (MessageReceived != null)
                MessageReceived(entry);
        }
        #endregion
    }
}
