using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AsteriaLibrary.Math;

namespace AsteriaWorldServer.Entities
{
    /// <summary>
    /// Helper class for validating movements.
    /// </summary>
    class MoveManager
    {
        #region Fields
        int worldWidth, worldHeight;
        #endregion

        #region Constructors
        /// <summary>
        /// Creates a new MoveManager instance.
        /// </summary>
        /// <param name="worldWidth"></param>
        /// <param name="worldHeight"></param>
        public MoveManager(int worldWidth, int worldHeight)
        {
            this.worldWidth = worldWidth;
            this.worldHeight = worldHeight;
        }
        #endregion

        #region Methods
        /// <summary>
        /// Initializes the walkable area.
        /// </summary>
        public void Initialize()
        {
            // TODO
        }

        /// <summary>
        /// Check if a position is in a walkable area.
        /// </summary>
        /// <param name="position"></param>
        /// <returns></returns>
        public bool IsValidPosition(ref Point position)
        {
            // TODO
            return true; // (GetData(position.X, position.Y) < 128);
        }

        /// <summary>
        /// Calculates movement.
        /// </summary>
        /// <param name="position">Current location.</param>
        /// <param name="destination">Target location.</param>
        /// <param name="duration">Duration in milliseconds.</param>
        /// <param name="velocity">Entities movement speed.</param>
        /// <param name="rotation">Entities rotation.</param>
        /// <param name="newPosition">Calculated new position.</param>
        /// <returns>MoveResult.Moved or MoveResult.DestinationReached on success.</returns>
        public MoveResult MoveTo(ref Point position, ref Point destination, float duration, float velocity, ref int rotation, out Point newPosition)
        {
            // Check if we are there
            Point movementPoint = destination - position;
            if (movementPoint.Length() == 0)
            {
                newPosition = destination;
                return MoveResult.DestinationReached;
            }

            // TODO: [HIGH] we should keep movement in sync with the cliet movement speed to make sure
            // we don't update the position until the current destination tile is reached.

            // then again that might cause serious problems keeping the clients in sync.

            // New position calculation
            newPosition = destination;

            return MoveResult.Moved;
        }
        #endregion
    }
}
