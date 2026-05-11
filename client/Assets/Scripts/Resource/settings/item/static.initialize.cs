
using System.Collections.Generic;

namespace game.resource.settings.item
{
    class Initialize
    {
        public Initialize()
        {
            Cache.Settings.Item.appearanceParsedMapping = Initialize.AppearanceParser(Initialize.LoadTableList(item.Listing.Appearance()));
            Cache.Settings.Item.equipmentBaseRowMapping = new Dictionary<string, EquipmentBase>();
            Cache.Settings.Item.equipmentBaseMapping = Initialize.EquipmentParser(settings.item.Listing.Equipment());
            Cache.Settings.Item.maskEquipBase = Initialize.MaskEquipParser(mapping.settings.Item.mask);
            Cache.Settings.Item.itemBaseMapping = new Dictionary<string, SimpleItemBase>();
            Cache.Settings.Item.itemBaseLevelMapping = new Dictionary<string, SimpleItemBase>();
            Cache.Settings.Item.itemBaseRowMapping = new Dictionary<string, SimpleItemBase>();
            Initialize.SimpleItemBaseParser(settings.item.Listing.SimpleItem());
            Cache.Settings.Item.magicAttribBaseMapping = Initialize.MagicAttribParser(mapping.settings.Item.magicattrib);
            Cache.Settings.Item.goldEquipBase = Initialize.GoldEquipBaseParser(mapping.settings.Item.goldEquip);
            Cache.Settings.Item.platinaEquipBase = Initialize.PlatinaEquipBaseParser(mapping.settings.Item.platinaEquip);
            Cache.Settings.Item.goldMagicBase = Initialize.GoldMagicBaseParser(mapping.settings.Item.magicattrib_ge);
            //Cache.Settings.Item.goldEquipRes = Initialize.GoldEquipResParser(mapping.settings.Item.goldEquipRes); // FIXME: native crash SIGSEGV in PluginApi.b
            Cache.Settings.Item.goldEquipRes = new System.Collections.Generic.Dictionary<int, GoldResBase>();
            //Cache.Settings.Item.magicScriptBase = Initialize.MagicScriptBaseParser(mapping.settings.Item.magicScript);
        }

        private static void SimpleItemBaseParser(Dictionary<string, SimpleItemKind> tableList)
        {
            foreach (KeyValuePair<string, SimpleItemKind> tableEntry in tableList)
            {
                resource.Table fileTable = Game.Resource(tableEntry.Key).Get<resource.Table>();

                if (fileTable.IsEmpty())
                {
                    UnityEngine.Debug.LogWarning("Item simple table missing or empty: " + tableEntry.Key);
                    continue;
                }

                for (int rowIndex = 1; rowIndex < fileTable.RowCount; rowIndex++)
                {
                    SimpleItemBase itemBase = new SimpleItemBase();
                    itemBase.Load(fileTable, rowIndex, tableEntry.Value);

                    if (string.IsNullOrEmpty(itemBase.name)
                        && itemBase.genre <= 0
                        && itemBase.detail <= 0
                        && itemBase.particular <= 0)
                    {
                        continue;
                    }

                    Cache.Settings.Item.itemBaseMapping[itemBase.GetKeyGDP()] = itemBase;
                    Cache.Settings.Item.itemBaseLevelMapping[itemBase.GetKeyGDPL()] = itemBase;
                    Cache.Settings.Item.itemBaseRowMapping[itemBase.GetRowKey()] = itemBase;
                }
            }
        }

        private static Dictionary<string, settings.item.MagicScriptBase> MagicScriptBaseParser(string filePath)
        {
            Dictionary<string, settings.item.MagicScriptBase> result = new Dictionary<string, MagicScriptBase>();
            resource.Table fileTable = Game.Resource(filePath).Get<resource.Table>();

            if (fileTable.IsEmpty())
            {
                UnityEngine.Debug.LogError(filePath);
                return result;
            }

            for (int rowIndex = 1; rowIndex < fileTable.RowCount; rowIndex++)
            {
                settings.item.MagicScriptBase magicScriptBase = new MagicScriptBase();
                magicScriptBase.Load(fileTable, rowIndex);
                result[magicScriptBase.GetKey()] = magicScriptBase;
            }

            return result;
        }

