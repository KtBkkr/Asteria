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
    public class Fleet : Entity
    {
        #region Properties
        /// <summary>
        /// Gets or sets the target player of the fleet.
        /// </summary>
        public int TargetId { get; set; }

        /// <summary>
        /// Gets or sets the target zone of the fleet.
        /// </summary>
        public int TargetZone { get; set; }
        #endregion

        #region Constructors
        /// <summary>
        /// Creates a new Unit instance.
        /// </summary>
        public Fleet()
        {
            this.Name = "Fleet";
        }

        /// <summary>
        /// Creates a new Unit instance from serialized data.
        /// </summary>
        public Fleet(string data)
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
            SetAttribute("_targetId", TargetId);
            SetAttribute("_targetZone", TargetZone);

            base.PrepareData();
        }

        /// <summary>
        /// Sets unique fields values based on attributes/properties loaded from the db.
        /// </summary>
        public override void LoadData()
        {
            TargetId = (int)GetAttribute("_targetId");
            TargetZone = (int)GetAttribute("_targetZone");

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

            sb.Append(TargetId);
            sb.Append(":");
            sb.Append(TargetZone);
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
            TargetId = int.Parse(split[counter++]);
            TargetZone = int.Parse(split[counter++]);

            // Update consumed for children.
            consumedElements = counter;
        }
        #endregion
    }
}
