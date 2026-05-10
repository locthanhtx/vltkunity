using System;
using System.Collections.Generic;

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
skill_reserve3
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

        private static readonly string[] MagicDescIniCandidates =
        {
            "\\settings\\MagicDesc_mobile_vn.Ini",
            "\\settings\\MagicDesc_mobile.Ini",
            "\\settings\\magicdesc.ini",
        };

        public class Table
        {
            public int id;
            public string key;
            public string desc;
        }

        public static void Initialize()
        {
            resource.Cache.Settings.MagicDesc.id = new Dictionary<int, Table>();
            resource.Cache.Settings.MagicDesc.key = new Dictionary<string, Table>();

            LoadTable(resource.mapping.Settings.magicDescTable);
            if (resource.Cache.Settings.MagicDesc.id.Count > 0)
            {
                return;
            }

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
            resource.Ini ini = Game.Resource(path).Get<resource.Ini>();
            if (ini.IsEmpty())
            {
                return false;
            }

            Dictionary<string, Dictionary<string, string>> mapping = ini.GetMappingData();
            if (!mapping.TryGetValue("descript", out Dictionary<string, string> descs))
            {
                return false;
            }

            for (int index = 0; index < MagicAttribKeys.Length; index++)
            {
                string key = MagicAttribKeys[index];
                descs.TryGetValue(key.ToLowerInvariant(), out string desc);
                AddEntry(index, key, desc ?? string.Empty);
            }

            return true;
        }

        private static void AddEntry(int id, string key, string desc)
        {
            if (id < 0 || string.IsNullOrWhiteSpace(key))
            {
                return;
            }

            Table newField = new Table
            {
                id = id,
                key = key,
                desc = desc ?? string.Empty
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
            EnsureInitialized();

            if (resource.Cache.Settings.MagicDesc.id.ContainsKey(magicId) == false)
            {
                return null;
            }

            return resource.Cache.Settings.MagicDesc.id[magicId].key;
        }

        public static int KeyToId(string key)
        {
            EnsureInitialized();

            if (resource.Cache.Settings.MagicDesc.key.ContainsKey(key) == false)
            {
                return -1;
            }

            return resource.Cache.Settings.MagicDesc.key[key].id;
        }

        public static string Get(settings.skill.SkillSettingData.KMagicAttrib magicAttrib)
        {
            EnsureInitialized();

            if (magicAttrib == null)
            {
                return string.Empty;
            }

            if (resource.Cache.Settings.MagicDesc.id.ContainsKey(magicAttrib.nAttribType) == false)
            {
                return "<không xác định: " + magicAttrib.nAttribType + ", table>";
            }

            Table magicDesc = resource.Cache.Settings.MagicDesc.id[magicAttrib.nAttribType];
            string keyDesc = settings.item.Getter.GetRichText(magicDesc.desc);
            string result = string.Empty;

            if (keyDesc == string.Empty || keyDesc == null)
            {
                return "<không xác định: " + magicAttrib.nAttribType + ", desc>";
            }

            for (int charIndex = 0; charIndex < keyDesc.Length;)
            {
                char charEntry = keyDesc[charIndex];

                if (charEntry != '#' || charIndex + 3 >= keyDesc.Length)
                {
                    result += charEntry;
                    charIndex++;
                    continue;
                }

                char charDataType = keyDesc[charIndex + 1];
                char charValue = keyDesc[charIndex + 2];
                char charAddType = keyDesc[charIndex + 3];
                int dataValue = ResolvePlaceholderValue(magicAttrib, charValue);

                result += ResolvePlaceholderText(charDataType, charAddType, dataValue);
                charIndex += 4;
            }

            return result;
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

        private static string ResolvePlaceholderText(char charDataType, char charAddType, int dataValue)
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
                    if (charAddType == '+' || charAddType == '-')
                    {
                        return (dataValue >= 0 ? "+" : "-") + Math.Abs(dataValue);
                    }

                    if (charAddType == '~')
                    {
                        return Math.Abs(dataValue).ToString();
                    }

                    return dataValue.ToString();

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
    }
}
