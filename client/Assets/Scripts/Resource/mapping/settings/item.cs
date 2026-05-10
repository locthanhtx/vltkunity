
namespace game.resource.mapping.settings
{
    struct Item
    {
        public const string directoryPath = "\\update10\\settings\\item\\004\\";

        public const string amulet = directoryPath + "Amulet.txt";
        public const string armor = directoryPath + "Armor.txt";
        public const string belt = directoryPath + "Belt.txt";
        public const string boot = directoryPath + "Boot.txt";
        public const string cuff = directoryPath + "Cuff.txt";
        public const string helm = directoryPath + "Helm.txt";
        public const string horse = directoryPath + "Horse.txt";
        public const string meleeweapon = directoryPath + "MeleeWeapon.txt";
        public const string pendant = directoryPath + "Pendant.txt";
        public const string pifeng = directoryPath + "Pifeng.txt";
        public const string rangeweapon = directoryPath + "rangeweapon.txt";
        public const string ring = directoryPath + "Ring.txt";
        public const string shipin = directoryPath + "Shipin.txt";
        public const string yinjian = directoryPath + "Yinjian.txt";

        public const string armorres = "\\settings\\item\\ArmorRes.txt";
        public const string helmres = "\\settings\\item\\HelmRes.txt";
        public const string horseres = "\\settings\\item\\HorseRes.txt";
        public const string meleeres = "\\settings\\item\\MeleeRes.txt";
        public const string rangeres = "\\settings\\item\\RangeRes.txt";

        public const string magicattrib = directoryPath + "magicattrib.txt";

        public const string mine = directoryPath + "mine.txt";
        public const string questKey = directoryPath + "questkey.txt";
        public const string fusion = directoryPath + "fusion.txt";
        public const string potion = directoryPath + "potion.txt";
        public const string medMaterialBase = directoryPath + "medmaterialbase.txt";
        public const string townPortal = directoryPath + "TownPortal.txt";
        public const string unique = directoryPath + "unique.txt";

        public const string goldEquip = directoryPath + "GoldItem.txt";
        public const string platinaEquip = directoryPath + "platinaequip.txt";
        public const string goldEquipRes = "\\settings\\item\\GolditemRes.txt";
        public const string magicattrib_ge = directoryPath + "GoldMagic.txt";
        public const string magicScript = directoryPath + "magicscript.txt";
        public const string mask = directoryPath + "Mask.txt";

        public struct HeaderIndexer
        {
            public enum Equipment
            {
                name,
                genre,
                detail,
                particular,
                imagePath,
                objIdx,
                width,
                height,
                intro,
                series,
                price,
                level,
                stack,
                propBasic0Type,
                propBasic0Min,
                propBasic0Max,
                propBasic1Type,
                propBasic1Min,
                propBasic1Max,
                propBasic2Type,
                propBasic2Min,
                propBasic2Max,
                propBasic3Type,
                propBasic3Min,
                propBasic3Max,
                propBasic4Type,
                propBasic4Min,
                propBasic4Max,
                propBasic5Type,
                propBasic5Min,
                propBasic5Max,
                propBasic6Type,
                propBasic6Min,
                propBasic6Max,
                requirement0Type,
                requirement0Para,
                requirement1Type,
                requirement1Para,
                requirement2Type,
                requirement2Para,
                requirement3Type,
                requirement3Para,
                requirement4Type,
                requirement4Para,
                requirement5Type,
                requirement5Para,
            }

            public enum GoldEquip
            {
                magic0 = HeaderIndexer.Equipment.requirement5Para + 1,
                magic1,
                magic2,
                magic3,
                magic4,
                magic5,
                idSet,
                set,
                setNum,
                upSet,
                setId,
                yinMagicAttribs0,
                yinMagicAttribs1,
                rongNum,
                wengangPin,
                binfujiazhi,
                chiBangRes,
            }

            public enum MagicAttrib
            {
                name,
                nextLevel,
                loadModel,
                pos,
                series,
                level,
                propKind,
                value0Min,
                value0Max,
                value1Min,
                value1Max,
                value2Min,
                value2Max,
                intro,
                dropRate0VuKhi,
                dropRate1AmKhi,
                dropRate2Ao,
                dropRate3Nhan,
                dropRate4DayChuyen,
                dropRate5Giay,
                dropRate6Dai,
                dropRate7Non,
                dropRate8BaoTay,
                dropRate9DayChuyen,
                dropRate10Ngua,
                dropRate11MatNa,
                dropRate12PhiPhong,
                dropRate13AnGiam,
                dropRate14TrangSuc,
            }

            public enum MagicAttribGoldEquip
            {
                name,
                prefix,
                series,
                faction,
                type,
                value1Min,
                value1Max,
                value2Min,
                value2Max,
                value3Min,
                value3Max,
                intro,
            }

            public enum GoldEquipRes
            {
                equipRowIndex,
                resId,
            }

            public enum MagicScript
            {
                name,
                genre,
                detail,
                particular,
                image,
                objectId,
                width,
                height,
                intro,
                series,
                price,
                columnL,
                stackAllowed,
                script,
                skillId,
                columnP,
                columnQ,
                columnR,
                columnS,
                target,
                stackValue,
                columnV,
                columnW,
                columnX,
                columnY,
                columnZ,
                columnAA,
                columnAB,
                columnAC,
                columnAD,
                columnAE,
            }

            public enum Appearance
            {
                declarationLine = 0,
                npcResLine,
                _count,
            }
        }
    }
}
