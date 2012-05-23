using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AsteriaLibrary.Math;

namespace AsteriaWorldServer.Entities
{
    enum MoveType
    {
        Stop,
        Walk,
        Run,
        Attack,
    }

    /// <summary>
    /// Encapsulates all character actions and action related logic.
    /// </summary>
    class CharacterState
    {
        #region Fields
        private int id;
        private MoveType movement;
        private Point movementTarget;
        #endregion

        #region Properties
        /// <summary>
        /// Character Id.
        /// </summary>
        public int Id { get { return id; } }

        /// <summary>
        /// Indicates if the character is moving.
        /// </summary>
        public bool IsMoving { get { return movement != MoveType.Stop; } }

        /// <summary>
        /// Returns the characters movement stance.
        /// </summary>
        public MoveType Movement { get { return movement; } }

        /// <summary>
        /// Returns the movement target.
        /// </summary>
        public Point MoveTarget { get { return movementTarget; } }
        #endregion

        #region Constructors
        public CharacterState(int id)
        {
            this.id = id;
        }
        #endregion

        #region Methods
        /// <summary>
        /// Assigns a new movement target.
        /// </summary>
        /// <param name="targetPosition"></param>
        /// <param name="movementSpeedInData"></param>
        public void SetMove(Point targetPosition, string movementSpeedInData)
        {
            if (string.IsNullOrEmpty(movementSpeedInData) || movementSpeedInData == "0")
            {
                Stop();
            }
            else
            {
                movementTarget = targetPosition;
                if (movementSpeedInData == "1")
                    movement = MoveType.Walk;
                else
                    movement = MoveType.Run;
            }
        }

        /// <summary>
        /// Stops the movement.
        /// </summary>
        public void Stop()
        {
            movement = MoveType.Stop;
        }
        #endregion
    }
}
