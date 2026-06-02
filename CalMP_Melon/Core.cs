using MelonLoader;
using UnityEngine;
using Steamworks;
using Steamworks.Data;

[assembly: MelonInfo(typeof(CalMP_Melon.Core), "CalMP_Melon", "1.0.0", "cablesalty feat. antalervin19", null)]
[assembly: MelonGame("Last Quarter Studios", "Cats are Liquid - A Better Place")]

namespace CalMP_Melon
{
    public class Core : MelonMod
    {
        Steam steam;

        private bool showUI = true;

        public override void OnInitializeMelon()
        {
            LoggerInstance.Msg("Steam initialized");
            steam = new Steam(LoggerInstance);
        }

        public override void OnGUI()
        {
            if (!showUI) return;

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


        public override void OnUpdate()
        {
            SteamClient.RunCallbacks();
        }

        public override void OnApplicationQuit()
        {
            SteamClient.Shutdown();
        }
    }
}