
using System;
using System.Collections.Generic;

namespace game.resource.settings.skill
{
    public class CastMissile : skill.cast.Spread
    {
        private int Param2PCoordinate(settings.skill.Params.Cast castParams, ref int npPX, ref int npPY, skill.Defination.eSkillLauncherType eLauncherType)
        {
            int nTargetId = -1;

            if (eLauncherType == skill.Defination.eSkillLauncherType.SKILL_SLT_Obj) return 0;

            // UnityEngine.Debug.Log("params 1: " + castParams.nParam1);

            switch (castParams.nParam1)
            {
                case -1://nParam2 参数指向某个Npc，或Obj的Index
                    nTargetId = castParams.nParam2;

                    if (eLauncherType == Defination.eSkillLauncherType.SKILL_SLT_Npc)
                    {
                        resource.map.Position position = castParams.target != null && castParams.target.HaveData()
                            ? castParams.target.GetMapPosition()
                            : castParams.launcher.GetMapPosition();
                        npPX = position.left;
                        npPY = position.top;
                    }
                    else if (eLauncherType == skill.Defination.eSkillLauncherType.SKILL_SLT_Missle)
                    {

                    }
                    else if (eLauncherType == skill.Defination.eSkillLauncherType.SKILL_SLT_Obj)
                    {

                    }

                    break;

                case -2://nParam 参数指向某个方向

                    break;
                default://默认时, nParam1 与nParam2 为实际点坐标
                    npPX = castParams.nParam1;
                    npPY = castParams.nParam2;
                    break;
            }

            return nTargetId;
        }

        private List<settings.skill.Missile> MissileForm_Wall(settings.skill.Params.Cast castParams)
        {
            List<settings.skill.Missile> result = new List<settings.skill.Missile>();

            int nSrcPX = 0;//点坐标
            int nSrcPY = 0;
            int nDesPX = 0;
            int nDesPY = 0;
            int nDir = 0;
            int nDirIndex = 0;
            int nTargetId = -1;
            int nRefPX = 0;
            int nRefPY = 0;

            settings.skill.Params.TOrdinSkillParam SkillParam = new settings.skill.Params.TOrdinSkillParam();
            SkillParam.nWaitTime = castParams.nWaitTime;

            if (castParams.nParam1 == (int)skill.Defination.eSkillParamType.SKILL_SPT_Direction)
            {
                return result;
            }

            switch (castParams.launcher.type)
            {
                case skill.Defination.eSkillLauncherType.SKILL_SLT_Npc:
                    {
                        if (castParams.nParam1 != 0 || castParams.nParam2 != 0)
                        {
                            nTargetId = Param2PCoordinate(castParams, ref nDesPX, ref nDesPY, skill.Defination.eSkillLauncherType.SKILL_SLT_Npc);
                        }
                        else
                        {
                            resource.map.Position targetMapPosition = castParams.target.GetMapPosition();
                            nDesPX = targetMapPosition.left;
                            nDesPY = targetMapPosition.top;
                        }

                        resource.map.Position launcherNpcMapPosition = castParams.launcher.GetMapPosition();
                        nSrcPX = launcherNpcMapPosition.left;
                        nSrcPY = launcherNpcMapPosition.top;

                        nDirIndex = skill.Static.g_GetDirIndex(nSrcPX, nSrcPY, nDesPX, nDesPY);
                        nDir = skill.Static.g_DirIndex2Dir(nDirIndex, skill.Defination.MaxMissleDir);
                        nDir = nDir + skill.Defination.MaxMissleDir / 4;
                        if (nDir >= skill.Defination.MaxMissleDir) nDir -= skill.Defination.MaxMissleDir;
                        SkillParam.launcher = castParams.launcher;
                        SkillParam.target = castParams.target;

                        if (this.skillSetting.m_nValue2 == 0)
                            result = CastWall(SkillParam, nDir, nDesPX, nDesPY);
                        else 
                            result = CastWall(SkillParam, nDir, nSrcPX, nSrcPY);
                    }
                    break;
                case skill.Defination.eSkillLauncherType.SKILL_SLT_Obj:
                    {
                    }
                    break;
                case skill.Defination.eSkillLauncherType.SKILL_SLT_Missle:
                    {
                        resource.map.Position launcherMissileMapPosition = castParams.launcher.GetMapPosition();
                        nRefPX = launcherMissileMapPosition.left;
                        nRefPY = launcherMissileMapPosition.top;

                        settings.skill.Missile missile = castParams.launcher.missile;

                        if (missile == null)
                        {
                            nDir = 0 + skill.Defination.MaxMissleDir / 4;
                        }
                        else
                        {
                            nDir = missile.m_nDir + skill.Defination.MaxMissleDir / 4;
                        }

                        if (nDir >= skill.Defination.MaxMissleDir) nDir -= skill.Defination.MaxMissleDir;
                        SkillParam.launcher = castParams.launcher.missile.GetLauncher();
                        SkillParam.parent = castParams.launcher;
                        SkillParam.target = castParams.target;
                        result = CastWall(SkillParam, nDir, nRefPX, nRefPY);
                    }
                    break;
            }

            return result;
        }

