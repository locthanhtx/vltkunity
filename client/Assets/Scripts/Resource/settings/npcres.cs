
namespace game.resource.settings
{
    public class NpcRes
    {
        public static void Initialize()
        {
            new settings.npcres.Initialize();
        }

        ////////////////////////////////////////////////////////////////////////////////

        public struct Part
        {
            public const string shadow = mapping.settings.NpcRes.Shadow.partName;
            public const string head = mapping.settings.NpcRes.Kind.Header.head;
            public const string hair = mapping.settings.NpcRes.Kind.Header.hair;
            public const string shoulder = mapping.settings.NpcRes.Kind.Header.shoulder;
            public const string body = mapping.settings.NpcRes.Kind.Header.body;
            public const string leftHand = mapping.settings.NpcRes.Kind.Header.leftHand;
            public const string rightHand = mapping.settings.NpcRes.Kind.Header.rightHand;
            public const string leftWeapon = mapping.settings.NpcRes.Kind.Header.leftWeapon;
            public const string rightWeapon = mapping.settings.NpcRes.Kind.Header.rightWeapon;
            public const string horseFront = mapping.settings.NpcRes.Kind.Header.horseFront;
            public const string horseMiddle = mapping.settings.NpcRes.Kind.Header.horseMiddle;
            public const string horseBack = mapping.settings.NpcRes.Kind.Header.horseBack;
            public const string mantle = mapping.settings.NpcRes.Kind.Header.mantle;
        }

        public struct PartGroup
        {
            public const string head = mapping.settings.NpcRes.Kind.Header.head;
            public const string body = mapping.settings.NpcRes.Kind.Header.body;
            public const string weapon = mapping.settings.NpcRes.Kind.Header.rightWeapon;
            public const string horse = mapping.settings.NpcRes.Kind.Header.horseFront;
        }

        public struct Action
        {
            public const string fightStand = mapping.settings.NpcRes.WeaponAction.fightStand;
            public const string normalStand1 = mapping.settings.NpcRes.WeaponAction.normalStand1;
            public const string normalStand2 = mapping.settings.NpcRes.WeaponAction.normalStand2;
            public const string fightWalk = mapping.settings.NpcRes.WeaponAction.fightWalk;
            public const string normalWalk = mapping.settings.NpcRes.WeaponAction.normalWalk;
            public const string fightRun = mapping.settings.NpcRes.WeaponAction.fightRun;
            public const string normalRun = mapping.settings.NpcRes.WeaponAction.normalRun;
            public const string wound = mapping.settings.NpcRes.WeaponAction.wound;
            public const string die = mapping.settings.NpcRes.WeaponAction.die;
            public const string attack1 = mapping.settings.NpcRes.WeaponAction.attack1;
            public const string attack2 = mapping.settings.NpcRes.WeaponAction.attack2;
            public const string magic = mapping.settings.NpcRes.WeaponAction.magic;
            public const string sitDown = mapping.settings.NpcRes.WeaponAction.sitDown;
            public const string junpFly = mapping.settings.NpcRes.WeaponAction.junpFly;
        }

        public struct SpecialType
        {
            public const string man = mapping.settings.NpcRes.Kind.CharacterName.mainMan;
            public const string lady = mapping.settings.NpcRes.Kind.CharacterName.mainLady;
        }

        ////////////////////////////////////////////////////////////////////////////////

        public class Special : npcres.Controller
        {
            public Special(string characterType = NpcRes.SpecialType.man, int direction = 1, string action = NpcRes.Action.normalStand1, int headIndex = 19, int bodyIndex = 19, int weaponIndex = 20, int horseIndex = -1) : base(typeof(game.resource.settings.NpcRes.Special).FullName)
            {
                this.initIsSpecial = true;
                this.InitForSpecial(characterType, direction, action, headIndex, bodyIndex, weaponIndex, horseIndex);
            }

            public void SetCharacterType(string _characterTypeName)
            {
                settings.Npcs.ApplyPlayerMotionProfile(this.data, _characterTypeName);
                this.shape.InValidPartList(this.animate.special.SetCharacterType(_characterTypeName));
            }

            public void SetRiding(bool _riding) => this.shape.InValidPartList(this.animate.special.SetRiding(_riding));

            public void SetHeadItemLine(int _headItemLine) => this.shape.InValidPartList(this.animate.special.SetHeadItemLine(_headItemLine));
            public void SetHeadRes(int _headResId) => this.shape.InValidPartList(this.animate.special.SetHeadRes(_headResId));

            public void SetBodyItemLine(int _bodyItemLine) => this.shape.InValidPartList(this.animate.special.SetBodyItemLine(_bodyItemLine));
            public void SetBodyRes(int _bodyResId) => this.shape.InValidPartList(this.animate.special.SetBodyRes(_bodyResId));

            public void SetWeaponItemLine(int _weaponItemLine) => this.shape.InValidPartList(this.animate.special.SetWeaponItemLine(_weaponItemLine));
            public void SetWeaponRes(int _weaponResId) => this.shape.InValidPartList(this.animate.special.SetWeaponRes(_weaponResId));

            public void SetHorseItemLine(int _horseItemLine) => this.shape.InValidPartList(this.animate.special.SetHorseItemLine(_horseItemLine));
            public void SetHorseRes(int _horseResId) => this.shape.InValidPartList(this.animate.special.SetHorseRes(_horseResId));

            public void SetHeadItemDefault() => this.shape.InValidPartList(this.animate.special.SetHeadItemDefault());
            public void SetBodyItemDefault() => this.shape.InValidPartList(this.animate.special.SetBodyItemDefault());
            public void SetWeaponItemDefault() => this.shape.InValidPartList(this.animate.special.SetHandItemDefault());
            public void SetHorseItemDefault() => this.shape.InValidPartList(this.animate.special.SetHorseItemDefault());

            public void SetSpeed(int _speed) => this.shape.InValidPartList(this.animate.special.SetSpeed(_speed));

            ////////////////////////////////////////////////////////////////////////////////

            private npcres.special.Animate.PartGroupRowIndex previousRes;

            public void BecomeNpc(int _declareLine)
            {
                if(this.animate.isSpecialNpc)
                {
                    this.previousRes = this.animate.special.GetPartGroupRowIndex();
                    this.shape.InValidPartList(this.animate.GetPartAnimation(), deactivate: true);
                }

                this.InitForNormal();
                this.shape.InValidPartList(this.animate.normal.SetNpcDeclareLine(_declareLine));
            }

            public void RestoreSpecialNpc()
            {
                this.animate.isSpecialNpc = true;

                this.SetHeadRes(this.previousRes.head);
                this.SetBodyRes(this.previousRes.body);
                this.SetWeaponRes(this.previousRes.weapon);
                this.SetHorseRes(this.previousRes.horse);
            }
        }

        public class Normal : npcres.Controller
        {
            public Normal() : base(typeof(game.resource.settings.NpcRes.Normal).FullName)
            {
                this.initIsSpecial = false;
                this.InitForNormal();
            }

            public Normal(int npcDeclareLine, string actionName = mapping.settings.NpcRes.WeaponAction.normalStand1, int direction = 1) : base(typeof(game.resource.settings.NpcRes.Normal).FullName)
            {
                this.initIsSpecial = false;
                this.InitForNormal(npcDeclareLine, actionName, direction);
            }

            public void SetNpcDeclareLine(int _declareLine)
            {
                settings.Npcs.ApplyNpcMotionProfile(this.data, _declareLine);
                this.shape.InValidPartList(this.animate.normal.SetNpcDeclareLine(_declareLine));
            }
        }
    }
}
