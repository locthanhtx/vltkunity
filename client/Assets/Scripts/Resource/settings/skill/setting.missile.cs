
using System.Text.RegularExpressions;

namespace game.resource.settings.skill
{
    public class MissileSetting
    {
        public class AnimateFile
        {
            public string AnimFileName;
            public int nTotalFrame;
            public int nInterval;
            public int nDir;
            public string SndFileName;

            public AnimateFile(string animFileName, string sndFileName, string animFileInfo)
            {
                this.AnimFileName = animFileName;
                this.SndFileName = sndFileName;
                this.nTotalFrame = 100;
                this.nDir = 16;
                this.nInterval = 1;

                string[] infoVector = (animFileInfo ?? string.Empty).Split(',');
                this.nTotalFrame = ParseAnimFileInfo(infoVector, 0, this.nTotalFrame);
                this.nDir = ParseAnimFileInfo(infoVector, 1, this.nDir);
                this.nInterval = ParseAnimFileInfo(infoVector, 2, this.nInterval);
            }

            public bool HasAnimation()
            {
                return string.IsNullOrWhiteSpace(this.AnimFileName) == false
                    && this.nTotalFrame > 0
                    && this.nDir > 0
                    && this.nInterval > 0;
            }

            private static int ParseAnimFileInfo(string[] infoVector, int index, int fallback)
            {
                if (infoVector == null || infoVector.Length <= index)
                {
                    return fallback;
                }

                string value = Regex.Replace(infoVector[index] ?? string.Empty, "[^0-9-]", string.Empty);
                if (string.IsNullOrEmpty(value))
                {
                    return fallback;
                }

                return int.TryParse(value, out int parsedValue) ? parsedValue : fallback;
            }
        }

        ////////////////////////////////////////////////////////////////////////////////

        public int m_nMissleId;
        public string m_szMissleName;
        public int m_nHeight;
        public int m_nLifeTime;
        public int m_nSpeed;
        public int m_nSkillId;
        public int m_nCollideRange;
        public int m_bCollideVanish;
        public int m_bCollideFriend;
        public int m_bCanSlow;
        public int m_bRangeDamage;
        public int m_nDamageRange;
        public skill.Defination.MissleMoveKind m_eMoveKind;
        public skill.Defination.MissleFollowKind m_eFollowKind;
        public int m_nZAcceleration;
        public int m_nHeightSpeed;
        public int m_nParam1;
        public int m_nParam2;
        public int m_nParam3;
        public int m_bAutoExplode;
        public int m_ulDamageInterval;
        public int m_btRedLum;
        public int m_btGreenLum;
        public int m_btBlueLum;
        public int m_usLightRadius;
        public int m_bMultiShow;
        public MissileSetting.AnimateFile[] m_MissleRes;
        public int m_bLoopAnim;
        public int m_bSubLoop;
        public int m_nSubStart;
        public int m_nSubStop;
        public int m_bFollowNpcWhenCollid;

        ////////////////////////////////////////////////////////////////////////////////

        public MissileSetting.AnimateFile GetAnimateFile(skill.Defination.MissleStatus status)
        {
            if (this.m_MissleRes == null)
            {
                return null;
            }

            int index = (int)status;
            if (index < 0 || index >= skill.Defination.MAX_MISSLE_STATUS)
            {
                return null;
            }

            MissileSetting.AnimateFile primary = this.m_MissleRes[index];
            if (primary != null && primary.HasAnimation())
            {
                return primary;
            }

            int fallbackIndex = index + skill.Defination.MAX_MISSLE_STATUS;
            if (fallbackIndex < this.m_MissleRes.Length)
            {
                MissileSetting.AnimateFile fallback = this.m_MissleRes[fallbackIndex];
                if (fallback != null && fallback.HasAnimation())
                {
                    return fallback;
                }
            }

            return null;
        }

        public bool IsValid()
        {
            return this.m_nMissleId > 0 && this.m_MissleRes != null;
        }

        ////////////////////////////////////////////////////////////////////////////////

