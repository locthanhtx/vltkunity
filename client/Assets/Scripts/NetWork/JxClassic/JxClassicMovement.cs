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
        public const int MaxClassicMoveSpeed = 31;

        private const float MinMoveDuration = 1f / 60f;
        private const float MaxMoveDuration = 2.0f;

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
            else
            {
                controller.data.m_WalkSpeed = NormalizeWalkSpeed(controller.data.m_WalkSpeed);
            }

            if (controller.data.m_RunSpeed <= 0)
            {
                controller.data.m_RunSpeed = DefaultRunSpeed;
            }
            else
            {
                controller.data.m_RunSpeed = NormalizeRunSpeed(controller.data.m_RunSpeed);
            }

            if (controller.data.m_CurrentWalkSpeed <= 0)
            {
                controller.data.m_CurrentWalkSpeed = controller.data.m_WalkSpeed;
            }
            else
            {
                controller.data.m_CurrentWalkSpeed = NormalizeWalkSpeed(controller.data.m_CurrentWalkSpeed);
            }

            if (controller.data.m_CurrentRunSpeed <= 0)
            {
                controller.data.m_CurrentRunSpeed = controller.data.m_RunSpeed;
            }
            else
            {
                controller.data.m_CurrentRunSpeed = NormalizeRunSpeed(controller.data.m_CurrentRunSpeed);
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
                controller.data.m_CurrentWalkSpeed = NormalizeWalkSpeed(walkSpeed);
            }

            if (runSpeed > 0)
            {
                controller.data.m_CurrentRunSpeed = NormalizeRunSpeed(runSpeed);
            }
        }

        public static int GetCurrentWalkSpeed(game.resource.settings.npcres.Controller controller)
        {
            EnsureBaseSpeed(controller);
            return NormalizeWalkSpeed(controller?.data.m_CurrentWalkSpeed ?? DefaultWalkSpeed);
        }

        public static int GetCurrentRunSpeed(game.resource.settings.npcres.Controller controller)
        {
            EnsureBaseSpeed(controller);
            return NormalizeRunSpeed(controller?.data.m_CurrentRunSpeed ?? DefaultRunSpeed);
        }

        public static int GetRunTargetDistance(int runSpeed)
        {
            return NormalizeRunSpeed(runSpeed) * RunTargetDistanceMultiplier;
        }

        public static int NormalizeWalkSpeed(int speed)
        {
            return NormalizeMoveSpeed(speed, DefaultWalkSpeed);
        }

        public static int NormalizeRunSpeed(int speed)
        {
            return NormalizeMoveSpeed(speed, DefaultRunSpeed);
        }

        public static int NormalizeMoveSpeed(int speed, int minimum)
        {
            return Math.Min(MaxClassicMoveSpeed, Math.Max(minimum, speed));
        }

        public static UnityEngine.Vector2 ToMpsPosition(game.resource.map.Position position)
        {
            if (position == null)
            {
                return UnityEngine.Vector2.zero;
            }

            return new UnityEngine.Vector2(position.left, position.top * 2f);
        }

        public static game.resource.map.Position ToMapPosition(UnityEngine.Vector2 mpsPosition)
        {
            return new game.resource.map.Position(
                UnityEngine.Mathf.RoundToInt(mpsPosition.y / 2f),
                UnityEngine.Mathf.RoundToInt(mpsPosition.x));
        }

        public static UnityEngine.Vector2 AdvanceMpsPosition(
            UnityEngine.Vector2 mpsPosition,
            int direction,
            float distance)
        {
            if (direction < 0 || direction > 63 || distance <= 0f)
            {
                return mpsPosition;
            }

            return new UnityEngine.Vector2(
                mpsPosition.x + (game.resource.settings.skill.Static.g_DirCos(direction, 64) * distance / 1024f),
                mpsPosition.y + (game.resource.settings.skill.Static.g_DirSin(direction, 64) * distance / 1024f));
        }

        public static int GetDirection(
            UnityEngine.Vector2 fromMps,
            UnityEngine.Vector2 toMps)
        {
            int direction = game.resource.settings.skill.Static.g_GetDirIndex(
                UnityEngine.Mathf.RoundToInt(fromMps.x),
                UnityEngine.Mathf.RoundToInt(fromMps.y),
                UnityEngine.Mathf.RoundToInt(toMps.x),
                UnityEngine.Mathf.RoundToInt(toMps.y));

            return direction >= 0 && direction <= 63 ? direction : -1;
        }

        public static int GetDirection(
            game.resource.map.Position from,
            game.resource.map.Position to)
        {
            if (from == null || to == null)
            {
                return -1;
            }

            return GetDirection(from, to.top, to.left);
        }

        public static int GetDirection(
            game.resource.map.Position from,
            int targetTop,
            int targetLeft)
        {
            if (from == null)
            {
                return -1;
            }

            int direction = game.resource.settings.skill.Static.g_GetDirIndex(
                from.left,
                from.top * 2,
                targetLeft,
                targetTop * 2);

            return direction >= 0 && direction <= 63 ? direction : -1;
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
            int safeSpeed = NormalizeMoveSpeed(speed, 1);
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
