using System;

namespace game.network.jx
{
    public static class JxClassicMovement
    {
        public const float CoreTickRate = 18f;
        public const float MoveSendInterval = 1f / CoreTickRate;
        public const int RunTargetDistanceMultiplier = 6;
        public const int DefaultWalkSpeed = 5;
        public const int DefaultRunSpeed = 10;

        private const float MinMoveDuration = 1f / 60f;
        private const float MaxMoveDuration = 0.8f;

        public static void EnsureBaseSpeed(game.resource.settings.npcres.Controller controller)
        {
            if (controller == null)
            {
                return;
            }

            if (controller.data.m_WalkSpeed <= 0)
            {
                controller.data.m_WalkSpeed = DefaultWalkSpeed;
            }

            if (controller.data.m_RunSpeed <= 0)
            {
                controller.data.m_RunSpeed = DefaultRunSpeed;
            }

            if (controller.data.m_CurrentWalkSpeed <= 0)
            {
                controller.data.m_CurrentWalkSpeed = controller.data.m_WalkSpeed;
            }

            if (controller.data.m_CurrentRunSpeed <= 0)
            {
                controller.data.m_CurrentRunSpeed = controller.data.m_RunSpeed;
            }
        }

        public static void ApplyCurrentSpeed(
            game.resource.settings.npcres.Controller controller,
            int walkSpeed,
            int runSpeed)
        {
            if (controller == null)
            {
                return;
            }

            EnsureBaseSpeed(controller);

            if (walkSpeed > 0)
            {
                controller.data.m_CurrentWalkSpeed = walkSpeed;
            }

            if (runSpeed > 0)
            {
                controller.data.m_CurrentRunSpeed = runSpeed;
            }
        }

        public static int GetCurrentWalkSpeed(game.resource.settings.npcres.Controller controller)
        {
            EnsureBaseSpeed(controller);
            return Math.Max(DefaultWalkSpeed, controller?.data.m_CurrentWalkSpeed ?? DefaultWalkSpeed);
        }

        public static int GetCurrentRunSpeed(game.resource.settings.npcres.Controller controller)
        {
            EnsureBaseSpeed(controller);
            return Math.Max(DefaultRunSpeed, controller?.data.m_CurrentRunSpeed ?? DefaultRunSpeed);
        }

        public static int GetRunTargetDistance(int runSpeed)
        {
            return Math.Max(DefaultRunSpeed, runSpeed) * RunTargetDistanceMultiplier;
        }

        public static float GetDuration(
            game.resource.map.Position from,
            game.resource.map.Position to,
            int speed)
        {
            if (from == null || to == null)
            {
                return MinMoveDuration;
            }

            int dx = to.left - from.left;
            int dy = (to.top - from.top) * 2;
            double distance = Math.Sqrt((dx * dx) + (dy * dy));
            return GetDuration(distance, speed);
        }

        public static float GetDuration(double mpsDistance, int speed)
        {
            int safeSpeed = Math.Max(1, speed);
            float duration = (float)(Math.Max(1d, mpsDistance) / (safeSpeed * CoreTickRate));

            if (duration < MinMoveDuration)
            {
                return MinMoveDuration;
            }

            if (duration > MaxMoveDuration)
            {
                return MaxMoveDuration;
            }

            return duration;
        }
    }
}
