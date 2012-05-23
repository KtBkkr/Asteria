using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AsteriaWorldServer.Entities
{
    enum MoveResult
    {
        /// <summary>
        /// Can not move at all towards the target so the position is unchanged.
        /// </summary>
        Blocked,

        /// <summary>
        /// Moved somewhat towards the destination but the movement is blocked.
        /// </summary>
        BlockedMove,

        /// <summary>
        /// Moved towards the destination.
        /// </summary>
        Moved,

        /// <summary>
        /// Moved towards the destination and reached the destination.
        /// </summary>
        DestinationReached,
    }
}
