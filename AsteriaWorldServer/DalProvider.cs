using System;
using System.Collections.Generic;
using System.Text;
using AsteriaLibrary.Entities;
using AsteriaLibrary.Math;
using AsteriaLibrary.Messages;
using AsteriaLibrary.Shared;
using AsteriaLibrary.Zones;
using AsteriaWorldServer.Entities;
using AsteriaWorldServer.Messages;
using AsteriaWorldServer.PlayerCache;
using Lidgren.Network;
using MySql.Data.MySqlClient;

namespace AsteriaWorldServer
{
    /// <summary>
    /// DAL support during the character management stage, handles all DB access for fetching players character data, creating and deleting characters.
    /// </summary>
    public sealed class DalProvider
    {
        #region Fields
        private ServerContext context;
        private CharacterManager characterMngr;

        private MySqlCommand cmdInsertCharacter;
        private MySqlCommand cmdEditCharacter;
        private MySqlCommand cmdDeleteCharacter;
        private MySqlCommand cmdGetCharacters;

        private MySqlCommand cmdGetCharacterById;
        private MySqlCommand cmdGetCharacterByName;

        private MySqlCommand cmdInsertAttribute;
        private MySqlCommand cmdEditAttribute;
        private MySqlCommand cmdGetAttributes;

        private MySqlCommand cmdInsertProperty;
        private MySqlCommand cmdEditProperty;
        private MySqlCommand cmdGetProperties;

        private MySqlCommand cmdDeleteInventory;

        private const int COMMAND_TIMEOUT = 5; // TODO: [LOW] do we need a parameter for timeout seconds?

        private const string PRP_GROUPS = "_groups";
        private const string PRP_POSITION = "_position";
        private const string PRP_ROTATION = "_rotation";
        private const string PRP_GOLD = "_gold";
        private const string PRP_INVENTORY = "_invBag";

        private const string ATR_DEAD = "_isdead";
        private const string ATR_TYPEID = "_typeid";
        private const string ATR_INVSIZEX = "_invSizeX";
        private const string ATR_INVSIZEY = "_invSizeY";
        #endregion

