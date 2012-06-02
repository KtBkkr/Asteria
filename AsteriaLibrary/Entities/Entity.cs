using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AsteriaLibrary.Shared;
using AsteriaLibrary.Math;

namespace AsteriaLibrary.Entities
{
    /// <summary>
    /// Defines all objects that can be placed in a world.
    /// </summary>
    public class Entity : IStringFormattable
    {
        #region Fields
        private int id;
        private int typeId;
        private Point position;

        protected Dictionary<string, EntityAttribute> attributes;
        protected Dictionary<string, EntityProperty> properties;
        #endregion

        #region Properties
        /// <summary>
        /// Gets or sets the entities name.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Gets the entities unique and global ID.
        /// </summary>
        public int Id
        {
            get { return id; }
            set { id = value; }
        }

        /// <summary>
        /// Gets the entities type ID (visual look, e.g. plant, monster, player)
        /// </summary>
        public int TypeId
        {
            get { return typeId; }
            set { typeId = value; }
        }

        /// <summary>
        /// Gets or sets the entities position.
        /// </summary>
        public Point Position
        {
            get { return position; }
            set { position = value; }
        }

        /// <summary>
        /// Gets or sets the gold amount.
        /// </summary>
        public uint Gold { get; set; }

        /// <summary>
        /// Gets or sets the current zone.
        /// </summary>
        public int CurrentZone { get; set; }

        /// <summary>
        /// Gets or sets the Zone this entity was during the last turn.
        /// </summary>
        public int LastZone { get; set; }

        /// <summary>
        /// This holds a reference to a scene node or similar data.
        /// </summary>
        public object Tag { get; set; }

        /// <summary>
        /// Returns a list of attributes.
        /// </summary>
        public Dictionary<string, EntityAttribute> Attributes
        {
            get { return attributes; }
        }

        /// <summary>
        /// Returns a list of properties.
        /// </summary>
        public Dictionary<string, EntityProperty> Properties
        {
            get { return properties; }
        }
        #endregion

        #region Constructors
        /// <summary>
        /// Creates a new Entity instance.
        /// </summary>
        public Entity()
        {
            attributes = new Dictionary<string, EntityAttribute>();
            properties = new Dictionary<string, EntityProperty>();
        }

        /// <summary>
        /// Creates a new Entity instance.
        /// </summary>
        /// <param name="id"></param>
        /// <param name="typeId"></param>
        /// <param name="name"></param>
        public Entity(int id, int typeId, string name)
        {
            this.id = id;
            this.typeId = typeId;
            this.Name = name;

            attributes = new Dictionary<string, EntityAttribute>();
            properties = new Dictionary<string, EntityProperty>();
        }

        /// <summary>
        /// Creates a new Entity instance.
        /// </summary>
        /// <param name="data"></param>
        public Entity(string data)
        {
            attributes = new Dictionary<string, EntityAttribute>();
            properties = new Dictionary<string, EntityProperty>();

            FromFormatString(data);
        }
        #endregion

        #region Methods
        /// <summary>
        /// Gets the current position.
        /// </summary>
        /// <param name="position"></param>
        public void GetPosition(out Point position)
        {
            position = this.position;
        }

        /// <summary>
        /// Returns an entity attribute.
        /// </summary>
        public EntityAttribute GetAttribute(string name)
        {
            if (attributes.ContainsKey(name))
                return attributes[name];

            return null;
        }

        /// <summary>
        /// Returns an entity attribute value.
        /// </summary>
        public int GetAttributeValue(string name)
        {
            if (attributes.ContainsKey(name))
                return attributes[name].Value;
#if DEBUG
            throw (new Exception("Attribute value not found: " + name));
#else
            return -1;
#endif
        }

        /// <summary>
        /// Sets an entity Attribute
        /// </summary>
        public void SetAttribute(string name, int value)
        {
            if (attributes.ContainsKey(name))
                attributes[name].Value = value;
            else
                attributes.Add(name, new EntityAttribute(name, name, value));
        }

        /// <summary>
        /// Sets an entity attribute.
        /// </summary>
        public void SetAttribute(string name, int value, string description)
        {
            if (attributes.ContainsKey(name))
            {
                attributes[name].Value = value;
                attributes[name].Description = description;
            }
            else
                attributes.Add(name, new EntityAttribute(name, description, value));
        }

        /// <summary>
        /// Returns an entity property.
        /// </summary>
        public EntityProperty GetProperty(string name)
        {
            if(properties.ContainsKey(name))
                return properties[name];

            return null;
        }

