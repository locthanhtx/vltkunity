
using System.Collections.Generic;

namespace game.resource.settings.item
{
    public class MagicattribBase
    {
        public const int NoSeri = -1;

        ///////////////////////////////////////////////////////////////////////////

        public string name;
        public int nextLevel;
        public int loadModel;
        public int pos;
        public int series;
        public int level;
        public int propKind;
        public int value0Min;
        public int value0Max;
        public int value1Min;
        public int value1Max;
        public int value2Min;
        public int value2Max;
        public string intro;
        public Dictionary<int, int> droprate;

        ///////////////////////////////////////////////////////////////////////////

        public void Load(resource.Table table, int rowIndex)
        {
            this.name = table.Get<string>((int)resource.mapping.settings.Item.HeaderIndexer.MagicAttrib.name, rowIndex);
            this.nextLevel = table.Get<int>((int)resource.mapping.settings.Item.HeaderIndexer.MagicAttrib.nextLevel, rowIndex);
            this.loadModel = table.Get<int>((int)resource.mapping.settings.Item.HeaderIndexer.MagicAttrib.loadModel, rowIndex);
            this.pos = table.Get<int>((int)resource.mapping.settings.Item.HeaderIndexer.MagicAttrib.pos, rowIndex);
            this.series = table.Get<int>((int)resource.mapping.settings.Item.HeaderIndexer.MagicAttrib.series, rowIndex, MagicattribBase.NoSeri);
            this.level = table.Get<int>((int)resource.mapping.settings.Item.HeaderIndexer.MagicAttrib.level, rowIndex);
            this.propKind = table.Get<int>((int)resource.mapping.settings.Item.HeaderIndexer.MagicAttrib.propKind, rowIndex);
            this.value0Min = table.Get<int>((int)resource.mapping.settings.Item.HeaderIndexer.MagicAttrib.value0Min, rowIndex);
            this.value0Max = table.Get<int>((int)resource.mapping.settings.Item.HeaderIndexer.MagicAttrib.value0Max, rowIndex);
            this.value1Min = table.Get<int>((int)resource.mapping.settings.Item.HeaderIndexer.MagicAttrib.value1Min, rowIndex);
            this.value1Max = table.Get<int>((int)resource.mapping.settings.Item.HeaderIndexer.MagicAttrib.value1Max, rowIndex);
            this.value2Min = table.Get<int>((int)resource.mapping.settings.Item.HeaderIndexer.MagicAttrib.value2Min, rowIndex);
            this.value2Max = table.Get<int>((int)resource.mapping.settings.Item.HeaderIndexer.MagicAttrib.value2Max, rowIndex);
            this.intro = table.Get<string>((int)resource.mapping.settings.Item.HeaderIndexer.MagicAttrib.intro, rowIndex);
            
            int dropRate0VuKhi = table.Get<int>((int)resource.mapping.settings.Item.HeaderIndexer.MagicAttrib.dropRate0VuKhi, rowIndex);
            int dropRate1AmKhi = table.Get<int>((int)resource.mapping.settings.Item.HeaderIndexer.MagicAttrib.dropRate1AmKhi, rowIndex);
            int dropRate2Ao = table.Get<int>((int)resource.mapping.settings.Item.HeaderIndexer.MagicAttrib.dropRate2Ao, rowIndex);
            int dropRate3Nhan = table.Get<int>((int)resource.mapping.settings.Item.HeaderIndexer.MagicAttrib.dropRate3Nhan, rowIndex);
            int dropRate4DayChuyen = table.Get<int>((int)resource.mapping.settings.Item.HeaderIndexer.MagicAttrib.dropRate4DayChuyen, rowIndex);
            int dropRate5Giay = table.Get<int>((int)resource.mapping.settings.Item.HeaderIndexer.MagicAttrib.dropRate5Giay, rowIndex);
            int dropRate6Dai = table.Get<int>((int)resource.mapping.settings.Item.HeaderIndexer.MagicAttrib.dropRate6Dai, rowIndex);
            int dropRate7Non = table.Get<int>((int)resource.mapping.settings.Item.HeaderIndexer.MagicAttrib.dropRate7Non, rowIndex);
            int dropRate8BaoTay = table.Get<int>((int)resource.mapping.settings.Item.HeaderIndexer.MagicAttrib.dropRate8BaoTay, rowIndex);
            int dropRate9DayChuyen = table.Get<int>((int)resource.mapping.settings.Item.HeaderIndexer.MagicAttrib.dropRate9DayChuyen, rowIndex);
            int dropRate10Ngua = table.Get<int>((int)resource.mapping.settings.Item.HeaderIndexer.MagicAttrib.dropRate10Ngua, rowIndex);
            int dropRate11MatNa = table.Get<int>((int)resource.mapping.settings.Item.HeaderIndexer.MagicAttrib.dropRate11MatNa, rowIndex);
            int dropRate12PhiPhong = table.Get<int>((int)resource.mapping.settings.Item.HeaderIndexer.MagicAttrib.dropRate12PhiPhong, rowIndex);
            int dropRate13AnGiam = table.Get<int>((int)resource.mapping.settings.Item.HeaderIndexer.MagicAttrib.dropRate13AnGiam, rowIndex);
            int dropRate14TrangSuc = table.Get<int>((int)resource.mapping.settings.Item.HeaderIndexer.MagicAttrib.dropRate14TrangSuc, rowIndex);

            this.droprate = new Dictionary<int, int>()
            {
                {(int)settings.item.Defination.Detail.equip_meleeweapon, dropRate0VuKhi},
                {(int)settings.item.Defination.Detail.equip_rangeweapon, dropRate1AmKhi},
                {(int)settings.item.Defination.Detail.equip_armor, dropRate2Ao},
                {(int)settings.item.Defination.Detail.equip_ring, dropRate3Nhan},
                {(int)settings.item.Defination.Detail.equip_amulet, dropRate4DayChuyen},
                {(int)settings.item.Defination.Detail.equip_boots, dropRate5Giay},
                {(int)settings.item.Defination.Detail.equip_belt, dropRate6Dai},
                {(int)settings.item.Defination.Detail.equip_helm, dropRate7Non},
                {(int)settings.item.Defination.Detail.equip_cuff, dropRate8BaoTay},
                {(int)settings.item.Defination.Detail.equip_pendant, dropRate9DayChuyen},
                {(int)settings.item.Defination.Detail.equip_horse, dropRate10Ngua},
                {(int)settings.item.Defination.Detail.equip_mask, dropRate11MatNa},
                {(int)settings.item.Defination.Detail.equip_pifeng, dropRate12PhiPhong},
                {(int)settings.item.Defination.Detail.equip_yinjian, dropRate13AnGiam},
                {(int)settings.item.Defination.Detail.equip_shiping, dropRate14TrangSuc},
            };
        }

        public int GetDropRate(int detail)
        {
            if(this.droprate.ContainsKey(detail) == false)
            {
                return 0;
            }

            return this.droprate[detail];
        }

        public List<string> GetCacheKeyList()
        {
            // ["detail, series, position"]

            List<string> result = new List<string>();
            List<int> seriesList = null;

            if(this.series != MagicattribBase.NoSeri)
            {
                seriesList = new List<int>()
                {
                    this.series
                };
            }
            else
            {
                seriesList = new List<int>()
                {
                    (int)item.Defination.Series.metal,
                    (int)item.Defination.Series.wood,
                    (int)item.Defination.Series.water,
                    (int)item.Defination.Series.fire,
                    (int)item.Defination.Series.earth,
                };
            }

            foreach(KeyValuePair<int, int> detailEntry in this.droprate)
            {
                if(detailEntry.Value > 0)
                {
                    foreach(int seriEntry in seriesList)
                    {
                        string key = string.Empty + detailEntry.Key + ", " + seriEntry + ", " + this.pos;
                        result.Add(key);
                    }
                }
            }

            return result;
        }
    }
}
