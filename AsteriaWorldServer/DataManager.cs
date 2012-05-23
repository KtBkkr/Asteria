using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using System.Xml.Linq;
using AsteriaLibrary.Entities;
using AsteriaLibrary.Math;
using AsteriaLibrary.Zones;
using AsteriaWorldServer.Entities;
using AsteriaLibrary.Shared;

namespace AsteriaWorldServer
{
    /// <summary>
    /// Contains all type information parsed from the Entities.xml and WorldParams from WorldData.xml.
    /// </summary>
    public class DataManager
    {
        #region Fields
        private List<EntityClassData> playerClasses = new List<EntityClassData>();
        private List<EntityClassData> entityClasses = new List<EntityClassData>();
        private Dictionary<string, string> worldParameters = new Dictionary<string, string>();

        private static readonly DataManager singletone = new DataManager();
        #endregion

        #region Properties
        /// <summary>
        /// Returns the singletone WseDataManager instance.
        /// </summary>
        public static DataManager Singletone { get { return singletone; } }

        /// <summary>
        /// Returns the player classes defined in the xml.
        /// </summary>
        public IEnumerable<EntityClassData> PlayerClasses { get { return playerClasses; } }

        /// <summary>
        /// Returns the player classes defined in the xml.
        /// </summary>
        public IEnumerable<EntityClassData> EntityClasses { get { return entityClasses; } }

        /// <summary>
        /// Returns the dictionary with world parameters.
        /// </summary>
        public Dictionary<string, string> WorldParameters { get { return worldParameters; } }
        #endregion

        #region Constructors
        /// <summary>
        /// Creates a new WseDataManager instance.
        /// </summary>
        internal DataManager()
        {
            ParseData();
        }
        #endregion

        #region Methods
        /// <summary>
        /// Reads the xml and parses the data.
        /// </summary>
        private void ParseData()
        {
            try
            {
                // Prepare all classes
                XDocument xDocEntities = XDocument.Load("Data/Entities.xml");

                IEnumerable<EntityClassData> q = from c in xDocEntities.Elements("XnaContent").Elements("Entity")
                                                 select new EntityClassData()
                                                 {
                                                     TypeId = Convert.ToInt32(c.Element("TypeId").Value),
                                                     Name = (string)c.Element("ClassName").Value,
                                                     Description = (string)c.Element("Description").Value,
                                                 };

                // Fill attributes and PCD list
                foreach (EntityClassData ecd in q)
                {
                    var node = from c in xDocEntities.Elements("XnaContent").Elements("Entity")
                               where Convert.ToInt32(c.Element("TypeId").Value) == ecd.TypeId
                               select c;

                    if (ecd.TypeId < 501)
                    {
                        // Player Classes
                        var attributes = from a in node.Elements("Attributes").Descendants()
                                         select new { Name = a.Name.LocalName, Value = a.Value };

                        foreach (var v in attributes)
                            ecd.DefaultAttributes.Add(v.Name, Convert.ToInt32(v.Value));

                        string sex = (from a in node.Elements("Sex") select (string)a.Value).First();
                        ecd.Sex = sex;

                        string race = (from a in node.Elements("Race") select (string)a.Value).First();
                        ecd.Race = race;

                        string size = (from a in node.Elements("Inventory") select (string)a.Value).First();
                        ecd.InventorySize = (Size)size;
                        ecd.SlotSize = Size.Zero;
                        playerClasses.Add(ecd);
                    }
                    else
                    {
                        // Other entities
                        string st = (from a in node.Elements("InventorySlots") select (string)a.Value).First();
                        if (st == "0")
                            ecd.SlotSize = Size.Zero;
                        else
                            ecd.SlotSize = (Size)st;

                        var maxStacks = from a in node.Elements("InventorySlots")
                                        where a.Attribute("maxStack") != null
                                        select a.Attribute("maxStack");

                        if (maxStacks.Count() > 0)
                            ecd.SlotStacks = int.Parse(maxStacks.First().Value);
                        else
                            ecd.SlotStacks = 0;

                        var attributes = from a in node.Elements("Attributes").Descendants()
                                         select new { Name = a.Name.LocalName, Value = a.Value };

                        foreach (var v in attributes)
                            ecd.ActionAttributes.Add(v.Name, v.Value);

                        entityClasses.Add(ecd);
                    }
                }

                XmlNodeList nodes;
                XmlDocument xd = new XmlDocument();
                xd.Load("Data/WorldData.xml");

                // Add params
                if (xd.SelectSingleNode("//Params") == null)
                    Logger.Output(this, "LoadWorld() no world parameters found in WorldData.xml!");
                else
                {
                    nodes = xd.SelectSingleNode("//Params").ChildNodes;
                    foreach (XmlNode node in nodes)
                    {
                        if (node.NodeType == XmlNodeType.Comment)
                            continue;

                        string name = node.Name;
                        worldParameters.Add(name, node.InnerText);
                        Logger.Output(this, "LoadWorld() added world parameter: '{0}'={1}", name, node.InnerText);
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.Output(this, "ParseData() exception: {0}, stacktrace: {1}", ex.Message, ex.StackTrace);
            }
        }

        /// <summary>
        /// Loads all static entities defined in the WorldData.xml
        /// Note that this can only be done after the IZoneManager instance has been initialized.
        /// </summary>
        /// <param name="zoneManager"></param>
        public void LoadStaticEntities(ZoneManager zoneManager)
        {
            XmlNodeList nodes;
            XmlDocument xd = new XmlDocument();
            xd.Load("Data/WorldData.xml");

            // Add pickable objects to the world. 
            nodes = xd.SelectNodes("//PickableItems/Item");
            foreach (XmlNode node in nodes)
            {
                if (node.NodeType == XmlNodeType.Comment)
                    continue;

                string location = node.SelectSingleNode("Location").InnerText;
                int typeId = Convert.ToInt32(node.Attributes["id"].Value);
                Entity e = new Entity(GameProcessor.GenerateEntityID(), typeId, "");
                Point p = (Point)location;
                e.Position = p;

                if (node.Attributes["Amount"] != null)
                {
                    // TODO: Amount node is currently only used for gold. If there are more items which canhave an amount
                    // we must specify how to implement this.
                    e.Gold = Convert.ToUInt32(node.Attributes["Amount"].Value);
                }
                zoneManager.AddEntity(e);
            }
        }

        /// <summary>
        /// Returns the player class with given typeId or null if no class found.
        /// </summary>
        /// <param name="typeId"></param>
        /// <returns></returns>
        public EntityClassData GetPlayerClass(int typeId)
        {
            var result = from c in playerClasses where c.TypeId == typeId select c;
            if (result.Count() > 0)
                return result.First();
            else
                return null;
        }

        /// <summary>
        /// Returns the entity class with given typeId or null if no class found.
        /// </summary>
        /// <param name="typeId"></param>
        /// <returns></returns>
        public EntityClassData GetEntityClass(int typeId)
        {
            var result = from c in entityClasses where c.TypeId == typeId select c;
            if (result.Count() > 0)
                return result.First();
            else
                return null;
        }
        #endregion
    }
}
