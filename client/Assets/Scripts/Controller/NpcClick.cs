using System.Collections.Generic;
using game.config;
using game.network;
using game.network.listener;
using Photon.ShareLibrary.Constant;
using Photon.ShareLibrary.Entities;
using Photon.ShareLibrary.Handlers;
using Photon.ShareLibrary.Utils;
using System.Collections;
using UnityEngine;
using static game.resource.settings.npcres.Identification;

public class NpcClick : MonoBehaviour, ICharacterObj
{
    private const int MainSkillLocation = 0;
    private const int FallbackBasicAttackSkillId = 53;
    private const byte FallbackBasicAttackSkillLevel = 1;

    private int npcId;
    public int Id { get { return npcId; } }
    private string npcName;
    public string Name { get { return npcName; } set { npcName = value; } }
    private NPCKIND kind;
    public NPCKIND Kind { get { return kind; } set { kind = value; } }
    private NPCSERIES series;
    public NPCSERIES Series { get { return series; } set { series = value; } }
    private NPCCAMP cam;
    public NPCCAMP CurrentCamp { get { return cam; } set { cam = value; } }
    public bool FightMode { get { return true; } set { } }
    public string MasterName { get; set; }
    public ICharacterObj MasterObj { get; set; }

    public virtual EnumPK GetNormalPKState()
    {
        return EnumPK.ENMITY_STATE_CLOSE;
    }
    public virtual EnumPK GetEnmityPKState()
    {
        return EnumPK.ENMITY_STATE_CLOSE;
    }
    public virtual int GetEnmityPKAim() { return 0; }
    public virtual int GetExercisePKAim() { return 0; }

    private NPCCMD npcCmd;
    private byte level;
    private byte dir;
    private int npcType;
    private int mapX;
    private int mapY;
    private Coroutine removeAfterDeathCoroutine;

