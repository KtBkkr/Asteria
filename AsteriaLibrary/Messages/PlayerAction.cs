using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AsteriaLibrary.Messages
{
    /// <summary>
    /// Defines player actions the world server/client must know about.
    /// Action members are also used as integers in the ServerToClientMessage.Code field for ZoneMessages.
    /// </summary>
    public enum PlayerAction
    {
        #region Client To Server
        /// <summary>
        /// No action, used for system related messages like logout etc.
        /// </summary>
        None,

        /// <summary>
        /// The player requests to create a unit.
        /// The message data contains the type and position requested.
        /// </summary>
        AddBuilding,

        /// <summary>
        /// The player requests to remove a unit.
        /// The message contains the building ID.
        /// </summary>
        RemoveBuilding,

        #endregion

        #region Both Ways
        /// <summary>
        /// C2S: The player requests to teleport.
        /// S2C: A player has been teleported, destination zone info is sent (to single player).
        /// </summary>
        Teleport,
        #endregion

        #region Server To Client
        /// <summary>
        /// S2C only (to whole zone).
        /// The target suffered some damage (or healed if negative value is given).
        /// </summary>
        Damage,

        /// <summary>
        /// S2C only (to whole zone).
        /// The target has received energy from the specified unit.
        /// </summary>
        Power,

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
        /// S2C only (to whole zone).
        /// An entity inside the zone has been updated and fresh entity data is sent.
        /// </summary>
        UpdateEntity,

        /// <summary>
        /// S2C only (to whole zone).
        /// Generated for entities leaving a zone.
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
        /// The action was invalid or unknown, the WSE should supply a reason if needed.
        /// </summary>
        InvalidAction
        #endregion
    }
}
