using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;

namespace AsteriaLibrary.Shared
{
    public class WorldServerInfo
    {
        #region Fields
        private int id;
        private string name;
        private DateTime isOnline;
        private IPEndPoint clientAddress;
        private IPEndPoint interAddress;
        private int online;
        private int allowed;
        #endregion

        #region Properties
        public int Id
        {
            get { return id; }
            set { id = value; }
        }

        public string Name
        {
            get { return name; }
            set { name = value; }
        }

        public DateTime IsOnline
        {
            get { return isOnline; }
            set { isOnline = value; }
        }

        public IPEndPoint ClientAddress
        {
            get { return clientAddress; }
            set { clientAddress = value; }
        }

        public IPEndPoint InterAddress
        {
            get { return interAddress; }
            set { interAddress = value; }
        }

        public int Online
        {
            get { return online; }
            set { online = value; }
        }

        public int Allowed
        {
            get { return allowed; }
            set { allowed = value; }
        }
        #endregion

        #region Constructors
        public WorldServerInfo(int id, string name, DateTime isOnline, IPEndPoint clientAddress, IPEndPoint interAddress, int online, int allowed)
        {
            this.id = id;
            this.name = name;
            this.isOnline = isOnline;
            this.clientAddress = clientAddress;
            this.interAddress = interAddress;
            this.online = online;
            this.allowed = allowed;
        }
        #endregion

        #region Methods
        /// <summary>
        /// Creates world info string with Inter address
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append(id);
            sb.Append('|');
            sb.Append(name);
            sb.Append('|');
            sb.Append(isOnline);
            sb.Append('|');
            sb.Append(clientAddress.ToString());
            sb.Append('|');
            sb.Append(interAddress.ToString());
            sb.Append('|');
            sb.Append(online);
            sb.Append('|');
            sb.Append(allowed);
            sb.Append('|');
            sb.Append(";");
            return sb.ToString();
        }

        /// <summary>
        /// Creats world info string without Inter address.
        /// </summary>
        /// <returns></returns>
        public string ToClientString()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append(id);
            sb.Append('|');
            sb.Append(name);
            sb.Append('|');
            sb.Append(isOnline);
            sb.Append('|');
            sb.Append(clientAddress.ToString());
            sb.Append('|');
            sb.Append(online);
            sb.Append('|');
            sb.Append(allowed);
            sb.Append('|');
            sb.Append(";");
            return sb.ToString();
        }

        public static implicit operator WorldServerInfo(string s)
        {
            string[] components = s.Split('|');
            int wsiId = int.Parse(components[0]);
            string wsiName = components[1];
            DateTime wsiIsOnline = DateTime.Parse(components[2]);

            // For client address
            int i = components[3].IndexOf(":");
            string clientAddress = components[3].Substring(0, i);
            string clientPort = components[3].Substring(i + 1);
            IPEndPoint wsiClientAddress = new IPEndPoint(IPAddress.Parse(clientAddress), int.Parse(clientPort));

            // For inter address
            //i = components[4].IndexOf(":");
            //string interAddress = components[4].Substring(0, i);
            //string interPort = components[4].Substring(i + 1);
            //IPEndPoint wsiInterAddress = new IPEndPoint(IPAddress.Parse(interAddress), int.Parse(interPort));

            int online = int.Parse(components[4]);
            int allowed = int.Parse(components[5]);

            //WorldServerInfo wsi = new WorldServerInfo(wsiId, wsiName, wsiIsOnline, wsiClientAddress, wsiInterAddress, online, allowed);
            WorldServerInfo wsi = new WorldServerInfo(wsiId, wsiName, wsiIsOnline, wsiClientAddress, wsiClientAddress, online, allowed);
            return wsi;
        }
        #endregion
    }
}
