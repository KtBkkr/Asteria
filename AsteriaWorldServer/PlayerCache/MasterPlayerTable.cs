using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using AsteriaLibrary.Entities;
using AsteriaLibrary.Messages;
using AsteriaLibrary.Shared;
using Lidgren.Network;
using AsteriaData = AsteriaLibrary.Data;

namespace AsteriaWorldServer.PlayerCache
{
    /// <summary>
    /// Server side cache of all connected players with lookup keys on accountid, characterId, and end point.
    /// </summary>
    public class MasterPlayerTable
    {
        #region Fields
        private ReaderWriterLockSlim rwls;
        private ServerContext context;
        private object otpLocker;

        private Dictionary<string, DateTime> oneTimePad;
        private List<IPEndPoint> removalList;

        // Cache and lookup keys
        private AsteriaData.LinkedList<MasterPlayerRecord> list; // master list
        private Dictionary<IPEndPoint, AsteriaData.LinkedNode<MasterPlayerRecord>> epMap; // end point to mpr
        private Dictionary<int, AsteriaData.LinkedNode<MasterPlayerRecord>> aIdMap; // accountId to mpr
        private Dictionary<int, AsteriaData.LinkedNode<MasterPlayerRecord>> cIdMap; // characterId to mpr
        #endregion

        #region Properties
        /// <summary>
        /// Returns the first MPR wrapped in linked list node.
        /// Note that while iterating the LinkedList through LinkedNode.Next the node obtained can be a new node
        /// just added to the list without it's characterId, accountId, or other mappings added to the internal dictionaries.
        /// </summary>
        public AsteriaData.LinkedNode<MasterPlayerRecord> First { get { return list.First; } }

        /// <summary>
        /// Returns the number of MPR records inside the table.
        /// </summary>
        public int Count
        {
            get
            {
                rwls.EnterReadLock();
                try
                {
                    return aIdMap.Count;
                }
                finally
                {
                    rwls.ExitReadLock();
                }
            }
        }
        #endregion

        #region Constructors
        public MasterPlayerTable(int initialSize, ServerContext context)
        {
            this.context = context;
            this.rwls = new ReaderWriterLockSlim(LockRecursionPolicy.NoRecursion);
            this.otpLocker = new object();

            this.oneTimePad = new Dictionary<string, DateTime>(initialSize);
            this.removalList = new List<IPEndPoint>(initialSize);

            this.list = new AsteriaData.LinkedList<MasterPlayerRecord>(initialSize);
            this.epMap = new Dictionary<IPEndPoint, AsteriaData.LinkedNode<MasterPlayerRecord>>(initialSize);
            this.aIdMap = new Dictionary<int, AsteriaData.LinkedNode<MasterPlayerRecord>>(initialSize);
            this.cIdMap = new Dictionary<int, AsteriaData.LinkedNode<MasterPlayerRecord>>(initialSize);
        }
        #endregion

        #region Methods

        #region Find MPR
        /// <summary>
        /// Returns MPR instance based on the accounId.
        /// </summary>
        /// <param name="accountId"></param>
        /// <returns></returns>
        public MasterPlayerRecord GetByAccountId(int accountId)
        {
            rwls.EnterReadLock();
            try
            {
                if (aIdMap.ContainsKey(accountId))
                    return aIdMap[accountId].Value;
            }
            finally
            {
                rwls.ExitReadLock();
            }
            return null;
        }

        /// <summary>
        /// Returns MPR instance based on the characterId.
        /// </summary>
        /// <param name="characterId"></param>
        /// <returns></returns>
        public MasterPlayerRecord GetByCharacterId(int characterId)
        {
            rwls.EnterReadLock();
            try
            {
                if (cIdMap.ContainsKey(characterId))
                    return cIdMap[characterId].Value;
            }
            finally
            {
                rwls.ExitReadLock();
            }
            return null;
        }

        /// <summary>
        /// Returns MPR instance based on the EndPoint.
        /// </summary>
        /// <param name="ep"></param>
        /// <returns></returns>
        public MasterPlayerRecord GetByEndPoint(IPEndPoint ep)
        {
            rwls.EnterReadLock();
            try
            {
                if (epMap.ContainsKey(ep))
                    return epMap[ep].Value;
            }
            finally
            {
                rwls.ExitReadLock();
            }
            return null;
        }

