
using System.Collections.Generic;

namespace game.resource.settings.npcres
{
    public class Datafield
    {
        public enum NPCKIND
        {
            kind_normal = 0,
            kind_player,
            kind_partner,
            kind_dialoger,
            kind_bird,
            kind_mouse,

            kind_num
        }

        public enum NPCCAMP
        {
            camp_begin,
            camp_justice,
            camp_evil,
            camp_balance,
            camp_free,
            camp_animal,
            camp_event,
            camp_num,
        }

        public enum NPCCMD
        {
            do_none,
            do_stand,
            do_walk,
            do_run,
            do_jump,
            do_skill,
            do_magic,
            do_attack,
            do_sit,
            do_hurt,
            do_death,
            do_defense,
            do_idle,
            do_specialskill,
            do_special1,
            do_special2,
            do_special3,
            do_special4,
            do_runattack,
            do_manyattack,
            do_jumpattack,
            do_revive,
        }

        public class KState
        {
            public int nMagicAttrib;
            public int[] nValue;
            public int nTime;

            public KState()
            {
                this.nValue = new int[2];
            }
        }

        public class CEnhanceInfo
        {
            public int nSkillIdx;
            public int nEnhance;
        }

        public class skillAuraInfo
        {
            public int skillid;
            public int skilllistIndex;
            public int level;
        }

        public Datafield.NPCKIND m_Kind;
        public Datafield.NPCCMD m_Doing;

        public KState m_PoisonState;
        public KState m_FreezeState;
        public KState m_BurnState;
        public KState m_ConfuseState;
        public KState m_StunState;
        public KState m_LifeState;
        public KState m_ManaState;
        public KState m_MenuState;
        public KState m_DrunkState;
        public KState m_Hide;
        public KState m_ZhuaState;
        public KState m_LoseMana;
        public KState m_ExpState;
        public KState m_DoScriptState;
        public KState m_randmove;
        public KState m_MapUseModel;
        public KState m_PhysicsArmor;
        public KState m_ColdArmor;
        public KState m_LightArmor;
        public KState m_PoisonArmor;
        public KState m_FireArmor;
        public KState m_ManaShield;
        public KState m_Returnskill;
        public KState m_Deathkill;
        public KState m_Rescueskill;
        public KState m_Replyskill;

        public int m_Camp;
        public int m_CurrentCamp;

        public int m_CurrentExperience;
        public int m_CurrentLife;
        public int m_CurrentLifeDamage;
        public int m_CurPoisonDamage;
        public int m_CurFireDamage;
        public int m_CurrentLifeMax;
        public int m_CurrentLifeReplenish;
        public int m_CurrentLifeReplenish_p;
        public int m_CurrentMana;
        public int m_CurrentManaMax;
        public int m_CurrentManaReplenish;
        public int m_CurrentManaReplenish_p;
        public int m_CurrentStamina;
        public int m_CurrentStaminaMax;
        public int m_CurrentStaminaGain;
        public int m_CurrentStaminaLoss;

        public int m_CurrentNuQi;
        public int m_CurrentNuQiMax;
        public int m_CurrentNuQiReplenish;

        public settings.skill.SkillSettingData.KMagicAttrib m_PhysicsDamage;
        public settings.skill.SkillSettingData.KMagicAttrib m_CurrentPhysicsMagicDamageP;
        public settings.skill.SkillSettingData.KMagicAttrib m_CurrentPhysicsMagicDamageV;
        public settings.skill.SkillSettingData.KMagicAttrib m_CurrentMagicFireDamage;
        public settings.skill.SkillSettingData.KMagicAttrib m_CurrentMagicColdDamage;
        public settings.skill.SkillSettingData.KMagicAttrib m_CurrentMagicLightDamage;
        public settings.skill.SkillSettingData.KMagicAttrib m_CurrentMagicPoisonDamage;
        public int m_CurrentAttackRating;
        public int m_CurrentDefend;

        public int m_CurrentSkillMingZhong;
        public int m_TempFireResist;
        public int m_TempColdResist;
        public int m_TempPoisonResist;
        public int m_TempLightResist;
        public int m_TempPhysicsResist;

        public int m_CurrentFireResist;
        public int m_CurrentColdResist;
        public int m_CurrentPoisonResist;
        public int m_CurrentLightResist;
        public int m_CurrentPhysicsResist;

        public int m_CurrentFireResistMax;
        public int m_CurrentColdResistMax;
        public int m_CurrentPoisonResistMax;
        public int m_CurrentLightResistMax;
        public int m_CurrentPhysicsResistMax;


