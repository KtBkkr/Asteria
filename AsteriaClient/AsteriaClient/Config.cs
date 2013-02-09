using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;
using System.IO;
using System.Xml;
using System.Text;

namespace AsteriaClient
{
    public class Config
    {
        #region Variables
        private static Dictionary<string, float> floatConfig = new Dictionary<string, float>();
        private static Dictionary<string, int> intConfig = new Dictionary<string, int>();
        private static Dictionary<string, string> stringConfig = new Dictionary<string, string>();
        #endregion

        #region Properties
        public int Count
        {
            get
            {
                int count = 0;
                count += floatConfig.Count;
                count += intConfig.Count;
                count += stringConfig.Count;
                return count;
            }
        }
        #endregion

        #region Constructors
        #endregion

        #region Methods
        public static float GetFloat(string name)
        {
            if (floatConfig.ContainsKey(name))
                return floatConfig[name];

            return 0;
        }

        public static int GetInt(string name)
        {
            if (intConfig.ContainsKey(name))
                return intConfig[name];

            return 0;
        }

        public static string GetString(string name)
        {
            if (stringConfig.ContainsKey(name))
                return stringConfig[name];

            return "";
        }

        public static void SetFloat(string name, float value)
        {
            if (floatConfig.ContainsKey(name))
                floatConfig[name] = value;
            else
                floatConfig.Add(name, value);
        }

        public static void SetInt(string name, int value)
        {
            if (intConfig.ContainsKey(name))
                intConfig[name] = value;
            else
                intConfig.Add(name, value);
        }

        public static void SetString(string name, string value)
        {
            if (stringConfig.ContainsKey(name))
                stringConfig[name] = value;
            else
                stringConfig.Add(name, value);
        }

