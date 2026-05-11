
using System.Text.RegularExpressions;

namespace game.resource.settings.skill
{
    public class SkillSettingLevel : skill.SkillSettingBase
    {
        public void LoadLevel(int skillId, int level)
        {
            if (Cache.Settings.Skill.skillsIdToRowIndexMapping.ContainsKey(skillId) == false)
            {
                return;
            }

            int rowIndex = Cache.Settings.Skill.skillsIdToRowIndexMapping[skillId];
            string scriptPath = Cache.Settings.Skill.skillsTable.Get<string>("LvlSetScript", rowIndex);

            if (scriptPath == null || scriptPath == string.Empty)
            {
                return;
            }
            else
            {
                this.skillLevel = level;
            }

            resource.Script script = new resource.Script(scriptPath);
            try
            {
                //string debugInfo = string.Empty;
                //UnityEngine.Debug.Log("--------------------------------");

                for (int i = 0; i < skill.Defination.MAX_SKILLVEDATA_COUNT; ++i)
                {
                    string szSettingName = "LvlSetting" + (i + 1);
                    string szSettingData = "LvlData" + (i + 1);

                    string szSettingNameValue = Cache.Settings.Skill.skillsTable.Get<string>(szSettingName, rowIndex);
                    string szSettingDataValue = Cache.Settings.Skill.skillsTable.Get<string>(szSettingData, rowIndex);

                    if (szSettingNameValue == null || szSettingNameValue == string.Empty)
                    {
                        continue;
                    }

                    //debugInfo += "|";
                    //debugInfo += szSettingNameValue;

                    string szResult = script.CallFunction<string>("GetSkillLevelData", szSettingNameValue, szSettingDataValue, level);

                    if (szResult != null && szResult != string.Empty)
                    {
                        this.ParseString2MagicAttrib(level, szSettingNameValue, szResult);
                    }
                }

                //UnityEngine.Debug.Log("skill: " + this.m_nId + ", debugInfo: " + debugInfo);
            }
            finally
            {
                script.Release();
            }
        }

        private int MAKELONG(int a, int b)
        {
            return ((a) & 0xffff) | ((b) & 0xffff) << 16;
        }

