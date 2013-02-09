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
        private DateTime created;
        private string name;
        private int zone;
        private Point position;
        private int owner;

        protected Dictionary<string, EntityAttribute> attributes;
        protected Dictionary<string, EntityProperty> properties;

        /// <summary>
        /// For internal asteria framework infrastructure support only, do not use!
        /// </summary>
        public DateTime LastSaved = DateTime.MinValue;

        /// <summary>
        /// For internal asteria framework infrastructure support only, do not use!
        /// </summary>
        public object Tag;
        #endregion

        #region Properties
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
        /// Entity creation time.
        /// </summary>
        public DateTime Created
        {
            get { return created; }
            set { created = value; }
        }

        /// <summary>
        /// Gets or sets the entities name.
        /// </summary>
        public string Name
        {
            get { return name; }
            set { name = value; }
        }

        /// <summary>
        /// Gets or sets the current zone.
        /// </summary>
        public int Zone
        {
            get { return zone; }
            set { zone = value; }
        }

        /// <summary>
        /// Gets or sets the entities position within the zone.
        /// </summary>
        public Point Position
        {
            get { return position; }
            set { position = value; }
        }

        public int Owner
        {
            get { return owner; }
            set { owner = value; }
        }

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
        /// Returns an entity attribute value.
        /// </summary>
        public float GetAttribute(string name)
        {
            if (attributes.ContainsKey(name))
                return attributes[name].Value;
#if DEBUG
            Logger.Output(this, "Attribute value not found: {0}", name);
#endif
            return -1;
        }

        /// <summary>
        /// Sets an entity Attribute
        /// </summary>
        public void SetAttribute(string name, float value)
        {
            if (attributes.ContainsKey(name))
                attributes[name].Value = value;
            else
                attributes.Add(name, new EntityAttribute(name, name, value));
        }

        /// <summary>
        /// Sets an entity attribute.
        /// </summary>
        public void SetAttribute(string name, float value, string description)
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
        /// Returns an entity property value.
        /// </summary>
        public string GetProperty(string name)
        {
            if (properties.ContainsKey(name))
                return properties[name].Value;
#if DEBUG
            Logger.Output(this, "Property value not found: {0}", name);
#endif
            return "";
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

        #region Database
        /// <summary>
        /// Copies all entity specific fields into attributes/properties.
        /// </summary>
        public virtual void PrepareData()
        {
        }

        /// <summary>
        /// Reads all special attributes/properties and sets the appropriate fields values.
        /// </summary>
        public virtual void LoadData()
        {
        }
        #endregion

        #region IStringFormattable Members
        /// <summary>
        /// Generic entity format for sending to clients.
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
            sb.Append(Zone);
            sb.Append(":");
            sb.Append(Position.ToString());
            sb.Append(":");
            sb.Append(Owner);
            sb.Append(":");

            int attribCount = 0;
            string attribString = "";
            foreach (string key in attributes.Keys)
            {
                if (key.StartsWith("_"))
                    continue;

                attribCount++;
                attribString += String.Format("{0},{1}:", key, attributes[key].Value);
            }
            sb.Append(attribCount);
            sb.Append(":");
            sb.Append(attribString);

            attribCount = 0;
            attribString = "";
            foreach (string key in properties.Keys)
            {
                if (key.StartsWith("_"))
                    continue;

                attribCount++;
                attribString += String.Format("{0},{1}:", key, properties[key].Value);
            }
            sb.Append(attribCount);
            sb.Append(":");
            sb.Append(attribString);

            return sb.ToString();
        }

        /// <summary>
        /// Parses the data parameter and populates the entity properties/attributes with data elements.
        /// </summary>
        public virtual void FromFormatString(string data)
        {
            // NOTE: this absolutely must be kept in sync with ToFormatString()
            int temp;
            FromFormatString(data, out temp);
        }

        /// <summary>
        /// Sets unique fields based on data in a format string.
        /// </summary>
        protected virtual void FromFormatString(string data, out int consumedElements)
        {
            // NOTE: this absolutely must be kept in sync with ToFormatString()
            string[] elements = data.Split(':');
            int counter = 0;

            Id = int.Parse(elements[counter++]);
            typeId = int.Parse(elements[counter++]);
            Name = elements[counter++];
            Zone = int.Parse(elements[counter++]);
            Position = (Point)elements[counter++];
            Owner = int.Parse(elements[counter++]);

            int attributeCount = int.Parse(elements[counter++]);
            for (int i = 0; i < attributeCount; i++)
            {
                int comma = elements[counter].IndexOf(',');
                string attributeName = elements[counter].Substring(0, comma);
                string attributeValue = elements[counter].Substring(comma + 1);
                EntityAttribute ea = new EntityAttribute(attributeName, (float)double.Parse(attributeValue));
                Attributes.Add(attributeName, ea);
                counter++;
            }

            int propertyCount = int.Parse(elements[counter++]);
            for (int i = 0; i < propertyCount; i++)
            {
                int comma = elements[counter].IndexOf(',');
                string propertyName = elements[counter].Substring(0, comma);
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
