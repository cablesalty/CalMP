using CalMP_Melon;
using UnityEngine;
using System.Threading.Tasks;

namespace CalMP_Melon
{
    // TODO: Simplify how notifications are handled
    public class Notification
    {
        public string Message;
        public float EndTime;
    }

    public class CalGUI
    {
        private static Steam steam = Steam.Instance;
        private static Notification activeNotification;

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

        public static void ShowNotification(string message, float time = 5f)
        {
            activeNotification = new Notification
            {
                Message = message,
                EndTime = Time.time + time * 1000
            };

        }

        public static void DrawNotificationInterface()
        {
            if (activeNotification == null)
                return;

            if (Time.time > activeNotification.EndTime)
            {
                activeNotification = null;
                return;
            }

            GUILayout.BeginArea(new Rect(Screen.width - 320, Screen.height - 100, 300, 80), GUI.skin.box);
            GUILayout.Label(activeNotification.Message);
            GUILayout.EndArea();
        }
    }
}