        private static Dictionary<int, settings.item.GoldResBase> GoldEquipResParser(string filePath)
        {
            Dictionary<int, settings.item.GoldResBase> result = new Dictionary<int, GoldResBase>();
            resource.Table fileTable = Game.Resource(filePath).Get<resource.Table>();

            if (fileTable.IsEmpty())
            {
                UnityEngine.Debug.LogError(filePath);
                return result;
            }

            for (int rowIndex = 1; rowIndex < fileTable.RowCount; rowIndex++)
            {
                settings.item.GoldResBase goldResBase = new GoldResBase();
                goldResBase.Load(fileTable, rowIndex);
                result[goldResBase.equipRowIndex] = goldResBase;
            }

            return result;
        }

        private static Dictionary<int, settings.item.GoldMagicBase> GoldMagicBaseParser(string filePath)
        {
            Dictionary<int, settings.item.GoldMagicBase> result = new Dictionary<int, GoldMagicBase>();
            resource.Table fileTable = Game.Resource(filePath).Get<resource.Table>();

            if (fileTable.IsEmpty())
            {
                UnityEngine.Debug.LogError(filePath);
                return result;
            }

            for (int rowIndex = 1; rowIndex < fileTable.RowCount; rowIndex++)
            {
                settings.item.GoldMagicBase goldMagicBase = new GoldMagicBase();
                goldMagicBase.Load(fileTable, rowIndex);
                result[rowIndex] = goldMagicBase;
            }

            return result;
        }

        private static Dictionary<int, settings.item.GoldEquipBase> GoldEquipBaseParser(string filePath)
        {
            Dictionary<int, settings.item.GoldEquipBase> result = new Dictionary<int, GoldEquipBase>();
            resource.Table fileTable = Game.Resource(filePath).Get<resource.Table>();

            if (fileTable.IsEmpty())
            {
                UnityEngine.Debug.LogError(filePath);
                return result;
            }

            Cache.Settings.Item.goldEquipSet = new Dictionary<int, List<int>>();

            for (int rowIndex = 1; rowIndex < fileTable.RowCount; rowIndex++)
            {
                settings.item.GoldEquipBase goldEquipBase = new GoldEquipBase();
                goldEquipBase.Load(fileTable, rowIndex);
                result[rowIndex] = goldEquipBase;

                if(goldEquipBase.idSet > 0)
                {
                    if(Cache.Settings.Item.goldEquipSet.ContainsKey(goldEquipBase.idSet) == false)
                    {
                        Cache.Settings.Item.goldEquipSet[goldEquipBase.idSet] = new List<int>();
                    }

                    bool isExisting = false;

                    if(goldEquipBase.detail == (int)item.Defination.Detail.equip_ring)
                    {
                        // nhẫn gồm 2 món, chung detail id
                        // thượng giới: nhẫn trên
                        // hạ giới: nhẫn dưới

                        int ringCount = 0;

                        foreach (int setElementEntry in Cache.Settings.Item.goldEquipSet[goldEquipBase.idSet])
                        {
                            if(((setElementEntry % 100) == goldEquipBase.detail)
                                && ((++ringCount) >= 2))
                            {
                                isExisting = true;
                                break;
                            }
                        }
                    }
                    else
                    {
                        foreach (int setElementEntry in Cache.Settings.Item.goldEquipSet[goldEquipBase.idSet])
                        {
                            if ((setElementEntry % 100) == goldEquipBase.detail)
                            {
                                isExisting = true;
                                break;
                            }
                        }
                    }

                    if(isExisting == false)
                    {
                        Cache.Settings.Item.goldEquipSet[goldEquipBase.idSet].Add((rowIndex * 100) + goldEquipBase.detail);
                    }
                }
            }

            return result;
        }

        private static Dictionary<int, settings.item.GoldEquipBase> PlatinaEquipBaseParser(string filePath)
        {
            Dictionary<int, settings.item.GoldEquipBase> result = new Dictionary<int, GoldEquipBase>();
            resource.Table fileTable = Game.Resource(filePath).Get<resource.Table>();

            if (fileTable.IsEmpty())
            {
                UnityEngine.Debug.LogWarning(filePath);
                return result;
            }

            for (int rowIndex = 1; rowIndex < fileTable.RowCount; rowIndex++)
            {
                settings.item.GoldEquipBase platinaBase = new GoldEquipBase();
                platinaBase.Load(fileTable, rowIndex);
                result[rowIndex] = platinaBase;
            }

            return result;
        }

