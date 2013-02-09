using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AsteriaLibrary.Math;
using AsteriaLibrary.Messages;
using AsteriaLibrary.Shared;
using Lidgren.Network;

namespace AsteriaLibrary.Entities
{
    /// <summary>
    /// A mining laser unit used to harvest minerals from asteroids.
    /// </summary>
    public class EnergyRelay : Structure
    {
        #region Fields
        #endregion

        #region Properties
        /// <summary>
        /// Gets or sets the max amount of connections this relay can hold.
        /// </summary>
        public int MaxConnections { get; set; }

        /// <summary>
        /// Gets or sets the list of connections this relay has to other units.
        /// </summary>
        public List<int> Connections { get; set; }
        #endregion

        #region Constructors
        /// <summary>
        /// Creates a new Unit instance.
        /// </summary>
        public EnergyRelay()
        {
            this.Name = "Energy Relay";
            this.Connections = new List<int>();
        }

        /// <summary>
        /// Creates a new Unit instance from serialized data.
        /// </summary>
        public EnergyRelay(string data)
        {
            this.Connections = new List<int>();
            FromFormatString(data);
        }
        #endregion

        #region Methods
        /// <summary>
        /// Prepares any unique fields to be entered into the db as attributes/properties.
        /// </summary>
        public override void PrepareData()
        {
            SetAttribute("_maxConnections", MaxConnections);

            if (Connections.Count > 0)
            {
                string connects = "";
                foreach (int value in Connections)
                    connects += value.ToString() + ",";
                SetProperty("_connections", connects.Trim(':'));
            }

            base.PrepareData();
        }

        /// <summary>
        /// Sets unique fields values based on attributes/properties loaded from the db.
        /// </summary>
        public override void LoadData()
        {
            MaxConnections = (int)GetAttribute("_maxConnections");

            string connectString = GetProperty("_connections");
            if (!string.IsNullOrWhiteSpace(connectString))
            {
                string[] connects = connectString.Split(',');
                foreach (string c in connects)
                {
                    if (!string.IsNullOrEmpty(c))
                        Connections.Add(int.Parse(c));
                }
            }

            base.LoadData();
        }

        /// <summary>
        /// Creates a format string used to send an entity over the network.
        /// </summary>
        /// <returns></returns>
        public override string ToFormatString()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append(base.ToFormatString());

            sb.Append(MaxConnections);
            sb.Append(":");

            if (Connections.Count > 0)
            {
                string connectString = "";
                foreach (int c in Connections)
                    connectString += c.ToString() + ",";

                sb.Append(connectString.Trim(','));
                sb.Append(":");
            }

            return sb.ToString();
        }

        /// <summary>
        /// Sets unique fields based on data in a format string.
        /// </summary>
        public override void FromFormatString(string data)
        {
            int temp;
            FromFormatString(data, out temp);
        }

        /// <summary>
        /// Sets unique fields based on data in a format string.
        /// </summary>
        protected override void FromFormatString(string data, out int consumedElements)
        {
            string[] split = data.Split(':');
            int counter;

            // Parse elements from base class.
            base.FromFormatString(data, out counter);

            // Parse our own.
            MaxConnections = int.Parse(split[counter++]);

            string connectString = split[counter++];
            if (!string.IsNullOrWhiteSpace(connectString))
            {
                string[] connects = connectString.Split(',');
                foreach (string c in connects)
                    Connections.Add(int.Parse(c));
            }

            // Update consumed for children.
            consumedElements = counter;
        }
        #endregion
    }
}
