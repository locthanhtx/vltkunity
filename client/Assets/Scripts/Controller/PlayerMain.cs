using System.Collections.Generic;
using game.network;
using Photon.ShareLibrary.Constant;
using Photon.ShareLibrary.Handlers;
using game.network.listener;
using game.resource.settings;
using Photon.ShareLibrary.Entities;
using game.resource.settings.npcres;
using game.resource.settings.item;
using UnityEngine;
using Photon.ShareLibrary.Utils;
using Unity.VisualScripting;
using game.scene.world.userInterface;

public class PlayerMain : CharacterClick, IMainPlayerClientListener
{
    public static PlayerMain instance;
    private CharacterData characterDetail;
    public override bool IsMaster { get { return true; } }
    public override NPCSERIES Series { get { return (NPCSERIES)PhotonManager.Instance.character.Fiveprop; } set { } }
    public override NPCCAMP CurrentCamp { get { return (NPCCAMP)PhotonManager.Instance.character.Camp; } set { } }
    public override bool FightMode { get { return PhotonManager.Instance.character.FightMode; } set { PhotonManager.Instance.character.FightMode = value; } }

/*
    public PanelUserEquipment equipTab;
    public PanelUserItems itemTab;
    public PanelUserProperties propertiesTab;
    public ViewportItem viewportItem;
*/
    // EquipItem Infor
    private Item item;
    private int bagCellIndex = -1;

    // UnEquipItem
    private PanelUserEquipment.Cell cell;

    // Player HP/MP/SP
    public override int HPMax { get { return PhotonManager.Instance.character.MaxLife; } set { PhotonManager.Instance.character.MaxLife = value; } }
    public override int HPCur { get { return PhotonManager.Instance.character.CurLife; } set { PhotonManager.Instance.character.CurLife = value; } }
    public override int MPMax { get { return PhotonManager.Instance.character.MaxInner; } set { PhotonManager.Instance.character.MaxInner = value; } }
    public override int MPCur { get { return PhotonManager.Instance.character.CurInner; } set { PhotonManager.Instance.character.CurInner = value; } }
    public override int SPMax { get { return PhotonManager.Instance.character.MaxStamina; } set { PhotonManager.Instance.character.MaxStamina = value; } }
    public override int SPCur { get { return PhotonManager.Instance.character.CurStamina; } set { PhotonManager.Instance.character.CurStamina = value; } }

    public bool IsHaveHorse = false;
    public bool IsUseHorse = false;

    private GameObject PlayerChatHandler;
    private GameObject PlayerCallNpcHandler;
    private GameObject NpcSellectObject;

    public int Exp { get { return PhotonManager.Instance.character.FightExp; } set { PhotonManager.Instance.character.FightExp = value; } }
    public byte Level { get { return PhotonManager.Instance.character.FightLevel; } set { PhotonManager.Instance.character.FightLevel = value; } }

    void Start()
    {
        instance = this;
        PhotonManager.Instance.SetMainPlayerClientListener(this);
    }