        public void LoadDefaultConfig()
        {
            //0 front, 1 back
            //0.00 ships
            //0.10 ship range
            //0.20 buildings
            //0.30 ship shot flares
            //0.40 ship shots
            //0.50 asteroids
            //0.60 building shots
            //0.70 building relays
            //0.80 building range

            //Building Layer Depth
            SetFloat("buildingLayer", 0.20f);
            SetFloat("buildingRangeLayer", 0.80f);
            SetFloat("buildingRelayLayer", 0.70f);
            SetFloat("buildingShotLayer", 0.60f);
            //Ship Layer Depth
            SetFloat("shipLayer", 0.00f);
            SetFloat("shipRangeLayer", 0.10f);
            // flare testing
            //SetFloat("shipShotFlareLayer", 0.01f);
            //SetFloat("shipShotLayer", 0.02f);
            SetFloat("shipShotFlareLayer", 0.30f);
            SetFloat("shipShotLayer", 0.40f);
            //Misc Layer Depth
            SetFloat("asteroidLayer", 0.50f);


            // Ships
            SetInt("fighterHealth", 50);
            SetInt("fighterCost", 10);
            SetInt("fighterRange", 100);
            SetInt("fighterBuildTime", 1);
            SetFloat("fighterSpeed", 1f);
            SetFloat("fighterTurnSpeed", 0.01f);

            SetInt("swarmerHealth", 30);
            SetInt("swarmerCost", 10);
            SetInt("swarmerRange", 100);
            SetInt("swarmerBuildTime", 1);
            SetFloat("swarmerSpeed", 1.5f);
            SetFloat("swarmerTurnSpeed", 0.03f);

            SetInt("motherHealth", 90);
            SetInt("motherCost", 10);
            SetInt("motherRange", 200);
            SetInt("motherBuildTime", 1);
            SetFloat("motherSpeed", 0.8f);
            SetFloat("motherTurnSpeed", 0.01f);

            SetInt("repairShipHealth", 10);
            SetInt("repairShipCost", 0);
            SetInt("repairShipRange", 100);
            SetInt("repairShipBuildTime", 1);
            SetFloat("repairShipSpeed", 0.5f);
            SetFloat("repairShipTurnSpeed", 0.02f);

            // Buildings
            SetInt("constructionBuildingHealth", 100);

            // Default Building
            SetInt("defaultBuildingHealth", 100);
            SetInt("defaultBuildingCost", 10);
            SetInt("defaultBuildingRange", 80);
            SetInt("defaultBuildingBuildTime", 1);
            SetInt("defaultBuildingMaxUpgrade", 1);
            SetInt("defaultBuildingUpgradeCost", 10);

            // Energy Station
            SetInt("energyStationHealth", 100);
            SetInt("energyStationCost", 10);
            SetInt("energyStationRange", 90);
            SetInt("energyStationBuildTime", 1);
            SetInt("energyStationMaxUpgrade", 1);
            SetInt("energyStationUpgradeCost", 100);
            SetInt("energyStationEnergy", 100);
            SetInt("energyStationRate", 1);
            SetInt("energyStationIncrease", 2);

            // Energy Relay
            SetInt("energyRelayHealth", 100);
            SetInt("energyRelayCost", 10);
            SetInt("energyRelayRange", 80);
            SetInt("energyRelayBuildTime", 1);
            SetInt("energyRelayMaxUpgrade", 1);
            SetInt("energyRelayUpgradeCost", 100);

            // Mineral Miner
            SetInt("mineralMinerHealth", 100);
            SetInt("mineralMinerCost", 10);
            SetInt("mineralMinerRange", 300);
            SetInt("mineralMinerBuildTime", 1);
            SetInt("mineralMinerMaxUpgrade", 1);
            SetInt("mineralMinerUpgradeCost", 100);

            // Repair Station
            SetInt("repairStationHealth", 100);
            SetInt("repairStationCost", 10);
            SetInt("repairStationRange", 120);
            SetInt("repairStationBuildTime", 1);
            SetInt("repairStationMaxUpgrade", 1);
            SetInt("repairStationUpgradeCost", 100);
            SetInt("repairStationSlots", 3);

            // Basic Laser
            SetInt("basicLaserHealth", 100);
            SetInt("basicLaserCost", 10);
            SetInt("basicLaserRange", 300);
            SetInt("basicLaserBuildTime", 1);
            SetInt("basicLaserMaxUpgrade", 1);
            SetInt("basicLaserUpgradeCost", 100);

            // Pulse Laser
            SetInt("pulseLaserHealth", 100);
            SetInt("pulseLaserCost", 10);
            SetInt("pulseLaserRange", 300);
            SetInt("pulseLaserBuildTime", 1);
            SetInt("pulseLaserMaxUpgrade", 1);
            SetInt("pulseLaserUpgradeCost", 100);

            // Tactical Laser
            SetInt("tacticalLaserHealth", 100);
            SetInt("tacticalLaserCost", 10);
            SetInt("tacticalLaserRange", 300);
            SetInt("tacticalLaserBuildTime", 1);
            SetInt("tacticalLaserMaxUpgrade", 1);
            SetInt("tacticalLaserUpgradeCost", 100);

            // Missile Launcher
            SetInt("missileLauncherHealth", 100);
            SetInt("missileLauncherCost", 10);
            SetInt("missileLauncherRange", 400);
            SetInt("missileLauncherBuildTime", 1);
            SetInt("missileLauncherMaxUpgrade", 1);
            SetInt("missileLauncherUpgradeCost", 100);

            // Warehouse
            SetInt("warehouseHealth", 100);
            SetInt("warehouseCost", 10);
            SetInt("warehouseRange", 120);
            SetInt("warehouseBuildTime", 1);
            SetInt("warehouseMaxUpgrade", 1);
            SetInt("warehouseUpgradeCost", 100);
            SetInt("warehouseCapacity", 10);

            // Factory
            SetInt("factoryHealth", 100);
            SetInt("factoryCost", 10);
            SetInt("factoryRange", 50);
            SetInt("factoryBuildTime", 1);
            SetInt("factoryMaxUpgrade", 1);
            SetInt("factoryUpgradeCost", 100);

            // Network
            SetInt("networkPort", 5961);

            // Gameplay
            SetInt("maxPlayers", 20);
            SetInt("startingMinerals", 100);
        }

