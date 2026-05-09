
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace game.resource.settings.npcres.special
{
    class Getters
    {
        private static readonly Dictionary<string, int> WeaponActionColumnIndex = new()
        {
            { mapping.settings.NpcRes.WeaponAction.fightStand, 1 },
            { mapping.settings.NpcRes.WeaponAction.normalStand1, 2 },
            { mapping.settings.NpcRes.WeaponAction.normalStand2, 3 },
            { mapping.settings.NpcRes.WeaponAction.fightWalk, 4 },
            { mapping.settings.NpcRes.WeaponAction.normalWalk, 5 },
            { mapping.settings.NpcRes.WeaponAction.fightRun, 6 },
            { mapping.settings.NpcRes.WeaponAction.normalRun, 7 },
            { mapping.settings.NpcRes.WeaponAction.wound, 8 },
            { mapping.settings.NpcRes.WeaponAction.die, 9 },
            { mapping.settings.NpcRes.WeaponAction.attack1, 10 },
            { mapping.settings.NpcRes.WeaponAction.attack2, 11 },
            { mapping.settings.NpcRes.WeaponAction.magic, 12 },
            { mapping.settings.NpcRes.WeaponAction.sitDown, 13 },
            { mapping.settings.NpcRes.WeaponAction.junpFly, 14 },
        };

        private static int EquipTypeToTableRow(int equipType)
        {
            return equipType + 1;
        }

        private static List<string> DefaultPartGroup(string _partMember)
        {
            if (_partMember.CompareTo(NpcRes.PartGroup.head) == 0)
            {
                return new()
                {
                    NpcRes.Part.head,
                    NpcRes.Part.hair,
                };
            }

            if (_partMember.CompareTo(NpcRes.PartGroup.body) == 0)
            {
                return new()
                {
                    NpcRes.Part.shoulder,
                    NpcRes.Part.body,
                    NpcRes.Part.leftHand,
                    NpcRes.Part.rightHand,
                };
            }

            if (_partMember.CompareTo(NpcRes.PartGroup.weapon) == 0)
            {
                return new()
                {
                    NpcRes.Part.leftWeapon,
                    NpcRes.Part.rightWeapon,
                };
            }

            if (_partMember.CompareTo(NpcRes.PartGroup.horse) == 0)
            {
                return new()
                {
                    NpcRes.Part.horseFront,
                    NpcRes.Part.horseMiddle,
                    NpcRes.Part.horseBack,
                };
            }

            return new();
        }

        public static List<string> PartGroup(Dictionary<string, resource.Table> _tableHeaderMapping, string _partMember)
        {
            List<string> defaultPartGroup = DefaultPartGroup(_partMember);
            if (defaultPartGroup.Count > 0)
            {
                return defaultPartGroup;
            }

            if (_tableHeaderMapping.ContainsKey(mapping.settings.NpcRes.Kind.Header.partFileName) == false)
            {
                UnityEngine.Debug.LogError(mapping.settings.NpcRes.Kind.Header.partFileName);
                return new();
            }

            resource.Table partTable = _tableHeaderMapping[mapping.settings.NpcRes.Kind.Header.partFileName];
            List<string> rowPartList;
            bool hasFound;

            for (int indexRow = 0 + 1; indexRow < partTable.RowCount; indexRow++)
            {
                rowPartList = new();
                hasFound = false;

                for (int indexColumn = 0 + 2; indexColumn < partTable.HeaderCount; indexColumn++)
                {
                    string crossData = partTable.Get<string>(indexColumn, indexRow);

                    if (crossData.Length <= 0)
                    {
                        continue;
                    }

                    rowPartList.Add(crossData);

                    if (crossData.CompareTo(_partMember) == 0)
                    {
                        hasFound = true;
                    }
                }

                if (hasFound)
                {
                    return rowPartList;
                }
            }

            return new();
        }

        public static List<string> PartGroup(string _specialType, string _partMember)
        {
            if (special.Validation.IsMainMan(_specialType))
            {
                return special.Getters.PartGroup(resource.Cache.Settings.NpcRes.mainManTableMapping, _partMember);
            }

            if (special.Validation.IsMainLady(_specialType))
            {
                return special.Getters.PartGroup(resource.Cache.Settings.NpcRes.mainLadyTableMapping, _partMember);
            }

            return new();
        }

        public static string AnimationName(Dictionary<string, resource.Table> _tableMapping, bool _riding, string _weaponAction, int _rowIndex)
        {
            resource.Table weaponActionTable;

            if (_riding)
            {
                if (_tableMapping.ContainsKey(mapping.settings.NpcRes.Kind.Header.weaponActionTab2))
                {
                    weaponActionTable = _tableMapping[mapping.settings.NpcRes.Kind.Header.weaponActionTab2];
                }
                else
                {
                    UnityEngine.Debug.LogError(mapping.settings.NpcRes.Kind.Header.weaponActionTab2);
                    return string.Empty;
                }
            }
            else
            {
                if (_tableMapping.ContainsKey(mapping.settings.NpcRes.Kind.Header.weaponActionTab1))
                {
                    weaponActionTable = _tableMapping[mapping.settings.NpcRes.Kind.Header.weaponActionTab1];
                }
                else
                {
                    UnityEngine.Debug.LogError(mapping.settings.NpcRes.Kind.Header.weaponActionTab1);
                    return string.Empty;
                }
            }

            int rowIndex = EquipTypeToTableRow(_rowIndex);
            string animationName = weaponActionTable.Get<string>(_weaponAction, rowIndex);
            if (!string.IsNullOrEmpty(animationName))
            {
                return animationName;
            }

            if (WeaponActionColumnIndex.TryGetValue(_weaponAction, out int columnIndex))
            {
                animationName = weaponActionTable.Get<string>(columnIndex, rowIndex);
                if (!string.IsNullOrEmpty(animationName))
                {
                    return animationName;
                }
            }

            UnityEngine.Debug.LogWarning("NpcRes special animation missing. action=" + _weaponAction +
                                         " equipType=" + _rowIndex +
                                         " tableRow=" + rowIndex +
                                         " riding=" + _riding);
            return string.Empty;
        }

        public static string AnimationName(string _specialType, bool _riding, string _weaponAction, int _rowIndex)
        {
            if (special.Validation.IsMainMan(_specialType))
            {
                return special.Getters.AnimationName(resource.Cache.Settings.NpcRes.mainManTableMapping, _riding, _weaponAction, _rowIndex);
            }
            else if (special.Validation.IsMainLady(_specialType))
            {
                return special.Getters.AnimationName(resource.Cache.Settings.NpcRes.mainLadyTableMapping, _riding, _weaponAction, _rowIndex);
            }

            return string.Empty;
        }

        public static int PartOrder(Dictionary<string, resource.Ini> _iniMapping, string _partName, string _animationName, int _direction)
        {
            if (_iniMapping.ContainsKey(mapping.settings.NpcRes.Kind.Header.actionRenderOrderTab) == false)
            {
                UnityEngine.Debug.LogError(mapping.settings.NpcRes.Kind.Header.actionRenderOrderTab);
                return 0;
            }

            resource.Ini orderIni = _iniMapping[mapping.settings.NpcRes.Kind.Header.actionRenderOrderTab];
            string orderString = orderIni.Get<string>(_animationName, mapping.settings.NpcRes.ActionRenderOrderTab.Key.prefix + "" + _direction);

            if (orderString == string.Empty)
            {
                orderString = orderIni.Get<string>(mapping.settings.NpcRes.ActionRenderOrderTab.Section.Default, mapping.settings.NpcRes.ActionRenderOrderTab.Key.prefix + "" + _direction);
            }

            string[] orderVectorString = orderString.Split(',');

            if (orderVectorString.Length <= 0)
            {
                return 0;
            }

            int result = 0;
            int nextOrderNumber = 0;
            Dictionary<int, string> allPartId = settings.npcres.special.Part.AllPartId();

            foreach (string indexPartId in orderVectorString)
            {
                nextOrderNumber++;

                string partIdString = Regex.Replace(indexPartId, "[^0-9-]", string.Empty);
                int partId = int.Parse(partIdString);
                if (allPartId.ContainsKey(partId) == false)
                {
                    continue;
                }

                if (allPartId[partId].CompareTo(_partName) != 0)
                {
                    continue;
                }

                result = nextOrderNumber;
                break;
            }

            return result;
        }

        public static int PartOrder(string _specialType, string _partName, string _animationName, int _direction)
        {
            if (special.Validation.IsMainMan(_specialType))
            {
                return special.Getters.PartOrder(resource.Cache.Settings.NpcRes.mainManIniMapping, _partName, _animationName, _direction);
            }
            else if (special.Validation.IsMainLady(_specialType))
            {
                return special.Getters.PartOrder(resource.Cache.Settings.NpcRes.mainLadyIniMapping, _partName, _animationName, _direction);
            }

            return 0;
        }

        public static settings.npcres.Structures.PartSprInfo PartSprInfo(
            Dictionary<string, resource.Table> _tableMapping,
            Dictionary<string, string> _headerMapping,
            Dictionary<string, resource.Table> _propertiesMapping,
            string _partName,
            string _animationName,
            int _rowIndex
        )
        {
            if (_tableMapping.ContainsKey(_partName) == false)
            {
                return new();
            }

            resource.Table sprTable = _tableMapping[_partName];
            int rowIndex = EquipTypeToTableRow(_rowIndex);
            string sprFile = sprTable.Get<string>(_animationName, rowIndex);

            if (sprFile.Length <= 0)
            {
                return new();
            }

            while (true)
            {
                if (sprFile.Length >= 9)
                {
                    if (sprFile[..9].CompareTo("..\\woman\\") == 0)
                    {
                        sprFile = sprFile.Remove(0, 3);
                        sprFile = sprFile.Insert(0, mapping.settings.NpcRes.Properties.sprFolderPrefix);
                        break;
                    }
                }

                if (sprFile.Length >= 7)
                {
                    if (sprFile[..7].CompareTo("..\\man\\") == 0)
                    {
                        sprFile = sprFile.Remove(0, 3);
                        sprFile = sprFile.Insert(0, mapping.settings.NpcRes.Properties.sprFolderPrefix);
                        break;
                    }
                }

                if (_headerMapping.ContainsKey(mapping.settings.NpcRes.Kind.Header.resFilePath) == true)
                {
                    sprFile = sprFile.Insert(0, "\\");
                    sprFile = sprFile.Insert(0, _headerMapping[mapping.settings.NpcRes.Kind.Header.resFilePath]);
                    sprFile = sprFile.Insert(0, "\\");
                }

                break;
            }

            if (_propertiesMapping.ContainsKey(_partName) == false)
            {
                settings.npcres.Structures.PartSprInfo fallbackResult = new()
                {
                    sprFullPath = sprFile,
                };

                FillMissingSprProperties(ref fallbackResult);
                return fallbackResult;
            }

            resource.Table sprPropertiesTable = _propertiesMapping[_partName];
            string sprPropertiesString = sprPropertiesTable.Get<string>(_animationName, rowIndex);
            settings.npcres.Structures.PartSprInfo result = new()
            {
                sprFullPath = sprFile,
            };

            string[] sprPropertiesVector = sprPropertiesString.Split(',');

            if (sprPropertiesVector.Length >= mapping.settings.NpcRes.SprPropertiesIndexer.frameCount + 1)
            {
                result.frameCount = ushort.Parse("0" + Regex.Replace(sprPropertiesVector[mapping.settings.NpcRes.SprPropertiesIndexer.frameCount], "[^0-9-]", string.Empty));
            }

            if (sprPropertiesVector.Length >= mapping.settings.NpcRes.SprPropertiesIndexer.directionCount + 1)
            {
                result.directionCount = int.Parse("0" + Regex.Replace(sprPropertiesVector[mapping.settings.NpcRes.SprPropertiesIndexer.directionCount], "[^0-9-]", string.Empty));
            }

            if (sprPropertiesVector.Length >= mapping.settings.NpcRes.SprPropertiesIndexer.intervalRatio + 1)
            {
                result.intervalRatio = int.Parse("0" + Regex.Replace(sprPropertiesVector[mapping.settings.NpcRes.SprPropertiesIndexer.intervalRatio], "[^0-9-]", string.Empty));
            }

            FillMissingSprProperties(ref result);
            return result;
        }

        public static settings.npcres.Structures.PartSprInfo PartSprInfo(
            string _specialType,
            string _partName,
            string _animationName,
            int _rowIndex
        )
        {
            if (npcres.special.Validation.IsMainMan(_specialType))
            {
                return special.Getters.PartSprInfo(
                    resource.Cache.Settings.NpcRes.mainManTableMapping,
                    resource.Cache.Settings.NpcRes.Kind.mainManHeaderValueMapping,
                    resource.Cache.Settings.NpcRes.mainManPartPropertiesTableMapping,
                    _partName,
                    _animationName,
                    _rowIndex
                );
            }

            if (npcres.special.Validation.IsMainLady(_specialType))
            {
                return special.Getters.PartSprInfo(
                    resource.Cache.Settings.NpcRes.mainLadyTableMapping,
                    resource.Cache.Settings.NpcRes.Kind.mainLadyHeaderValueMapping,
                    resource.Cache.Settings.NpcRes.mainLadyPartPropertiesTableMapping,
                    _partName,
                    _animationName,
                    _rowIndex
                );
            }

            return new();
        }

        private static void FillMissingSprProperties(ref settings.npcres.Structures.PartSprInfo sprInfo)
        {
            if (string.IsNullOrEmpty(sprInfo.sprFullPath))
            {
                return;
            }

            if (sprInfo.frameCount > 0 && sprInfo.directionCount > 0 && sprInfo.intervalRatio > 0)
            {
                return;
            }

            try
            {
                resource.SPR.Info realSprInfo = Game.Resource(sprInfo.sprFullPath).Get<resource.SPR.Info>();

                if (realSprInfo != null)
                {
                    if (sprInfo.frameCount <= 0)
                    {
                        sprInfo.frameCount = realSprInfo.frameCount;
                    }

                    if (sprInfo.directionCount <= 0)
                    {
                        sprInfo.directionCount = realSprInfo.directionCount;
                    }

                    if (sprInfo.intervalRatio <= 0)
                    {
                        sprInfo.intervalRatio = realSprInfo.interval;
                    }
                }
            }
            catch (System.Exception exception)
            {
                UnityEngine.Debug.LogWarning("NpcRes special SPR info fallback failed: " + sprInfo.sprFullPath + " " + exception.Message);
            }

            if (sprInfo.frameCount > 0 && sprInfo.directionCount > 0 && sprInfo.intervalRatio <= 0)
            {
                sprInfo.intervalRatio = 1;
            }
        }

        public static npcres.Structures.PartAnimation PartAnimation(string _specialType, string _animationName, string _partName, int _direction, int _rowIndex, int _speed)
        {
            if (_rowIndex < 0)
            {
                return new();
            }

            settings.npcres.Structures.PartSprInfo partSprInfo = special.Getters.PartSprInfo(_specialType, _partName, _animationName, _rowIndex);

            if (partSprInfo.frameCount <= 0 || partSprInfo.directionCount <= 0)
            {
                return new();
            }

            if (partSprInfo.directionCount <= 0)
            {
                return new();
            }

            npcres.Structures.PartAnimation result = new();
            result.sprPath = partSprInfo.sprFullPath;
            result.framePerDirection = partSprInfo.frameCount / partSprInfo.directionCount;
            if (result.framePerDirection <= 0)
            {
                return new();
            }

            result.frameBegin = (ushort)(result.framePerDirection * (_direction - 1));
            result.frameEnd = (ushort)(result.frameBegin + result.framePerDirection - 1);
            result.framePerSeconds = _speed * partSprInfo.intervalRatio;
            result.layerOrder = special.Getters.PartOrder(_specialType, _partName, _animationName, _direction);

            return result;
        }

        public static npcres.Structures.PartAnimation ShadowAnimation(string _specialType, string _animationName, int _direction, int _speed)
        {
            settings.npcres.Structures.PartSprInfo partSprInfo;

            if (npcres.special.Validation.IsMainMan(_specialType))
            {
                if (resource.Cache.Settings.NpcRes.Shadow.mainManAnimationMapping.ContainsKey(_animationName))
                {
                    partSprInfo = resource.Cache.Settings.NpcRes.Shadow.mainManAnimationMapping[_animationName];
                }
                else
                {
                    return new();
                }
            }
            else if (npcres.special.Validation.IsMainLady(_specialType))
            {
                if (resource.Cache.Settings.NpcRes.Shadow.mainLadyAnimationMapping.ContainsKey(_animationName))
                {
                    partSprInfo = resource.Cache.Settings.NpcRes.Shadow.mainLadyAnimationMapping[_animationName];
                }
                else
                {
                    return new();
                }
            }
            else
            {
                return new();
            }

            if (partSprInfo.frameCount <= 0 || partSprInfo.directionCount <= 0)
            {
                return new();
            }

            npcres.Structures.PartAnimation result = new();
            result.sprPath = partSprInfo.sprFullPath;
            result.framePerDirection = partSprInfo.frameCount / partSprInfo.directionCount;
            if (result.framePerDirection <= 0)
            {
                return new();
            }

            result.frameBegin = (ushort)(result.framePerDirection * (_direction - 1));
            result.frameEnd = (ushort)(result.frameBegin + result.framePerDirection - 1);
            result.framePerSeconds = _speed * partSprInfo.intervalRatio;
            result.layerOrder = special.Getters.PartOrder(_specialType, mapping.settings.NpcRes.Shadow.partName, _animationName, _direction);

            return result;
        }

        public static Dictionary<string, npcres.Structures.PartAnimation> PartGroupAnimation(string _specialType, string _animationName, string _partName, int _direction, int _rowIndex, int _speed)
        {
            List<string> partGrouping = special.Getters.PartGroup(_specialType, _partName);
            Dictionary<string, npcres.Structures.PartAnimation> result = new();

            foreach (string part in partGrouping)
            {
                result[part] = special.Getters.PartAnimation(_specialType, _animationName, part, _direction, _rowIndex, _speed);
            }

            return result;
        }
    }
}
