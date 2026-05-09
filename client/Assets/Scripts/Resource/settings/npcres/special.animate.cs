
using System.Collections.Generic;
using Unity.VisualScripting;

namespace game.resource.settings.npcres.special
{
    public class Animate
    {
        public struct PartGroupRowIndex
        {
            public int head;
            public int body;
            public int weapon;
            public int horse;
        };

        private readonly npcres.Identification identification;
        private readonly npcres.State state;

        private string characterType;
        private int direction;
        private bool riding;
        private string action;
        private string animation;
        private int playerSpeed;

        private special.Animate.PartGroupRowIndex partGroupRowIndex;
        private readonly Dictionary<string, npcres.Structures.PartAnimation> partAnimations;

        public Animate(
            npcres.Identification identification,
            npcres.State state,
            string characterType = NpcRes.SpecialType.man,
            int direction = 1,
            string action = NpcRes.Action.normalStand1,
            int headIndex = 19,
            int bodyIndex = 19,
            int weaponIndex = 20,
            int horseIndex = -1
        )
        {
            this.partGroupRowIndex.head = headIndex;
            this.partGroupRowIndex.body = bodyIndex;
            this.partGroupRowIndex.weapon = weaponIndex;
            this.partGroupRowIndex.horse = horseIndex;

            this.characterType = characterType;
            this.direction = direction;
            this.riding = horseIndex >= 0;
            this.action = action;

            this.playerSpeed = resource.SPR.FPS;

            this.animation = npcres.special.Getters.AnimationName(this.characterType, this.riding, this.action, this.partGroupRowIndex.weapon);

            this.partAnimations = new();

            this.identification = identification;
            this.identification.SetNpcPate(this.GetNpcPate());

            this.state = state;
            this.state.SetNpcPate(this.GetNpcPate() - this.GetNpcPateBase());
            this.state.SetNpcPateType2(this.GetNpcPate());

            this.ResetAllPartAnimation(_newAnimation: this.animation);
        }


        private Dictionary<string, npcres.Structures.PartAnimation> ResetAllPartAnimation(string _newAnimation = "")
        {
            if (_newAnimation.CompareTo("") == 0)
            {
                this.animation = npcres.special.Getters.AnimationName(this.characterType, this.riding, this.action, this.partGroupRowIndex.weapon);
            }
            else
            {
                this.animation = _newAnimation;
            }

            Dictionary<string, npcres.Structures.PartAnimation> headGroup = npcres.special.Getters.PartGroupAnimation(this.characterType, this.animation, NpcRes.PartGroup.head, this.direction, this.partGroupRowIndex.head, this.playerSpeed);
            Dictionary<string, npcres.Structures.PartAnimation> bodyGroup = npcres.special.Getters.PartGroupAnimation(this.characterType, this.animation, NpcRes.PartGroup.body, this.direction, this.partGroupRowIndex.body, this.playerSpeed);
            Dictionary<string, npcres.Structures.PartAnimation> weaponGroup = npcres.special.Getters.PartGroupAnimation(this.characterType, this.animation, NpcRes.PartGroup.weapon, this.direction, this.partGroupRowIndex.weapon, this.playerSpeed);

            Dictionary<string, npcres.Structures.PartAnimation> horseGroup;

            if (this.riding == true && this.partGroupRowIndex.horse >= 0)
            {
                horseGroup = npcres.special.Getters.PartGroupAnimation(this.characterType, this.animation, NpcRes.PartGroup.horse, this.direction, this.partGroupRowIndex.horse, this.playerSpeed);
            }
            else
            {
                horseGroup = new();
                List<string> partGrouping = npcres.special.Getters.PartGroup(this.characterType, NpcRes.PartGroup.horse);

                foreach (string part in partGrouping)
                {
                    horseGroup[part] = new npcres.Structures.PartAnimation();
                }
            }

            this.partAnimations.Clear();
            this.partAnimations.AddRange(headGroup);
            this.partAnimations.AddRange(bodyGroup);
            this.partAnimations.AddRange(weaponGroup);
            this.partAnimations.AddRange(horseGroup);
            this.partAnimations.Add(mapping.settings.NpcRes.Shadow.partName, npcres.special.Getters.ShadowAnimation(this.characterType, this.animation, this.direction, this.playerSpeed));

            return this.partAnimations;
        }

