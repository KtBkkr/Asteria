using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using System.Xml.Linq;
using AsteriaLibrary.Entities;
using AsteriaLibrary.Math;
using AsteriaLibrary.Zones;
using AsteriaLibrary.Shared;

namespace AsteriaWorldServer
{
    /// <summary>
    /// Contains all type information parsed from the Entities.xml and WorldParams from WorldData.xml.
    /// </summary>
    public class DataManager
    {
        #region Fields
        private List<EntityData> entities = new List<EntityData>();
        private Dictionary<string, string> worldParameters = new Dictionary<string, string>();

        private static readonly DataManager singletone = new DataManager();
        #endregion

        #region Properties
        /// <summary>
        /// Returns the singletone WseDataManager instance.
        /// </summary>
        public static DataManager Singletone { get { return singletone; } }

        /// <summary>
        /// Returns the entities defined in the xml.
        /// </summary>
        public IEnumerable<EntityData> Entities { get { return entities; } }

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

                IEnumerable<EntityData> q = from c in xDocEntities.Elements("XnaContent").Elements("Entity")
                                            select new EntityData()
                                                 {
                                                     TypeId = Convert.ToInt32(c.Element("TypeId").Value),
                                                     Name = (string)c.Element("Name").Value,
                                                     Description = (string)c.Element("Description").Value,
                                                 };

                // Fill attributes and add to list
                foreach (EntityData ed in q)
                {
                    var node = from c in xDocEntities.Elements("XnaContent").Elements("Entity")
                               where Convert.ToInt32(c.Element("TypeId").Value) == ed.TypeId
                               select c;

                    // Attributes
                    var attributes = from a in node.Elements("Attributes").Descendants()
                                     select new { Name = a.Name.LocalName, Value = a.Value };

                    foreach (var v in attributes)
                        ed.Attributes.Add(v.Name, Convert.ToInt32(v.Value));

                    // Properties
                    var properties = from a in node.Elements("Properties").Descendants()
                                     select new { Name = a.Name.LocalName, Value = a.Value };

                    foreach (var v in attributes)
                        ed.Properties.Add(v.Name, v.Value);

                    entities.Add(ed);
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

                //if (node.Attributes["Amount"] != null)
                //{
                //    // TODO: Amount node is currently only used for gold. If there are more items which canhave an amount
                //    // we must specify how to implement this.
                //    e.Gold = Convert.ToUInt32(node.Attributes["Amount"].Value);
                //}
                zoneManager.AddEntity(e);
            }
        }

        ///// <summary>
        ///// Returns the player class with given typeId or null if no class found.
        ///// </summary>
        ///// <param name="typeId"></param>
        ///// <returns></returns>
        //public EntityClassData GetPlayerClass(int typeId)
        //{
        //    var result = from c in playerClasses where c.TypeId == typeId select c;
        //    if (result.Count() > 0)
        //        return result.First();
        //    else
        //        return null;
        //}

        /// <summary>
        /// Returns the entity class with given typeId or null if no class found.
        /// </summary>
        /// <param name="typeId"></param>
        /// <returns></returns>
        public EntityData GetEntityData(int typeId)
        {
            var result = from c in entities where c.TypeId == typeId select c;
            if (result.Count() > 0)
                return result.First();
            else
                return null;
        }
        #endregion
    }
}
