using CalMP_Melon;
using Rewired;
using System;
using System.Threading.Tasks;
using UnityEngine;

namespace CalMP_Melon
{
    public class CalGUI
    {
        private static Steam steam => Steam.Instance;

        public static void DrawInterface()
        {
            GUILayout.BeginArea(new Rect(20, 20, 300, 250), GUI.skin.box);

            GUILayout.Label("Cats are Liquid Multiplayer");

            if (steam == null)
            {
                GUILayout.Label("Steam not initialized.");
                GUILayout.EndArea();
                return;
            }

            bool inLobby = steam.CurrentLobby.Id.Value != 0;

            // If the User Hosts a lobby, their "Host Lobby" button will turn into a "Disband Lobby" button
            //
            // If the User Joins a lobby, their "Host Lobby" button will turn into a "Disconnect" button

            if (!inLobby)
            {
                if (GUILayout.Button("Host Lobby"))
                {
                    _ = steam.CreateLobbyAsync(3, "New Lobby");
                }
                GUILayout.Label("Not in a lobby.");
            }
            else
            {
                bool isHost = steam.CurrentLobby.Owner.IsMe;

                if (isHost)
                {
                    if (GUILayout.Button("Disband Lobby"))
                    {
                        steam.LeaveLobby();
                    }
                }
                else
                {
                    if (GUILayout.Button("Disconnect"))
                    {
                        steam.LeaveLobby();
                    }
                }

                GUILayout.Space(10);
                GUILayout.Label($"LobbyID: {steam.CurrentLobby.Id}");

                // Updated the Player list to show Host and "You".

                GUILayout.Label("Players:");
                foreach (var player in steam.CurrentLobby.Members)
                {
                    string role = player.IsMe ? " (You)" : "";
                    if (player.Id == steam.CurrentLobby.Owner.Id) role += " [Host]";

                    GUILayout.Label($" - {player.Name}{role}");
                }
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