        private void ParseString2MagicAttrib(int ulLevel, string magicKey, string szValue)
        {
            int magicId = resource.settings.MagicDesc.KeyToId(magicKey);
            if (magicId < 0)
            {
                UnityEngine.Debug.Log("ParseString2MagicAttrib: <không xác định: " + magicKey + ", table>");
                return;
            }

            string[] valueSplited = szValue.Split(',');
            resource.settings.skill.SkillSettingLevel.KMagicAttrib magic = new KMagicAttrib(magicId);
            if (valueSplited.Length > 0) magic.nValue[0] = ParseMagicValue(valueSplited[0]);
            if (valueSplited.Length > 1) magic.nValue[1] = ParseMagicValue(valueSplited[1]);
            if (valueSplited.Length > 2) magic.nValue[2] = ParseMagicValue(valueSplited[2]);

            ////////////////////////////////////////////////////////////////////////////////

            if (ApplyMagicAttribLikeAxmol(ulLevel, magicKey, magic))
            {
                return;
            }

            switch (magicKey)
            {
                case "skill_cost_v": this.m_nCost = magic.nValue[0]; break;
                case "skill_costtype_v": this.m_nSkillCostType = (Defination.NPCATTRIB)magic.nValue[0]; break;
                case "skill_mintimepercast_v": this.m_nMinTimePerCast = magic.nValue[0]; break;
                case "skill_misslenum_v": this.m_nChildSkillNum = magic.nValue[0]; break;
                case "skill_attackradius": this.m_nAttackRadius = magic.nValue[0]; break;
                case "skill_skillexp_v": break;
                case "skill_appendskill": break;
                case "skill_eventskilllevel": this.m_nEventSkillLevel = magic.nValue[0]; break;
                case "missle_speed_v": this.m_MissleAttribs.Add(magic); break;
                case "missle_lifetime_v": this.m_MissleAttribs.Add(magic); break;
                case "missle_damagerange_v": this.m_MissleAttribs.Add(magic); break;
                case "weapondamagemin_v": break;
                case "weapondamagemax_v": break;
                case "armordefense_v": break;
                case "durability_v": break;
                case "requirestr": break;
                case "requiredex": break;
                case "requirevit": break;
                case "requireeng": break;
                case "requirelevel": break;
                case "requireseries": break;
                case "requiresex": break;
                case "requiremenpai": break;
                case "weapondamageenhance_p": break;
                case "armordefenseenhance_p": break;
                case "requirementreduce_p": break;
                case "indestructible_b": break;
                case "require_translife": break;
                case "require_fortune_value": break;
                case "attackrating_v": this.m_DamageAttribs.Add(magic); break;
                case "attackrating_p": this.m_DamageAttribs.Add(magic); break;
                case "ignoredefense_p": this.m_DamageAttribs.Add(magic); break;
                case "physicsdamage_v": this.m_DamageAttribs.Add(magic); break;
                case "colddamage_v": this.m_DamageAttribs.Add(magic); break;
                case "firedamage_v": this.m_DamageAttribs.Add(magic); break;
                case "lightingdamage_v": this.m_DamageAttribs.Add(magic); break;
                case "poisondamage_v": this.m_DamageAttribs.Add(magic); break;
                case "magicdamage_v": this.m_DamageAttribs.Add(magic); break;
                case "physicsenhance_p": this.m_DamageAttribs.Add(magic); break;
                case "steallife_p": this.m_DamageAttribs.Add(magic); break;
                case "stealmana_p": this.m_DamageAttribs.Add(magic); break;
                case "stealstamina_p": this.m_DamageAttribs.Add(magic); break;
                case "knockback_p": break;
                case "deadlystrike_p": this.m_DamageAttribs.Add(magic); break;
                case "fatallystrike_p": this.m_DamageAttribs.Add(magic); break;
                case "stun_p": this.m_DamageAttribs.Add(magic); break;
                case "seriesdamage_p": this.m_ImmediateAttribs.Add(magic); break;
                case "lifemax_v": this.m_ImmediateAttribs.Add(magic); break;
                case "lifemax_p": this.m_ImmediateAttribs.Add(magic); break;
                case "life_v": this.m_ImmediateAttribs.Add(magic); break;
                case "lifereplenish_v": this.m_ImmediateAttribs.Add(magic); break;
                case "manamax_v": this.m_ImmediateAttribs.Add(magic); break;
                case "manamax_p": this.m_ImmediateAttribs.Add(magic); break;
                case "mana_v": this.m_ImmediateAttribs.Add(magic); break;
                case "manareplenish_v": this.m_ImmediateAttribs.Add(magic); break;
                case "staminamax_v": this.m_ImmediateAttribs.Add(magic); break;
                case "staminamax_p": this.m_ImmediateAttribs.Add(magic); break;
                case "stamina_v": this.m_ImmediateAttribs.Add(magic); break;
                case "staminareplenish_v": this.m_ImmediateAttribs.Add(magic); break;
                case "strength_v": this.m_ImmediateAttribs.Add(magic); break;
                case "dexterity_v": this.m_ImmediateAttribs.Add(magic); break;
                case "vitality_v": this.m_ImmediateAttribs.Add(magic); break;
                case "energy_v": this.m_ImmediateAttribs.Add(magic); break;
                case "poisonres_p": this.m_ImmediateAttribs.Add(magic); break;
                case "fireres_p": this.m_ImmediateAttribs.Add(magic); break;
                case "lightingres_p": this.m_ImmediateAttribs.Add(magic); break;
                case "physicsres_p": this.m_ImmediateAttribs.Add(magic); break;
                case "coldres_p": this.m_ImmediateAttribs.Add(magic); break;
                case "freezetimereduce_p": this.m_ImmediateAttribs.Add(magic); break;
                case "poisontimereduce_p": this.m_ImmediateAttribs.Add(magic); break;
                case "poisondamagereduce_v": this.m_ImmediateAttribs.Add(magic); break;
                case "stuntimereduce_p": this.m_ImmediateAttribs.Add(magic); break;
                case "fastwalkrun_p": this.m_ImmediateAttribs.Add(magic); break;
                case "visionradius_p": this.m_ImmediateAttribs.Add(magic); break;
                case "fasthitrecover_v": this.m_ImmediateAttribs.Add(magic); break;
                case "allres_p": this.m_ImmediateAttribs.Add(magic); break;
                case "attackspeed_v": break;
                case "castspeed_v": break;
                case "meleedamagereturn_v": this.m_DamageAttribs.Add(magic); break;
                case "meleedamagereturn_p": this.m_DamageAttribs.Add(magic); break;
                case "rangedamagereturn_v": this.m_DamageAttribs.Add(magic); break;
                case "rangedamagereturn_p": this.m_DamageAttribs.Add(magic); break;
                case "addphysicsdamage_v": this.m_DamageAttribs.Add(magic); break;
                case "addfiredamage_v": this.m_DamageAttribs.Add(magic); break;
                case "addcolddamage_v": this.m_DamageAttribs.Add(magic); break;
                case "addlightingdamage_v": this.m_DamageAttribs.Add(magic); break;
                case "addpoisondamage_v": this.m_DamageAttribs.Add(magic); break;
                case "addphysicsdamage_p": this.m_DamageAttribs.Add(magic); break;
                case "slowmissle_b": break;
                case "changecamp_b": break;
                case "physicsarmor_v": this.m_ImmediateAttribs.Add(magic); break;
                case "coldarmor_v": this.m_ImmediateAttribs.Add(magic); break;
                case "firearmor_v": this.m_ImmediateAttribs.Add(magic); break;
                case "poisonarmor_v": this.m_ImmediateAttribs.Add(magic); break;
                case "lightingarmor_v": this.m_ImmediateAttribs.Add(magic); break;
                case "damage2addmana_p": this.m_ImmediateAttribs.Add(magic); break;
                case "lucky_v": break;
                case "allskill_v": break;
                case "metalskill_v": break;
                case "woodskill_v": break;
                case "waterskill_v": break;
                case "fireskill_v": break;
                case "earthskill_v": break;
                case "deadlystrikeenhance_p": this.m_ImmediateAttribs.Add(magic); break;
                case "badstatustimereduce_v": this.m_ImmediateAttribs.Add(magic); break;
                case "manashield_p": this.m_ImmediateAttribs.Add(magic); break;
                case "adddefense_v": this.m_ImmediateAttribs.Add(magic); break;
                case "adddefense_p": this.m_ImmediateAttribs.Add(magic); break;
                case "fatallystrikeenhance_p": this.m_ImmediateAttribs.Add(magic); break;
                case "physicsresmax_p": this.m_ImmediateAttribs.Add(magic); break;
                case "coldresmax_p": this.m_ImmediateAttribs.Add(magic); break;
                case "fireresmax_p": this.m_ImmediateAttribs.Add(magic); break;
                case "lightingresmax_p": this.m_ImmediateAttribs.Add(magic); break;
                case "poisonresmax_p": this.m_ImmediateAttribs.Add(magic); break;
                case "allresmax_p": this.m_ImmediateAttribs.Add(magic); break;
                case "coldenhance_p": this.m_ImmediateAttribs.Add(magic); break;
                case "fireenhance_p": this.m_ImmediateAttribs.Add(magic); break;
                case "lightingenhance_p": this.m_ImmediateAttribs.Add(magic); break;
                case "poisonenhance_p": this.m_ImmediateAttribs.Add(magic); break;
                case "magicenhance_p": this.m_ImmediateAttribs.Add(magic); break;
                case "attackratingenhance_v": this.m_ImmediateAttribs.Add(magic); break;
                case "attackratingenhance_p": this.m_ImmediateAttribs.Add(magic); break;
                case "addphysicsmagic_v": this.m_ImmediateAttribs.Add(magic); break;
                case "addcoldmagic_v": this.m_ImmediateAttribs.Add(magic); break;
                case "addfiremagic_v": this.m_ImmediateAttribs.Add(magic); break;
                case "addlightingmagic_v": this.m_ImmediateAttribs.Add(magic); break;
                case "addpoisonmagic_v": this.m_ImmediateAttribs.Add(magic); break;
                case "fatallystrikeres_p": this.m_ImmediateAttribs.Add(magic); break;
                case "addskilldamage1": this.m_ImmediateAttribs.Add(magic); break;
                case "addskilldamage2": this.m_ImmediateAttribs.Add(magic); break;
                case "expenhance_p": this.m_ImmediateAttribs.Add(magic); break;
                case "dynamicmagicshield_v": this.m_ImmediateAttribs.Add(magic); break;
                case "addstealfeatureskill": break;
                case "lucky_v_partner": break;
                case "lifereplenish_p": this.m_ImmediateAttribs.Add(magic); break;
                case "ignoreskill_p": this.m_ImmediateAttribs.Add(magic); break;
                case "returnskill_p": this.m_ImmediateAttribs.Add(magic); break;
                case "poisondamagereturn_v": this.m_DamageAttribs.Add(magic); break;
                case "poisondamagereturn_p": this.m_DamageAttribs.Add(magic); break;
                case "autoreplyskill": this.m_StateAttribs.Add(magic); break;
                case "hide": this.m_StateAttribs.Add(magic); break;
                case "poison2decmana_p": this.m_StateAttribs.Add(magic); break;
                case "returnres_p": this.m_ImmediateAttribs.Add(magic); break;
                case "skill_startevent": this.m_bStartEvent = magic.nValue[0]; this.m_nStartSkillId = magic.nValue[2]; break;
                case "skill_flyevent": this.m_bFlyingEvent = magic.nValue[0]; this.m_nFlyEventTime = magic.nValue[1]; this.m_nFlySkillId = magic.nValue[2]; break;
                case "skill_collideevent": this.m_bCollideEvent = magic.nValue[0]; this.m_nCollideSkillId = magic.nValue[2]; break;
                case "skill_vanishedevent": this.m_bVanishedEvent = magic.nValue[0]; this.m_nVanishedSkillId = magic.nValue[2]; break;
                case "dec_percasttimehorse": break;
                case "dec_percasttime": break;
                case "enhance_709_auto": this.m_StateAttribs.Add(magic); break;
                case "enhance_708_life_p": this.m_StateAttribs.Add(magic); break;
                case "enhance_93_life_v": this.m_StateAttribs.Add(magic); break;
                case "enhance_711_auto": this.m_StateAttribs.Add(magic); break;
                case "enhance_714_auto": this.m_StateAttribs.Add(magic); break;
                case "enhance_717_auto": this.m_StateAttribs.Add(magic); break;
                case "enhance_723_miss_p": this.m_StateAttribs.Add(magic); break;
                case "sorbdamage_p": this.m_ImmediateAttribs.Add(magic); break;
                case "anti_hitrecover": break;
                case "anti_stuntimereduce_p": break;
                case "anti_poisonres_p": break;
                case "anti_fireres_p": break;
                case "anti_lightingres_p": break;
                case "anti_physicsres_p": break;
                case "anti_coldres_p": break;
                case "block_rate": break;
                case "enhancehit_rate": break;
                case "poisonres_yan_p": this.m_ImmediateAttribs.Add(magic); break;
                case "lightingres_yan_p": this.m_ImmediateAttribs.Add(magic); break;
                case "fireres_yan_p": this.m_ImmediateAttribs.Add(magic); break;
                case "physicsres_yan_p": this.m_ImmediateAttribs.Add(magic); break;
                case "coldres_yan_p": this.m_ImmediateAttribs.Add(magic); break;
                case "lifemax_yan_v": this.m_ImmediateAttribs.Add(magic); break;
                case "lifemax_yan_p": this.m_ImmediateAttribs.Add(magic); break;
                case "manamax_yan_v": this.m_ImmediateAttribs.Add(magic); break;
                case "manamax_yan_p": this.m_ImmediateAttribs.Add(magic); break;
                case "sorbdamage_yan_p": this.m_ImmediateAttribs.Add(magic); break;
                case "fastwalkrun_yan_p": this.m_ImmediateAttribs.Add(magic); break;
                case "attackspeed_yan_v": this.m_ImmediateAttribs.Add(magic); break;
                case "castspeed_yan_v": this.m_ImmediateAttribs.Add(magic); break;
                case "allres_yan_p": this.m_ImmediateAttribs.Add(magic); break;
                case "anti_maxres_p": this.m_ImmediateAttribs.Add(magic); break;
                case "skill_enhance": this.m_ImmediateAttribs.Add(magic); break;
                case "magicdamage_p": this.m_ImmediateAttribs.Add(magic); break;
                case "fasthitrecover_yan_v": break;
                case "five_elements_enhance_v": break;
                case "five_elements_resist_v": break;
                case "manareplenish_p": break;
                case "add_damage_p": break;
                case "pk_punish_weaken": break;
                case "add_boss_damage": break;
                case "pk_punish_enhance": break;
                case "anti_poisontimereduce_p": break;
                case "do_hurt_p": break;
                case "anti_do_hurt_p": break;
                case "do_stun_p": break;
                case "anti_do_stun_p": break;
                case "anti_physicsres_yan_p": break;
                case "anti_poisonres_yan_p": break;
                case "anti_coldres_yan_p": break;
                case "anti_fireres_yan_p": break;
                case "anti_lightingres_yan_p": break;
                case "anti_allres_yan_p": break;
                case "anti_sorbdamage_yan_p": break;
                case "anti_block_rate": break;
                case "anti_enhancehit_rate": break;
                case "enhancehiteffect_rate ": break;
                case "skill_showevent": this.m_nShowEvent = magic.nValue[0]; break;
                case "addskillexp1": break;
                default:
                    UnityEngine.Debug.Log("ParseString2MagicAttrib catched fail: " + magicKey);
                    break;
            }
        }

