
namespace game.resource.settings.npcres.state
{
    public class SpecialSpr
    {
        private string m_szName;
        private int m_nTotalDir;
        private int m_nTotalFrame;
        private int m_dwInterval;
        private int m_dwTimer;
        private int m_nCurDir;
        private int m_dwCurrentTime;
        private int m_nCurFrame;

        private bool isActive;
        private float npcPateOffsetY;
        private int updateRemaining;
        private int destroyRemaning;

        private UnityEngine.GameObject parent;
        private UnityEngine.SpriteRenderer spriteRendererComponent;
        private UnityEngine.RectTransform rectTransformComponent;

        public SpecialSpr()
        {
            this.m_szName = string.Empty;
            this.m_nTotalDir = 1;
            this.m_nTotalFrame = 1;
            this.m_dwInterval = 1;
            this.m_dwTimer = 0;
            this.m_nCurDir = 0;
            this.m_dwCurrentTime = 0;
            this.m_nCurFrame = 0;

            this.isActive = false;
            this.npcPateOffsetY = 0;
            this.updateRemaining = 0;
            this.destroyRemaning = 0;

            this.parent = null;
            this.spriteRendererComponent = null;
            this.rectTransformComponent = null;
        }

        public bool IsActive() => this.isActive;

        public void SetSpecialSpr(string sprPath)
        {
            if (this.isActive == true)
            {
                return;
            }

            this.m_szName = sprPath;
            this.isActive = true;
        }

        public void SetNpcPate(int value)
        {
            if (value == 0)
            {
                this.npcPateOffsetY = 0;
                return;
            }

            this.npcPateOffsetY = (((value * 887) >> 10) / -100f);
        }

        public void Initialize(UnityEngine.GameObject parent)
        {
            if (this.parent != null)
            {
                return;
            }

            resource.SPR.Info sprInfo = Game.Resource(this.m_szName).Get<resource.SPR.Info>();

            if (sprInfo != null)
            {
                int local_nInterval = sprInfo.interval;
                int local_nNumFramesGroup = sprInfo.directionCount;
                int local_nNumFrames = sprInfo.frameCount;

                if (local_nInterval <= 0)
                    local_nInterval = 1;
                if (local_nInterval > 1000)
                    local_nInterval = 1000;
                if (local_nNumFramesGroup <= 0)
                    local_nNumFramesGroup = 1;
                if (local_nNumFrames < local_nNumFramesGroup)
                    local_nNumFrames = local_nNumFramesGroup;

                this.m_nTotalDir = local_nNumFramesGroup;

                if (local_nNumFrames < this.m_nTotalDir)
                {
                    m_nTotalFrame = this.m_nTotalDir;
                }
                else
                {
                    m_nTotalFrame = local_nNumFrames;
                }
                if (m_nTotalFrame < 1)
                    m_nTotalFrame = 1;

                this.m_dwInterval = (local_nNumFrames / local_nNumFramesGroup) * local_nInterval / 50;
                this.m_dwTimer = 0;
            }

            this.parent = new UnityEngine.GameObject("special.spr");
            this.spriteRendererComponent = this.parent.AddComponent<UnityEngine.SpriteRenderer>();
            this.rectTransformComponent = this.parent.AddComponent<UnityEngine.RectTransform>();

            this.parent.transform.SetParent(parent.transform, false);
            this.spriteRendererComponent.sortingOrder = State.orderSpecialSpr;
        }

        public bool CalcNextFrame(bool bLoop = true)
        {
            int previousFrame = this.m_nCurFrame;
            bool bRetVal = true;

            if (m_dwInterval <= 0)
                m_dwInterval = 1;

            if (m_dwCurrentTime - m_dwTimer >= m_dwInterval)
            {
                if (bLoop)
                {
                    m_dwTimer = m_dwCurrentTime;
                    m_nCurFrame = (m_nTotalFrame / m_nTotalDir) * m_nCurDir;
                    bRetVal = true;
                }
                else
                {
                    m_nCurFrame = (m_nTotalFrame / m_nTotalDir) * (m_nCurDir + 1) - 1;
                    bRetVal = true;
                }
            }
            else
            {
                m_nCurFrame = (m_nTotalFrame / m_nTotalDir) * m_nCurDir + (m_nTotalFrame / m_nTotalDir) * (m_dwCurrentTime - m_dwTimer) / m_dwInterval;
                bRetVal = true;
            }

            this.m_dwCurrentTime++;

            if (m_nCurFrame != previousFrame)
            {
                this.updateRemaining++;
            }

            return bRetVal;
        }

        public bool CheckEnd()
        {
            if (m_nCurFrame == (m_nTotalFrame / m_nTotalDir) * (m_nCurDir + 1) - 1)
                return true;
            return false;
        }

        public void Release()
        {
            this.isActive = false;
            this.destroyRemaning++;

            this.m_szName = string.Empty;
            this.m_nTotalDir = 1;
            this.m_nTotalFrame = 1;
            this.m_dwInterval = 1;
            this.m_dwTimer = 0;
            this.m_nCurDir = 0;
            this.m_dwCurrentTime = 0;
            this.m_nCurFrame = 0;
        }

        public void Destroy()
        {
            if (this.destroyRemaning <= 0)
            {
                return;
            }

            this.destroyRemaning--;

            UnityEngine.GameObject.Destroy(this.parent);

            this.parent = null;
            this.spriteRendererComponent = null;
            this.rectTransformComponent = null;
        }

        public void Update()
        {
            if (this.updateRemaining > 0)
            {
                this.updateRemaining--;
            }
            else
            {
                return;
            }

            skill.texture.SprCache.Data.SprFrame sprFrame = skill.texture.Cache.GetSprFrame(this.m_szName, (ushort)this.m_nCurFrame);

            if (sprFrame == null)
            {
                return;
            }

            this.rectTransformComponent.sizeDelta = sprFrame.sizeDelta;
            this.rectTransformComponent.anchoredPosition = new UnityEngine.Vector2(sprFrame.anchoredPosition.x, sprFrame.anchoredPosition.y - this.npcPateOffsetY);
            this.spriteRendererComponent.sprite = sprFrame.sprite;
        }
    }
}