        private List<settings.skill.Missile> MissileForm_Line(settings.skill.Params.Cast castParams)
        {
            List<settings.skill.Missile> result = new List<settings.skill.Missile>();

            int nSrcPX = 0;//点坐标
            int nSrcPY = 0;
            int nDesPX = 0;
            int nDesPY = 0;
            int nDistance = 0;
            int nDir = 0;
            int nDirIndex = 0;
            int nTargetId = -1;
            int nRefPX = 0;
            int nRefPY = 0;

            settings.skill.Params.TOrdinSkillParam SkillParam = new settings.skill.Params.TOrdinSkillParam();
            SkillParam.nWaitTime = castParams.nWaitTime;

            if (castParams.nParam1 == (int)skill.Defination.eSkillParamType.SKILL_SPT_Direction)
            {
                // UnityEngine.Debug.Log("if");

                switch (castParams.launcher.type)
                {
                    case Defination.eSkillLauncherType.SKILL_SLT_Npc:
                        {
                            if (castParams.nParam2 > skill.Defination.MaxMissleDir || castParams.nParam2 < 0)
                            {
                                // UnityEngine.Debug.Log("return 1");
                                return result;
                            }

                            resource.map.Position launcherNpcPosition = castParams.launcher.GetMapPosition();
                            nSrcPX = launcherNpcPosition.left;
                            nSrcPY = launcherNpcPosition.top;

                            nDir = castParams.nParam2;
                            SkillParam.launcher = castParams.launcher;
                            SkillParam.target = castParams.target;
                            result = CastLine(SkillParam, nDir, nSrcPX, nSrcPY);

                        }
                        break;
                    case Defination.eSkillLauncherType.SKILL_SLT_Obj:
                        {

                        }
                        break;
                    case Defination.eSkillLauncherType.SKILL_SLT_Missle:
                        {
                            if (castParams.nParam2 > skill.Defination.MaxMissleDir || castParams.nParam2 < 0)
                            {
                                // UnityEngine.Debug.Log("return 2");
                                return result;
                            }

                            resource.map.Position launcherPosition = castParams.launcher.GetMapPosition();
                            nRefPX = launcherPosition.left;
                            nRefPY = launcherPosition.top;

                            SkillParam.launcher = castParams.launcher.missile.GetLauncher();
                            SkillParam.parent = castParams.launcher;
                            SkillParam.target = castParams.target;
                            result = CastWall(SkillParam, nDir, nRefPX, nRefPY);
                        }
                        break;

                    default:
                        // UnityEngine.Debug.Log("return 8");
                        break;
                }

            }
            else
            {
                // UnityEngine.Debug.Log("else --> " + castParams.launcher.type);

                switch (castParams.launcher.type)
                {
                    case Defination.eSkillLauncherType.SKILL_SLT_Npc:
                        {
                            if (castParams.nParam1 != 0 || castParams.nParam2 != 0)
                            {
                                nTargetId = Param2PCoordinate(castParams, ref nDesPX, ref nDesPY, Defination.eSkillLauncherType.SKILL_SLT_Npc);
                            }
                            else
                            {
                                resource.map.Position targetMapPosition = castParams.target.GetMapPosition();
                                nDesPX = targetMapPosition.left;
                                nDesPY = targetMapPosition.top;
                            }

                            resource.map.Position launcherMapPostion = castParams.launcher.GetMapPosition();
                            nSrcPX = launcherMapPostion.left;
                            nSrcPY = launcherMapPostion.top;

                            nDirIndex = skill.Static.g_GetDirIndex(nSrcPX, nSrcPY, nDesPX, nDesPY);
                            nDir = skill.Static.g_DirIndex2Dir(nDirIndex, skill.Defination.MaxMissleDir);
                            SkillParam.launcher = castParams.launcher;
                            SkillParam.target = castParams.target;

                            skill.Defination.MissleMoveKind childMissileMoveKind = skill.MissileSetting.Get(this.skillSetting.m_nChildSkillId).m_eMoveKind;

                            if (this.skillSetting.m_nChildSkillNum == 1 
                                && (childMissileMoveKind == Defination.MissleMoveKind.MISSLE_MMK_Line || childMissileMoveKind == Defination.MissleMoveKind.MISSLE_MMK_Parabola))
                            {
                                // UnityEngine.Debug.Log("if 2");

                                if (nSrcPX == nDesPX && nSrcPY == nDesPY)
                                {
                                    // UnityEngine.Debug.Log("return 3");
                                    return result;
                                }
                                nDistance = skill.Static.g_GetDistance(nSrcPX, nSrcPY, nDesPX, nDesPY);

                                if (nDistance == 0)
                                {
                                    // UnityEngine.Debug.Log("return 4");
                                    return result;
                                }
                                int nYLength = nDesPY - nSrcPY;
                                int nXLength = nDesPX - nSrcPX;
                                int nSin = (nYLength << 10) / nDistance;    // 放大1024倍
                                int nCos = (nXLength << 10) / nDistance;

                                if (Math.Abs(nSin) > 1024)
                                {
                                    // UnityEngine.Debug.Log("return 5");
                                    return result;
                                }

                                if (Math.Abs(nCos) > 1024)
                                {
                                    // UnityEngine.Debug.Log("return 6");
                                    return result;
                                }


                                result = CastExtractiveLineMissle(SkillParam, nDir, nSrcPX, nSrcPY, nCos, nSin, nDesPX, nDesPY);
                            }
                            else
                            {
                                // UnityEngine.Debug.Log("else 2");
                                result = CastLine(SkillParam, nDir, nSrcPX, nSrcPY);
                            }
                        }
                        break;
                    case Defination.eSkillLauncherType.SKILL_SLT_Obj:
                        {
                        }
                        break;
                    case Defination.eSkillLauncherType.SKILL_SLT_Missle:
                        {
                            resource.map.Position launcherMapPosition = castParams.launcher.GetMapPosition();
                            nRefPX = launcherMapPosition.left;
                            nRefPY = launcherMapPosition.top;

                            settings.skill.Missile pMissle = castParams.launcher.missile;

                            SkillParam.launcher = castParams.launcher;
                            SkillParam.parent = castParams.launcher;
                            SkillParam.target = castParams.target;
                            result = CastLine(SkillParam, pMissle.m_nDir, nRefPX, nRefPY);
                        }
                        break;

                    default:
                        // UnityEngine.Debug.Log("return 7");
                        break;
                }
            }

            // UnityEngine.Debug.Log("return count: " + result.Count);
            return result;
        }

