using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AsteriaLibrary.Entities
{
    /// <summary>
    /// Delegate signature for updating attributes.
    /// </summary>
    /// <param name="atr"></param>
    public delegate void EntityAttributeHandler(EntityAttribute atr);

    /// <summary>
    /// A character attribute is a float value that is attached to an entity.
    /// Examples for this are: health, mana, or character stats.
    /// </summary>
    public class EntityAttribute
    {
        #region DB Persistable Properties
        /// <summary>
        /// Returns the name of the attribute.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Returns the current value of the attribute.
        /// </summary>
        public float Value { get; set; }
        #endregion

        #region WSE Specific Properties
        /// <summary>
        /// Returns the maximum value of the attribute.
        /// </summary>
        public int MaxValue { get; set; }

        /// <summary>
        /// Returns the description of the attribute.
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// Returns the tag of the attribute.
        /// </summary>
        public object Tag { get; set; }

        public EntityAttributeHandler Handler { get; set; }
        #endregion

        #region Constructors
        /// <summary>
        /// Creates a new EntityAttribute instance.
        /// Note that the name is always stored in lowercase.
        /// </summary>
        /// <param name="name"></param>
        /// <param name="description"></param>
        /// <param name="value"></param>
        public EntityAttribute(string name, string description, float value)
        {
            this.Name = name;
            this.Description = description;
            this.Value = value;
        }

        /// <summary>
        /// Creates a new EntityAttribute instance.
        /// </summary>
        /// <param name="name"></param>
        /// <param name="description"></param>
        public EntityAttribute(string name, string description)
        {
            this.Name = name;
            this.Description = description;
        }

        /// <summary>
        /// Creates a new EntityAttribute instance.
        /// </summary>
        /// <param name="name"></param>
        /// <param name="value"></param>
        public EntityAttribute(string name, float value)
        {
            this.Name = name;
            this.Value = value;
        }
        #endregion

        #region Methods
        public override string ToString()
        {
            return Name + "-" + Value.ToString();
        }
        #endregion
    }
}