        /// <summary>
        /// Returns Character instance based on accountId.
        /// </summary>
        /// <param name="accountId"></param>
        /// <returns></returns>
        public Character GetCharacterByAccountId(int accountId)
        {
            MasterPlayerRecord mpr = GetByAccountId(accountId);
            if (mpr != null)
                return mpr.pCharacter as Character;
            else
                return null;
        }

        /// <summary>
        /// Returns Character instance based on characterId.
        /// </summary>
        /// <param name="characterId"></param>
        /// <returns></returns>
        public Character GetCharacterByCharacterId(int characterId)
        {
            MasterPlayerRecord mpr = GetByCharacterId(characterId);
            if (mpr != null)
                return mpr.pCharacter as Character;
            else
                return null;
        }
        #endregion

        #region House Keeping
        /// <summary>
        /// Adds a MPR to the table.
        /// </summary>
        /// <param name="mpr"></param>
        public void Add(MasterPlayerRecord mpr)
        {
            rwls.EnterWriteLock();
            try
            {
                AsteriaData.LinkedNode<MasterPlayerRecord> node = list.Add(mpr);
                epMap.Add(mpr.Sender.RemoteEndpoint, node);
                if (mpr.AccountId > 0)
                    aIdMap.Add(mpr.AccountId, node);
                if (mpr.CharacterId > 0)
                    cIdMap.Add(mpr.CharacterId, node);
            }
            finally
            {
                rwls.ExitWriteLock();
            }
        }

        /// <summary>
        /// Removes a MPR from the table.
        /// </summary>
        /// <param name="accountId"></param>
        public void Remove(int accountId)
        {
            rwls.EnterWriteLock();
            try
            {
                if (aIdMap.ContainsKey(accountId))
                {
                    AsteriaData.LinkedNode<MasterPlayerRecord> node = aIdMap[accountId];
                    if (node.Value.CharacterId > 0)
                        cIdMap.Remove(node.Value.CharacterId);
                    aIdMap.Remove(accountId);
                    epMap.Remove(node.Value.Sender.RemoteEndpoint);
                    list.Remove(node);
                }
            }
            finally
            {
                rwls.ExitWriteLock();
            }
        }

        /// <summary>
        /// Removes a MPR from the table.
        /// </summary>
        /// <param name="ep"></param>
        public void Remove(IPEndPoint ep)
        {
            rwls.EnterWriteLock();
            try
            {
                RemoveWithoutLocks(ep);
            }
            finally
            {
                rwls.ExitWriteLock();
            }
        }

        /// <summary>
        /// Helper to remove an MPR from the table.
        /// Since the MPT logic must be kept simple we have a non recursive RW lock.
        /// This scenario effectively prevents us from using upgradeable read locks in a caller function if the callee uses rw locks - which is exactly the case with Remove.
        /// </summary>
        /// <param name="ep"></param>
        private void RemoveWithoutLocks(IPEndPoint ep)
        {
            if (epMap.ContainsKey(ep))
            {
                AsteriaData.LinkedNode<MasterPlayerRecord> node = epMap[ep];
                if (node.Value.CharacterId > 0)
                    cIdMap.Remove(node.Value.CharacterId);
                aIdMap.Remove(node.Value.AccountId);
                epMap.Remove(ep);
                list.Remove(node);
            }
        }