        private List<settings.skill.Missile> MissileForm_Spread(settings.skill.Params.Cast castParams)
        {
            List<settings.skill.Missile> result = new List<settings.skill.Missile>();

            int nSrcPX = 0;//点坐标
            int nSrcPY = 0;
            int nDesPX = 0;
            int nDesPY = 0;
            int nDistance = 0;
            int nDir = 0;
            int nDirIndex = 0;
            int nTargetId = -1;
            int nRefPX = 0;
            int nRefPY = 0;

            settings.skill.Params.TOrdinSkillParam SkillParam = new settings.skill.Params.TOrdinSkillParam();
            SkillParam.nWaitTime = castParams.nWaitTime;

            if (castParams.nParam1 == (int)skill.Defination.eSkillParamType.SKILL_SPT_Direction)
            {
                switch (castParams.launcher.type)
                {
                    case Defination.eSkillLauncherType.SKILL_SLT_Npc:
                        {
                            if (castParams.nParam2 > skill.Defination.MaxMissleDir || castParams.nParam2 < 0)
                            {
                                return result;
                            }

                            resource.map.Position npcLauncherPosition = castParams.launcher.GetMapPosition();
                            nSrcPX = npcLauncherPosition.left;
                            nSrcPY = npcLauncherPosition.top;

                            nDir = castParams.nParam2;
                            SkillParam.launcher = castParams.launcher;
                            result = CastSpread(SkillParam, nDir, nSrcPX, nSrcPY);
                        }
                        break;
                    case Defination.eSkillLauncherType.SKILL_SLT_Obj:
                        {

                        }
                        break;
                    case Defination.eSkillLauncherType.SKILL_SLT_Missle:
                        {
                            if (castParams.nParam2 > skill.Defination.MaxMissleDir || castParams.nParam2 < 0)
                            {
                                return result;
                            }
                            nDir = castParams.nParam2;

                            resource.map.Position launcherPosition = castParams.launcher.GetMapPosition();
                            nRefPX = launcherPosition.left;
                            nRefPY = launcherPosition.top;

                            SkillParam.launcher = castParams.launcher.missile.GetLauncher();
                            SkillParam.parent = castParams.launcher;
                            SkillParam.target = castParams.target;
                            result = CastSpread(SkillParam, nDir, nRefPX, nRefPY);
                        }
                        break;
                }
            }
            else
            {
                switch (castParams.launcher.type)
                {
                    case Defination.eSkillLauncherType.SKILL_SLT_Npc:
                        {
                            if (castParams.nParam1 != 0 || castParams.nParam2 != 0)
                            {
                                nTargetId = Param2PCoordinate(castParams, ref nDesPX, ref nDesPY, Defination.eSkillLauncherType.SKILL_SLT_Npc);
                            }
                            else
                            {
                                resource.map.Position targetMapPosition = castParams.target.GetMapPosition();
                                nDesPX = targetMapPosition.left;
                                nDesPY = targetMapPosition.top;
                            }

                            resource.map.Position launcherPosition = castParams.launcher.GetMapPosition();
                            nSrcPX = launcherPosition.left;
                            nSrcPY = launcherPosition.top;

                            nDirIndex = skill.Static.g_GetDirIndex(nSrcPX, nSrcPY, nDesPX, nDesPY);
                            nDir = skill.Static.g_DirIndex2Dir(nDirIndex, skill.Defination.MaxMissleDir);
                            SkillParam.launcher = castParams.launcher;
                            SkillParam.target = castParams.target;

                            skill.Defination.MissleMoveKind childMissileMoveKind = skill.MissileSetting.Get(this.skillSetting.m_nChildSkillId).m_eMoveKind;

                            if (this.skillSetting.m_nChildSkillNum == 1 
                                && (childMissileMoveKind == Defination.MissleMoveKind.MISSLE_MMK_Line))
                            {
                                if (nSrcPX == nDesPX && nSrcPY == nDesPY)
                                {
                                    return result;
                                }
                                nDistance = skill.Static.g_GetDistance(nSrcPX, nSrcPY, nDesPX, nDesPY);

                                if (nDistance == 0)
                                {
                                    return result;
                                }
                                int nYLength = nDesPY - nSrcPY;
                                int nXLength = nDesPX - nSrcPX;
                                int nSin = (nYLength << 10) / nDistance;    // 放大1024倍
                                int nCos = (nXLength << 10) / nDistance;

                                if (Math.Abs(nSin) > 1024)
                                    return result;

                                if (Math.Abs(nCos) > 1024)
                                    return result;

                                result = CastExtractiveLineMissle(SkillParam, nDir, nSrcPX, nSrcPY, nCos, nSin, nDesPX, nDesPY);
                            }
                            else
                                result = CastSpread(SkillParam, nDir, nSrcPX, nSrcPY);
                        }
                        break;
                    case Defination.eSkillLauncherType.SKILL_SLT_Obj:
                        {

                        }
                        break;
                    case Defination.eSkillLauncherType.SKILL_SLT_Missle:
                        {
                            resource.map.Position launcherPosition = castParams.launcher.GetMapPosition();
                            nRefPX = launcherPosition.left;
                            nRefPY = launcherPosition.top;

                            skill.Missile pMissle = castParams.launcher.missile;

                            SkillParam.launcher = castParams.launcher;
                            SkillParam.parent = castParams.launcher;
                            SkillParam.target = castParams.target;
                            result =  CastSpread(SkillParam, pMissle.m_nDir, nRefPX, nRefPY);
                        }
                        break;
                }
            }

            return result;
        }