        private static int ParseMagicValue(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return 0;
            }

            string normalized = Regex.Replace(value, "[^0-9-]", string.Empty);
            return int.TryParse(normalized, out int result) ? result : 0;
        }

        private bool ApplyMagicAttribLikeAxmol(int level, string magicKey, KMagicAttrib magic)
        {
            int magicId = magic.nAttribType;

            int missleBegin = resource.settings.MagicDesc.KeyToId("missle_begin");
            int missleEnd = resource.settings.MagicDesc.KeyToId("missle_end");
            if (magicId > missleBegin && magicId < missleEnd)
            {
                this.m_MissleAttribs.Add(magic);
                return true;
            }

            int skillBegin = resource.settings.MagicDesc.KeyToId("skill_begin");
            int skillEnd = resource.settings.MagicDesc.KeyToId("skill_end");
            if (magicId > skillBegin && magicId < skillEnd)
            {
                ApplySkillControlAttrib(magicKey, magic);
                return true;
            }

            int damageBegin = resource.settings.MagicDesc.KeyToId("damage_begin");
            int damageEnd = resource.settings.MagicDesc.KeyToId("damage_end");
            if (magicId > damageBegin && magicId < damageEnd)
            {
                this.m_DamageAttribs.Add(magic);
                if (MagicKeyEquals(magicKey, "autoattackskill"))
                {
                    this.m_StateAttribs.Add(new KMagicAttrib(magic.nAttribType, magic.nValue[0], magic.nValue[1], magic.nValue[2]));
                }

                return true;
            }

            if (MagicKeyEquals(magicKey, "mintimepercastonhorse_v"))
            {
                this.m_nMinTimePerCastOnHorse = magic.nValue[0];
                return true;
            }

            if (ShouldAddImmediateAttrib(magicKey, magic.nValue[1]))
            {
                this.m_ImmediateAttribs.Add(magic);
                ApplyEventAttrib(magicKey, magic);
                return true;
            }

            if (MagicKeyEquals(magicKey, "autoreplyskill"))
            {
                KMagicAttrib stateMagic = new KMagicAttrib(magic.nAttribType);
                stateMagic.nValue[0] = MAKELONG((magic.nValue[0] - level) / 256, level);
                stateMagic.nValue[1] = magic.nValue[1];
                stateMagic.nValue[2] = magic.nValue[2] / (256 * 18) + magic.nValue[2] % (256 * 18);
                this.m_StateAttribs.Add(stateMagic);
                return true;
            }

            this.m_StateAttribs.Add(magic);
            return true;
        }

