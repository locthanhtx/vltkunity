
using System.Collections.Generic;

namespace game.resource.settings.skill
{
    public class CastSkill : skill.CastPassivity
    {
        public List<settings.skill.Missile> Cast(skill.Params.Cast castParams)
        {
            List<settings.skill.Missile> result = new List<settings.skill.Missile>();

            if (this.skillSetting == null || castParams == null)
            {
                return result;
            }

            if (castParams.launcher.HaveData() == false)
            {
                return result;
            }

            switch (castParams.launcher.type)
            {
                case skill.Defination.eSkillLauncherType.SKILL_SLT_Npc:
                    break;

                case skill.Defination.eSkillLauncherType.SKILL_SLT_Obj:
                    return result;

                case skill.Defination.eSkillLauncherType.SKILL_SLT_Missle:
                    break;

                default:
                    return result;
            }

            //if (castParams.nParam1 < 0 && castParams.nParam2 < 0)
            //    return result;

            //if (castParams.nWaitTime < 0)
            //{
            //    castParams.nWaitTime = 0;
            //}

            //------------------------------------------------------------------------------

            //if (this.skillSetting.m_bTargetSelf != 0)
            //{
            //    castParams.nParam1 = -1;
            //}

            //------------------------------------------------------------------------------

            //UnityEngine.Debug.Log("this.skillSetting.m_eSkillStyle: " + this.skillSetting.m_eSkillStyle);

            switch (this.skillSetting.m_eSkillStyle)
            {

                case skill.Defination.SKillStyle.SKILL_SS_Missles:              //发子弹
                    {
                        //UnityEngine.Debug.Log("cast Missile!!!");
                        result = this.CastMissles(castParams);
                    }
                    break;
                case skill.Defination.SKillStyle.SKILL_SS_Melee:
                    break;

                case skill.Defination.SKillStyle.SKILL_SS_InitiativeNpcState:   //改变角色的主动状态
                    {
                        result = this.CastInitiatives(castParams);
                    }
                    break;

                case skill.Defination.SKillStyle.SKILL_SS_PassivityNpcState:    //改变角色的被动状态	
                    {
                        result = this.CastPassivitys(castParams);
                    }
                    break;

                case skill.Defination.SKillStyle.SKILL_SS_CreateNpc:            //产生新的Npc、怪物 
                    break;

                case skill.Defination.SKillStyle.SKILL_SS_BuildPoison:          //炼毒术
                    break;

                case skill.Defination.SKillStyle.SKILL_SS_AddPoison:
                    break;

                case skill.Defination.SKillStyle.SKILL_SS_GetObjDirectly:
                    break;

                case skill.Defination.SKillStyle.SKILL_SS_StrideObstacle:
                    break;

                case skill.Defination.SKillStyle.SKILL_SS_BodyToObject:
                    break;

                case skill.Defination.SKillStyle.SKILL_SS_Mining:
                    break;

                case skill.Defination.SKillStyle.SKILL_SS_RepairWeapon:
                    break;

                case skill.Defination.SKillStyle.SKILL_SS_Capture:
                    break;
            }

            if (this.skillSetting.m_bStartEvent != 0 && this.skillSetting.m_nStartSkillId > 0 && this.skillSetting.m_nEventSkillLevel > 0)
            {
                settings.Skill pOrdinSkill = new settings.Skill(this.skillSetting.m_nStartSkillId, this.skillSetting.m_nEventSkillLevel, this.map);
                result.AddRange(pOrdinSkill.Cast(castParams));
            }

            if(castParams.launcher.type == Defination.eSkillLauncherType.SKILL_SLT_Npc)
            {
                if(this.skillSetting.m_szPreCastEffectFile != string.Empty)
                {
                    castParams.launcher.npc.SetStateSpecialSpr(this.skillSetting.m_szPreCastEffectFile);
                }

                if(this.skillSetting.m_nCharActionId == Defination.CLIENTACTION.cdo_attack
                    || this.skillSetting.m_nCharActionId == Defination.CLIENTACTION.cdo_attack1
                    || this.skillSetting.m_nCharActionId == Defination.CLIENTACTION.cdo_magic)
                {
                    if(this.skillSetting.m_bIsPhysical != 0)
                    {
                        if(skill.Static.g_Random(3) != 0)
                        {
                            castParams.launcher.npc.SetAction(resource.settings.NpcRes.Action.attack1);
                        }
                        else
                        {
                            castParams.launcher.npc.SetAction(resource.settings.NpcRes.Action.attack2);

                        }
                    }
                    else
                    {
                        castParams.launcher.npc.SetAction(resource.settings.NpcRes.Action.magic);
                    }
                }

                if (castParams.target != null && castParams.target.HaveData())
                {
                    resource.map.Position sourcePos = castParams.launcher.GetMapPosition();
                    resource.map.Position targetPos = castParams.target.GetMapPosition();
                    castParams.launcher.npc.SetDirection(skill.Static.Dir64To8(skill.Static.g_GetDirIndex(sourcePos.left, sourcePos.top, targetPos.left, targetPos.top)) + 1);
                }

                castParams.launcher.npc.Update();
            }

            return result;
        }

        public List<settings.skill.Missile> Cast(settings.npcres.Controller npcController, int nParam1, int nParam2, int nWaitTime, skill.Defination.eSkillLauncherType eLauncherType)
        {
            skill.Params.Cast castParams = new skill.Params.Cast();
            castParams.nParam1 = nParam1;
            castParams.nParam2 = nParam2;
            castParams.nWaitTime = nWaitTime;
            castParams.launcher.SetData(npcController);

            return this.Cast(castParams);
        }

        public List<settings.skill.Missile> Cast(settings.skill.Missile missile, int nParam1, int nParam2, int nWaitTime, skill.Defination.eSkillLauncherType eLauncherType)
        {
            skill.Params.Cast castParams = new skill.Params.Cast();
            castParams.nParam1 = nParam1;
            castParams.nParam2 = nParam2;
            castParams.nWaitTime = nWaitTime;
            castParams.launcher.SetData(missile);

            return this.Cast(castParams);
        }

        public List<settings.skill.Missile> Cast(skill.Params.Owner launcher, int nParam1, int nParam2, int nWaitTime, skill.Defination.eSkillLauncherType eLauncherType)
        {
            skill.Params.Cast castParams = new skill.Params.Cast();
            castParams.nParam1 = nParam1;
            castParams.nParam2 = nParam2;
            castParams.nWaitTime = nWaitTime;
            castParams.launcher = launcher;

            return this.Cast(castParams);
        }
    }

}
