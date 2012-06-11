using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AsteriaLibrary.Entities;
using AsteriaLibrary.Math;
using AsteriaLibrary.Messages;
using AsteriaLibrary.Shared;
using AsteriaLibrary.Zones;
using AsteriaWorldServer.PlayerCache;
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
        #region Methods
        /// <summary>
        /// Formats the ServerToClientMessage with entity data.
        /// Data holds colon separated entity data.
        /// Buffer holds 5 bytes for Id(4), isChar(1).
        /// </summary>
        /// <param name="e">The entity instance to read data from.</param>
        /// <param name="wm">The ServerToClientMessage instance that will hold the data.</param>
        public static void CreateAddEntityToZoneMessage(Entity e, ServerToClientMessage wm)
        {
            wm.MessageType = MessageType.S2C_ZoneMessage;
            wm.Buffer = new byte[5];
            wm.Code = (int)PlayerAction.AddEntity;
            wm.DeliveryMethod = NetDeliveryMethod.ReliableOrdered;
            wm.DeliveryChannel = 1;

            BitConverter.GetBytes(e.Id).CopyTo(wm.Buffer, 0); // 4b: 0-3 entity Id
            wm.Buffer[4] = (byte)(e is Character ? 1 : 0); // 1b: 4 1 = character, 0 = entity

            if (e is Character)
                wm.Data = ((Character)e).ToFormatString();
            else
                wm.Data = e.ToFormatString();
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
                        sb.Append(entity.ToFormatString());
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

        /// <summary>
        /// Formats a ServerToClientMessage with InvalidAction message data.
        /// Data contains a textual description which is a WSE implementation detail.
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

        public static void CreateChatMessage(string name, int channel, int destination, string message, ServerToClientMessage wm)
        {
            if (wm.Sender == null)
                throw (new Exception("wm.Sender can't be NULL on outgoing messages."));

            wm.MessageType = MessageType.S2C_ChatMessage;
            wm.Code = (channel != 0) ? channel : destination;
            wm.DeliveryMethod = NetDeliveryMethod.ReliableOrdered;
            wm.DeliveryChannel = 0;
            wm.Data = String.Format("({0}) {1}", name, message);
        }
        #endregion
    }
}