        /// <summary>
        /// Returns an entity property value.
        /// </summary>
        public string GetPropertyValue(string name)
        {
            if (properties.ContainsKey(name))
                return properties[name].Value;
#if DEBUG
            throw (new Exception("Property value not found: " + name));
#else
            return "";
#endif
        }

        /// <summary>
        /// Sets an entity property.
        /// </summary>
        public void SetProperty(string name, string value)
        {
            if (properties.ContainsKey(name))
                properties[name].Value = value;
            else
                properties.Add(name, new EntityProperty(name, name, value));
        }

        /// <summary>
        /// Sets an entity property.
        /// </summary>
        public void SetProperty(string name, string value, string description)
        {
            if (properties.ContainsKey(name))
            {
                properties[name].Value = value;
                properties[name].Description = description;
            }
            else
                properties.Add(name, new EntityProperty(name, description, value));
        }

        /// <summary>
        /// Returns an unknown character value.
        /// </summary>
        public object GetCharacterValue(string name)
        {
            if (attributes.ContainsKey(name))
                return attributes[name].Value;

            if (properties.ContainsKey(name))
                return properties[name].Value;
#if DEBUG
            throw (new Exception("Character value not found: " + name));
#else
            return null;
#endif
        }

        public void SetCharacterValue(string name, object value, string description)
        {
            if (value.GetType() == typeof(int))
                SetAttribute(name, (int)value, description);
            else if (value.GetType() == typeof(string))
                SetProperty(name, (string)value, description);
#if DEBUG
            throw (new Exception("Character value is neither int nor string: " + name));
#endif
        }

        #region IStringFormattable Members
        /// <summary>
        /// Generic eneity format for sending to clients.
        /// </summary>
        /// <returns></returns>
        public virtual string ToFormatString()
        {
            // NOTE: this absolutely must be kept in sync with static FromLongString()
            StringBuilder sb = new StringBuilder();
            sb.Append(Id);
            sb.Append(":");
            sb.Append(TypeId);
            sb.Append(":");
            sb.Append(Name);
            sb.Append(":");
            sb.Append(CurrentZone);
            sb.Append(":");
            sb.Append(Position.ToString());
            sb.Append(":");

            sb.Append(Attributes.Count);
            sb.Append(":");

            foreach (KeyValuePair<string, EntityAttribute> pair in Attributes)
            {
                sb.Append(pair.Key);
                sb.Append(",");
                sb.Append(pair.Value.Value);
                sb.Append(":");
            }

            sb.Append(Properties.Count);
            sb.Append(":");

            foreach (KeyValuePair<string, EntityProperty> pair in Properties)
            {
                sb.Append(pair.Key);
                sb.Append(",");
                sb.Append(pair.Value.Value);
                sb.Append(":");
            }
            return sb.ToString();
        }

        /// <summary>
        /// Parses the data parameter and populates the entity properties/attributes with data elements.
        /// </summary>
        /// <param name="data"></param>
        public virtual void FromFormatString(string data)
        {
            // NOTE: this absolutely must be kept in sync with ToFormatString()
            int temp;
            FromFormatString(data, out temp);
        }

        protected void FromFormatString(string data, out int consumedElements)
        {
            // NOTE: this absolutely must be kept in sync with ToFormatString()
            string[] elements = data.Split(':');
            int counter = 0;

            id = int.Parse(elements[counter++]);
            typeId = int.Parse(elements[counter++]);
            Name = elements[counter++];
            CurrentZone = int.Parse(elements[counter++]);
            position = (Point)elements[counter++];

            int attributeCount = int.Parse(elements[counter++]);

            for (int i = 0; i < attributeCount; i++)
            {
                int comma = elements[counter].IndexOf(',');
                string attributeName = elements[counter].Substring(0, comma).ToLowerInvariant();
                string attributeValue = elements[counter].Substring(comma + 1);
                EntityAttribute ea = new EntityAttribute(attributeName, int.Parse(attributeValue));
                Attributes.Add(attributeName, ea);
                counter++;
            }

            int propertyCount = int.Parse(elements[counter++]);

            for (int i = 0; i < propertyCount; i++)
            {
                int comma = elements[counter].IndexOf(',');
                string propertyName = elements[counter].Substring(0, comma).ToLowerInvariant();
                string propertyValue = elements[counter].Substring(comma + 1);
                EntityProperty ep = new EntityProperty(propertyName, null, propertyValue);
                Properties.Add(propertyName, ep);
                counter++;
            }
            consumedElements = counter;
        }
        #endregion

        #endregion
    }
}
