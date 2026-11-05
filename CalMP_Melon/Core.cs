using Cat;
using HarmonyLib;
using MelonLoader;
using ProphecySystem;
using Steamworks;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;

[assembly: MelonInfo(typeof(CalMP_Melon.Core), "CalMP_Melon", "1.0.0", "cablesalty feat. antalervin19", null)]
[assembly: MelonGame("Last Quarter Studios", "Cats are Liquid - A Better Place")]

namespace CalMP_Melon
{
    public class Core : MelonMod
    {
        private bool showUI = true;
        private static bool showNotificationUI;
        private static string notificationMessage;
        private static float notificationEndTime;
        private float nextNetworkDiscoveryTime;

        public static List<GameObject> AvailableNPCs = new List<GameObject>();

        public override void OnInitializeMelon()
        {
            HarmonyInstance.PatchAll();
            ShowNotification(
                "Mod initialized successfully.\n\n" +
                "Press F1 to open the lobby interface");
        }

        public override void OnGUI()
        {
            if (showUI)
                CalGUI.DrawInterface();

            if (showNotificationUI)
            {
                if (notificationEndTime <= Time.time)
                    showNotificationUI = false;
                CalGUI.ShowNotificationInterface(
                    notificationMessage, notificationEndTime - Time.time);
            }
        }

        public override void OnUpdate()
        {
            if (Steam.Instance == null && SteamClient.IsValid)
                Steam.Init(LoggerInstance);

            Steam.Instance?.PumpNetwork();

            if (Input.GetKeyDown(KeyCode.F1))
                showUI = !showUI;

            if (Time.unscaledTime < nextNetworkDiscoveryTime)
                return;

            nextNetworkDiscoveryTime = Time.unscaledTime + 1f;
            if (Steam.IsInLobby)
            {
                InjectLocalCatScript();
                InjectNPCCatScript();
            }
        }

        public override void OnSceneWasLoaded(int buildIndex, string sceneName)
        {
            AvailableNPCs.Clear();
            nextNetworkDiscoveryTime = 0f;
        }

        public override void OnApplicationQuit()
        {
            Steam.Instance?.Shutdown();
        }

        public static void ShowNotification(string message, float duration = 5f)
        {
            notificationMessage = message;
            notificationEndTime = Time.time + duration;
            showNotificationUI = true;
        }

        public static void InjectLocalCatScript()
        {
            GameObject localCat = FindLocalCat();
            if (localCat == null || localCat.GetComponent<LocalPlayerNetworkSync>() != null)
                return;

            localCat.AddComponent<LocalPlayerNetworkSync>();
            MelonLogger.Msg("[Core] Attached network sync to the local player.");
        }

        public static void InjectNPCCatScript()
        {
            List<GameObject> greenCats = FindNPCCat("green");
            GameObject greenCat = AvailableNPCs.FirstOrDefault(
                npc => npc != null && greenCats.Contains(npc));
            if (greenCat == null)
                greenCat = greenCats.FirstOrDefault();

            AvailableNPCs.RemoveAll(npc => npc == null || npc != greenCat);
            if (greenCat == null)
                return;

            if (!AvailableNPCs.Contains(greenCat))
                AvailableNPCs.Add(greenCat);

            if (greenCat.GetComponent<RemotePlayerNetworkSync>() == null)
            {
                greenCat.AddComponent<RemotePlayerNetworkSync>();
                MelonLogger.Msg("[Core] Attached network sync to the green NPC.");
            }
        }

        public static GameObject FindLocalCat()
        {
            return GameObject.FindWithTag("Player");
        }

        public static List<GameObject> FindNPCCat(string catType = "green")
        {
            List<GameObject> matchingCats = new List<GameObject>();
            GameObject[] allObjects = UnityEngine.Object.FindObjectsByType<GameObject>(
                FindObjectsSortMode.None);

            foreach (GameObject gameObject in allObjects)
            {
                if (!gameObject.name.Contains("NPC Cat(Clone)"))
                    continue;

                NPCPathfinder pathfinder = gameObject.GetComponent<NPCPathfinder>();
                if (pathfinder == null)
                    continue;

                FieldInfo colorField = pathfinder.GetType().GetField(
                    "npcColor", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
                object color = colorField?.GetValue(pathfinder);
                if (color != null &&
                    string.Equals(color.ToString(), catType, StringComparison.OrdinalIgnoreCase))
                {
                    matchingCats.Add(gameObject);
                }
            }

            return matchingCats;
        }

        [HarmonyPatch(typeof(CanvasManager), "SetCursorHidden")]
        public class CanvasmanagerPatch
        {
            private static bool Prefix(bool hidden)
            {
                return false;
            }
        }
    }
}
