using MelonLoader;
using Rewired;
using Steamworks;
using Steamworks.Data;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace CalMP_Melon
{
    public class Steam
    {
        private readonly MelonLogger.Instance logger;
        public static Lobby CurrentLobby;
        private static Steam _instance;
        private CancellationTokenSource receiveTokenSource;

        public static Steam Instance
        {
            get
            {
                return _instance;
            }
        }

        public static void Init(MelonLogger.Instance logger)
        {
            if (_instance == null)
                _instance = new Steam(logger);
        }

        public Steam(MelonLogger.Instance logger)
        {
            this.logger = logger;

            SteamMatchmaking.OnLobbyMemberJoined += OnLobbyMemberJoined;
            SteamMatchmaking.OnLobbyMemberLeave += OnMemberLeft;
            SteamFriends.OnGameLobbyJoinRequested += OnGameLobbyJoinRequested;
            SteamClient.RunCallbacks();
            StartReceiving();
        }

        private async void OnGameLobbyJoinRequested(Lobby lobby, SteamId steamId)
        {
            logger.Msg($"Received a lobby join request for lobby: {lobby.Id}");
            Core.ShowNotification($"Joining lobby...");

            var joinResult = await SteamMatchmaking.JoinLobbyAsync(lobby.Id);

            if (joinResult.HasValue) //Check if the join was successful (dumb Facepunch API..)
            {
                //This is required to set the CurrentLobby for the Client too! Othervise the client wont see the lobby.
                CurrentLobby = joinResult.Value;
                logger.Msg("Successfully joined the lobby.");
                Core.ShowNotification("Joined lobby successfully!");
            }
            else
            {
                logger.Error("Failed to join lobby.");
                Core.ShowNotification("Failed to join lobby.");
            }
        }

        //Receiving & Sending Functions
        public void StartReceiving()
        {
            StopReceiving();
            receiveTokenSource = new CancellationTokenSource();
            Task.Run(() => ReceiveLoop(receiveTokenSource.Token));
        }

        public void StopReceiving()
        {
            receiveTokenSource?.Cancel();
            receiveTokenSource?.Dispose();
            receiveTokenSource = null;
        }

        private async Task ReceiveLoop(CancellationToken token)
        {
            while (!token.IsCancellationRequested)
            {
                while (SteamNetworking.IsP2PPacketAvailable(0))
                {
                    var packet = SteamNetworking.ReadP2PPacket();
                    if (packet.HasValue)
                    {
                        ProcessIncomingPacket(packet.Value.SteamId, packet.Value.Data);
                    }
                }
                await Task.Delay(16, token); //16ms ~ 60FPS
            }
        }

        private void ProcessIncomingPacket(SteamId senderId, byte[] data)
        {
            if (data == null || data.Length == 0) return;

            PacketType packetType = (PacketType)data[0];

            switch (packetType)
            {
                case PacketType.Movement:
                    PlayerStatePacket statePacket = PlayerStatePacket.Deserialize(data);

                    //Do movement
                    HandlePlayerMovement(senderId, statePacket);
                    break;

                default:
                    logger.Warning($"Received unknown packet type ID: {data[0]}");
                    break;
            }
        }

        private void HandlePlayerMovement(SteamId playerSteamId, PlayerStatePacket packet)
        { }


        // Callback Hooks

        private void OnLobbyMemberJoined(Lobby lobby, Friend member)
        {
            logger.Msg($"{member.Name} joined the lobby");
            Core.ShowNotification($"{member.Name} joined the lobby.");
        }

        private void OnMemberLeft(Lobby lobby, Friend member)
        {
            logger.Msg($"{member.Name} left the lobby");
            Core.ShowNotification($"{member.Name} left the lobby.");
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

        public void LeaveLobby()
        {
            if (CurrentLobby.Id.IsValid)
            {
                CurrentLobby.Leave();
                CurrentLobby = default;
                logger.Msg("Left the lobby.");
                Core.ShowNotification("You have left the lobby.");
            }
        }

        public static void SendPlayerStatePacket(PlayerStatePacket packet)
        {
            if (!CurrentLobby.Id.IsValid) return;

            byte[] data = packet.Serialize();

            foreach (var member in CurrentLobby.Members)
            {
                if (member.Id == SteamClient.SteamId) continue;
                SteamNetworking.SendP2PPacket(member.Id, data, data.Length, 0, P2PSend.Unreliable);
            }
        }
    }
}
