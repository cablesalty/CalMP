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
        private bool showNotificationUI = true;
        private bool showUI = true;

        public override void OnInitializeMelon()
        {
            HarmonyInstance.PatchAll();
            Steam.Init(LoggerInstance);
        }

        public override void OnGUI()
        {
            if (showUI)
                CalGUI.DrawInterface();

            if (showNotificationUI)
                CalGUI.ShowNotification("Kabbeszalty");
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

        //Patch Cursor Hiding

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