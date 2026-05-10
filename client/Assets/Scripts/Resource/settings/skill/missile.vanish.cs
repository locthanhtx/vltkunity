
namespace game.resource.settings.skill.missile
{
    public class Vanish : skill.missile.Collision
    {
        public void DoVanish()
        {
            if (this.m_eMissleStatus == Defination.MissleStatus.MS_DoVanish) return;

            if (this.skillSetting.m_bVanishedEvent != 0)
            {
                OnMissleEvent(Defination.MissileEvent.Missle_VanishEvent);
            }

            skill.MissileSetting.AnimateFile missileRes = this.missileSetting.GetAnimateFile(Defination.MissleStatus.MS_DoVanish);
            if (missileRes == null && this.missileSetting.m_bCollideVanish != 0)
            {
                missileRes = this.missileSetting.GetAnimateFile(Defination.MissleStatus.MS_DoCollision);
            }

            if(missileRes != null)
            {
                this.vanishEffect.m_nBeginTime = this.m_nCurrentLife;
                this.vanishEffect.m_nEndTime = this.vanishEffect.m_nBeginTime + (missileRes.nInterval * missileRes.nTotalFrame / missileRes.nDir);
                this.vanishEffect.m_nCurDir = skill.Static.g_DirIndex2Dir(m_nDirIndex, missileRes.nDir);
            }

            this.m_eMissleStatus = Defination.MissleStatus.MS_DoVanish;
            return;
        }

        protected void OnVanish()
        {

        }
    }
}