        private List<settings.skill.Missile> MissileForm_Circle(settings.skill.Params.Cast castParams)
        {
            List<settings.skill.Missile> result = new List<settings.skill.Missile>();

            int nSrcPX = 0;//点坐标
            int nSrcPY = 0;
            int nDesPX = 0;
            int nDesPY = 0;
            int nDir = 0;
            int nDirIndex = 0;
            int nTargetId = -1;
            int nRefPX = 0;
            int nRefPY = 0;

            settings.skill.Params.TOrdinSkillParam SkillParam = new settings.skill.Params.TOrdinSkillParam();
            SkillParam.nWaitTime = castParams.nWaitTime;

            if (castParams.nParam1 == (int)skill.Defination.eSkillParamType.SKILL_SPT_Direction)
            {
                return result;
            }

            switch (castParams.launcher.type)
            {
                case Defination.eSkillLauncherType.SKILL_SLT_Npc:
                    {
                        if (castParams.nParam1 != 0 || castParams.nParam2 != 0)
                        {
                            nTargetId = Param2PCoordinate(castParams, ref nDesPX, ref nDesPY, castParams.launcher.type);
                        }
                        else
                        {
                            resource.map.Position targetMapPosition = castParams.target.GetMapPosition();
                            nDesPX = targetMapPosition.left;
                            nDesPY = targetMapPosition.top;
                        }

                        resource.map.Position launcherPosition = castParams.launcher.GetMapPosition();
                        nSrcPX = launcherPosition.left;
                        nSrcPY = launcherPosition.top;

                        nDirIndex = skill.Static.g_GetDirIndex(nSrcPX, nSrcPY, nDesPX, nDesPY);
                        nDir = skill.Static.g_DirIndex2Dir(nDirIndex, skill.Defination.MaxMissleDir);
                        SkillParam.launcher = castParams.launcher;
                        SkillParam.target = castParams.target;

                        if (this.skillSetting.m_nValue1 == 0)
                            result = CastCircle(SkillParam, nDir, nSrcPX, nSrcPY);
                        else
                            result = CastCircle(SkillParam, nDir, nDesPX, nDesPY);
                    }
                    break;
                case Defination.eSkillLauncherType.SKILL_SLT_Obj:
                    {

                    }
                    break;
                case Defination.eSkillLauncherType.SKILL_SLT_Missle:
                    {
                        resource.map.Position launcherPosition = castParams.launcher.GetMapPosition();
                        nRefPX = launcherPosition.left;
                        nRefPY = launcherPosition.top;

                        skill.Missile pMissle = castParams.launcher.missile;

                        SkillParam.launcher = castParams.launcher.missile.GetLauncher();
                        SkillParam.parent = castParams.launcher;
                        SkillParam.target = castParams.target;
                        result = CastCircle(SkillParam, pMissle.m_nDir, nRefPX, nRefPY);
                    }
                    break;
            }

            return result;
        }