        private void ApplySkillControlAttrib(string magicKey, KMagicAttrib magic)
        {
            if (MagicKeyEquals(magicKey, "skill_cost_v"))
            {
                this.m_nCost = magic.nValue[0];
            }
            else if (MagicKeyEquals(magicKey, "skill_costtype_v"))
            {
                this.m_nSkillCostType = (Defination.NPCATTRIB)magic.nValue[0];
            }
            else if (MagicKeyEquals(magicKey, "skill_mintimepercast_v"))
            {
                this.m_nMinTimePerCast = magic.nValue[0];
            }
            else if (MagicKeyEquals(magicKey, "skill_mintimepercastonhorse_v"))
            {
                this.m_nMinTimePerCastOnHorse = magic.nValue[0];
            }
            else if (MagicKeyEquals(magicKey, "skill_misslenum_v"))
            {
                this.m_nChildSkillNum = magic.nValue[0];
            }
            else if (MagicKeyEquals(magicKey, "skill_misslesform_v"))
            {
                this.m_eMisslesForm = (Defination.MisslesForm)magic.nValue[0];
            }
            else if (MagicKeyEquals(magicKey, "skill_param1_v"))
            {
                this.m_nValue1 = magic.nValue[0];
            }
            else if (MagicKeyEquals(magicKey, "skill_param2_v"))
            {
                this.m_nValue2 = magic.nValue[1];
            }
            else if (MagicKeyEquals(magicKey, "skill_eventskilllevel"))
            {
                this.m_nEventSkillLevel = magic.nValue[0];
            }
            else if (MagicKeyEquals(magicKey, "skill_attackradius"))
            {
                this.m_nAttackRadius = magic.nValue[0];
            }
        }

