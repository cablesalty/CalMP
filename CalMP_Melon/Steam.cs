using MelonLoader;
using Steamworks;
using Steamworks.Data;
using System;
using System.Threading.Tasks;

namespace CalMP_Melon
{
    public class Steam
    {
        private readonly MelonLogger.Instance logger;
        private static Steam instance;

        public static Lobby CurrentLobby;
        public static Steam Instance => instance;
        public static bool IsInLobby => CurrentLobby.Id.IsValid;
        public static bool IsReady => instance != null;
        public static string InitializationError { get; private set; }

        public static void Init(MelonLogger.Instance logger)
        {
            if (instance != null || InitializationError != null)
                return;

            if (!SteamClient.IsValid)
                return;

            try
            {
                instance = new Steam(logger);
                InitializationError = null;
                logger.Msg("[Steam] Connected to the game's initialized Steamworks client.");
                Core.ShowNotification("Steam multiplayer ready.");
            }
            catch (Exception exception)
            {
                InitializationError = exception.Message;
                logger.Error($"[Steam] Failed to initialize Steamworks: {exception}");
                Core.ShowNotification("Steam initialization failed. Make sure Steam is running.");
            }
        }

        private Steam(MelonLogger.Instance logger)
        {
            this.logger = logger;

            SteamNetworking.AllowP2PPacketRelay(true);
            SteamNetworking.OnP2PSessionRequest += OnP2PSessionRequest;
            SteamNetworking.OnP2PConnectionFailed += OnP2PConnectionFailed;
            SteamMatchmaking.OnLobbyMemberJoined += OnLobbyMemberJoined;
            SteamMatchmaking.OnLobbyMemberLeave += OnMemberLeft;
            SteamFriends.OnGameLobbyJoinRequested += OnGameLobbyJoinRequested;
        }

        private void OnGameLobbyJoinRequested(Lobby lobby, SteamId steamId)
        {
            logger.Msg($"Received a lobby join request for lobby {lobby.Id} from {steamId}.");
            _ = JoinLobbyAsync(lobby);
        }

        public async Task<bool> JoinLobbyAsync(Lobby lobby)
        {
            if (!SteamClient.IsValid)
            {
                Core.ShowNotification("Steam is not initialized.");
                return false;
            }

            if (CurrentLobby.Id == lobby.Id)
                return true;

            if (CurrentLobby.Id.IsValid)
                LeaveLobby();

            Core.ShowNotification("Joining lobby...");
            try
            {
                Lobby? joinResult = await SteamMatchmaking.JoinLobbyAsync(lobby.Id);
                if (!joinResult.HasValue)
                {
                    logger.Error($"Failed to join lobby {lobby.Id}.");
                    Core.ShowNotification("Failed to join lobby.");
                    return false;
                }

                CurrentLobby = joinResult.Value;
                logger.Msg($"Joined lobby {CurrentLobby.Id}.");
                Core.ShowNotification("Joined lobby successfully!");
                return true;
            }
            catch (Exception exception)
            {
                logger.Error($"Failed to join lobby {lobby.Id}: {exception}");
                Core.ShowNotification("Failed to join lobby.");
                return false;
            }
        }

        public void PumpNetwork()
        {
            if (!SteamClient.IsValid || !CurrentLobby.Id.IsValid)
                return;

            while (SteamNetworking.IsP2PPacketAvailable(0))
            {
                var packet = SteamNetworking.ReadP2PPacket();
                if (packet.HasValue)
                    ProcessIncomingPacket(packet.Value.SteamId, packet.Value.Data);
                else
                    break;
            }
        }

