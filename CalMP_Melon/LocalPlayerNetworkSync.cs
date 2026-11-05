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
            // Cache the cat part once at start to avoid expensive/find issues in FixedUpdate
            var found = gameObject.transform.Find("Cat Part 0");
            if (found != null)
            {
                catPart = found.gameObject;
            }
            else
            {
                MelonLoader.MelonLogger.Msg("[LocalPlayerNetworkSync] Could not find 'Cat Part 0' on object. Falling back to root.");
                catPart = gameObject;
            }
        }

        void FixedUpdate()
        {
            // Ensure we still have a valid reference
            if (catPart == null)
            {
                var found = gameObject.transform.Find("Cat Part 0");
                if (found != null) catPart = found.gameObject;
                else catPart = gameObject;
            }

            SendPlayerState();
            //Core.ShowNotification($"X: {catPart.transform.position.x}, Y: {catPart.transform.position.x}");
        }

        private void SendPlayerState()
        {
            var packet = new PlayerStatePacket
            {
                X = catPart.transform.position.x,
                Y = catPart.transform.position.y,
                // Use Rigidbody2D.velocity which is the standard API
                VelocityX = rb != null ? rb.velocity.x : 0f,
                VelocityY = rb != null ? rb.velocity.y : 0f,
                IsInLiquidForm = false // TODO: Implement check //Todo, Todo, Todo
            };

            MelonLoader.MelonLogger.Msg($"[LocalPlayerNetworkSync] Sending packet: X={packet.X} Y={packet.Y} VX={packet.VelocityX} VY={packet.VelocityY} lobbyValid={Steam.CurrentLobby.Id.IsValid}");
            Steam.SendPlayerStatePacket(packet);
        }
    }
}
