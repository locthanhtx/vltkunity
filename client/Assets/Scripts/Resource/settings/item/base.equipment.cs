
using System.Collections.Generic;

namespace game.resource.settings.item
{
    public class EquipmentBase
    {
        public int rowIndex;
        public string name;
        public int genre;
        public int detail;
        public int particular;
        public string imagePath;
        public int objIdx;
        public int width;
        public int height;
        public string intro;
        public int series;
        public int price;
        public int level;
        public int stack;
        public int propBasic0Type;
        public int propBasic0Min;
        public int propBasic0Max;
        public int propBasic1Type;
        public int propBasic1Min;
        public int propBasic1Max;
        public int propBasic2Type;
        public int propBasic2Min;
        public int propBasic2Max;
        public int propBasic3Type;
        public int propBasic3Min;
        public int propBasic3Max;
        public int propBasic4Type;
        public int propBasic4Min;
        public int propBasic4Max;
        public int propBasic5Type;
        public int propBasic5Min;
        public int propBasic5Max;
        public int propBasic6Type;
        public int propBasic6Min;
        public int propBasic6Max;
        public int requirement0Type;
        public int requirement0Para;
        public int requirement1Type;
        public int requirement1Para;
        public int requirement2Type;
        public int requirement2Para;
        public int requirement3Type;
        public int requirement3Para;
        public int requirement4Type;
        public int requirement4Para;
        public int requirement5Type;
        public int requirement5Para;

        ///////////////////////////////////////////////////////////////////////////

        private List<settings.skill.SkillSettingData.KMagicAttrib> baseAttrib;
        private List<settings.skill.SkillSettingData.KMagicAttrib> requiredAttrib;

        ///////////////////////////////////////////////////////////////////////////