        private static bool ShouldAddImmediateAttrib(string magicKey, int value2)
        {
            return value2 == 0
                   || MagicKeyEquals(magicKey, "skill_flyevent")
                   || MagicKeyEquals(magicKey, "skill_collideevent")
                   || MagicKeyEquals(magicKey, "skill_vanishedevent")
                   || MagicKeyEquals(magicKey, "skill_startevent")
                   || MagicKeyEquals(magicKey, "skill_showevent")
                   || IsAddSkillDamageKey(magicKey);
        }

        private static bool IsAddSkillDamageKey(string magicKey)
        {
            return MagicKeyEquals(magicKey, "addskilldamage1")
                   || MagicKeyEquals(magicKey, "addskilldamage2")
                   || MagicKeyEquals(magicKey, "addskilldamage3")
                   || MagicKeyEquals(magicKey, "addskilldamage4")
                   || MagicKeyEquals(magicKey, "addskilldamage5")
                   || MagicKeyEquals(magicKey, "addskilldamage6")
                   || MagicKeyEquals(magicKey, "addskilldamage7")
                   || MagicKeyEquals(magicKey, "addskilldamage8")
                   || MagicKeyEquals(magicKey, "addskilldamage9")
                   || MagicKeyEquals(magicKey, "addskilldamage10");
        }

