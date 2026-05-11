
using System.Collections.Generic;
using System.Diagnostics;

namespace game.resource.settings.skill
{
    public class SkillSettingGetter : skill.SkillSettingLevel
    {
#if SKILL_TIMING
        private static readonly HashSet<int> DescriptionTimingLogged = new();
#endif

        private string GetMagicProperties()
        {
            string result = string.Empty;

            if((int)this.m_nSkillCostType >= 0 && (int)this.m_nSkillCostType <= 2)
            {
                string[] costString =
                {
                    "Nội lực",
                    "Thể lực",
                    "Sinh lực"
                };

                result += "Tiêu hao " + costString[(int)this.m_nSkillCostType] + ": " + this.m_nCost;
                result += "\n";
            }
            
            result += "Phạm vi hiệu quả: " + this.m_nAttackRadius;
            result += "\n";

            if (this.m_nMinTimePerCast > 0)
            {
                result += "Thời gian phục hồi chiêu thức: " + (this.m_nMinTimePerCast / 18f) + " giây";
                result += "\n";
            }

            if (this.m_nMinTimePerCastOnHorse > 0)
            {
                result += "Thời gian phục hồi chiêu thức trên ngựa: " + (this.m_nMinTimePerCastOnHorse / 18f) + " giây";
                result += "\n";
            }

            foreach (settings.skill.SkillSettingLevel.KMagicAttrib magicEntry in this.m_DamageAttribs)
            {
                AppendMagicDesc(ref result, magicEntry);
            }

            foreach (settings.skill.SkillSettingLevel.KMagicAttrib magicEntry in this.m_ImmediateAttribs)
            {
                AppendMagicDesc(ref result, magicEntry);
            }

            foreach (settings.skill.SkillSettingLevel.KMagicAttrib magicEntry in this.m_StateAttribs)
            {
                AppendMagicDesc(ref result, magicEntry);
            }

            if (this.m_nEventSkillLevel > 0)
            {
                result += "\n";

                int[] eventList =
                {
                    this.m_nStartSkillId,
                    this.m_nFlySkillId,
                    this.m_nCollideSkillId,
                    this.m_nVanishedSkillId
                };

                for (int index = 0, eventNum = 1; index < eventList.Length; index++)
                {
                    if (eventList[index] <= 0)
                    {
                        continue;
                    }

                    settings.skill.SkillSetting skillEvent = settings.skill.SkillSetting.Get(eventList[index], this.m_nEventSkillLevel);

                    result += "<color=#a8a8ff>Tầng thứ " + (eventNum++) + ": </color>";
                    result += "<color=yellow>" + skillEvent.m_szName + "</color> ";
                    result += "<color=#a8a8ff>cấp " + this.m_nEventSkillLevel + "</color>";
                    result += "\n";

                    foreach (settings.skill.SkillSetting.KMagicAttrib magicEntry in skillEvent.m_DamageAttribs)
                    {
                        AppendMagicDesc(ref result, magicEntry);
                    }
                }
            }

            return result;
        }

        private static void AppendMagicDesc(ref string result, settings.skill.SkillSettingLevel.KMagicAttrib magicEntry)
        {
            string desc = settings.MagicDesc.Get(magicEntry);
            if (string.IsNullOrWhiteSpace(desc))
            {
                return;
            }

            result += desc;
            result += "\n";
        }

        public string GetDescription()
        {
            if (this.description != null)
            {
                return this.description;
            }

#if SKILL_TIMING
            int timingKey = (this.m_nId * 100) + this.skillLevel;
            bool logTiming = DescriptionTimingLogged.Add(timingKey);
            Stopwatch totalWatch = logTiming ? Stopwatch.StartNew() : null;
            Stopwatch nextLevelWatch = null;
#endif

            string result = string.Empty;

            result += "Đẳng cấp hiện thời: " + this.skillLevel;
            result += "\n";

            if (this.m_nEquiptLimited >= 0 && this.m_nEquiptLimited <= 9)
            {
                string[] equipLimited =
                {
                    "Trường Kiếm",
                    "Đơn Đao",
                    "Côn Bổng",
                    "Thương Kích",
                    "Song Chùy",
                    "Song Đao",
                    "kxđ","kxđ","kxđ",
                    "Tay Không"
                };

                result += "Hạn chế vũ khí: <color=teal>" + equipLimited[this.m_nEquiptLimited] + "</color>";
                result += "\n";
            }

            if (this.m_nHorseLimited == 1)
            {
                result += "Trong lúc cưỡi ngựa không thể thi triển";
                result += "\n";
            }

            result += "\n";
            result += this.GetMagicProperties();
            result += "\n";
            result += "<color=red>Đẳng cấp kế tiếp";
            result += "\n";
            settings.skill.SkillSetting nextSkill;
#if SKILL_TIMING
            if (logTiming)
            {
                nextLevelWatch = Stopwatch.StartNew();
            }
#endif
            nextSkill = settings.skill.SkillSetting.Get(this.m_nId, this.skillLevel + 1);
#if SKILL_TIMING
            if (logTiming)
            {
                nextLevelWatch.Stop();
            }
#endif
            if (nextSkill != null)
            {
                result += nextSkill.GetMagicProperties();
            }
            result += "</color>";

            this.description = result;
#if SKILL_TIMING
            if (logTiming)
            {
                totalWatch.Stop();
                UnityEngine.Debug.Log(
                    "Skill timing: GetDescription skillId=" + this.m_nId +
                    " level=" + this.skillLevel +
                    " nextLevelGet=" + (nextLevelWatch != null ? nextLevelWatch.ElapsedMilliseconds : 0) + "ms" +
                    " total=" + totalWatch.ElapsedMilliseconds + "ms");
            }
#endif
            return this.description;
        }
    }
}
