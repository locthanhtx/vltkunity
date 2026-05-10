
using System.Collections.Generic;
using System.Linq;

namespace game.resource.settings.item
{
    public class Generator : item.Getter
    {
        /// <summary>
        /// tạo thuộc tính ma pháp
        /// </summary>
        /// <param name="equipmentBase"></param>
        /// <param name="series"></param>
        /// <param name="luckyPoint">điểm may mắn, từ 0 -> 100 %</param>
        /// <returns></returns>
        private void GenerateEquipment(settings.item.EquipmentBase equipmentBase, int series, int luckyPercent)
        {
            if (equipmentBase == null)
            {
                return;
            }

            this.type = Defination.Type.normalEquip;
            this.equipmentBase = equipmentBase;
            this.series = series;
            this.magicAttrib = new List<skill.SkillSettingData.KMagicAttrib>();

            // [propType] => [level] => <...>
            Dictionary<int, Dictionary<int, settings.item.MagicattribBase>> showMagicMapping = item.Getters.GetMagicAttribBase(equipmentBase.detail, series, 1);
            Dictionary<int, Dictionary<int, settings.item.MagicattribBase>> hideMagicMapping = item.Getters.GetMagicAttribBase(equipmentBase.detail, series, 0);

            if (showMagicMapping == null)
            {
                return;
            }

            if (luckyPercent > 90)
            {
                luckyPercent = 90;
            }

            int magicCount = 4 + ((skill.Static.g_Random(100) <= luckyPercent) ? 2 : 0);
            Dictionary<int, bool> usedMagic = new Dictionary<int, bool>();

            for (int indexMagic = 0; indexMagic < magicCount;)
            {
                Dictionary<int, Dictionary<int, settings.item.MagicattribBase>> magicMapping = null;

                if ((indexMagic & 1) == 0)
                {
                    magicMapping = showMagicMapping;
                }
                else
                {
                    magicMapping = hideMagicMapping;
                }

                int magicMapRand = settings.skill.Static.g_Random(magicMapping.Count);

                if (usedMagic.ContainsKey(magicMapRand))
                {
                    magicCount--;
                    continue;
                }
                else
                {
                    usedMagic[magicMapRand] = true;
                }

                int randIntPercent = settings.skill.Static.g_Random(100 - luckyPercent);
                int randFloatPercent = settings.skill.Static.g_Random(100 - luckyPercent);
                int randPercent = ((randIntPercent * 100) + randFloatPercent) * 100;
                item.MagicattribBase addMagic = null;

                for (int levelItem = equipmentBase.level; levelItem > 0; levelItem--)
                {
                    if (magicMapping.ElementAt(magicMapRand).Value.ContainsKey(levelItem) == false)
                    {
                        continue;
                    }

                    int magicDroprate = magicMapping.ElementAt(magicMapRand).Value[levelItem].GetDropRate(equipmentBase.detail);

                    if (randPercent <= magicDroprate)
                    {
                        addMagic = magicMapping.ElementAt(magicMapRand).Value[levelItem];
                        break;
                    }
                }

                if (addMagic == null)
                {
                    magicCount--;
                    continue;
                }

                skill.SkillSettingData.KMagicAttrib newMagicAttrib = new skill.SkillSettingData.KMagicAttrib();
                newMagicAttrib.nAttribType = addMagic.propKind;
                newMagicAttrib.nValue[0] = skill.Static.GetRandomNumber(addMagic.value0Min, addMagic.value0Max);
                newMagicAttrib.nValue[1] = skill.Static.GetRandomNumber(addMagic.value1Min, addMagic.value1Max);
                newMagicAttrib.nValue[2] = skill.Static.GetRandomNumber(addMagic.value2Min, addMagic.value2Max);

                this.magicAttrib.Add(newMagicAttrib);
                indexMagic++;
            }
        }