        private void ProcessIncomingPacket(SteamId senderId, byte[] data)
        {
            if (!IsLobbyMember(senderId) || senderId == SteamClient.SteamId)
            {
                logger.Warning($"Ignoring movement packet from non-member {senderId}.");
                return;
            }

            if (data == null || data.Length == 0)
            {
                logger.Warning($"Ignoring empty packet from {senderId}.");
                return;
            }

            PacketType packetType = (PacketType)data[0];
            if (packetType != PacketType.Movement)
            {
                logger.Warning($"Ignoring unknown packet type {data[0]} from {senderId}.");
                return;
            }

            if (data.Length < PlayerStatePacket.MinSerializedSize)
            {
                logger.Warning(
                    $"Dropping malformed movement packet from {senderId}: expected at least " +
                    $"{PlayerStatePacket.MinSerializedSize} bytes but received {data.Length}.");
                return;
            }

            PlayerStatePacket statePacket;
            try
            {
                statePacket = PlayerStatePacket.Deserialize(data);
            }
            catch (ArgumentException exception)
            {
                logger.Warning($"Dropping malformed movement packet from {senderId}: {exception.Message}");
                return;
            }

            if (statePacket.CatParts != null)
            {
                foreach (CatPartPosition part in statePacket.CatParts)
                {
                    if (part.IsPresent && (!IsFinite(part.X) || !IsFinite(part.Y)))
                    {
                        logger.Warning($"Dropping movement packet with invalid cat-part positions from {senderId}.");
                        return;
                    }
                }
            }

            RouteIncomingPlayerState(senderId, statePacket);
        }

        public void RouteIncomingPlayerState(SteamId playerSteamId, PlayerStatePacket packet)
        {
            if (!CurrentLobby.Id.IsValid || playerSteamId == SteamClient.SteamId ||
                !IsLobbyMember(playerSteamId))
            {
                return;
            }

            foreach (var npc in Core.AvailableNPCs)
            {
                if (npc == null)
                    continue;

                RemotePlayerNetworkSync sync = npc.GetComponent<RemotePlayerNetworkSync>();
                if (sync != null && sync.OwnerId == playerSteamId)
                {
                    sync.OnDataReceived(packet);
                    return;
                }
            }

            foreach (var npc in Core.AvailableNPCs)
            {
                if (npc == null)
                    continue;

                RemotePlayerNetworkSync sync = npc.GetComponent<RemotePlayerNetworkSync>();
                if (sync == null || sync.OwnerId.IsValid)
                    continue;

                sync.SetOwner(playerSteamId);
                sync.OnDataReceived(packet);
                logger.Msg($"Assigned {playerSteamId} to the green NPC {npc.name}.");
                return;
            }
        }

        private static bool IsFinite(float value)
        {
            return !float.IsNaN(value) && !float.IsInfinity(value);
        }

        private bool IsLobbyMember(SteamId steamId)
        {
            foreach (Friend member in CurrentLobby.Members)
            {
                if (member.Id == steamId)
                    return true;
            }

            return false;
        }

        private void OnP2PSessionRequest(SteamId remoteUser)
        {
            if (!CurrentLobby.Id.IsValid || !IsLobbyMember(remoteUser))
            {
                logger.Warning($"Ignoring P2P session request from non-member {remoteUser}.");
                return;
            }

            if (!SteamNetworking.AcceptP2PSessionWithUser(remoteUser))
                logger.Warning($"Failed to accept P2P session from lobby member {remoteUser}.");
            else
                logger.Msg($"Accepted P2P session from {remoteUser}.");
        }

        private void OnP2PConnectionFailed(SteamId remoteUser, P2PSessionError error)
        {
            logger.Warning($"P2P connection to {remoteUser} failed: {error}.");
        }

        private void OnLobbyMemberJoined(Lobby lobby, Friend member)
        {
            if (!CurrentLobby.Id.IsValid || lobby.Id != CurrentLobby.Id)
                return;

            logger.Msg($"{member.Name} joined the lobby.");
            Core.ShowNotification($"{member.Name} joined the lobby.");
        }

