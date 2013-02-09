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
        /// Amount of currency the character has.
        /// </summary>
        public int Gold { get; set; }

        /// <summary>
        /// The group Id's this character participates in.
        /// Temporary membership, like a party or raid.
        /// </summary>
        public List<int> Groups { get; private set; }

        /// <summary>
        /// The group Id's this character participates in.
        /// Permanent membership, like a guild or alliance.
        /// </summary>
        public List<int> Guilds { get; private set; }

        /// <summary>
        /// The chat channel Id's this character participates in.
        /// </summary>
        public List<int> Channels { get; private set; }
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
            Groups = new List<int>();
            Guilds = new List<int>();
            Channels = new List<int>();

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
            Groups = new List<int>();
            Guilds = new List<int>();
            Channels = new List<int>();
            FromFormatString(data);
        }
        #endregion

        #region Methods
        public override void PrepareData()
        {
            // Characters don't have a position field, it must be added to properties.
            SetProperty("_position", Position.ToString());

            SetAttribute("_gold", Gold);

            string guilds = "";
            foreach (int value in Guilds)
                guilds += value.ToString() + ":";
            SetProperty("_guilds", guilds.Trim(':'));

            string chans = "";
            foreach (int value in Channels)
                chans += value.ToString() + ":";
            SetProperty("_channels", chans.Trim(':'));

            base.PrepareData();
        }

        public override void LoadData()
        {
            Position = (Point)GetProperty("_position");

            Gold = (int)GetAttribute("_gold");

            string[] groups = GetProperty("_guilds").Split(':');
            foreach (string g in groups)
            {
                if (!string.IsNullOrEmpty(g))
                    Guilds.Add(Convert.ToInt32(g));
            }

            string[] chans = GetProperty("_channels").Split(':');
            foreach (string c in chans)
            {
                if (!string.IsNullOrEmpty(c))
                    Channels.Add(Convert.ToInt32(c));
            }

            base.LoadData();
        }

        public override string ToFormatString()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append(base.ToFormatString());

            //  We must append instead of prepending data, 
            //  else we could not extract an Entity instance from a character string - which must be possible!
            sb.Append(AccountId);
            sb.Append(":");
            sb.Append(CharacterId);
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

            // Parse elements from base class.
            base.FromFormatString(data, out counter);

            // Parse our own.
            AccountId = int.Parse(split[counter++]);
            CharacterId = int.Parse(split[counter++]);

            // Update consumed for children.
            consumedElements = counter;
        }
        #endregion
    }
}
