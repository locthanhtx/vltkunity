
using System.Collections.Generic;
using Unity.VisualScripting;
using static game.resource.SPR;

namespace game.resource.settings.npcres.normal
{
    public class Animate
    {
        private readonly npcres.Identification identification;
        private readonly npcres.State state;

        private int declareLine;
        private string npcResType;
        private string actionName;
        private int direction;

        private readonly Dictionary<string, npcres.Structures.PartAnimation> partAnimations;

        public Animate(npcres.Identification identification, npcres.State state)
        {
            this.identification = identification;
            this.state = state;

            this.declareLine = 45;
            this.npcResType = "ani063";
            this.actionName = mapping.settings.NpcRes.WeaponAction.normalStand1;
            this.direction = 1;

            this.partAnimations = new();

            int npcStature = resource.settings.Npcs.GetNpcStature(this.declareLine);

            this.identification.SetNpcPate(npcStature);
            this.state.SetNpcPateType2(npcStature);
            this.ResetAllPartAnimation();
        }

        //public Animate(string npcResType, string actionName = mapping.settings.NpcRes.WeaponAction.normalStand1, int direction = 1)
        //{
        //    this.npcResType = npcResType;
        //    this.actionName = actionName;
        //    this.direction = direction;

        //    this.partAnimations = new();
        //    this.ResetAllPartAnimation();
        //}

        public Animate(npcres.Identification identification, npcres.State state, int npcDeclareLine, string actionName = mapping.settings.NpcRes.WeaponAction.normalStand1, int direction = 1)
        {
            this.identification = identification;
            this.state = state;

            this.declareLine = npcDeclareLine;
            this.npcResType = resource.settings.Npcs.GetNpcResType(npcDeclareLine);
            this.actionName = actionName;
            this.direction = direction;

            this.partAnimations = new();

            int npcStature = resource.settings.Npcs.GetNpcStature(this.declareLine);

            this.identification.SetNpcPate(npcStature);
            this.state.SetNpcPateType2(npcStature);
            this.ResetAllPartAnimation();
        }

        private Dictionary<string, npcres.Structures.PartAnimation> ResetAllPartAnimation()
        {
            this.partAnimations.Clear();
            this.partAnimations.AddRange(npcres.normal.Getters.FullPartAnimation(this.npcResType, this.actionName, this.direction));

            return this.partAnimations;
        }

        public Dictionary<string, npcres.Structures.PartAnimation> GetPartAnimation() => this.partAnimations;
        public int GetDeclareLine() => this.declareLine;
        public string GetResType() => this.npcResType;
        public string GetActionName() => this.actionName;
        public int GetDirection() => this.direction;

        public Dictionary<string, npcres.Structures.PartAnimation> SetNpcResType(string _npcResType)
        {
            if (this.npcResType.CompareTo(_npcResType) == 0)
            {
                return new();
            }

            this.npcResType = _npcResType;
            return this.ResetAllPartAnimation();
        }

        public Dictionary<string, npcres.Structures.PartAnimation> SetNpcDeclareLine(int _declareLine)
        {
            if (this.declareLine == _declareLine)
            {
                return new();
            }

            this.declareLine = _declareLine;

            int npcStature = resource.settings.Npcs.GetNpcStature(this.declareLine);

            this.identification.SetNpcPate(npcStature);
            this.state.SetNpcPateType2(npcStature);

            return this.SetNpcResType(resource.settings.Npcs.GetNpcResType(_declareLine));
        }

        public Dictionary<string, npcres.Structures.PartAnimation> SetAction(string _actionName)
        {
            if (this.actionName.CompareTo(_actionName) == 0)
            {
                return new();
            }

            this.actionName = _actionName;
            return this.ResetAllPartAnimation();
        }

        public string GetAction() => this.actionName;

        public Dictionary<string, npcres.Structures.PartAnimation> SetDirection(int _direction)
        {
            if (this.direction == _direction)
            {
                return new();
            }

            this.direction = _direction;
            return this.ResetAllPartAnimation();
        }

        public Dictionary<string, npcres.Structures.PartAnimation> SyncDirection(int _direction)
        {
            resource.Cache.Settings.NpcRes.NormalNpc.PartInfo allPartInfo = resource.Cache.Settings.NpcRes.NormalNpc.animationMapping[this.npcResType][this.actionName];
            if (allPartInfo.fullBody.directionCount <= 0)
            {
                return new();
            }

            var nTempDir = (_direction + (32 / allPartInfo.fullBody.directionCount)) / (64 / allPartInfo.fullBody.directionCount);
            if (nTempDir >= allPartInfo.fullBody.directionCount)
                nTempDir -= allPartInfo.fullBody.directionCount;

            _direction = ++nTempDir;

            if (this.direction == _direction)
            {
                return new();
            }

            this.direction = _direction;
            return this.ResetAllPartAnimation();
        }
    }
}
