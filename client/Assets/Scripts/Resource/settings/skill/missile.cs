
namespace game.resource.settings.skill
{
	public class Missile : skill.missile.Active
    {
        private static readonly System.Collections.Generic.HashSet<string> SkillProbeLogs = new();

		public Missile(
			skill.SkillSetting skillSetting, 
			skill.MissileSetting missileSetting, 
			resource.Map map,
			skill.Params.TOrdinSkillParam skillParam)
		{
			this.self = this;
            this.map = map;
            this.skillSetting = skillSetting;
            this.missileSetting = missileSetting;
			this.skillParam = skillParam;

            this.texture = new skill.Texture();

            this.m_nCurrentLife = 0;
            this.m_nXOffset = 0;
            this.m_nYOffset = 0;
            this.m_eMissleStatus = Defination.MissleStatus.MS_DoFly;
            this.m_nHeight = this.missileSetting.m_nHeight;
            this.m_nHeightSpeed = this.missileSetting.m_nHeightSpeed;
            this.m_nCurrentMapZ = this.m_nHeight >> 10;

            this.m_nTempParam1 = 0;
            this.m_nTempParam2 = 0;

			this.vanishEffect = new missile.Data.VanishEffect();

			this.isActive = false;
			this.isPainting = false;

			if(this.skillSetting.m_nInteruptTypeWhenMove != 0)
			{
				resource.map.Position launcherMapPosition = skillParam.launcher.GetMapPosition();
				this.m_nLauncherSrcPX = launcherMapPosition.left;
				this.m_nLauncherSrcPY = launcherMapPosition.top;
            }

            skill.MissileSetting.AnimateFile flyResource = missileSetting.GetAnimateFile(this.m_eMissleStatus);
            this.texture.SetSprPath(flyResource?.AnimFileName);
            this.texture.SetSprFrame(0);

            string probeKey = (skillSetting != null ? skillSetting.m_nId : 0) + ":" +
                              (missileSetting != null ? missileSetting.m_nMissleId : 0) + ":" +
                              (flyResource != null ? flyResource.AnimFileName : "<null>");
            if (SkillProbeLogs.Add(probeKey))
            {
                UnityEngine.Debug.Log(
                    "SkillProbe missile-created skill=" + (skillSetting != null ? skillSetting.m_nId : 0) +
                    " missile=" + (missileSetting != null ? missileSetting.m_nMissleId : 0) +
                    " status=" + this.m_eMissleStatus +
                    " spr=" + this.texture.GetSprPath() +
                    " frame=" + this.texture.GetSprFrame() +
                    " animFrames=" + (flyResource != null ? flyResource.nTotalFrame : 0) +
                    " animDirs=" + (flyResource != null ? flyResource.nDir : 0) +
                    " animInterval=" + (flyResource != null ? flyResource.nInterval : 0));
            }
        }

        ////////////////////////////////////////////////////////////////////////////////

        public UnityEngine.GameObject GetAppearance() => this.texture.GetAppearance();

        public string GetSprPath() => this.texture.GetSprPath();

        public ushort GetSprFrame() => this.texture.GetSprFrame();

        public resource.map.Position GetMapPosition() => this.texture.GetMapPosition();

    }
}
