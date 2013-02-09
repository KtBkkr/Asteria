using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AsteriaLibrary.Entities
{
    /// <summary>
    /// Delegate signature for updating properties.
    /// </summary>
    /// <param name="atr"></param>
    public delegate void EntityPropertyHandler(EntityProperty atr);

    /// <summary>
    /// A character property is a string value that is attached to an entity.
    /// Examples for this are: equipment on a character, a class name, or skills.
    /// </summary>
    public class EntityProperty
    {
        #region DB Persistant Properties
        /// <summary>
        /// Returns the name of the property.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Returns the current value of the property.
        /// </summary>
        public string Value { get; set; }
        #endregion

        #region WSE Specific Properties
        /// <summary>
        /// Returns the description of the attribute.
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// Returns the tag of the attribute.
        /// </summary>
        public object Tag { get; set; }

        public EntityPropertyHandler Handler { get; set; }
        #endregion

        #region Constructors
        /// <summary>
        /// Creates a new EntityProperty instance.
        /// Note that the name is always stored in lowercase.
        /// </summary>
        /// <param name="name"></param>
        /// <param name="description"></param>
        /// <param name="value"></param>
        public EntityProperty(string name, string description, string value)
        {
            this.Name = name;
            this.Description = description;
            this.Value = value;
        }

        /// <summary>
        /// Creates a new EntityProperty instance.
        /// </summary>
        /// <param name="name"></param>
        /// <param name="description"></param>
        public EntityProperty(string name, string description)
        {
            this.Name = name;
            this.Description = description;
        }
        #endregion
    }
}
