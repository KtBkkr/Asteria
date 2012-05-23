using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AsteriaLibrary.Math;
using AsteriaLibrary.Shared;

namespace AsteriaLibrary.Entities
{
    /// <summary>
    /// Generic inventory item described by an id, amount and position inside the inventory.
    /// </summary>
    public class InventoryBag : IStringFormattable
    {
        #region Fields
        /// <summary>
        /// The Id represending the stored items class.
        /// If used the WSE must decide about the meaning of ItemClassId.
        /// </summary>
        public int ItemClassTypeId;

        /// <summary>
        /// Number of items stored (if stackable).
        /// </summary>
        public int Amount;

        /// <summary>
        /// Position inside characters inventory.
        /// </summary>
        public Point Position;
        #endregion

        #region Methods
        /// <summary>
        /// Adds one item to the bag.
        /// </summary>
        public void AddItem()
        {
            this.Amount++;
        }

        /// <summary>
        /// Adds multuple items to the bag.
        /// </summary>
        public void AddItem(int amount)
        {
            this.Amount += amount;
        }

        /// <summary>
        /// Removes one item from the bag.
        /// </summary>
        public void RemoveItem()
        {
            this.Amount--;
        }

        /// <summary>
        /// Removes multiple items from the bag.
        /// </summary>
        public void RemoveItem(int amount)
        {
            this.Amount -= amount;
        }

        /// <summary>
        /// Creates a new InventoryBag instance based on the passed in bag.
        /// </summary>
        /// <param name="bag"></param>
        /// <returns></returns>
        public static InventoryBag FromInventoryBag(InventoryBag bag)
        {
            InventoryBag newBag = new InventoryBag();
            newBag.ItemClassTypeId = bag.ItemClassTypeId;
            newBag.Amount = bag.Amount;
            newBag.Position = bag.Position;
            return newBag;
        }

        #region IStringFormattable Members
        string IStringFormattable.ToFormatString()
        {
            return string.Format("{0};{1};{2}", ItemClassTypeId, Amount, Position);
        }

        void IStringFormattable.FromFormatString(string data)
        {
            string[] param = data.Split(';');
            ItemClassTypeId = int.Parse(param[0]);
            Amount = int.Parse(param[1]);
            Position = (Point)param[2];
        }
        #endregion

        #endregion
    }
}