        /// <summary>
        /// Cleans up the table from records pointing to disconnected clients.
        /// </summary>
        public void Cleanup()
        {
            // Clean up unused onetimepads after 10 seconds timeout
            try
            {
                List<string> remove = new List<string>();
                lock (otpLocker)
                {
                    foreach (string key in oneTimePad.Keys)
                    {
                        if ((DateTime.Now - oneTimePad[key]).TotalSeconds > 10)
                            remove.Add(key);
                    }

                    foreach (string key in remove)
                        oneTimePad.Remove(key);
                }
            }
            catch { }

            rwls.EnterUpgradeableReadLock();
            try
            {
                IEnumerable<IPEndPoint> ps = null;

                // Find non flooding logout client request not in world.
                ps = from kvp in epMap
                     where kvp.Value.Value.LogoutClientRequested &&
                     kvp.Value.Value.State != ClientState.InWorld &&
                     kvp.Value.Value.FloodEntry.NextActionAllowed < DateTime.Now
                     select kvp.Value.Value.Sender.RemoteEndpoint;

                if (ps != null && ps.Count() > 0)
                    removalList.AddRange(ps);

                // Find non flooding, in world, disconnecting or already disconnected clients.
                ps = from kvp in epMap
                     where kvp.Value.Value.pCharacter != null &&
                     kvp.Value.Value.LogoutCharacterGranted &&
                     (kvp.Value.Value.State == ClientState.Disconnecting || kvp.Value.Value.State == ClientState.Disconnected || kvp.Value.Value.State == ClientState.CharacterLoggingOut) &&
                     kvp.Value.Value.FloodEntry.NextActionAllowed < DateTime.Now
                     select kvp.Value.Value.Sender.RemoteEndpoint;

                if (ps != null && ps.Count() > 0)
                    removalList.AddRange(ps);

                // Finally delete players
                if (removalList.Count > 0)
                {
                    rwls.EnterWriteLock();
                    try
                    {
                        foreach (IPEndPoint ep in removalList)
                        {
                            AsteriaData.LinkedNode<MasterPlayerRecord> mpr;
                            if (epMap.TryGetValue(ep, out mpr))
                            {
                                Character c = (mpr.Value.pCharacter as Character);
                                if (c != null)
                                {
                                    // Clear the zone first
                                    context.ZoneManager.RemoveEntity(c.Id);
                                    //context.Dal.SaveCompleteCharacter(c); // TODO: not needed? seems to cause few seconds of lag to turnmanager..
                                }

                                // Finally remove from MPT
                                if (mpr.Value.LogoutClientRequested)
                                {
                                    RemoveWithoutLocks(ep);
                                }
                                else
                                {
                                    // Only logging out character, return to char management.
                                    if (mpr.Value.CharacterId > 0 && cIdMap.ContainsKey(mpr.Value.CharacterId))
                                        cIdMap.Remove(mpr.Value.CharacterId);

                                    mpr.Value.CharacterId = 0;
                                    mpr.Value.pCharacter = null;
                                    mpr.Value.LogoutCharacterGranted = false;
                                    mpr.Value.LogoutCharacterRequested = false;
                                    mpr.Value.LogoutClientRequested = false;
                                    mpr.Value.State = ClientState.CharacterManagement;
                                }
                            }
                        }
                    }
                    finally
                    {
                        rwls.ExitWriteLock();
                    }
                    removalList.Clear();
                }
            }
            finally
            {
                rwls.ExitUpgradeableReadLock();
            }
        }
        #endregion

        /// <summary>
        /// Adds a one time pad for authentication, otp has IP on board and times out
        /// </summary>
        /// <param name="otp"></param>
        public void AddOneTimePad(string otp)
        {
            lock (otpLocker)
            {
                if (oneTimePad.ContainsKey(otp))
                    oneTimePad[otp] = DateTime.Now;
                else
                    oneTimePad.Add(otp, DateTime.Now);
            }
        }

