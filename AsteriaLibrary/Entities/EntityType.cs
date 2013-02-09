using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AsteriaLibrary.Entities
{
    public enum EntityType : int
    {
        // Main Entities
        Player = 1,
        Unit = 2,

        // Building Units
        EnergyStation = 3,
        EnergyRelay = 4,
        MineralMiner = 5,
        BasicLaser = 6,
        PulseLaser = 7,
        TacticalLaser = 8,
        MissileLauncher = 9,

        // Ship Units
        Ship = 10,
        Fighter = 11,
        Swarmer = 12,
        Mothership = 13,

        // Other Entities
        Asteroid = 14,
        Portal = 15
    }
}
