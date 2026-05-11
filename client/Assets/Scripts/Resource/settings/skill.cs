
namespace game.resource.settings
{
	public class Skill : skill.CastSkill
	{
        private static readonly System.Collections.Generic.HashSet<string> SkillProbeLogs = new();

		public static void Initialize()
		{
			new settings.skill.Initialize();
		}

		////////////////////////////////////////////////////////////////////////////////
		
		public Skill(int skillId, int skillLevel, resource.Map map)
		{
            this.self = this;
            this.map = map;
            this.skillSetting = settings.skill.SkillSetting.GetRuntimeBase(skillId, skillLevel);
            if (this.skillSetting == null || this.skillSetting.m_nId <= 0)
            {
                UnityEngine.Debug.LogWarning("Skill runtime setting missing. skill=" + skillId + " level=" + skillLevel);
                return;
            }

            if (this.skillSetting != null && this.skillSetting.m_nChildSkillId > 0)
            {
                this.missileSetting = settings.skill.MissileSetting.Get(this.skillSetting.m_nChildSkillId);
            }

            settings.skill.MissileSetting.AnimateFile flyResource =
                this.missileSetting?.GetAnimateFile(skill.Defination.MissleStatus.MS_DoFly);
            string probeKey = skillId + ":" + skillLevel + ":" + this.skillSetting.m_nChildSkillId;
            if (SkillProbeLogs.Add(probeKey))
            {
                UnityEngine.Debug.Log(
                    "SkillProbe setting skill=" + skillId +
                    " level=" + skillLevel +
                    " loadedSkill=" + this.skillSetting.m_nId +
                    " style=" + this.skillSetting.m_eSkillStyle +
                    " form=" + this.skillSetting.m_eMisslesForm +
                    " childSkill=" + this.skillSetting.m_nChildSkillId +
                    " missile=" + (this.missileSetting != null ? this.missileSetting.m_nMissleId : 0) +
                    " moveKind=" + (this.missileSetting != null ? this.missileSetting.m_eMoveKind.ToString() : "<null>") +
                    " followKind=" + (this.missileSetting != null ? this.missileSetting.m_eFollowKind.ToString() : "<null>") +
                    " flySpr=" + (flyResource != null ? flyResource.AnimFileName : "<null>") +
                    " flyFrames=" + (flyResource != null ? flyResource.nTotalFrame : 0) +
                    " flyDirs=" + (flyResource != null ? flyResource.nDir : 0) +
                    " flyInterval=" + (flyResource != null ? flyResource.nInterval : 0));
            }
        }
    }
}