        private void LoadMissile(int missileId)
        {
            if (TryResolveMissileRowIndex(missileId, out int rowIndex) == false)
            {
                return;
            }

            this.m_nMissleId = missileId;
            this.m_szMissleName = Cache.Settings.Skill.missilesTable.Get<string>((int)mapping.settings.Missile.HeaderIndexer.MissleName, rowIndex);
            this.m_nHeight = Cache.Settings.Skill.missilesTable.Get<int>((int)mapping.settings.Missile.HeaderIndexer.MissleHeight, rowIndex) << 10; // << 10
            this.m_nLifeTime = Cache.Settings.Skill.missilesTable.Get<int>((int)mapping.settings.Missile.HeaderIndexer.LifeTime, rowIndex);
            this.m_nSpeed = Cache.Settings.Skill.missilesTable.Get<int>((int)mapping.settings.Missile.HeaderIndexer.Speed, rowIndex);
            this.m_nSkillId = Cache.Settings.Skill.missilesTable.Get<int>((int)mapping.settings.Missile.HeaderIndexer.ResponseSkill, rowIndex);
            this.m_nCollideRange = Cache.Settings.Skill.missilesTable.Get<int>((int)mapping.settings.Missile.HeaderIndexer.CollidRange, rowIndex);
            this.m_bCollideVanish = Cache.Settings.Skill.missilesTable.Get<int>((int)mapping.settings.Missile.HeaderIndexer.ColVanish, rowIndex);
            this.m_bCollideFriend = Cache.Settings.Skill.missilesTable.Get<int>((int)mapping.settings.Missile.HeaderIndexer.CanColFriend, rowIndex);
            this.m_bCanSlow = Cache.Settings.Skill.missilesTable.Get<int>((int)mapping.settings.Missile.HeaderIndexer.CanSlow, rowIndex);
            this.m_bRangeDamage = Cache.Settings.Skill.missilesTable.Get<int>((int)mapping.settings.Missile.HeaderIndexer.IsRangeDmg, rowIndex);
            this.m_nDamageRange = Cache.Settings.Skill.missilesTable.Get<int>((int)mapping.settings.Missile.HeaderIndexer.DmgRange, rowIndex);
            this.m_eMoveKind = (skill.Defination.MissleMoveKind)Cache.Settings.Skill.missilesTable.Get<int>((int)mapping.settings.Missile.HeaderIndexer.MoveKind, rowIndex);
            this.m_eFollowKind = (skill.Defination.MissleFollowKind)Cache.Settings.Skill.missilesTable.Get<int>((int)mapping.settings.Missile.HeaderIndexer.FollowKind, rowIndex);
            this.m_nZAcceleration = Cache.Settings.Skill.missilesTable.Get<int>((int)mapping.settings.Missile.HeaderIndexer.Zacc, rowIndex);
            this.m_nHeightSpeed = Cache.Settings.Skill.missilesTable.Get<int>((int)mapping.settings.Missile.HeaderIndexer.Zspeed, rowIndex);
            this.m_nParam1 = Cache.Settings.Skill.missilesTable.Get<int>((int)mapping.settings.Missile.HeaderIndexer.Param1, rowIndex);
            this.m_nParam2 = Cache.Settings.Skill.missilesTable.Get<int>((int)mapping.settings.Missile.HeaderIndexer.Param2, rowIndex);
            this.m_nParam3 = Cache.Settings.Skill.missilesTable.Get<int>((int)mapping.settings.Missile.HeaderIndexer.Param3, rowIndex);
            this.m_bAutoExplode = Cache.Settings.Skill.missilesTable.Get<int>((int)mapping.settings.Missile.HeaderIndexer.AutoExplode, rowIndex);
            this.m_ulDamageInterval = Cache.Settings.Skill.missilesTable.Get<int>((int)mapping.settings.Missile.HeaderIndexer.DmgInterval, rowIndex);
            this.m_btRedLum = Cache.Settings.Skill.missilesTable.Get<int>((int)mapping.settings.Missile.HeaderIndexer.RedLum, rowIndex);
            this.m_btGreenLum = Cache.Settings.Skill.missilesTable.Get<int>((int)mapping.settings.Missile.HeaderIndexer.GreenLum, rowIndex);
            this.m_btBlueLum = Cache.Settings.Skill.missilesTable.Get<int>((int)mapping.settings.Missile.HeaderIndexer.BlueLum, rowIndex);
            this.m_usLightRadius = Cache.Settings.Skill.missilesTable.Get<int>((int)mapping.settings.Missile.HeaderIndexer.LightRadius, rowIndex);
            this.m_bMultiShow = Cache.Settings.Skill.missilesTable.Get<int>((int)mapping.settings.Missile.HeaderIndexer.MultiShow, rowIndex);

            this.m_MissleRes = new MissileSetting.AnimateFile[skill.Defination.MAX_MISSLE_STATUS * 2];

            for (int index = 0; index < skill.Defination.MAX_MISSLE_STATUS; index++)
            {
                this.m_MissleRes[index] = new MissileSetting.AnimateFile(
                    Cache.Settings.Skill.missilesTable.Get<string>("AnimFile" + (index + 1), rowIndex),
                    Cache.Settings.Skill.missilesTable.Get<string>("SndFile" + (index + 1), rowIndex),
                    Cache.Settings.Skill.missilesTable.Get<string>("AnimFileInfo" + (index + 1), rowIndex)
                );

                this.m_MissleRes[index + skill.Defination.MAX_MISSLE_STATUS] = new MissileSetting.AnimateFile(
                    Cache.Settings.Skill.missilesTable.Get<string>("AnimFileB" + (index + 1), rowIndex),
                    Cache.Settings.Skill.missilesTable.Get<string>("SndFileB" + (index + 1), rowIndex),
                    Cache.Settings.Skill.missilesTable.Get<string>("AnimFileInfoB" + (index + 1), rowIndex)
                );
            }

            this.m_bLoopAnim = Cache.Settings.Skill.missilesTable.Get<int>((int)mapping.settings.Missile.HeaderIndexer.LoopPlay, rowIndex);
            this.m_bSubLoop = Cache.Settings.Skill.missilesTable.Get<int>((int)mapping.settings.Missile.HeaderIndexer.SubLoop, rowIndex);
            this.m_nSubStart = Cache.Settings.Skill.missilesTable.Get<int>((int)mapping.settings.Missile.HeaderIndexer.SubStart, rowIndex);
            this.m_nSubStop = Cache.Settings.Skill.missilesTable.Get<int>((int)mapping.settings.Missile.HeaderIndexer.SubStop, rowIndex);
            this.m_bFollowNpcWhenCollid = Cache.Settings.Skill.missilesTable.Get<int>((int)mapping.settings.Missile.HeaderIndexer.ColFollowTarget, rowIndex);
        }