        public Dictionary<string, npcres.Structures.PartAnimation> GetPartAnimation() => this.partAnimations;
        public special.Animate.PartGroupRowIndex GetPartGroupRowIndex() => this.partGroupRowIndex;

        public int GetNpcPateBase()
        {
            int result = 84;

            if (this.action.CompareTo(settings.NpcRes.Action.sitDown) == 0)
            {
                result = 56;
            }

            return result;
        }

        public int GetNpcPate()
        {
            int result = this.GetNpcPateBase();

            if (this.riding)
            {
                result += 38;
                //result += 21;
            }

            return result;
        }

        public Dictionary<string, npcres.Structures.PartAnimation> SetCharacterType(string _type)
        {
            if (this.characterType.CompareTo(_type) == 0) return new();
            else this.characterType = _type;
            return ResetAllPartAnimation();
        }

        public Dictionary<string, npcres.Structures.PartAnimation> SetAction(string _action)
        {
            if (this.action.CompareTo(_action) == 0) return new();
            else this.action = _action;
            return ResetAllPartAnimation();
        }

        public string GetAction() => this.action;

        public Dictionary<string, npcres.Structures.PartAnimation> SetDirection(int _direction)
        {
            if (this.direction == _direction) return new();
            else this.direction = _direction;
            return ResetAllPartAnimation();
        }

        public Dictionary<string, npcres.Structures.PartAnimation> SyncDirection(int _direction)
        {
            settings.npcres.Structures.PartSprInfo partSprInfo = special.Getters.PartSprInfo(this.characterType, NpcRes.PartGroup.body, this.animation, this.partGroupRowIndex.body);
            if (partSprInfo.directionCount <= 0)
            {
                return new();
            }

            _direction = (_direction + (32 / partSprInfo.directionCount)) / (64 / partSprInfo.directionCount);
            if (_direction >= partSprInfo.directionCount)
            {
                _direction -= partSprInfo.directionCount;
            }
            _direction++;

            if (this.direction == _direction) return new();
            else this.direction = _direction;
            return ResetAllPartAnimation();
        }

        public int GetDirection() => this.direction;

        public Dictionary<string, npcres.Structures.PartAnimation> SetRiding(bool _riding)
        {
            if (this.riding == _riding) return new();
            else this.riding = _riding;

            if (this.riding == true)
            {
                if (this.partGroupRowIndex.horse < 0)
                {
                    this.riding = false;
                    return new();
                }
            }

            this.identification.SetNpcPate(this.GetNpcPate());
            this.state.SetNpcPate(this.GetNpcPate() - this.GetNpcPateBase());
            this.state.SetNpcPateType2(this.GetNpcPate());

            return ResetAllPartAnimation();
        }

        public Dictionary<string, npcres.Structures.PartAnimation> SetHeadRes(int _headResId)
        {
            if (_headResId < 0) return new();
            if (this.partGroupRowIndex.head == _headResId) return new();

            this.partGroupRowIndex.head = _headResId;
            Dictionary<string, npcres.Structures.PartAnimation> headGroup = npcres.special.Getters.PartGroupAnimation(this.characterType, this.animation, NpcRes.PartGroup.head, this.direction, this.partGroupRowIndex.head, this.playerSpeed);

            foreach (KeyValuePair<string, npcres.Structures.PartAnimation> pair in headGroup)
            {
                this.partAnimations[pair.Key] = pair.Value;
            }

            return headGroup;
        }

        public Dictionary<string, npcres.Structures.PartAnimation> SetHeadItemLine(int _line)
        {
            return this.SetHeadRes(item.Getters.Appearance.HelmLine(_line) - 1);
        }