        #region Constructors
        public DalProvider(ServerContext context)
        {
            this.context = context;
            this.characterMngr = context.CharacterManager;

            // Command for adding new characters.
            cmdInsertCharacter = new MySqlCommand();
            cmdInsertCharacter.CommandText = "INSERT INTO characters (account_id,name,last_played,created,time_played) VALUES (@accountId, @name, NULL, @datenow, 0)";
            cmdInsertCharacter.CommandTimeout = COMMAND_TIMEOUT;
            cmdInsertCharacter.Parameters.Add("@accountId", MySqlDbType.Int32, 5);
            cmdInsertCharacter.Parameters.Add("@name", MySqlDbType.String, 50);
            cmdInsertCharacter.Parameters.Add("@datenow", MySqlDbType.DateTime, 4);

            // Command for editing characters.
            cmdEditCharacter = new MySqlCommand();
            cmdEditCharacter.CommandText = "UPDATE characters SET last_played = @lastplayed, time_played = @timeplayed WHERE account_id = @accountId AND character_id = @characterId";
            cmdEditCharacter.CommandTimeout = COMMAND_TIMEOUT;
            cmdEditCharacter.Parameters.Add("@accountId", MySqlDbType.Int32, 4);
            cmdEditCharacter.Parameters.Add("@characterId", MySqlDbType.Int32, 4);
            cmdEditCharacter.Parameters.Add("@timeplayed", MySqlDbType.Int32, 4);
            cmdEditCharacter.Parameters.Add("@lastplayed", MySqlDbType.DateTime, 4);

            // Command for deleting characters.
            cmdDeleteCharacter = new MySqlCommand();
            cmdDeleteCharacter.CommandText = "DELETE FROM characters WHERE account_id = @accountId AND character_id = @characterId; DELETE FROM character_attributes WHERE character_id = @characterId; DELETE FROM character_properties WHERE character_id = @characterId;";
            cmdDeleteCharacter.CommandTimeout = COMMAND_TIMEOUT;
            cmdDeleteCharacter.Parameters.Add("@accountId", MySqlDbType.Int32, 4);
            cmdDeleteCharacter.Parameters.Add("@characterId", MySqlDbType.Int32, 4);

            // Command for loading characters by ID.
            cmdGetCharacterById = new MySqlCommand();
            cmdGetCharacterById.CommandText = "SELECT character_id, name, last_played, created, time_played FROM characters WHERE account_id = @accountId AND character_id = @characterId";
            cmdGetCharacterById.CommandTimeout = COMMAND_TIMEOUT;
            cmdGetCharacterById.Parameters.Add("@accountId", MySqlDbType.Int32, 4);
            cmdGetCharacterById.Parameters.Add("@characterId", MySqlDbType.Int32, 4);

            // Command for loading characters by name.
            cmdGetCharacterByName = new MySqlCommand();
            cmdGetCharacterByName.CommandText = "SELECT character_id, name, last_played, created, time_played FROM  characters WHERE account_id = @accountId AND name = @name";
            cmdGetCharacterByName.CommandTimeout = COMMAND_TIMEOUT;
            cmdGetCharacterByName.Parameters.Add("@accountId", MySqlDbType.Int32, 4);
            cmdGetCharacterByName.Parameters.Add("@name", MySqlDbType.String, 50);

            // Command for retrieving account characters.
            cmdGetCharacters = new MySqlCommand();
            cmdGetCharacters.CommandText = "SELECT account_id, character_id, name, last_played, time_played FROM characters WHERE account_id = @accountId";
            cmdGetCharacters.CommandTimeout = COMMAND_TIMEOUT;
            cmdGetCharacters.Parameters.Add("@accountId", MySqlDbType.Int32, 4);

            //Command for inserting character attributes.
            cmdInsertAttribute = new MySqlCommand();
            cmdInsertAttribute.CommandText = "INSERT INTO character_attributes (character_id,attribute,value,description) VALUES (@characterId, @attributeName, @attributeValue, @attributeDescription)";
            cmdInsertAttribute.CommandTimeout = COMMAND_TIMEOUT;
            cmdInsertAttribute.Parameters.Add("@characterId", MySqlDbType.Int32, 4);
            cmdInsertAttribute.Parameters.Add("@attributeName", MySqlDbType.String, 15);
            cmdInsertAttribute.Parameters.Add("@attributeValue", MySqlDbType.Int32, 4);
            cmdInsertAttribute.Parameters.Add("@attributeDescription", MySqlDbType.String, 50);

            // Command for editing character attributes.
            cmdEditAttribute = new MySqlCommand();
            cmdEditAttribute.CommandText = "UPDATE character_attributes SET value = @attributeValue, description = @attributeDescription WHERE character_id = @characterId AND attribute = @attributeName";
            cmdEditAttribute.CommandTimeout = COMMAND_TIMEOUT;
            cmdEditAttribute.Parameters.Add("@characterId", MySqlDbType.Int32, 4);
            cmdEditAttribute.Parameters.Add("@attributeName", MySqlDbType.String, 15);
            cmdEditAttribute.Parameters.Add("@attributeValue", MySqlDbType.Int32, 4);
            cmdEditAttribute.Parameters.Add("@attributeDescription", MySqlDbType.String, 50);

            // Command for fetching all character attributes.
            cmdGetAttributes = new MySqlCommand();
            cmdGetAttributes.CommandText = "SELECT * FROM character_attributes WHERE character_id = @characterId";
            cmdGetAttributes.CommandTimeout = COMMAND_TIMEOUT;
            cmdGetAttributes.Parameters.Add("@characterId", MySqlDbType.Int32, 4);

            //Command for inserting character properties.
            cmdInsertProperty = new MySqlCommand();
            cmdInsertProperty.CommandText = "INSERT INTO character_properties (character_id,property,value,description) VALUES (@characterId, @propertyName, @propertyValue, @propertyDescription)";
            cmdInsertProperty.CommandTimeout = COMMAND_TIMEOUT;
            cmdInsertProperty.Parameters.Add("@characterId", MySqlDbType.Int32, 4);
            cmdInsertProperty.Parameters.Add("@propertyName", MySqlDbType.String, 15);
            cmdInsertProperty.Parameters.Add("@propertyValue", MySqlDbType.String, 500);
            cmdInsertProperty.Parameters.Add("@propertyDescription", MySqlDbType.String, 50);

            // Command for editing character properties.
            cmdEditProperty = new MySqlCommand();
            cmdEditProperty.CommandText = "UPDATE character_properties SET value = @propertyValue, description = @propertyDescription WHERE character_id = @characterId AND property = @propertyName";
            cmdEditProperty.CommandTimeout = COMMAND_TIMEOUT;
            cmdEditProperty.Parameters.Add("@characterId", MySqlDbType.Int32, 4);
            cmdEditProperty.Parameters.Add("@propertyName", MySqlDbType.String, 15);
            cmdEditProperty.Parameters.Add("@propertyValue", MySqlDbType.String, 500);
            cmdEditProperty.Parameters.Add("@propertyDescription", MySqlDbType.String, 50);

            // Command for fetching all character properties.
            cmdGetProperties = new MySqlCommand();
            cmdGetProperties.CommandText = "SELECT * FROM character_properties WHERE character_id = @characterId";
            cmdGetProperties.CommandTimeout = COMMAND_TIMEOUT;
            cmdGetProperties.Parameters.Add("@characterId", MySqlDbType.Int32, 4);

            // Command for deleting character inventory.
            cmdDeleteInventory = new MySqlCommand();
            cmdDeleteInventory.CommandText = String.Format("DELETE FROM character_properties WHERE character_id = @characterId AND property LIKE '{0}%'", PRP_INVENTORY);
            cmdDeleteInventory.CommandTimeout = COMMAND_TIMEOUT;
            cmdDeleteInventory.Parameters.Add("@characterId", MySqlDbType.Int32, 4);
        }
        #endregion