        private static bool TryResolveMissileRowIndex(int missileId, out int rowIndex)
        {
            rowIndex = -1;

            if (missileId <= 0 || Cache.Settings.Skill.missilesTable == null)
            {
                return false;
            }

            if (missileId > 0 && missileId < Cache.Settings.Skill.missilesTable.RowCount)
            {
                rowIndex = missileId;
                return true;
            }

            if (Cache.Settings.Skill.missilesIdToRowIndexMapping != null
                && Cache.Settings.Skill.missilesIdToRowIndexMapping.TryGetValue(missileId, out int mappedRowIndex)
                && mappedRowIndex > 0
                && mappedRowIndex < Cache.Settings.Skill.missilesTable.RowCount)
            {
                rowIndex = mappedRowIndex;
                return true;
            }

            UnityEngine.Debug.LogWarning(
                "SkillProbe missile row missing id=" + missileId +
                " rowCount=" + Cache.Settings.Skill.missilesTable.RowCount);
            return false;
        }

        ////////////////////////////////////////////////////////////////////////////////
        
        public static skill.MissileSetting Get(int missileId)
        {
            if(Cache.Settings.Skill.missilesIdToDataMapping.ContainsKey(missileId) == false)
            {
                skill.MissileSetting newMissileSetting = new skill.MissileSetting();
                newMissileSetting.LoadMissile(missileId);

                Cache.Settings.Skill.missilesIdToDataMapping[missileId] = newMissileSetting;
            }

            return Cache.Settings.Skill.missilesIdToDataMapping[missileId];
        }
    }
}
