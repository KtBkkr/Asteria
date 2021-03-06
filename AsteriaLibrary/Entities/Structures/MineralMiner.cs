﻿using System;
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
    public class MineralMiner : Structure
    {
        #region Variables
        private List<Asteroid> targets;
        #endregion

        #region Properties
        /// <summary>
        /// Gets or sets a list of asteroids the miner will mine from.
        /// </summary>
        public List<Asteroid> Targets
        {
            get { return targets; }
            set { targets = value; }
        }
        #endregion

        #region Constructors
        /// <summary>
        /// Creates a new Unit instance.
        /// </summary>
        public MineralMiner()
        {
            this.Name = "Mineral Miner";
            this.targets = new List<Asteroid>();
        }

        /// <summary>
        /// Creates a new Unit instance from serialized data.
        /// </summary>
        public MineralMiner(string data)
        {
            FromFormatString(data);
            this.targets = new List<Asteroid>();
        }
        #endregion

        #region Methods
        /// <summary>
        /// Prepares any unique fields to be entered into the db as attributes/properties.
        /// </summary>
        public override void PrepareData()
        {
            base.PrepareData();
        }

        /// <summary>
        /// Sets unique fields values based on attributes/properties loaded from the db.
        /// </summary>
        public override void LoadData()
        {
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


            // Update consumed for children.
            consumedElements = counter;
        }
        #endregion
    }
}
