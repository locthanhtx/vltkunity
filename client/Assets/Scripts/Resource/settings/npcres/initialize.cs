
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace game.resource.settings.npcres
{
    class Initialize
    {
        private static readonly Dictionary<string, int> KindColumnIndex = new()
        {
            { mapping.settings.NpcRes.Kind.Header.characterName, 0 },
            { mapping.settings.NpcRes.Kind.Header.characterType, 1 },
            { mapping.settings.NpcRes.Kind.Header.resFilePath, 2 },
            { mapping.settings.NpcRes.Kind.Header.partFileName, 3 },
            { mapping.settings.NpcRes.Kind.Header.weaponActionTab1, 4 },
            { mapping.settings.NpcRes.Kind.Header.weaponActionTab2, 5 },
            { mapping.settings.NpcRes.Kind.Header.actionRenderOrderTab, 6 },
            { mapping.settings.NpcRes.Kind.Header.head, 7 },
            { mapping.settings.NpcRes.Kind.Header.hair, 8 },
            { mapping.settings.NpcRes.Kind.Header.shoulder, 9 },
            { mapping.settings.NpcRes.Kind.Header.body, 10 },
            { mapping.settings.NpcRes.Kind.Header.leftHand, 11 },
            { mapping.settings.NpcRes.Kind.Header.rightHand, 12 },
            { mapping.settings.NpcRes.Kind.Header.leftWeapon, 13 },
            { mapping.settings.NpcRes.Kind.Header.rightWeapon, 14 },
            { mapping.settings.NpcRes.Kind.Header.horseFront, 15 },
            { mapping.settings.NpcRes.Kind.Header.horseMiddle, 16 },
            { mapping.settings.NpcRes.Kind.Header.horseBack, 17 },
            { mapping.settings.NpcRes.Kind.Header.mantle, 18 },
        };

        public Initialize()
        {
            npcres.AttribModify.Initialize();

            resource.Table kindTable = Game.Resource(mapping.settings.NpcRes.Kind.filePath).Get<resource.Table>();

            if(kindTable.IsEmpty())
            {
                UnityEngine.Debug.LogError(mapping.settings.NpcRes.Kind.filePath);
                return;
            }

            resource.Cache.Settings.NpcRes.Kind.mainManHeaderValueMapping = Initialize.GetSpecialCharacterMapping(kindTable, mapping.settings.NpcRes.Kind.CharacterName.mainMan);
            resource.Cache.Settings.NpcRes.Kind.mainLadyHeaderValueMapping = Initialize.GetSpecialCharacterMapping(kindTable, mapping.settings.NpcRes.Kind.CharacterName.mainLady);

            resource.Cache.Settings.NpcRes.mainManTableMapping = Initialize.GetSpecialCharacterTable(resource.Cache.Settings.NpcRes.Kind.mainManHeaderValueMapping);
            resource.Cache.Settings.NpcRes.mainLadyTableMapping = Initialize.GetSpecialCharacterTable(resource.Cache.Settings.NpcRes.Kind.mainLadyHeaderValueMapping);

            resource.Cache.Settings.NpcRes.mainManPartPropertiesTableMapping = Initialize.GetSpecialPartPropertiesTable(resource.Cache.Settings.NpcRes.Kind.mainManHeaderValueMapping);
            resource.Cache.Settings.NpcRes.mainLadyPartPropertiesTableMapping = Initialize.GetSpecialPartPropertiesTable(resource.Cache.Settings.NpcRes.Kind.mainLadyHeaderValueMapping);

            resource.Cache.Settings.NpcRes.mainManIniMapping = Initialize.GetSpecialCharacterIni(resource.Cache.Settings.NpcRes.Kind.mainManHeaderValueMapping);
            resource.Cache.Settings.NpcRes.mainLadyIniMapping = Initialize.GetSpecialCharacterIni(resource.Cache.Settings.NpcRes.Kind.mainLadyHeaderValueMapping);

            game.resource.Table shadowTable = Game.Resource(mapping.settings.NpcRes.Shadow.filePath).Get<resource.Table>();

            if(shadowTable.IsEmpty())
            {
                UnityEngine.Debug.LogError(mapping.settings.NpcRes.Shadow.filePath);
            }
            else
            {
                resource.Cache.Settings.NpcRes.Shadow.mainManAnimationMapping = Initialize.GetSpecialShadowAnimationMapping(kindTable, shadowTable, mapping.settings.NpcRes.Kind.CharacterName.mainMan);
                resource.Cache.Settings.NpcRes.Shadow.mainLadyAnimationMapping = Initialize.GetSpecialShadowAnimationMapping(kindTable, shadowTable, mapping.settings.NpcRes.Kind.CharacterName.mainLady);
            }

            resource.Cache.Settings.NpcRes.NormalNpc.animationMapping = GetNormalNpcMapping(kindTable);
            resource.Cache.Settings.NpcRes.textures = new Dictionary<string, skill.texture.SprCache.Data>();
        }

        private static Dictionary<string, string> GetSpecialCharacterMapping(resource.Table _kindTable, string _characterName)
        {
            Dictionary<string, string> result = new();

            List<string> headerKey = _kindTable.GetHeaderKeyList();
            int rowIndex = FindCharacterRowIndex(_kindTable, _characterName);

            if (rowIndex <= 0)
            {
                UnityEngine.Debug.LogError("NpcRes character row missing: " + _characterName);
                return result;
            }

            foreach (string key in headerKey)
            {
                result.Add(key, _kindTable.Get<string>(key, rowIndex));
            }

            foreach (KeyValuePair<string, int> canonicalColumn in KindColumnIndex)
            {
                string value = _kindTable.Get<string>(canonicalColumn.Value, rowIndex);
                if (string.IsNullOrEmpty(value))
                {
                    continue;
                }

                result[canonicalColumn.Key] = value;
            }

            return result;
        }

        private static int FindCharacterRowIndex(resource.Table table, string characterName)
        {
            int rowIndex = table.FindRowIndex(mapping.settings.NpcRes.Kind.Header.characterName, characterName);
            if (rowIndex > 0)
            {
                return rowIndex;
            }

            for (int index = 1; index < table.RowCount; index++)
            {
                if (table.Get<string>(0, index).CompareTo(characterName) == 0)
                {
                    return index;
                }
            }

            if (characterName.CompareTo(mapping.settings.NpcRes.Kind.CharacterName.mainMan) == 0 && table.RowCount > 1)
            {
                return 1;
            }

            if (characterName.CompareTo(mapping.settings.NpcRes.Kind.CharacterName.mainLady) == 0 && table.RowCount > 2)
            {
                return 2;
            }

            return -1;
        }

        private static string GetKindTableValue(resource.Table kindTable, string canonicalHeader, int rowIndex)
        {
            string value = kindTable.Get<string>(canonicalHeader, rowIndex);
            if (!string.IsNullOrEmpty(value))
            {
                return value;
            }

            if (KindColumnIndex.TryGetValue(canonicalHeader, out int columnIndex))
            {
                return kindTable.Get<string>(columnIndex, rowIndex);
            }

            return string.Empty;
        }

        private static Dictionary<string, resource.Table> GetSpecialCharacterTable(Dictionary<string, string> _headerMapping)
        {
            Dictionary<string, resource.Table> result = new();
            List<string> allTabFileHeader = settings.npcres.special.Part.AllTabFileList();

            foreach(string indexTabFileHeader in allTabFileHeader)
            {
                if (IsOptionalSpecialPartTable(indexTabFileHeader))
                {
                    continue;
                }

                if(_headerMapping.ContainsKey(indexTabFileHeader) == false)
                {
                    UnityEngine.Debug.LogError(indexTabFileHeader);
                    continue;
                }

                string tableFilePath = mapping.settings.NpcRes.directoryPath + _headerMapping[indexTabFileHeader];
                resource.Table table = Game.Resource(tableFilePath).Get<resource.Table>();

                if (table.IsEmpty())
                {
                    UnityEngine.Debug.LogError(tableFilePath);
                    continue;
                }

                result.Add(indexTabFileHeader, table);
            }

            return result;
        }

        private static bool IsOptionalSpecialPartTable(string tableHeader)
        {
            return tableHeader.CompareTo(mapping.settings.NpcRes.Kind.Header.shoulder) == 0 ||
                   tableHeader.CompareTo(mapping.settings.NpcRes.Kind.Header.mantle) == 0;
        }

        private static Dictionary<string, resource.Ini> GetSpecialCharacterIni(Dictionary<string, string> _headerMapping)
        {
            Dictionary<string, resource.Ini> result = new();
            List<string> allIniFileHeader = settings.npcres.special.Part.AllIniFileList();

            foreach(string indexIniFileHeader in allIniFileHeader)
            {
                if (_headerMapping.ContainsKey(indexIniFileHeader) == false)
                {
                    UnityEngine.Debug.LogError(indexIniFileHeader);
                    continue;
                }

                string iniFilePath = mapping.settings.NpcRes.directoryPath + _headerMapping[indexIniFileHeader];
                resource.Ini ini = Game.Resource(iniFilePath).Get<resource.Ini>();

                if (ini.IsEmpty())
                {
                    UnityEngine.Debug.LogError(iniFilePath);
                    continue;
                }

                result.Add(indexIniFileHeader, ini);
            }

            return result;
        }

        private static Dictionary<string, resource.Table> GetSpecialPartPropertiesTable(Dictionary<string, string> _headerMapping)
        {
            Dictionary<string, resource.Table> result = new();
            List<string> partElements = resource.settings.npcres.special.Part.AllPartList();

            foreach(string partElement in partElements)
            {
                if(_headerMapping.ContainsKey(partElement) == false)
                {
                    continue;
                }

                string specialFilePath = mapping.settings.NpcRes.directoryPath + _headerMapping[partElement];
                string tabFileExtension = mapping.settings.NpcRes.Properties.tabFileExtension;

                if (specialFilePath[^tabFileExtension.Length..].CompareTo(tabFileExtension) != 0)
                {
                    UnityEngine.Debug.LogError(specialFilePath);
                    continue;
                }

                specialFilePath = specialFilePath.Insert(specialFilePath.Length - tabFileExtension.Length, mapping.settings.NpcRes.Properties.sprPropertiesSuffix);
                result[partElement] = Game.Resource(specialFilePath).Get<resource.Table>();
            }

            return result;
        }

        private static Dictionary<string, settings.npcres.Structures.PartSprInfo> GetSpecialShadowAnimationMapping(resource.Table _kindTable, game.resource.Table _shadowTable, string _specialName)
        {
            Dictionary<string, string> characterMapping = Initialize.GetSpecialCharacterMapping(_kindTable, _specialName);
            characterMapping.TryGetValue(mapping.settings.NpcRes.Kind.Header.resFilePath, out string resourceDirectory);

            if(string.IsNullOrEmpty(resourceDirectory))
            {
                UnityEngine.Debug.LogError(_specialName);
                return new();
            }

            int specialRowIndex = FindCharacterRowIndex(_shadowTable, _specialName);

            if(specialRowIndex < 0)
            {
                UnityEngine.Debug.LogError(_specialName);
                return new();
            }

            Dictionary<string, settings.npcres.Structures.PartSprInfo> result = new();

            for (int headIndex = 1; headIndex < _shadowTable.HeaderCount;)
            {
                string ainmationName = _shadowTable.GetHeaderKey(headIndex);
                string sprFileName = _shadowTable.Get<string>(headIndex, specialRowIndex);
                headIndex++;
                string sprProperties = _shadowTable.Get<string>(headIndex, specialRowIndex);
                headIndex++;

                string[] sprPropertiesSplited = sprProperties.Split(',');
                ushort sprFrameCount = ushort.Parse(Regex.Replace(sprPropertiesSplited[0], "[^0-9-]", string.Empty));
                int sprDirections = int.Parse(Regex.Replace(sprPropertiesSplited[1], "[^0-9-]", string.Empty));
                int sprInterval = int.Parse(Regex.Replace(sprPropertiesSplited[2], "[^0-9-]", string.Empty));

                settings.npcres.Structures.PartSprInfo newSprPartInfo = new settings.npcres.Structures.PartSprInfo();
                newSprPartInfo.sprFullPath = "\\" + resourceDirectory + "\\" + sprFileName;
                newSprPartInfo.frameCount = sprFrameCount;
                newSprPartInfo.directionCount = sprDirections;
                newSprPartInfo.intervalRatio = sprInterval;

                result[ainmationName] = newSprPartInfo;
            }

            return result;
        }

        private static Dictionary<string, Dictionary<string, resource.Cache.Settings.NpcRes.NormalNpc.PartInfo>> GetNormalNpcMapping(resource.Table _kindTable)
        {
            Dictionary<string, Dictionary<string, resource.Cache.Settings.NpcRes.NormalNpc.PartInfo>> result = new();
            resource.Table npcActionTable = Game.Resource(mapping.settings.NpcRes.NormalNpc.sprActionPath).Get<resource.Table>();
            resource.Table npcPropertiesTable = Game.Resource(mapping.settings.NpcRes.NormalNpc.sprPropertiesPath).Get<resource.Table>();

            if(npcActionTable.IsEmpty())
            {
                UnityEngine.Debug.LogError(mapping.settings.NpcRes.NormalNpc.sprActionPath);
                return result;
            }

            if(npcPropertiesTable.IsEmpty())
            {
                UnityEngine.Debug.LogError(mapping.settings.NpcRes.NormalNpc.sprPropertiesPath);
                return result;
            }

            Dictionary<string, int> actionKeyIndexer = new Dictionary<string, int>();
            Dictionary<string, int> propertiesKeyIndexer = new Dictionary<string, int>();

            for(int rowIndex = 1; rowIndex < npcActionTable.RowCount; rowIndex++)
            {
                actionKeyIndexer[npcActionTable.Get<string>(mapping.settings.NpcRes.NormalNpc.Header.npcList, rowIndex)] = rowIndex;
            }

            for(int rowIndex = 1; rowIndex < npcPropertiesTable.RowCount; rowIndex++)
            {
                propertiesKeyIndexer[npcPropertiesTable.Get<string>(mapping.settings.NpcRes.NormalNpc.Header.npcList, rowIndex)] = rowIndex;
            }

            for (int rowIndex = 3; rowIndex < _kindTable.RowCount; rowIndex++)
            {
                string characterName = GetKindTableValue(_kindTable, mapping.settings.NpcRes.Kind.Header.characterName, rowIndex);
                string sprDirectoryPath = GetKindTableValue(_kindTable, mapping.settings.NpcRes.Kind.Header.resFilePath, rowIndex);

                if(actionKeyIndexer.ContainsKey(characterName) == false)
                {
                    continue;
                }

                if(propertiesKeyIndexer.ContainsKey(characterName) == false)
                {
                    continue;
                }

                int actionRowIndex = actionKeyIndexer[characterName];
                int propertiesIndex = propertiesKeyIndexer[characterName];

                if(sprDirectoryPath.StartsWith('\\') == false)
                {
                    sprDirectoryPath = "\\" + sprDirectoryPath;
                }

                if(sprDirectoryPath.EndsWith('\\') == false)
                {
                    sprDirectoryPath += "\\";
                }

                result[characterName] = new();

                foreach (KeyValuePair<string, int> actionHeaderPair in npcActionTable.GetHeaderKeyIndexPair())
                {
                    if(actionHeaderPair.Key.CompareTo(mapping.settings.NpcRes.NormalNpc.Header.npcList) == 0)
                    {
                        continue;
                    }

                    string sprName = npcActionTable.Get<string>(actionHeaderPair.Key, actionRowIndex);
                    string sprPropertiesLiteral = npcPropertiesTable.Get<string>(actionHeaderPair.Key, propertiesIndex);

                    if(sprName == string.Empty
                        || sprPropertiesLiteral == string.Empty)
                    {
                        continue;
                    }

                    string[] sprPropertiesSplited = sprPropertiesLiteral.Split(',');

                    resource.Cache.Settings.NpcRes.NormalNpc.PartInfo newPartInfo = new();
                    newPartInfo.fullBody.sprFullPath = sprDirectoryPath + sprName;
                    newPartInfo.fullBody.frameCount = sprPropertiesSplited.Length >= 1 ? ushort.Parse(Regex.Replace(sprPropertiesSplited[0], "[^0-9-]", string.Empty)) : (ushort)0;
                    newPartInfo.fullBody.directionCount = sprPropertiesSplited.Length >= 2 ? int.Parse(Regex.Replace(sprPropertiesSplited[1], "[^0-9-]", string.Empty)) : 0;
                    newPartInfo.fullBody.intervalRatio = sprPropertiesSplited.Length >= 3 ? int.Parse(Regex.Replace(sprPropertiesSplited[2], "[^0-9-]", string.Empty)) : 0;

                    newPartInfo.shadow.sprFullPath = sprDirectoryPath + sprName.Insert(sprName.Length - mapping.settings.NpcRes.Properties.sprFileExtension.Length, mapping.settings.NpcRes.NormalNpc.shadowSuffix);
                    newPartInfo.shadow.frameCount = newPartInfo.fullBody.frameCount;
                    newPartInfo.shadow.directionCount = newPartInfo.fullBody.directionCount;
                    newPartInfo.shadow.intervalRatio = newPartInfo.fullBody.intervalRatio;

                    result[characterName][actionHeaderPair.Key] = newPartInfo;
                }
            }

            return result;
        }
    }
}
