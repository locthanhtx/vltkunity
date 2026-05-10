
using System.Collections.Generic;

namespace game.resource.settings.skill.texture
{
    public class SprCache
    {
        private static readonly HashSet<string> missingSprLogs = new HashSet<string>();
        private static readonly HashSet<string> frameFailureLogs = new HashSet<string>();

        public class Data
        {
            public class SprFrame
            {
                public UnityEngine.Sprite sprite;
                public UnityEngine.Vector2 sizeDelta;
                public UnityEngine.Vector2 anchoredPosition;
            }

            public resource.SPR.Info sprInfo;
            public Dictionary<ushort, skill.texture.SprCache.Data.SprFrame> sprFrame; // [frame.index] --> <...>

            public Data()
            {
                this.sprFrame = new Dictionary<ushort, SprFrame>();
            }
        }

        ////////////////////////////////////////////////////////////////////////////////

        private static skill.texture.SprCache.Data.SprFrame CreateSprFrame(
            Dictionary<string, skill.texture.SprCache.Data> storage, string sprPath, ushort frameIndex)
        {
            if (string.IsNullOrWhiteSpace(sprPath) || storage.ContainsKey(sprPath) == false)
            {
                return null;
            }

            resource.SPR.Info sprInfo = storage[sprPath].sprInfo;

            if(sprInfo == null || sprInfo.frameCount <= frameIndex)
            {
                string frameKey = sprPath + "#" + frameIndex + ":range";
                if (frameFailureLogs.Add(frameKey))
                {
                    UnityEngine.Debug.LogWarning(
                        "SkillProbe spr-frame unavailable path=" + sprPath +
                        " frame=" + frameIndex +
                        " frameCount=" + (sprInfo != null ? sprInfo.frameCount : 0));
                }
                return null;
            }

            resource.SPR.FrameInfo frameInfo = Game.Resource(sprPath).Get<resource.SPR.FrameInfo>(frameIndex);

            if(frameInfo == null
                || frameInfo.width == 0)
            {
                string frameKey = sprPath + "#" + frameIndex + ":info";
                if (frameFailureLogs.Add(frameKey))
                {
                    UnityEngine.Debug.LogWarning(
                        "SkillProbe spr-frame-info missing path=" + sprPath +
                        " frame=" + frameIndex);
                }
                return null;
            }

            UnityEngine.Sprite sprite = Game.Resource(sprPath).Get<UnityEngine.Sprite>(frameInfo);

            if(sprite == null)
            {
                string frameKey = sprPath + "#" + frameIndex + ":sprite";
                if (frameFailureLogs.Add(frameKey))
                {
                    UnityEngine.Debug.LogWarning(
                        "SkillProbe spr-frame-sprite missing path=" + sprPath +
                        " frame=" + frameIndex +
                        " size=" + frameInfo.width + "x" + frameInfo.height);
                }
                return null;
            }

            skill.texture.SprCache.Data.SprFrame sprFrame = new Data.SprFrame();

            sprFrame.sprite = sprite;
            sprFrame.sizeDelta = new UnityEngine.Vector2(frameInfo.width / 100.0f, frameInfo.height / 100.0f);
            sprFrame.anchoredPosition = new UnityEngine.Vector2();
            sprFrame.anchoredPosition.x = -0 + (frameInfo.offsetX / 100.0f) + (frameInfo.width / 100.0f / 2);
            sprFrame.anchoredPosition.y = +0 - (frameInfo.offsetY / 100.0f) - (frameInfo.height / 100.0f / 2);

            if (sprInfo.centerX > 0 || sprInfo.centerY > 0)
            {
                sprFrame.anchoredPosition.x -= (sprInfo.centerX / 100.0f);
                sprFrame.anchoredPosition.y += (sprInfo.centerY / 100.0f);
            }
            else if (sprInfo.width > 160)
            {
                sprFrame.anchoredPosition.x -= 1.6f;
                sprFrame.anchoredPosition.y += 1.92f;
            }

            storage[sprPath].sprFrame[frameIndex] = sprFrame;

            return sprFrame;
        }

        private static resource.SPR.Info CreateSpr(
            Dictionary<string, skill.texture.SprCache.Data> storage, string sprPath)
        {
            if (string.IsNullOrWhiteSpace(sprPath))
            {
                return null;
            }

            skill.texture.SprCache.Data newSpr = new skill.texture.SprCache.Data();
            newSpr.sprInfo = Game.Resource(sprPath).Get<resource.SPR.Info>();

            if(newSpr.sprInfo == null)
            {
                if (missingSprLogs.Add(sprPath))
                {
                    UnityEngine.Debug.LogWarning("SkillProbe spr-info missing path=" + sprPath);
                }
                return null;
            }

            storage[sprPath] = newSpr;

            return newSpr.sprInfo;
        }

        ////////////////////////////////////////////////////////////////////////////////

        public static resource.SPR.Info GetSprInfo(
            Dictionary<string, skill.texture.SprCache.Data> storage, string sprPath)
        {
            if (string.IsNullOrWhiteSpace(sprPath))
            {
                return null;
            }

            if(storage.ContainsKey(sprPath) == true)
            {
                return storage[sprPath].sprInfo;
            }

            return skill.texture.SprCache.CreateSpr(storage, sprPath);
        }

        public static skill.texture.SprCache.Data.SprFrame GetSprFrame(
            Dictionary<string, skill.texture.SprCache.Data> storage, string sprPath, ushort frameIndex)
        {
            if (string.IsNullOrWhiteSpace(sprPath))
            {
                return null;
            }

            if(storage.ContainsKey(sprPath) == true)
            {
                if(storage[sprPath].sprFrame.ContainsKey(frameIndex) == true)
                {
                    return storage[sprPath].sprFrame[frameIndex];
                }
                else
                {
                    return skill.texture.SprCache.CreateSprFrame(storage, sprPath, frameIndex);
                }
            }
            else
            {
                if (skill.texture.SprCache.CreateSpr(storage, sprPath) == null)
                {
                    return null;
                }

                return skill.texture.SprCache.CreateSprFrame(storage, sprPath, frameIndex);
            }
        }
    }

    public class Cache
    {
        public static skill.texture.SprCache.Data.SprFrame GetSprFrame(string sprPath, ushort frameIndex)
        {
            return skill.texture.SprCache.GetSprFrame(resource.Cache.Settings.Skill.textures, sprPath, frameIndex);
        }
    }
}
