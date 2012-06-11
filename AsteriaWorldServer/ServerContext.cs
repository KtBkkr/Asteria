using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AsteriaWorldServer.PlayerCache;
using AsteriaLibrary.Zones;

namespace AsteriaWorldServer
{
    /// <summary>
    /// Holder for server side references that must be passed to many components both inside the WS and WSE.
    /// </summary>
    public sealed class ServerContext
    {
        #region Fields
        public string Css;
        public ZoneManager ZoneManager;
        public GameProcessor GameProcessor;
        public DalProvider Dal;
        public MasterPlayerTable Mpt;
        public Dictionary<string, string> ServerConfig;

        /// <summary>
        /// For WSE usag. Not used by the framework.
        /// </summary>
        public object WseData;

        /// <summary>
        /// Turn duration in milliseconds.
        /// This is the targeted turn duration not the actual turn duration, turns can last considerably longer but it is guaranteed that they will not last less than this time.
        /// </summary>
        public int TurnDuration;
        #endregion
    }
}
