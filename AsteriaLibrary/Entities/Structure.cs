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
    public class Structure : Unit
    {
        #region Variables
        #endregion

        #region Properties
        /// <summary>
        /// Gets or sets the energy this unit takes on each cycle.
        /// </summary>
        public int EnergyCost { get; set; }
        #endregion

        #region Constructors
        /// <summary>
        /// Creates a new Unit instance.
        /// </summary>
        public Structure()
        {
            this.Name = "Structure";
        }

        /// <summary>
        /// Creates a new Unit instance from serialized data.
        /// </summary>
        public Structure(string data)
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
            SetAttribute("_energyCost", EnergyCost);

            base.PrepareData();
        }

        /// <summary>
        /// Sets unique fields values based on attributes/properties loaded from the db.
        /// </summary>
        public override void LoadData()
        {
            EnergyCost = (int)GetAttribute("_energyCost");

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