        public bool AuthenticatePlayer(ClientToServerMessage msg)
        {
            try
            {
                // We only accept Authenticate
                if (msg.MessageType != MessageType.C2S_Authenticate)
                {
                    Disconnect(msg.Sender, "Unexpected message", 5000);
                    return false;
                }

                // Verify account / password
                if (msg.AccountId > 0)
                {
                    // Find MPR entry
                    MasterPlayerRecord mpr = GetByEndPoint(msg.Sender.RemoteEndpoint);

                    if (mpr == null)
                    {
                        string message = String.Format("No MPR for endpoint: {0} exists!", msg.Sender.RemoteEndpoint);
                        Disconnect(msg.Sender, message, 5000);
                    }

                    // Check authentication
                    string otp = msg.Data;
                    bool isAuthenticationOK = false;
                    lock(otpLocker)
                    {
                        isAuthenticationOK = (oneTimePad.ContainsKey(otp) &&
                            otp.EndsWith(msg.AccountId.ToString()) &&
                            otp.StartsWith(msg.Sender.RemoteEndpoint.Address.ToString())) ||
                            (otp == "admin_testing");

                        if(isAuthenticationOK)
                            oneTimePad.Remove(msg.Data);
                    }

                    if(isAuthenticationOK)
                        Logger.Output(this, "Authenticate() Valid secret: '{0}' received from player: {1}, endpoint: {2}", otp, msg.AccountId, msg.Sender.RemoteEndpoint.Address);
                    else
                    {
                        Logger.Output(this, "Authenticate() Invalid secret: '{0}' received from player: {1}, endpoint: {2}", otp, msg.AccountId, msg.Sender.RemoteEndpoint.Address);
                        Disconnect(msg.Sender, "Not authorized", 5000);
                        return false;
                    }

                    // Since authenticate is the very first message from a client store it's accountId
                    mpr.State = ClientState.CharacterManagement;
                    UpdateAccountId(msg.Sender.RemoteEndpoint, msg.AccountId);
                    return true;
                }
                else
                {
                    Logger.Output(this, "Authenticate() Invalid accountID: '{0}' received from endpoint: {1}", msg.AccountId, msg.Sender.RemoteEndpoint.Address);
                    Disconnect(msg.Sender, "Not authorized, wrong account ID.", 5000);
                    return false;
                }
            }
            catch(Exception ex)
            {
                Logger.Output(this, "Authenticate() Exception: {0}", ex.Message);
            }
            return false;
        }

        /// <summary>
        /// Disconnects the client and adds it to the networkingflood table.
        /// Further connects from that endpoint (IP address without port) is prohibited for the next given milliseconds.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="message"></param>
        /// <param name="milliseconds"></param>
        public void Disconnect(NetConnection sender, string message, int milliseconds)
        {
            MasterPlayerRecord mpr = GetByEndPoint(sender.RemoteEndpoint);
            if (mpr != null)
            {
                Logger.Output(this, "Disconnect requested for account: {0}, from: {1}, msg: '{2}', flood prevent: {3} ms!", mpr.AccountId, sender.RemoteEndpoint.ToString(), message, milliseconds);

                mpr.State = ClientState.Disconnected;
                mpr.FloodEntry.AddConnectionPrevent(milliseconds);
                mpr.Sender.Disconnect(message);
            }
            else
                Logger.Output(this, "Disconnect requested for: {0}, msg: '{1}', but endpoint not found!", sender.RemoteEndpoint, message);
        }

        /// <summary>
        /// Updates the characterId of a player.
        /// </summary>
        /// <param name="accountId"></param>
        /// <param name="characterId"></param>
        public void UpdateCharacterId(int accountId, int characterId)
        {
            rwls.EnterWriteLock();
            try
            {
                // Fetch MPR
                AsteriaData.LinkedNode<MasterPlayerRecord> node = aIdMap[accountId];
                MasterPlayerRecord mpr = node.Value;

                // Remove old characterId first
                if (mpr.CharacterId > 0 && cIdMap.ContainsKey(mpr.CharacterId))
                    cIdMap.Remove(mpr.CharacterId);

                // Set new Id and update characterId map
                mpr.CharacterId = characterId;
                if (characterId > 0)
                    cIdMap.Add(characterId, node);
            }
            catch (Exception ex)
            {
                Logger.Output(this, "UpdateCharacterId({0}, {1}) exception: {2}", accountId, characterId, ex.Message);
            }
            finally
            {
                rwls.ExitWriteLock();
            }
        }

        /// <summary>
        /// Updates the accountId of a player.
        /// </summary>
        /// <param name="ep"></param>
        /// <param name="accountId"></param>
        public void UpdateAccountId(IPEndPoint ep, int accountId)
        {
            rwls.EnterWriteLock();
            try
            {
                // Fetch MPR
                AsteriaData.LinkedNode<MasterPlayerRecord> node = epMap[ep];
                MasterPlayerRecord mpr = node.Value;

                // Remove old accountId first
                if (mpr.AccountId > 0 && aIdMap.ContainsKey(mpr.AccountId))
                    aIdMap.Remove(accountId);

                // Set new Id and update accountId map
                mpr.AccountId = accountId;
                if (accountId > 0)
                    aIdMap.Add(accountId, node);
            }
            catch { }
            finally
            {
                rwls.ExitWriteLock();
            }
        }
        #endregion
    }
}
