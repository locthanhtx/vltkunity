
namespace game.resource.mapping.settings
{
    struct NpcRes
    {
        public const string directoryPath = "\\settings\\npcres\\";
        public const string stateMagicTable = "\\settings\\npcres\\StatusGraphicMapping.txt";

        public struct Kind
        {
            public const string filePath = NpcRes.directoryPath + "CharacterType.txt";

            public struct Header
            {
                public const string characterName = "CharacterName";
                public const string characterType = "CharacterType";
                public const string resFilePath = "ResFilePath";
                public const string partFileName = "PartFileName";
                public const string weaponActionTab1 = "WeaponActionTab1";
                public const string weaponActionTab2 = "WeaponActionTab2";
                public const string actionRenderOrderTab = "ActionRenderOrderTab";
                public const string head = "Head";
                public const string hair = "Hair";
                public const string shoulder = "Shoulder";
                public const string body = "Body";
                public const string leftHand = "LeftHand";
                public const string rightHand = "RightHand";
                public const string leftWeapon = "LeftWeapon";
                public const string rightWeapon = "RightWeapon";
                public const string horseFront = "HorseFront";
                public const string horseMiddle = "HorseMiddle";
                public const string horseBack = "HorseBack";
                public const string mantle = "Mantle";
            }

            public struct CharacterName
            {
                public const string mainMan = "MainMan";
                public const string mainLady = "MainLady";
            }
        }

        public struct Properties
        {
            public const string sprPropertiesSuffix = "ÐÅÏ¢";
            public const string tabFileExtension = ".txt";
            public const string sprFolderPrefix = "\\spr\\npcres\\";
            public const string sprFileExtension = ".spr";
        }

        public struct WeaponAction
        {
            public const string fightStand = "FightStand";
            public const string normalStand1 = "NormalStand1";
            public const string normalStand2 = "NormalStand2";
            public const string fightWalk = "FightWalk";
            public const string normalWalk = "NormalWalk";
            public const string fightRun = "FightRun";
            public const string normalRun = "NormalRun";
            public const string wound = "Wound";
            public const string die = "Die";
            public const string attack1 = "Attack1";
            public const string attack2 = "Attack2";
            public const string magic = "Magic";
            public const string sitDown = "SitDown";
            public const string junpFly = "JunpFly";
        }

        public struct Shadow
        {
            public const string partName = "Shadow";
            public const string filePath = NpcRes.directoryPath + "Ö÷½Ç¶¯×÷ÒõÓ°¶ÔÓ¦±í.txt";
        }

        public struct ActionRenderOrderTab
        {
            public struct Section
            {
                public const string Default = "DEFAULT";
            }

            public struct Key
            {
                public const string prefix = "Dir";
            }
        }

        public struct SprPropertiesIndexer
        {
            public const int frameCount = 0;
            public const int directionCount = 1;
            public const int intervalRatio = 2;
        }

        public struct NormalNpc
        {
            public struct Header
            {
                public const string npcList = "NpcList";
            }

            public const string sprActionPath = NpcRes.directoryPath + "NormalNPCResources.txt";
            public const string sprPropertiesPath = NpcRes.directoryPath + "CommonNPCResourceInfo.txt";
            public const string soundPath = NpcRes.directoryPath + "npc¶¯×÷ÉùÒô±í.txt";
            public const string shadowSuffix = "b";
        }
    }
}
