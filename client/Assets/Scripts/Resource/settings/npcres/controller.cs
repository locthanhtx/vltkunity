
using System;
using System.Collections.Generic;

namespace game.resource.settings.npcres
{
    public class Controller
    {
        protected struct AnimateHandler
        {
            public bool isSpecialNpc;
            public npcres.special.Animate special;
            public npcres.normal.Animate normal;

            public readonly Dictionary<string, npcres.Structures.PartAnimation> SetDirection(int direction)
            {
                if (this.isSpecialNpc)
                {
                    return this.special.SetDirection(direction);
                }

                return this.normal.SetDirection(direction);
            }


            public readonly Dictionary<string, npcres.Structures.PartAnimation> SyncDirection(int direction)
            {
                if (direction >= 0 && direction < 64)
                {
                    if (this.isSpecialNpc)
                    {
                        return this.special.SyncDirection(direction);
                    }
                    return this.normal.SyncDirection(direction);
                }
                return new();
            }



            public readonly int GetDirection()
            {
                if (this.isSpecialNpc)
                {
                    return this.special.GetDirection();
                }

                return this.normal.GetDirection();
            }


            public readonly Dictionary<string, npcres.Structures.PartAnimation> SetAction(string _action)
            {

                if (this.isSpecialNpc)
                {
                    return this.special.SetAction(_action);
                }

                return this.normal.SetAction(_action);
            }

            public readonly Dictionary<string, npcres.Structures.PartAnimation> GetPartAnimation() => this.isSpecialNpc ? this.special.GetPartAnimation() : this.normal.GetPartAnimation();

            public readonly string GetAction() => this.isSpecialNpc ? this.special.GetAction() : this.normal.GetAction();
        }

        ////////////////////////////////////////////////////////////////////////////////

        public readonly npcres.Map map;

        protected readonly npcres.Shape shape;
        protected readonly npcres.Callback callback;
        protected readonly npcres.Identification identify;
        protected readonly npcres.Position position;
        protected Controller.AnimateHandler animate;
        protected readonly npcres.State state;
        public readonly npcres.Datafield data;
        protected readonly npcres.Damage damage;
        private bool notUpdate = false;
        protected bool initIsSpecial;
        private int drivenTotalFrame;
        private int drivenCurrentFrame;
        private int drivenLastTick = -1;

        ////////////////////////////////////////////////////////////////////////////////

        protected Controller(string _objectName)
        {
            this.map = new npcres.Map();

            this.shape = new npcres.Shape(_objectName);
            this.callback = new npcres.Callback();
            this.identify = new npcres.Identification();
            this.position = new npcres.Position(this.identify);
            this.animate = new Controller.AnimateHandler();
            this.state = new npcres.State(this, this.shape.GetAppearance());
            this.data = new npcres.Datafield();
            this.damage = new npcres.Damage(this.data, this);
        }

        protected void InitForSpecial(string characterType = NpcRes.SpecialType.man, int direction = 1, string action = NpcRes.Action.normalStand1, int headIndex = 19, int bodyIndex = 19, int weaponIndex = 20, int horseIndex = -1)
        {
            this.animate.isSpecialNpc = true;
            this.animate.special = new npcres.special.Animate(this.identify, this.state, characterType, direction, action, headIndex, bodyIndex, weaponIndex, horseIndex);
            settings.Npcs.ApplyPlayerMotionProfile(this.data, characterType);
        }

        protected void InitForNormal()
        {
            this.animate.isSpecialNpc = false;
            this.animate.normal = new npcres.normal.Animate(this.identify, this.state);
            settings.Npcs.ApplyNpcMotionProfile(this.data, this.animate.normal.GetDeclareLine());
        }

        protected void InitForNormal(int npcDeclareLine, string actionName = mapping.settings.NpcRes.WeaponAction.normalStand1, int direction = 1)
        {
            this.animate.isSpecialNpc = false;
            this.animate.normal = new npcres.normal.Animate(this.identify, this.state, npcDeclareLine, actionName, direction);
            settings.Npcs.ApplyNpcMotionProfile(this.data, npcDeclareLine);
        }

        public void Destroy()
        {
            this.shape.Destroy();
            this.identify.Destroy();
        }

        ////////////////////////////////////////////////////////////////////////////////

        public void Activate()
        {
            this.state.Activate();
        }