        private void GenerateMaskEquip(settings.item.EquipmentBase equipmentBase)
        {
            this.type = Defination.Type.normalEquip;
            this.equipmentBase = equipmentBase;
            List<skill.SkillSettingData.KMagicAttrib> basicAttrib = this.equipmentBase.GetBasicAttrib();

            if (basicAttrib != null)
            {
                foreach (skill.SkillSettingData.KMagicAttrib magicEntry in basicAttrib)
                {
                    if(magicEntry.nAttribType == 44)
                    {
                        // this.timeUse nắm giữ thời hạn sử dụng đến thời điểm chỉ định
                        // tính bằng giây
                        this.timeUse = System.DateTimeOffset.UtcNow.ToUnixTimeSeconds() + (settings.skill.Static.GetRandomNumber(magicEntry.nValue[0], magicEntry.nValue[1]) * 60);
                    }
                    else if (magicEntry.nAttribType == 31)
                    {
                        // this.timeUse nắm giữ số lần sử dụng còn lại
                        // tính bằng lần
                        this.timeUse = settings.skill.Static.GetRandomNumber(magicEntry.nValue[0], magicEntry.nValue[1]);
                    }
                }
            }
        }

        private void GenerateMagicScript(int g, int d, int p, int l, int s)
        {
            item.MagicScriptBase magicScriptBase = item.Getters.GetMagicScriptBase(g, d, p);

            if (magicScriptBase == null)
            {
                return;
            }

            this.magicScriptBase = magicScriptBase;
            this.level = l;
            this.series = s;
        }

        private void GenerateSimpleItem(int g, int d, int p, int l, int s)
        {
            item.SimpleItemBase simpleItemBase = item.Getters.GetSimpleItemBase(g, d, p, l);

            if (simpleItemBase == null)
            {
                return;
            }

            this.type = Defination.Type.normalItem;
            this.simpleItemBase = simpleItemBase;
            this.level = l > 0 ? l : simpleItemBase.level;
            this.series = s >= 0 ? s : simpleItemBase.series;
            this.stack = simpleItemBase.stack > 0 ? 1 : 0;
        }

        /// <summary>
        /// tạo vật phẩm dựa vào file goldequip.txt
        /// </summary>
        /// <param name="index">row index</param>
        private void GenerateGoldEquip(int index)
        {
            item.GoldEquipBase goldEquipBase = item.Getters.GetGoldEquipBase(index);

            if (goldEquipBase == null)
            {
                return;
            }

            this.type = Defination.Type.goldEquip;
            this.equipmentBase = goldEquipBase;
            this.series = goldEquipBase.series;
            this.magicAttrib = new List<skill.SkillSettingData.KMagicAttrib>();

            List<int> magicIndexList = goldEquipBase.GetMagicIndexList();

            for (int i = 0; i < magicIndexList.Count; i++)
            {
                settings.item.GoldMagicBase goldMagicBase = item.Getters.GetGoldMagicBase(magicIndexList[i]);

                if (goldMagicBase == null)
                {
                    break;
                }

                skill.SkillSettingData.KMagicAttrib newMagicAttrib = new skill.SkillSettingData.KMagicAttrib();
                newMagicAttrib.nAttribType = goldMagicBase.type;
                newMagicAttrib.nValue[0] = settings.skill.Static.GetRandomNumber(goldMagicBase.value1Min, goldMagicBase.value1Max);
                newMagicAttrib.nValue[1] = settings.skill.Static.GetRandomNumber(goldMagicBase.value2Min, goldMagicBase.value2Max);
                newMagicAttrib.nValue[2] = settings.skill.Static.GetRandomNumber(goldMagicBase.value3Min, goldMagicBase.value3Max);

                this.magicAttrib.Add(newMagicAttrib);
            }
        }

        ///////////////////////////////////////////////////////////////////////////

        protected void Generate(int g, int d, int p, int l = 1, int s = 0, int luckyPercent = 5)
        {
            switch (g)
            {
                case 0:
                    switch (d)
                    {
                        case 11:
                            this.GenerateMaskEquip(item.Getters.GetMaskBase(g, d, p));
                            break;

                        default:
                            this.GenerateEquipment(item.Getters.GetEquipmentBase(g, d, p, l), s, luckyPercent);
                            break;
                    }
                    break;

                case 6:
                    this.GenerateSimpleItem(g, d, p, l, s);
                    break;

                case 1:
                case 2:
                case 3:
                case 4:
                case 5:
                    this.GenerateSimpleItem(g, d, p, l, s);
                    break;
            }
        }

        protected void Generate(int id, item.Defination.Type type)
        {
            switch (type)
            {
                case Defination.Type.goldEquip:
                    this.GenerateGoldEquip(id);
                    break;
            }
        }
    }
}
