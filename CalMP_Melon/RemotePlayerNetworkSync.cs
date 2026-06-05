using Cat;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace CalMP_Melon
{
    public class RemotePlayerNetworkSync : MonoBehaviour
    {
        private Vector2 targetPosition;
        private Vector2 lastVelocity;
        private bool isLiquid;

        private GameObject catPart;

        private const float InterpolationSpeed = 15f;

        void Start()
        {
            catPart = gameObject.transform.Find("Cat Part 0").gameObject;
            targetPosition = catPart.transform.position;
            // TODO: Add prophecy moves for the cat (for smoother movement)
        }

        void Update()
        {
            catPart.transform.position = Vector2.Lerp(catPart.transform.position, targetPosition, Time.deltaTime * InterpolationSpeed);
            Core.ShowNotification($"X: {catPart.transform.position.x}, Y: {catPart.transform.position.x}");
        }

        public void OnDataReceived(PlayerStatePacket packet)
        {
            targetPosition = new Vector2(packet.X, packet.Y);
            lastVelocity = new Vector2(packet.VelocityX, packet.VelocityY);
            isLiquid = false; // TODO: Handle liquid form state

            ApplyGameStates();
        }

        private void ApplyGameStates()
        {

        }
    }
}