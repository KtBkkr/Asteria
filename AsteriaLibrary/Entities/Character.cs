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
    public class Character : Entity
    {
        #region Fields
        /// <summary>
        /// For internal asteria framework infrastructure support only, do not use!
        /// </summary>
        public NetConnection Sender;

        /// <summary>
        /// For internal asteria framework infrastructure support only, do not use!
        /// </summary>
        public DateTime LastSaved = DateTime.MinValue;

        /// <summary>
        /// For internal asteria framework infrastructure support only, do not use!
        /// </summary>
        public List<ServerToClientMessage> MessageBuffer = new List<ServerToClientMessage>();
        #endregion

        #region Properties
        /// <summary>
        /// Characters accountId.
        /// </summary>
        public int AccountId { get; set; }

        /// <summary>
        /// Characters characterId.
        /// </summary>
        public int CharacterId { get; set; }

        /// <summary>
        /// When did the player login?
        /// </summary>
        public DateTime LoginTime { get; set; }

        /// <summary>
        /// Characters time played in seconds.
        /// </summary>
        public int TimePlayed { get; set; }

        /// <summary>
        /// Player creation time.
        /// </summary>
        public DateTime PlayerCreated { get; set; }

        /// <summary>
        /// Gets the players inventory size.
        /// </summary>
        public Size InventorySize { get; set; }

        /// <summary>
        /// List of the items stored in the inventory.
        /// </summary>
        public List<InventoryBag> InventoryItems { get; private set; }

        /// <summary>
        /// True if the character is dead.
        /// </summary>
        public bool IsDead { get; set; }

        /// <summary>
        /// The team Id's this character participates in.
        /// Temporary membership, like a party or raid.
        /// </summary>
        public List<int> TeamsMember { get; private set; }

        /// <summary>
        /// The group Id's this character participates in.
        /// Permanent membership, like a guild or alliance.
        /// </summary>
        public List<int> GroupsMember { get; private set; }
        #endregion

        #region Constructors
        /// <summary>
        /// Creates a new Character instance.
        /// </summary>
        public Character() : this(-1, -1, "Player") { }

        /// <summary>
        /// Creates a new Character instance.
        /// </summary>
        /// <param name="accountId"></param>
        /// <param name="characterId"></param>
        /// <param name="name"></param>
        public Character(int accountId, int characterId, string name)
        {
            TeamsMember = new List<int>();
            GroupsMember = new List<int>();

            this.InventoryItems = new List<InventoryBag>();
            this.AccountId = accountId;
            this.CharacterId = characterId;
            this.Name = name;
        }

        /// <summary>
        /// Creates a new Character instance from serialized data.
        /// </summary>
        /// <param name="data"></param>
        public Character(string data)
        {
            TeamsMember = new List<int>();
            GroupsMember = new List<int>();
            FromFormatString(data);
        }
        #endregion

        #region Methods
        /// <summary>
        /// Generic character format for sending to clients.
        /// </summary>
        /// <returns></returns>
        public override string ToFormatString()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append(base.ToFormatString());

            //  We must append instead of prepending data, 
            //  else we could not extract an Entity instance from a character string - which must be possible!
            sb.Append(":");
            sb.Append(AccountId);
            sb.Append(":");
            sb.Append(CharacterId);
            sb.Append(":");

            sb.Append(InventorySize.ToString());
            sb.Append(":");
            sb.Append(InventoryItems.Count);
            sb.Append(":");
            foreach (InventoryBag ib in this.InventoryItems)
            {
                sb.Append(((IStringFormattable)ib).ToFormatString());
                sb.Append(":");
            }
            return sb.ToString();
        }

        public override void FromFormatString(string data)
        {
            int elements;
            FromFormatString(data, out elements);
            string[] split = data.Split(':');
            elements++;
            AccountId = int.Parse(split[elements++]);
            CharacterId = int.Parse(split[elements++]);

            //  inventory
            InventoryItems = new List<InventoryBag>();
            Size size = (Size)split[elements++];
            int inventoryCount = int.Parse(split[elements++]);
            for (int i = 0; i < inventoryCount; i++)
            {
                InventoryBag ib = new InventoryBag();
                ((IStringFormattable)ib).FromFormatString(split[elements++]);
                this.InventoryItems.Add(ib);
            }
        }
        #endregion
    }
}