        public void Load(resource.Table table, int rowIndex)
        {
            this.rowIndex = rowIndex;
            this.name = table.Get<string>((int)resource.mapping.settings.Item.HeaderIndexer.Equipment.name, rowIndex);
            this.genre = table.Get<int>((int)resource.mapping.settings.Item.HeaderIndexer.Equipment.genre, rowIndex);
            this.detail = table.Get<int>((int)resource.mapping.settings.Item.HeaderIndexer.Equipment.detail, rowIndex);
            this.particular = table.Get<int>((int)resource.mapping.settings.Item.HeaderIndexer.Equipment.particular, rowIndex);
            this.imagePath = table.Get<string>((int)resource.mapping.settings.Item.HeaderIndexer.Equipment.imagePath, rowIndex);
            this.objIdx = table.Get<int>((int)resource.mapping.settings.Item.HeaderIndexer.Equipment.objIdx, rowIndex);
            this.width = table.Get<int>((int)resource.mapping.settings.Item.HeaderIndexer.Equipment.width, rowIndex);
            this.height = table.Get<int>((int)resource.mapping.settings.Item.HeaderIndexer.Equipment.height, rowIndex);
            this.intro = table.Get<string>((int)resource.mapping.settings.Item.HeaderIndexer.Equipment.intro, rowIndex);
            this.series = table.Get<int>((int)resource.mapping.settings.Item.HeaderIndexer.Equipment.series, rowIndex);
            this.price = table.Get<int>((int)resource.mapping.settings.Item.HeaderIndexer.Equipment.price, rowIndex);
            this.level = table.Get<int>((int)resource.mapping.settings.Item.HeaderIndexer.Equipment.level, rowIndex);
            this.stack = table.Get<int>((int)resource.mapping.settings.Item.HeaderIndexer.Equipment.stack, rowIndex);
            this.propBasic0Type = table.Get<int>((int)resource.mapping.settings.Item.HeaderIndexer.Equipment.propBasic0Type, rowIndex);
            this.propBasic0Min = table.Get<int>((int)resource.mapping.settings.Item.HeaderIndexer.Equipment.propBasic0Min, rowIndex);
            this.propBasic0Max = table.Get<int>((int)resource.mapping.settings.Item.HeaderIndexer.Equipment.propBasic0Max, rowIndex);
            this.propBasic1Type = table.Get<int>((int)resource.mapping.settings.Item.HeaderIndexer.Equipment.propBasic1Type, rowIndex);
            this.propBasic1Min = table.Get<int>((int)resource.mapping.settings.Item.HeaderIndexer.Equipment.propBasic1Min, rowIndex);
            this.propBasic1Max = table.Get<int>((int)resource.mapping.settings.Item.HeaderIndexer.Equipment.propBasic1Max, rowIndex);
            this.propBasic2Type = table.Get<int>((int)resource.mapping.settings.Item.HeaderIndexer.Equipment.propBasic2Type, rowIndex);
            this.propBasic2Min = table.Get<int>((int)resource.mapping.settings.Item.HeaderIndexer.Equipment.propBasic2Min, rowIndex);
            this.propBasic2Max = table.Get<int>((int)resource.mapping.settings.Item.HeaderIndexer.Equipment.propBasic2Max, rowIndex);
            this.propBasic3Type = table.Get<int>((int)resource.mapping.settings.Item.HeaderIndexer.Equipment.propBasic3Type, rowIndex);
            this.propBasic3Min = table.Get<int>((int)resource.mapping.settings.Item.HeaderIndexer.Equipment.propBasic3Min, rowIndex);
            this.propBasic3Max = table.Get<int>((int)resource.mapping.settings.Item.HeaderIndexer.Equipment.propBasic3Max, rowIndex);
            this.propBasic4Type = table.Get<int>((int)resource.mapping.settings.Item.HeaderIndexer.Equipment.propBasic4Type, rowIndex);
            this.propBasic4Min = table.Get<int>((int)resource.mapping.settings.Item.HeaderIndexer.Equipment.propBasic4Min, rowIndex);
            this.propBasic4Max = table.Get<int>((int)resource.mapping.settings.Item.HeaderIndexer.Equipment.propBasic4Max, rowIndex);
            this.propBasic5Type = table.Get<int>((int)resource.mapping.settings.Item.HeaderIndexer.Equipment.propBasic5Type, rowIndex);
            this.propBasic5Min = table.Get<int>((int)resource.mapping.settings.Item.HeaderIndexer.Equipment.propBasic5Min, rowIndex);
            this.propBasic5Max = table.Get<int>((int)resource.mapping.settings.Item.HeaderIndexer.Equipment.propBasic5Max, rowIndex);
            this.propBasic6Type = table.Get<int>((int)resource.mapping.settings.Item.HeaderIndexer.Equipment.propBasic6Type, rowIndex);
            this.propBasic6Min = table.Get<int>((int)resource.mapping.settings.Item.HeaderIndexer.Equipment.propBasic6Min, rowIndex);
            this.propBasic6Max = table.Get<int>((int)resource.mapping.settings.Item.HeaderIndexer.Equipment.propBasic6Max, rowIndex);
            this.requirement0Type = table.Get<int>((int)resource.mapping.settings.Item.HeaderIndexer.Equipment.requirement0Type, rowIndex);
            this.requirement0Para = table.Get<int>((int)resource.mapping.settings.Item.HeaderIndexer.Equipment.requirement0Para, rowIndex);
            this.requirement1Type = table.Get<int>((int)resource.mapping.settings.Item.HeaderIndexer.Equipment.requirement1Type, rowIndex);
            this.requirement1Para = table.Get<int>((int)resource.mapping.settings.Item.HeaderIndexer.Equipment.requirement1Para, rowIndex);
            this.requirement2Type = table.Get<int>((int)resource.mapping.settings.Item.HeaderIndexer.Equipment.requirement2Type, rowIndex);
            this.requirement2Para = table.Get<int>((int)resource.mapping.settings.Item.HeaderIndexer.Equipment.requirement2Para, rowIndex);
            this.requirement3Type = table.Get<int>((int)resource.mapping.settings.Item.HeaderIndexer.Equipment.requirement3Type, rowIndex);
            this.requirement3Para = table.Get<int>((int)resource.mapping.settings.Item.HeaderIndexer.Equipment.requirement3Para, rowIndex);
            this.requirement4Type = table.Get<int>((int)resource.mapping.settings.Item.HeaderIndexer.Equipment.requirement4Type, rowIndex);
            this.requirement4Para = table.Get<int>((int)resource.mapping.settings.Item.HeaderIndexer.Equipment.requirement4Para, rowIndex);
            this.requirement5Type = table.Get<int>((int)resource.mapping.settings.Item.HeaderIndexer.Equipment.requirement5Type, rowIndex);
            this.requirement5Para = table.Get<int>((int)resource.mapping.settings.Item.HeaderIndexer.Equipment.requirement5Para, rowIndex);

            this.baseAttrib = new List<skill.SkillSettingData.KMagicAttrib>()
            {
                new skill.SkillSettingData.KMagicAttrib(this.propBasic0Type, this.propBasic0Min, this.propBasic0Max, 0),
                new skill.SkillSettingData.KMagicAttrib(this.propBasic1Type, this.propBasic1Min, this.propBasic1Max, 0),
                new skill.SkillSettingData.KMagicAttrib(this.propBasic2Type, this.propBasic2Min, this.propBasic2Max, 0),
                new skill.SkillSettingData.KMagicAttrib(this.propBasic3Type, this.propBasic3Min, this.propBasic3Max, 0),
                new skill.SkillSettingData.KMagicAttrib(this.propBasic4Type, this.propBasic4Min, this.propBasic4Max, 0),
                new skill.SkillSettingData.KMagicAttrib(this.propBasic5Type, this.propBasic5Min, this.propBasic5Max, 0),
                new skill.SkillSettingData.KMagicAttrib(this.propBasic6Type, this.propBasic6Min, this.propBasic6Max, 0),
            };

            this.requiredAttrib = new List<skill.SkillSettingData.KMagicAttrib>()
            {
                new skill.SkillSettingData.KMagicAttrib(this.requirement0Type, this.requirement0Para),
                new skill.SkillSettingData.KMagicAttrib(this.requirement1Type, this.requirement1Para),
                new skill.SkillSettingData.KMagicAttrib(this.requirement2Type, this.requirement2Para),
                new skill.SkillSettingData.KMagicAttrib(this.requirement3Type, this.requirement3Para),
                new skill.SkillSettingData.KMagicAttrib(this.requirement4Type, this.requirement4Para),
                new skill.SkillSettingData.KMagicAttrib(this.requirement5Type, this.requirement5Para),
            };
        }

        public string GetKeyGDP()
        {
            return this.genre + ", " + this.detail + ", " + this.particular;
        }

        public string GetKeyGDPL()
        {
            return this.GetKeyGDP() + ", " + this.level;
        }

        public string GetDetailRowKey()
        {
            return MakeDetailRowKey(this.detail, this.rowIndex - 1);
        }

        public static string MakeDetailRowKey(int detail, int axmolRecordIndex)
        {
            return detail + ", " + axmolRecordIndex;
        }

        public List<skill.SkillSettingData.KMagicAttrib> GetBasicAttrib()
        {
            return this.baseAttrib;
        }

        public List<skill.SkillSettingData.KMagicAttrib> GetRequiredAttrib()
        {
            return this.requiredAttrib;
        }
    }
}
