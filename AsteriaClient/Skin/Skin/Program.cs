using System;

namespace Skin
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        static void Main(string[] args)
        {
            try
            {
                System.Diagnostics.Process proc = new System.Diagnostics.Process();
                proc.StartInfo.FileName = "BuildSkin.bat";
                proc.Start();
            }
            catch
            {
            }
        }
    }
}

