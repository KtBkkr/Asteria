using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AsteriaLibrary.Entities;
using AsteriaLibrary.Math;
using AsteriaLibrary.Messages;
using AsteriaLibrary.Shared;
using AsteriaWorldServer.PlayerCache;
using AsteriaWorldServer.Zones;
using Lidgren.Network;

namespace AsteriaWorldServer.Messages
{
    /// <summary>
    /// This class is used to have all message building in one place.
    /// The method used to build the messag especially the Buffer[] must be reversed on the client side.
    /// If adding new messages the Buffer must be checked and enough space allocated.
    /// </summary>
    static class MessageFormatter
    {
        #region Entities
        /// <summary>
        /// Generates an entity format string based on the type.
        /// </summary>
        public static string GenerateEntityFormat(Entity entity)
        {
            if (entity == null)
                return null;

            string format = null;
            if (entity is Character)
                format = ((Character)entity).ToFormatString();
            else if (entity is EnergyStation)
                format = ((EnergyStation)entity).ToFormatString();
            else if (entity is EnergyRelay)
                format = ((EnergyRelay)entity).ToFormatString();
            else if (entity is MineralMiner)
                format = ((MineralMiner)entity).ToFormatString();
            else if (entity is BasicLaser)
                format = ((BasicLaser)entity).ToFormatString();
            else if (entity is PulseLaser)
                format = ((PulseLaser)entity).ToFormatString();
            else if (entity is TacticalLaser)
                format = ((TacticalLaser)entity).ToFormatString();
            else if (entity is MissileLauncher)
                format = ((MissileLauncher)entity).ToFormatString();
            else if (entity is Asteroid)
                format = ((Asteroid)entity).ToFormatString();
            else if (entity is Unit)
                format = ((Unit)entity).ToFormatString();
            else
                format = entity.ToFormatString();

            return format;
        }

        /// <summary>
        /// Formats the ServerToClientMessage with entity data.
        /// Data holds colon separated entity data.
        /// Buffer holds 8 bytes for Id(4), TypeId(4)
        /// </summary>
        /// <param name="e">The entity instance to read data from.</param>
        /// <param name="wm">The ServerToClientMessage instance that will hold the data.</param>
        public static void CreateAddEntityToZoneMessage(Entity e, ServerToClientMessage wm)
        {
            wm.MessageType = MessageType.S2C_ZoneMessage;
            wm.Buffer = new byte[8];
            wm.Code = (int)PlayerAction.AddEntity;
            wm.DeliveryMethod = NetDeliveryMethod.ReliableOrdered;
            wm.DeliveryChannel = 1;

            BitConverter.GetBytes(e.Id).CopyTo(wm.Buffer, 0); // 4b: 0-3 entity Id
            BitConverter.GetBytes(e.TypeId).CopyTo(wm.Buffer, 4); // 1=character, 2=structure, so on,

            wm.Data = GenerateEntityFormat(e);
        }

        /// <summary>
        /// Formats a RemoveEntity zone message.
        /// </summary>
        /// <param name="entity">The entity to be removed.</param>
        /// <param name="wm">The ServerToClientMessage instance that will hold the data.</param>
        public static void CreateRemoveEntityFromZoneMessage(Entity entity, ServerToClientMessage wm)
        {
            wm.MessageType = MessageType.S2C_ZoneMessage;
            wm.Buffer = BitConverter.GetBytes(entity.Id);
            wm.Code = (int)PlayerAction.RemoveEntity;
            wm.DeliveryMethod = NetDeliveryMethod.ReliableUnordered;
            wm.DeliveryChannel = 0;
            wm.Data = null;
        }

        public static void CreateDamageEntityMessage(Entity to, Entity from, int amount, ServerToClientMessage wm)
        {
            wm.MessageType = MessageType.S2C_ZoneMessage;
            wm.Buffer = BitConverter.GetBytes(to.Id);
            wm.Code = (int)PlayerAction.Damage;
            wm.DeliveryMethod = NetDeliveryMethod.ReliableUnordered;
            wm.DeliveryChannel = 0;

            string data = amount.ToString();
            if (from != null)
                data = String.Format("{0}:{1}", amount, from.Id);

            wm.Data = data;
        }
        #endregion

        #region Zones
        /// <summary>
        /// Formats the ServerToClientMessage with zone data.
        /// Data holds colon separated entity data.
        /// </summary>
        /// <param name="zones">The zone instance(s) to read data from.</param>
        /// <param name="wm">The ServerToClientMessage instance that will hold the data.</param>
        public static void CreateZoneSyncMessage(IEnumerable<Zone> zones, ServerToClientMessage wm)
        {
            if (wm.Sender == null)
                throw (new Exception("wm.Sender can't be NULL on outgoing messages."));

            wm.MessageType = MessageType.S2C_ZoneMessage;
            wm.Buffer = null;
            wm.Code = (int)PlayerAction.AddZone;
            wm.DeliveryMethod = NetDeliveryMethod.ReliableOrdered;
            wm.DeliveryChannel = 1;

            const string ZONE_DELIMITER = "#Z";
            const string CHARACTER_DELIMITER = "#C";
            const string ENTITY_DELIMITER = "#E";

            StringBuilder sb = new StringBuilder();

            // Build a list of entities in zones.
            foreach (Zone zone in zones)
            {
                sb.Append(ZONE_DELIMITER);
                sb.Append(zone.Id);
                sb.Append(":");
                sb.Append(zone.Name);
                sb.Append(":");
                sb.Append(zone.Width);
                sb.Append(":");
                sb.Append(zone.Height);

                foreach (Entity entity in zone.AllEntities)
                {
                    if (entity is Character)
                    {
                        sb.Append(CHARACTER_DELIMITER);
                        sb.Append(((Character)entity).ToFormatString());
                    }
                    else
                    {
                        sb.Append(ENTITY_DELIMITER);
                        sb.Append(entity.TypeId);
                        sb.Append("|");
                        sb.Append(GenerateEntityFormat(entity));
                    }
                }
            }
            wm.Data = sb.ToString();
        }