        #region Methods
        /// <summary>
        /// Retrieves character information of all characters for an existing player (accountId).
        /// </summary>
        /// <param name="accountId"></param>
        /// <returns></returns>
        private List<Character> LoadAccountCharacterList(int accountId)
        {
            MySqlDataReader dr;
            List<Character> result = new List<Character>();

            using (MySqlConnection conn = new MySqlConnection(context.Css))
            {
                conn.Open();

                lock (cmdGetCharacters)
                {
                    cmdGetCharacters.Connection = conn;
                    cmdGetCharacters.Parameters["@accountId"].Value = accountId;
                    dr = cmdGetCharacters.ExecuteReader();
                }

                while (dr.Read())
                {
                    Character item = new Character(
                        DatabaseHelper.GetInt32(dr, "account_id"),
                        DatabaseHelper.GetInt32(dr, "character_id"),
                        DatabaseHelper.GetString(dr, "name"));

                    result.Add(item);
                }

                dr.Close();

                foreach (Character ch in result)
                    LoadCompleteCharacter(ch);
            }
            return result;
        }

        /// <summary>
        /// Returns a ServerToClientMessage containing a response to a MessageType.GetCharacterList message.
        /// The character list is fetched from the DB and the message passed to the WSE before sending it to the client.
        /// </summary>
        /// <param name="mpr"></param>
        /// <returns></returns>
        public ServerToClientMessage GetAccountCharacterListMessage(MasterPlayerRecord mpr)
        {
            ServerToClientMessage wm = ServerToClientMessage.CreateMessageSafe(mpr.Sender);
            wm.Code = mpr.AccountId;
            wm.TurnNumber = -1;
            wm.MessageType = MessageType.S2C_CharacterList;

            List<Character> characterList = LoadAccountCharacterList(mpr.AccountId);

            // Build a pipe delimited list of colon separated character data.
            bool isFirst = true;
            StringBuilder sb = new StringBuilder();
            foreach (Character ch in characterList)
            {
                if (isFirst)
                    isFirst = false;
                else
                    sb.Append("|");

                sb.Append(ch.ToFormatString());
            }
            wm.Data = sb.ToString();

            return wm;
        }

        /// <summary>
        /// Creates an empty character, sends it with the client data to the WSE for initialization,
        /// adds default attribute set to the character and finally saves the character.
        /// </summary>
        /// <param name="accountId"></param>
        /// <param name="msg"></param>
        /// <returns></returns>
        public ServerToClientMessage CreateCharacter(int accountId, ClientToServerMessage msg)
        {
            ServerToClientMessage wm = ServerToClientMessage.CreateMessageSafe(msg.Sender);
            wm.TurnNumber = -1;

            Character ch = new Character();
            ch.AccountId = accountId;

            try
            {
                // Pass empty char to character manager for initialization (add attributes and properties).
                if (characterMngr.HandleCreateCharacterMsg(ch, msg.GameData))
                {
                    using (MySqlConnection conn = new MySqlConnection(context.Css))
                    {
                        conn.Open();

                        // Check character name doesn't exist.
                        lock (cmdGetCharacterByName)
                        {
                            cmdGetCharacterByName.Connection = conn;
                            cmdGetCharacterByName.Parameters["@accountId"].Value = accountId;
                            cmdGetCharacterByName.Parameters["@name"].Value = ch.Name;
                            object obj = cmdGetCharacterByName.ExecuteScalar();

                            if (obj != null)
                            {
                                Logger.Output(this, "Player ID {0} character creation failed: A character named '{1}' exists already.", accountId, ch.Name);
                                wm.MessageType = MessageType.S2C_CreateFailed;
                                return wm;
                            }
                        }

                        // Insert new character into the DB.
                        lock (cmdInsertCharacter)
                        {
                            cmdInsertCharacter.Connection = conn;
                            cmdInsertCharacter.Parameters["@accountId"].Value = accountId;
                            cmdInsertCharacter.Parameters["@name"].Value = ch.Name;
                            cmdInsertCharacter.Parameters["@datenow"].Value = DateTime.Now;
                            cmdInsertCharacter.ExecuteNonQuery();
                        }

                        // Check character index.
                        lock (cmdGetCharacterByName)
                        {
                            cmdGetCharacterByName.Connection = conn;
                            cmdGetCharacterByName.Parameters["@accountId"].Value = accountId;
                            cmdGetCharacterByName.Parameters["@name"].Value = ch.Name;
                            object obj = cmdGetCharacterByName.ExecuteScalar();

                            if (obj != null)
                                ch.CharacterId = (int)obj;
                        }
                    }

                    Logger.Output(this, "Player ID {0} created character: '{1}' ID: {2}.", msg.AccountId, ch.Name, ch.CharacterId);

                    // Save the rest of the character to the DB so the attributes and properties are saved.
                    InsertCharacterAttributes(ch);

                    wm.Data = "CREATE";
                    wm.MessageType = MessageType.S2C_CreateSuccess;
                }
                else
                {
                    Logger.Output(this, "Character manager refused creation! Data ('{0}').", msg.GameData);
                    wm.Data = "CREATE";
                    wm.MessageType = MessageType.S2C_CreateFailed;
                }
            }
            catch (Exception ex)
            {
                Logger.Output(this, "CreateCharacter exception: {0}, stacktrace: {1}", ex.Message, ex.StackTrace);
                wm.Data = "CREATE";
                wm.MessageType = MessageType.S2C_CreateFailed;
            }
            return wm;
        }