        private List<settings.skill.Missile> MissileForm_AtTarget(settings.skill.Params.Cast castParams)
        {
            List<settings.skill.Missile> result = new List<settings.skill.Missile>();

            int nSrcPX = 0;//点坐标
            int nSrcPY = 0;
            int nDesPX = 0;
            int nDesPY = 0;
            int nDir = 0;
            int nDirIndex = 0;
            int nTargetId = -1;
            int nRefPX = 0;
            int nRefPY = 0;

            settings.skill.Params.TOrdinSkillParam SkillParam = new settings.skill.Params.TOrdinSkillParam();
            SkillParam.nWaitTime = castParams.nWaitTime;

            if (castParams.nParam1 == (int)skill.Defination.eSkillParamType.SKILL_SPT_Direction)
            {
                //UnityEngine.Debug.Log("return 1");

                return result;
            }

            switch (castParams.launcher.type)
            {
                case Defination.eSkillLauncherType.SKILL_SLT_Npc:
                    {
                        if(this.skillSetting.m_bTargetSelf != 0)
                        {
                            resource.map.Position launcherMapPosition = castParams.launcher.GetMapPosition();
                            nDesPX = launcherMapPosition.left;
                            nDesPY = launcherMapPosition.top;
                        }
                        else if (castParams.nParam1 != 0 || castParams.nParam2 != 0)
                        {
                            nTargetId = Param2PCoordinate(castParams, ref nDesPX, ref nDesPY, Defination.eSkillLauncherType.SKILL_SLT_Npc);
                        }
                        else
                        {
                            resource.map.Position targetMapPosition = castParams.target.GetMapPosition();
                            nDesPX = targetMapPosition.left;
                            nDesPY = targetMapPosition.top;
                        }

                        nDirIndex = skill.Static.g_GetDirIndex(nSrcPX, nSrcPY, nDesPX, nDesPY);
                        nDir = skill.Static.g_DirIndex2Dir(nDirIndex, skill.Defination.MaxMissleDir);
                        SkillParam.launcher = castParams.launcher;
                        SkillParam.target = castParams.target;
                        result = CastZone(SkillParam, nDir, nDesPX, nDesPY);
                    }
                    break;
                case Defination.eSkillLauncherType.SKILL_SLT_Obj:
                    {

                    }
                    break;
                case Defination.eSkillLauncherType.SKILL_SLT_Missle:
                    {
                        resource.map.Position launcherPosition = castParams.launcher.GetMapPosition();
                        nRefPX = launcherPosition.left;
                        nRefPY = launcherPosition.top;

                        skill.Missile pMissle = castParams.launcher.missile;

                        SkillParam.launcher = castParams.launcher.missile.GetLauncher();
                        SkillParam.parent = castParams.launcher;
                        SkillParam.target = castParams.target;
                        result = CastZone(SkillParam, pMissle.m_nDir, nRefPX, nRefPY);
                    }
                    break;
            }

            //UnityEngine.Debug.Log("MissileForm : LauncherType: " + castParams.launcher.type + " --> result.size: " + result.Count);

            return result;
        }