        /// <summary>
        /// Formats a RemoveZone message.
        /// </summary>
        /// <param name="zones">The zone instance(s) to be removed from the client.</param>
        /// <param name="wm">The ServerToClientMessage instance that willhold the data.</param>
        public static void CreateRemoveZoneMessage(IEnumerable<Zone> zones, ServerToClientMessage wm)
        {
            if (wm.Sender == null)
                throw (new Exception("wm.Sender can't be NULL on outgoing messages."));

            wm.MessageType = MessageType.S2C_ZoneMessage;
            wm.Buffer = null;
            wm.Code = (int)PlayerAction.RemoveZone;
            wm.DeliveryMethod = NetDeliveryMethod.ReliableUnordered;
            wm.DeliveryChannel = 0;

            StringBuilder sb = new StringBuilder();
            foreach (Zone zone in zones)
            {
                sb.Append(zone.Id);
                sb.Append(":");
            }
            wm.Data = sb.ToString();
        }
        #endregion

        #region Character
        public static void CreateTeleportMessage(Zone newZone, ServerToClientMessage wm)
        {
            if (wm.Sender == null)
                throw (new Exception("wm.Sender can't be NULL on outgoing messages."));

            wm.MessageType = MessageType.S2C_ZoneMessage;
            wm.Buffer = null;
            wm.Code = (int)PlayerAction.Teleport;
            wm.DeliveryMethod = NetDeliveryMethod.ReliableOrdered;
            wm.DeliveryChannel = 0;

            StringBuilder sb = new StringBuilder();
            sb.Append("#Z");
            sb.Append(newZone.Id);
            sb.Append(":");
            sb.Append(newZone.Name);
            sb.Append(":");
            sb.Append(newZone.Width);
            sb.Append(":");
            sb.Append(newZone.Height);

            foreach (Entity entity in newZone.AllEntities)
            {
                if (entity is Character)
                {
                    sb.Append("#C");
                    sb.Append(((Character)entity).ToFormatString());
                }
                else
                {
                    sb.Append("#E");
                    sb.Append(entity.TypeId);
                    sb.Append("|");
                    sb.Append(GenerateEntityFormat(entity));
                }
            }
            wm.Data = sb.ToString();
        }

        /// <summary>
        /// Formats a ServerToClientMessage with logout message data.
        /// </summary>
        /// <param name="mpr">Player record of the player logging out.</param>
        /// <param name="turnNumber">Current turn number.</param>
        /// <param name="wm"></param>
        public static void CreateCharacterLogoutMessage(MasterPlayerRecord mpr, int turnNumber, ServerToClientMessage wm)
        {
            if (wm.Sender == null)
                throw (new Exception("Character has null sender, this will crash the MessageSender!"));

            if (mpr.State != ClientState.InWorld)
                wm.MessageType = mpr.LogoutCharacterGranted ? MessageType.S2C_PlayerLoggedOut : MessageType.S2C_LogoutDenied;
            else
                wm.MessageType = mpr.LogoutCharacterGranted ? MessageType.S2C_CharacterLoggedOut : MessageType.S2C_LogoutDenied;

            wm.Code = (int)PlayerAction.RemoveEntity;
            wm.Data = mpr.LogoutCharacterGranted ? "Logout OK" : "Logout denied";
            wm.TurnNumber = turnNumber;
            wm.Buffer = BitConverter.GetBytes(mpr.CharacterId);
            wm.DeliveryChannel = 0;
            wm.DeliveryMethod = NetDeliveryMethod.ReliableUnordered;
        }
        #endregion

        #region Misc.
        /// <summary>
        /// Formats a ServerToClientMessage with InvalidAction message data.
        /// Data contains a textual description which is a GP implementation detail.
        /// Buffer contains 4 bytes for the player action.
        /// </summary>
        /// <param name="action"></param>
        /// <param name="message"></param>
        /// <param name="wm"></param>
        public static void CreateInvalidActionMessage(PlayerAction action, string message, ServerToClientMessage wm)
        {
            if (wm.Sender == null)
                throw (new Exception("wm.Sender can't be NULL on outgoing messages."));

            wm.MessageType = MessageType.S2C_ZoneMessage;
            wm.Code = (int)PlayerAction.InvalidAction;
            wm.Buffer = new byte[4];
            wm.DeliveryMethod = NetDeliveryMethod.ReliableOrdered;
            wm.DeliveryChannel = 0;
            wm.Data = message;
            BitConverter.GetBytes((int)action).CopyTo(wm.Buffer, 0); // 4b: 0-3 player action.
        }

        public static void CreateChatMessage(ChatType type, string message, ServerToClientMessage wm)
        {
            if (wm.Sender == null)
                throw (new Exception("wm.Sender can't be NULL on outgoing messages."));

            wm.MessageType = MessageType.S2C_ChatMessage;
            wm.Code = (int)type;
            wm.DeliveryMethod = NetDeliveryMethod.ReliableOrdered;
            wm.DeliveryChannel = 0;
            wm.Data = message;
        }
        #endregion
    }
}