        /// <summary>
        /// Deletes a character from the database.
        /// </summary>
        /// <param name="msg"></param>
        /// <returns></returns>
        public ServerToClientMessage DeleteCharacter(ClientToServerMessage msg)
        {
            ServerToClientMessage wm = ServerToClientMessage.CreateMessageSafe(msg.Sender);
            int characterId = -1;

            try
            {
                using (MySqlConnection conn = new MySqlConnection(context.Css))
                {
                    conn.Open();

                    // Check if player has ownership of the characterId.
                    lock (cmdGetCharacterById)
                    {
                        cmdGetCharacterById.Connection = conn;
                        cmdGetCharacterById.Parameters["@accountId"].Value = msg.AccountId;
                        cmdGetCharacterById.Parameters["@characterId"].Value = msg.CharacterId;
                        object obj = cmdGetCharacterById.ExecuteScalar();

                        if (obj != null)
                            characterId = (int)obj;
                    }

                    // If characterId matches delete all.
                    if (characterId != -1)
                    {
                        // Open DB connection and delete attributes.
                        lock (cmdDeleteCharacter)
                        {
                            cmdDeleteCharacter.Connection = conn;
                            cmdDeleteCharacter.Parameters["@accountId"].Value = msg.AccountId;
                            cmdDeleteCharacter.Parameters["@characterId"].Value = msg.CharacterId;
                            cmdDeleteCharacter.ExecuteNonQuery();
                        }

                        Logger.Output(this, "Player ID {0} deleted character ID: {1}.", msg.AccountId, characterId);
                    }

                    wm.Data = "DELETE";
                    wm.MessageType = MessageType.S2C_DeleteSuccess;
                }
            }
            catch (Exception ex)
            {
                Logger.Output(this, "DalProvider.DeleteCharacter() exception: {0}, stacktrace: {1}", ex.Message, ex.StackTrace);
                wm.Data = "DELETE";
                wm.MessageType = MessageType.S2C_DeleteFailed;
            }
            return wm;
        }