        public Dictionary<string, npcres.Structures.PartAnimation> SetBodyRes(int resId)
        {
            if (resId < 0) return new();
            if (this.partGroupRowIndex.body == resId) return new();

            this.partGroupRowIndex.body = resId;
            Dictionary<string, npcres.Structures.PartAnimation> bodyGroup = npcres.special.Getters.PartGroupAnimation(this.characterType, this.animation, NpcRes.PartGroup.body, this.direction, this.partGroupRowIndex.body, this.playerSpeed);

            foreach (KeyValuePair<string, npcres.Structures.PartAnimation> pair in bodyGroup)
            {
                this.partAnimations[pair.Key] = pair.Value;
            }

            return bodyGroup;
        }

        public Dictionary<string, npcres.Structures.PartAnimation> SetBodyItemLine(int _line)
        {
            return this.SetBodyRes(item.Getters.Appearance.ArmorLine(_line) - 1);
        }

        public Dictionary<string, npcres.Structures.PartAnimation> SetWeaponRes(int resId)
        {
            if (resId < 0) return new();
            if (this.partGroupRowIndex.weapon == resId) return new();

            this.partGroupRowIndex.weapon = resId;

            string newWeaponAnimation = npcres.special.Getters.AnimationName(this.characterType, this.riding, this.action, this.partGroupRowIndex.weapon);

            if (newWeaponAnimation.CompareTo(this.animation) != 0)
            {
                return this.ResetAllPartAnimation(_newAnimation: newWeaponAnimation);
            }
            else
            {
                Dictionary<string, npcres.Structures.PartAnimation> handGroup = npcres.special.Getters.PartGroupAnimation(this.characterType, this.animation, NpcRes.PartGroup.weapon, this.direction, this.partGroupRowIndex.weapon, this.playerSpeed);

                foreach (KeyValuePair<string, npcres.Structures.PartAnimation> pair in handGroup)
                {
                    this.partAnimations[pair.Key] = pair.Value;
                }

                return handGroup;
            }
        }

        public Dictionary<string, npcres.Structures.PartAnimation> SetWeaponItemLine(int _line)
        {
            return this.SetWeaponRes(item.Getters.Appearance.MeleWeaponLine(_line) - 1);
        }

        public Dictionary<string, npcres.Structures.PartAnimation> SetHorseRes(int _horseResId)
        {
            if (_horseResId < 0) return new();
            if (this.partGroupRowIndex.horse == _horseResId) return new();

            this.partGroupRowIndex.horse = _horseResId;

            if (this.riding == false)
            {
                return this.SetRiding(true);
            }
            else
            {
                Dictionary<string, npcres.Structures.PartAnimation> horseGroup = npcres.special.Getters.PartGroupAnimation(this.characterType, this.animation, NpcRes.PartGroup.horse, this.direction, this.partGroupRowIndex.horse, this.playerSpeed);

                foreach (KeyValuePair<string, npcres.Structures.PartAnimation> pair in horseGroup)
                {
                    this.partAnimations[pair.Key] = pair.Value;
                }

                return horseGroup;
            }
        }

        public Dictionary<string, npcres.Structures.PartAnimation> SetHorseItemLine(int _line)
        {
            return this.SetHorseRes(item.Getters.Appearance.HorseLine(_line) - 1);
        }

        public Dictionary<string, npcres.Structures.PartAnimation> SetHeadItemDefault() => this.SetHeadItemLine(1);

        public Dictionary<string, npcres.Structures.PartAnimation> SetBodyItemDefault() => this.SetBodyItemLine(1);

        public Dictionary<string, npcres.Structures.PartAnimation> SetHandItemDefault() => this.SetWeaponItemLine(1);

        public Dictionary<string, npcres.Structures.PartAnimation> SetHorseItemDefault()
        {
            this.partGroupRowIndex.horse = -1;
            this.riding = false;

            return this.ResetAllPartAnimation();
        }

        public Dictionary<string, npcres.Structures.PartAnimation> SetSpeed(int speed)
        {
            if (this.playerSpeed == speed)
            {
                return new();
            }

            this.playerSpeed = speed;
            return this.ResetAllPartAnimation();
        }


    }
}