        private void OnMemberLeft(Lobby lobby, Friend member)
        {
            if (CurrentLobby.Id.IsValid && lobby.Id == CurrentLobby.Id)
            {
                foreach (var npc in Core.AvailableNPCs)
                {
                    if (npc == null)
                        continue;

                    RemotePlayerNetworkSync sync = npc.GetComponent<RemotePlayerNetworkSync>();
                    if (sync != null && sync.OwnerId == member.Id)
                        sync.SetOwner(default);
                }

                SteamNetworking.CloseP2PSessionWithUser(member.Id);
                logger.Msg($"{member.Name} left the lobby.");
                Core.ShowNotification($"{member.Name} left the lobby.");
            }
        }

        public async Task<bool> CreateLobbyAsync(int maxPlayers = 2, string lobbyName = "Lobby")
        {
            if (!SteamClient.IsValid)
            {
                Core.ShowNotification("Steam is not initialized.");
                return false;
            }

            if (CurrentLobby.Id.IsValid)
            {
                Core.ShowNotification("You are already in a lobby.");
                return false;
            }

            if (maxPlayers < 2)
            {
                logger.Error("A multiplayer lobby must allow at least two players.");
                Core.ShowNotification("A lobby must allow at least two players.");
                return false;
            }

            try
            {
                Lobby? result = await SteamMatchmaking.CreateLobbyAsync(maxPlayers);
                if (!result.HasValue)
                {
                    logger.Error("Steam failed to create a lobby.");
                    Core.ShowNotification("Failed to create lobby.");
                    return false;
                }

                Lobby lobby = result.Value;
                lobby.SetPublic();
                lobby.SetJoinable(true);
                lobby.SetData("name", lobbyName);
                lobby.MaxMembers = maxPlayers;

                CurrentLobby = lobby;
                logger.Msg($"Created lobby {CurrentLobby.Id}.");
                Core.ShowNotification("Lobby created. Invite a friend to join.");
                return true;
            }
            catch (Exception exception)
            {
                logger.Error($"Failed to create lobby: {exception}");
                Core.ShowNotification("Failed to create lobby.");
                return false;
            }
        }

        public void LeaveLobby()
        {
            if (!CurrentLobby.Id.IsValid)
                return;

            Lobby lobby = CurrentLobby;
            foreach (Friend member in lobby.Members)
            {
                if (!member.IsMe)
                    SteamNetworking.CloseP2PSessionWithUser(member.Id);
            }

            foreach (var npc in Core.AvailableNPCs)
            {
                if (npc == null)
                    continue;

                RemotePlayerNetworkSync sync = npc.GetComponent<RemotePlayerNetworkSync>();
                if (sync != null)
                    sync.SetOwner(default);
            }

            CurrentLobby = default;
            lobby.Leave();
            logger.Msg("Left the lobby.");
            Core.ShowNotification("You have left the lobby.");
        }

        public static void SendPlayerStatePacket(PlayerStatePacket packet)
        {
            if (!SteamClient.IsValid || !CurrentLobby.Id.IsValid)
                return;

            byte[] data = packet.Serialize();
            foreach (Friend member in CurrentLobby.Members)
            {
                if (member.IsMe)
                    continue;

                bool sent = SteamNetworking.SendP2PPacket(
                    member.Id, data, data.Length, 0, P2PSend.Unreliable);
                if (!sent)
                    Instance.logger.Warning($"Failed to send movement packet to {member.Id}.");
            }
        }

        public void Shutdown()
        {
            LeaveLobby();
            SteamMatchmaking.OnLobbyMemberJoined -= OnLobbyMemberJoined;
            SteamMatchmaking.OnLobbyMemberLeave -= OnMemberLeft;
            SteamFriends.OnGameLobbyJoinRequested -= OnGameLobbyJoinRequested;
            SteamNetworking.OnP2PSessionRequest -= OnP2PSessionRequest;
            SteamNetworking.OnP2PConnectionFailed -= OnP2PConnectionFailed;

            instance = null;
        }
    }
}
