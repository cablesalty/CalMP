using CalMP_Melon;
using Rewired;
using System.Threading.Tasks;
using UnityEngine;

namespace CalMP_Melon
{
    public class CalGUI
    {
        private static Steam steam = Steam.Instance;

        public static void DrawInterface()
        {
            GUILayout.BeginArea(new Rect(20, 20, 300, 200), GUI.skin.box);

            GUILayout.Label("Cats are Liquid");

            if (GUILayout.Button("Host Lobby"))
            {
                _ = steam.CreateLobbyAsync(3, "New Lobby");
            }

            GUILayout.Label($"LobbyID: {steam.CurrentLobby.Id}");

            GUILayout.Label("Players:");
            foreach (var player in steam.CurrentLobby.Members)
            {
                GUILayout.Label(" - " + player.Name);
            }

            GUILayout.EndArea();
        }

        public static void ShowNotificationInterface(string message, float timeRemaining)
        {
            GUILayout.BeginArea(new Rect(20, Screen.height - 100, 300, 80), GUI.skin.box);
            GUILayout.Label($"Cats are Liquid Multiplayer ({Math.Round(timeRemaining)}s)\n{message}");
            GUILayout.EndArea();
        }
    }
}