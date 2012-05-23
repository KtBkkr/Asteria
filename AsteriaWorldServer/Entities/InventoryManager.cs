using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AsteriaLibrary.Entities;
using AsteriaLibrary.Math;
using AsteriaLibrary.Shared;
using AsteriaLibrary.Zones;

namespace AsteriaWorldServer.Entities
{
    /// <summary>
    /// Handles inventory layout and adding/removing items from inventory.
    /// </summary>
    public static class InventoryManager
    {
        #region Methods
        /// <summary>
        /// Adds an item to the inventory.
        /// The item is expected to be a pickable item so no check is done.
        /// The inventory is supposed to be initialized and the sizes are not changed.
        /// </summary>
        /// <param name="character"></param>
        /// <param name="entityIdToBeAdded"></param>
        /// <param name="amount"></param>
        /// <param name="ecd"></param>
        /// <param name="bag"></param>
        /// <returns>True on success else false.</returns>
        public static bool AddItem(Character character, int entityIdToBeAdded, int amount, out EntityClassData ecd, out InventoryBag bag)
        {
            // 1. Find slot type & entity class
            // 2. Find first slot the item can fit
            // 3. Add to inventory

            // Find entity class data.
            if (GetEntityClassData(entityIdToBeAdded, out ecd))
            {
                // Check if bag already exists.
                // TODO: [LOW] if theres room in another bag fit what we can into it then create a new bag for whatever amount is left.
                if (FindInventoryBag(character, ecd, amount, out bag))
                {
                    bag.AddItem(amount);
                    return true;
                }
                else
                {
                    // Check there is room for a new bag.
                    Point location;
                    if (FindEmptySlot(character, ecd.SlotSize, out location))
                    {
                        // There is room, create bag and add to inventory.
                        bag = new InventoryBag();
                        bag.ItemClassTypeId = ecd.TypeId;
                        bag.Position = location;
                        bag.Amount = amount;
                        character.InventoryItems.Add(bag);
                        return true;
                    }
                }
            }
            bag = null;
            return false;
        }

        /// <summary>
        /// Looks up the entity class info.
        /// </summary>
        /// <param name="entityId"></param>
        /// <param name="ecd"></param>
        /// <returns></returns>
        private static bool GetEntityClassData(int entityId, out EntityClassData ecd)
        {
            // Check entity
            Entity e = ZoneManager.Singletone.GetEntity(entityId);
            if (e == null)
            {
                //Logger.Output(this, "GetEntityClassData() entity: {0} not found!", entityId);
                ecd = null;
                return false;
            }

            // Check entities class
            ecd = DataManager.Singletone.GetEntityClass(e.TypeId);
            if (ecd == null)
            {
                //Logger.Output(this, "GetEntityClassData() entity: {0}, no class data found!", entityId);
                return false;
            }
            return true;
        }

        private static bool FindInventoryBag(Character c, EntityClassData ecd, int amount, out InventoryBag bag)
        {
            // Check if item is stackable and a bag already exists.
            var matches = from match in c.InventoryItems where match.ItemClassTypeId == ecd.TypeId && (match.Amount + amount <= ecd.SlotStacks) select match;

            if (matches.Count() > 0)
                bag = matches.First();
            else
                bag = null;

            return bag != null;
        }

        /// <summary>
        /// Searches the passed inventory for the first empty slot matching the slot type dimensions.
        /// TODO: [MID] rethink if inventory items can take up multiple slots..
        /// </summary>
        /// <param name="c"></param>
        /// <param name="slotSize"></param>
        /// <param name="location"></param>
        /// <returns></returns>
        private static bool FindEmptySlot(Character c, Size slotSize, out Point location)
        {
            // TODO: [HIGH] this is too slow to calculate for each inventory access and should be cached per character
            int[,] inventoryMap = CreateInventoryMap(c);

            location = Point.Zero;
            for (location.X = 0; location.X < c.InventorySize.Width; location.X++)
            {
                for (location.Y = 0; location.Y < c.InventorySize.Height; location.Y++)
                {
                    // Is this slot empty?
                    if (inventoryMap[location.X, location.Y] == 0)
                    {
                        // Check if there is enough space according to items slot consumption.
                        bool canFit = true;
                        for (int x = 0; x < slotSize.X; x++)
                        {
                            for (int y = 0; y < slotSize.Y; y++)
                            {
                                if (inventoryMap[location.X + x, location.Y + y] == 1)
                                {
                                    canFit = false;
                                    break;
                                }
                                if (!canFit) break;
                            }
                            if (!canFit) break;
                        }
                        if (canFit) return true;
                    }
                }
            }
            return false;
        }

        private static int[,] CreateInventoryMap(Character c)
        {
            int[,] i = new int[c.InventorySize.X, c.InventorySize.Y];

            DataManager wseDmngr = DataManager.Singletone;
            var ecdList = from bag in c.InventoryItems
                          select new
                              {
                                  Ecd = wseDmngr.GetEntityClass(bag.ItemClassTypeId),
                                  Position = bag.Position
                              };

            foreach (var item in ecdList)
            {
                for (int x = 0; x < item.Ecd.SlotSize.X; x++)
                {
                    for (int y = 0; y < item.Ecd.SlotSize.Y; y++)
                    {
                        i[item.Position.X + x, item.Position.Y + y] = 1;
                    }
                }
            }

            return i;
        }
        #endregion
    }
}
