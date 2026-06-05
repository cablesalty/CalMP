using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace CalMP_Melon
{
    internal class LocalPlayerNetworkSync : MonoBehaviour
    {
        private Rigidbody2D rb;

        private GameObject catPart;

        void Start()
        {
            rb = GetComponent<Rigidbody2D>();
        }

        void FixedUpdate()
        {
            catPart = gameObject.transform.Find("Cat Part 0").gameObject;
            SendPlayerState();
            //Core.ShowNotification($"X: {catPart.transform.position.x}, Y: {catPart.transform.position.x}");
        }

        private void SendPlayerState()
        {
            var packet = new PlayerStatePacket
            {
                X = catPart.transform.position.x,
                Y = catPart.transform.position.y,
                // Note to ervin: turns out .velocity is deprecated
                VelocityX = rb != null ? rb.linearVelocity.x : 0f,
                VelocityY = rb != null ? rb.linearVelocity.y : 0f,
                IsInLiquidForm = false // TODO: Implement check //Todo, Todo, Todo
            };

            Steam.SendPlayerStatePacket(packet);
        }
    }
}
