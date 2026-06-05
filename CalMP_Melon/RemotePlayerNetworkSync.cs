using Cat;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using UnityEngine;
using UnityEngine.Playables;

namespace CalMP_Melon
{
    public class RemotePlayerNetworkSync : MonoBehaviour
    {
        private Vector2 targetPosition;
        private Vector2 lastVelocity;
        private bool lastLiquidState;

        private GameObject catPart;

        private const float InterpolationSpeed = 15f;

        void Start()
        {
            catPart = gameObject.transform.Find("Cat Part 0").gameObject;
            targetPosition = catPart.transform.position;
            // TODO: Add prophecy moves for the cat (for smoother movement)
        }

        void FixedUpdate()
        {
            catPart.transform.position = Vector2.Lerp(catPart.transform.position, targetPosition, Time.deltaTime * InterpolationSpeed);
            //Core.ShowNotification($"X: {catPart.transform.position.x}, Y: {catPart.transform.position.x}");
        }

        public void OnDataReceived(PlayerStatePacket packet)
        {
            targetPosition = new Vector2(packet.X, packet.Y);
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
                performActionMethod.Invoke(npcPathfinder, [ catAction, duration ]);
            }
            catch (Exception)
            {
                Core.ShowNotification("Error occoured while trying to execute a prophecy action.");
            }
        }
    }
}