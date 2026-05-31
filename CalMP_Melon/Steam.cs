using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Steamworks;
using Steamworks.Data;
using MelonLoader;

namespace CalMP_Melon
{
    public class Steam
    {
        private readonly MelonLogger.Instance logger;


        public Lobby CurrentLobby;

        public Steam(MelonLogger.Instance logger)
        {
            SteamMatchmaking.OnLobbyMemberJoined += OnMemberJoined;
            SteamMatchmaking.OnLobbyMemberLeave += OnMemberLeft;
            this.logger = logger;
        }

        private void OnMemberJoined(Lobby lobby, Friend member)
        {
            logger.Msg($"{member.Name} joined the lobby");
        }

        private void OnMemberLeft(Lobby lobby, Friend member)
        {
            logger.Msg($"{member.Name} left the lobby");
        }

        public async Task<bool> CreateLobbyAsync(int maxPlayers, string lobbyName = "Lobby")
        {
            var result = await SteamMatchmaking.CreateLobbyAsync(maxPlayers);
            if (result is not Lobby lobby) return false;

            lobby.SetPublic();
            lobby.SetJoinable(true);
            lobby.SetData("name", lobbyName);
            lobby.MaxMembers = maxPlayers;

            CurrentLobby = lobby;

            return true;
        }
    }
}
