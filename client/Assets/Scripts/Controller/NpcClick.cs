using System.Collections.Generic;
using game.network;
using game.network.listener;
using Photon.ShareLibrary.Constant;
using Photon.ShareLibrary.Entities;
using Photon.ShareLibrary.Handlers;
using Photon.ShareLibrary.Utils;
using UnityEngine;
using static game.resource.settings.npcres.Identification;

public class NpcClick : MonoBehaviour, ICharacterObj
{
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

    private int HPMax, HPCur;
    public int CurrentHPMax { get { return HPMax; } set { HPMax = value; SyncNpcHP(); } }
    public int CurrentHPCur { get { return HPCur; } set { HPCur = value; SyncNpcHP(); } }
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

    public void BuildNPC(int npcId, game.resource.settings.NpcRes.Normal controller, GameObject selectGameObject)
    {
        this.npcId = npcId;
        this.controller = controller;
        this.selectGameObject = selectGameObject;

        this.selectGameObject.SetActive(false);
    }

    public void ChangeSelect(bool isSelect)
    {
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

            if (this.kind == NPCKIND.kind_normal)
            {
                controller.SetHealthPercent((int)(CalculateHPPercentage(this.HPCur, this.HPMax) * 100));
                controller.SetCamp(MapCampClient(this.cam));
                controller.SetSeries(MapSeriesClient(this.series));
            }
        }
    }

    public void SetGold(byte gold) => controller.GetIdentify().SetGold(gold);

    public void DoSkill(int id, byte level)
    {
        NpcAction.DoAction(controller, NPCCMD.do_attack);
    }

    private void OnMouseUp()
    {
        var enemy = Utils.GetRelation(PlayerMain.instance, this);
        if (enemy == NPCRELATION.relation_enemy)
        {
            //npcListener.NpcMouseUP(npcId);

            Dictionary<byte, object> opParameters = new()
            {
                { (byte)ParamterCode.Id, npcId},
                { (byte)ParamterCode.SkillId, 53},
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
