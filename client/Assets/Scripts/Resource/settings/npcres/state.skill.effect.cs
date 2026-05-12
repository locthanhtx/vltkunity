
using System.Collections.Generic;

namespace game.resource.settings.npcres.state
{
    public class KStateSpr : skill.StateSetting.Data
    {
        private int m_nCurDir = 0;
        private int m_dwTimer = 0;
        private int m_nCurFrame = 0;
        private int m_dwCurrentTime = 0;

        public int m_LeftTime = 0;
        public List<settings.skill.SkillSettingData.KMagicAttrib> m_State;

        private bool isActive;
        private int updateRemaining;
        private bool needDestroy;
        private float npcPateOffsetY;
        private UnityEngine.Vector2 anchoredOffset;
        private short order;

        public UnityEngine.GameObject parent;
        public UnityEngine.SpriteRenderer spriteRendererComponent;
        public UnityEngine.RectTransform rectTransformComponent;

        public KStateSpr()
        {
            this.m_State = new List<skill.SkillSettingData.KMagicAttrib>();

            this.isActive = false;
            this.updateRemaining = 0;
            this.needDestroy = false;
            this.npcPateOffsetY = 0;
            this.anchoredOffset = new UnityEngine.Vector2(0, 0);
            this.order = 0;
        }

        public void Initialize(UnityEngine.GameObject parent)
        {
            if (this.parent != null)
            {
                return;
            }

            this.parent = new UnityEngine.GameObject("state.id." + this.m_nID);
            this.spriteRendererComponent = this.parent.AddComponent<UnityEngine.SpriteRenderer>();
            this.rectTransformComponent = this.parent.AddComponent<UnityEngine.RectTransform>();

            this.parent.transform.SetParent(parent.transform, false);
        }

        public void SetActive(bool active) => this.isActive = active;

        public bool IsActive() => this.isActive;

        public void SetOrderLayer(short order) => this.order = order;

        public bool CalcNextFrame(bool bLoop = true)
        {
            int previousFrame = m_nCurFrame;
            bool bRetVal = true;

            if (m_nInterVal <= 0)
                m_nInterVal = 1;

            if (m_dwCurrentTime - m_dwTimer >= m_nInterVal)
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
                m_nCurFrame = (m_nTotalFrame / m_nTotalDir) * m_nCurDir + (m_nTotalFrame / m_nTotalDir) * (m_dwCurrentTime - m_dwTimer) / m_nInterVal;
                bRetVal = true;
            }

            this.m_dwCurrentTime++;

            if (m_nCurFrame != previousFrame)
            {
                this.updateRemaining++;
            }

            return bRetVal;
        }

        public void CalcAnchoredOffsetY()
        {
            if (this.m_nType == skill.Defination.StateMagicType.STATE_MAGIC_FOOT)
            {
                this.anchoredOffset.y = 0;
                return;
            }

            this.anchoredOffset.y = -this.npcPateOffsetY;
        }

        public void CalcOrderLayer()
        {
            if (this.m_nType != skill.Defination.StateMagicType.STATE_MAGIC_BODY)
            {
                return;
            }

            if (this.m_nBackStart <= this.m_nCurFrame && this.m_nCurFrame < this.m_nBackEnd)
            {
                this.order = State.orderBack;
                return;
            }

            if (this.m_nCurFrame < this.m_nBackStart || this.m_nCurFrame >= this.m_nBackEnd)
            {
                this.order = State.orderFront;
                return;
            }
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

        public void Activate()
        {
            if(this.m_LeftTime <= 0)
            {
                this.SetActive(false);
                this.needDestroy = true;
                return;
            }

            this.CalcNextFrame();
            this.CalcAnchoredOffsetY();
            this.CalcOrderLayer();

            this.m_LeftTime--;
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

            this.anchoredOffset.x = sprFrame.anchoredPosition.x;
            this.anchoredOffset.y += sprFrame.anchoredPosition.y;

            this.rectTransformComponent.sizeDelta = sprFrame.sizeDelta;
            this.rectTransformComponent.anchoredPosition = this.anchoredOffset;
            this.spriteRendererComponent.sprite = sprFrame.sprite;
            this.spriteRendererComponent.sortingOrder = this.order;
        }

        public void Destroy(npcres.Controller npcController)
        {
            if(this.needDestroy == false)
            {
                return;
            }

            if(this.parent == null)
            {
                return;
            }

            UnityEngine.GameObject.Destroy(this.parent);

            npcController.ModifyAttrib(this.m_State);

            this.m_State.Clear();
            this.parent = null;
            this.needDestroy = false;
        }
    }
}