        /// <summary>
        /// Loads all character attributes, the Character istance is
        /// expected to have a valid characterId and accountId.
        /// </summary>
        /// <param name="character"></param>
        public void LoadCompleteCharacter(Character character)
        {
            try
            {
                MySqlDataReader dr;
                using (MySqlConnection conn = new MySqlConnection(context.Css))
                {
                    conn.Open();

                    lock (cmdGetCharacterById)
                    {
                        cmdGetCharacterById.Connection = conn;
                        cmdGetCharacterById.Parameters["@accountId"].Value = character.AccountId;
                        cmdGetCharacterById.Parameters["@characterId"].Value = character.CharacterId;
                        dr = cmdGetCharacterById.ExecuteReader();
                    }

                    while (dr.Read())
                    {
                        character.Name = DatabaseHelper.GetString(dr, "name");
                        character.PlayerCreated = DatabaseHelper.GetDateTime(dr, "created");
                        character.TimePlayed = DatabaseHelper.GetInt32(dr, "time_played");
                    }
                    dr.Close();

                    lock (cmdGetAttributes)
                    {
                        cmdGetAttributes.Connection = conn;
                        cmdGetAttributes.Parameters["@characterId"].Value = character.CharacterId;
                        dr = cmdGetAttributes.ExecuteReader();
                    }

                    while (dr.Read())
                    {
                        // Get attribute name and value.
                        string name = DatabaseHelper.GetString(dr, "attribute").ToLowerInvariant();
                        int value = DatabaseHelper.GetInt32(dr, "value");
                        string description = DatabaseHelper.GetString(dr, "description");

                        // Check if special attribute.
                        switch (name)
                        {
                            case ATR_DEAD:
                                if (value == 1)
                                    character.IsDead = true;
                                else
                                    character.IsDead = false;
                                break;

                            case ATR_TYPEID:
                                character.TypeId = value;
                                break;

                            case ATR_INVSIZEX:
                                character.InventorySize = new Size(value, character.InventorySize.Y);
                                break;

                            case ATR_INVSIZEY:
                                character.InventorySize = new Size(character.InventorySize.X, value);
                                break;

                            default:
                                character.SetAttribute(name, value, description);
                                break;
                        }
                    }
                    dr.Close();

                    lock (cmdGetProperties)
                    {
                        cmdGetProperties.Connection = conn;
                        cmdGetProperties.Parameters["@characterId"].Value = character.CharacterId;
                        dr = cmdGetProperties.ExecuteReader();
                    }

                    while (dr.Read())
                    {
                        // Get property name and value.
                        string name = DatabaseHelper.GetString(dr, "property").ToLowerInvariant();
                        string value = DatabaseHelper.GetString(dr, "value");
                        string description = DatabaseHelper.GetString(dr, "description");

                        // Handle special properties.
                        if (name == PRP_GROUPS)
                        {
                            string[] groups = value.Split(':');
                            character.GroupsMember.Clear();

                            int valout;
                            foreach (string val in groups)
                            {
                                if (int.TryParse(val, out valout))
                                    character.GroupsMember.Add(valout);
                            }
                            continue;
                        }
                        else if (name == PRP_POSITION)
                        {
                            if (!string.IsNullOrEmpty(value))
                                character.Position = (Point)value;
                            else
                                character.Position = Point.Zero;

                            continue;
                        }
                        else if (name == PRP_ROTATION)
                        {
                            if (!string.IsNullOrEmpty(value))
                                character.Rotation = Convert.ToInt32(value);
                            else
                                character.Rotation = 0;

                            continue;
                        }
                        else if (name == PRP_GOLD)
                        {
                            if (!string.IsNullOrEmpty(value))
                                character.Gold = Convert.ToUInt32(value);
                            else
                                character.Gold = 0;

                            continue;
                        }
                        else if (name.StartsWith(PRP_INVENTORY, StringComparison.InvariantCultureIgnoreCase))
                        {
                            if (!string.IsNullOrEmpty(value))
                            {
                                IStringFormattable ib = new InventoryBag();
                                ib.FromFormatString(value);
                                character.InventoryItems.Add((InventoryBag)ib);
                            }
                            continue;
                        }
                        else if (name.StartsWith("_", StringComparison.InvariantCultureIgnoreCase))
                        {
                            Logger.Output(this, "LoadCompleteCharacter found unhandled property: {0}, character: {1}, ignoring..", name, character.CharacterId);
                            continue;
                        }
                        else
                            character.SetProperty(name, value, description);
                    }
                    dr.Close();
                }

                Point pos = character.Position;
                var zone = context.ZoneManager.FindZoneContaining(ref pos);
                character.LastZone = zone.Id;
                character.LoginTime = DateTime.Now;
            }
            catch (Exception ex)
            {
                Logger.Output(this, "LoadCompleteCharacter exception: {0}, stacktrace: {1}", ex.Message, ex.StackTrace);
            }
        }