        private List<settings.skill.Missile> MissileForm_AtFirer(settings.skill.Params.Cast castParams)
        {
            List<settings.skill.Missile> result = new List<settings.skill.Missile>();

            int nSrcPX = 0;//点坐标
            int nSrcPY = 0;
            int nDesPX = 0;
            int nDesPY = 0;
            int nDir = 0;
            int nDirIndex = 0;
            int nRefPX = 0;
            int nRefPY = 0;

            settings.skill.Params.TOrdinSkillParam SkillParam = new settings.skill.Params.TOrdinSkillParam();
            SkillParam.nWaitTime = castParams.nWaitTime;

            if (castParams.nParam1 == (int)skill.Defination.eSkillParamType.SKILL_SPT_Direction)
            {
                return result;
            }

            switch (castParams.launcher.type)
            {
                case Defination.eSkillLauncherType.SKILL_SLT_Npc:
                    {
                        resource.map.Position launcherPosition = castParams.launcher.GetMapPosition();
                        nSrcPX = launcherPosition.left;
                        nSrcPY = launcherPosition.top;  

                        nDirIndex = skill.Static.g_GetDirIndex(nSrcPX, nSrcPY, nDesPX, nDesPY);
                        nDir = skill.Static.g_DirIndex2Dir(nDirIndex, skill.Defination.MaxMissleDir);
                        SkillParam.launcher = castParams.launcher;
                        SkillParam.target = castParams.target;
                        result = CastZone(SkillParam, nDir, nSrcPX, nSrcPY);
                    }
                    break;
                case Defination.eSkillLauncherType.SKILL_SLT_Obj:
                    {

                    }
                    break;
                case Defination.eSkillLauncherType.SKILL_SLT_Missle:
                    {
                        resource.map.Position launcherPosition = castParams.launcher.GetMapPosition();
                        nRefPX = launcherPosition.left;
                        nRefPY = launcherPosition.top;

                        skill.Missile pMissle = castParams.launcher.missile;

                        SkillParam.launcher = castParams.launcher.missile.GetLauncher();
                        SkillParam.parent = castParams.launcher;
                        SkillParam.target = castParams.target;
                        result = CastZone(SkillParam, pMissle.m_nDir, nRefPX, nRefPY);
                    }
                    break;
            }

            return result;
        }

