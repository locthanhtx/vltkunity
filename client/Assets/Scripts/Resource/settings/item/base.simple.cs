using System.Collections.Generic;

namespace game.resource.settings.item
{
    public enum SimpleItemKind
    {
        Medicine,
        Mine,
        Quest,
        Fusion,
        TownPortal,
        MedMaterial,
    }

    public class SimpleItemBase
    {
        public int rowIndex;
        public SimpleItemKind kind;
        public string name;
        public int genre;
        public int detail;
        public int particular;
        public string imagePath;
        public int objIdx;
        public int width = 1;
        public int height = 1;
        public string intro;
        public string script;
        public int series = -1;
        public int price;
        public int priceXu;
        public int level = 1;
        public int stack;
        public int useMap;
        public int useKind;
        public int isBang;
        public int isKuaiJie;
        public int isMagic;
        public int isUse;
        public int isShowValue;
        public int isSell;
        public int isTrade;
        public int isDrop;
        public int skillType;
        public int magicId;
        public int qualityPin;
        public int magicIndex;

        private List<settings.skill.SkillSettingData.KMagicAttrib> basicAttrib =
            new List<settings.skill.SkillSettingData.KMagicAttrib>();

        public void Load(resource.Table table, int tableRowIndex, SimpleItemKind itemKind)
        {
            this.rowIndex = tableRowIndex - 1;
            this.kind = itemKind;
            this.basicAttrib.Clear();

            switch (itemKind)
            {
                case SimpleItemKind.Medicine:
                    this.LoadMedicine(table, tableRowIndex);
                    break;

                case SimpleItemKind.Mine:
                    this.LoadMine(table, tableRowIndex);
                    break;

                case SimpleItemKind.Quest:
                    this.LoadQuest(table, tableRowIndex);
                    break;

                case SimpleItemKind.Fusion:
                    this.LoadFusion(table, tableRowIndex);
                    break;

                case SimpleItemKind.TownPortal:
                    this.LoadTownPortal(table, tableRowIndex);
                    break;

                case SimpleItemKind.MedMaterial:
                    this.LoadMedMaterial(table, tableRowIndex);
                    break;
            }
        }

        private void LoadMedicine(resource.Table table, int row)
        {
            this.LoadCommonGdpi(table, row);
            this.series = table.Get<int>(9, row, 0);
            this.price = table.Get<int>(10, row, 0);
            this.level = table.Get<int>(11, row, 1);
            this.stack = table.Get<int>(12, row, 0);
            this.isKuaiJie = table.Get<int>(13, row, 0);

            int column = 14;
            for (int index = 0; index < 6; index++, column += 3)
            {
                int type = table.Get<int>(column, row, 0);
                int value = table.Get<int>(column + 1, row, 0);
                int time = table.Get<int>(column + 2, row, 0);
                this.basicAttrib.Add(new settings.skill.SkillSettingData.KMagicAttrib(type, value, time, 0));
            }

            this.useMap = table.Get<int>(32, row, 0);
        }

        private void LoadMine(resource.Table table, int row)
        {
            this.LoadCommonGdpi(table, row);
            this.series = table.Get<int>(9, row, 0);
            this.price = table.Get<int>(10, row, 0);
            this.level = table.Get<int>(11, row, 1);
            this.stack = table.Get<int>(12, row, 0);
            this.script = table.Get<string>(13, row);
            this.magicId = table.Get<int>(14, row, 0);
            this.priceXu = table.Get<int>(20, row, 0);
            this.isKuaiJie = table.Get<int>(21, row, 0);
            this.isMagic = table.Get<int>(22, row, 0);
            this.isUse = table.Get<int>(23, row, 0);
            this.isShowValue = table.Get<int>(24, row, 0);
        }

        private void LoadQuest(resource.Table table, int row)
        {
            this.name = table.Get<string>(0, row);
            this.genre = table.Get<int>(1, row, (int)Defination.Genre.item_task);
            this.detail = table.Get<int>(2, row, 0);
            this.particular = 0;
            this.imagePath = table.Get<string>(3, row);
            this.objIdx = table.Get<int>(4, row, 0);
            this.width = table.Get<int>(5, row, 1);
            this.height = table.Get<int>(6, row, 1);
            this.script = table.Get<string>(7, row);
            this.intro = table.Get<string>(8, row);
            this.price = table.Get<int>(9, row, 0);
            this.priceXu = table.Get<int>(10, row, 0);
            this.isSell = table.Get<int>(12, row, 0);
            this.isTrade = table.Get<int>(13, row, 0);
            this.isDrop = table.Get<int>(14, row, 0);
            this.isKuaiJie = table.Get<int>(15, row, 0);
            this.skillType = table.Get<int>(16, row, 0);
            this.series = table.Get<int>(17, row, -1);
            this.isMagic = table.Get<int>(18, row, 0);
            this.level = table.Get<int>(19, row, 1);
            this.stack = table.Get<int>(20, row, 0);
            this.magicId = table.Get<int>(21, row, 0);
            this.isUse = table.Get<int>(22, row, 0);
        }