        /// <summary>
        /// Saves a character with all attributes and properties.
        /// </summary>
        /// <param name="character"></param>
        public void SaveCompleteCharacter(Character character)
        {
            try
            {
                using (MySqlConnection conn = new MySqlConnection(context.Css))
                {
                    conn.Open();

                    lock (cmdEditCharacter)
                    {
                        cmdEditCharacter.Connection = conn;
                        cmdEditCharacter.Parameters["@characterId"].Value = character.CharacterId;
                        cmdEditCharacter.Parameters["@accountId"].Value = character.AccountId;
                        cmdEditCharacter.Parameters["@lastplayed"].Value = DateTime.Now;
                        cmdEditCharacter.Parameters["@timeplayed"].Value = character.TimePlayed + (DateTime.Now - character.LoginTime).Seconds;
                        cmdEditCharacter.ExecuteNonQuery();
                    }

                    lock (cmdEditAttribute)
                    {
                        cmdEditAttribute.Connection = conn;
                        cmdEditAttribute.Parameters["@characterId"].Value = character.CharacterId;

                        foreach (KeyValuePair<string, EntityAttribute> kvp in character.Attributes)
                        {
                            // Attributes with a "_" in front are reserved.
                            if (kvp.Value.Name.StartsWith("_"))
                                continue;

                            cmdEditAttribute.Parameters["@attributeName"].Value = kvp.Value.Name.ToLowerInvariant();
                            cmdEditAttribute.Parameters["@attributeValue"].Value = kvp.Value.Value;
                            cmdEditAttribute.Parameters["@attributeDescription"].Value = kvp.Value.Description;
                            cmdEditAttribute.ExecuteNonQuery();
                        }

                        // The IsDead attribute needs to be saved as well.
                        cmdEditAttribute.Parameters["@attributeName"].Value = ATR_DEAD;
                        cmdEditAttribute.Parameters["@attributeValue"].Value = (character.IsDead ? 1 : 0);
                        cmdEditAttribute.Parameters["@attributeDescription"].Value = "";
                        cmdEditAttribute.ExecuteNonQuery();

                        // The IsDead attribute needs to be saved as well.
                        cmdEditAttribute.Parameters["@attributeName"].Value = ATR_DEAD;
                        cmdEditAttribute.Parameters["@attributeValue"].Value = (character.IsDead ? 1 : 0);
                        cmdEditAttribute.Parameters["@attributeDescription"].Value = "";
                        cmdEditAttribute.ExecuteNonQuery();

                        // The TypeID attribute needs to be saved as well.
                        cmdEditAttribute.Parameters["@attributeName"].Value = ATR_TYPEID;
                        cmdEditAttribute.Parameters["@attributeValue"].Value = character.TypeId;
                        cmdEditAttribute.Parameters["@attributeDescription"].Value = "";
                        cmdEditAttribute.ExecuteNonQuery();

                        // InventorySize
                        cmdEditAttribute.Parameters["@attributeName"].Value = ATR_INVSIZEX;
                        cmdEditAttribute.Parameters["@attributeValue"].Value = character.InventorySize.X;
                        cmdEditAttribute.Parameters["@attributeDescription"].Value = "";
                        cmdEditAttribute.ExecuteNonQuery();
                        cmdEditAttribute.Parameters["@attributeName"].Value = ATR_INVSIZEY;
                        cmdEditAttribute.Parameters["@attributeValue"].Value = character.InventorySize.Y;
                        cmdEditAttribute.Parameters["@attributeDescription"].Value = "";
                        cmdEditAttribute.ExecuteNonQuery();
                    }

                    lock (cmdEditProperty)
                    {
                        cmdEditProperty.Connection = conn;
                        cmdEditProperty.Parameters["@characterId"].Value = character.CharacterId;

                        foreach (KeyValuePair<string, EntityProperty> kvp in character.Properties)
                        {
                            // Properties with a "_" in front are reserved.
                            if (kvp.Value.Name.StartsWith("_"))
                                continue;

                            cmdEditProperty.Parameters["@propertyName"].Value = kvp.Value.Name.ToLowerInvariant();
                            cmdEditProperty.Parameters["@propertyValue"].Value = kvp.Value.Value;
                            cmdEditProperty.Parameters["@propertyDescription"].Value = kvp.Value.Description;
                            cmdEditProperty.ExecuteNonQuery();
                        }

                        // Group memberships are permanent as this is a guild or alliance you are in.
                        string groups = "";
                        foreach (int value in character.GroupsMember)
                            groups += value.ToString() + ":";

                        cmdEditProperty.Parameters["@propertyName"].Value = PRP_GROUPS;
                        cmdEditProperty.Parameters["@propertyValue"].Value = groups;
                        cmdEditProperty.Parameters["@propertyDescription"].Value = "";
                        cmdEditProperty.ExecuteNonQuery();

                        // Save the position
                        cmdEditProperty.Parameters["@propertyName"].Value = PRP_POSITION;
                        cmdEditProperty.Parameters["@propertyValue"].Value = character.Position.ToString();
                        cmdEditProperty.Parameters["@propertyDescription"].Value = "";
                        cmdEditProperty.ExecuteNonQuery();

                        // Save the rotation
                        cmdEditProperty.Parameters["@propertyName"].Value = PRP_ROTATION;
                        cmdEditProperty.Parameters["@propertyValue"].Value = character.Rotation.ToString();
                        cmdEditProperty.Parameters["@propertyDescription"].Value = "";
                        cmdEditProperty.ExecuteNonQuery();

                        // Save gold
                        cmdEditProperty.Parameters["@propertyName"].Value = PRP_GOLD;
                        cmdEditProperty.Parameters["@propertyValue"].Value = character.Gold.ToString();
                        cmdEditProperty.Parameters["@propertyDescription"].Value = "";
                        cmdEditProperty.ExecuteNonQuery();
                    }

                    // Inventory requires special care not to destroy
                    // user data if the system dies at this moment.
                    MySqlTransaction transaction = conn.BeginTransaction();
                    lock (cmdInsertProperty)
                    {
                        try
                        {
                            // Prepare everything first.
                            cmdInsertProperty.Connection = conn;
                            cmdInsertProperty.Parameters["@characterId"].Value = character.CharacterId;
                            cmdInsertProperty.Transaction = transaction;

                            cmdDeleteInventory.Connection = conn;
                            cmdDeleteInventory.Parameters["@characterId"].Value = character.CharacterId;
                            cmdDeleteInventory.Transaction = transaction;

                            // Delete existing inventory and fill in new data.
                            cmdDeleteInventory.ExecuteNonQuery();
                            foreach (InventoryBag ib in character.InventoryItems)
                            {
                                cmdInsertProperty.Parameters["@propertyName"].Value = PRP_INVENTORY + ib.Position.ToString();
                                cmdInsertProperty.Parameters["@propertyValue"].Value = ((IStringFormattable)ib).ToFormatString();
                                cmdInsertProperty.Parameters["@propertyDescription"].Value = "";
                                cmdInsertProperty.ExecuteNonQuery();
                            }
                        }
                        catch (Exception ex)
                        {
                            Logger.Output(this, "SaveCompleteCharacter exception while doing inventory save: {0}, stacktrace: {1}", ex.Message, ex.StackTrace);
                            transaction.Rollback();
                        }
                        finally
                        {
                            transaction.Commit();
                        }
                    }
                    character.LastSaved = DateTime.Now;
                }
            }
            catch (Exception ex)
            {
                Logger.Output(this, "SaveCompleteCharacterException: {0}, stacktrace: {1}", ex.Message, ex.StackTrace);
            }
        }

