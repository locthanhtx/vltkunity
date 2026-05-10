
namespace game.resource.settings.npcres
{
    public class Structures
    {
        public struct PartSprInfo
        {
            public string sprFullPath;
            public ushort frameCount;
            public int directionCount;
            public int intervalRatio;
        }

        public class PartAnimation
        {
            public string sprPath;
            public ushort frameBegin;
            public ushort frameEnd;
            public int framePerDirection;
            public int framePerSeconds;
            public int frameIntervalTicks;
            public int layerOrder;

            public ushort GetNowFrameIndex(float _delta = 0)
            {
                if (this.framePerDirection <= 0)
                {
                    return this.frameBegin;
                }

                float delta = _delta != 0 ? _delta : UnityEngine.Time.timeSinceLevelLoad;
                int tickRate = this.framePerSeconds > 0 ? this.framePerSeconds : resource.SPR.FPS;

                ushort indexOnCount = (ushort)(UnityEngine.Mathf.FloorToInt(delta * tickRate) % this.framePerDirection);
                return indexOnCount += this.frameBegin;
            }

            public ushort GetFrameIndex(int totalFrame, int currentFrame)
            {
                if (this.framePerDirection <= 0 || totalFrame <= 0)
                {
                    return this.GetNowFrameIndex();
                }

                int safeCurrentFrame = UnityEngine.Mathf.Clamp(currentFrame, 0, totalFrame - 1);
                int indexOnCount = this.framePerDirection * safeCurrentFrame / totalFrame;
                if (indexOnCount >= this.framePerDirection)
                {
                    indexOnCount = this.framePerDirection - 1;
                }

                return (ushort)(this.frameBegin + indexOnCount);
            }
        }
    }
}
