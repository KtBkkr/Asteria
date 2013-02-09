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
    /// A slow firing, mid damage, mid range laser turret.
    /// </summary>
    public class MissileLauncher : Structure
    {
        #region Properties
        /// <summary>
        /// Gets or sets the amount of missiles fired each cycle.
        /// </summary>
        public int MissileCount { get; set; }

        /// <summary>
        /// Gets or sets the speed the missiles fly at.
        /// </summary>
        public int MissileSpeed { get; set; }

        /// <summary>
        /// Gets or sets the mineral cost of each missile.
        /// </summary>
        public int MissileCost { get; set; }
        #endregion

        #region Constructors
        /// <summary>
        /// Creates a new Unit instance.
        /// </summary>
        public MissileLauncher()
        {
            this.Name = "Missile Launcher";
        }

        /// <summary>
        /// Creates a new Unit instance from serialized data.
        /// </summary>
        public MissileLauncher(string data)
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
            SetAttribute("_missileCount", MissileCount);
            SetAttribute("_missileSpeed", MissileSpeed);
            SetAttribute("_missileCost", MissileCost);

            base.PrepareData();
        }

        /// <summary>
        /// Sets unique fields values based on attributes/properties loaded from the db.
        /// </summary>
        public override void LoadData()
        {
            MissileCount = (int)GetAttribute("_missileCount");
            MissileSpeed = (int)GetAttribute("_missileSpeed");
            MissileCost = (int)GetAttribute("_missileCost");

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

            sb.Append(MissileCount);
            sb.Append(":");
            sb.Append(MissileSpeed);
            sb.Append(":");
            sb.Append(MissileCost);
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
            MissileCount = int.Parse(split[counter++]);
            MissileSpeed = int.Parse(split[counter++]);
            MissileCost = int.Parse(split[counter++]);

            // Update consumed for children.
            consumedElements = counter;
        }
        #endregion
    }
}
