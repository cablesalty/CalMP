using Cat;
using UnityEngine;

namespace CalMP_Melon
{
    internal class LocalPlayerNetworkSync : MonoBehaviour
    {
        private CatControls catControls;
        private readonly Transform[] catPartTransforms =
            new Transform[PlayerStatePacket.MaxCatParts];
        private readonly CatPartPosition[] catParts =
            new CatPartPosition[PlayerStatePacket.MaxCatParts];

        private void Start()
        {
            catControls = GetComponent<CatControls>();
            if (catControls == null)
                catControls = GetComponentInChildren<CatControls>();
            CacheCatParts();
        }

        private void FixedUpdate()
        {
            if (!Steam.IsInLobby)
                return;

            if (catControls == null)
            {
                catControls = GetComponent<CatControls>();
                if (catControls == null)
                    catControls = GetComponentInChildren<CatControls>();
            }

            for (int i = 0; i < PlayerStatePacket.MaxCatParts; i++)
            {
                Transform part = catPartTransforms[i];
                catParts[i].IsPresent = part != null;
                if (part == null)
                    continue;

                catParts[i].X = part.position.x;
                catParts[i].Y = part.position.y;
            }

            sbyte movementDirection = 0;
            if (catControls != null)
            {
                float direction = catControls.MovementDirection;
                if (direction > 0.1f)
                    movementDirection = 1;
                else if (direction < -0.1f)
                    movementDirection = -1;
            }

            Steam.SendPlayerStatePacket(new PlayerStatePacket
            {
                CatParts = catParts,
                MovementDirection = movementDirection,
                IsJumping = catControls != null && catControls.Jumping,
                IsInLiquidForm = catControls != null && catControls.CurrentState == State.Liquid,
            });
        }

        private void OnTransformChildrenChanged()
        {
            CacheCatParts();
        }

        private void CacheCatParts()
        {
            for (int i = 0; i < PlayerStatePacket.MaxCatParts; i++)
            {
                catPartTransforms[i] = null;
                catParts[i].IsPresent = false;
            }

            foreach (Transform child in GetComponentsInChildren<Transform>(true))
            {
                if (child == transform)
                    continue;

                for (int i = 0; i < PlayerStatePacket.MaxCatParts; i++)
                {
                    if (child.name == "Cat Part " + i)
                    {
                        catPartTransforms[i] = child;
                        catParts[i].IsPresent = true;
                        break;
                    }
                }
            }
        }
    }
}