        /// <summary>
        /// Inserts a characters attributes and properties into the db, this is used for character creation only.
        /// </summary>
        /// <param name="character"></param>
        public void InsertCharacterAttributes(Character character)
        {
            try
            {
                using (MySqlConnection conn = new MySqlConnection(context.Css))
                {
                    conn.Open();

                    lock (cmdInsertAttribute)
                    {
                        cmdInsertAttribute.Connection = conn;
                        cmdInsertAttribute.Parameters["@characterId"].Value = character.CharacterId;

                        foreach (KeyValuePair<string, EntityAttribute> kvp in character.Attributes)
                        {
                            if (kvp.Value.Name.StartsWith("_"))
                                continue;

                            cmdInsertAttribute.Parameters["@attributeName"].Value = kvp.Value.Name.ToLowerInvariant();
                            cmdInsertAttribute.Parameters["@attributeValue"].Value = kvp.Value.Value;
                            cmdInsertAttribute.Parameters["@attributeDescription"].Value = kvp.Value.Description;

                            // In case the attribute exists.. for whatever reason
                            try
                            {
                                cmdInsertAttribute.ExecuteNonQuery();
                            }
                            catch (Exception ex)
                            {
                                Logger.Output(this, "InsertCharacterAttributes() exception: {0}, stacktrace: {1}", ex.Message, ex.StackTrace);
                            }
                        }

                        try
                        {
                            // Attributes with a "_" in front are not saved to the database,
                            // thus the WSE can't overwrite this.

                            // The _isdead attribute needs to be saved as well.
                            cmdInsertAttribute.Parameters["@attributeName"].Value = ATR_DEAD;
                            cmdInsertAttribute.Parameters["@attributeValue"].Value = (character.IsDead ? 1 : 0);
                            cmdInsertAttribute.Parameters["@attributeDescription"].Value = "";
                            cmdInsertAttribute.ExecuteNonQuery();

                            // The _typeid attribute needs to be saved as well.
                            cmdInsertAttribute.Parameters["@attributeName"].Value = ATR_TYPEID;
                            cmdInsertAttribute.Parameters["@attributeValue"].Value = character.TypeId;
                            cmdInsertAttribute.Parameters["@attributeDescription"].Value = "";
                            cmdInsertAttribute.ExecuteNonQuery();
                        }
                        catch (Exception ex)
                        {
                            Logger.Output(this, "InsertCharacterAttributes() exception: {0}, stacktrace: {1}", ex.Message, ex.StackTrace);
                        }
                    }

                    lock (cmdInsertProperty)
                    {
                        cmdInsertProperty.Connection = conn;
                        cmdInsertProperty.Parameters["@characterId"].Value = character.CharacterId;

                        foreach (KeyValuePair<string, EntityProperty> kvp in character.Properties)
                        {
                            if (kvp.Value.Name.StartsWith("_"))
                                continue;

                            cmdInsertProperty.Parameters["@propertyName"].Value = kvp.Value.Name.ToLowerInvariant();
                            cmdInsertProperty.Parameters["@propertyValue"].Value = kvp.Value.Value;
                            cmdInsertProperty.Parameters["@propertyDescription"].Value = kvp.Value.Description;

                            // In case the property exists.. for whatever reason
                            try
                            {
                                cmdInsertProperty.ExecuteNonQuery();
                            }
                            catch (Exception ex)
                            {
                                Logger.Output(this, "InsertCharacterAttributes() exception: {0}, stacktrace: {1}", ex.Message, ex.StackTrace);
                            }
                        }

                        try
                        {
                            // Properties with a "_" in front are not saved to the database, thus the WSE can't
                            // overwrite this.

                            // Saving group memberships - permanent as this is a guild or alliance they are in.
                            string groups = "";
                            foreach (int value in character.GroupsMember)
                                groups += value.ToString() + ":";

                            cmdInsertProperty.Parameters["@propertyName"].Value = PRP_GROUPS;
                            cmdInsertProperty.Parameters["@propertyValue"].Value = groups;
                            cmdInsertProperty.Parameters["@propertyDescription"].Value = "";
                            cmdInsertProperty.ExecuteNonQuery();

                            // Insert the position.
                            cmdInsertProperty.Parameters["@propertyName"].Value = PRP_POSITION;
                            cmdInsertProperty.Parameters["@propertyValue"].Value = character.Position.ToString();
                            cmdInsertProperty.Parameters["@propertyDescription"].Value = "";
                            cmdInsertProperty.ExecuteNonQuery();

                            // Insert the rotation.
                            cmdInsertProperty.Parameters["@propertyName"].Value = PRP_ROTATION;
                            cmdInsertProperty.Parameters["@propertyValue"].Value = character.Rotation.ToString();
                            cmdInsertProperty.Parameters["@propertyDescription"].Value = "";
                        }
                        catch (Exception ex)
                        {
                            Logger.Output(this, "InsertCharacterAttributes() exception: {0}, stacktrace: {1}", ex.Message, ex.StackTrace);
                        }
                    }
                    character.LastSaved = DateTime.Now;
                }
            }
            catch (Exception ex)
            {
                Logger.Output(this, "InsertCharacterAttributes() exception: {0}, stacktrace: {1}", ex.Message, ex.StackTrace);
            }
        }

