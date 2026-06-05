using HarmonyLib;
using MelonLoader;
using ProphecySystem;
using Steamworks;
using Steamworks.Data;
using UnityEngine;

[assembly: MelonInfo(typeof(CalMP_Melon.Core), "CalMP_Melon", "1.0.0", "cablesalty feat. antalervin19", null)]
[assembly: MelonGame("Last Quarter Studios", "Cats are Liquid - A Better Place")]

namespace CalMP_Melon
{
    public class Core : MelonMod
    {
        private bool showUI = true;
        private static bool showNotificationUI = false;
        private static string notificationMessage;
        private static float notificationEndTime;

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
            if (Input.GetKeyDown(KeyCode.F2)) InjectLocalCatScript(); // TODO: Remove in first stable build
            if (Input.GetKeyDown(KeyCode.F3)) InjectNPCCatScript();
            if (Input.GetKeyDown(KeyCode.F4))
            {
                //[12:50:43.021] [CalMP_Melon] Object search: Game Center
                //[12:50:43.021] [CalMP_Melon] Object search: Settings Resetter
                //[12:50:43.022] [CalMP_Melon] Object search: Main Camera
                //[12:50:43.022] [CalMP_Melon] Object search: Intro
                //[12:50:43.023] [CalMP_Melon] Object search: EventSystem
                //[12:50:43.023] [CalMP_Melon] Object search: Canvas
                //[12:51:24.880] [CalMP_Melon] Object search: Theme System
                //[12:51:24.880] [CalMP_Melon] Object search: Dev Steam Manager
                //[12:51:24.881] [CalMP_Melon] Object search: Music
                //[12:51:24.881] [CalMP_Melon] Object search: Tutorial System
                //[12:51:24.881] [CalMP_Melon] Object search: Look At
                //[12:51:24.882] [CalMP_Melon] Object search: Prophecy System Items
                //[12:51:24.882] [CalMP_Melon] Object search: Main Camera
                //[12:51:24.882] [CalMP_Melon] Object search: Main Cinemachine Virtual Camera
                //[12:51:24.883] [CalMP_Melon] Object search: Object Pools
                //[12:51:24.883] [CalMP_Melon] Object search: Floater Container
                //[12:51:24.883] [CalMP_Melon] Object search: Level Load Manager
                //[12:51:24.883] [CalMP_Melon] Object search: Canvas Manager
                //[12:51:24.884] [CalMP_Melon] Object search: Slow-mo Cinemachine Virtual Camera
                //[12:51:24.884] [CalMP_Melon] Object search: EventSystem
                //[12:51:24.884] [CalMP_Melon] Object search: PolyMap
                //[12:51:24.885] [CalMP_Melon] Object search: Text History
                //[12:51:24.885] [CalMP_Melon] Object search: Rich Presence Updater
                //[12:51:24.885] [CalMP_Melon] Object search: Cat
                //[12:51:24.886] [CalMP_Melon] Object search: 6677a8de-4995-498a-9c40-54ab809458d9
                //[12:51:24.886] [CalMP_Melon] Object search: NPC Cat(Clone)
                //[12:51:24.886] [CalMP_Melon] Object search: NPC Audio Source(Clone)[12:50:43.021] [CalMP_Melon] Object search: Game Center
                //[12:50:43.021] [CalMP_Melon] Object search: Settings Resetter
                //[12:50:43.022] [CalMP_Melon] Object search: Main Camera
                //[12:50:43.022] [CalMP_Melon] Object search: Intro
                //[12:50:43.023] [CalMP_Melon] Object search: EventSystem
                //[12:50:43.023] [CalMP_Melon] Object search: Canvas
                //[12:51:24.880] [CalMP_Melon] Object search: Theme System
                //[12:51:24.880] [CalMP_Melon] Object search: Dev Steam Manager
                //[12:51:24.881] [CalMP_Melon] Object search: Music
                //[12:51:24.881] [CalMP_Melon] Object search: Tutorial System
                //[12:51:24.881] [CalMP_Melon] Object search: Look At
                //[12:51:24.882] [CalMP_Melon] Object search: Prophecy System Items
                //[12:51:24.882] [CalMP_Melon] Object search: Main Camera
                //[12:51:24.882] [CalMP_Melon] Object search: Main Cinemachine Virtual Camera
                //[12:51:24.883] [CalMP_Melon] Object search: Object Pools
                //[12:51:24.883] [CalMP_Melon] Object search: Floater Container
                //[12:51:24.883] [CalMP_Melon] Object search: Level Load Manager
                //[12:51:24.883] [CalMP_Melon] Object search: Canvas Manager
                //[12:51:24.884] [CalMP_Melon] Object search: Slow-mo Cinemachine Virtual Camera
                //[12:51:24.884] [CalMP_Melon] Object search: EventSystem
                //[12:51:24.884] [CalMP_Melon] Object search: PolyMap
                //[12:51:24.885] [CalMP_Melon] Object search: Text History
                //[12:51:24.885] [CalMP_Melon] Object search: Rich Presence Updater
                //[12:51:24.885] [CalMP_Melon] Object search: Cat
                //[12:51:24.886] [CalMP_Melon] Object search: 6677a8de-4995-498a-9c40-54ab809458d9
                //[12:51:24.886] [CalMP_Melon] Object search: NPC Cat(Clone)
                //[12:51:24.886] [CalMP_Melon] Object search: NPC Audio Source(Clone)
                ShowNotification("Objects dumped into console.");
                foreach (GameObject obj in UnityEngine.SceneManagement.SceneManager.GetActiveScene().GetRootGameObjects())
                {
                    LoggerInstance.Msg("Object search: " + obj.name);
                }
            }
        }

        public override void OnApplicationQuit()
        {
             SteamClient.Shutdown();
        }

        public void OnSceneLoaded()
        {
            InjectLocalCatScript();
        }

        // --- Helpers ---

        public static void ShowNotification(string message, float duration = 5f)
        {
            notificationMessage = message;
            notificationEndTime = Time.time + duration;
            showNotificationUI = true;
        }

        public static void InjectLocalCatScript()
        {
            GameObject localCat = FindLocalCat();

            if (localCat == null || localCat.GetComponent<LocalPlayerNetworkSync>() != null) return;
            localCat.AddComponent<LocalPlayerNetworkSync>();
        }

        public static void InjectNPCCatScript()
        {
            GameObject npcCat = FindNPCCat();

            if (npcCat == null || npcCat.GetComponent<RemotePlayerNetworkSync>() != null) return;
            npcCat.AddComponent<RemotePlayerNetworkSync>();
        }

        public static GameObject FindLocalCat()
        {
            return GameObject.FindWithTag("Player");
        }

        public static GameObject FindNPCCat(string catType = "green")
        {
            return GameObject.Find("NPC Cat(Clone)");
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

        [HarmonyPatch(typeof(UnityEngine.Object), "Instantiate",
    new[] { typeof(UnityEngine.Object) })]
        class InstantiatePatch
        {
            static void Postfix(UnityEngine.Object __result)
            {
                if (__result is GameObject go)
                {
                    MelonLogger.Msg($"Spawned: {go.name}");
                }
            }
        }
    }
}