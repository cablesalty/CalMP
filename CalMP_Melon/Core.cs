using MelonLoader;
using HarmonyLib;
using UnityEngine;
using Steamworks;
using Steamworks.Data;

[assembly: MelonInfo(typeof(CalMP_Melon.Core), "CalMP_Melon", "1.0.0", "cablesalty feat. antalervin19", null)]
[assembly: MelonGame("Last Quarter Studios", "Cats are Liquid - A Better Place")]

namespace CalMP_Melon
{
    public class Core : MelonMod
    {
        private bool showUI = true;
        private bool showNotificationUI = false;
        private string notificationMessage;
        private float notificationEndTime;

        // --- Event Methods ---

        public override void OnInitializeMelon()
        {
            HarmonyInstance.PatchAll();
            Steam.Init(LoggerInstance);
            ShowNotification(
                "Mod has initialized successfully.\n\n" +
                "Press F1 to open the lobby interface"
            );
        }

        public override void OnGUI()
        {
            if (showUI)
                CalGUI.DrawInterface();

            if (showNotificationUI)
            {
                // I know this shows for a frame after, but frankly, I don't give a fuck
                if (notificationEndTime <= Time.time) showNotificationUI = false;
                CalGUI.ShowNotificationInterface(notificationMessage, notificationEndTime - Time.time);
            }
        }

        public override void OnUpdate()
        {
            SteamClient.RunCallbacks();
            if (Input.GetKeyDown(KeyCode.F1)) showUI = !showUI;
        }

        public override void OnApplicationQuit()
        {
             SteamClient.Shutdown();
        }

        // --- Helpers ---

        public void ShowNotification(string message, float duration = 5f)
        {
            notificationMessage = message;
            notificationEndTime = Time.time + duration;
            showNotificationUI = true;
        }

        // --- Patches ---

        // Cursor patch to always show cursor
        // TODO: Remove in first stable build
        [HarmonyPatch(typeof(CanvasManager), "SetCursorHidden")]
        public class CanvasmanagerPatch()
        {
            static bool Prefix(bool hidden)
            {
                return false;
            }
        }
    }
}