        public int m_CurrentTempSpeed;
        public int m_CurrentWalkSpeed;
        public int m_CurrentRunSpeed;
        public int m_CurrentJumpSpeed;
        public int m_CurrentJumpFrame;
        public int m_CurrentAttackSpeed;
        public int m_CurrentCastSpeed;
        public int m_CurrentVisionRadius;
        public int m_CurrentAttackRadius;
        public int m_CurrentActiveRadius;
        public int m_CurrentHitRecover;
        public int m_CurrentHitNpcRecover;
        public int m_CurrentTreasure;
        public int m_CurrentHitRank;

        public int m_CurrentMeleeDmgRetPercent;
        public int m_CurrentMeleeDmgRet;
        public int m_CurrentRangeDmgRetPercent;
        public int m_CurrentRangeDmgRet;
        public bool m_CurrentSlowMissle;
        public int m_CurrentHulueMeleeDmgRet;
        public int m_CurrentHulueRangeDmgRet;

        public int m_CurrentDamageReduce;


        public int m_CurrentDamage2Mana;

        public int m_CurrentLifeStolen;
        public int m_CurrentManaStolen;
        public int m_CurrentStaminaStolen;
        public int m_CurrentKnockBack;
        public int m_CurrentDeadlyStrike;


        public int m_CurrentFreezeTimeReducePercent;
        public int m_CurrentPoisonTimeReducePercent;
        public int m_CurrentStunTimeReducePercent;
        public int m_CurrentBurnTimeReducePercent;
        public int m_CurrentautoReviverate;
        public int m_CurrentStunRank_p;

        public int m_EnemyPoisonTimeReducePercent;
        public int m_EnemyStunTimeReducePercent;

        public int m_CurrentBuZhuoRate;
        public int m_CurrentUpExp;
        public int m_CurrentdanggeRate;
        public int m_CurrentzhongjiRate;
        public int m_CurrentcjdanggeRate;
        public int m_CurrentcjzhongjiRate;
        public int m_Currentsorbdamage;
        public int m_Currentsorbdamage_v;
        public int m_Currenadddamagev;
        public int m_Currenadddamagep;
        public int m_Currentpoisonres;
        public int m_Currentfireres;
        public int m_Currentlightingres;
        public int m_Currentphysicsres;
        public int m_Currentcoldres;
        public int m_Currentallres;
        public int m_CurrentIgnoredefensep;
        public int m_CurrentIgnorenAttacRating;
        public int m_Currentnopkvalue;
        public int m_Currentbossdamage;
        public int m_Currentelementsenhance;
        public int m_Currentelementsresist;

        public int m_Currentskillenhance;
        public int m_CurrentFullManaskillenhance;

        public int m_CurrentFireEnhance;
        public int m_CurrentColdEnhance;
        public int m_CurrentPoisonEnhance;
        public int m_CurrentLightEnhance;
        public int m_CurrentPoisonTime;

        public int m_CurrentAttackRatingEnhancep;
        public int m_CurrentAttackRatingEnhancev;

        public int m_CurrentAddPhysicsDamage;
        public int m_CurrentAddPhysicsDamageP;
        public int m_CurrentAddFireDamagev;
        public int m_CurrentAddColdDamagev;
        public int m_CurrentAddLighDamagev;
        public int m_CurrentAddPoisonDamagev;
        public bool m_IsDel;
        public int m_CurrentAddmagicphysicsDamage;
        public int m_CurrentAddmagicphysicsDamageP;
        public int m_CurrentAddmagicColdDamagicv;
        public int m_CurrentAddmagicFireDamagicv;
        public int m_CurrentAddmagicLightDamagicv;
        public int m_CurrentAddmagicPoisonDamagicv;

        public int[] m_CurrentMeleeEnhance;
        public int m_CurrentRangeEnhance;
        public int m_CurrentHandEnhance;
        public int m_CurrentSerisesEnhance;

        public int m_CurrentPoisondamagereturnV;
        public int m_CurrentPoisondamagereturnP;
        public int m_CurrentReturnskillp;
        public int m_CurrentIgnoreskillp;
        public int m_CurrentReturnresp;
        public int m_CurrentCreatnpcv;
        public int m_CurrentAllJiHuo;
        public int m_CurrentCreatStatus;
        public int m_Currentbaopoisondmax_p;
        public int m_nCurNpcLucky;

        public int m_Me2metaldamage_p;
        public int m_Metal2medamage_p;
        public int m_Me2wooddamage_p;
        public int m_Wood2medamage_p;
        public int m_Me2waterdamage_p;
        public int m_Water2medamage_p;
        public int m_Me2firedamage_p;
        public int m_Fire2medamage_p;
        public int m_Me2earthdamage_p;
        public int m_Earth2medamage_p;

        public int m_Staticmagicshield_p;

        public int m_nPeopleIdx;
        public int m_nLastDamageIdx;
        public int m_nLastPoisonDamageIdx;
        public int m_nLastBurnDamageIdx;
        public int m_nObjectIdx;


        public int m_Experience;
        public int m_LifeMax;
        public int m_LifeReplenish;