    private int HPMax, HPCur;
    public float LastClassicSyncTime { get; private set; } = -1f;
    public int CurrentHPMax
    {
        get { return HPMax; }
        set
        {
            HPMax = value;
            if (controller != null)
            {
                controller.data.m_CurrentLifeMax = HPMax;
            }
            SyncNpcHP();
        }
    }
    public int CurrentHPCur
    {
        get { return HPCur; }
        set
        {
            HPCur = value;
            if (controller != null)
            {
                controller.data.m_CurrentLife = HPCur;
            }
            SyncNpcHP();
        }
    }
    public bool IsDead => npcCmd == NPCCMD.do_death || HPCur <= 0;
    public bool IsAlive => !IsDead && controller != null && controller.GetAppearance().parent.activeSelf;
    private game.resource.settings.NpcRes.Normal controller;
    private GameObject selectGameObject;

    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }

    public void SyncNpcHP()
    {
        if (controller == null)
        {
            return;
        }

        float HPPecent = CalculateHPPercentage(CurrentHPCur, CurrentHPMax) * 100;
        controller.SetHealthPercent((int)HPPecent);
    }

    static float CalculateHPPercentage(int current, int maxHP)
    {
        if (current <= 0 || maxHP <= 0)
            return 0;

        return (float)current / maxHP;
    }

    public game.resource.settings.NpcRes.Normal GetController() => controller;

    public void MarkClassicSynced()
    {
        LastClassicSyncTime = Time.time;
    }

    public void BuildNPC(int npcId, game.resource.settings.NpcRes.Normal controller, GameObject selectGameObject)
    {
        this.npcId = npcId;
        this.controller = controller;
        this.selectGameObject = selectGameObject;

        this.selectGameObject.SetActive(false);
    }

    public void ChangeSelect(bool isSelect)
    {
        if (isSelect && IsDead)
        {
            isSelect = false;
        }

        this.selectGameObject.SetActive(isSelect);
    }

    public void InitNpcDetail(NPCKIND kind, NPCSERIES series, NPCCAMP cam,
        int HPMax, int HPCur, byte level, int npcType, int mapX, int mapY, string name, byte dir)
    {
        if (controller != null)
        {
            this.npcType = npcType;
            controller.SetNpcDeclareLine(this.npcType + 2);

            this.dir = dir;
            controller.SyncDirection(this.dir);

            this.mapX = mapX;
            this.mapY = mapY;
            controller.SetMapPosition(new game.resource.map.Position(this.mapY / 2, this.mapX));

            this.npcName = name;
            controller.SetName(this.npcName);

            this.kind = kind;
            this.cam = cam;
            this.series = series;
            this.HPMax = HPMax;
            this.HPCur = HPCur;
            this.level = level;
            controller.data.m_Kind = (game.resource.settings.npcres.Datafield.NPCKIND)(int)this.kind;
            controller.data.m_CurrentLifeMax = this.HPMax;
            controller.data.m_CurrentLife = this.HPCur;
            this.npcCmd = HPCur > 0 ? NPCCMD.do_stand : NPCCMD.do_death;
            MarkClassicSynced();
            if (HPCur > 0)
            {
                CancelDeathRemoval();
            }

            if (this.kind == NPCKIND.kind_normal)
            {
                controller.SetHealthPercent((int)(CalculateHPPercentage(this.HPCur, this.HPMax) * 100));
                controller.SetCamp(MapCampClient(this.cam));
                controller.SetSeries(MapSeriesClient(this.series));
            }
        }
    }

    public void SetGold(byte gold) => controller.GetIdentify().SetGold(gold);

    public void DoSkill(int id, byte level, int targetId = -1)
    {
        if (controller != null && IsAlive)
        {
            NpcAction.DoAction(controller, NPCCMD.do_attack);
        }
    }

    public void MarkAlive()
    {
        if (HPCur <= 0)
        {
            return;
        }

        if (npcCmd == NPCCMD.do_death || npcCmd == NPCCMD.do_revive)
        {
            npcCmd = NPCCMD.do_stand;
        }

        CancelDeathRemoval();
    }

    public void MarkDead(float removeDelaySeconds)
    {
        npcCmd = NPCCMD.do_death;
        CurrentHPCur = 0;
        ChangeSelect(false);
        if (controller != null)
        {
            NpcAction.DoAction(controller, NPCCMD.do_death);
        }

        if (removeAfterDeathCoroutine != null)
        {
            StopCoroutine(removeAfterDeathCoroutine);
        }

        removeAfterDeathCoroutine = StartCoroutine(RemoveAfterDeath(removeDelaySeconds));
    }

    private IEnumerator RemoveAfterDeath(float removeDelaySeconds)
    {
        yield return new WaitForSeconds(Mathf.Max(0.05f, removeDelaySeconds));

        removeAfterDeathCoroutine = null;
        if (IsDead && PhotonManager.Instance != null)
        {
            PhotonManager.Instance.NpcMgrs?.DelNpc(npcId);
        }
    }

    private void CancelDeathRemoval()
    {
        if (removeAfterDeathCoroutine == null)
        {
            return;
        }

        StopCoroutine(removeAfterDeathCoroutine);
        removeAfterDeathCoroutine = null;
    }

    private void OnMouseUp()
    {
        if (IsDead)
        {
            return;
        }

        var enemy = Utils.GetRelation(PlayerMain.instance, this);
        if (enemy == NPCRELATION.relation_enemy)
        {
            PhotonManager.Instance.NpcMgrs?.NpcMouseUP(npcId);
            ResolveMainSkill(out int skillId, out byte skillLevel);

            Dictionary<byte, object> opParameters = new()
            {
                { (byte)ParamterCode.Id, npcId},
                { (byte)ParamterCode.NpcType, npcId},
                { (byte)ParamterCode.SkillId, skillId},
                { (byte)ParamterCode.SkillLevel, skillLevel},
            };
            PhotonManager.Instance.TrySendOperation(OperationCode.NpcSkill, opParameters);
        }
        else
        {
            controller.SetAction(game.resource.settings.NpcRes.Action.normalStand2);
            controller.OnActionEnd(game.resource.settings.NpcRes.Action.normalStand2, () =>
            {
                controller.SetAction(game.resource.settings.NpcRes.Action.normalStand1);
                return null;
            });

            Dictionary<byte, object> opParameters = new()
            {
                { (byte)ParamterCode.Id, npcId},
            };
            PhotonManager.Instance.TrySendOperation(OperationCode.NpcQuery, opParameters);
        }
    }

    private static void ResolveMainSkill(out int skillId, out byte skillLevel)
    {
        string locationSkill = PlayerPrefsKey.USER_SKILL_LOCATION + MainSkillLocation;
        int targetSkillId = PlayerPrefs.GetInt(locationSkill, FallbackBasicAttackSkillId);

        Dictionary<ushort, PlayerSkill> playerSkills = PhotonManager.Instance.GetPlayerSkill();
        if (targetSkillId > 0 &&
            playerSkills != null &&
            playerSkills.TryGetValue((ushort)targetSkillId, out PlayerSkill skill))
        {
            skillId = skill.id;
            skillLevel = skill.level > 0 ? skill.level : FallbackBasicAttackSkillLevel;
            return;
        }

        skillId = FallbackBasicAttackSkillId;
        skillLevel = FallbackBasicAttackSkillLevel;
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        //Debug.Log("VA CHAM VOI PLAYER");
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        //Debug.Log("VA CHAM VOI PLAYER");
    }

    public Camp MapCampClient(NPCCAMP camp)
    {
        return camp switch
        {
            NPCCAMP.camp_balance => Camp.Balance,
            NPCCAMP.camp_evil => Camp.Evil,
            NPCCAMP.camp_justice => Camp.Justice,
            NPCCAMP.camp_free => Camp.Free,
            _ => Camp.Begin,
        };
    }

    public Series MapSeriesClient(NPCSERIES series)
    {
        return series switch
        {
            NPCSERIES.series_earth => game.resource.settings.npcres.Identification.Series.Earth,
            NPCSERIES.series_fire => game.resource.settings.npcres.Identification.Series.Fire,
            NPCSERIES.series_water => game.resource.settings.npcres.Identification.Series.Water,
            NPCSERIES.series_wood => game.resource.settings.npcres.Identification.Series.Wood,
            NPCSERIES.series_metal => game.resource.settings.npcres.Identification.Series.Metal,
            _ => game.resource.settings.npcres.Identification.Series.Metal,
        };
    }
}
