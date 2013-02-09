using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Graphics;
using AsteriaLibrary.Shared;
using Microsoft.Xna.Framework.Content;

namespace AsteriaClient
{
    public class TextureManager
    {
        #region Fields
        private Dictionary<string, Texture2D> textures = new Dictionary<string, Texture2D>();
        private static TextureManager singletone;
        #endregion

        #region Properties
        public static TextureManager Singletone
        {
            get { return singletone; }
        }
        #endregion

        #region Constructors
        public TextureManager()
        {
            TextureManager.singletone = this;
        }
        #endregion

        #region Methods
        public Texture2D Get(string name)
        {
            if (textures.ContainsKey(name))
                return textures[name];

            Logger.Output(this, "Could not get texture '{0}'", name);
            return textures["Unknown"];
        }

        public string GetName(Texture2D texture)
        {
            foreach (KeyValuePair<string, Texture2D> tex in textures)
            {
                if (tex.Value == texture)
                    return tex.Key;
            }
            Logger.Output(this, "Could not get texture name from texture.");
            return "Unknown";
        }

        public void LoadTextures(ContentManager Content)
        {
            //Textures
            textures.Add("Unknown", Content.Load<Texture2D>("Unknown"));

            //World
            textures.Add("Asteroid1", Content.Load<Texture2D>("World/Asteroid1"));
            textures.Add("Asteroid2", Content.Load<Texture2D>("World/Asteroid2"));
            textures.Add("Asteroid3", Content.Load<Texture2D>("World/Asteroid3"));
            textures.Add("Asteroid4", Content.Load<Texture2D>("World/Asteroid4"));
            textures.Add("Background1", Content.Load<Texture2D>("World/Background1"));
            textures.Add("Background2", Content.Load<Texture2D>("World/Background2"));
            textures.Add("Background3", Content.Load<Texture2D>("World/Background3"));
            textures.Add("Background4", Content.Load<Texture2D>("World/Background4"));
            textures.Add("Ground", Content.Load<Texture2D>("World/Ground"));

            //Buildings
            textures.Add("MineralMiner", Content.Load<Texture2D>("Buildings/MineralMiner"));
            textures.Add("EnergyStation", Content.Load<Texture2D>("Buildings/EnergyStation"));
            textures.Add("EnergyRelay", Content.Load<Texture2D>("Buildings/EnergyRelay"));
            textures.Add("EnergyStorage", Content.Load<Texture2D>("Buildings/EnergyStorage"));
            textures.Add("Warehouse", Content.Load<Texture2D>("Buildings/Warehouse"));
            textures.Add("Windmill", Content.Load<Texture2D>("BUildings/Windmill"));

            //Constructing Buildings
            //textures.Add("MineralMinerBuilding", Content.Load<Texture2D>("Buildings/MineralMinerBuilding"));
            //textures.Add("EnergyStorageBuilding", Content.Load<Texture2D>("Buildings/EnergyStorageBuilding"));
            //textures.Add("EnergyRelayBuilding", Content.Load<Texture2D>("Buildings/EnergyRelayBuilding"));

            //Shots
            textures.Add("Range", Content.Load<Texture2D>("Buildings/Range"));
            textures.Add("Beam", Content.Load<Texture2D>("Shots/Beam"));
            textures.Add("Flare", Content.Load<Texture2D>("Shots/Flare"));

            //Ships
            textures.Add("Fighter", Content.Load<Texture2D>("Ships/Fighter"));
            textures.Add("Swarmer", Content.Load<Texture2D>("Ships/Swarmer"));
            textures.Add("MotherShip", Content.Load<Texture2D>("Ships/MotherShip"));
            textures.Add("BattleShip", Content.Load<Texture2D>("Ships/BattleShip"));
            textures.Add("Cruiser", Content.Load<Texture2D>("Ships/Cruiser"));
            textures.Add("RepairShip", Content.Load<Texture2D>("Ships/RepairShip"));

            //Interface
            //textures.Add("HealthBarGreen", Content.Load<Texture2D>("Sprites/Interface/HealthBarGreen"));
            //textures.Add("HealthBarRed", Content.Load<Texture2D>("Sprites/Interface/HealthBarRed"));
        }
        #endregion
    }
}
