
using System;

namespace game.resource.settings.skill
{
    public class SkillSettingBase : SkillSettingData
    {
        private const string DefaultSkillIcon = "\\spr\\Ui\\技能图标\\枪法.spr";

        protected void LoadBase(int skillId)
        {
            if (Cache.Settings.Skill.skillsIdToRowIndexMapping.ContainsKey(skillId) == false)
            {
                return;
            }

            this.InitSettingData();

            int rowIndex = Cache.Settings.Skill.skillsIdToRowIndexMapping[skillId];
            resource.Table table = Cache.Settings.Skill.skillsTable;

            this.m_nId = skillId;
            this.m_szName = GetString(table, "SkillName", rowIndex);
            this.m_property = GetString(table, "Property", rowIndex);
            this.m_nAttrib = GetInt(table, "Attrib", rowIndex);
            this.m_usReqLevel = GetInt(table, "ReqLevel", rowIndex);
            this.m_maxLevel = GetInt(table, "MaxLevel", rowIndex, 20);
            this.m_nEquiptLimited = GetInt(table, "EqtLimit", rowIndex, -2);
            this.m_nHorseLimited = GetInt(table, "HorseLimit", rowIndex);
            this.m_bDoHurt = GetInt(table, "DoHurt", rowIndex);
            this.m_nChildSkillNum = GetInt(table, "ChildSkillNum", rowIndex);
            this.m_eMisslesForm = (skill.Defination.MisslesForm)GetInt(table, "MisslesForm", rowIndex);
            this.m_nCharClass = GetInt(table, "CharClass", rowIndex);
            this.m_eSkillStyle = (skill.Defination.SKillStyle)GetInt(table, "SkillStyle", rowIndex);
            this.m_nCharActionId = (skill.Defination.CLIENTACTION)GetInt(table, "CharAnimId", rowIndex);
            this.m_bIsAura = GetBoolInt(table, "IsAura", rowIndex);
            this.m_bUseAttackRate = GetBoolInt(table, "IsUseAR", rowIndex);
            this.m_bTargetOnly = GetBoolInt(table, "TargetOnly", rowIndex);
            this.m_bTargetEnemy = GetBoolInt(table, "TargetEnemy", rowIndex);
            this.m_bTargetAlly = GetBoolInt(table, "TargetAlly", rowIndex);
            this.m_bTargetObj = GetBoolInt(table, "TargetObj", rowIndex);
            this.m_bTargetOther = GetBoolInt(table, "TargetOther", rowIndex);
            this.m_bTargetNoNpc = GetBoolInt(table, "TargetNoNpc", rowIndex);
            this.m_bBaseSkill = GetBoolInt(table, "BaseSkill", rowIndex);
            this.m_bByMissle = GetBoolInt(table, "ByMissle", rowIndex);
            this.m_nChildSkillId = GetInt(table, "ChildSkillId", rowIndex);
            this.m_bFlyingEvent = GetBoolInt(table, "FlyEvent", rowIndex);
            this.m_bStartEvent = GetBoolInt(table, "StartEvent", rowIndex);
            this.m_bCollideEvent = GetBoolInt(table, "CollideEvent", rowIndex);
            this.m_bVanishedEvent = GetBoolInt(table, "VanishedEvent", rowIndex);
            this.m_nFlySkillId = GetInt(table, "FlySkillId", rowIndex);
            this.m_nStartSkillId = GetInt(table, "StartSkillId", rowIndex);
            this.m_nVanishedSkillId = GetInt(table, "VanishedSkillId", rowIndex);
            this.m_nCollideSkillId = GetInt(table, "CollidSkillId", rowIndex);
            this.m_nSkillCostType = (skill.Defination.NPCATTRIB)GetInt(table, "SkillCostType", rowIndex);
            this.m_nCost = GetInt(table, "CostValue", rowIndex);
            this.m_nMinTimePerCast = GetInt(table, "TimePerCast", rowIndex);
            this.m_nMinTimePerCastOnHorse = GetInt(table, "TimePerCastOnHorse", rowIndex);
            this.m_nValue1 = GetInt(table, "Param1", rowIndex);
            this.m_nValue2 = GetInt(table, "Param2", rowIndex);
            this.m_nChildSkillLevel = GetInt(table, "ChildSkillLevel", rowIndex);
            this.m_nEventSkillLevel = GetInt(table, "EventSkillLevel", rowIndex);
            this.m_bIsMelee = GetBoolInt(table, "IsMelee", rowIndex);
            this.m_nFlyEventTime = GetInt(table, "FlyEventTime", rowIndex);
            this.m_nShowEvent = GetInt(table, "ShowEvent", rowIndex);
            this.m_eMisslesGenerateStyle = (skill.Defination.MisslesGenerateStyle)GetInt(table, "MslsGenerate", rowIndex);
            this.m_nMisslesGenerateData = GetInt(table, "MslsGenerateData", rowIndex);
            this.m_nMaxShadowNum = GetInt(table, "MaxShadowNum", rowIndex);
            this.m_nAttackRadius = GetInt(table, "AttackRadius", rowIndex, 50);
            this.m_nWaitTime = GetInt(table, "WaitTime", rowIndex);
            this.m_bClientSend = GetBoolInt(table, "ClientSend", rowIndex);
            this.m_bTargetSelf = GetBoolInt(table, "TargetSelf", rowIndex);
            this.m_nInteruptTypeWhenMove = GetInt(table, "StopWhenMove", rowIndex);
            this.m_bHeelAtParent = GetBoolInt(table, "HeelAtParent", rowIndex);
            this.m_nIsExpSkill = GetInt(table, "IsExpSkill", rowIndex);
            this.m_nSeries = GetInt(table, "Series", rowIndex, -1);
            this.m_nShowAddition = GetInt(table, "ShowAddition", rowIndex);
            this.m_bIsPhysical = GetBoolInt(table, "IsPhysical", rowIndex);
            this.m_nStateSpecialId = GetInt(table, "StateSpecialId", rowIndex);

            m_eRelation = 0;
            if (this.m_bTargetEnemy != 0)
                m_eRelation |= (int)skill.Defination.NPC_RELATION.relation_enemy;

            if (this.m_bTargetAlly != 0)
                m_eRelation |= (int)skill.Defination.NPC_RELATION.relation_ally;

            if (this.m_bTargetSelf != 0)
                m_eRelation |= (int)skill.Defination.NPC_RELATION.relation_self;

            if (this.m_bTargetOther != 0)
            {
                m_eRelation |= (int)skill.Defination.NPC_RELATION.relation_dialog;
                m_eRelation |= (int)skill.Defination.NPC_RELATION.relation_none;
            }

            this.m_szSkillDesc = GetString(table, "SkillDesc", rowIndex, "<null data>");
            this.m_bNeedShadow = GetBoolInt(table, "NeedShadow", rowIndex);
            this.m_szSkillIcon = GetString(table, "SkillIcon", rowIndex, DefaultSkillIcon);
            this.m_eLRSkillInfo = (skill.Defination.SkillLRInfo)GetInt(table, "LRSkill", rowIndex);
            this.m_szPreCastEffectFile = GetString(table, "PreCastSpr", rowIndex);
            this.m_szManPreCastSoundFile = GetString(table, "ManCastSnd", rowIndex);
            this.m_szFMPreCastSoundFile = GetString(table, "FMCastSnd", rowIndex);

            if (Cache.Settings.Skill.skillsTable.GetEncoding().byteOrderMarks == 0)
            {
                this.m_szName = formater.TCVN3.UTF8(this.m_szName);
                this.m_property = formater.TCVN3.UTF8(this.m_property);
                this.m_szSkillDesc = formater.TCVN3.UTF8(this.m_szSkillDesc);
            }
        }