        private List<settings.skill.Missile> MissileForm_Zone(settings.skill.Params.Cast castParams)
        {
            List<settings.skill.Missile> result = new List<settings.skill.Missile>();

            int nSrcPX = 0;//点坐标
            int nSrcPY = 0;
            int nDesPX = 0;
            int nDesPY = 0;
            int nDir = 0;
            int nDirIndex = 0;
            int nTargetId = -1;
            int nRefPX = 0;
            int nRefPY = 0;

            settings.skill.Params.TOrdinSkillParam SkillParam = new settings.skill.Params.TOrdinSkillParam();
            SkillParam.nWaitTime = castParams.nWaitTime;

            if (castParams.nParam1 == (int)skill.Defination.eSkillParamType.SKILL_SPT_Direction)
            {
                return result;
            }

            switch (castParams.launcher.type)
            {
                case Defination.eSkillLauncherType.SKILL_SLT_Npc:
                    {
                        if (castParams.nParam1 != 0 || castParams.nParam2 != 0)
                        {
                            nTargetId = Param2PCoordinate(castParams, ref nDesPX, ref nDesPY, Defination.eSkillLauncherType.SKILL_SLT_Npc);
                        }
                        else
                        {
                            resource.map.Position targetMapPosition = castParams.target.GetMapPosition();
                            nDesPX = targetMapPosition.left;
                            nDesPY = targetMapPosition.top;
                        }

                        resource.map.Position launcherPosition = castParams.launcher.GetMapPosition();
                        nSrcPX = launcherPosition.left;
                        nSrcPY = launcherPosition.top;

                        nDirIndex = skill.Static.g_GetDirIndex(nSrcPX, nSrcPY, nDesPX, nDesPY);
                        nDir = skill.Static.g_DirIndex2Dir(nDirIndex, skill.Defination.MaxMissleDir);
                        SkillParam.launcher = castParams.launcher;
                        SkillParam.target = castParams.target;
                        result = CastZone(SkillParam, nDir, nSrcPX, nSrcPY);
                    }
                    break;
                case Defination.eSkillLauncherType.SKILL_SLT_Obj:
                    {

                    }
                    break;
                case Defination.eSkillLauncherType.SKILL_SLT_Missle:
                    {
                        resource.map.Position launcherPosition = castParams.launcher.GetMapPosition();
                        nRefPX = launcherPosition.left;
                        nRefPY = launcherPosition.top;

                        skill.Missile pMissle = castParams.launcher.missile;

                        SkillParam.launcher = castParams.launcher.missile.GetLauncher();
                        SkillParam.parent = castParams.launcher;
                        SkillParam.target = castParams.target;
                        result = CastZone(SkillParam, pMissle.m_nDir, nRefPX, nRefPY);
                    }
                    break;
            }

            return result;
        }

