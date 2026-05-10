
using System.Collections.Generic;

namespace game.resource.settings.item
{
    class Getters
    {
        public class Appearance
        {
            public static int Line(string _partName, int _declareLine)
            {
                if (Cache.Settings.Item.appearanceParsedMapping.ContainsKey(_partName) == false) return 0;
                if (Cache.Settings.Item.appearanceParsedMapping[_partName].ContainsKey(_declareLine) == false) return 0;

                return Cache.Settings.Item.appearanceParsedMapping[_partName][_declareLine];
            }

            public static int ArmorLine(int _declareLine) => Appearance.Line(mapping.settings.Item.armorres, _declareLine);

            public static int HelmLine(int _declareLine) => Appearance.Line(mapping.settings.Item.helmres, _declareLine);

            public static int HorseLine(int _declareLine) => Appearance.Line(mapping.settings.Item.horseres, _declareLine);

            public static int MeleWeaponLine(int _declareLine) => Appearance.Line(mapping.settings.Item.meleeres, _declareLine);

            public static int RangeWeaponLine(int _declareLine) => Appearance.Line(mapping.settings.Item.rangeres, _declareLine);
        }

        public static settings.item.EquipmentBase GetEquipmentBase(int g, int d, int p, int l)
        {
            string key = string.Empty + g + ", " + d + ", " + p + ", " + l;

            if (Cache.Settings.Item.equipmentBaseMapping != null
                && Cache.Settings.Item.equipmentBaseMapping.ContainsKey(key))
            {
                return Cache.Settings.Item.equipmentBaseMapping[key];
            }

            if (Cache.Settings.Item.equipmentBaseRowMapping == null)
            {
                return null;
            }

            int level = System.Math.Max(1, l);
            int axmolRecordIndex = d == (int)Defination.Detail.equip_mask
                ? p
                : (p * 10) + level - 1;
            string rowKey = settings.item.EquipmentBase.MakeDetailRowKey(d, axmolRecordIndex);

            return Cache.Settings.Item.equipmentBaseRowMapping.ContainsKey(rowKey)
                ? Cache.Settings.Item.equipmentBaseRowMapping[rowKey]
                : null;
        }

        public static settings.item.EquipmentBase GetMaskBase(int g, int d, int p)
        {
            string key = string.Empty + g + ", " + d + ", " + p;

            if(Cache.Settings.Item.maskEquipBase == null
                || Cache.Settings.Item.maskEquipBase.ContainsKey(key) == false)
            {
                return null;
            }

            return Cache.Settings.Item.maskEquipBase[key];
        }

        public static settings.item.SimpleItemBase GetSimpleItemBase(int g, int d, int p, int l)
        {
            if (Cache.Settings.Item.itemBaseLevelMapping == null
                || Cache.Settings.Item.itemBaseMapping == null)
            {
                return null;
            }

            string levelKey = settings.item.SimpleItemBase.MakeKeyGDPL(g, d, p, l);
            if (Cache.Settings.Item.itemBaseLevelMapping.ContainsKey(levelKey))
            {
                return Cache.Settings.Item.itemBaseLevelMapping[levelKey];
            }

            string key = settings.item.SimpleItemBase.MakeKeyGDP(g, d, p);
            if (Cache.Settings.Item.itemBaseMapping.ContainsKey(key))
            {
                return Cache.Settings.Item.itemBaseMapping[key];
            }

            if (g == (int)Defination.Genre.item_medicine)
            {
                int rowIndex = (d * 5) + System.Math.Max(1, l) - 1;
                settings.item.SimpleItemBase rowItem = GetSimpleItemBaseByRow(g, rowIndex);
                if (rowItem != null)
                {
                    return rowItem;
                }
            }
            else if (g == (int)Defination.Genre.item_task)
            {
                settings.item.SimpleItemBase rowItem = GetSimpleItemBaseByRow(g, d);
                if (rowItem != null)
                {
                    return rowItem;
                }

                key = settings.item.SimpleItemBase.MakeKeyGDP(g, d, 0);
                if (Cache.Settings.Item.itemBaseMapping.ContainsKey(key))
                {
                    return Cache.Settings.Item.itemBaseMapping[key];
                }
            }
            else if (g == (int)Defination.Genre.item_mine)
            {
                settings.item.SimpleItemBase rowItem = GetSimpleItemBaseByRow(g, p);
                if (rowItem != null)
                {
                    return rowItem;
                }

                rowItem = GetSimpleItemBaseByRow(g, d);
                if (rowItem != null)
                {
                    return rowItem;
                }
            }
            else if (g == (int)Defination.Genre.item_fusion)
            {
                settings.item.SimpleItemBase rowItem = GetSimpleItemBaseByRow(g, p);
                if (rowItem != null)
                {
                    return rowItem;
                }
            }
            else if (g == (int)Defination.Genre.item_townportal)
            {
                return GetSimpleItemBaseByRow(g, 0);
            }

            return null;
        }

