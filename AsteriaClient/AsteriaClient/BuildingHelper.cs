using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using AsteriaLibrary.Entities;

namespace AsteriaClient
{
    public class BuildingHelper
    {
        #region Methods
        public static float DirectionFromTarget(Vector2 from, Vector2 to)
        {
            Vector2 direction = from - to;
            direction.Normalize();

            return (float)Math.Atan2(-direction.X, direction.Y);
        }

        public static float DirectionToTarget(Vector2 from, Vector2 to)
        {
                Vector2 direction = from - to;
                direction.Normalize();

                return (float)Math.Atan2(direction.X, -direction.Y);
        }

        public static float SubRad(float a, float b)
        {
            float r = a - b;
            float abs_r = (float)Math.Abs(r);
            float pi = (float)Math.PI;

            if (abs_r > pi)
            {
                r = abs_r / r * (pi - abs_r);
            }
            return r;
        }

        public static float AddRad(float a, float b)
        {
            float r = a + b;
            if (r < -MathHelper.Pi)
                r += MathHelper.Pi * 2;
            if (r > MathHelper.Pi)
                r -= MathHelper.Pi * 2;
            return r;
        }
        #endregion
    }
}
