using System;
using System.Collections.Generic;
using System.Text;
using AsteriaLibrary.Entities;
using AsteriaLibrary.Math;
using AsteriaLibrary.Messages;
using AsteriaLibrary.Shared;
using AsteriaWorldServer.Messages;
using AsteriaWorldServer.PlayerCache;
using AsteriaWorldServer.Zones;
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

        //

        private MySqlCommand cmdInsertCharacter;
        private MySqlCommand cmdEditCharacter;
        private MySqlCommand cmdDeleteCharacter;
        private MySqlCommand cmdGetCharacters;

        private MySqlCommand cmdGetCharacterById;
        private MySqlCommand cmdGetCharacterByName;

        private MySqlCommand cmdInsertCharacterAttribute;
        private MySqlCommand cmdEditCharacterAttribute;
        private MySqlCommand cmdGetCharacterAttributes;

        private MySqlCommand cmdInsertCharacterProperty;
        private MySqlCommand cmdEditCharacterProperty;
        private MySqlCommand cmdGetCharacterProperties;

        //

        private MySqlCommand cmdInsertEntity;
        private MySqlCommand cmdEditEntity;
        private MySqlCommand cmdDeleteEntity;
        private MySqlCommand cmdGetEntities;

        private MySqlCommand cmdGetEntityById;
        private MySqlCommand cmdGetEntityByOwner;

        private MySqlCommand cmdInsertEntityAttribute;
        private MySqlCommand cmdEditEntityAttribute;
        private MySqlCommand cmdGetEntityAttributes;

        private MySqlCommand cmdInsertEntityProperty;
        private MySqlCommand cmdEditEntityProperty;
        private MySqlCommand cmdGetEntityProperties;

        //

        private MySqlCommand cmdInsertZone;
        private MySqlCommand cmdEditZone;
        private MySqlCommand cmdDeleteZone;

        private MySqlCommand cmdGetZoneById;
        private MySqlCommand cmdGetZoneByName;

        //

        private MySqlCommand cmdInsertChannel;
        private MySqlCommand cmdEditChannel;
        private MySqlCommand cmdDeleteChannel;
        private MySqlCommand cmdGetChannels;

        private MySqlCommand cmdGetChannelById;
        private MySqlCommand cmdGetChannelByName;

        //

        private const int COMMAND_TIMEOUT = 5; // TODO: [LOW] do we need a parameter for timeout seconds?

        #endregion

        #region Constructors
        public DalProvider(ServerContext context)
        {
            this.context = context;

            #region Characters
            // Command for adding new characters.
            cmdInsertCharacter = new MySqlCommand();
            cmdInsertCharacter.CommandText = "INSERT INTO characters (account_id,name,zone,last_played,created,time_played) VALUES (@accountId, @name, @zone, NULL, @datenow, 0)";
            cmdInsertCharacter.CommandTimeout = COMMAND_TIMEOUT;
            cmdInsertCharacter.Parameters.Add("@accountId", MySqlDbType.Int32, 5);
            cmdInsertCharacter.Parameters.Add("@name", MySqlDbType.String, 50);
            cmdInsertCharacter.Parameters.Add("@zone", MySqlDbType.String, 45);
            cmdInsertCharacter.Parameters.Add("@datenow", MySqlDbType.DateTime, 4);

            // Command for editing characters.
            cmdEditCharacter = new MySqlCommand();
            cmdEditCharacter.CommandText = "UPDATE characters SET name = @name, zone = @zone, last_played = @lastplayed, time_played = @timeplayed WHERE account_id = @accountId AND character_id = @characterId";
            cmdEditCharacter.CommandTimeout = COMMAND_TIMEOUT;
            cmdEditCharacter.Parameters.Add("@accountId", MySqlDbType.Int32, 4);
            cmdEditCharacter.Parameters.Add("@characterId", MySqlDbType.Int32, 4);
            cmdEditCharacter.Parameters.Add("@name", MySqlDbType.String, 50);
            cmdEditCharacter.Parameters.Add("@zone", MySqlDbType.Int32, 10);
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
            cmdGetCharacterById.CommandText = "SELECT character_id, name, zone, last_played, created, time_played FROM characters WHERE account_id = @accountId AND character_id = @characterId";
            cmdGetCharacterById.CommandTimeout = COMMAND_TIMEOUT;
            cmdGetCharacterById.Parameters.Add("@accountId", MySqlDbType.Int32, 4);
            cmdGetCharacterById.Parameters.Add("@characterId", MySqlDbType.Int32, 4);

            // Command for loading characters by name.
            cmdGetCharacterByName = new MySqlCommand();
            cmdGetCharacterByName.CommandText = "SELECT character_id, name, zone, last_played, created, time_played FROM  characters WHERE account_id = @accountId AND name = @name";
            cmdGetCharacterByName.CommandTimeout = COMMAND_TIMEOUT;
            cmdGetCharacterByName.Parameters.Add("@accountId", MySqlDbType.Int32, 4);
            cmdGetCharacterByName.Parameters.Add("@name", MySqlDbType.String, 50);

            // Command for retrieving account characters.
            cmdGetCharacters = new MySqlCommand();
            cmdGetCharacters.CommandText = "SELECT account_id, character_id, name, zone, last_played, time_played FROM characters WHERE account_id = @accountId";
            cmdGetCharacters.CommandTimeout = COMMAND_TIMEOUT;
            cmdGetCharacters.Parameters.Add("@accountId", MySqlDbType.Int32, 4);

            //Command for inserting character attributes.
            cmdInsertCharacterAttribute = new MySqlCommand();
            cmdInsertCharacterAttribute.CommandText = "INSERT INTO character_attributes (character_id,attribute,value,description) VALUES (@characterId, @attributeName, @attributeValue, @attributeDescription)";
            cmdInsertCharacterAttribute.CommandTimeout = COMMAND_TIMEOUT;
            cmdInsertCharacterAttribute.Parameters.Add("@characterId", MySqlDbType.Int32, 4);
            cmdInsertCharacterAttribute.Parameters.Add("@attributeName", MySqlDbType.String, 15);
            cmdInsertCharacterAttribute.Parameters.Add("@attributeValue", MySqlDbType.Int32, 4);
            cmdInsertCharacterAttribute.Parameters.Add("@attributeDescription", MySqlDbType.String, 50);

            // Command for editing character attributes.
            cmdEditCharacterAttribute = new MySqlCommand();
            cmdEditCharacterAttribute.CommandText = "UPDATE character_attributes SET value = @attributeValue, description = @attributeDescription WHERE character_id = @characterId AND attribute = @attributeName";
            cmdEditCharacterAttribute.CommandTimeout = COMMAND_TIMEOUT;
            cmdEditCharacterAttribute.Parameters.Add("@characterId", MySqlDbType.Int32, 4);
            cmdEditCharacterAttribute.Parameters.Add("@attributeName", MySqlDbType.String, 15);
            cmdEditCharacterAttribute.Parameters.Add("@attributeValue", MySqlDbType.Int32, 4);
            cmdEditCharacterAttribute.Parameters.Add("@attributeDescription", MySqlDbType.String, 50);

            // Command for fetching all character attributes.
            cmdGetCharacterAttributes = new MySqlCommand();
            cmdGetCharacterAttributes.CommandText = "SELECT * FROM character_attributes WHERE character_id = @characterId";
            cmdGetCharacterAttributes.CommandTimeout = COMMAND_TIMEOUT;
            cmdGetCharacterAttributes.Parameters.Add("@characterId", MySqlDbType.Int32, 4);

            //Command for inserting character properties.
            cmdInsertCharacterProperty = new MySqlCommand();
            cmdInsertCharacterProperty.CommandText = "INSERT INTO character_properties (character_id,property,value,description) VALUES (@characterId, @propertyName, @propertyValue, @propertyDescription)";
            cmdInsertCharacterProperty.CommandTimeout = COMMAND_TIMEOUT;
            cmdInsertCharacterProperty.Parameters.Add("@characterId", MySqlDbType.Int32, 4);
            cmdInsertCharacterProperty.Parameters.Add("@propertyName", MySqlDbType.String, 15);
            cmdInsertCharacterProperty.Parameters.Add("@propertyValue", MySqlDbType.String, 500);
            cmdInsertCharacterProperty.Parameters.Add("@propertyDescription", MySqlDbType.String, 50);

            // Command for editing character properties.
            cmdEditCharacterProperty = new MySqlCommand();
            cmdEditCharacterProperty.CommandText = "UPDATE character_properties SET value = @propertyValue, description = @propertyDescription WHERE character_id = @characterId AND property = @propertyName";
            cmdEditCharacterProperty.CommandTimeout = COMMAND_TIMEOUT;
            cmdEditCharacterProperty.Parameters.Add("@characterId", MySqlDbType.Int32, 4);
            cmdEditCharacterProperty.Parameters.Add("@propertyName", MySqlDbType.String, 15);
            cmdEditCharacterProperty.Parameters.Add("@propertyValue", MySqlDbType.String, 500);
            cmdEditCharacterProperty.Parameters.Add("@propertyDescription", MySqlDbType.String, 50);

            // Command for fetching all character properties.
            cmdGetCharacterProperties = new MySqlCommand();
            cmdGetCharacterProperties.CommandText = "SELECT * FROM character_properties WHERE character_id = @characterId";
            cmdGetCharacterProperties.CommandTimeout = COMMAND_TIMEOUT;
            cmdGetCharacterProperties.Parameters.Add("@characterId", MySqlDbType.Int32, 4);
            #endregion

            #region Entities
            // Command for adding new entities.
            cmdInsertEntity = new MySqlCommand();
            cmdInsertEntity.CommandText = "INSERT INTO entities (entity_id, type_id, zone_id, position, created, owner_id) VALUES (@entityId, @typeId, @zoneId, @position, @datenow, @ownerId)";
            cmdInsertEntity.CommandTimeout = COMMAND_TIMEOUT;
            cmdInsertEntity.Parameters.Add("@entityId", MySqlDbType.Int32, 10);
            cmdInsertEntity.Parameters.Add("@typeId", MySqlDbType.Int32, 10);
            cmdInsertEntity.Parameters.Add("@zoneId", MySqlDbType.Int32, 10);
            cmdInsertEntity.Parameters.Add("@position", MySqlDbType.String, 45);
            cmdInsertEntity.Parameters.Add("@datenow", MySqlDbType.DateTime, 4);
            cmdInsertEntity.Parameters.Add("@ownerId", MySqlDbType.Int32, 10);

            // Command for editing entities.
            cmdEditEntity = new MySqlCommand();
            cmdEditEntity.CommandText = "UPDATE entities SET type_id = @typeId, zone_id = @zoneId, position = @position, owner_id = @ownerId WHERE entity_id = @entityId";
            cmdEditEntity.CommandTimeout = COMMAND_TIMEOUT;
            cmdEditEntity.Parameters.Add("@entityId", MySqlDbType.Int32, 10);
            cmdEditEntity.Parameters.Add("@typeId", MySqlDbType.Int32, 10);
            cmdEditEntity.Parameters.Add("@zoneId", MySqlDbType.Int32, 10);
            cmdEditEntity.Parameters.Add("@position", MySqlDbType.String, 45);
            cmdEditEntity.Parameters.Add("@ownerId", MySqlDbType.Int32, 10);

            // Command for deleting entities.
            cmdDeleteEntity = new MySqlCommand();
            cmdDeleteEntity.CommandText = "DELETE FROM entities WHERE entity_id = @entityId; DELETE FROM entity_attributes WHERE entity_id = @entityId; DELETE FROM entity_properties WHERE entity_id = @entityId;";
            cmdDeleteEntity.CommandTimeout = COMMAND_TIMEOUT;
            cmdDeleteEntity.Parameters.Add("@entityId", MySqlDbType.Int32, 10);

            // Command for retrieving zone entities.
            cmdGetEntities = new MySqlCommand();
            cmdGetEntities.CommandText = "SELECT entity_id, type_id, position, created, owner_id FROM entities WHERE zone_id = @zoneId";
            cmdGetEntities.CommandTimeout = COMMAND_TIMEOUT;
            cmdGetEntities.Parameters.Add("@zoneId", MySqlDbType.Int32, 10);

            // Command for loading entities by ID.
            cmdGetEntityById = new MySqlCommand();
            cmdGetEntityById.CommandText = "SELECT entity_id, type_id, zone_id, position, created, owner_id FROM entities WHERE entity_id = @entityId";
            cmdGetEntityById.CommandTimeout = COMMAND_TIMEOUT;
            cmdGetEntityById.Parameters.Add("@entityId", MySqlDbType.Int32, 10);

            // Command for loading entities by character ID.
            cmdGetEntityByOwner = new MySqlCommand();
            cmdGetEntityByOwner.CommandText = "SELECT entity_id, type_id, zone_id, position, created, owner_id FROM entities WHERE owner_id = @ownerId";
            cmdGetEntityByOwner.CommandTimeout = COMMAND_TIMEOUT;
            cmdGetEntityByOwner.Parameters.Add("@ownerId", MySqlDbType.Int32, 10);

            //Command for inserting entity attributes.
            cmdInsertEntityAttribute = new MySqlCommand();
            cmdInsertEntityAttribute.CommandText = "INSERT INTO entity_attributes (entity_id, attribute, value, description) VALUES (@entityId, @attributeName, @attributeValue, @attributeDescription)";
            cmdInsertEntityAttribute.CommandTimeout = COMMAND_TIMEOUT;
            cmdInsertEntityAttribute.Parameters.Add("@entityId", MySqlDbType.Int32, 10);
            cmdInsertEntityAttribute.Parameters.Add("@attributeName", MySqlDbType.String, 15);
            cmdInsertEntityAttribute.Parameters.Add("@attributeValue", MySqlDbType.Int32, 10);
            cmdInsertEntityAttribute.Parameters.Add("@attributeDescription", MySqlDbType.String, 50);

            // Command for editing character attributes.
            cmdEditEntityAttribute = new MySqlCommand();
            cmdEditEntityAttribute.CommandText = "UPDATE entity_attributes SET value = @attributeValue, description = @attributeDescription WHERE entity_id = @entityId AND attribute = @attributeName";
            cmdEditEntityAttribute.CommandTimeout = COMMAND_TIMEOUT;
            cmdEditEntityAttribute.Parameters.Add("@entityId", MySqlDbType.Int32, 10);
            cmdEditEntityAttribute.Parameters.Add("@attributeName", MySqlDbType.String, 15);
            cmdEditEntityAttribute.Parameters.Add("@attributeValue", MySqlDbType.Int32, 10);
            cmdEditEntityAttribute.Parameters.Add("@attributeDescription", MySqlDbType.String, 50);

            // Command for fetching all entity attributes.
            cmdGetEntityAttributes = new MySqlCommand();
            cmdGetEntityAttributes.CommandText = "SELECT * FROM entity_attributes WHERE entity_id = @entityId";
            cmdGetEntityAttributes.CommandTimeout = COMMAND_TIMEOUT;
            cmdGetEntityAttributes.Parameters.Add("@entityId", MySqlDbType.Int32, 10);

            //Command for inserting entity properties.
            cmdInsertEntityProperty = new MySqlCommand();
            cmdInsertEntityProperty.CommandText = "INSERT INTO entity_properties (entity_id, property, value, description) VALUES (@entityId, @propertyName, @propertyValue, @propertyDescription)";
            cmdInsertEntityProperty.CommandTimeout = COMMAND_TIMEOUT;
            cmdInsertEntityProperty.Parameters.Add("@entityId", MySqlDbType.Int32, 10);
            cmdInsertEntityProperty.Parameters.Add("@propertyName", MySqlDbType.String, 15);
            cmdInsertEntityProperty.Parameters.Add("@propertyValue", MySqlDbType.String, 500);
            cmdInsertEntityProperty.Parameters.Add("@propertyDescription", MySqlDbType.String, 50);

            // Command for editing entity properties.
            cmdEditEntityProperty = new MySqlCommand();
            cmdEditEntityProperty.CommandText = "UPDATE entity_properties SET value = @propertyValue, description = @propertyDescription WHERE entity_id = @entityId AND property = @propertyName";
            cmdEditEntityProperty.CommandTimeout = COMMAND_TIMEOUT;
            cmdEditEntityProperty.Parameters.Add("@entityId", MySqlDbType.Int32, 4);
            cmdEditEntityProperty.Parameters.Add("@propertyName", MySqlDbType.String, 15);
            cmdEditEntityProperty.Parameters.Add("@propertyValue", MySqlDbType.String, 500);
            cmdEditEntityProperty.Parameters.Add("@propertyDescription", MySqlDbType.String, 50);

            // Command for fetching all entity properties.
            cmdGetEntityProperties = new MySqlCommand();
            cmdGetEntityProperties.CommandText = "SELECT * FROM entity_properties WHERE entity_id = @entityId";
            cmdGetEntityProperties.CommandTimeout = COMMAND_TIMEOUT;
            cmdGetEntityProperties.Parameters.Add("@entityId", MySqlDbType.Int32, 4);
            #endregion

            #region Zones
            // Command for adding new zones.
            cmdInsertZone = new MySqlCommand();
            cmdInsertZone.CommandText = "INSERT INTO zones (name, width, height) VALUES (@name, @width, @height)";
            cmdInsertZone.CommandTimeout = COMMAND_TIMEOUT;
            cmdInsertZone.Parameters.Add("@name", MySqlDbType.String, 50);
            cmdInsertZone.Parameters.Add("@width", MySqlDbType.Int32, 10);
            cmdInsertZone.Parameters.Add("@height", MySqlDbType.Int32, 10);

            // Command for editing zones.
            cmdEditZone = new MySqlCommand();
            cmdEditZone.CommandText = "UPDATE zones SET name = @name, width = @width, height = @height, last_loaded = @datenow WHERE zone_id = @zoneId";
            cmdEditZone.CommandTimeout = COMMAND_TIMEOUT;
            cmdEditZone.Parameters.Add("@zoneId", MySqlDbType.Int32, 10);
            cmdEditZone.Parameters.Add("@name", MySqlDbType.String, 50);
            cmdEditZone.Parameters.Add("@width", MySqlDbType.Int32, 10);
            cmdEditZone.Parameters.Add("@height", MySqlDbType.Int32, 10);
            cmdEditZone.Parameters.Add("@datenow", MySqlDbType.DateTime, 4);

            // Command for deleting zones.
            cmdDeleteZone = new MySqlCommand();
            cmdDeleteZone.CommandText = "DELETE FROM zones WHERE zone_id = @zoneId; DELETE FROM zone_attributes WHERE zone_id = @zoneId; DELETE FROM zone_properties WHERE zone_id = @zoneId;";
            cmdDeleteZone.CommandTimeout = COMMAND_TIMEOUT;
            cmdDeleteZone.Parameters.Add("@zoneId", MySqlDbType.Int32, 10);

            // Command for loading zones by ID.
            cmdGetZoneById = new MySqlCommand();
            cmdGetZoneById.CommandText = "SELECT zone_id, name, width, height, last_loaded FROM zones WHERE zone_id = @zoneId";
            cmdGetZoneById.CommandTimeout = COMMAND_TIMEOUT;
            cmdGetZoneById.Parameters.Add("@zoneId", MySqlDbType.Int32, 10);

            // Command for loading zones by name.
            cmdGetZoneByName = new MySqlCommand();
            cmdGetZoneByName.CommandText = "SELECT zone_id, name, width, height, last_loaded FROM zones WHERE name = @name";
            cmdGetZoneByName.CommandTimeout = COMMAND_TIMEOUT;
            cmdGetZoneByName.Parameters.Add("@name", MySqlDbType.String, 50);
            #endregion

            #region Channels
            // Command for adding new channels.
            cmdInsertChannel = new MySqlCommand();
            cmdInsertChannel.CommandText = "INSERT INTO channels (name, type, motd, password, owner) VALUES (@name, @type, NULL, @password, @owner)";
            cmdInsertChannel.CommandTimeout = COMMAND_TIMEOUT;
            cmdInsertChannel.Parameters.Add("@name", MySqlDbType.String, 45);
            cmdInsertChannel.Parameters.Add("@type", MySqlDbType.Int32, 4);
            cmdInsertChannel.Parameters.Add("@password", MySqlDbType.String, 45);
            cmdInsertChannel.Parameters.Add("@owner", MySqlDbType.Int32, 10);

            // Command for editing channels.
            cmdEditChannel = new MySqlCommand();
            cmdEditChannel.CommandText = "UPDATE channels SET name = @name, type = @type, motd = @motd, password = @password, owner = @owner WHERE channel_id = @channelId";
            cmdEditChannel.CommandTimeout = COMMAND_TIMEOUT;
            cmdEditChannel.Parameters.Add("@channelId", MySqlDbType.Int32, 10);
            cmdEditChannel.Parameters.Add("@name", MySqlDbType.String, 45);
            cmdEditChannel.Parameters.Add("@type", MySqlDbType.Int32, 4);
            cmdEditChannel.Parameters.Add("@motd", MySqlDbType.Int32, 500);
            cmdEditChannel.Parameters.Add("@password", MySqlDbType.String, 45);
            cmdEditChannel.Parameters.Add("@owner", MySqlDbType.Int32, 10);

            // Command for deleting channels.
            cmdDeleteChannel = new MySqlCommand();
            cmdDeleteChannel.CommandText = "DELETE FROM channels WHERE channel_id = @channelId";
            cmdDeleteChannel.CommandTimeout = COMMAND_TIMEOUT;
            cmdDeleteChannel.Parameters.Add("@channelId", MySqlDbType.Int32, 10);

            // Command for loading channels by ID.
            cmdGetChannelById = new MySqlCommand();
            cmdGetChannelById.CommandText = "SELECT * FROM channels WHERE channel_id = @channelId";
            cmdGetChannelById.CommandTimeout = COMMAND_TIMEOUT;
            cmdGetChannelById.Parameters.Add("@channelId", MySqlDbType.Int32, 10);

            // Command for loading channels by name.
            cmdGetChannelByName = new MySqlCommand();
            cmdGetChannelByName.CommandText = "SELECT * FROM channels WHERE name = @name";
            cmdGetChannelByName.CommandTimeout = COMMAND_TIMEOUT;
            cmdGetChannelByName.Parameters.Add("@name", MySqlDbType.String, 45);

            // Command for retrieving all channels.
            cmdGetChannels = new MySqlCommand();
            cmdGetChannels.CommandText = "SELECT * FROM channels";
            cmdGetChannels.CommandTimeout = COMMAND_TIMEOUT;
            #endregion
        }
        #endregion

        #region Methods

        #region Characters
        /// <summary>
        /// Retrieves character information of all characters for an existing player (accountId).
        /// </summary>
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

                // TODO: [LOW] is this really needed for character select?
                foreach (Character ch in result)
                    LoadCompleteCharacter(ch);
            }
            return result;
        }

        /// <summary>
        /// Returns a ServerToClientMessage containing a response to a MessageType.GetCharacterList message.
        /// The character list is fetched from the DB and the message passed to the WSE before sending it to the client.
        /// </summary>
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
        /// Parses the character creation data and populates empty character with required info.
        /// </summary>
        private bool HandleCreateCharacterData(Character character, string gameData)
        {
            string[] data = gameData.Split('|');

            if (data.Length == 2)
            {
                EntityData ed = DataManager.Singletone.GetEntityData((int)EntityType.Player);

                // Invalid typeId.
                if (ed == null)
                    return false;

                // Empty or blank name.
                if (string.IsNullOrEmpty(data[1]) || string.IsNullOrWhiteSpace(data[1]))
                    return false;

                foreach (KeyValuePair<string, int> kvp in ed.Attributes)
                    character.SetAttribute(kvp.Key, kvp.Value);

                foreach (KeyValuePair<string, string> kvp in ed.Properties)
                    character.SetProperty(kvp.Key, kvp.Value);

                // Here we add all props used in the client.
                character.Name = data[1];
                character.TypeId = (int)EntityType.Player;
                character.Position = (Point)DataManager.Singletone.WorldParameters["PlayerStartPoint"];
                character.Gold = 0;

                // TODO: [MID] generate a new zone for the new player.
                character.Zone = Convert.ToInt32(DataManager.Singletone.WorldParameters["PlayerStartZone"]);

                return true;
            }
            return false;
        }

        /// <summary>
        /// Creates an empty character, sends it with the client data to the GP for initialization,
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
                if (HandleCreateCharacterData(ch, msg.GameData))
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
                            cmdInsertCharacter.Parameters["@zone"].Value = ch.Zone;
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

                    Logger.Output(this, "Account ID {0} created character: '{1}' ID: {2}.", msg.AccountId, ch.Name, ch.CharacterId);

                    // Save the rest of the character to the DB so the attributes and properties are saved.
                    InsertCharacterAttributes(ch);

                    wm.Data = "CREATE";
                    wm.MessageType = MessageType.S2C_CreateSuccess;
                }
                else
                {
                    Logger.Output(this, "Invalid character creation data! Data ('{0}').", msg.GameData);
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

                        Logger.Output(this, "Account ID {0} deleted character ID: {1}.", msg.AccountId, characterId);
                    }

                    wm.Data = "DELETE";
                    wm.MessageType = MessageType.S2C_DeleteSuccess;
                }
            }
            catch (Exception ex)
            {
                Logger.Output(this, "DeleteCharacter() exception: {0}, stacktrace: {1}", ex.Message, ex.StackTrace);
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
                        character.Zone = DatabaseHelper.GetInt32(dr, "zone");
                        character.Created = DatabaseHelper.GetDateTime(dr, "created");
                        character.TimePlayed = DatabaseHelper.GetInt32(dr, "time_played");
                    }
                    dr.Close();

                    lock (cmdGetCharacterAttributes)
                    {
                        cmdGetCharacterAttributes.Connection = conn;
                        cmdGetCharacterAttributes.Parameters["@characterId"].Value = character.CharacterId;
                        dr = cmdGetCharacterAttributes.ExecuteReader();
                    }

                    while (dr.Read())
                    {
                        // Get attribute name and value.
                        string name = DatabaseHelper.GetString(dr, "attribute");
                        int value = DatabaseHelper.GetInt32(dr, "value");
                        string description = DatabaseHelper.GetString(dr, "description");
                        character.SetAttribute(name, value, description);
                    }
                    dr.Close();

                    lock (cmdGetCharacterProperties)
                    {
                        cmdGetCharacterProperties.Connection = conn;
                        cmdGetCharacterProperties.Parameters["@characterId"].Value = character.CharacterId;
                        dr = cmdGetCharacterProperties.ExecuteReader();
                    }

                    while (dr.Read())
                    {
                        // Get property name and value.
                        string name = DatabaseHelper.GetString(dr, "property");
                        string value = DatabaseHelper.GetString(dr, "value");
                        string description = DatabaseHelper.GetString(dr, "description");
                        character.SetProperty(name, value, description);
                    }
                    dr.Close();
                }

                character.LoginTime = DateTime.Now;
                character.LoadData();
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
                character.PrepareData();
                using (MySqlConnection conn = new MySqlConnection(context.Css))
                {
                    conn.Open();

                    lock (cmdEditCharacter)
                    {
                        cmdEditCharacter.Connection = conn;
                        cmdEditCharacter.Parameters["@characterId"].Value = character.CharacterId;
                        cmdEditCharacter.Parameters["@accountId"].Value = character.AccountId;
                        cmdEditCharacter.Parameters["@name"].Value = character.Name;
                        cmdEditCharacter.Parameters["@zone"].Value = character.Zone;
                        cmdEditCharacter.Parameters["@lastplayed"].Value = DateTime.Now;
                        cmdEditCharacter.Parameters["@timeplayed"].Value = character.TimePlayed + (DateTime.Now - character.LoginTime).Seconds;
                        cmdEditCharacter.ExecuteNonQuery();
                    }

                    lock (cmdEditCharacterAttribute)
                    {
                        cmdEditCharacterAttribute.Connection = conn;
                        cmdEditCharacterAttribute.Parameters["@characterId"].Value = character.CharacterId;

                        foreach (KeyValuePair<string, EntityAttribute> kvp in character.Attributes)
                        {
                            cmdEditCharacterAttribute.Parameters["@attributeName"].Value = kvp.Value.Name;
                            cmdEditCharacterAttribute.Parameters["@attributeValue"].Value = kvp.Value.Value;
                            cmdEditCharacterAttribute.Parameters["@attributeDescription"].Value = kvp.Value.Description;
                            cmdEditCharacterAttribute.ExecuteNonQuery();
                        }
                    }

                    lock (cmdEditCharacterProperty)
                    {
                        cmdEditCharacterProperty.Connection = conn;
                        cmdEditCharacterProperty.Parameters["@characterId"].Value = character.CharacterId;

                        foreach (KeyValuePair<string, EntityProperty> kvp in character.Properties)
                        {
                            cmdEditCharacterProperty.Parameters["@propertyName"].Value = kvp.Value.Name;
                            cmdEditCharacterProperty.Parameters["@propertyValue"].Value = kvp.Value.Value;
                            cmdEditCharacterProperty.Parameters["@propertyDescription"].Value = kvp.Value.Description;
                            cmdEditCharacterProperty.ExecuteNonQuery();
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
                character.PrepareData();
                using (MySqlConnection conn = new MySqlConnection(context.Css))
                {
                    conn.Open();

                    lock (cmdInsertCharacterAttribute)
                    {
                        cmdInsertCharacterAttribute.Connection = conn;
                        cmdInsertCharacterAttribute.Parameters["@characterId"].Value = character.CharacterId;

                        foreach (KeyValuePair<string, EntityAttribute> kvp in character.Attributes)
                        {
                            cmdInsertCharacterAttribute.Parameters["@attributeName"].Value = kvp.Value.Name;
                            cmdInsertCharacterAttribute.Parameters["@attributeValue"].Value = kvp.Value.Value;
                            cmdInsertCharacterAttribute.Parameters["@attributeDescription"].Value = kvp.Value.Description;

                            // In case the attribute exists.. for whatever reason
                            try
                            {
                                cmdInsertCharacterAttribute.ExecuteNonQuery();
                            }
                            catch (Exception ex)
                            {
                                Logger.Output(this, "InsertCharacterAttributes() exception: {0}, stacktrace: {1}", ex.Message, ex.StackTrace);
                            }
                        }
                    }

                    lock (cmdInsertCharacterProperty)
                    {
                        cmdInsertCharacterProperty.Connection = conn;
                        cmdInsertCharacterProperty.Parameters["@characterId"].Value = character.CharacterId;

                        foreach (KeyValuePair<string, EntityProperty> kvp in character.Properties)
                        {
                            cmdInsertCharacterProperty.Parameters["@propertyName"].Value = kvp.Value.Name;
                            cmdInsertCharacterProperty.Parameters["@propertyValue"].Value = kvp.Value.Value;
                            cmdInsertCharacterProperty.Parameters["@propertyDescription"].Value = kvp.Value.Description;

                            // In case the property exists.. for whatever reason
                            try
                            {
                                cmdInsertCharacterProperty.ExecuteNonQuery();
                            }
                            catch (Exception ex)
                            {
                                Logger.Output(this, "InsertCharacterAttributes() exception: {0}, stacktrace: {1}", ex.Message, ex.StackTrace);
                            }
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
                    c.TypeId = (int)EntityType.Player;

                    c.Id = GameProcessor.GenerateEntityID();

                    // Update states.
                    context.Mpt.UpdateCharacterId(state[0], state[1]);
                    mpr.State = ClientState.InWorld;
                    mpr.pCharacter = c;
                    
                    // Check if the zone is loaded already.
                    if (!context.ZoneManager.ZoneExists(c.Zone))
                    {
                        // If not then load it.
                        context.ZoneManager.LoadZone(c.Zone);
                        Logger.Output(this, "LoadPlayerCharacter() zone didn't exist.. Loaded.");
                    }
                    Zone zone = context.ZoneManager.GetZone(c.Zone);

                    ServerToClientMessage wm = ServerToClientMessage.CreateMessageSafe(mpr.Sender);
#if DEBUG
                    if (mpr.Sender == null)
                        throw (new Exception("Character has NULL sender, this will crash the MessageSender."));
#endif
                    wm.TurnNumber = -1;
                    wm.MessageType = MessageType.S2C_StartSuccess;

                    if (zone != null)
                    {
                        // We save the zone info here into the character so the client gets
                        // an initial picture of the zone before the zone message hits.
                        StringBuilder sb = new StringBuilder();
                        sb.Append(zone.Id);
                        sb.Append(";");
                        sb.Append(zone.Name);
                        sb.Append(";");
                        sb.Append(zone.Width);
                        sb.Append(";");
                        sb.Append(zone.Height);
                        c.SetProperty("zoneinfo", sb.ToString());
                    }
                    else
                        throw (new Exception("Zone is null when it should be loaded either previously or by now."));

                    // Store colon separated character data list.
                    wm.Data = c.ToFormatString();

                    wm.Code = c.CharacterId;
                    wm.DeliveryMethod = NetDeliveryMethod.ReliableOrdered;
                    QueueManager.WorldMessageQueueReadWrite = wm;

                    // Tell the game processor to start the character.
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

        #region Entities
        private bool HandleCreateEntityData(Entity entity, string gameData)
        {
            string[] data = gameData.Split(':');

            if (data.Length > 2)
            {
                int typeId = int.Parse(data[0]);
                EntityData ed = DataManager.Singletone.GetEntityData(typeId);

                // Invalid typeId.
                if (ed == null)
                    return false;

                // Invalid position format. (x,y)
                if (data[2].Split(',').Length < 2)
                    return false;

                foreach (KeyValuePair<string, int> kvp in ed.Attributes)
                    entity.SetAttribute(kvp.Key, kvp.Value);

                foreach (KeyValuePair<string, string> kvp in ed.Properties)
                    entity.SetProperty(kvp.Key, kvp.Value);

                // Here we add all props used in the client.
                entity.Id = GameProcessor.GenerateEntityID();
                entity.TypeId = typeId;
                entity.Name = ed.Name;
                entity.Zone = Convert.ToInt32(data[1]);
                entity.Position = (Point)data[2];

                if (data.Length == 4)
                    entity.Owner = Convert.ToInt32(data[3]);

                return true;
            }
            return false;
        }

        /// <summary>
        /// Creates an empty entity, adds default attributes to it, then finally saves it.
        /// </summary>
        public Entity CreateEntity(string data)
        {
            Entity entity = new Entity();
            try
            {
                // Pass empty char to character manager for initialization (add attributes and properties).
                if (HandleCreateEntityData(entity, data))
                {
                    using (MySqlConnection conn = new MySqlConnection(context.Css))
                    {
                        conn.Open();

                        // Insert new character into the DB.
                        lock (cmdInsertEntity)
                        {
                            cmdInsertEntity.Connection = conn;
                            cmdInsertEntity.Parameters["@entityId"].Value = entity.Id;
                            cmdInsertEntity.Parameters["@typeId"].Value = entity.TypeId;
                            cmdInsertEntity.Parameters["@zoneId"].Value = entity.Zone;
                            cmdInsertEntity.Parameters["@position"].Value = entity.Position.ToString();
                            cmdInsertEntity.Parameters["@datenow"].Value = DateTime.Now;

                            if (((Entity)entity).Owner != -1)
                                cmdInsertEntity.Parameters["@ownerId"].Value = entity.Owner;

                            cmdInsertEntity.ExecuteNonQuery();
                        }
                    }

                    Logger.Output(this, "Entity ID: {0} created ({1}).", entity.Id, entity.Name);

                    // Save the rest of the entity to the DB so the attributes and properties are saved.
                    InsertEntityAttributes(entity);
                    return entity;
                }
                else
                {
                    Logger.Output(this, "Entity creation data was invalid! Data ('{0}').", data);
                }
            }
            catch (Exception ex)
            {
                Logger.Output(this, "CreateEntity() exception: {0}, stacktrace: {1}", ex.Message, ex.StackTrace);
            }
            return null;
        }

        /// <summary>
        /// Deletes an entity from the database.
        /// </summary>
        public bool DeleteEntity(int entityId)
        {
            int entity = -1;
            try
            {
                using (MySqlConnection conn = new MySqlConnection(context.Css))
                {
                    conn.Open();

                    // Check if the entity exists.
                    lock (cmdGetEntityById)
                    {
                        cmdGetEntityById.Connection = conn;
                        cmdGetEntityById.Parameters["@entityId"].Value = entityId;
                        object obj = cmdGetEntityById.ExecuteScalar();

                        if (obj != null)
                            entity = (int)obj;
                    }

                    // If entityId matches delete all.
                    if (entity != -1)
                    {
                        lock (cmdDeleteEntity)
                        {
                            cmdDeleteEntity.Connection = conn;
                            cmdDeleteEntity.Parameters["@entityId"].Value = entityId;
                            cmdDeleteEntity.ExecuteNonQuery();
                        }

                        Logger.Output(this, "Entity ID {0} deleted.", entity);
                        return true;
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.Output(this, "DalProvider.DeleteEntity() exception: {0}, stacktrace: {1}", ex.Message, ex.StackTrace);
            }
            return false;
        }

        /// <summary>
        /// Loads all entities from a zone and adds them to the zone.
        /// Note: the zone is expected to have a valid zone ID.
        /// </summary>
        public void LoadZoneEntities(Zone zone)
        {
            int zoneId = -1;
            try
            {
                MySqlDataReader dr;
                using (MySqlConnection conn = new MySqlConnection(context.Css))
                {
                    conn.Open();

                    // Check if the zone exists.
                    lock (cmdGetZoneById)
                    {
                        cmdGetZoneById.Connection = conn;
                        cmdGetZoneById.Parameters["@zoneId"].Value = zone.Id;
                        object obj = cmdGetZoneById.ExecuteScalar();

                        if (obj != null)
                            zoneId = (int)obj;
                    }

                    // If zone exists then load all entities from db.
                    if (zoneId != -1)
                    {
                        lock (cmdGetEntities)
                        {
                            cmdGetEntities.Connection = conn;
                            cmdGetEntities.Parameters["@zoneId"].Value = zone.Id;
                            dr = cmdGetEntities.ExecuteReader();
                        }

                        // Compile a list of entities.
                        Dictionary<int, int> entities = new Dictionary<int, int>();
                        while (dr.Read())
                        {
                            int entityId = DatabaseHelper.GetInt32(dr, "entity_id"); ;
                            int typeId = DatabaseHelper.GetInt32(dr, "type_id");
                            entities.Add(entityId, typeId);
                        }
                        dr.Close();

                        // Load each entity from the db and initialize it.
                        foreach (KeyValuePair<int, int> kvp in entities)
                        {
                            int entityId = kvp.Key;
                            int typeId = kvp.Value;
                            if (typeId == (int)EntityType.Unit)
                            {
                                Unit e = new Unit();
                                e.Id = entityId;
                                LoadEntity(e);
                                e.LoadData();
                                zone.AddEntity(e);
                            }
                            else if (typeId == (int)EntityType.EnergyStation)
                            {
                                EnergyStation e = new EnergyStation();
                                e.Id = entityId;
                                LoadEntity(e);
                                e.LoadData();
                                zone.AddEntity(e);
                            }
                            else if (typeId == (int)EntityType.EnergyRelay)
                            {
                                EnergyRelay e = new EnergyRelay();
                                e.Id = entityId;
                                LoadEntity(e);
                                e.LoadData();
                                zone.AddEntity(e);
                            }
                            else if (typeId == (int)EntityType.MineralMiner)
                            {
                                MineralMiner e = new MineralMiner();
                                e.Id = entityId;
                                LoadEntity(e);
                                e.LoadData();
                                zone.AddEntity(e);
                            }
                            else if (typeId == (int)EntityType.BasicLaser)
                            {
                                BasicLaser e = new BasicLaser();
                                e.Id = entityId;
                                LoadEntity(e);
                                e.LoadData();
                                zone.AddEntity(e);
                            }
                            else if (typeId == (int)EntityType.PulseLaser)
                            {
                                PulseLaser e = new PulseLaser();
                                e.Id = entityId;
                                LoadEntity(e);
                                e.LoadData();
                                zone.AddEntity(e);
                            }
                            else if (typeId == (int)EntityType.TacticalLaser)
                            {
                                TacticalLaser e = new TacticalLaser();
                                e.Id = entityId;
                                LoadEntity(e);
                                e.LoadData();
                                zone.AddEntity(e);
                            }
                            else if (typeId == (int)EntityType.MissileLauncher)
                            {
                                MissileLauncher e = new MissileLauncher();
                                e.Id = entityId;
                                LoadEntity(e);
                                e.LoadData();
                                zone.AddEntity(e);
                            }
                            else if (typeId == (int)EntityType.Asteroid)
                            {
                                Asteroid e = new Asteroid();
                                e.Id = entityId;
                                LoadEntity(e);
                                e.LoadData();
                                zone.AddEntity(e);
                            }
                            else
                            {
                                Entity e = new Entity();
                                e.Id = entityId;
                                LoadEntity(e);
                                e.LoadData();
                                zone.AddEntity(e);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.Output(this, "LoadZoneEntities() exception: {0}, stacktrace: {1}", ex.Message, ex.StackTrace);
            }
        }

        /// <summary>
        /// Loads an entities attributes and properties.
        /// Note: entity is expected to have a valid entity ID.
        /// </summary>
        /// <param name="entity"></param>
        public void LoadEntity(Entity entity)
        {
            int entityId = -1;
            try
            {
                MySqlDataReader dr;
                using (MySqlConnection conn = new MySqlConnection(context.Css))
                {
                    conn.Open();

                    // Check if the entity exists.
                    lock (cmdGetEntityById)
                    {
                        cmdGetEntityById.Connection = conn;
                        cmdGetEntityById.Parameters["@entityId"].Value = entity.Id;
                        object obj = cmdGetEntityById.ExecuteScalar();

                        if (obj != null)
                            entityId = (int)obj;
                    }

                    // If entityId matches load it from the db.
                    if (entityId != -1)
                    {
                        lock (cmdGetEntityById)
                        {
                            cmdGetEntityById.Connection = conn;
                            cmdGetEntityById.Parameters["@entityId"].Value = entity.Id;
                            dr = cmdGetEntityById.ExecuteReader();
                        }

                        while (dr.Read())
                        {
                            entity.TypeId = DatabaseHelper.GetInt32(dr, "type_id");
                            entity.Zone = DatabaseHelper.GetInt32(dr, "zone_id");
                            entity.Position = (Point)DatabaseHelper.GetString(dr, "position");

                            int owner = DatabaseHelper.GetInt32(dr, "owner_id");
                            if (owner != -1)
                                entity.Owner = owner;

                            EntityData ed = DataManager.Singletone.GetEntityData(entity.TypeId);
                            entity.Name = ed.Name; // TODO: [LOW] is this enough? should we save name in the database too?
                        }
                        dr.Close();

                        lock (cmdGetEntityAttributes)
                        {
                            cmdGetEntityAttributes.Connection = conn;
                            cmdGetEntityAttributes.Parameters["@entityId"].Value = entity.Id;
                            dr = cmdGetEntityAttributes.ExecuteReader();
                        }

                        while (dr.Read())
                        {
                            // Get attribute name and value.
                            string name = DatabaseHelper.GetString(dr, "attribute");
                            int value = DatabaseHelper.GetInt32(dr, "value");
                            string description = DatabaseHelper.GetString(dr, "description");
                            entity.SetAttribute(name, value, description);
                        }
                        dr.Close();

                        lock (cmdGetEntityProperties)
                        {
                            cmdGetEntityProperties.Connection = conn;
                            cmdGetEntityProperties.Parameters["@entityId"].Value = entity.Id;
                            dr = cmdGetEntityProperties.ExecuteReader();
                        }

                        while (dr.Read())
                        {
                            // Get property name and value.
                            string name = DatabaseHelper.GetString(dr, "property");
                            string value = DatabaseHelper.GetString(dr, "value");
                            string description = DatabaseHelper.GetString(dr, "description");
                            entity.SetProperty(name, value, description);
                        }
                        dr.Close();
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.Output(this, "LoadEntity() exception: {0}, stacktrace: {1}", ex.Message, ex.StackTrace);
            }
        }

        /// <summary>
        /// Saves an entity with all attributes and properties.
        /// </summary>
        /// <param name="character"></param>
        public void SaveEntity(Entity entity)
        {
            try
            {
                using (MySqlConnection conn = new MySqlConnection(context.Css))
                {
                    conn.Open();

                    lock (cmdEditEntity)
                    {
                        cmdEditEntity.Connection = conn;
                        cmdEditEntity.Parameters["@entityId"].Value = entity.Id;
                        cmdEditEntity.Parameters["@typeId"].Value = entity.TypeId;
                        cmdEditEntity.Parameters["@zoneId"].Value = entity.Zone;
                        cmdEditEntity.Parameters["@position"].Value = entity.Position.ToString();
                        cmdEditEntity.Parameters["@ownerId"].Value = entity.Owner;
                        cmdEditEntity.ExecuteNonQuery();
                    }

                    lock (cmdEditEntityAttribute)
                    {
                        cmdEditEntityAttribute.Connection = conn;
                        cmdEditEntityAttribute.Parameters["@entityId"].Value = entity.Id;

                        foreach (KeyValuePair<string, EntityAttribute> kvp in entity.Attributes)
                        {
                            cmdEditEntityAttribute.Parameters["@attributeName"].Value = kvp.Value.Name;
                            cmdEditEntityAttribute.Parameters["@attributeValue"].Value = kvp.Value.Value;
                            cmdEditEntityAttribute.Parameters["@attributeDescription"].Value = kvp.Value.Description;
                            cmdEditEntityAttribute.ExecuteNonQuery();
                        }
                    }

                    lock (cmdEditEntityProperty)
                    {
                        cmdEditEntityProperty.Connection = conn;
                        cmdEditEntityProperty.Parameters["@entityId"].Value = entity.Id;

                        foreach (KeyValuePair<string, EntityProperty> kvp in entity.Properties)
                        {
                            cmdEditEntityProperty.Parameters["@propertyName"].Value = kvp.Value.Name;
                            cmdEditEntityProperty.Parameters["@propertyValue"].Value = kvp.Value.Value;
                            cmdEditEntityProperty.Parameters["@propertyDescription"].Value = kvp.Value.Description;
                            cmdEditEntityProperty.ExecuteNonQuery();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.Output(this, "SaveEntity() Exception: {0}, stacktrace: {1}", ex.Message, ex.StackTrace);
            }
        }

        /// <summary>
        /// Inserts an entities attributes and properties into the db, this is used for entity creation only.
        /// </summary>
        /// <param name="character"></param>
        public void InsertEntityAttributes(Entity entity)
        {
            try
            {
                entity.PrepareData();

                using (MySqlConnection conn = new MySqlConnection(context.Css))
                {
                    conn.Open();

                    lock (cmdInsertEntityAttribute)
                    {
                        cmdInsertEntityAttribute.Connection = conn;
                        cmdInsertEntityAttribute.Parameters["@entityId"].Value = entity.Id;

                        foreach (KeyValuePair<string, EntityAttribute> kvp in entity.Attributes)
                        {
                            cmdInsertEntityAttribute.Parameters["@attributeName"].Value = kvp.Value.Name;
                            cmdInsertEntityAttribute.Parameters["@attributeValue"].Value = kvp.Value.Value;
                            cmdInsertEntityAttribute.Parameters["@attributeDescription"].Value = kvp.Value.Description;

                            // In case the attribute exists.. for whatever reason
                            try
                            {
                                cmdInsertEntityAttribute.ExecuteNonQuery();
                            }
                            catch (Exception ex)
                            {
                                Logger.Output(this, "InsertEntityAttributes() exception: {0}, stacktrace: {1}", ex.Message, ex.StackTrace);
                            }
                        }
                    }

                    lock (cmdInsertEntityProperty)
                    {
                        cmdInsertEntityProperty.Connection = conn;
                        cmdInsertEntityProperty.Parameters["@entityId"].Value = entity.Id;

                        foreach (KeyValuePair<string, EntityProperty> kvp in entity.Properties)
                        {
                            cmdInsertEntityProperty.Parameters["@propertyName"].Value = kvp.Value.Name;
                            cmdInsertEntityProperty.Parameters["@propertyValue"].Value = kvp.Value.Value;
                            cmdInsertEntityProperty.Parameters["@propertyDescription"].Value = kvp.Value.Description;

                            // In case the property exists.. for whatever reason
                            try
                            {
                                cmdInsertEntityProperty.ExecuteNonQuery();
                            }
                            catch (Exception ex)
                            {
                                Logger.Output(this, "InsertEntityAttributes() exception: {0}, stacktrace: {1}", ex.Message, ex.StackTrace);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.Output(this, "InsertEntityAttributes() exception: {0}, stacktrace: {1}", ex.Message, ex.StackTrace);
            }
        }
        #endregion

        #region Zones
        /// <summary>
        /// Creates a zone and enters it into the database.
        /// </summary>
        /// <param name="zone"></param>
        public Zone CreateZone(string name, int width, int height)
        {
            int zone = -1;
            try
            {
                using (MySqlConnection conn = new MySqlConnection(context.Css))
                {
                    conn.Open();

                    // Check zone name doesn't exist.
                    // TODO: [LOW] do zones really need a unique name?
                    lock (cmdGetZoneByName)
                    {
                        cmdGetZoneByName.Connection = conn;
                        cmdGetZoneByName.Parameters["@name"].Value = name;
                        object obj = cmdGetZoneByName.ExecuteScalar();

                        if (obj != null)
                        {
                            Logger.Output(this, "Zone creation failed: A zone named '{0}' exists already.", name);
                            return null;
                        }
                    }

                    // Insert new channel into the DB.
                    lock (cmdInsertZone)
                    {
                        cmdInsertZone.Connection = conn;
                        cmdInsertZone.Parameters["@name"].Value = name;
                        cmdInsertZone.Parameters["@width"].Value = width;
                        cmdInsertZone.Parameters["@height"].Value = height;
                        cmdInsertZone.ExecuteNonQuery();
                    }

                    // Check character index.
                    lock (cmdGetZoneByName)
                    {
                        cmdGetZoneByName.Connection = conn;
                        cmdGetZoneByName.Parameters["@name"].Value = name;
                        object obj = cmdGetZoneByName.ExecuteScalar();

                        if (obj != null)
                            zone = (int)obj;
                    }

                    if (zone != -1)
                    {
                        Logger.Output(this, "Created zone: '{0}' ID: {1}.", name, zone);
                        Zone z = new Zone();
                        z.Initialize(zone, name, width, height);
                        return z;
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.Output(this, "CreateZone() exception: {0}, stacktrace: {1}", ex.Message, ex.StackTrace);
            }
            return null;
        }

        /// <summary>
        /// Loads a zone from the database.
        /// </summary>
        public Zone LoadZone(int zoneId)
        {
            int zone = -1;
            try
            {
                MySqlDataReader dr;
                using (MySqlConnection conn = new MySqlConnection(context.Css))
                {
                    conn.Open();

                    // Check if the zone exists.
                    lock (cmdGetZoneById)
                    {
                        cmdGetZoneById.Connection = conn;
                        cmdGetZoneById.Parameters["@zoneId"].Value = zoneId;
                        object obj = cmdGetZoneById.ExecuteScalar();

                        if (obj != null)
                            zone = (int)obj;
                    }

                    // If zone exists then load it from the db.
                    if (zone != -1)
                    {
                        Zone z = new Zone();
                        lock (cmdGetZoneById)
                        {
                            cmdGetZoneById.Connection = conn;
                            cmdGetZoneById.Parameters["@zoneId"].Value = zoneId;
                            dr = cmdGetZoneById.ExecuteReader();
                        }

                        while (dr.Read())
                        {
                            string name = DatabaseHelper.GetString(dr, "name");
                            int width = DatabaseHelper.GetInt32(dr, "width");
                            int height = DatabaseHelper.GetInt32(dr, "height");
                            //DateTime lastLoaded = DatabaseHelper.GetDateTime(dr, "last_loaded");
                            z.Initialize(zoneId, name, width, height);
                        }
                        dr.Close();

                        return z;
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.Output(this, "LoadZone() exception: {0}, stacktrace: {1}", ex.Message, ex.StackTrace);
            }
            return null;
        }

        /// <summary>
        /// Deletes a zone from the database
        /// </summary>
        public bool DeleteZone(int zoneId)
        {
            int zone = -1;
            try
            {
                using (MySqlConnection conn = new MySqlConnection(context.Css))
                {
                    conn.Open();

                    // Check if the zone exists.
                    lock (cmdGetZoneById)
                    {
                        cmdGetZoneById.Connection = conn;
                        cmdGetZoneById.Parameters["@zoneId"].Value = zoneId;
                        object obj = cmdGetZoneById.ExecuteScalar();

                        if (obj != null)
                            zone = (int)obj;
                    }

                    // If the zone is found then delete it.
                    if (zone != -1)
                    {
                        lock (cmdDeleteZone)
                        {
                            cmdDeleteZone.Connection = conn;
                            cmdDeleteZone.Parameters["@zoneId"].Value = zoneId;
                            cmdDeleteZone.ExecuteNonQuery();
                        }

                        Logger.Output(this, "Zone ID {0} deleted.", zoneId);
                        return true;
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.Output(this, "DeleteZone() exception: {0}, stacktrace: {1}", ex.Message, ex.StackTrace);
            }
            return false;
        }

        /// <summary>
        /// Saves a zone to the database.
        /// </summary>
        public void SaveZone(Zone zone)
        {
            try
            {
                using (MySqlConnection conn = new MySqlConnection(context.Css))
                {
                    conn.Open();

                    lock (cmdEditZone)
                    {
                        cmdEditZone.Connection = conn;
                        cmdEditZone.Parameters["@zoneId"].Value = zone.Id;
                        cmdEditZone.Parameters["@name"].Value = zone.Name;
                        cmdEditZone.Parameters["@width"].Value = zone.Width;
                        cmdEditZone.Parameters["@height"].Value = zone.Height;
                        cmdEditZone.Parameters["@datenow"].Value = DateTime.Now;
                        cmdEditZone.ExecuteNonQuery();                   
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.Output(this, "SaveZone() Exception: {0}, stacktrace: {1}", ex.Message, ex.StackTrace);
            }
        }
        #endregion

        #region Channels
        /// <summary>
        /// Creates a new channel instance and inserts its data into the database.
        /// </summary>
        public Channel CreateChannel(string name, int type, string password, int owner)
        {
            int channel = -1;
            try
            {
                using (MySqlConnection conn = new MySqlConnection(context.Css))
                {
                    conn.Open();

                    // Check channel name doesn't exist.
                    lock (cmdGetChannelByName)
                    {
                        cmdGetChannelByName.Connection = conn;
                        cmdGetChannelByName.Parameters["@name"].Value = name;
                        object obj = cmdGetChannelByName.ExecuteScalar();

                        if (obj != null)
                        {
                            Logger.Output(this, "Channel creation failed: A channel named '{0}' exists already.", name);
                            return null;
                        }
                    }

                    // Insert new channel into the DB.
                    lock (cmdInsertChannel)
                    {
                        cmdInsertChannel.Connection = conn;
                        cmdInsertChannel.Parameters["@name"].Value = name;
                        cmdInsertChannel.Parameters["@type"].Value = type;
                        cmdInsertChannel.Parameters["@password"].Value = password;
                        cmdInsertChannel.Parameters["@owner"].Value = owner;
                        cmdInsertChannel.ExecuteNonQuery();
                    }

                    // Check character index.
                    lock (cmdGetChannelByName)
                    {
                        cmdGetChannelByName.Connection = conn;
                        cmdGetChannelByName.Parameters["@name"].Value = name;
                        object obj = cmdGetChannelByName.ExecuteScalar();

                        if (obj != null)
                            channel = (int)obj;
                    }

                    if(channel != -1)
                    {
                        Logger.Output(this, "Created channel: '{0}' ID: {1}.", name, channel);
                        return new Channel(channel, name, type, owner);
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.Output(this, "CreateChannel() exception: {0}, stacktrace: {1}", ex.Message, ex.StackTrace);
            }
            return null;
        }

        /// <summary>
        /// Deletes a channel from the database.
        /// </summary>
        public bool DeleteChannel(int channelId)
        {
            int channel = -1;
            try
            {
                using (MySqlConnection conn = new MySqlConnection(context.Css))
                {
                    conn.Open();

                    // Check if the zone exists.
                    lock (cmdGetChannelById)
                    {
                        cmdGetChannelById.Connection = conn;
                        cmdGetChannelById.Parameters["@channelId"].Value = channelId;
                        object obj = cmdGetChannelById.ExecuteScalar();

                        if (obj != null)
                            channel = (int)obj;
                    }

                    // If the zone is found then delete it.
                    if (channel != -1)
                    {
                        lock (cmdDeleteChannel)
                        {
                            cmdDeleteChannel.Connection = conn;
                            cmdDeleteChannel.Parameters["@channelId"].Value = channelId;
                            cmdDeleteChannel.ExecuteNonQuery();
                        }

                        Logger.Output(this, "Channel ID {0} deleted.", channel);
                        return true;
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.Output(this, "DeleteChannel() exception: {0}, stacktrace: {1}", ex.Message, ex.StackTrace);
            }
            return false;
        }

        /// <summary>
        /// Loads a channel from the database.
        /// </summary>
        public Channel LoadChannel(int channelId)
        {
            int channel = -1;
            try
            {
                MySqlDataReader dr;
                using (MySqlConnection conn = new MySqlConnection(context.Css))
                {
                    conn.Open();

                    // Check if the channel exists.
                    lock (cmdGetChannelById)
                    {
                        cmdGetChannelById.Connection = conn;
                        cmdGetChannelById.Parameters["@channelId"].Value = channelId;
                        object obj = cmdGetChannelById.ExecuteScalar();

                        if (obj != null)
                            channel = (int)obj;
                    }

                    // If channel exists then load it from the db.
                    if (channel != -1)
                    {
                        Channel c = null;
                        lock (cmdGetChannelById)
                        {
                            cmdGetChannelById.Connection = conn;
                            cmdGetChannelById.Parameters["@channelId"].Value = channelId;
                            dr = cmdGetChannelById.ExecuteReader();
                        }

                        while (dr.Read())
                        {
                            string name = DatabaseHelper.GetString(dr, "name");
                            int type = DatabaseHelper.GetInt32(dr, "type");
                            string motd = DatabaseHelper.GetString(dr, "motd");
                            string password = DatabaseHelper.GetString(dr, "password");
                            int owner = DatabaseHelper.GetInt32(dr, "owner");
                            c = new Channel(channelId, name, type, owner);

                            if (!string.IsNullOrWhiteSpace(motd))
                                c.Motd = motd;

                            if (!string.IsNullOrWhiteSpace(password))
                                c.Password = password;
                        }
                        dr.Close();

                        return c;
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.Output(this, "LoadChannel() exception: {0}, stacktrace: {1}", ex.Message, ex.StackTrace);
            }
            return null;
        }

        /// <summary>
        /// Saves a channel to the database.
        /// </summary>
        public void SaveChannel(Channel channel)
        {
            try
            {
                using (MySqlConnection conn = new MySqlConnection(context.Css))
                {
                    conn.Open();
                    lock (cmdEditChannel)
                    {
                        cmdEditChannel.Connection = conn;
                        cmdEditChannel.Parameters["@channelId"].Value = channel.Id;
                        cmdEditChannel.Parameters["@name"].Value = channel.Name;
                        cmdEditChannel.Parameters["@type"].Value = channel.Type;
                        cmdEditChannel.Parameters["@motd"].Value = channel.Motd;
                        cmdEditChannel.Parameters["@password"].Value = channel.Password;
                        cmdEditChannel.Parameters["@owner"].Value = channel.Owner;
                        cmdEditChannel.ExecuteNonQuery();
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.Output(this, "SaveChannel() Exception: {0}, stacktrace: {1}", ex.Message, ex.StackTrace);
            }
        }
        #endregion

        #endregion
    }
}
