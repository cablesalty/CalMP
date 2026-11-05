using Cat;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using UnityEngine;
using UnityEngine.Playables;
using Steamworks;

namespace CalMP_Melon
{
    public class RemotePlayerNetworkSync : MonoBehaviour
    {
        private Vector3 targetPosition;
        private Vector2 lastVelocity;
        private bool lastLiquidState;

        private GameObject catPart;
        // Thread-safe packet handoff: network thread enqueues last packet, main thread applies it in FixedUpdate
        private PlayerStatePacket pendingPacket;
        private readonly object packetLock = new object();
        private bool hasPendingPacket = false;

        public SteamId OwnerId;

        private const float InterpolationSpeed = 15f;

        void Start()
        {
            var found = gameObject.transform.Find("Cat Part 0");
            if (found != null)
            {
                catPart = found.gameObject;
                targetPosition = catPart.transform.position;
            }
            else
            {
                Debug.Log("[RemotePlayerNetworkSync] Could not find 'Cat Part 0' on object. Falling back to root transform.");
                catPart = gameObject;
                targetPosition = gameObject.transform.position;
            }
    MelonLoader.MelonLogger.Msg($"[RemotePlayerNetworkSync] Started for OwnerId={OwnerId} initialPos={targetPosition}"); // touched for consistency.
            // TODO: Add prophecy moves for the cat (for smoother movement)
        }

        void FixedUpdate()
        {
            if (catPart == null) return;
            // Apply pending network packet on main thread if present
            PlayerStatePacket? packetToApply = null;
            lock (packetLock)
            {
                if (hasPendingPacket)
                {
                    packetToApply = pendingPacket;
                    hasPendingPacket = false;
                }
            }

            if (packetToApply.HasValue)
            {
                var packet = packetToApply.Value;
                // Update targetPosition directly (keeps z)
                targetPosition = new Vector3(packet.X, packet.Y, catPart.transform.position.z);
                lastVelocity = new Vector2(packet.VelocityX, packet.VelocityY);
                if (packet.IsInLiquidForm != lastLiquidState)
                {
                    if (packet.IsInLiquidForm)
                        InvokeProphecyAction("SwitchToLiquidState");
                    else
                        InvokeProphecyAction("SwitchToDefaultState");

                    lastLiquidState = packet.IsInLiquidForm;
                }
            }

            // Use fixedDeltaTime in FixedUpdate for stable interpolation
            catPart.transform.position = Vector3.Lerp(catPart.transform.position, targetPosition, Time.fixedDeltaTime * InterpolationSpeed);
            //Core.ShowNotification($"X: {catPart.transform.position.x}, Y: {catPart.transform.position.x}");
        }

        public void OnDataReceived(PlayerStatePacket packet)
        {
            // Log incoming network data for debugging
            MelonLoader.MelonLogger.Msg($"[RemotePlayerNetworkSync] OnDataReceived called for OwnerId={OwnerId} rawPacket: X={packet.X} Y={packet.Y} VX={packet.VelocityX} VY={packet.VelocityY} IsInLiquidForm={packet.IsInLiquidForm}");

            // Enqueue packet for main-thread application in FixedUpdate
            lock (packetLock)
            {
                pendingPacket = packet;
                hasPendingPacket = true;
            }
        }

        private void InvokeProphecyAction(string actionName, float duration = 0.0f)
        {
            // Taken from older mod
            Component npcPathfinder = gameObject.GetComponent("NPCPathfinder");
            if (npcPathfinder == null) return;

            MethodInfo performActionMethod = npcPathfinder.GetType().GetMethod("PerformAction", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            if (performActionMethod == null) return;
            Type catActionType = performActionMethod.GetParameters()[0].ParameterType;

            try
            {
                object catAction = Enum.Parse(catActionType, actionName);
                // Build parameter array for Invoke
                object[] parameters = new object[] { catAction, duration };
                performActionMethod.Invoke(npcPathfinder, parameters);
            }
            catch (Exception)
            {
                Core.ShowNotification("Error occoured while trying to execute a prophecy action.");
            }
        }
    }
}