        private void ApplyEventAttrib(string magicKey, KMagicAttrib magic)
        {
            if (MagicKeyEquals(magicKey, "skill_showevent"))
            {
                this.m_nShowEvent = magic.nValue[0];
            }
            else if (MagicKeyEquals(magicKey, "skill_startevent"))
            {
                this.m_bStartEvent = magic.nValue[0] > 0 ? 1 : 0;
                this.m_nStartSkillId = magic.nValue[2];
            }
            else if (MagicKeyEquals(magicKey, "skill_flyevent"))
            {
                this.m_bFlyingEvent = magic.nValue[0] > 0 ? 1 : 0;
                this.m_nFlyEventTime = magic.nValue[1] > 0 ? magic.nValue[1] : 0;
                this.m_nFlySkillId = magic.nValue[2];
            }
            else if (MagicKeyEquals(magicKey, "skill_collideevent"))
            {
                this.m_bCollideEvent = magic.nValue[0] > 0 ? 1 : 0;
                this.m_nCollideSkillId = magic.nValue[2];
            }
            else if (MagicKeyEquals(magicKey, "skill_vanishedevent"))
            {
                this.m_bVanishedEvent = magic.nValue[0] > 0 ? 1 : 0;
                this.m_nVanishedSkillId = magic.nValue[2];
            }
        }

        private static bool MagicKeyEquals(string magicKey, string expected)
        {
            return string.Equals(magicKey, expected, System.StringComparison.OrdinalIgnoreCase);
        }
    }
}
