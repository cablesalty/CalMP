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
            Core.ShowNotification("Joining lobby...");

            var joinResult = await SteamMatchmaking.JoinLobbyAsync(lobby.Id);

            if (joinResult.HasValue) //Check if the join was successful (dumb Facepunch API..)
            {
                //This is required to set the CurrentLobby for the Client too! Othervise the client wont see the lobby.
                CurrentLobby = joinResult.Value;
                StartReceiving();
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
            logger.Msg("[Steam] Receive loop started.");
            if (CurrentLobby.Id.IsValid)
            {
                logger.Msg($"[Steam] Current lobby valid: Id={CurrentLobby.Id}"); // Members={CurrentLobby.Members.Count}
                foreach (var m in CurrentLobby.Members)
                {
                    logger.Msg($"[Steam] Lobby member: {m.Id} Name={m.Name} IsMe={m.IsMe}");
                }
            }
            else
            {
                logger.Msg("[Steam] No valid CurrentLobby when starting receive loop.");
            }
            // No-op edit to ensure file update consistency.
            // Patch applied to stabilize file state.
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
                    else
                    {
                        logger.Msg("[Steam] ReadP2PPacket returned no value despite IsP2PPacketAvailable reporting true.");
                    }
                }
                await Task.Delay(16, token); //16ms ~ 60FPS
            }
        }

        private void ProcessIncomingPacket(SteamId senderId, byte[] data)
        {
            if (data == null || data.Length == 0)
            {
                logger.Warning($"[Steam] Ignoring empty packet from {senderId}.");
                return;
            }

            PacketType packetType = (PacketType)data[0];
            logger.Msg($"[Steam] Processing incoming packet from {senderId} length={data.Length} type={(byte)packetType}");

            switch (packetType)
            {
                case PacketType.Movement:
                    if (data.Length < PlayerStatePacket.MinSerializedSize)
                    {
                        logger.Warning($"[Steam] Dropping malformed movement packet from {senderId}: expected at least {PlayerStatePacket.MinSerializedSize} bytes but got {data.Length}.");
                        return;
                    }

                    PlayerStatePacket statePacket = PlayerStatePacket.Deserialize(data);

                    //Do movement
                    HandlePlayerMovement(senderId, statePacket);
                    // Route to in-game component
                    RouteIncomingPlayerState(senderId, statePacket);
                    break;

                default:
                    logger.Warning($"Received unknown packet type ID: {data[0]}");
                    break;
            }
        }

        private void HandlePlayerMovement(SteamId playerSteamId, PlayerStatePacket packet)
        { }

        // Public accessor to route incoming packet to the correct NPC instance
        public void RouteIncomingPlayerState(SteamId playerSteamId, PlayerStatePacket packet)
        {
            if (Core.AvailableNPCs == null || Core.AvailableNPCs.Count == 0)
            {
                logger.Warning($"[Steam] No NPCs available to route packet from {playerSteamId}.");
                return;
            }

            // Find the RemotePlayerNetworkSync attached to any available NPCs matching this playerSteamId
            foreach (var npc in Core.AvailableNPCs)
            {
                if (npc == null) continue;
                var sync = npc.GetComponent<RemotePlayerNetworkSync>();
                if (sync == null) continue;
                if (sync.OwnerId == playerSteamId)
                {
                    sync.OnDataReceived(packet);
                    return;
                }
            }

            // If no matching OwnerId found, assign the first unowned NPC to avoid attaching every packet to a different NPC.
            foreach (var npc in Core.AvailableNPCs)
            {
                if (npc == null) continue;
                var sync = npc.GetComponent<RemotePlayerNetworkSync>();
                if (sync == null || sync.OwnerId.IsValid) continue;

                sync.OwnerId = playerSteamId;
                sync.OnDataReceived(packet);
                logger.Msg($"[Steam] Assigned OwnerId {playerSteamId} to NPC {npc.name}");
                return;
            }

            logger.Warning($"[Steam] Could not route packet from {playerSteamId}: no free NPC slots available.");
        }


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
            StartReceiving();

            return true;
        }

        public void LeaveLobby()
        {
            if (CurrentLobby.Id.IsValid)
            {
                StopReceiving();
                CurrentLobby.Leave();
                CurrentLobby = default;
                logger.Msg("Left the lobby.");
                Core.ShowNotification("You have left the lobby.");
            }
        }

        public static void SendPlayerStatePacket(PlayerStatePacket packet)
        {
            if (!CurrentLobby.Id.IsValid)
            {
                MelonLoader.MelonLogger.Msg("[Steam] SendPlayerStatePacket skipped: no valid CurrentLobby.");
                return;
            }

            byte[] data = packet.Serialize();

            int attempted = 0;
            foreach (var member in CurrentLobby.Members)
            {
                if (member.Id == SteamClient.SteamId) continue;
                attempted++;
                bool ok = SteamNetworking.SendP2PPacket(member.Id, data, data.Length, 0, P2PSend.Unreliable);
                if (!ok) MelonLoader.MelonLogger.Msg($"[Steam] Failed to send P2P packet to {member.Id}");
            }
            MelonLoader.MelonLogger.Msg($"[Steam] SendPlayerStatePacket: attempted sends={attempted}"); // lobbyMembers={CurrentLobby.Members.Count}
        }
    }
}