        private static Dictionary<string, Dictionary<int, Dictionary<int, settings.item.MagicattribBase>>> MagicAttribParser(string filePath)
        {
            // ["detail, series, position"] => [propType] => [level] => <...>
            Dictionary<string, Dictionary<int, Dictionary<int, settings.item.MagicattribBase>>> result = new Dictionary<string, Dictionary<int, Dictionary<int, MagicattribBase>>>();

            resource.Table fileTable = Game.Resource(filePath).Get<resource.Table>();

            if (fileTable.IsEmpty())
            {
                UnityEngine.Debug.LogError(filePath);
                return result;
            }

            for (int rowIndex = 1; rowIndex < fileTable.RowCount; rowIndex++)
            {
                item.MagicattribBase magicattribBase = new item.MagicattribBase();
                magicattribBase.Load(fileTable, rowIndex);

                List<string> keyList = magicattribBase.GetCacheKeyList();

                foreach (string keyEntry in keyList)
                {
                    if (result.ContainsKey(keyEntry) == false)
                    {
                        result[keyEntry] = new Dictionary<int, Dictionary<int, MagicattribBase>>();
                    }

                    if (result[keyEntry].ContainsKey(magicattribBase.propKind) == false)
                    {
                        result[keyEntry][magicattribBase.propKind] = new Dictionary<int, MagicattribBase>();
                    }

                    result[keyEntry][magicattribBase.propKind][magicattribBase.level] = magicattribBase;
                }
            }

            return result;
        }

        private static Dictionary<string, settings.item.EquipmentBase> EquipmentParser(List<string> pathList)
        {
            Dictionary<string, settings.item.EquipmentBase> result = new Dictionary<string, EquipmentBase>();

            foreach (string pathEntry in pathList)
            {
                resource.Table fileTable = Game.Resource(pathEntry).Get<resource.Table>();

                if (fileTable.IsEmpty())
                {
                    UnityEngine.Debug.LogError(pathEntry);
                    continue;
                }

                for (int rowIndex = 1; rowIndex < fileTable.RowCount; rowIndex++)
                {
                    item.EquipmentBase equipmentBase = new item.EquipmentBase();
                    equipmentBase.Load(fileTable, rowIndex);

                    result[equipmentBase.GetKeyGDPL()] = equipmentBase;
                    Cache.Settings.Item.equipmentBaseRowMapping[equipmentBase.GetDetailRowKey()] = equipmentBase;
                }
            }

            return result;
        }

        private static Dictionary<string, settings.item.EquipmentBase> MaskEquipParser(string filePath)
        {
            Dictionary<string, settings.item.EquipmentBase> result = new Dictionary<string, EquipmentBase>();

            resource.Table fileTable = Game.Resource(filePath).Get<resource.Table>();

            if (fileTable.IsEmpty())
            {
                UnityEngine.Debug.LogError(filePath);
                return result;
            }

            for (int rowIndex = 1; rowIndex < fileTable.RowCount; rowIndex++)
            {
                item.EquipmentBase equipmentBase = new item.EquipmentBase();
                equipmentBase.Load(fileTable, rowIndex);

                result[equipmentBase.GetKeyGDP()] = equipmentBase;
            }

            return result;
        }

        private static Dictionary<string, resource.Table> LoadTableList(List<string> _fileList)
        {
            Dictionary<string, resource.Table> result = new();

            foreach (string indexFile in _fileList)
            {
                string fullFilePath = indexFile;
                resource.Table fileTable = Game.Resource(fullFilePath).Get<resource.Table>();

                if (fileTable.IsEmpty())
                {
                    UnityEngine.Debug.LogError(fullFilePath);
                    continue;
                }

                result.Add(indexFile, fileTable);
            }

            return result;
        }

        private static Dictionary<string, Dictionary<int, int>> AppearanceParser(Dictionary<string, resource.Table> _fileTables)
        {
            Dictionary<string, Dictionary<int, int>> result = new();

            foreach (KeyValuePair<string, resource.Table> indexFileTable in _fileTables)
            {
                if (indexFileTable.Value.HeaderCount < ((int)mapping.settings.Item.HeaderIndexer.Appearance._count))
                {
                    UnityEngine.Debug.LogError(indexFileTable.Key);
                    continue;
                }

                result[indexFileTable.Key] = new();

                for (int indexRow = 0; indexRow < indexFileTable.Value.RowCount; indexRow++)
                {
                    result
                        [indexFileTable.Key]
                        [indexFileTable.Value.Get<int>((int)mapping.settings.Item.HeaderIndexer.Appearance.declarationLine, indexRow)]
                        =
                        indexFileTable.Value.Get<int>((int)mapping.settings.Item.HeaderIndexer.Appearance.npcResLine, indexRow);
                }
            }

            return result;
        }
    }
}
