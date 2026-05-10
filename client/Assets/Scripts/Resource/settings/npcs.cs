using System;
using System.Collections.Generic;

namespace game.resource.settings
{
    class Npcs
    {
        private const string PlayerBaseValuePath = "\\settings\\npc\\player\\BaseValue.ini";

        public struct MotionProfile
        {
            public int standFrame;
            public int walkFrame;
            public int runFrame;
            public int deathFrame;
            public int hurtFrame;
            public int walkSpeed;
            public int runSpeed;

            public static MotionProfile NpcDefault()
            {
                return new MotionProfile
                {
                    standFrame = 15,
                    walkFrame = 15,
                    runFrame = 15,
                    deathFrame = 12,
                    hurtFrame = 10,
                    walkSpeed = 5,
                    runSpeed = 10
                };
            }

            public static MotionProfile PlayerDefault()
            {
                return new MotionProfile
                {
                    standFrame = 15,
                    walkFrame = 15,
                    runFrame = 15,
                    deathFrame = 15,
                    hurtFrame = 12,
                    walkSpeed = 5,
                    runSpeed = 10
                };
            }

            public void ApplyTo(npcres.Datafield data)
            {
                if (data == null)
                {
                    return;
                }

                int previousWalkSpeed = data.m_WalkSpeed;
                int previousRunSpeed = data.m_RunSpeed;

                data.m_StandFrame = PositiveOrDefault(this.standFrame, 15);
                data.m_WalkFrame = PositiveOrDefault(this.walkFrame, 15);
                data.m_RunFrame = PositiveOrDefault(this.runFrame, 15);
                data.m_DeathFrame = PositiveOrDefault(this.deathFrame, 15);
                data.m_HurtFrame = PositiveOrDefault(this.hurtFrame, 10);
                data.m_WalkSpeed = PositiveOrDefault(this.walkSpeed, 5);
                data.m_RunSpeed = PositiveOrDefault(this.runSpeed, 10);

                if (data.m_CurrentWalkSpeed <= 0 || data.m_CurrentWalkSpeed == previousWalkSpeed)
                {
                    data.m_CurrentWalkSpeed = data.m_WalkSpeed;
                }

                if (data.m_CurrentRunSpeed <= 0 || data.m_CurrentRunSpeed == previousRunSpeed)
                {
                    data.m_CurrentRunSpeed = data.m_RunSpeed;
                }
            }
        }

        private static bool playerMotionProfilesLoaded;
        private static MotionProfile playerMaleMotionProfile;
        private static MotionProfile playerFemaleMotionProfile;

        public static void Initialize()
        {
            resource.Table declareTable = Game.Resource(mapping.settings.Npcs.fileFullPath).Get<resource.Table>();

            if(declareTable.IsEmpty())
            {
                UnityEngine.Debug.LogError(mapping.settings.Npcs.fileFullPath);
                return;
            }

            Cache.Settings.Npcs.declareRowIndexToResTypeMapping = Npcs.DeclareRowIndexToResTypeMapping(declareTable);
            Cache.Settings.Npcs.declareRowIndexToStatureMapping = Npcs.DeclareRowIndexToStatureMapping(declareTable);
            Cache.Settings.Npcs.declareRowIndexToMotionProfileMapping = Npcs.DeclareRowIndexToMotionProfileMapping(declareTable);
        }

        public static string GetNpcResType(int _declareLine)
        {
            int declareRowIndex = _declareLine - 1;

            if(Cache.Settings.Npcs.declareRowIndexToResTypeMapping.ContainsKey(declareRowIndex) == false)
            {
                return string.Empty;
            }

            return Cache.Settings.Npcs.declareRowIndexToResTypeMapping[declareRowIndex];
        }

        public static int GetNpcStature(int _declareLine)
        {
            int declareRowIndex = _declareLine - 1;

            if (Cache.Settings.Npcs.declareRowIndexToStatureMapping.ContainsKey(declareRowIndex) == false)
            {
                return 0;
            }

            return Cache.Settings.Npcs.declareRowIndexToStatureMapping[declareRowIndex];
        }

        public static MotionProfile GetNpcMotionProfile(int _declareLine)
        {
            int declareRowIndex = _declareLine - 1;

            if (Cache.Settings.Npcs.declareRowIndexToMotionProfileMapping == null ||
                Cache.Settings.Npcs.declareRowIndexToMotionProfileMapping.ContainsKey(declareRowIndex) == false)
            {
                return MotionProfile.NpcDefault();
            }

            return Cache.Settings.Npcs.declareRowIndexToMotionProfileMapping[declareRowIndex];
        }

        public static void ApplyNpcMotionProfile(npcres.Datafield data, int _declareLine)
        {
            GetNpcMotionProfile(_declareLine).ApplyTo(data);
        }

        public static void ApplyPlayerMotionProfile(npcres.Datafield data, string characterType)
        {
            GetPlayerMotionProfile(characterType).ApplyTo(data);
        }

        ///////////////////////////////////////////////////////////////////////////

        private static int PositiveOrDefault(int value, int defaultValue)
        {
            return value > 0 ? value : defaultValue;
        }

        private static MotionProfile GetPlayerMotionProfile(string characterType)
        {
            EnsurePlayerMotionProfiles();
            return characterType == NpcRes.SpecialType.lady
                ? playerFemaleMotionProfile
                : playerMaleMotionProfile;
        }

