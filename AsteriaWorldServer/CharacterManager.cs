﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AsteriaLibrary.Entities;
using AsteriaLibrary.Math;
using AsteriaLibrary.Messages;
using AsteriaWorldServer.Entities;

namespace AsteriaWorldServer
{
    /// <summary>
    /// Vanilla character manager, contains ony minimalistic functionality.
    /// </summary>
    public class CharacterManager
    {
        #region Fields
        private string connectionString;
        #endregion

        #region Constructors
        /// <summary>
        /// Creates a new CharacterManager instance.
        /// </summary>
        /// <param name="connectionString"></param>
        public CharacterManager(string connectionString)
        {
            this.connectionString = connectionString;
        }
        #endregion

        #region Methods
        /// <summary>
        /// Creates a new player character.
        /// Parses the typeId and name and updates the character fields.
        /// </summary>
        /// <param name="character">And empty character instance.</param>
        /// <param name="gameData">Contains client data formated as 'typeId|character name'</param>
        /// <returns></returns>
        public bool HandleCreateCharacterMsg(Character character, string gameData)
        {
            string[] data = gameData.Split('|');

            if (data.Length == 2)
            {
                int typeId = int.Parse(data[0]);

                EntityClassData ecd = DataManager.Singletone.GetPlayerClass(typeId);

                // invalid typeId.
                if (ecd == null)
                    return false;

                foreach (KeyValuePair<string, int> kvp in ecd.DefaultAttributes)
                    character.SetAttribute(kvp.Key, kvp.Value);

                // Here we add all props used in the client.
                character.Name = data[1];
                character.TypeId = typeId;
                character.Rotation = 0; // TODO: [LOW] have the player start out facing screen.
                character.Position = (Point)DataManager.Singletone.WorldParameters["PlayerStartPoint"];
                character.Gold = 0;

                character.SetAttribute("_typeid", typeId);
                character.SetAttribute("level", 0);
                character.SetProperty("class", ecd.Name);
                character.SetAttribute("movementoption", 1, "Movement Stance");
                character.SetAttribute("weaponoption", 1, "Weapon Option");
                character.InventorySize = ecd.InventorySize;
                return true;
            }
            return false;
        }
        #endregion
    }
}