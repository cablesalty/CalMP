using Cat;
using Steamworks;
using System;
using UnityEngine;

namespace CalMP_Melon
{
    public class RemotePlayerNetworkSync : MonoBehaviour
    {
        private const float PositionCorrectionSpeed = 15f;

        private readonly Transform[] catParts = new Transform[PlayerStatePacket.MaxCatParts];
        private readonly Rigidbody2D[] catPartBodies = new Rigidbody2D[PlayerStatePacket.MaxCatParts];
        private readonly Vector2[] targetPositions = new Vector2[PlayerStatePacket.MaxCatParts];

        private NPCPathfinder npcPathfinder;
        private CatControls catControls;
        private PlayerStatePacket pendingPacket;
        private bool hasPendingPacket;
        private bool hasReceivedPacket;
        private sbyte movementDirection;
        private bool isJumping;
        private bool isInLiquidForm;

        public SteamId OwnerId;

        private void Start()
        {
            CacheComponents();
            CacheCatParts();

            if (catControls != null)
                isInLiquidForm = catControls.CurrentState == State.Liquid;
        }

        private void FixedUpdate()
        {
            if (!OwnerId.IsValid || !hasReceivedPacket)
                return;

            ApplyPendingPacket();
            CorrectCatPartPositions();
        }

        public void SetOwner(SteamId ownerId)
        {
            if (OwnerId == ownerId)
                return;

            if (OwnerId.IsValid)
                CancelActions();

            OwnerId = ownerId;
            movementDirection = 0;
            isJumping = false;
            hasReceivedPacket = false;
            CacheComponents();

            if (OwnerId.IsValid)
            {
                if (catControls != null)
                    isInLiquidForm = catControls.CurrentState == State.Liquid;

                InvokeProphecyAction(CatAction.StopMoving);
                InvokeProphecyAction(CatAction.StopClimb);
            }
        }

        public void OnDataReceived(PlayerStatePacket packet)
        {
            pendingPacket = packet;
            hasPendingPacket = true;
            hasReceivedPacket = true;
        }

        private void ApplyPendingPacket()
        {
            if (!hasPendingPacket)
                return;

            PlayerStatePacket packet = pendingPacket;
            hasPendingPacket = false;

            if (packet.CatParts != null)
            {
                int count = Math.Min(packet.CatParts.Length, PlayerStatePacket.MaxCatParts);
                for (int i = 0; i < count; i++)
                {
                    if (!packet.CatParts[i].IsPresent)
                        continue;

                    targetPositions[i] = new Vector2(packet.CatParts[i].X, packet.CatParts[i].Y);
                }
            }

            if (packet.MovementDirection != movementDirection)
            {
                if (movementDirection != 0)
                    InvokeProphecyAction(CatAction.StopMoving);

                if (packet.MovementDirection < 0)
                    InvokeProphecyAction(CatAction.MoveLeft);
                else if (packet.MovementDirection > 0)
                    InvokeProphecyAction(CatAction.MoveRight);

                movementDirection = packet.MovementDirection;
            }

            if (packet.IsJumping != isJumping)
            {
                if (packet.IsJumping)
                {
                    InvokeProphecyAction(CatAction.Climb);
                    InvokeProphecyAction(CatAction.Jump);
                }
                else
                {
                    InvokeProphecyAction(CatAction.StopClimb);
                }

                isJumping = packet.IsJumping;
            }

            if (packet.IsInLiquidForm != isInLiquidForm)
            {
                InvokeProphecyAction(packet.IsInLiquidForm
                    ? CatAction.SwitchToLiquidState
                    : CatAction.SwitchToDefaultState);
                isInLiquidForm = packet.IsInLiquidForm;
            }
        }

        private void CorrectCatPartPositions()
        {
            float interpolation = 1f - Mathf.Exp(-PositionCorrectionSpeed * Time.fixedDeltaTime);
            for (int i = 0; i < catParts.Length; i++)
            {
                Transform part = catParts[i];
                if (part == null)
                    continue;

                Vector2 correctedPosition = Vector2.Lerp(
                    part.position, targetPositions[i], interpolation);

                Rigidbody2D body = catPartBodies[i];
                if (body != null)
                    body.MovePosition(correctedPosition);
                else
                    part.position = new Vector3(
                        correctedPosition.x, correctedPosition.y, part.position.z);
            }
        }

        private void CacheComponents()
        {
            if (npcPathfinder == null)
                npcPathfinder = GetComponent<NPCPathfinder>();

            if (catControls == null)
            {
                catControls = GetComponent<CatControls>();
                if (catControls == null)
                    catControls = GetComponentInChildren<CatControls>();
            }
        }

        private void OnTransformChildrenChanged()
        {
            CacheCatParts();
        }

        private void CacheCatParts()
        {
            for (int i = 0; i < catParts.Length; i++)
            {
                catParts[i] = null;
                catPartBodies[i] = null;
            }

            foreach (Transform child in GetComponentsInChildren<Transform>(true))
            {
                if (child == transform)
                    continue;

                for (int i = 0; i < PlayerStatePacket.MaxCatParts; i++)
                {
                    if (child.name != "Cat Part " + i)
                        continue;

                    catParts[i] = child;
                    catPartBodies[i] = child.GetComponent<Rigidbody2D>();
                    targetPositions[i] = child.position;
                    break;
                }
            }
        }

        private void CancelActions()
        {
            if (movementDirection != 0)
                InvokeProphecyAction(CatAction.StopMoving);
            if (isJumping)
                InvokeProphecyAction(CatAction.StopClimb);
            if (isInLiquidForm)
                InvokeProphecyAction(CatAction.SwitchToDefaultState);

            movementDirection = 0;
            isJumping = false;
            isInLiquidForm = false;
        }

        private void InvokeProphecyAction(CatAction action)
        {
            if (npcPathfinder == null)
            {
                CacheComponents();
                if (npcPathfinder == null)
                {
                    MelonLoader.MelonLogger.Warning(
                        $"[RemotePlayerNetworkSync] Cannot apply {action}: NPCPathfinder is missing.");
                    return;
                }
            }

            try
            {
                npcPathfinder.PerformAction(action, 0f);
            }
            catch (Exception exception)
            {
                MelonLoader.MelonLogger.Error(
                    $"[RemotePlayerNetworkSync] Failed to apply prophecy action {action}: {exception}");
                Core.ShowNotification($"Could not apply remote cat action: {action}.");
            }
        }
    }
}