        public int m_NuqiMax;
        public int m_NuqiReplenish;

        public int m_ManaMax;
        public int m_ManaReplenish;

        public int m_StaminaMax;
        public int m_StaminaGain;
        public int m_StaminaLoss;

        public int m_AttackRating;
        public int m_Defend;
        public int m_FireResist;
        public int m_ColdResist;
        public int m_PoisonResist;
        public int m_LightResist;
        public int m_PhysicsResist;

        public int m_FireResistMax;
        public int m_ColdResistMax;
        public int m_PoisonResistMax;
        public int m_LightResistMax;
        public int m_PhysicsResistMax;

        public int m_WalkSpeed;
        public int m_RunSpeed;
        public int m_JumpSpeed;
        public int m_AttackSpeed;
        public int m_CastSpeed;
        public int m_VisionRadius;
        public int m_DialogRadius;
        public int m_ActiveRadius;
        public int m_HitRecover;
        public int m_Treasure;
        public bool m_bClientOnly;



        public int m_nCurrentMeleeTime;

        public int m_Series;
        public int m_nCurLucky;
        public int m_nTempLucky_p;
        public int m_nLucky;

        public int m_nLeftSkillID;
        public int m_nLeftListidx;

        public int m_nUpExp;

        public bool m_IsMoreAura;
        public skillAuraInfo[] m_TmpAuraID;

        public int m_StandFrame;
        public int m_WalkFrame;
        public int m_RunFrame;
        public int m_DeathFrame;
        public int m_HurtFrame;

        public npcres.SkillList m_SkillList;
        public Dictionary<int, CEnhanceInfo> nEnhanceInfo;

        public Datafield()
        {
            this.m_Kind = NPCKIND.kind_player;
            this.m_Doing = NPCCMD.do_none;

            this.m_PoisonState = new KState();
            this.m_FreezeState = new KState();
            this.m_BurnState = new KState();
            this.m_ConfuseState = new KState();
            this.m_StunState = new KState();
            this.m_LifeState = new KState();
            this.m_ManaState = new KState();
            this.m_MenuState = new KState();
            this.m_DrunkState = new KState();
            this.m_Hide = new KState();
            this.m_ZhuaState = new KState();
            this.m_LoseMana = new KState();
            this.m_ExpState = new KState();
            this.m_DoScriptState = new KState();
            this.m_randmove = new KState();
            this.m_MapUseModel = new KState();
            this.m_PhysicsArmor = new KState();
            this.m_ColdArmor = new KState();
            this.m_LightArmor = new KState();
            this.m_PoisonArmor = new KState();
            this.m_FireArmor = new KState();
            this.m_ManaShield = new KState();
            this.m_Returnskill = new KState();
            this.m_Deathkill = new KState();
            this.m_Rescueskill = new KState();
            this.m_Replyskill = new KState();

            this.m_PhysicsDamage = new settings.skill.SkillSettingData.KMagicAttrib();
            this.m_CurrentPhysicsMagicDamageP = new settings.skill.SkillSettingData.KMagicAttrib();
            this.m_CurrentPhysicsMagicDamageV = new settings.skill.SkillSettingData.KMagicAttrib();
            this.m_CurrentMagicFireDamage = new settings.skill.SkillSettingData.KMagicAttrib();
            this.m_CurrentMagicColdDamage = new settings.skill.SkillSettingData.KMagicAttrib();
            this.m_CurrentMagicLightDamage = new settings.skill.SkillSettingData.KMagicAttrib();
            this.m_CurrentMagicPoisonDamage = new settings.skill.SkillSettingData.KMagicAttrib();

            this.m_CurrentMeleeEnhance = new int[6];

            this.m_TmpAuraID = new skillAuraInfo[5];
            for (int i = 0; i < 5; i++)
            {
                this.m_TmpAuraID[i] = new skillAuraInfo();
            }

            this.m_SkillList = new SkillList();
            this.nEnhanceInfo = new Dictionary<int, CEnhanceInfo>();

            this.m_WalkSpeed = 5;
            this.m_RunSpeed = 10;
            this.m_CurrentWalkSpeed = this.m_WalkSpeed;
            this.m_CurrentRunSpeed = this.m_RunSpeed;
            this.m_StandFrame = 15;
            this.m_WalkFrame = 15;
            this.m_RunFrame = 15;
            this.m_DeathFrame = 15;
            this.m_HurtFrame = 10;
        }

        public void ChangeCurDexterity(int nData)
        {

        }

        public void ChangeCurEngergy(int nData)
        {

        }

        public void ChangeCurStrength(int nData)
        {

        }

        public void ChangeCurVitality(int nData)
        {

        }

        public int GetLeftSkill()
        {
            return m_nLeftSkillID;
        }

        public int GetLeftSkillListidx()
        {
            return m_nLeftListidx;
        }
    }
}