        protected List<settings.skill.Missile> CastMissles(settings.skill.Params.Cast castParams)
        {
            List<settings.skill.Missile> result = new List<settings.skill.Missile>();

            //int nRegionId = 0;
            //int nDesMapX = 0;//地图坐标
            //int nDesMapY = 0;
            //int nDesOffX = 0;
            //int nDesOffY = 0;
            //int nSrcOffX = 0;
            //int nSrcOffY = 0;
            //int nSrcPX = 0;//点坐标
            //int nSrcPY = 0;
            //int nDesPX = 0;
            //int nDesPY = 0;
            //int nDistance = 0;
            //int nDir = 0;
            //int nDirIndex = 0;
            //int nTargetId = -1;
            //int nRefPX = 0;
            //int nRefPY = 0;

            //settings.skill.Params.TOrdinSkillParam SkillParam = new settings.skill.Params.TOrdinSkillParam();
            //SkillParam.nWaitTime = castParams.nWaitTime;


            if (castParams.launcher.HaveData() == false)
            {
                return result;
            }

            if (this.skillSetting.m_bBaseSkill != 0 && this.HasValidMissileSetting() == false)
            {
                UnityEngine.Debug.LogWarning(
                    "Skill missile data missing. skill=" + this.skillSetting.m_nId +
                    " childMissile=" + this.skillSetting.m_nChildSkillId);
                return result;
            }

            //UnityEngine.Debug.Log("this.skillSetting.m_eMisslesForm: " + this.skillSetting.m_eMisslesForm);

            switch (this.skillSetting.m_eMisslesForm)
            {
                case Defination.MisslesForm.SKILL_MF_Wall:
                    result = this.MissileForm_Wall(castParams);
                    break;

                case Defination.MisslesForm.SKILL_MF_Line:
                    result = this.MissileForm_Line(castParams);
                    break;

                case Defination.MisslesForm.SKILL_MF_Spread:
                    result = this.MissileForm_Spread(castParams);
                    break;

                case Defination.MisslesForm.SKILL_MF_Circle:
                    result = this.MissileForm_Circle(castParams);
                    break;

                case Defination.MisslesForm.SKILL_MF_Random:
                    break;

                case Defination.MisslesForm.SKILL_MF_AtTarget:
                    result = this.MissileForm_AtTarget(castParams);
                    break;

                case Defination.MisslesForm.SKILL_MF_AtFirer:
                    result = this.MissileForm_AtFirer(castParams);
                    break;

                case Defination.MisslesForm.SKILL_MF_Zone:
                    result = this.MissileForm_Zone(castParams);
                    break;
            }

            return result;
        }
    }
}
