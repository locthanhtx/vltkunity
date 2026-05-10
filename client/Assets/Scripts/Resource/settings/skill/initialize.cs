
using System.Collections.Generic;

namespace game.resource.settings.skill
{
    public class Initialize
    {
        public Initialize()
        {
            resource.Table skillsTable = Game.Resource(resource.mapping.settings.Skill.filePath).Get<resource.Table>();

            if (skillsTable.IsEmpty())
            {
                UnityEngine.Debug.LogError(resource.mapping.settings.Skill.filePath);
            }
            else
            {
                Cache.Settings.Skill.skillsTable = skillsTable;
                Cache.Settings.Skill.skillsIdToRowIndexMapping = Initialize.SkillIdToRowIndex(skillsTable);
                Cache.Settings.Skill.skillsIdToDataMapping = new Dictionary<int, SkillSetting>();
            }

            resource.Table missilesTable = Game.Resource(resource.mapping.settings.Missile.filePath).Get<resource.Table>();

            if (missilesTable.IsEmpty())
            {
                UnityEngine.Debug.LogError(resource.mapping.settings.Missile.filePath);
            }
            else
            {
                Cache.Settings.Skill.missilesTable = missilesTable;
                Cache.Settings.Skill.missilesIdToRowIndexMapping = Initialize.MissileIdToRowIndex(missilesTable);
                Cache.Settings.Skill.missilesIdToDataMapping = new Dictionary<int, MissileSetting>();
            }

            Cache.Settings.Skill.textures = new Dictionary<string, texture.SprCache.Data>();
            Cache.Settings.Skill.stateMagicTable = skill.StateSetting.Load();
        }

        private static Dictionary<int, int> SkillIdToRowIndex(resource.Table skillTable)
        {
            Dictionary<int, int> result = new Dictionary<int, int>();

            for(int rowIndex = 1; rowIndex < skillTable.RowCount; rowIndex++)
            {
                int skillId = skillTable.Get<int>("SkillId", rowIndex);
                if (skillId > 0)
                {
                    result[skillId] = rowIndex;
                }
            }

            return result;
        }

        private static Dictionary<int, int> MissileIdToRowIndex(resource.Table missileTable)
        {
            Dictionary<int, int> result = new Dictionary<int, int>();

            for(int rowIndex = 1; rowIndex < missileTable.RowCount; rowIndex++)
            {
                result[rowIndex] = rowIndex;

                int missileId = missileTable.Get<int>("MissleId", rowIndex);
                if (missileId > 0 && result.ContainsKey(missileId) == false)
                {
                    result[missileId] = rowIndex;
                }
            }

            return result;
        }
    }
}
