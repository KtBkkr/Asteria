using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AsteriaLibrary.Messages
{
    public enum MessageType : short
    {
        #region Client to Login Server
        /// <summary>
        /// Initial request to login server. Contains account and password info.
        /// </summary>
        C2L_Login,

        /// <summary>
        /// Client world selection request, the message contains the world Id.
        /// </summary>
        C2L_SelectWorld,
        #endregion

        #region Login Server to Client
        /// <summary>
        /// Authentication acknowledged, message contains the accountId in code and WSI list in data.
        /// Note that there is no negative response, all errors during the authentication state result in client disconnect.
        /// </summary>
        L2C_LoginResponse,

        /// <summary>
        /// Response to a SelectWorld message, the message data contains the one time pad.
        /// </summary>
        L2C_WorldSelectResponse,
        #endregion

        #region Client to World Server (Out of Game)
        C2S_Authenticate,               // Initial user Authentication with world server. Contains accountId and secret in Data.
        C2S_GetCharacterList,           // Retreives the players account characters.
        C2S_CreateCharacter,            // Creates a new player character on the server.
        C2S_DeleteCharacter,            // Deletes an ingame character from the server.
        C2S_StartCharacter,             // Starts playing with the chosen character.
        #endregion

        #region World Server to Client (Out of Game)
        S2C_CharacterList,              // Server response to character list request. Contains pipe separated characters in Data.
        
        S2C_CreateSuccess,              // Server OK response to CreateCharacter.
        S2C_CreateFailed,               // Server response to CreateCharacter.

        S2C_DeleteSuccess,              // Server OK response to DeleteCharacter.
        S2C_DeleteFailed,               // Server response to DeleteCharacter.

        S2C_StartSuccess,               // Server OK response to StartCharacter.
        S2C_StartFailed,                // Server response to StartCharacter.

        #endregion

        #region Client to World Server (In Game)
        C2S_PlayerLogoutRequest,      // The player requests logout. Note that this message can be sent during the character management phase as well.
        C2S_CharacterLogoutRequest,   // The player requests to logout his character.

        C2S_PlayerChat,                 // The player sends a chat message.
        C2S_PlayerAction,               // The player requests an action.
        #endregion

        #region World Server to Client (In Game)
        S2C_Container,                  // Server sends multiple messages stored inside the container.

        S2C_PlayerLoggedOut,            // Answer to C2S_PlayerLogoutRequest, this will be followed by a disconnec message later on.
        S2C_CharacterLoggedOut,         // Answer to C2S_CharacterLogoutRequest, player remains connect and is back to char management.
        S2C_LogoutDenied,               // Answer to C2S_XXXLogoutRequest, player remains in game.

        S2C_ZoneMessage,
        S2C_ChatMessage,
        #endregion

        #region Login to World Server
        L2S_GetStatus,                  // Login server polls the world servers with this message.
        L2S_SendOneTimePad,             // Login server sends the one time pad to the world server that can be used by a client to connect.
        #endregion

        #region World Server to Login
        S2L_SendStatus,                 // World server sends it' status to login server.
        #endregion

        None,
    }
}