    private void OnMouseUp()
    {
        if (PlayerCallNpcHandler.activeSelf && NpcSellectObject != null)
        {
            NpcClick npcClick = NpcSellectObject.GetComponent<NpcClick>();
            npcClick.ChangeSelect(true);
            npcClick.GetController().SetAction(NpcRes.Action.normalStand2);
            npcClick.GetController().OnActionEnd(NpcRes.Action.normalStand2, () =>
            {
                npcClick.GetController().SetAction(NpcRes.Action.normalStand1);
                return null;
            });

            Dictionary<byte, object> opParameters = new()
                {
                { (byte)ParamterCode.Id, npcClick.Id},
            };
            PhotonManager.Instance.TrySendOperation(OperationCode.NpcQuery, opParameters);
        }
        else
        {
            PopUpCanvas.instance.OpenPlayerPopUp();
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.name.Contains("NpcRes+Normal"))
        {
            NpcClick npcClick = collision.gameObject.GetComponent<NpcClick>();
            var enemy = Utils.GetRelation(PlayerMain.instance, npcClick);
            if (enemy != NPCRELATION.relation_enemy)
            {
                PlayerCallNpcHandler.SetActive(true);
                NpcSellectObject = collision.gameObject;
            }
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (NpcSellectObject != null)
        {
            NpcClick npcClick = NpcSellectObject.GetComponent<NpcClick>();
            npcClick.ChangeSelect(false);
            PlayerCallNpcHandler.SetActive(false);
            NpcSellectObject = null;
        }
    }
/*
    public void SetUIWord(PanelUserEquipment equipTab, PanelUserProperties propertiesTab, PanelUserItems itemTab, ViewportItem viewportItem)
    {
        this.equipTab = equipTab;
        this.propertiesTab = propertiesTab;
        this.itemTab = itemTab;
        this.viewportItem = viewportItem;
    }
*/
    public void InitCharacter(int RegionId, NpcRes.Special mainControler, GameObject PlayerChatHandler, GameObject PlayerCallNpcHandler)
    {
        characterDetail = PhotonManager.Instance.GetChracter();
        this.id = RegionId;
        this.controller = mainControler;
        this.Name = characterDetail.Name;

        //RepeatedField<Faction> factions = PhotonManager.Instance.factionList.FactionList_;
        //Faction faction = factions.First(f => f.Id == characterDetail.Sect);

        this.Kind = NPCKIND.kind_player;

        //controller.SetCamp(MapCampClient(faction.Camp.ToString()));
        controller.SetCamp((Identification.Camp)characterDetail.Camp);

        controller.SetName(characterDetail.Name);
        controller.SetCharacterType(characterDetail.Sex? NpcRes.SpecialType.man : NpcRes.SpecialType.lady);
/*
        propertiesTab.SetHighLight(1234);
        propertiesTab.SetName(characterDetail.Name);
        //propertiesTab.SetId(characterDetail.Uid);
        //propertiesTab.SetVerificationState(string.IsNullOrEmpty(characterDetail.Uid));
        //propertiesTab.SetTitle(characterDetail.Uid);
        propertiesTab.SetExp(characterDetail.FightExp.ToString());

        if (0 <= characterDetail.Sect && characterDetail.Sect <= 9)
            propertiesTab.SetFaction(characterDetail.Sect);

        //propertiesTab.SetRank(characterDetail.WorldLevelSort);

        propertiesTab.SetPhucDuyen(characterDetail.Luck);
        propertiesTab.SetPk(10);
        propertiesTab.SetDanhVong(123);
        propertiesTab.SetHealth(HPMax);
        propertiesTab.SetMana(MPMax);
        propertiesTab.SetStamina(SPMax);

        //propertiesTab.SetSeries((int)MapSeriesClient(faction.Series.ToString()));
        propertiesTab.SetSeries(characterDetail.Fiveprop);

        propertiesTab.SetSucManh(characterDetail.Power);
        propertiesTab.SetNoiCong(characterDetail.Inside);
        propertiesTab.SetSinhKhi(characterDetail.Outer);
        propertiesTab.SetThanPhap(characterDetail.Agility);
        propertiesTab.SetTiemNang(characterDetail.LeftProp);
        propertiesTab.SetLevel(characterDetail.FightLevel);

        SetUpPlayerItem();
*/
        this.PlayerChatHandler = PlayerChatHandler;
        this.PlayerCallNpcHandler = PlayerCallNpcHandler;
    }

    public void PlayerSit()
    {
        if (!IsUseHorse)
        {
            PhotonManager.Instance.TrySendOperation(OperationCode.DoSit, new Dictionary<byte, object>()
            {
            });
        }
    }

    public void PlayerRun()
    {
        PhotonManager.Instance.TrySendOperation(OperationCode.DoRun, new Dictionary<byte, object>()
        {
        });
    }

    public bool PlayerSwitchHorse()
    {
        SetHorseRidingLocal(!IsUseHorse);
        PhotonManager.Instance?.RequestClassicRideToggle();
        return IsUseHorse;
    }

    public void SetHorseRidingLocal(bool isUseHorse)
    {
        IsUseHorse = isUseHorse;
        if (isUseHorse)
        {
            IsHaveHorse = true;
        }

        if (this.controller != null)
        {
            this.controller.SetRiding(IsUseHorse);
        }

        UpdateHorseUI();
    }

    void UpdateHorseUI()
    {
        MainCanvas.instance?.CommonSwitch()?.GetComponent<ActionSp>()?.UpdateHosreUI(IsUseHorse);
    }

    // SYNC ITEM
    void SetUpPlayerItem()
    {
/*
        foreach (var data in PhotonManager.Instance.GetPlayerItems())
        {
            ItemData itemData = data.Value;
            Item item = RestoreItemFromDatabase(itemData);

            if (itemData.Local == (byte)ItemPosition.pos_equip)
            {
                this.equipTab.EquipItem(item);
                this.EquipItemToPlayer(item);
            }
            else
            {
                this.itemTab.AddItem(item);
            }
        }
*/
    }

    Item RestoreItemFromDatabase(ItemData itemdata)
    {
        return ItemUiMapper.RestoreItem(itemdata);
    }

    public void SyncNewItem(ItemData itemData)
    {
        PopUpCanvas.instance?.RefreshStorageIfOpen();
/*
        Item item = RestoreItemFromDatabase(itemData);
        if (itemData.Local == (byte)ItemPosition.pos_equip)
        {
            //this.equipTab.EquipItem(item);
            this.EquipItemToPlayer(item);
        }
        else
        {
            this.itemTab.AddItem(item);
        }
*/
    }

    public void SyncUpdateItem(ItemData itemData)
    {
        PopUpCanvas.instance?.RefreshStorageIfOpen();
/*
        if (itemData.Local == (byte)ItemPosition.pos_equip)
        {
            if (this.item == null)
            {
                ShowMessageBox("Trang bị không thành công!", "error");
                return;
            }


            if (itemData.id != this.item.GetDatabaseId())
            {
                ShowMessageBox("Trang bị không thành công!", "error");
                return;
            }

            EquipItemToPlayer(item);
        }
        else
        {
            if (item != null && bagCellIndex > 0)
            {
                Item previousEquip = this.equipTab.EquipItem(item);
                this.itemTab.ClearCell(bagCellIndex);
                if (previousEquip != null)
                {
                    this.itemTab.AddItem(previousEquip, bagCellIndex);
                }
            }
            else
            {
                if (this.cell == null)
                {
                    ShowMessageBox("Gỡ trang bị không thành công!", "error");
                    return;
                }
                Item cellItem = cell.item;
                if (itemData.id != cellItem.GetDatabaseId())
                {
                    ShowMessageBox("Gỡ trang bị không thành công!", "error");
                    return;
                }
                UnequipItem(cell);
            }
        }
*/
    }

    // Storage
    public void MoveItemToStorage(Item item)
    {
/*
        viewportItem.Open(
             item,
             new Dictionary<ViewportItem.Button, System.Action>()
             {
                    {ViewportItem.Button.place, () => { UnityEngine.Debug.Log("on button place click"); } }
             }
         );
*/
    }

    public void MoveStorageToBags(Item item)
    {
/*
        viewportItem.Open(
            item,
            new Dictionary<ViewportItem.Button, System.Action>()
            {
                    {ViewportItem.Button.retrieve, () => { UnityEngine.Debug.Log("on button retrieve click"); } }
            }
        );
*/
    }

    // EquipItemFromBag
    public void RequestEquipItemFromBag(Item item, int bagCellIndex)
    {
        RequestEquipItemFromBag(item, null, bagCellIndex);
    }

    public void RequestEquipItemFromBag(Item item, ItemData itemData, int bagCellIndex)
    {
        this.item = item;
        this.bagCellIndex = bagCellIndex;
        if (itemData != null)
        {
            Debug.Log("PlayerMain request use item id=" + itemData.id +
                      " local=" + itemData.Local +
                      " x=" + itemData.X +
                      " y=" + itemData.Y +
                      " equip=" + (item != null && item.IsEquipment()));
        }

        ChangeEquip(item.GetDatabaseId(), true);
    }

    public void EquipItemToPlayer(Item item)
    {
        EquipmentBase equipmentBase = item.GetEquipmentBase();

        if (equipmentBase != null
               && equipmentBase.genre == (int)Defination.Genre.item_equip)
        {   // thay đổi hình ảnh nhân vật chính trong game, dựa vào vật phẩm vừa sử dụng

            Dictionary<int, System.Action<int>[]> resProcess = new Dictionary<int, System.Action<int>[]>()
                {
                    {(int)Defination.Detail.equip_meleeweapon, new System.Action<int>[2]{ this.controller.SetWeaponItemLine, this.controller.SetWeaponRes }},
                    {(int)Defination.Detail.equip_rangeweapon, new System.Action<int>[2]{ this.controller.SetWeaponItemLine, this.controller.SetWeaponRes }},
                    {(int)Defination.Detail.equip_armor, new System.Action<int>[2]{ this.controller.SetBodyItemLine, this.controller.SetBodyRes }},
                    {(int)Defination.Detail.equip_helm, new System.Action<int>[2]{ this.controller.SetHeadItemLine, this.controller.SetHeadRes }},
                    {(int)Defination.Detail.equip_horse, new System.Action<int>[2]{ this.controller.SetHorseItemLine, this.controller.SetHorseRes }},
                };

            if (resProcess.ContainsKey(equipmentBase.detail))
            {
                int res = equipmentBase.rowIndex + 1;

                if (item.GetItemType() == Defination.Type.goldEquip)
                {
                    res = game.resource.settings.item.Getters.GetGoldEquipRes(equipmentBase.rowIndex) - 1;
                }
                resProcess[equipmentBase.detail][(int)item.GetItemType()](res);
            }

            if (equipmentBase.detail == (int)Defination.Detail.equip_horse)
            {
                this.IsHaveHorse = true;
                SetHorseRidingLocal(true);
            }
        }

        item = null;
        bagCellIndex = -1;
        cell = null;
    }

    // UnequipItem
    public void RequestUnequipItem(PanelUserEquipment.Cell cell)
    {
/*
        this.cell = cell;
        Item item = cell.item;
        ChangeEquip(item.GetDatabaseId(), false);
*/
    }

    public void UnequipItem(PanelUserEquipment.Cell cell)
    {
/*
        Item item = cell.item;
        cell.Clear();
        this.itemTab.AddItem(item);
        EquipmentBase equipmentBase = item.GetEquipmentBase();
        if (equipmentBase == null)
        {
            return;
        }

        if (equipmentBase.genre == (int)Defination.Genre.item_equip)
        {
            switch (equipmentBase.detail)
            {
                case (int)Defination.Detail.equip_meleeweapon:
                    this.controller.SetWeaponItemDefault();
                    break;

                case (int)Defination.Detail.equip_armor:
                    this.controller.SetBodyItemDefault();
                    break;

                case (int)Defination.Detail.equip_helm:
                    this.controller.SetHeadItemDefault();
                    break;

                case (int)Defination.Detail.equip_horse:
                    this.controller.SetHorseItemDefault();
                    this.IsHaveHorse = false;
                    SetHorseRidingLocal(false);
                    break;
            }
        }

        item = null;
        bagCellIndex = -1;
        cell = null;
*/
    }

    // SELL
    public void RequestSellItemFromBag(Item item, int bagCellIndex)
    {
        this.item = item;
        this.bagCellIndex = bagCellIndex;

        PhotonManager.Instance.TrySendOperation(OperationCode.RemoveItem, new Dictionary<byte, object>()
        {
            [(byte)ParamterCode.ItemId] = item.GetDatabaseId(),
        });
    }

    public void SyncRemoveItem()
    {
        PopUpCanvas.instance?.RefreshStorageIfOpen();
/*
        Debug.Log("ClearCell" + bagCellIndex);
        itemTab.ClearCell(bagCellIndex);
*/
    }

    public void ChangeEquip(uint itemId, bool isEquip)
    {
        PhotonManager.Instance.TrySendOperation(OperationCode.AutoEquip, new Dictionary<byte, object>()
        {
            [(byte)ParamterCode.ItemId] = itemId,
            [(byte)ParamterCode.IsEquip] = isEquip,
        });
    }

    public Dictionary<ushort, PlayerSkill> playerSkills() => PhotonManager.Instance.GetPlayerSkill();

    public void SyncAddSkill(PlayerSkill skill)
    {

    }

    public void SyncUpdateSkill(PlayerSkill skill)
    {

    }

    public void SynCharMove(int left, int top)
    {
        SynCharMoveMps(left, top * 2);
    }

    public void SynCharMoveMps(int left, int mapY)
    {
        Dictionary<byte, object> opParameters = new()
        {
            {(byte) ParamterCode.MapId, 0 },
            {(byte) ParamterCode.MapX, left},
            {(byte) ParamterCode.MapY, mapY},
        };
        if (!PhotonManager.Instance.TrySendOperation(OperationCode.DoMove, opParameters))
        {
            PhotonManager.Instance.world?.TeleportMps(left, mapY);
        }
    }

    public Identification.Camp MapCampClient(string camp)
    {
        return camp switch
        {
            "Balance" => Identification.Camp.Balance,
            "Evil" => Identification.Camp.Evil,
            "Justice" => Identification.Camp.Justice,
            "Free" => Identification.Camp.Free,
            _ => Identification.Camp.Begin,
        };
    }

    public Identification.Series MapSeriesClient(string series)
    {
        return series switch
        {
            "Earth" => Identification.Series.Earth,
            "Fire" => Identification.Series.Fire,
            "Water" => Identification.Series.Water,
            "Wood" => Identification.Series.Wood,
            _ => Identification.Series.Metal,
        };
    }

    public void SyncTask()
    {

    }

    public void SyncChracter()
    {
    }

    public void ChangeWorld()
    {
    }

    public override void DoAudio(NPCCMD cmd)
    {
        switch (cmd)
        {
            case NPCCMD.do_stand:
                MusicManagerGame.Instance.StopFootStep();
                break;

            case NPCCMD.do_hurt:
                MusicManagerGame.Instance.Hurt();
                break;

            case NPCCMD.do_death:
                MusicManagerGame.Instance.Die(characterDetail.Sex);
                break;

            case NPCCMD.do_revive:

                break;

            case NPCCMD.do_walk:
                MusicManagerGame.Instance.StartFootStep(IsUseHorse);
                break;
            case NPCCMD.do_run:
                MusicManagerGame.Instance.StartFootStep(IsUseHorse);
                break;
            case NPCCMD.do_attack:
                MusicManagerGame.Instance.Hit(characterDetail.Sex);
                break;
        }
    }

}
