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
    /// Represents an asteroid in the game world which minerals are mined from.
    /// </summary>
    public class Asteroid : Entity
    {
        #region Properties
        /// <summary>
        /// Gets or sets the total amount of minerals this asteroid can hold.
        /// </summary>
        public int TotalMinerals { get; set; }

        /// <summary>
        /// Gets or sets the current amount of minerals.
        /// </summary>
        public int CurrentMinerals { get; set; }
        #endregion

        #region Constructors
        /// <summary>
        /// Creates a new Unit instance.
        /// </summary>
        public Asteroid()
        {
            this.Name = "Asteroid";
        }

        /// <summary>
        /// Creates a new Unit instance from serialized data.
        /// </summary>
        public Asteroid(string data)
        {
            FromFormatString(data);
        }
        #endregion

        #region Methods
        /// <summary>
        /// Prepares any unique fields to be entered into the db as attributes/properties.
        /// </summary>
        public override void PrepareData()
        {
            SetAttribute("_totalMinerals", TotalMinerals);
            SetAttribute("_currentMinerals", CurrentMinerals);

            base.PrepareData();
        }

        /// <summary>
        /// Sets unique fields values based on attributes/properties loaded from the db.
        /// </summary>
        public override void LoadData()
        {
            TotalMinerals = (int)GetAttribute("_totalMinerals");
            CurrentMinerals = (int)GetAttribute("_currentMinerals");

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

            sb.Append(TotalMinerals);
            sb.Append(":");
            sb.Append(CurrentMinerals);
            sb.Append(":");

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
            TotalMinerals = int.Parse(split[counter++]);
            CurrentMinerals = int.Parse(split[counter++]);

            // Update consumed for children.
            consumedElements = counter;
        }
        #endregion
    }
}