        public void Update()
        {
            if (notUpdate)
            {
                this.state.Update();
                return;
            }

            float delta = UnityEngine.Time.timeSinceLevelLoad;
            ushort currentFrameIndex = 0, endFrameIndex = ushort.MaxValue;

            foreach (KeyValuePair<string, game.resource.settings.npcres.Structures.PartAnimation> partPair in this.animate.GetPartAnimation())
            {
                if (partPair.Value.sprPath == null)
                {
                    this.shape.InValidPart(partPair.Key);
                    continue;
                }

                currentFrameIndex = this.drivenTotalFrame > 0
                    ? partPair.Value.GetFrameIndex(this.drivenTotalFrame, this.drivenCurrentFrame)
                    : partPair.Value.GetNowFrameIndex(delta);
                endFrameIndex = partPair.Value.frameEnd;

                npcres.Shape.PartFields part = this.shape.GetPartFields(partPair.Key);
                npcres.Shape.PartFrame frameData = this.shape.GetPartFrame(partPair.Key, currentFrameIndex, partPair.Value);

                if (part.spriteRenderer.sprite == frameData.sprite)
                {
                    continue;
                }

                part.spriteRenderer.sortingOrder = partPair.Value.layerOrder;
                part.rectTransform.anchoredPosition = frameData.anchoredPosition;
                part.rectTransform.sizeDelta = frameData.sizeDelta;
                part.spriteRenderer.sprite = frameData.sprite;
            }

            this.callback.OnActionEnd(this.animate.GetAction(), currentFrameIndex, endFrameIndex);

            this.state.Update();
        }

        ////////////////////////////////////////////////////////////////////////////////

        public bool IsSpecialNpc() => this.initIsSpecial;

        public npcres.Shape.Appearance GetAppearance() => this.shape.GetAppearance();

        public npcres.Shape.PartFields GetPartField(string _partName) => this.shape.GetPartField(_partName);

        public Dictionary<string, npcres.Shape.PartFields> GetPartList() => this.shape.GetPartList();

        public npcres.Identification GetIdentify() => this.identify;

        public void SetDirection(int _direction) => this.shape.InValidPartList(this.animate.SetDirection(_direction));

        public void SyncDirection(int _direction) => this.shape.InValidPartList(this.animate.SyncDirection(_direction));

        public int GetDirection() => this.animate.GetDirection();

        public void SetAction(string _actionName)
        {
            string currentAction = animate.GetAction();

            if (_actionName != currentAction)
            {
                if (!GetAppearance().parent.activeSelf)// death -> revive
                {
                    GetAppearance().parent.SetActive(true);
                    GetIdentify().GetAppearance().SetActive(true);
                }

                notUpdate = false;
            }

            if (!IsMoveAction(_actionName))
            {
                this.ClearDrivenFrame();
            }
            else if (_actionName != currentAction)
            {
                this.ResetDrivenFrame();
            }

            this.shape.InValidPartList(this.animate.SetAction(_actionName));
        }

        private static bool IsMoveAction(string actionName)
        {
            return actionName == settings.NpcRes.Action.normalWalk ||
                   actionName == settings.NpcRes.Action.fightWalk ||
                   actionName == settings.NpcRes.Action.normalRun ||
                   actionName == settings.NpcRes.Action.fightRun;
        }

        public void SetDrivenFrame(int totalFrame, int currentFrame)
        {
            if (totalFrame <= 0)
            {
                this.ClearDrivenFrame();
                return;
            }

            this.drivenTotalFrame = totalFrame;
            this.drivenCurrentFrame = UnityEngine.Mathf.Clamp(currentFrame, 0, totalFrame - 1);
            this.drivenLastTick = GetCurrentCoreTick();
        }

        public void AdvanceDrivenFrame(int totalFrame)
        {
            if (totalFrame <= 0)
            {
                this.ClearDrivenFrame();
                return;
            }

            int currentTick = GetCurrentCoreTick();
            if (this.drivenTotalFrame != totalFrame)
            {
                this.drivenTotalFrame = totalFrame;
                this.drivenCurrentFrame %= totalFrame;
            }

            if (this.drivenLastTick < 0)
            {
                this.drivenLastTick = currentTick;
                return;
            }

            int elapsedTicks = currentTick - this.drivenLastTick;
            if (elapsedTicks <= 0)
            {
                return;
            }

            this.drivenCurrentFrame = (this.drivenCurrentFrame + elapsedTicks) % this.drivenTotalFrame;
            this.drivenLastTick = currentTick;
        }

        public void ClearDrivenFrame()
        {
            this.drivenTotalFrame = 0;
            this.drivenCurrentFrame = 0;
            this.drivenLastTick = -1;
        }

        private void ResetDrivenFrame()
        {
            this.drivenTotalFrame = 0;
            this.drivenCurrentFrame = 0;
            this.drivenLastTick = GetCurrentCoreTick();
        }

