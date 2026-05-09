
namespace game.resource.mapping.settings
{
    struct Item
    {
        public const string directoryPath = "\\settings\\item\\004\\";

        public const string amulet = directoryPath + "amulet.txt";
        public const string armor = directoryPath + "armor.txt";
        public const string belt = directoryPath + "belt.txt";
        public const string boot = directoryPath + "boot.txt";
        public const string cuff = directoryPath + "cuff.txt";
        public const string helm = directoryPath + "helm.txt";
        public const string horse = directoryPath + "horse.txt";
        public const string meleeweapon = directoryPath + "meleeweapon.txt";
        public const string pendant = directoryPath + "pendant.txt";
        public const string rangeweapon = directoryPath + "rangeweapon.txt";
        public const string ring = directoryPath + "ring.txt";

        public const string armorres = directoryPath + "armorres.txt";
        public const string helmres = directoryPath + "helmres.txt";
        public const string horseres = directoryPath + "horseres.txt";
        public const string meleeres = directoryPath + "meleeres.txt";
        public const string rangeres = directoryPath + "rangeres.txt";

        public const string magicattrib = directoryPath + "magicattrib.txt";

        public const string goldEquip = directoryPath + "goldequip.txt";
        public const string goldEquipRes = "\\settings\\item\\GolditemRes.txt";
        public const string magicattrib_ge = directoryPath + "magicattrib_ge.txt";
        public const string magicScript = directoryPath + "magicscript.txt";
        public const string mask = directoryPath + "mask.txt";

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