        public static List<string> DumpValues()
        {
            List<string> values = new List<string>();
            foreach (KeyValuePair<string, float> pair in floatConfig)
                values.Add(string.Format("Float value \"{0}\" set \"{1}\".", pair.Key, pair.Value));
            foreach (KeyValuePair<string, int> pair in intConfig)
                values.Add(string.Format("Int value \"{0}\" set \"{1}\".", pair.Key, pair.Value));
            foreach (KeyValuePair<string, string> pair in stringConfig)
                values.Add(string.Format("String value \"{0}\" set \"{1}\".", pair.Key, pair.Value));
            return values;
        }

        public void Load(string filename)
        {
            if (!File.Exists(filename))
            {
                //Logger.Log("[Config]: Generating new config file.");
                LoadDefaultConfig();
                Save(filename);
                return;
            }

            Dictionary<string, float> tempFloatConfig = new Dictionary<string, float>();
            Dictionary<string, int> tempIntConfig = new Dictionary<string, int>();
            Dictionary<string, string> tempStringConfig = new Dictionary<string, string>();
            using (XmlReader r = new XmlTextReader(filename))
            {
                while (r.Read())
                {
                    if (r.NodeType != XmlNodeType.Element || r.Name != "entry")
                        continue;

                    string name = r.GetAttribute("name");
                    switch (r.GetAttribute("type"))
                    {
                        case "float":
                            tempFloatConfig.Add(name, (float)Convert.ToDouble(r.GetAttribute("value")));
                            break;
                        case "int":
                            tempIntConfig.Add(name, Convert.ToInt32(r.GetAttribute("value")));
                            break;
                        case "string":
                            tempStringConfig.Add(name, r.GetAttribute("value"));
                            break;
                    }
                }
            }
            int totalCount = tempFloatConfig.Count + tempIntConfig.Count + tempStringConfig.Count;

            if (totalCount > 0)
            {
                floatConfig = tempFloatConfig;
                intConfig = tempIntConfig;
                stringConfig = tempStringConfig;
                //Logger.Log(String.Format("[Config]: Loaded {0} config values. ({1}).", totalCount, filename));
            }
            else
            {
                LoadDefaultConfig();
                //Logger.Log(String.Format("[Config]: Unable to load config values ({0}).", filename));
            }
        }

        public void Save(string filename)
        {
            using (FileStream fs = new FileStream(filename, FileMode.Create))
            {
                using (XmlTextWriter w = new XmlTextWriter(fs, Encoding.UTF8))
                {
                    w.WriteStartDocument();
                    w.WriteStartElement("Config");

                    foreach (KeyValuePair<string, float> pair in floatConfig)
                    {
                        w.WriteStartElement("entry");
                        w.WriteAttributeString("type", "float");
                        w.WriteAttributeString("name", pair.Key);
                        w.WriteAttributeString("value", pair.Value.ToString());
                        w.WriteEndElement();
                    }

                    foreach (KeyValuePair<string, int> pair in intConfig)
                    {
                        w.WriteStartElement("entry");
                        w.WriteAttributeString("type", "int");
                        w.WriteAttributeString("name", pair.Key);
                        w.WriteAttributeString("value", pair.Value.ToString());
                        w.WriteEndElement();
                    }

                    foreach (KeyValuePair<string, string> pair in stringConfig)
                    {
                        w.WriteStartElement("entry");
                        w.WriteAttributeString("type", "string");
                        w.WriteAttributeString("name", pair.Key);
                        w.WriteAttributeString("value", pair.Value.ToString());
                        w.WriteEndElement();
                    }

                    w.WriteEndElement();
                    w.WriteEndDocument();
                }
            }
            int totalCount = floatConfig.Count + intConfig.Count + stringConfig.Count;
            //Logger.Log(String.Format("[Config]: Saving {0} values to config. ({1}).", totalCount, filename));
        }
        #endregion
    }
}
