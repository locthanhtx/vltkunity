
using System.Collections.Generic;
using System.Diagnostics;

namespace game.resource.settings.skill
{
    public class SkillSetting : skill.SkillSettingGetter
    {
        #if SKILL_TIMING
        private static readonly HashSet<int> LoadTimingLogged = new();
        #endif

        public static skill.SkillSetting GetBase(int skillId)
        {
            int cacheKey = -skillId;

            if (skillId <= 0)
            {
                return null;
            }

            if (Cache.Settings.Skill.skillsIdToDataMapping.ContainsKey(cacheKey) == false)
            {
                skill.SkillSetting newSkillSetting = new skill.SkillSetting();
                newSkillSetting.LoadBase(skillId);
                Cache.Settings.Skill.skillsIdToDataMapping[cacheKey] = newSkillSetting;
            }

            return Cache.Settings.Skill.skillsIdToDataMapping[cacheKey];
        }

        public static skill.SkillSetting Get(int skillId, int skillLevel)
        {
            int cacheKey = (skillId * 100) + skillLevel;

            if (skillId <= 0)
            {
                return null;
            }

            if (Cache.Settings.Skill.skillsIdToDataMapping.ContainsKey(cacheKey) == false)
            {
#if SKILL_TIMING
                bool logTiming = LoadTimingLogged.Add(cacheKey);
                Stopwatch totalWatch = logTiming ? Stopwatch.StartNew() : null;
#endif
                skill.SkillSetting newSkillSetting = new skill.SkillSetting();
                newSkillSetting.LoadBase(skillId);
                try
                {
                    newSkillSetting.LoadLevel(skillId, skillLevel);
                }
                catch (System.Exception exception)
                {
                    UnityEngine.Debug.LogWarning(
                        "Skill level data failed. skillId=" + skillId +
                        " level=" + skillLevel +
                        " error=" + exception);
                }

                Cache.Settings.Skill.skillsIdToDataMapping[cacheKey] = newSkillSetting;
#if SKILL_TIMING
                if (logTiming)
                {
                    totalWatch.Stop();
                    UnityEngine.Debug.Log(
                        "Skill timing: Get skillId=" + skillId +
                        " level=" + skillLevel +
                        " total=" + totalWatch.ElapsedMilliseconds + "ms");
                }
#endif
            }

            return Cache.Settings.Skill.skillsIdToDataMapping[cacheKey];
        }

        public static skill.SkillSetting GetRuntimeBase(int skillId, int skillLevel)
        {
            int safeSkillLevel = System.Math.Max(1, skillLevel);
            int cacheKey = int.MinValue + (skillId * 100) + safeSkillLevel;

            if (skillId <= 0)
            {
                return null;
            }

            if (Cache.Settings.Skill.skillsIdToDataMapping.ContainsKey(cacheKey) == false)
            {
                skill.SkillSetting newSkillSetting = new skill.SkillSetting();
                newSkillSetting.LoadBase(skillId);
                newSkillSetting.skillLevel = safeSkillLevel;
                Cache.Settings.Skill.skillsIdToDataMapping[cacheKey] = newSkillSetting;
            }

            return Cache.Settings.Skill.skillsIdToDataMapping[cacheKey];
        }
    }
}
