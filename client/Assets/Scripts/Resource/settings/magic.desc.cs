using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace game.resource.settings
{
    public class MagicDesc
    {
        private static readonly string[] MagicAttribKeys = @"
skill_begin
skill_cost_v
skill_costtype_v
skill_mintimepercast_v
skill_misslenum_v
skill_misslesform_v
skill_param1_v
skill_param2_v
skill_attackradius
skill_mintimepercastonhorse_v
skill_skillexp_v
skill_desc
skill_eventskilllevel
skill_end
missle_begin
missle_movekind_v
missle_speed_v
missle_lifetime_v
missle_height_v
missle_damagerange_v
missle_radius_v
missle_missrate
missle_hitcount
missle_reserve3
missle_reserve4
missle_reserve5
missle_end
item_begin
weapondamagemin_v
weapondamagemax_v
armordefense_v
durability_v
requirestr
requiredex
requirevit
requireeng
requirelevel
requireseries
requiresex
requiremenpai
weapondamageenhance_p
armordefenseenhance_p
requirementreduce_p
indestructible_b
item_nouser
item_needskill
item_needreborn
item_needtongban
item_needbangzhu
item_needcity
item_noseries
item_reserve8
item_reserve9
item_reserve10
item_end
damage_begin
attackrating_v
attackrating_p
ignoredefense_p
physicsdamage_v
colddamage_v
firedamage_v
lightingdamage_v
poisondamage_v
magicdamage_v
physicsenhance_p
steallife_p
stealmana_p
stealstamina_p
knockback_p
deadlystrike_p
fatallystrike_p
stun_p
damage_reserve1
damage_reserve2
damage_reserve3
damage_reserve4
damage_reserve5
damage_reserve6
damage_reserve7
addzhuabu_v
autoattackskill
seriesdamage_p
damage_end
normal_begin
lifemax_v
lifemax_p
life_v
lifereplenish_v
manamax_v
manamax_p
mana_v
manareplenish_v
staminamax_v
staminamax_p
stamina_v
staminareplenish_v
strength_v
dexterity_v
vitality_v
energy_v
poisonres_p
fireres_p
lightingres_p
physicsres_p
coldres_p
freezetimereduce_p
burntimereduce_p
poisontimereduce_p
poisondamagereduce_v
stuntimereduce_p
fastwalkrun_p
visionradius_p
fasthitrecover_v
allres_p
attackspeed_v
castspeed_v
meleedamagereturn_v
meleedamagereturn_p
rangedamagereturn_v
rangedamagereturn_p
addphysicsdamage_v
addfiredamage_v
addcolddamage_v
addlightingdamage_v
addpoisondamage_v
addphysicsdamage_p
slowmissle_b
changecamp_b
physicsarmor_v
coldarmor_v
firearmor_v
poisonarmor_v
lightingarmor_v
damagetomana_p
lucky_v
steallifeenhance_p
stealmanaenhance_p
stealstaminaenhance_p
allskill_v
metalskill_v
woodskill_v
waterskill_v
fireskill_v
earthskill_v
knockbackenhance_p
deadlystrikeenhance_p
stunenhance_p
badstatustimereduce_v
manashield_p
adddefense_v
adddefense_p
fatallystrikeenhance_p
lifepotion_v
manapotion_v
physicsresmax_p
coldresmax_p
fireresmax_p
lightingresmax_p
poisonresmax_p
allresmax_p
coldenhance_p
fireenhance_p
lightingenhance_p
poisonenhance_p
magicenhance_p
attackratingenhance_v
attackratingenhance_p
addphysicsmagic_v
addcoldmagic_v
addfiremagic_v
addlightingmagic_v
addpoisonmagic_v
fatallystrikeres_p
addskilldamage1
addskilldamage2
expenhance_p
addskilldamage3
addskilldamage4
addskilldamage5
addskilldamage6
dynamicmagicshield_v
addstealfeatureskill
lifereplenish_p
ignoreskill_p
poisondamagereturn_v
poisondamagereturn_p
returnskill_p
autoreplyskill
mintimepercastonhorse_v
poison2decmana_p
skill_appendskill
hide
clearnegativestate
returnres_p
dec_percasttimehorse
dec_percasttime
enhance_autoskill
enhance_life_p
enhance_life_v
enhance_711_auto
enhance_714_auto
enhance_717_auto
enhance_723_miss_p
nomagic
skill_collideevent
skill_vanishedevent
skill_startevent
skill_flyevent
block_rate
enhancehit_rate
anti_block_rate
anti_enhancehit_rate
sorbdamage_p
anti_poisonres_p
anti_fireres_p
anti_lightingres_p
anti_physicsres_p
anti_coldres_p
not_add_pkvalue_p
add_boss_damage
five_elements_enhance_v
five_elements_resist_v
skill_enhance
anti_allres_p
add_alldamage_p
auto_Revive_rate
addphysicsmagic_p
addcreatnpc_v
reduceskillcd1
reduceskillcd2
reduceskillcd3
clearallcd
addblockrate
walkrunshadow
returnskill2enemy
manatoskill_enhance
add_alldamage_v
addskilldamage7
ignoreattacrating_v
alljihuo_v
addexp_v
doscript_v
me2metaldamage_p
metal2medamage_p
me2wooddamage_p
wood2medamage_p
me2waterdamage_p
water2medamage_p
me2firedamage_p
fire2medamage_p
me2earthdamage_p
earth2medamage_p
manareplenish_p
fasthitrecover_p
stuntrank_p
sorbdamage_v
creatstatus_v
randmove
addbaopoisondmax_p
dupotion_v
npcallattackSpeed_v
eqaddskill_v
autodeathskill
autorescueskill
staticmagicshield_p
ignorenegativestate_p
poisonres_yan_p
fireres_yan_p
lightingres_yan_p
physicsres_yan_p
coldres_yan_p
lifemax_yan_v
lifemax_yan_p
manamax_yan_v
manamax_yan_p
sorbdamage_yan_p
fastwalkrun_yan_p
attackspeed_yan_v
castspeed_yan_v
allres_yan_p
fasthitrecover_yan_v
anti_physicsres_yan_p
anti_poisonres_yan_p
anti_coldres_yan_p
anti_fireres_yan_p
anti_lightingres_yan_p
anti_allres_yan_p
anti_sorbdamage_yan_p
anti_hitrecover
do_hurt_p
skill_showevent
addskillexp1
anti_poisontimereduce_p
anti_stuntimereduce_p
addskilldamage8
addskilldamage9
addskilldamage10
anti_do_hurt_p
do_stun_p
anti_do_stun_p
meleedamagereturnmana_p
rangedamagereturnmana_p
lock_life
special_point
resume_life_p
reset_bufftime
reset_bufftime_recv
cost_sp
cast_when_buff_removed
autocastskill
normal_end
".Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);

        private static readonly Dictionary<string, int> MagicAttribKeyToId = BuildMagicAttribKeyToId();
        private static readonly HashSet<int> MissingDescWarnings = new HashSet<int>();

        private static readonly string[] MagicDescIniCandidates =
        {
            "\\settings\\MagicDesc_mobile.Ini",
            "\\settings\\MagicDesc_mobile_vn.Ini",
            "\\settings\\magicdesc.ini",
        };

        public class Table
        {
            public int id;
            public string key;
            public string desc;
        }

        private static Dictionary<string, int> BuildMagicAttribKeyToId()
        {
            Dictionary<string, int> result = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
            for (int index = 0; index < MagicAttribKeys.Length; index++)
            {
                string key = MagicAttribKeys[index];
                if (string.IsNullOrWhiteSpace(key) || result.ContainsKey(key))
                {
                    continue;
                }

                result[key] = index;
            }

            return result;
        }

        public static void Initialize()
        {
            resource.Cache.Settings.MagicDesc.id = new Dictionary<int, Table>();
            resource.Cache.Settings.MagicDesc.key = new Dictionary<string, Table>(StringComparer.OrdinalIgnoreCase);

            LoadTable(resource.mapping.Settings.magicDescTable);

            foreach (string iniPath in MagicDescIniCandidates)
            {
                if (LoadIni(iniPath))
                {
                    return;
                }
            }
        }

        private static void LoadTable(string path)
        {
            resource.Table table = Game.Resource(path).Get<resource.Table>();
            if (table.IsEmpty())
            {
                return;
            }

            for (int rowIndex = 1; rowIndex < table.RowCount; rowIndex++)
            {
                AddEntry(table.Get<int>(0, rowIndex), table.Get<string>(1, rowIndex), table.Get<string>(2, rowIndex));
            }
        }

        private static bool LoadIni(string path)
        {
            resource.Buffer buffer = Game.Resource(path).Get<resource.Buffer>();
            if (buffer == null || buffer.size <= 0)
            {
                return false;
            }

            resource.Ini ini = new resource.Ini(ReadMagicDescIniText(buffer));
            if (ini.IsEmpty())
            {
                return false;
            }

            Dictionary<string, Dictionary<string, string>> mapping = ini.GetMappingData();
            if (!mapping.TryGetValue("descript", out Dictionary<string, string> descs))
            {
                return false;
            }

            int loadedDescCount = 0;
            for (int index = 0; index < MagicAttribKeys.Length; index++)
            {
                string key = MagicAttribKeys[index];
                descs.TryGetValue(key.ToLowerInvariant(), out string desc);
                if (string.IsNullOrEmpty(desc) == false)
                {
                    loadedDescCount++;
                }

                AddEntry(index, key, DecodeMagicDescText(desc ?? string.Empty));
            }

            return loadedDescCount > 0;
        }

        private static string ReadMagicDescIniText(resource.Buffer buffer)
        {
            resource.Buffer.Encoding detectedEncoding = buffer.GetEncoding(System.Text.Encoding.GetEncoding(1252));
            if (detectedEncoding.byteOrderMarks > 0)
            {
                return detectedEncoding.encoding.GetString(
                    buffer.data,
                    detectedEncoding.byteOrderMarks,
                    buffer.size - detectedEncoding.byteOrderMarks);
            }

            return System.Text.Encoding.GetEncoding(1252).GetString(buffer.data, 0, buffer.size);
        }

        private static string DecodeMagicDescText(string desc)
        {
            if (string.IsNullOrEmpty(desc))
            {
                return string.Empty;
            }

            return resource.formater.TCVN3.UTF8(desc);
        }

        private static void AddEntry(int id, string key, string desc)
        {
            if (id < 0 || string.IsNullOrWhiteSpace(key))
            {
                return;
            }

            key = key.Trim();
            desc ??= string.Empty;
            if (string.IsNullOrEmpty(desc)
                && resource.Cache.Settings.MagicDesc.id.TryGetValue(id, out Table existingById)
                && string.IsNullOrEmpty(existingById.desc) == false)
            {
                desc = existingById.desc;
            }

            Table newField = new Table
            {
                id = id,
                key = key,
                desc = desc
            };

            resource.Cache.Settings.MagicDesc.id[newField.id] = newField;
            resource.Cache.Settings.MagicDesc.key[newField.key] = newField;
        }

        private static void EnsureInitialized()
        {
            if (resource.Cache.Settings.MagicDesc.id == null
                || resource.Cache.Settings.MagicDesc.key == null)
            {
                Initialize();
            }
        }

        public static string IdToKey(int magicId)
        {
            if (magicId >= 0 && magicId < MagicAttribKeys.Length)
            {
                return MagicAttribKeys[magicId];
            }

            EnsureInitialized();
            if (resource.Cache.Settings.MagicDesc.id.TryGetValue(magicId, out Table magicDesc) == false)
            {
                return null;
            }

            return magicDesc.key;
        }

        public static int KeyToId(string key)
        {
            if (string.IsNullOrWhiteSpace(key))
            {
                return -1;
            }

            key = key.Trim();
            if (MagicAttribKeyToId.TryGetValue(key, out int staticId))
            {
                return staticId;
            }

            EnsureInitialized();

            if (resource.Cache.Settings.MagicDesc.key.TryGetValue(key, out Table magicDesc) == false)
            {
                return -1;
            }

            return magicDesc.id;
        }

        public static string Get(settings.skill.SkillSettingData.KMagicAttrib magicAttrib)
        {
            EnsureInitialized();

            if (magicAttrib == null)
            {
                return string.Empty;
            }

            string magicKey = IdToKey(magicAttrib.nAttribType);
            if (string.IsNullOrWhiteSpace(magicKey))
            {
                return "<không xác định: " + magicAttrib.nAttribType + ", table>";
            }

            Table magicDesc = null;
            resource.Cache.Settings.MagicDesc.id.TryGetValue(magicAttrib.nAttribType, out magicDesc);
            if ((magicDesc == null || string.IsNullOrEmpty(magicDesc.desc))
                && resource.Cache.Settings.MagicDesc.key.TryGetValue(magicKey, out Table magicDescByKey))
            {
                magicDesc = magicDescByKey;
            }

            if (magicDesc == null || string.IsNullOrEmpty(magicDesc.desc))
            {
                return GetMissingDescFallback(magicKey, magicAttrib);
            }

            string keyDesc = settings.item.Getter.GetRichText(magicDesc.desc);
            string result = string.Empty;

            if (keyDesc == string.Empty || keyDesc == null)
            {
                return GetMissingDescFallback(magicKey, magicAttrib);
            }

            for (int charIndex = 0; charIndex < keyDesc.Length;)
            {
                char charEntry = keyDesc[charIndex];

                if (charEntry != '#'
                    || charIndex + 3 >= keyDesc.Length
                    || IsPlaceholderType(keyDesc[charIndex + 1]) == false)
                {
                    result += charEntry;
                    charIndex++;
                    continue;
                }

                char charDataType = keyDesc[charIndex + 1];
                char charValue = keyDesc[charIndex + 2];
                char charAddType = keyDesc[charIndex + 3];
                int dataValue = ResolvePlaceholderValue(magicAttrib, charValue);

                result += ResolvePlaceholderText(charDataType, charValue, charAddType, dataValue);
                charIndex += 4;
            }

            return NormalizeRichMarkup(result);
        }

        private static bool IsPlaceholderType(char value)
        {
            switch (value)
            {
                case 'm':
                case 's':
                case 'k':
                case 'd':
                case 'x':
                case 'f':
                case 't':
                case 'l':
                    return true;
                default:
                    return false;
            }
        }

        private static string GetMissingDescFallback(string magicKey, settings.skill.SkillSettingData.KMagicAttrib magicAttrib)
        {
            if (MissingDescWarnings.Add(magicAttrib.nAttribType))
            {
                UnityEngine.Debug.LogWarning(
                    "MagicDesc missing. id=" + magicAttrib.nAttribType +
                    " key=" + magicKey +
                    " values=" + GetValueSummary(magicAttrib));
            }

            return string.Empty;
        }

        private static string GetValueSummary(settings.skill.SkillSettingData.KMagicAttrib magicAttrib)
        {
            int value0 = magicAttrib.nValue != null && magicAttrib.nValue.Length > 0 ? magicAttrib.nValue[0] : 0;
            int value1 = magicAttrib.nValue != null && magicAttrib.nValue.Length > 1 ? magicAttrib.nValue[1] : 0;
            int value2 = magicAttrib.nValue != null && magicAttrib.nValue.Length > 2 ? magicAttrib.nValue[2] : 0;
            return value0 + "," + value1 + "," + value2;
        }

        private static string NormalizeRichMarkup(string value)
        {
            if (string.IsNullOrEmpty(value))
            {
                return string.Empty;
            }

            value = Regex.Replace(
                value,
                "<c=([^>]+)>",
                match => "<color=" + ResolveRichColor(match.Groups[1].Value) + ">",
                RegexOptions.IgnoreCase);

            value = Regex.Replace(
                value,
                "<color=([^>]+)>",
                match => "<color=" + ResolveRichColor(match.Groups[1].Value) + ">",
                RegexOptions.IgnoreCase);

            value = Regex.Replace(value, "<c>", "</color>", RegexOptions.IgnoreCase);
            value = Regex.Replace(value, "<color>", "</color>", RegexOptions.IgnoreCase);
            return value;
        }

        private static string ResolveRichColor(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return "#ffffff";
            }

            string key = value.Trim().Trim('\'', '"').ToLowerInvariant();
            if (key.StartsWith("#"))
            {
                return key;
            }

            if (key.StartsWith("0x"))
            {
                return "#" + key.Substring(2);
            }

            return key switch
            {
                "yellow" => "#ffff00",
                "white" => "#ffffff",
                "red" => "#ff0000",
                "blue" => "#4aa3ff",
                "green" => "#2ecc71",
                "orange" => "#eabd0b",
                "water" => "#66d9ff",
                "teal" => "#00ffff",
                _ => key
            };
        }

        private static int ResolvePlaceholderValue(settings.skill.SkillSettingData.KMagicAttrib magicAttrib, char charValue)
        {
            int value0 = magicAttrib.nValue != null && magicAttrib.nValue.Length > 0 ? magicAttrib.nValue[0] : 0;
            int value1 = magicAttrib.nValue != null && magicAttrib.nValue.Length > 1 ? magicAttrib.nValue[1] : 0;
            int value2 = magicAttrib.nValue != null && magicAttrib.nValue.Length > 2 ? magicAttrib.nValue[2] : 0;

            return charValue switch
            {
                '1' => value0,
                '2' => value1,
                '3' => value2,
                '4' => value0 & 0xffff,
                '6' => value2,
                '7' => (short)((value0 >> 16) & 0xffff),
                '9' => value1,
                _ => value0
            };
        }

        private static string ResolvePlaceholderText(char charDataType, char charValue, char charAddType, int dataValue)
        {
            switch (charDataType)
            {
                case 'm':
                    string[] faction =
                    {
                        "thiếu lâm", "thiên vương", "đường môn", "ngũ độc", "nga mi",
                        "thúy yên", "cái bang", "thiên nhẫn", "võ đang", "côn lôn"
                    };
                    return dataValue >= 0 && dataValue < faction.Length ? faction[dataValue] : "môn phái " + dataValue;

                case 's':
                    string[] series = { "kim", "mộc", "thủy", "hỏa", "thổ" };
                    return dataValue >= 0 && dataValue < series.Length ? series[dataValue] : "ngũ hành " + dataValue;

                case 'k':
                    string[] type = { "nội lực", "sinh lực", "thể lực", "ngân lượng" };
                    return dataValue >= 0 && dataValue < type.Length ? type[dataValue] : "loại " + dataValue;

                case 'd':
                    int displayValue = dataValue;
                    bool wasNegative = dataValue < 0;
                    string prefix = string.Empty;

                    if (charAddType == '+' || charAddType == '-')
                    {
                        prefix = dataValue >= 0 ? "+" : "-";
                        displayValue = Math.Abs(dataValue);
                    }
                    else if (charAddType == '~')
                    {
                        displayValue = Math.Abs(dataValue);
                    }

                    if (wasNegative && displayValue == 1 && IsSecondValueSelector(charValue))
                    {
                        displayValue = 100;
                    }

                    return prefix + displayValue;

                case 'x':
                    return dataValue != 0 ? "nữ" : "nam";

                case 'f':
                    return dataValue.ToString();

                case 't':
                    return (dataValue / 18).ToString();

                case 'l':
                    if (dataValue <= 1)
                    {
                        return "tất cả";
                    }

                    try
                    {
                        settings.skill.SkillSetting skill = settings.skill.SkillSetting.Get(dataValue, 1);
                        return skill != null ? "[" + skill.m_szName + "]" : "[skill " + dataValue + "]";
                    }
                    catch
                    {
                        return "[skill " + dataValue + "]";
                    }

                default:
                    return string.Empty;
            }
        }

        private static bool IsSecondValueSelector(char charValue)
        {
            return charValue == '2' || charValue == '6' || charValue == '9';
        }
    }
}
