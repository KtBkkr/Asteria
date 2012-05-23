using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AsteriaLibrary.Math;

namespace AsteriaWorldServer.Entities
{
    /// <summary>
    /// Data holder for class related information.
    /// </summary>
    public class EntityClassData
    {
        public int TypeId;
        public string Name;
        public string Sex;
        public string Description;
        public string Race;

        /// <summary>
        /// The inventory with and height, exists only for characters.
        /// </summary>
        public Size InventorySize;

        /// <summary>
        /// Inventory slots consumption.
        /// </summary>
        public Size SlotSize;

        /// <summary>
        /// Number of items allowed to coexist at same inventory location.
        /// </summary>
        public int SlotStacks;

        public Dictionary<string, int> DefaultAttributes = new Dictionary<string, int>();
        public Dictionary<string, string> ActionAttributes = new Dictionary<string, string>(); // TODO: rethink if we really need this or can hold everything in default attribs.

        public override string ToString()
        {
            return Name + " (id=" + TypeId.ToString() + ")";
        }
    }
}
