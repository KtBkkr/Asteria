using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace AsteriaClient.Sprites.Shots
{
    public class BeamMining : Beam
    {
        #region Constructors
        public BeamMining(Texture2D texture, Vector2 pos, Vector2 target, float rotation, int length)
            :base(texture, pos, target, rotation, Color.Green, 6, length)
        {
        }
        #endregion
    }
}
