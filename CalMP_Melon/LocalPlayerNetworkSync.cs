using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace CalMP_Melon
{
    internal class LocalPlayerNetworkSync : MonoBehaviour
    {
        private Rigidbody2D rb;
        private float sendTimer;
        private const float SendRate = 1f / 30f; // 30 times per second

        void Start()
        {
            rb = GetComponent<Rigidbody2D>();
        }

        void Update()
        {
            sendTimer += Time.deltaTime;
            if (sendTimer >= SendRate)
            {
                sendTimer = 0f;
                //SendNPCState();
                Core.ShowNotification($"X: {transform.position.x}, Y: {transform.position.y}");
            }
        }

        private void SendNPCState()
        {
            var packet = new PlayerStatePacket
            {
                X = transform.position.x,
                Y = transform.position.y,
                // Note to ervin: turns out .velocity is deprecated
                VelocityX = rb != null ? rb.linearVelocity.x : 0f,
                VelocityY = rb != null ? rb.linearVelocity.y : 0f,
                IsInLiquidForm = false // TODO: Implement check
            };

            Steam.SendPlayerStatePacket(packet);
        }
    }
}