        /// <summary>
        /// Function for loading a character and assigning it to a player which is used for placing characters in the world.
        /// The state is passed as an object so that the function signature matches the WaitCallback and can be executed on a ThreadPool thread.
        /// State holds an integer array of two elements with accountId at index 0 and characterId and index 1.
        /// </summary>
        /// <param name="obj"></param>
        public void LoadPlayerCharacter(object obj)
        {
            try
            {
                // Cast to int and find MPR.
                int[] state = (int[])obj;

                MasterPlayerRecord mpr = context.Mpt.GetByAccountId(state[0]);
                if (mpr == null)
                {
                    Logger.Output(this, "LoadPlayerCharacter() could not retreive MPR for accountId: {0}.", state[0]);
                    return;
                }

                // First reset the MPR state.
                mpr.LogoutCharacterGranted = false;
                mpr.LogoutCharacterRequested = false;
                mpr.LogoutClientRequested = false;

                // Check if player has ownership of the characterId.
                int characterId = -1;
                using (MySqlConnection conn = new MySqlConnection(context.Css))
                {
                    conn.Open();

                    lock (cmdGetCharacterById)
                    {
                        cmdGetCharacterById.Connection = conn;
                        cmdGetCharacterById.Parameters["@accountId"].Value = state[0];
                        cmdGetCharacterById.Parameters["@characterId"].Value = state[1];
                        object p = cmdGetCharacterById.ExecuteScalar();

                        if (p != null)
                            characterId = (int)p;
                    }
                }

                // If character is found, load and start.
                if (characterId != -1)
                {
                    Character c = new Character();
                    c.AccountId = state[0];
                    c.CharacterId = state[1];

                    // Load character.
                    LoadCompleteCharacter(c);
                    c.Sender = mpr.Sender;

                    c.Id = GameProcessor.GenerateEntityID();

                    // Update states.
                    context.Mpt.UpdateCharacterId(state[0], state[1]);
                    mpr.State = ClientState.InWorld;
                    mpr.pCharacter = c;

                    ServerToClientMessage wm = ServerToClientMessage.CreateMessageSafe(mpr.Sender);
#if DEBUG
                    if (mpr.Sender == null)
                        throw (new Exception("Character has NULL sender, this will crash the MessageSender."));
#endif
                    wm.TurnNumber = -1;
                    wm.MessageType = MessageType.S2C_StartSuccess;

                    // We save the zone info here into the character so the client gets
                    // an initial picture of the players zone before the zone message hits.
                    Point position;
                    c.GetPosition(out position);
                    Zone zone = context.ZoneManager.FindZoneContaining(ref position);
                    c.CurrentZone = zone.Id;

                    StringBuilder sb = new StringBuilder();
                    sb.Append(zone.Id);
                    sb.Append(";");
                    sb.Append(zone.Name);
                    sb.Append(";");
                    sb.Append(zone.Min.ToString());
                    sb.Append(";");
                    sb.Append(zone.Max.ToString());
                    c.SetProperty("zoneinfo", sb.ToString());

                    // This is to allow characters saved to the DB without inventory information to be correctly initialized.
                    if (c.InventorySize == Size.Zero)
                    {
                        EntityClassData ecd = DataManager.Singletone.GetPlayerClass(c.TypeId);
                        c.InventorySize = ecd.InventorySize;
                    }

                    // Store colon separated character data list.
                    wm.Data = c.ToFormatString();

                    wm.Code = c.CharacterId;
                    wm.DeliveryMethod = NetDeliveryMethod.ReliableOrdered;
                    QueueManager.WorldMessageQueueReadWrite = wm;

                    // Tell the WSE to start the character.
                    context.GameProcessor.StartCharacter(c);
                }
                else
                {
                    ServerToClientMessage wm = ServerToClientMessage.CreateMessageSafe(mpr.Sender);
#if DEBUG
                    if (mpr.Sender == null)
                        throw (new Exception("Character has NULL sender, this will crash the MessageSender."));
#endif
                    wm.TurnNumber = -1;
                    wm.MessageType = MessageType.S2C_StartFailed;
                    context.Mpt.UpdateCharacterId(state[0], 0);
                    mpr.pCharacter = null;
                    wm.DeliveryMethod = NetDeliveryMethod.ReliableUnordered;
                    QueueManager.WorldMessageQueueReadWrite = wm;
                }
            }
            catch (Exception ex)
            {
                Logger.Output(this, "LoadPlayerCharacter() exception: {0}, stacktrace: {1}", ex.Message, ex.StackTrace);
            }
        }
        #endregion
    }
}
