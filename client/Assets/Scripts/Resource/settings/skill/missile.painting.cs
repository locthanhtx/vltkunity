
namespace game.resource.settings.skill.missile
{
    public class Painting : skill.missile.Wait
    {
        private void DrawOnFly(skill.Defination.MissleStatus eStatus, int nX, int nY, int nZ, int nDir, int nAllFrame, int nCurLifeFrame)
        {
            //UnityEngine.Debug.Log("status == doFly");

            if (nCurLifeFrame < 0 || (nAllFrame != 0 && nAllFrame < nCurLifeFrame))
            {
                //if (this.skillSetting.m_nId == 301)
                //{
                //    UnityEngine.Debug.Log(this.skillSetting.m_nId + " --> return 1");
                //}

                //UnityEngine.Debug.Log("return 1");

                return;
            }

            skill.MissileSetting.AnimateFile missileRes = this.missileSetting.GetAnimateFile(eStatus);
            if (missileRes == null)
            {
                return;
            }

            int nSprDir = missileRes.nDir;
            int nSprFrames = missileRes.nTotalFrame;
            if (nSprDir != 0 && nSprFrames != 0)
            {
                int dirSegment = System.Math.Max(1, 64 / nSprDir);
                int halfDirSegment = System.Math.Max(1, 32 / nSprDir);
                int nImageDir = (nDir / dirSegment);
                int nImageDir1 = (nDir % dirSegment);
                if (nImageDir1 >= halfDirSegment) nImageDir++;
                if (nImageDir >= nSprDir) nImageDir = 0;

                int nFramePerDir = (nSprFrames / nSprDir);
                if (nFramePerDir <= 0)
                {
                    return;
                }

                if (nAllFrame == 0) nAllFrame = nFramePerDir;
                int nFirstFrame = nImageDir * nFramePerDir;
                int nTotalFrame = nSprFrames / nSprDir;
                int nFrame = nCurLifeFrame;

                {
                    if (this.missileSetting.m_bLoopAnim != 0)
                    {
                        if (this.missileSetting.m_bSubLoop == 0)
                        {
                            nFrame = (nCurLifeFrame / System.Math.Max(1, missileRes.nInterval)) % nTotalFrame;
                        }
                        else
                        {
                            if ((nCurLifeFrame / System.Math.Max(1, missileRes.nInterval)) < this.missileSetting.m_nSubStart)
                                nFrame = nCurLifeFrame / System.Math.Max(1, missileRes.nInterval);
                            else
                            {
                                if (this.missileSetting.m_nSubStart == this.missileSetting.m_nSubStop)
                                    nFrame = this.missileSetting.m_nSubStart;
                                else
                                    nFrame = this.missileSetting.m_nSubStart + ((nCurLifeFrame - this.missileSetting.m_nSubStart) / System.Math.Max(1, missileRes.nInterval)) % (this.missileSetting.m_nSubStop - this.missileSetting.m_nSubStart);
                            }
                        }
                    }
                    else
                    {
                        nFrame = nTotalFrame * nCurLifeFrame / nAllFrame;
                    }

                    if (nFrame > (nTotalFrame - 1))
                    {
                        //UnityEngine.Debug.Log(this.skillSetting.m_nId + " --> return 2");

                        //if (this.skillSetting.m_nId == 301)
                        //{
                        //    UnityEngine.Debug.Log(this.skillSetting.m_nId + " --> return 2");
                        //}

                        //UnityEngine.Debug.Log("return 2");

                        return;
                    }
                }
                nFrame = nFirstFrame + nFrame;

                this.texture.SetSprPath(missileRes.AnimFileName);
                this.texture.SetSprFrame((ushort)nFrame);
                this.texture.SetMapPosition(nY, nX);
                this.texture.Apply();

                //UnityEngine.Debug.Log("updated!!!");

                //UnityEngine.Debug.Log(this.skillSetting.m_nId + ", sprPath: " + missileRes.AnimFileName + ", frame: " + nFrame); 

                //if(this.skillSetting.m_nId == 301)
                //{
                //    UnityEngine.Debug.Log(this.skillSetting.m_nId + " --> texture up to date!!!");
                //}
            }
        }

        private void DrawOnVanish(int nCurLifeFrame, int m_nPX, int m_nPY)
        {
            //UnityEngine.Debug.Log("Start Vanish Effect");

            skill.MissileSetting.AnimateFile missileRes = this.missileSetting.GetAnimateFile(Defination.MissleStatus.MS_DoVanish);

            if (missileRes != null && missileRes.nInterval > 0 && missileRes.nDir > 0 && missileRes.nTotalFrame > 0)
            {
                int dirSegment = System.Math.Max(1, 64 / missileRes.nDir);
                int halfDirSegment = System.Math.Max(1, 32 / missileRes.nDir);
                int nImageDir = (this.vanishEffect.m_nCurDir / dirSegment);
                int nImageDir1 = (this.vanishEffect.m_nCurDir % dirSegment);
                if (nImageDir1 >= halfDirSegment) nImageDir++;
                if (nImageDir >= missileRes.nDir) nImageDir = 0;

                int nFrame = (nCurLifeFrame - this.vanishEffect.m_nBeginTime) / missileRes.nInterval + nImageDir * missileRes.nTotalFrame / missileRes.nDir;
                if (nFrame < 0 || nFrame >= missileRes.nTotalFrame)
                {
                    return;
                }

                //UnityEngine.Debug.Log("painting at: " + nCurLifeFrame + ", nFrame: " +  nFrame);

                this.texture.SetSprPath(missileRes.AnimFileName);
                this.texture.SetSprFrame((ushort)nFrame);
                this.texture.SetMapPosition(m_nPY, m_nPX);
                this.texture.Apply();
            }
        }

        protected void PreParePaint()
        {
            int nSrcX;
            int nSrcY;

            nSrcY = this.m_nRefPY + this.m_nYOffset;
            nSrcY -= (this.m_nCurrentMapZ * 887) >> 10;
            nSrcY += 5;
            nSrcX = this.m_nRefPX + this.m_nXOffset;

            switch (this.m_eMissleStatus)
            {
                case Defination.MissleStatus.MS_DoFly:
                    if (this.missileSetting.m_nZAcceleration == 0)
                    {
                        this.DrawOnFly(m_eMissleStatus, nSrcX, nSrcY, m_nCurrentMapZ, m_nDir, m_nLifeTime - m_nStartLifeTime, m_nCurrentLife - m_nStartLifeTime);
                    }
                    else
                    {
                        int nDirIndex = skill.Static.g_GetDirIndex(0, 0, m_nXFactor, m_nYFactor);
                        int nDir = skill.Static.g_DirIndex2Dir(nDirIndex, 64);
                        this.DrawOnFly(m_eMissleStatus, nSrcX, nSrcY, m_nCurrentMapZ, nDir, m_nLifeTime - m_nStartLifeTime, m_nCurrentLife - m_nStartLifeTime);
                    }
                    break;

                case Defination.MissleStatus.MS_DoVanish:
                    this.DrawOnVanish(this.m_nCurrentLife, nSrcX, nSrcY);
                    break;
            }
        }

        public bool Paint()
        {
            if (this.isPainting == true)
            {
                //System.Diagnostics.Stopwatch performance = System.Diagnostics.Stopwatch.StartNew();

                this.texture.Update();

                //performance.Stop();
                //UnityEngine.Debug.Log("paiting performance: " +  performance.ElapsedMilliseconds + " milliseconds");

                return true;
            }
            else
            {

            }

            if (this.isActive == true)
            {

            }
            else
            {
                this.texture.Release();
                return false;
            }

            return true;
        }
    }
}
