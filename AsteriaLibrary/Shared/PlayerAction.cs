using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AsteriaLibrary.Shared
{
    /// <summary>
    /// Defines player actions the world server/client must know about.
    /// Action members are also used as integers in the ServerToClientMessage.Code field for ZoneMessages.
    /// </summary>
    public enum PlayerAction
    {
        #region Client To Server
        /// <summary>
        /// C2S only (Server responds with a MoveEntity message).
        /// Position/Rotation/Stance change request.
        /// </summary>
        Move,

        /// <summary>
        /// C2S only (Server responds with a InventorySync message).
        /// A player requests to pickup an item.
        /// </summary>
        Pickup,

        /// <summary>
        /// C2S only (Server responds with a InventorySync message).
        /// A player requests to drop an item.
        /// </summary>
        Drop,

        /// <summary>
        /// C2S only (Server responds with a InventorySync message and possible additional messages).
        /// A player requests to use an item.
        /// </summary>
        Use,

        /// <summary>
        /// C2S only (Server responds with an EquipmentSync message).
        /// The player has changed equipment/clothes/mount/whatever effects the overall appearance to other clients.
        /// </summary>
        EquipmentChange,

        /// <summary>
        /// C2S only (Server responds with an InventorySync message).
        /// A player requests to use, drop, trade an item from it's inventory.
        /// NOTE: For using an inventory item as equipment the RequipmentChange action must be used.
        /// </summary>
        InventoryChange,

        /// <summary>
        /// No action, used for system related messages like logout etc.
        /// </summary>
        None,

        #endregion

        #region Both Ways
        /// <summary>
        /// C2S: The player requests to attack something.
        /// S2C (to whole zone): A player/mob attacks something.
        /// </summary>
        Attack,

        /// <summary>
        /// C2S: The player requests to teleport.
        /// S2C: A player/mob has been teleported (to whole zone).
        /// </summary>
        Teleport,
        #endregion

        #region Server To Client
        /// <summary>
        /// S2C only (to whole zone).
        /// The target suffered some damage.
        /// </summary>
        Damage,

        /// <summary>
        /// S2C only (to whole zone).
        /// A player has died.
        /// </summary>
        PlayerDied,

        /// <summary>
        /// S2C only (to single player).
        /// This is a response to clients that either used, dropped, sold, traded, bought, pickedup, got an inventory item.
        /// </summary>
        InventorySync,

        /// <summary>
        /// S2C only (to whole zone).
        /// An entity has changed appearence/weapon/mount. The payload contains the equipment data (sent to all zone players) and inventory data (sent to the affected player only).
        /// </summary>
        EquipmentSync,

        /// <summary>
        /// S2C only (to whole zone).
        /// An entity has become visible or has entered the zone.
        /// The ServerToClientMessage payload must contain all data important for visual and client game logic representation of the entity.
        /// </summary>
        AddEntity,

        /// <summary>
        /// S2C only (to whole zone).
        /// An entity inside the zone has moved or changed it's movement state.
        /// </summary>
        MoveEntity,

        /// <summary>
        /// S2C only (to single player).
        /// Generated for character leaving a (linked) zone.
        /// </summary>
        RemoveEntity,

        /// <summary>
        /// S2C only (to single player).
        /// Generated for character entering a new (linked) zone.
        /// The message contains information about all entities residing inside a new zone.
        /// </summary>
        AddZone,

        /// <summary>
        /// S2C only (to single player).
        /// Generated for character leaving a (linked) zone.
        /// </summary>
        RemoveZone,

        /// <summary>
        /// S2C only (to single player).
        /// The clients move is either outside world, outside walking area, has invalid coordinates, or gets invalid by any other mean.
        /// It could be sent during a movement in progress if teh path gets blocked or immediately after the client issues a move request and move is invalid.
        /// </summary>
        InvalidMove,

        /// <summary>
        /// S2C only (to single player).
        /// The action had an invalid or unreachable target, the WSE should supply an exact reason if needed.
        /// </summary>
        InvalidTarget,

        /// <summary>
        /// S2C only (to single player).
        /// The action was invalid or unknown, the WSE should supply a reason if needed.
        /// </summary>
        InvalidAction
        #endregion
    }
}