        public static settings.item.SimpleItemBase GetSimpleItemBaseByRow(int g, int rowIndex)
        {
            if (Cache.Settings.Item.itemBaseRowMapping == null)
            {
                return null;
            }

            string rowKey = settings.item.SimpleItemBase.MakeRowKey(g, rowIndex);
            if (Cache.Settings.Item.itemBaseRowMapping.ContainsKey(rowKey) == false)
            {
                return null;
            }

            return Cache.Settings.Item.itemBaseRowMapping[rowKey];
        }

        public static settings.item.MagicScriptBase GetMagicScriptBase(int g, int d, int p)
        {
            string key = string.Empty + g + ", " + d + ", " + p;

            if(Cache.Settings.Item.magicScriptBase.ContainsKey(key) == false)
            {
                return null;
            }

            return Cache.Settings.Item.magicScriptBase[key];
        }

        /// <summary>
        /// position: 1: show, 0: hide
        /// return nPropKind mapping
        /// </summary>
        public static Dictionary<int, Dictionary<int, settings.item.MagicattribBase>> GetMagicAttribBase(int detail, int series, int position)
        {
            // ["detail, series, position"]
            string key = string.Empty + detail + ", " + series + ", " + position;

            if (Cache.Settings.Item.magicAttribBaseMapping.ContainsKey(key) == false)
            {
                return null;
            }

            return Cache.Settings.Item.magicAttribBaseMapping[key];
        }

        public static settings.item.GoldEquipBase GetGoldEquipBase(int index)
        {
            if(Cache.Settings.Item.goldEquipBase == null
                || Cache.Settings.Item.goldEquipBase.ContainsKey(index) == false)
            {
                return null;
            }

            return Cache.Settings.Item.goldEquipBase[index];
        }

        public static settings.item.GoldEquipBase GetPlatinaEquipBase(int index)
        {
            if (Cache.Settings.Item.platinaEquipBase == null
                || Cache.Settings.Item.platinaEquipBase.ContainsKey(index) == false)
            {
                return null;
            }

            return Cache.Settings.Item.platinaEquipBase[index];
        }

        public static settings.item.GoldMagicBase GetGoldMagicBase(int index)
        {
            if(Cache.Settings.Item.goldMagicBase.ContainsKey(index) == false)
            {
                return null;
            }

            return Cache.Settings.Item.goldMagicBase[index];
        }

        public static Dictionary<int, string> GetGoldItemSet(int idSet)
        {
            Dictionary<int, string> result = new Dictionary<int, string>();

            if(Cache.Settings.Item.goldEquipSet.ContainsKey(idSet) == false)
            {
                return result;
            }

            List<int> setList = Cache.Settings.Item.goldEquipSet[idSet];

            for(int i = 0; i < setList.Count; i++)
            {
                int setElementIndex = (setList[i] - (setList[i] % 100)) / 100;
                result[setElementIndex] = item.Getters.GetGoldEquipBase(setElementIndex).name;
            }

            return result;
        }

        public static int GetGoldEquipRes(int goldEquipRowIndex)
        {
            if(Cache.Settings.Item.goldEquipRes.ContainsKey(goldEquipRowIndex) == false)
            {
                return 0;
            }

            return Cache.Settings.Item.goldEquipRes[goldEquipRowIndex].resId;
        }
    }
}
