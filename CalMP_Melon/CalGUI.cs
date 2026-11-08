using Steamworks;
using System;
using UnityEngine;

namespace CalMP_Melon
{
    public class CalGUI
    {
        private static Steam steam => Steam.Instance;

        public static void DrawInterface()
        {
            GUILayout.BeginArea(new Rect(20, 20, 320, 350), GUI.skin.box);

            GUILayout.Label("Cats are Liquid Multiplayer");

            if (steam == null)
            {
                GUILayout.Label(Steam.InitializationError == null
                    ? "Waiting for the game to initialize Steam..."
                    : $"Steam unavailable: {Steam.InitializationError}");
                GUILayout.EndArea();
                return;
            }

            if (!Steam.IsInLobby)
            {
                if (GUILayout.Button("Host Lobby"))
                    _ = steam.CreateLobbyAsync(2, "Cats are Liquid co-op");

                GUILayout.Label("Not in a lobby.");
            }
            else
            {
                if (GUILayout.Button("Leave Lobby"))
                    steam.LeaveLobby();

                GUILayout.Space(10);
                GUILayout.Label($"LobbyID: {Steam.CurrentLobby.Id}");

                if (GUILayout.Button("Invite Friend"))
                    SteamFriends.OpenGameInviteOverlay(Steam.CurrentLobby.Id);

                GUILayout.Label("Players:");
                foreach (var player in Steam.CurrentLobby.Members)
                {
                    string role = player.IsMe ? " (You)" : "";
                    if (player.Id == Steam.CurrentLobby.Owner.Id) role += " [Host]";

                    GUILayout.Label($" - {player.Name}{role}");
                }

                GUILayout.Space(5);
                GUILayout.Label("Remote cat: Green (pre-placed NPC required)");
            }

            GUILayout.EndArea();
        }

        public static void ShowNotificationInterface(string message, float timeRemaining)
        {
            GUILayout.BeginArea(new Rect(20, Screen.height - 100, 300, 80), GUI.skin.box);
            GUILayout.Label($"Notification ({Math.Max(0, Math.Round(timeRemaining))}s)\n{message}");
            GUILayout.EndArea();
        }
    }
}