        private static int GetCurrentCoreTick()
        {
            return UnityEngine.Mathf.FloorToInt(UnityEngine.Time.timeSinceLevelLoad * resource.SPR.FPS);
        }

        ////////////////////////////////////////////////////////////////////////////////

        public void SetMapPosition(resource.map.Position _position) => this.position.SetMapPosition(_position);

        public void SetMapPosition(int _top, int _left) => this.position.SetMapPosition(_top, _left);

        public resource.map.Position GetMapPosition() => this.position.GetMapPosition();

        public UnityEngine.Vector3 GetScenePosition() => this.position.GetScenePosition();

        public UnityEngine.Vector3 GetCameraPosition(int z = -10) => this.position.GetCameraPosition(z);

        public int GetOrderInMap() => this.position.GetOrderInMap();

        ////////////////////////////////////////////////////////////////////////////////

        public void OnActionEnd(string _actionName, Func<object> _callback, int loopTimes = int.MaxValue) => this.callback.SetActionEnd(_actionName, _callback, loopTimes);

        ////////////////////////////////////////////////////////////////////////////////

        public void SetHealthPercent(int percent) => this.identify.SetHealthPercent(percent);

        public void SetName(string name) => this.identify.SetName(name);

        public void SetSeries(npcres.Identification.Series series) => this.identify.SetSeries(series);

        public void SetTong(string tongName, string tongTitle) => this.identify.SetTong(tongName, tongTitle);

        public void SetTitle(string title) => this.identify.SetTitle(title);

        public void SetCamp(npcres.Identification.Camp camp) => this.identify.SetCamp(camp);

        ////////////////////////////////////////////////////////////////////////////////

        public void ModifyAttrib(List<settings.skill.SkillSettingData.KMagicAttrib> magicAttribs)
        {
            foreach (settings.skill.SkillSettingData.KMagicAttrib index in magicAttribs)
            {
                npcres.AttribModify.ModifyAttrib(this, index);
            }
        }

        public void ModifyAttrib(settings.skill.SkillSettingData.KMagicAttrib[] pData, int nDataNum)
        {
            for (int i = 0; i < nDataNum; i++)
            {
                npcres.AttribModify.ModifyAttrib(this, pData[i]);
            }
        }

        ////////////////////////////////////////////////////////////////////////////////

        public void SetStateSkillEffect(skill.StateSetting.Data stateData, settings.skill.SkillSettingData.KMagicAttrib[] pData, int nDataNum, int nTime = -1)
            => this.state.Add(stateData, pData, nDataNum, nTime);

        public void SetStateSkillEffect(int stateId, settings.skill.SkillSettingData.KMagicAttrib[] pData, int nDataNum, int nTime = -1)
            => this.state.Add(stateId, pData, nDataNum, nTime);

        public void SetStateSkillEffect(int stateId)
            => this.state.Add(stateId, new skill.SkillSettingData.KMagicAttrib[] { }, 0, -1);

        public void SetImmediatelySkillEffect(settings.skill.SkillSettingData.KMagicAttrib[] pData, int nDataNum)
            => this.ModifyAttrib(pData, nDataNum);

        public void SetStateSpecialSpr(string sprPath) => this.state.SetSpecialSpr(sprPath);

        public void AddStateReceivedCriticalDamage(int damage) => this.state.AddTextStateCriticalDamage(damage);

        public void AddStateReceivedNormalDamage(int damage) => this.state.AddTextStateNormalDamage(damage);

        public void AddStateReceivedAppendDamage(int damage) => this.state.AppendTotalDamage(damage);

        public void AddStateTextEXP(int exp) => this.state.AddTextStateEXP(exp);

        public void AddStateTextHealth(int value) => this.state.AddTextStateHealth(value);

        public void StopAnimation(bool isStop) => this.notUpdate = isStop;

        ////////////////////////////////////////////////////////////////////////////////

        public void CalcDamage(settings.npcres.Controller nAttacker, int nMin, int nMax, npcres.Damage.DAMAGE_TYPE nType, bool bIsMelee, bool bDoHurt = true, bool bReturn = false, bool bDeaDly = false)
            => this.damage.CalcDamage(nAttacker, nMin, nMax, nType, bIsMelee, bDoHurt, bReturn, bDeaDly);

        public bool ReceiveDamage(settings.npcres.Controller nLauncher, bool bIsMelee, settings.skill.SkillSettingData.KMagicAttrib[] pData, bool bUseAR, bool bDoHurt, int nEnChance)
            => this.damage.ReceiveDamage(nLauncher, bIsMelee, pData, bUseAR, bDoHurt, nEnChance);
    }
}
