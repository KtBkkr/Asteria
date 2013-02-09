using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AsteriaLibrary.Math;
using Lidgren.Network;
using AsteriaLibrary.Messages;
using AsteriaLibrary.Shared;

namespace AsteriaLibrary.Entities
{
    /// <summary>
    /// Represents an in game entity owned by the player.
    /// This class holds only game related information.
    /// </summary>
    public class Unit : Entity
    {
        #region Properties
        /// <summary>
        /// Gets or sets total amount of health the unit has.
        /// </summary>
        public int TotalHealth { get; set; }

        /// <summary>
        /// Gets or sets current amount of health the unit has.
        /// </summary>
        public int CurrentHealth { get; set; }

        /// <summary>
        /// Gets or sets the amount of damage reduction the unit has.
        /// TODO: [LOW] implement different damage types and shield types. (ie. kinetic, thermal)
        /// </summary>
        public int Shield { get; set; }

        /// <summary>
        /// Gets or sets the range this unit can reach.
        /// </summary>
        public int Range { get; set; }

        /// <summary>
        /// Gets or sets the amount of damage (or healing) the unit performs.
        /// </summary>
        public int Damage { get; set; }

        /// <summary>
        /// Gets or sets the rate (in seconds) that the unit performs at.
        /// </summary>
        public float Rate { get; set; }

        /// <summary>
        /// Gets or sets the progress (in seconds) before it reaches rate.
        /// </summary>
        public float Timer { get; set; }

        /// <summary>
        /// Gets or sets the total cost of the unit.
        /// </summary>
        public int Cost { get; set; }
        #endregion

        #region Constructors
        /// <summary>
        /// Creates a new Unit instance.
        /// </summary>
        public Unit()
        {
            this.Name = "Unit";
        }

        /// <summary>
        /// Creates a new Unit instance from serialized data.
        /// </summary>
        /// <param name="data"></param>
        public Unit(string data)
        {
            FromFormatString(data);
        }
        #endregion

        #region Methods
        public override void PrepareData()
        {
            SetAttribute("_totalHealth", TotalHealth);
            SetAttribute("_currentHealth", CurrentHealth);
            SetAttribute("_shield", Shield);
            SetAttribute("_range", Range);
            SetAttribute("_damage", Damage);
            SetAttribute("_rate", Rate);
            SetAttribute("_timer", Timer);
            SetAttribute("_cost", Cost);

            base.PrepareData();
        }

        public override void LoadData()
        {
            TotalHealth = (int)GetAttribute("_totalHealth");
            CurrentHealth = (int)GetAttribute("_currentHealth");
            Shield = (int)GetAttribute("_shield");
            Range = (int)GetAttribute("_range");
            Damage = (int)GetAttribute("_damage");
            Rate = GetAttribute("_rate");
            Timer = GetAttribute("_timer");
            Cost = (int)GetAttribute("_cost");

            base.LoadData();
        }

        public override string ToFormatString()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append(base.ToFormatString());

            //  We must append instead of prepending data, 
            //  else we could not extract an Entity instance from a character string - which must be possible!
            sb.Append(TotalHealth);
            sb.Append(":");
            sb.Append(CurrentHealth);
            sb.Append(":");
            sb.Append(Shield);
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

            // Parse elements from Entity class.
            base.FromFormatString(data, out counter);

            // Parse our own.
            TotalHealth = int.Parse(split[counter++]);
            CurrentHealth = int.Parse(split[counter++]);
            Shield = int.Parse(split[counter++]);

            // Update consumed for children.
            consumedElements = counter;
        }
        #endregion
    }
}
