using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AsteriaLibrary.Entities;
using AsteriaLibrary.Math;
using AsteriaLibrary.Messages;
using AsteriaLibrary.Shared;
using AsteriaLibrary.Zones;
using Lidgren.Network;
using AsteriaWorldServer.PlayerCache;

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
        /// Formats the ServerToClientMessagewith MoveEntity data.
        /// Data is empty.
        /// Buffer contains 17 bytes for Id(4), position(8), rotation(4), state(1)
        /// </summary>
        /// <param name="c">The Character instance to read data from.</param>
        /// <param name="wm">The ServerToClientMessage instance that will hold the data.</param>
        public static void CreateMoveEntityMessage(Entity c, ServerToClientMessage wm)
        {
            wm.MessageType = MessageType.S2C_ZoneMessage;
            wm.Buffer = new byte[17];
            wm.Code = (int)PlayerAction.MoveEntity;
            wm.DeliveryMethod = NetDeliveryMethod.Unreliable;
            wm.DeliveryChannel = 0;

            BitConverter.GetBytes(c.Id).CopyTo(wm.Buffer, 0); // 4b: 0-3 entity Id
            c.Position.SaveToBuffer(ref wm.Buffer, 4); // 4b: 4-11 position
            BitConverter.GetBytes(c.Rotation).CopyTo(wm.Buffer, 12); // 12b: 12-16 rotation
            wm.Buffer[17] = (byte)c.GetAttributeValue("movementoption"); // 1b: 17 movement state
            wm.Data = null;
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
                sb.Append(zone.Min.ToString());
                sb.Append(":");
                sb.Append(zone.Max.ToString());

                foreach (Entity entity in zone.Entities)
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
        /// Formats the ServerToClientMessage with InvalidMove message data.
        /// Data is empty.
        /// Buffer contains 19 bytes for Id(4), current position(8), unreachable position(8).
        /// </summary>
        /// <param name="c">Character instance to read data from.</param>
        /// <param name="invalidPosition"></param>
        /// <param name="wm">The ServerToClientMessage instance that will hold the data.</param>
        public static void CreateInvalidMoveMessage(Character c, ref Point invalidPosition, ServerToClientMessage wm)
        {
            if (wm.Sender == null)
                throw (new Exception("wm.Sender can't be NULL on outgoing messages."));

            wm.MessageType = MessageType.S2C_ZoneMessage;
            wm.Code = (int)PlayerAction.InvalidMove;
            wm.Buffer = new byte[19];
            wm.DeliveryMethod = NetDeliveryMethod.ReliableOrdered;
            wm.DeliveryChannel = 0;

            BitConverter.GetBytes(c.Id).CopyTo(wm.Buffer, 0); // 4b: 0-3 entity Id
            c.Position.SaveToBuffer(ref wm.Buffer, 4); // 4b: 4-11 position
            invalidPosition.SaveToBuffer(ref wm.Buffer, 12); // 4b: 12-19 invalid position
            wm.Data = null;
        }

        /// <summary>
        /// Formats the ServerToClientMessage with InvalidTarget message data.
        /// This is a negative server response to Attack/Pickup item messages.
        /// Data contains a textual description which is a WSE implementation detail.
        /// Buffer contains 4 bytes for the target Id(4).
        /// </summary>
        /// <param name="targetId">The requested target entity Id.</param>
        /// <param name="reason"></param>
        /// <param name="wm">The ServerToClientMessage instance that will hold the data.</param>
        public static void CreateInvalidTargetMessage(int targetId, string reason, ServerToClientMessage wm)
        {
            if (wm.Sender == null)
                throw (new Exception("wm.Sender can't be NULL on outgoing messages."));

            wm.MessageType = MessageType.S2C_ZoneMessage;
            wm.Code = (int)PlayerAction.InvalidTarget;
            wm.Buffer = new byte[4];
            wm.DeliveryMethod = NetDeliveryMethod.ReliableOrdered;
            wm.DeliveryChannel = 0;
            wm.Data = reason;
            BitConverter.GetBytes(targetId).CopyTo(wm.Buffer, 0);
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
        /// Formats a ServerToClientMessage with InventorySync message data.
        /// The inventory layout is stored in the Buffer (6 bytes).
        /// While data contains the serialized inventory bags as a pipe separated list.
        /// </summary>
        /// <param name="c"></param>
        /// <param name="wm"></param>
        public static void CreateInventorySyncMessage(Character c, ServerToClientMessage wm)
        {
            if (wm.Sender == null)
                throw (new Exception("wm.Sender can't be NULL on outgoing messages."));

            wm.MessageType = MessageType.S2C_ZoneMessage;
            wm.Code = (int)PlayerAction.InventorySync;
            wm.Buffer = new byte[6];
            wm.DeliveryMethod = NetDeliveryMethod.Unreliable;
            wm.DeliveryChannel = 0;

            wm.Buffer[0] = (byte)c.InventorySize.X; // 1b: X size
            wm.Buffer[1] = (byte)c.InventorySize.Y; // 1b: Y size
            BitConverter.GetBytes(c.InventoryItems.Count()).CopyTo(wm.Buffer, 2); // 4b: #items

            StringBuilder sb = new StringBuilder();
            foreach (IStringFormattable bag in c.InventoryItems)
            {
                sb.Append(bag.ToFormatString());
                sb.Append(":");
            }
            wm.Data = sb.ToString();
        }

        /// <summary>
        /// Formats a ServerToClientMessage with InventoryChange message data.
        /// The inventory layout is stored in the Buffer (7 bytes).
        /// While Data contains the serialized inventory bags as a pipe separated list.
        /// Note that only changed items (new or removed) are sent to the client.
        /// </summary>
        /// <param name="c"></param>
        /// <param name="changedItems"></param>
        /// <param name="wm"></param>
        public static void CreateInventoryChangeMessage(Character c, InventoryChangeType changeType, IEnumerable<InventoryBag> changedItems, ServerToClientMessage wm)
        {
            if (wm.Sender == null)
                throw (new Exception("wm.Sender can't be NULL on outgoing messages."));

            wm.MessageType = MessageType.S2C_ZoneMessage;
            wm.Code = (int)PlayerAction.InventoryChange;
            wm.Buffer = new byte[7];
            wm.DeliveryMethod = NetDeliveryMethod.Unreliable;
            wm.DeliveryChannel = 0;

            wm.Buffer[0] = (byte)changeType; // 1b: change type
            wm.Buffer[1] = (byte)c.InventorySize.X; // 1b: X size
            wm.Buffer[2] = (byte)c.InventorySize.Y; // 1b: Y size
            BitConverter.GetBytes(changedItems.Count()).CopyTo(wm.Buffer, 3); // 4b: #items

            StringBuilder sb = new StringBuilder();
            foreach (IStringFormattable bag in changedItems)
            {
                sb.Append(bag.ToFormatString());
                sb.Append(":");
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

        public static void CreateChatMessage(string name, int channel, int destination, string message, ServerToClientMessage wm)
        {
            if (wm.Sender == null)
                throw (new Exception("wm.Sender can't be NULL on outgoing messages."));

            wm.MessageType = MessageType.S2C_ChatMessage;
            wm.Code = (channel != 0) ? channel : destination;
            wm.DeliveryMethod = NetDeliveryMethod.ReliableOrdered;
            wm.DeliveryChannel = 0;
            wm.Data = String.Format("{0}|{1}", name, message);
        }
        #endregion
    }
}