        private static void EnsurePlayerMotionProfiles()
        {
            if (playerMotionProfilesLoaded)
            {
                return;
            }

            playerMotionProfilesLoaded = true;
            playerMaleMotionProfile = MotionProfile.PlayerDefault();
            playerFemaleMotionProfile = MotionProfile.PlayerDefault();

            try
            {
                resource.Ini baseValue = Game.Resource(PlayerBaseValuePath).Get<resource.Ini>();
                if (baseValue == null || baseValue.IsEmpty())
                {
                    return;
                }

                int walkSpeed = IniInt(baseValue, "Common", "WalkSpeed", playerMaleMotionProfile.walkSpeed);
                int runSpeed = IniInt(baseValue, "Common", "RunSpeed", playerMaleMotionProfile.runSpeed);
                int hurtFrame = IniInt(baseValue, "Common", "HurtFrame", playerMaleMotionProfile.hurtFrame);

                playerMaleMotionProfile.walkSpeed = walkSpeed;
                playerMaleMotionProfile.runSpeed = runSpeed;
                playerMaleMotionProfile.hurtFrame = hurtFrame;
                playerMaleMotionProfile.standFrame = IniInt(baseValue, "Male", "StandFrame", playerMaleMotionProfile.standFrame);
                playerMaleMotionProfile.walkFrame = IniInt(baseValue, "Male", "WalkFrame", playerMaleMotionProfile.walkFrame);
                playerMaleMotionProfile.runFrame = IniInt(baseValue, "Male", "RunFrame", playerMaleMotionProfile.runFrame);

                playerFemaleMotionProfile.walkSpeed = walkSpeed;
                playerFemaleMotionProfile.runSpeed = runSpeed;
                playerFemaleMotionProfile.hurtFrame = hurtFrame;
                playerFemaleMotionProfile.standFrame = IniInt(baseValue, "Female", "StandFrame", playerFemaleMotionProfile.standFrame);
                playerFemaleMotionProfile.walkFrame = IniInt(baseValue, "Female", "WalkFrame", playerFemaleMotionProfile.walkFrame);
                playerFemaleMotionProfile.runFrame = IniInt(baseValue, "Female", "RunFrame", playerFemaleMotionProfile.runFrame);
            }
            catch (Exception exception)
            {
                UnityEngine.Debug.LogWarning("Npcs BaseValue profile fallback: " + exception.Message);
            }
        }

        private static int IniInt(resource.Ini ini, string section, string key, int defaultValue)
        {
            int value = ini.Get<int>(section, key);
            return PositiveOrDefault(value, defaultValue);
        }
        
        private static Dictionary<int, string> DeclareRowIndexToNpcName(resource.Table _declareTable)
        {
            Dictionary<int, string> result = new Dictionary<int, string>();

            for (int rowIndex = 1; rowIndex < _declareTable.RowCount; rowIndex++)
            {
                string npcResType = _declareTable.Get<string>((int)mapping.settings.Npcs.HeaderIndexer.Name, rowIndex);
                result.Add(rowIndex, npcResType);
            }

            return result;
        }

        private static Dictionary<int, string> DeclareRowIndexToResTypeMapping(resource.Table _declareTable)
        {
            Dictionary<int, string> result = new Dictionary<int, string>();

            for(int rowIndex = 1; rowIndex < _declareTable.RowCount; rowIndex++)
            {
                string npcResType = _declareTable.Get<string>((int)mapping.settings.Npcs.HeaderIndexer.NpcResType, rowIndex);
                result.Add(rowIndex, npcResType);
            }

            return result;
        }

        private static Dictionary<int, int> DeclareRowIndexToStatureMapping(resource.Table _declareTable)
        {
            Dictionary<int, int> result = new Dictionary<int, int>();

            for (int rowIndex = 1; rowIndex < _declareTable.RowCount; rowIndex++)
            {
                int stature = _declareTable.Get<int>((int)mapping.settings.Npcs.HeaderIndexer.Stature, rowIndex);
                result.Add(rowIndex, stature);
            }

            return result;
        }

        private static Dictionary<int, MotionProfile> DeclareRowIndexToMotionProfileMapping(resource.Table _declareTable)
        {
            Dictionary<int, MotionProfile> result = new Dictionary<int, MotionProfile>();

            for (int rowIndex = 1; rowIndex < _declareTable.RowCount; rowIndex++)
            {
                MotionProfile profile = MotionProfile.NpcDefault();
                profile.standFrame = _declareTable.Get<int>((int)mapping.settings.Npcs.HeaderIndexer.StandFrame, rowIndex, profile.standFrame);
                profile.walkFrame = _declareTable.Get<int>((int)mapping.settings.Npcs.HeaderIndexer.WalkFrame, rowIndex, profile.walkFrame);
                profile.runFrame = _declareTable.Get<int>((int)mapping.settings.Npcs.HeaderIndexer.RunFrame, rowIndex, profile.runFrame);
                profile.deathFrame = _declareTable.Get<int>((int)mapping.settings.Npcs.HeaderIndexer.DeathFrame, rowIndex, profile.deathFrame);
                profile.hurtFrame = _declareTable.Get<int>((int)mapping.settings.Npcs.HeaderIndexer.HurtFrame, rowIndex, profile.hurtFrame);
                profile.walkSpeed = _declareTable.Get<int>((int)mapping.settings.Npcs.HeaderIndexer.WalkSpeed, rowIndex, profile.walkSpeed);
                profile.runSpeed = _declareTable.Get<int>((int)mapping.settings.Npcs.HeaderIndexer.RunSpeed, rowIndex, profile.runSpeed);

                result.Add(rowIndex, profile);
            }

            return result;
        }
    }
}
