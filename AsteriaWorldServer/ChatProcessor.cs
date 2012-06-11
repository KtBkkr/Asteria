using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using AsteriaLibrary.Shared;
using AsteriaWorldServer.Messages;
using AsteriaLibrary.Messages;
using AsteriaLibrary.Entities;

namespace AsteriaWorldServer
{
    /// <summary>
    /// Handles incoming chat messages and formats outgoing messages accordingly.
    /// </summary>
    public class ChatProcessor
    {
        #region Fields
        private ServerContext context;
        #endregion

        #region Properties
        #endregion

        #region Constructors
        public ChatProcessor(ServerContext context)
        {
            this.context = context;
        }
        #endregion

        #region Methods
        public void HandleChatMessage(ClientToServerMessage msg)
        {
            Character character = (Character)context.Mpt.GetByCharacterId(msg.CharacterId).pCharacter;
            int channel = Convert.ToInt32(msg.Data.Split('|')[0]);
            int dest = Convert.ToInt32(msg.Data.Split('|')[1]);
            string message = msg.GameData;

            ServerToClientMessage wm = ServerToClientMessage.CreateMessageSafe(character.Sender);
            MessageFormatter.CreateChatMessage(character.Name, channel, dest, message, wm);
            context.GameProcessor.AddMessageToZone(character.Zone, wm);
            ServerToClientMessage.FreeSafe(wm);
        }
        #endregion
    }
}