        private void LoadFusion(resource.Table table, int row)
        {
            this.LoadCommonGdpi(table, row);
            this.series = table.Get<int>(9, row, -1);
            this.price = table.Get<int>(10, row, 0);
            this.level = table.Get<int>(11, row, 1);
            this.stack = table.Get<int>(12, row, 0);
            this.qualityPin = table.Get<int>(13, row, 1);
            this.magicIndex = table.Get<int>(14, row, 0);
            this.isBang = table.Get<int>(21, row, 0);
            this.priceXu = table.Get<int>(22, row, 0);

            for (int index = 0; index < 6; index++)
            {
                int equipPart = table.Get<int>(15 + index, row, -1);
                this.basicAttrib.Add(new settings.skill.SkillSettingData.KMagicAttrib(0, equipPart, 0, 0));
            }
        }

        private void LoadTownPortal(resource.Table table, int row)
        {
            this.name = table.Get<string>(0, row);
            this.genre = table.Get<int>(1, row, (int)Defination.Genre.item_townportal);
            this.detail = 0;
            this.particular = 0;
            this.imagePath = table.Get<string>(2, row);
            this.objIdx = table.Get<int>(3, row, 0);
            this.width = table.Get<int>(4, row, 1);
            this.height = table.Get<int>(5, row, 1);
            this.price = table.Get<int>(6, row, 0);
            this.intro = table.Get<string>(7, row);
            this.series = -1;
            this.level = 1;
            this.stack = 0;
            this.isKuaiJie = 1;
        }

        private void LoadMedMaterial(resource.Table table, int row)
        {
            this.LoadCommonGdpi(table, row);
            this.series = table.Get<int>(9, row, 0);
            this.price = table.Get<int>(10, row, 0);
            this.level = table.Get<int>(11, row, 1);
            this.stack = table.Get<int>(12, row, 0);

            this.basicAttrib.Add(new settings.skill.SkillSettingData.KMagicAttrib(
                table.Get<int>(13, row, 0),
                table.Get<int>(14, row, 0)));
            this.basicAttrib.Add(new settings.skill.SkillSettingData.KMagicAttrib(
                table.Get<int>(15, row, 0),
                table.Get<int>(16, row, 0)));
            this.basicAttrib.Add(new settings.skill.SkillSettingData.KMagicAttrib(
                table.Get<int>(17, row, 0),
                table.Get<int>(18, row, 0)));
        }

        private void LoadCommonGdpi(resource.Table table, int row)
        {
            this.name = table.Get<string>(0, row);
            this.genre = table.Get<int>(1, row, 0);
            this.detail = table.Get<int>(2, row, 0);
            this.particular = table.Get<int>(3, row, 0);
            this.imagePath = table.Get<string>(4, row);
            this.objIdx = table.Get<int>(5, row, 0);
            this.width = table.Get<int>(6, row, 1);
            this.height = table.Get<int>(7, row, 1);
            this.intro = table.Get<string>(8, row);
        }

        public string GetKeyGDP()
        {
            return MakeKeyGDP(this.genre, this.detail, this.particular);
        }

        public string GetKeyGDPL()
        {
            return MakeKeyGDPL(this.genre, this.detail, this.particular, this.level);
        }

        public string GetRowKey()
        {
            return MakeRowKey(this.genre, this.rowIndex);
        }

        public List<settings.skill.SkillSettingData.KMagicAttrib> GetBasicAttrib()
        {
            return this.basicAttrib;
        }

        public static string MakeKeyGDP(int genre, int detail, int particular)
        {
            return genre + ", " + detail + ", " + particular;
        }

        public static string MakeKeyGDPL(int genre, int detail, int particular, int level)
        {
            return MakeKeyGDP(genre, detail, particular) + ", " + level;
        }

        public static string MakeRowKey(int genre, int rowIndex)
        {
            return genre + ", " + rowIndex;
        }
    }
}
