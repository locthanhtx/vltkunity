
namespace game.resource.settings.item
{
    public class Setters : item.Datafield
    {
        private void SetCommonData(item.Database database)
        {
            this.databaseId = database.databaseId;
            this.type = database.type;
            this.stack = database.stack;
            this.timeUse = database.timeUse;
            this.level = database.level;
            this.series = database.series;
        }

        private void SetDataEquipment(item.Database database)
        {
            this.magicAttrib = new System.Collections.Generic.List<skill.SkillSettingData.KMagicAttrib>();
            this.SetCommonData(database);

            switch (this.type)
            {
                case Defination.Type.normalEquip:
                    this.equipmentBase = item.Getters.GetEquipmentBase(database.genre, database.detail, database.particular, database.level);
                    if (this.equipmentBase == null && database.detail == (int)Defination.Detail.equip_mask)
                    {
                        this.equipmentBase = item.Getters.GetMaskBase(database.genre, database.detail, database.particular);
                    }
                    this.series = database.series;
                    break;

                case Defination.Type.goldEquip:
                    this.equipmentBase = item.Getters.GetGoldEquipBase(database.rowIndex);
                    if (this.equipmentBase != null)
                    {
                        this.series = this.equipmentBase.series;
                    }
                    break;

                case Defination.Type.platinaEquip:
                    this.equipmentBase = item.Getters.GetPlatinaEquipBase(database.rowIndex);
                    if (this.equipmentBase != null)
                    {
                        this.series = this.equipmentBase.series;
                    }
                    break;
            }

            System.Collections.Generic.List<skill.SkillSettingData.KMagicAttrib> magicList = new System.Collections.Generic.List<skill.SkillSettingData.KMagicAttrib>()
            {
                new skill.SkillSettingData.KMagicAttrib(database.magic0Type, database.magic0Value0, database.magic0Value1, database.magic0Value2),
                new skill.SkillSettingData.KMagicAttrib(database.magic1Type, database.magic1Value0, database.magic1Value1, database.magic1Value2),
                new skill.SkillSettingData.KMagicAttrib(database.magic2Type, database.magic2Value0, database.magic2Value1, database.magic2Value2),
                new skill.SkillSettingData.KMagicAttrib(database.magic3Type, database.magic3Value0, database.magic3Value1, database.magic3Value2),
                new skill.SkillSettingData.KMagicAttrib(database.magic4Type, database.magic4Value0, database.magic4Value1, database.magic4Value2),
                new skill.SkillSettingData.KMagicAttrib(database.magic5Type, database.magic5Value0, database.magic5Value1, database.magic5Value2),
            };

            foreach (skill.SkillSettingData.KMagicAttrib magicEntry in magicList)
            {
                if (magicEntry.nAttribType == 0)
                {
                    continue;
                }

                this.magicAttrib.Add(magicEntry);
            }
        }

        private void SetDataMaskEquip(item.Database database)
        {
            this.SetCommonData(database);
            this.type = Defination.Type.normalEquip;
            this.equipmentBase = item.Getters.GetMaskBase(database.genre, database.detail, database.particular);
        }

        private void SetDataMagicScript(item.Database database)
        {
            this.SetCommonData(database);
            this.magicScriptBase = item.Getters.GetMagicScriptBase(database.genre, database.detail, database.particular);
        }

        private void SetDataSimpleItem(item.Database database)
        {
            this.magicAttrib = new System.Collections.Generic.List<skill.SkillSettingData.KMagicAttrib>();
            this.SetCommonData(database);
            this.type = Defination.Type.normalItem;
            this.simpleItemBase = item.Getters.GetSimpleItemBase(database.genre, database.detail, database.particular, database.level);

            if (this.simpleItemBase != null)
            {
                this.level = database.level > 0 ? database.level : this.simpleItemBase.level;
                this.series = database.series >= 0 ? database.series : this.simpleItemBase.series;

                if (this.stack <= 0)
                {
                    this.stack = this.simpleItemBase.stack > 0 ? 1 : 0;
                }
            }
        }

        protected void SetData(item.Database database)
        {
            switch (database.genre)
            {
                case 0:
                    switch(database.detail)
                    {
                        case 11:
                            this.SetDataMaskEquip(database);
                            break;

                        default:
                            this.SetDataEquipment(database);
                            break;
                    }
                    break;

                case 1:
                case 2:
                case 3:
                case 4:
                case 5:
                    this.SetDataSimpleItem(database);
                    break;

                case 6:
                    this.SetDataSimpleItem(database);
                    break;
            }
        }
    }
}
