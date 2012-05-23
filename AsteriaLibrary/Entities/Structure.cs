using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AsteriaLibrary.Entities
{
    /// <summary>
    /// Represents an in game structure/building owned by a character.
    /// </summary>
    class Structure : Entity
    {
        #region Fields
        #endregion

        #region Properties
        public int CharacterId { get; set; }
        #endregion

        #region Constructors
        /// <summary>
        /// Creates a new Structure instance.
        /// </summary>
        public Structure() : base(-1, -1, "Structure") { }

        /// <summary>
        /// Creates a new Structure instance.
        /// </summary>
        /// <param name="accountId"></param>
        /// <param name="characterId"></param>
        /// <param name="name"></param>
        public Structure(int accountId, int characterId, string name)
        {
            this.Name = name;
        }

        /// <summary>
        /// Creates a new Structure instance from serialized data.
        /// </summary>
        /// <param name="data"></param>
        public Structure(string data)
        {
            FromFormatString(data);
        }
        #endregion

        #region Methods
        /// <summary>
        /// Generic structure format for sending to clients.
        /// </summary>
        /// <returns></returns>
        public override string ToFormatString()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append(base.ToFormatString());

            //  We must append instead of prepending data, 
            //  else we could not extract an Entity instance from a character string - which must be possible!
            sb.Append(":");
            sb.Append(CharacterId);
            sb.Append(":");

            return sb.ToString();
        }

        public override void FromFormatString(string data)
        {
            int elements;
            FromFormatString(data, out elements);
            string[] split = data.Split(':');
            elements++;

            CharacterId = int.Parse(split[elements++]);
        }
        #endregion
    }
}