        ////////////////////////////////////////////////////////////////////////////////

        private static int GetInt(resource.Table table, string header, int rowIndex, int defaultValue = 0)
        {
            int columnIndex = table.GetHeaderIndex(header);
            return columnIndex >= 0 ? table.Get<int>(columnIndex, rowIndex, defaultValue) : defaultValue;
        }

        private static int GetBoolInt(resource.Table table, string header, int rowIndex)
        {
            return GetInt(table, header, rowIndex) > 0 ? 1 : 0;
        }

        private static string GetString(resource.Table table, string header, int rowIndex, string defaultValue = "")
        {
            int columnIndex = table.GetHeaderIndex(header);
            if (columnIndex < 0)
            {
                return defaultValue;
            }

            string value = table.Get<string>(columnIndex, rowIndex);
            return string.IsNullOrEmpty(value) ? defaultValue : value;
        }

        ////////////////////////////////////////////////////////////////////////////////

        public int GetMissleGenerateTime(int nNo)
        {
            switch (this.m_eMisslesGenerateStyle)
            {
                case skill.Defination.MisslesGenerateStyle.SKILL_MGS_NULL:
                    {
                        return this.m_nWaitTime;
                    }

                case skill.Defination.MisslesGenerateStyle.SKILL_MGS_SAMETIME:
                    {
                        return this.m_nWaitTime + this.m_nMisslesGenerateData;
                    }

                case skill.Defination.MisslesGenerateStyle.SKILL_MGS_ORDER:
                    {
                        return this.m_nWaitTime + nNo * this.m_nMisslesGenerateData;
                    }

                case skill.Defination.MisslesGenerateStyle.SKILL_MGS_RANDONORDER:
                    {
                        if (skill.Static.g_Random(2) == 1)
                            return this.m_nWaitTime + nNo * this.m_nMisslesGenerateData + skill.Static.g_Random(this.m_nMisslesGenerateData);
                        else
                            return this.m_nWaitTime + nNo * this.m_nMisslesGenerateData - skill.Static.g_Random(this.m_nMisslesGenerateData / 2);
                    }

                case skill.Defination.MisslesGenerateStyle.SKILL_MGS_RANDONSAME:
                    {
                        return this.m_nWaitTime + skill.Static.g_Random(this.m_nMisslesGenerateData);
                    }

                case skill.Defination.MisslesGenerateStyle.SKILL_MGS_CENTEREXTENDLINE:
                    {
                        if (this.m_nChildSkillNum <= 1) return this.m_nWaitTime;
                        int nCenter = this.m_nChildSkillNum / 2;
                        return this.m_nWaitTime + Math.Abs(nNo - nCenter) * this.m_nMisslesGenerateData;
                    }
            }
            return this.m_nWaitTime;
        }
    }
}
