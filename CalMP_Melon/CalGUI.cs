
using Steamworks;
using System;
using UnityEngine;
using System.Linq;

namespace CalMP_Melon
{
    public class CalGUI
    {
        private static Steam steam => Steam.Instance;

        private static GUIStyle windowStyle, titleStyle, labelStyle, buttonStyle, mutedStyle;
        private static Texture2D backgroundTexture, buttonTexture, buttonHoverTexture, buttonActiveTexture;
        private static float MaxwindowHeight = 350f;
        private static float PlayerRowHeight = 20f;
        private static float MaxPlayerListHeight = 200f;

        private static bool stylesInitialized;
        private static Vector2 playerScroll;

        private static Texture2D CreateGUITexture(Color color)
        {
            var texture = new Texture2D(1, 1);
            texture.SetPixel(0, 0, color);
            texture.Apply();
            return texture;
        }

        private static void OnInitializeGUI()
        {
            if (stylesInitialized)
                return;

            backgroundTexture = CreateGUITexture(new Color(0.15f, 0.15f, 0.15f));
            buttonTexture = CreateGUITexture(new Color(0.20f, 0.20f, 0.20f));
            buttonHoverTexture = CreateGUITexture(new Color(0.30f, 0.30f, 0.30f));
            buttonActiveTexture = CreateGUITexture(new Color(0.20f, 0.30f, 0.20f));

            windowStyle = new GUIStyle(GUI.skin.box)
            {
                normal = { background = backgroundTexture },
                padding = new RectOffset(12, 12, 10, 10),
                border = new RectOffset(1, 1, 1, 1)
            };

            titleStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = 14,
                fontStyle = FontStyle.Bold,
                normal = { textColor = new Color(0.90f, 0.90f, 0.90f) },
                margin = new RectOffset(0, 0, 0, 10)
            };

            labelStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = 12,
                normal = { textColor = new Color(0.80f, 0.80f, 0.80f) }
            };

            mutedStyle = new GUIStyle(labelStyle)
            {
                normal = { textColor = new Color(0.55f, 0.60f, 0.55f) }
            };

            buttonStyle = new GUIStyle(GUI.skin.button)
            {
                fontSize = 12,
                fontStyle = FontStyle.Normal,
                fixedHeight = 28,
                alignment = TextAnchor.MiddleCenter,
                normal =
                {
                    background = buttonTexture,
                    textColor = new Color(0.90f, 0.90f, 0.90f)
                },
                hover =
                {
                    background = buttonHoverTexture,
                    textColor = Color.white
                },
                active =
                {
                    background = buttonActiveTexture,
                    textColor = Color.white
                },
                focused =
                {
                    background = buttonTexture,
                    textColor = Color.white
                },
                border = new RectOffset(3, 3, 3, 3),
                padding = new RectOffset(8, 8, 4, 4)
            };

            stylesInitialized = true;
        }

        public static void DrawInterface()
        {
            OnInitializeGUI();

            int playerCount = Steam.IsInLobby ? Steam.CurrentLobby.Members.Count() : 0;
            float playerListHeight = Mathf.Min(playerCount * PlayerRowHeight, MaxPlayerListHeight);
            float windowHeight = Steam.IsInLobby ? 145f + playerListHeight : 105f;

            windowHeight = Mathf.Min(windowHeight, MaxwindowHeight, Screen.height - 40f);

            GUILayout.BeginArea(new Rect(20, 20, 320, windowHeight), windowStyle);

            GUILayout.Label("Cats are Liquid Multiplayer", titleStyle);

            if (steam == null)
            {
                GUILayout.Label(Steam.InitializationError == null
                    ? "Waiting for Steam..."
                    : $"Steam unavailable: {Steam.InitializationError}", mutedStyle);

                GUILayout.EndArea();
                return;
            }

            if (!Steam.IsInLobby)
            {
                if (GUILayout.Button("Host Lobby", buttonStyle))
                    _ = steam.CreateLobbyAsync(2, "Cats are Liquid co-op");

                GUILayout.Space(5);
                GUILayout.Label("Not in a lobby.", mutedStyle);
            }
            else
            {
                GUILayout.Label($"Lobby: {Steam.CurrentLobby.Id}", labelStyle);

                GUILayout.Space(8);
                GUILayout.Label($"Players ({Steam.CurrentLobby.Members.Count()}):", labelStyle);

                foreach (var player in Steam.CurrentLobby.Members)
                {
                    string role = player.IsMe ? " (You)" : "";

                    if (player.Id == Steam.CurrentLobby.Owner.Id)
                        role += " [Host]";

                    GUILayout.Label($"  {player.Name}{role}", labelStyle);
                }

                GUILayout.Space(5);
                GUILayout.BeginHorizontal();

                if (GUILayout.Button("Invite", buttonStyle))
                    SteamFriends.OpenGameInviteOverlay(Steam.CurrentLobby.Id);

                if (GUILayout.Button("Leave Lobby", buttonStyle))
                    steam.LeaveLobby();

                GUILayout.EndHorizontal();
            }

            GUILayout.EndArea();
        }

        public static void ShowNotificationInterface(string message, float timeRemaining)
        {
            OnInitializeGUI();

            GUILayout.BeginArea(new Rect(20, Screen.height - 100, 300, 80), windowStyle);

            GUILayout.Label(
                $"Notification ({Math.Max(0, Math.Round(timeRemaining))}s)",
                titleStyle
            );

            GUILayout.Label(message, labelStyle);

            GUILayout.EndArea();
        }
    }
}
