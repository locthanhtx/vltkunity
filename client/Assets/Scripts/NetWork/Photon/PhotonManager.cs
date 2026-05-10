using ExitGames.Client.Photon;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

using Photon.ShareLibrary.Handlers;
using System.Reflection;
using game.network.jx;
using game.network.listener;
using UnityEngine.SceneManagement;
using game.ui;
using game.config;
using Photon.ShareLibrary.Constant;
using Photon.ShareLibrary.Entities;
using Photon.ShareLibrary.Utils;

public class PhotonManager : MonoBehaviour, IPhotonPeerListener
{
    private static PhotonManager instance;
    public PhotonPeer client;

    public game.scene.World world;
    public game.scene.CharManager CharMgrs;
    public game.scene.NpcManager NpcMgrs;
    public JxClassicClient ClassicClient { get; private set; }

    List<MessageHandlers> resHandlers = new List<MessageHandlers>();
    List<MessageHandlers> syncHandlers = new List<MessageHandlers>();
    private bool isConnected = false;
    private bool isDestroy = false;
    private readonly Dictionary<int, ClassicPlayerSync> pendingClassicPlayerSyncs = new();
    private const float ClassicNpcNormalSyncDuration = 0.22f;
    private const float ClassicRunEchoIgnoreSeconds = 0.35f;
    private const float ClassicSkillMinSendInterval = 0.16f;
    private const float ClassicSkillMaxSendInterval = 0.75f;
    private bool classicLocalMovementActive;
    private float classicRunEchoIgnoreUntil;
    private float nextClassicSkillSendTime;

    [SerializeField]
    private bool usePhotonServer = false;

    string PHOTON_SERVER_URL = "154.26.129.47:4530";

    GameObject reConnect;


    public static PhotonManager Instance
    {
        get
        {
            if (instance == null)
            {
                GameObject singletonObject = new();
                instance = singletonObject.AddComponent<PhotonManager>();
                singletonObject.name = "PhotonManager (Singleton)";
                DontDestroyOnLoad(singletonObject);
            }
            return instance;
        }
    }

    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
        }
        else
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
    }

    void Start()
    {
        LoadHandles();
        RegisterTypes();

        if (!usePhotonServer)
        {
            Debug.Log("----- Photon disabled for classic JX client flow");
            return;
        }

        client = new PhotonPeer(this, ConnectionProtocol.Tcp);
        client.TimePingInterval = 3000;

        if (client.Connect(PHOTON_SERVER_URL, "JXServer"))
        {
            Debug.Log("----- Photon Connecting");
        }
    }

    void LoadHandles()
    {
        var types = Assembly.GetExecutingAssembly().GetTypes();
        foreach (var type in types)
        {
            if (type.Namespace == "Photon.ShareLibrary.Handlers")
            {
                try
                {
                    var temp = (MessageHandlers)Activator.CreateInstance(type);
                    if (temp.Type == MessageType.Response)
                        resHandlers.Add(temp);
                    else
                        syncHandlers.Add(temp);
                }
                catch (Exception ex)
                {
                    Debug.LogError((ex.InnerException ?? ex).ToString());
                }
            };
        }
    }
    public void RegisterTypes()
    {
        PhotonPeer.RegisterType(typeof(ushort), (byte)CustomTypeCode.UShort,
                                       SerializerMethods.SerializeUShort,
                                       SerializerMethods.DeserializeUShort);

        PhotonPeer.RegisterType(typeof(uint), (byte)CustomTypeCode.UInt,
                                       SerializerMethods.SerializeUInt,
                                       SerializerMethods.DeserializeUInt);

        PhotonPeer.RegisterType(typeof(ulong), (byte)CustomTypeCode.ULong,
                                       SerializerMethods.SerializeULong,
                                       SerializerMethods.DeserializeULong);

        //PhotonPeer.RegisterType(typeof(MmoGuid), (byte)CustomTypeCode.Guid,
        //                               GlobalSerializerMethods.SerializeGuid,
        //                               GlobalSerializerMethods.DeserializeGuid);
    }

    public void ReConnect()
    {
        if (!usePhotonServer || client == null)
        {
            return;
        }

        client.Connect(PHOTON_SERVER_URL, "JXServer");
    }

    public void ShowReConnect()
    {
        if (isDestroy)
        {
            return;
        }
        Scene currentScene = SceneManager.GetActiveScene();

        if (isConnected && reConnect == null && currentScene.name == "GameWorldScene")
        {
            reConnect = UIHelpers.BringPrefabToScene("Reconnect");
            reConnect.GetComponent<Reconnect>().SetError("Mất kết nối đến máy chủ. Thử lại hoặc thoát game.");
        }
    }

    public void OnDisableReConnect()
    {
        if (reConnect != null)
        {
            Destroy(reConnect);
        }
    }

    public void AttachClassicClient(JxClassicClient classicClient)
    {
        if (ClassicClient != null && ClassicClient != classicClient)
        {
            ClassicClient.Dispose();
        }

        ClassicClient = classicClient;
    }

    public void SetClassicLocalMovementActive(bool active)
    {
        classicLocalMovementActive = active;
        if (!active)
        {
            classicRunEchoIgnoreUntil = Time.time + ClassicRunEchoIgnoreSeconds;
        }
    }

    public void DebugReturn(DebugLevel level, string message)
    {
        Debug.Log(message);
    }

    bool ischarge = false;
    bool issync = false;

    public void ServerCharge(string ip, bool sync)
    {
        ischarge = true;
        issync = sync;

        if (!usePhotonServer || client == null)
        {
            return;
        }

        client.Disconnect();
        client.Connect(ip, "JXServer");
    }

    public void OnStatusChanged(StatusCode statusCode)
    {
        switch (statusCode)
        {
            case StatusCode.Connect:
                Debug.Log("----- Photon Connected");
                isConnected = true;
                if (ischarge)
                {
                    Dictionary<byte, object> opParameters = new()
                        {
                            {(byte) ParamterCode.UserId, (uint)PlayerPrefs.GetInt(PlayerPrefsKey.USER_ID)},
                            {(byte) ParamterCode.CharacterId, (uint)PlayerPrefs.GetInt(PlayerPrefsKey.CHARACTER_ID)},
                            {(byte) ParamterCode.Data, issync},
                        };
                    TrySendOperation(OperationCode.WorldJoin, opParameters);
                    ischarge = false; return;
                }

                OnDisableReConnect();
                break;

            case StatusCode.Disconnect:
                if (ischarge)
                    return;

                Debug.Log("----- Photon DisConnected");
                ShowReConnect();
                isConnected = false;
                break;
        }
    }

    public void OnOperationResponse(OperationResponse op)
    {

        Debug.Log((OperationCode)op.OperationCode);
        if (op.DebugMessage != null)
        {
            Debug.Log(op.DebugMessage);
        }
        foreach (var handle in resHandlers)
        {
            if (handle.Code == (OperationCode)op.OperationCode)
            {
                try
                {
                    handle.Process(op.ReturnCode, op.Parameters);
                }
                catch (Exception e)
                {
                    Debug.LogError(e);
                }
                break;
            }
        }
    }

    public void OnEvent(EventData ev)
    {
        Debug.Log((OperationCode)ev.Code);
        foreach (var handle in syncHandlers)
        {
            if (handle.Code == (OperationCode)ev.Code)
            {
                try
                {
                    handle.Process(0, ev.Parameters);
                }
                catch (Exception e)
                {
                    Debug.LogError(e);
                }
                break;
            }
        }
    }

    int msDeltaForServiceCalls = 50, msTimestampOfLastServiceCall = 0;

    // Update is called once per frame
    void Update()
    {
        ProcessClassicWorldEvents();

        if (this.client == null)
            return;

        while (this.client.DispatchIncomingCommands())
        {
        }
        if (Environment.TickCount - this.msTimestampOfLastServiceCall > this.msDeltaForServiceCalls || this.msTimestampOfLastServiceCall == 0)
        {
            this.msTimestampOfLastServiceCall = Environment.TickCount;

            while (this.client.SendOutgoingCommands())
            {
            }
        }
    }

    private void ProcessClassicWorldEvents()
    {
        if (ClassicClient == null || world == null || CharMgrs == null || NpcMgrs == null)
        {
            return;
        }

        int processed = 0;
        while (processed++ < 128 && ClassicClient.TryDequeueWorldEvent(out ClassicWorldEvent worldEvent))
        {
            try
            {
                switch (worldEvent.Type)
                {
                    case ClassicWorldEventType.CurrentPlayerSync:
                        ApplyClassicCurrentPlayerSync(worldEvent.CurrentPlayer);
                        break;

                    case ClassicWorldEventType.CurrentPlayerNormalSync:
                        ApplyClassicCurrentPlayerNormalSync(worldEvent.Character);
                        break;

                    case ClassicWorldEventType.NpcFullSync:
                        ApplyClassicNpcSync(worldEvent.Npc, true);
                        break;

                    case ClassicWorldEventType.NpcNormalSync:
                        ApplyClassicNpcSync(worldEvent.Npc, false);
                        break;

                    case ClassicWorldEventType.PlayerFullSync:
                    case ClassicWorldEventType.PlayerNormalSync:
                        ApplyClassicPlayerSync(worldEvent.Player);
                        break;

                    case ClassicWorldEventType.PlayerPositionSync:
                        ApplyClassicPositionSync(worldEvent.Position);
                        break;

                    case ClassicWorldEventType.ActorCommandSync:
                        ApplyClassicActorCommandSync(worldEvent.Command);
                        break;

                    case ClassicWorldEventType.PlayerAttributeSync:
                        ApplyClassicAttributeSync(worldEvent.Attribute);
                        break;

                    case ClassicWorldEventType.PlayerSkillListSync:
                        ApplyClassicSkillListSync(worldEvent.SkillList);
                        break;

                    case ClassicWorldEventType.PlayerSkillLevelSync:
                        ApplyClassicSkillLevelSync(worldEvent.Skill);
                        break;

                    case ClassicWorldEventType.ItemSync:
                        ApplyClassicItemSync(worldEvent.Item);
                        break;

                    case ClassicWorldEventType.ItemRemoveSync:
                        ApplyClassicItemRemoveSync(worldEvent.ItemRemove);
                        break;

                    case ClassicWorldEventType.MoneySync:
                        ApplyClassicMoneySync(worldEvent.Money);
                        break;

                    case ClassicWorldEventType.XuSync:
                        ApplyClassicXuSync(worldEvent.Xu);
                        break;

                    case ClassicWorldEventType.ItemMoveSync:
                        ApplyClassicItemMoveSync(worldEvent.ItemMove);
                        break;

                    case ClassicWorldEventType.AutoEquipSync:
                        ApplyClassicAutoEquipSync(worldEvent.AutoEquip);
                        break;

                    case ClassicWorldEventType.SkillPropPointSync:
                        ApplyClassicSkillPropPointSync(worldEvent.SkillPropPoint);
                        break;

                    case ClassicWorldEventType.PlayerExpSync:
                        ApplyClassicPlayerExpSync(worldEvent.PlayerExp);
                        break;

                    case ClassicWorldEventType.LeadExpSync:
                        ApplyClassicLeadExpSync(worldEvent.LeadExp);
                        break;

                    case ClassicWorldEventType.PlayerLevelUpSync:
                        ApplyClassicPlayerLevelUpSync(worldEvent.LevelUp);
                        break;

                    case ClassicWorldEventType.SkillCastSync:
                        ApplyClassicSkillCastSync(worldEvent.SkillCast);
                        break;
                }
            }
            catch (Exception exception)
            {
                Debug.LogWarning("JxClassicClient world event skipped. type=" + worldEvent.Type +
                                 " error=" + exception.Message);
            }
        }
    }

    private void ApplyClassicCurrentPlayerSync(ClassicCurrentPlayerSync sync)
    {
        if (sync == null)
        {
            return;
        }

        if (sync.Id > 0)
        {
            PlayerId = sync.Id;
            CharMgrs?.RegisterMainPlayer(sync.Id, sync.Character?.Name);
            if (PlayerMain.instance != null)
            {
                PlayerMain.instance.id = sync.Id;
            }
        }

        JxClassicMovement.EnsureBaseSpeed(world?.GetMainPlayer());
        MergeClassicCharacterData(sync.Character, true);
    }

    private void ApplyClassicCurrentPlayerNormalSync(CharacterData sync)
    {
        MergeClassicCharacterData(sync, false);
    }

    private void MergeClassicCharacterData(CharacterData sync, bool includeStaticData)
    {
        if (sync == null)
        {
            return;
        }

        if (character == null)
        {
            character = new CharacterData();
        }

        character.CurLife = sync.CurLife > 0 ? sync.CurLife : character.CurLife;
        character.CurStamina = sync.CurStamina > 0 ? sync.CurStamina : character.CurStamina;
        character.CurInner = sync.CurInner > 0 ? sync.CurInner : character.CurInner;
        character.MaxLife = sync.MaxLife > 0 ? sync.MaxLife : character.MaxLife;
        character.MaxStamina = sync.MaxStamina > 0 ? sync.MaxStamina : character.MaxStamina;
        character.MaxInner = sync.MaxInner > 0 ? sync.MaxInner : character.MaxInner;

        if (!includeStaticData)
        {
            return;
        }

        character.FightLevel = sync.FightLevel > 0 ? sync.FightLevel : character.FightLevel;
        character.Sex = sync.Sex;
        character.Fiveprop = sync.Fiveprop;
        character.LeftProp = sync.LeftProp;
        character.LeftFight = sync.LeftFight;
        character.Power = sync.Power;
        character.Agility = sync.Agility;
        character.Outer = sync.Outer;
        character.Inside = sync.Inside;
        character.Luck = sync.Luck;
        character.FightExp = sync.FightExp;
        character.LeadExp = sync.LeadExp;
        character.Sect = sync.Sect;
        character.FirstSect = sync.FirstSect;
        character.JoinCount = sync.JoinCount;
    }

    private void ApplyClassicAttributeSync(ClassicAttributeSync sync)
    {
        if (sync == null)
        {
            return;
        }

        EnsureClassicCharacter();
        character.LeftProp = ClampUInt16(sync.LeavePoint);

        switch (sync.Attribute)
        {
            case 0:
                character.Power = ClampUInt16(sync.BasePoint);
                break;

            case 1:
                character.Agility = ClampUInt16(sync.BasePoint);
                break;

            case 2:
                character.Outer = ClampUInt16(sync.BasePoint);
                break;

            case 3:
                character.Inside = ClampUInt16(sync.BasePoint);
                break;
        }

        iMainPlayerClientListener?.SyncChracter();
    }

    private void ApplyClassicSkillLevelSync(ClassicSkillLevelSync sync)
    {
        if (sync == null || sync.SkillId <= 0)
        {
            return;
        }

        EnsureClassicCharacter();
        character.LeftFight = ClampUInt16(sync.LeavePoint);

        PlayerSkill skill = new PlayerSkill
        {
            id = ClampUInt16(sync.SkillId),
            level = ClampByte(sync.SkillLevel),
            exp = (uint)Math.Max(0, sync.SkillExp)
        };

        SetPlayerSkill(skill);
        iMainPlayerClientListener?.SyncChracter();
    }

    private void ApplyClassicSkillListSync(ClassicSkillListSync sync)
    {
        if (sync?.Skills == null)
        {
            return;
        }

        playerSkills.Clear();
        foreach (ClassicSkillLevelSync skillSync in sync.Skills)
        {
            if (skillSync == null || skillSync.SkillId <= 0)
            {
                continue;
            }

            PlayerSkill skill = new PlayerSkill
            {
                id = ClampUInt16(skillSync.SkillId),
                level = ClampByte(Math.Max(1, skillSync.SkillLevel)),
                exp = (uint)Math.Max(0, skillSync.SkillExp)
            };

            SetPlayerSkill(skill);
        }

        PopUpCanvas.instance?.RefreshSkillIfOpen();
        Debug.Log("JxClassicClient << skill list count=" + playerSkills.Count);
    }

    private void ApplyClassicItemSync(ClassicItemSync sync)
    {
        if (sync?.Item == null || sync.Item.id == 0)
        {
            return;
        }

        playerItemDetails[sync.Item.id] = sync;
        SetPlayerItem(sync.Item);
    }

    private void ApplyClassicItemRemoveSync(ClassicItemRemoveSync sync)
    {
        if (sync == null || sync.Id == 0)
        {
            return;
        }

        playerItemDetails.Remove(sync.Id);
        RemovePlayerItem(sync.Id);
    }

    private void ApplyClassicMoneySync(ClassicMoneySync sync)
    {
        if (sync == null)
        {
            return;
        }

        EnsureClassicCharacter();
        character.Money = Math.Max(0, sync.EquipMoney);
        character.SaveMoney = Math.Max(0, sync.RepositoryMoney);
        iMainPlayerClientListener?.SyncChracter();
    }

    private void ApplyClassicXuSync(ClassicXuSync sync)
    {
        if (sync == null)
        {
            return;
        }

        EnsureClassicCharacter();
        character.RoleParm1 = Math.Max(0, sync.Xu);
        iMainPlayerClientListener?.SyncChracter();
    }

    private void ApplyClassicItemMoveSync(ClassicItemMoveSync sync)
    {
        if (sync == null)
        {
            return;
        }

        if (sync.ItemId != 0 && playerItems.TryGetValue(sync.ItemId, out ItemData itemById))
        {
            itemById.Local = ClampByte(sync.UpPlace);
            itemById.X = ClampByte(sync.UpX);
            itemById.Y = ClampByte(sync.UpY);
            SetPlayerItem(itemById);
            return;
        }

        foreach (ItemData item in playerItems.Values)
        {
            if (item.Local == ClampByte(sync.DownPlace)
                && item.X == ClampByte(sync.DownX)
                && item.Y == ClampByte(sync.DownY))
            {
                item.Local = ClampByte(sync.UpPlace);
                item.X = ClampByte(sync.UpX);
                item.Y = ClampByte(sync.UpY);
                SetPlayerItem(item);
                return;
            }
        }
    }

    private void ApplyClassicAutoEquipSync(ClassicAutoEquipSync sync)
    {
        if (sync == null)
        {
            return;
        }

        if (!playerItems.TryGetValue(sync.ItemId, out ItemData item))
        {
            Debug.LogWarning("JxClassicClient auto equip callback skipped. item not found id=" + sync.ItemId);
            return;
        }

        item.Local = ClampByte(sync.DestPlace);
        item.X = ClampByte(sync.DestX);
        item.Y = ClampByte(sync.DestY);
        SetPlayerItem(item);

        Debug.Log("JxClassicClient << auto equip item id=" + sync.ItemId +
                  " kind=" + sync.Kind +
                  " src=" + sync.SourcePlace + "/" + sync.SourceX + "/" + sync.SourceY +
                  " dest=" + sync.DestPlace + "/" + sync.DestX + "/" + sync.DestY);
    }

    private void ApplyClassicSkillPropPointSync(ClassicSkillPropPointSync sync)
    {
        if (sync == null)
        {
            return;
        }

        EnsureClassicCharacter();
        character.LeftFight = ClampUInt16(sync.SkillPoint);
        character.LeftProp = ClampUInt16(sync.AttributePoint);
        iMainPlayerClientListener?.SyncChracter();
    }

    private void ApplyClassicPlayerExpSync(ClassicPlayerExpSync sync)
    {
        if (sync == null)
        {
            return;
        }

        EnsureClassicCharacter();
        character.FightExp = ClampInt32(sync.Exp);
        iMainPlayerClientListener?.SyncChracter();
    }

    private void ApplyClassicLeadExpSync(ClassicLeadExpSync sync)
    {
        if (sync == null)
        {
            return;
        }

        EnsureClassicCharacter();
        character.LeadExp = sync.LeadExp > int.MaxValue ? int.MaxValue : (int)sync.LeadExp;
        iMainPlayerClientListener?.SyncChracter();
    }

    private void ApplyClassicPlayerLevelUpSync(ClassicPlayerLevelUpSync sync)
    {
        if (sync == null)
        {
            return;
        }

        EnsureClassicCharacter();
        character.FightLevel = ClampByte(sync.Level);
        character.FightExp = ClampInt32(sync.Exp);
        character.LeftProp = ClampUInt16(sync.AttributePoint);
        character.LeftFight = ClampUInt16(sync.SkillPoint);
        character.MaxLife = ClampInt32(sync.BaseLifeMax);
        character.MaxStamina = ClampInt32(sync.BaseStaminaMax);
        character.MaxInner = ClampInt32(sync.BaseManaMax);
        iMainPlayerClientListener?.SyncChracter();
    }

    private void EnsureClassicCharacter()
    {
        if (character == null)
        {
            character = new CharacterData();
        }
    }

    private static byte ClampByte(int value)
    {
        return (byte)Math.Max(byte.MinValue, Math.Min(byte.MaxValue, value));
    }

    private static ushort ClampUInt16(int value)
    {
        return (ushort)Math.Max(ushort.MinValue, Math.Min(ushort.MaxValue, value));
    }

    private static int ClampInt32(long value)
    {
        if (value <= 0)
        {
            return 0;
        }

        return value > int.MaxValue ? int.MaxValue : (int)value;
    }

    private static int ClampInt32(uint value)
    {
        return value > int.MaxValue ? int.MaxValue : (int)value;
    }

    private void ApplyClassicNpcSync(ClassicNpcSync sync, bool hasAppearanceData)
    {
        if (sync == null || sync.Id == 0)
        {
            return;
        }

        byte direction = sync.Direction;
        bool isPlayerNpc = IsClassicPlayerNpcSync(sync);

        if (IsClassicMainPlayerNpc(sync))
        {
            if (!isPlayerNpc)
            {
                ApplyClassicPositionSync(new ClassicNpcPositionSync
                {
                    Id = sync.Id,
                    MapX = sync.MapX,
                    MapY = sync.MapY,
                    OffsetX = sync.OffsetX,
                    OffsetY = sync.OffsetY,
                    IsPlayer = true,
                    Direction = sync.Direction,
                    HasDirection = sync.HasDirection,
                    Command = sync.Doing
                });
            }

            if (hasAppearanceData && isPlayerNpc)
            {
                ApplyClassicMainPlayerFromNpcSync(sync, direction);
            }

            return;
        }

        if (!hasAppearanceData)
        {
            if (isPlayerNpc)
            {
                bool hadPlayer = CharMgrs.FindPlayer(sync.Id) != null;
                PlayerClick player = EnsureClassicPlayerSpawned(sync, direction, false);
                ApplyClassicPlayerRuntimeState(player, sync, direction);
                ApplyPendingClassicPlayerSync(sync.Id);
                if (!hadPlayer)
                {
                    Debug.Log("JxClassicClient spawned player from npc min id=" + sync.Id +
                              " mapX=" + sync.MapX +
                              " mapY=" + sync.MapY +
                              " name=" + sync.Name +
                              " hasPlayerObject=" + (player != null));
                }
                return;
            }

            ApplyClassicPositionSync(new ClassicNpcPositionSync
            {
                Id = sync.Id,
                MapX = sync.MapX,
                MapY = sync.MapY,
                OffsetX = sync.OffsetX,
                OffsetY = sync.OffsetY,
                IsPlayer = isPlayerNpc,
                Direction = sync.Direction,
                HasDirection = sync.HasDirection,
                Command = sync.Doing
            });

            NpcClick existingNpc = NpcMgrs.FindNpc(sync.Id);
            if (existingNpc != null)
            {
                ApplyClassicNpcRuntimeState(existingNpc, sync, direction);
            }

            return;
        }

        if (isPlayerNpc)
        {
            PlayerClick player = EnsureClassicPlayerSpawned(sync, direction, true);
            ApplyClassicPlayerRuntimeState(player, sync, direction);

            ApplyPendingClassicPlayerSync(sync.Id);
            Debug.Log("JxClassicClient spawned player id=" + sync.Id +
                      " setting=" + sync.NpcSettingIndex +
                      " mapX=" + sync.MapX +
                      " mapY=" + sync.MapY +
                      " name=" + sync.Name +
                      " hasPlayerObject=" + (player != null));
            return;
        }

        if (sync.Id != PlayerId)
        {
            CharMgrs.DelPlayer(sync.Id);
        }

        if (sync.NpcSettingIndex <= 0)
        {
            Debug.LogWarning("JxClassicClient skipped npc without setting index. id=" + sync.Id + " name=" + sync.Name);
            return;
        }

        NpcClick npc = NpcMgrs.SpwanNpc(sync.Id);
        if (npc == null)
        {
            return;
        }

        int hpMax = Math.Max(1, sync.CurrentLifeMax > 0 ? sync.CurrentLifeMax : sync.MaxLife);
        int hpCur = Math.Max(1, sync.CurrentLife > 0 ? sync.CurrentLife : hpMax);
        byte level = (byte)Math.Max(1, Math.Min(255, sync.Level));
        NPCSERIES series = (NPCSERIES)Math.Max(0, Math.Min((int)NPCSERIES.series_earth, (int)sync.Series));
        NPCCAMP camp = (NPCCAMP)Math.Max(0, Math.Min((int)NPCCAMP.camp_num - 1, (int)sync.CurrentCamp));
        NPCKIND kind = (NPCKIND)Math.Max(0, Math.Min((int)NPCKIND.kind_num - 1, (int)sync.Kind));

        int npcType = ResolveClassicNpcType(sync);

        npc.InitNpcDetail(
            kind,
            series,
            camp,
            hpMax,
            hpCur,
            level,
            npcType,
            sync.MapX,
            sync.MapY,
            string.IsNullOrEmpty(sync.Name) ? "npc_" + sync.Id : sync.Name,
            direction);

        ApplyClassicNpcRuntimeState(npc, sync, direction);
        JxClassicMovement.EnsureBaseSpeed(npc.GetController());
        Debug.Log("JxClassicClient spawned npc id=" + sync.Id +
                  " setting=" + sync.NpcSettingIndex +
                  " npcType=" + npcType +
                  " kind=" + sync.Kind +
                  " mapX=" + sync.MapX +
                  " mapY=" + sync.MapY +
                  " name=" + sync.Name);
    }

    private int ResolveClassicNpcType(ClassicNpcSync sync)
    {
        int[] candidates =
        {
            sync.NpcSettingIndex,
            sync.NpcSettingIndex - 2,
            sync.NpcSettingIndex - 1,
            sync.NpcSettingIndex + 1,
            sync.NpcSettingIndex + 2
        };

        foreach (int candidate in candidates)
        {
            if (candidate < 0)
            {
                continue;
            }

            int declareLine = candidate + 2;
            string resType = game.resource.settings.Npcs.GetNpcResType(declareLine);
            if (!string.IsNullOrEmpty(resType))
            {
                if (candidate != sync.NpcSettingIndex)
                {
                    Debug.LogWarning("JxClassicClient npc setting fallback. id=" + sync.Id +
                                     " rawSetting=" + sync.NpcSettingIndex +
                                     " npcType=" + candidate +
                                     " declareLine=" + declareLine +
                                     " resType=" + resType);
                }
                else
                {
                    Debug.Log("JxClassicClient npc resource. id=" + sync.Id +
                              " setting=" + sync.NpcSettingIndex +
                              " declareLine=" + declareLine +
                              " resType=" + resType);
                }

                return candidate;
            }
        }

        Debug.LogWarning("JxClassicClient npc resource missing. id=" + sync.Id +
                         " setting=" + sync.NpcSettingIndex +
                         " name=" + sync.Name);
        return Math.Max(0, sync.NpcSettingIndex);
    }

    private void ApplyClassicMainPlayerFromNpcSync(ClassicNpcSync sync, byte direction)
    {
        CharMgrs.RegisterMainPlayer(sync.Id, sync.Name);
        CharacterClick player = CharMgrs.FindPlayer(sync.Id);
        if (player == null || player.controller == null)
        {
            return;
        }

        if (!string.IsNullOrEmpty(sync.Name))
        {
            player.Name = sync.Name;
            player.controller.SetName(sync.Name);
        }

        player.controller.SetCharacterType(ResolveClassicPlayerSex(sync)
            ? game.resource.settings.NpcRes.SpecialType.man
            : game.resource.settings.NpcRes.SpecialType.lady);
        if (sync.HasDirection)
        {
            player.controller.SyncDirection(direction);
        }
        JxClassicMovement.EnsureBaseSpeed(player.controller);

        if (sync.MapX > 0 && sync.MapY > 0)
        {
            player.controller.SetMapPosition(sync.MapY / 2, sync.MapX);
        }

        ApplyPendingClassicPlayerSync(sync.Id);
        Debug.Log("JxClassicClient updated main player appearance from npc sync. id=" + sync.Id +
                  " setting=" + sync.NpcSettingIndex +
                  " name=" + sync.Name);
    }

    private bool ResolveClassicPlayerSex(ClassicNpcSync sync)
    {
        return sync.NpcSettingIndex != unchecked((ushort)-2);
    }

    private PlayerClick EnsureClassicPlayerSpawned(ClassicNpcSync sync, byte direction, bool refreshAppearance)
    {
        if (sync == null || CharMgrs == null || sync.Id == 0)
        {
            return null;
        }

        string playerName = string.IsNullOrEmpty(sync.Name) ? "player_" + sync.Id : sync.Name;
        int top = sync.MapY > 0 ? sync.MapY / 2 : MapY / 2;
        int left = sync.MapX > 0 ? sync.MapX : MapX;
        bool hasDirection = sync.HasDirection && direction <= 63;

        if (sync.Id == PlayerId)
        {
            CharMgrs.RegisterMainPlayer(sync.Id, playerName);
            CharacterClick mainPlayer = CharMgrs.FindPlayer(sync.Id);
            if (mainPlayer != null && mainPlayer.controller != null)
            {
                mainPlayer.Name = playerName;
                mainPlayer.controller.SetName(playerName);
                mainPlayer.controller.SetCharacterType(ResolveClassicPlayerSex(sync)
                    ? game.resource.settings.NpcRes.SpecialType.man
                    : game.resource.settings.NpcRes.SpecialType.lady);
                if (hasDirection)
                {
                    mainPlayer.controller.SyncDirection(direction);
                }
                JxClassicMovement.EnsureBaseSpeed(mainPlayer.controller);
            }

            return null;
        }

        CharacterClick existingPlayer = CharMgrs.FindPlayer(sync.Id);
        PlayerClick player = existingPlayer as PlayerClick;

        if (existingPlayer == null)
        {
            NpcMgrs?.DelNpc(sync.Id);
            player = CharMgrs.SpwanPlayer(
                sync.Id,
                playerName,
                ResolveClassicPlayerSex(sync),
                hasDirection ? direction : -1,
                left,
                top);
            existingPlayer = player;
        }

        if (existingPlayer != null && existingPlayer.controller != null)
        {
            existingPlayer.Name = playerName;
            existingPlayer.controller.SetName(playerName);
            if (refreshAppearance)
            {
                existingPlayer.controller.SetCharacterType(ResolveClassicPlayerSex(sync)
                    ? game.resource.settings.NpcRes.SpecialType.man
                    : game.resource.settings.NpcRes.SpecialType.lady);
            }

            if (hasDirection)
            {
                existingPlayer.controller.SyncDirection(direction);
            }
            JxClassicMovement.EnsureBaseSpeed(existingPlayer.controller);
        }

        return player;
    }

    private void ApplyPendingClassicPlayerSync(int playerId)
    {
        if (!pendingClassicPlayerSyncs.TryGetValue(playerId, out ClassicPlayerSync pending))
        {
            return;
        }

        pendingClassicPlayerSyncs.Remove(playerId);
        ApplyClassicPlayerSync(pending);
    }

    private void ApplyClassicPlayerSync(ClassicPlayerSync sync)
    {
        if (sync == null || sync.Id == 0)
        {
            return;
        }

        ApplyClassicMainPlayerSpeedState(sync);
        if (sync.Id == PlayerId)
        {
            CharMgrs.RegisterMainPlayer(sync.Id);
        }

        CharacterClick player = CharMgrs.FindPlayer(sync.Id);
        if (player == null || player.controller == null)
        {
            player = TryPromoteClassicNpcToPlayer(sync);
        }

        if (player == null || player.controller == null)
        {
            pendingClassicPlayerSyncs[sync.Id] = sync;
            Debug.Log("JxClassicClient deferred player sync. id=" + sync.Id +
                      " helm=" + sync.HelmType +
                      " armor=" + sync.ArmorType +
                      " weapon=" + sync.WeaponType +
                      " horse=" + sync.HorseType +
                      " walkSpeed=" + sync.WalkSpeed +
                      " runSpeed=" + sync.RunSpeed);
            return;
        }

        ApplyClassicPlayerEquipment(player.controller, sync);
        ApplyClassicMainPlayerHorseState(sync);
        ApplyClassicPlayerControllerSpeedState(player.controller, sync);
        JxClassicMovement.EnsureBaseSpeed(player.controller);

        ApplyClassicPlayerTong(player, sync);

        if (sync.Id == PlayerId && character != null)
        {
            character.FightMode = (sync.Flags & 0x02) != 0;
            character.Sect = (byte)Math.Max(0, Math.Min(255, sync.Faction));
        }

        Debug.Log("JxClassicClient applied player sync. id=" + sync.Id +
                  " helm=" + sync.HelmType +
                  " armor=" + sync.ArmorType +
                  " weapon=" + sync.WeaponType +
                  " horse=" + sync.HorseType +
                  " walkSpeed=" + sync.WalkSpeed +
                  " runSpeed=" + sync.RunSpeed +
                  " figure=" + sync.Figure +
                  " tong=" + sync.TongName +
                  " title=" + sync.TongTitle +
                  " flags=0x" + sync.Flags.ToString("X2"));
    }

    private bool IsClassicMainPlayerNpc(ClassicNpcSync sync)
    {
        if (sync == null || sync.Id <= 0)
        {
            return false;
        }

        return PlayerId > 0 && sync.Id == PlayerId;
    }

    private bool IsClassicPlayerNpcSync(ClassicNpcSync sync)
    {
        return sync != null &&
               (sync.Kind == (byte)NPCKIND.kind_player || pendingClassicPlayerSyncs.ContainsKey(sync.Id));
    }

    private static void ApplyClassicPlayerTong(CharacterClick player, ClassicPlayerSync sync)
    {
        if (player?.controller == null || sync == null)
        {
            return;
        }

        string tongName = sync.TongName ?? string.Empty;
        string tongTitle = sync.TongTitle ?? string.Empty;
        string tongText = (tongName + tongTitle).Replace(" ", string.Empty);
        string playerName = (player.Name ?? string.Empty).Replace(" ", string.Empty);

        if (sync.Figure < 0 || string.IsNullOrEmpty(tongName))
        {
            player.controller.SetTong(string.Empty, string.Empty);
            return;
        }

        if (!string.IsNullOrEmpty(playerName) &&
            string.Equals(tongText, playerName, StringComparison.OrdinalIgnoreCase))
        {
            player.controller.SetTong(string.Empty, string.Empty);
            return;
        }

        player.controller.SetTong(tongName, tongTitle);
    }

    private CharacterClick TryPromoteClassicNpcToPlayer(ClassicPlayerSync sync)
    {
        if (sync == null || NpcMgrs == null || CharMgrs == null)
        {
            return null;
        }

        NpcClick npc = NpcMgrs.FindNpc(sync.Id);
        if (npc == null || npc.GetController() == null)
        {
            return null;
        }

        game.resource.map.Position position = npc.GetController().GetMapPosition();
        string playerName = string.IsNullOrEmpty(npc.Name) ? "player_" + sync.Id : npc.Name;
        NpcMgrs.DelNpc(sync.Id);

        PlayerClick player = CharMgrs.SpwanPlayer(
            sync.Id,
            playerName,
            true,
            -1,
            position.left,
            position.top);

        Debug.Log("JxClassicClient promoted npc to player. id=" + sync.Id +
                  " name=" + playerName +
                  " left=" + position.left +
                  " top=" + position.top);

        return player;
    }

    private void ApplyClassicMainPlayerSpeedState(ClassicPlayerSync sync)
    {
        if (sync == null || sync.Id != PlayerId)
        {
            return;
        }

        if (sync.WalkSpeed > 0)
        {
            ClassicWalkSpeed = JxClassicMovement.NormalizeWalkSpeed(sync.WalkSpeed);
        }

        if (sync.RunSpeed > 0)
        {
            ClassicRunSpeed = JxClassicMovement.NormalizeRunSpeed(sync.RunSpeed);
        }
    }

    private static void ApplyClassicPlayerControllerSpeedState(
        game.resource.settings.npcres.Controller controller,
        ClassicPlayerSync sync)
    {
        if (controller == null || sync == null)
        {
            return;
        }

        JxClassicMovement.ApplyCurrentSpeed(controller, sync.WalkSpeed, sync.RunSpeed);
    }

    private void ApplyClassicPlayerEquipment(game.resource.settings.NpcRes.Special controller, ClassicPlayerSync sync)
    {
        if (sync.HelmType >= 0)
        {
            controller.SetHeadRes(sync.HelmType);
        }

        if (sync.ArmorType >= 0)
        {
            controller.SetBodyRes(sync.ArmorType);
        }

        if (sync.WeaponType >= 0)
        {
            controller.SetWeaponRes(sync.WeaponType);
        }

        if (sync.HorseType >= 0)
        {
            controller.SetHorseRes(sync.HorseType);
        }
        else
        {
            controller.SetHorseItemDefault();
        }
    }

    private void ApplyClassicMainPlayerHorseState(ClassicPlayerSync sync)
    {
        if (sync == null || sync.Id != PlayerId || PlayerMain.instance == null)
        {
            return;
        }

        PlayerMain.instance.SetHorseRidingLocal(sync.HorseType >= 0);
    }

    private void ApplyClassicNpcRuntimeState(NpcClick npc, ClassicNpcSync sync, byte direction)
    {
        if (npc == null || sync == null)
        {
            return;
        }

        if (sync.CurrentLifeMax > 0 || sync.MaxLife > 0)
        {
            npc.CurrentHPMax = Math.Max(1, sync.CurrentLifeMax > 0 ? sync.CurrentLifeMax : sync.MaxLife);
        }

        if (sync.CurrentLife > 0)
        {
            npc.CurrentHPCur = Math.Max(1, sync.CurrentLife);
        }

        ApplyClassicControllerRuntimeState(npc.GetController(), sync, direction);
    }

    private static void ApplyClassicPlayerVitals(CharacterClick player, ClassicNpcSync sync)
    {
        if (player == null || sync == null)
        {
            return;
        }

        if (sync.CurrentLifeMax > 0 || sync.MaxLife > 0)
        {
            player.HPMax = Math.Max(1, sync.CurrentLifeMax > 0 ? sync.CurrentLifeMax : sync.MaxLife);
        }

        if (sync.CurrentLife > 0)
        {
            player.HPCur = Math.Max(1, sync.CurrentLife);
        }
    }

    private void ApplyClassicPlayerRuntimeState(CharacterClick player, ClassicNpcSync sync, byte direction)
    {
        if (player == null || sync == null)
        {
            return;
        }

        ApplyClassicPlayerVitals(player, sync);

        if (player.controller == null)
        {
            return;
        }

        JxClassicMovement.EnsureBaseSpeed(player.controller);

        if (player.id != PlayerId && sync.Doing == (byte)NPCCMD.do_stand)
        {
            world?.StopNpcMove(player.controller);
            NpcAction.DoAction(player.controller, NPCCMD.do_stand);
        }

        if (sync.HasDirection)
        {
            player.controller.SyncDirection(direction);
        }
    }

    private void ApplyClassicControllerRuntimeState(
        game.resource.settings.npcres.Controller controller,
        ClassicNpcSync sync,
        byte direction,
        bool applyPosition = true)
    {
        if (controller == null)
        {
            return;
        }

        JxClassicMovement.EnsureBaseSpeed(controller);

        int top = sync.MapY / 2;
        int left = sync.MapX;
        int resolvedDirection = sync.HasDirection
            ? direction
            : -1;
        if (resolvedDirection < 0 && sync.MapX > 0 && sync.MapY > 0 && IsClassicMovingCommand(sync.Doing))
        {
            int vectorDirection = ResolveClassicDirection(controller.GetMapPosition(), top, left);
            if (vectorDirection >= 0)
            {
                resolvedDirection = vectorDirection;
            }
        }

        if (resolvedDirection >= 0)
        {
            controller.SyncDirection(resolvedDirection);
        }

        if (applyPosition && sync.MapX > 0 && sync.MapY > 0)
        {
            var targetPosition = new game.resource.map.Position(top, left);
            bool isRunning = sync.Doing == (byte)NPCCMD.do_run;
            int moveSpeed = isRunning
                ? JxClassicMovement.NormalizeRunSpeed(JxClassicMovement.GetCurrentRunSpeed(controller))
                : JxClassicMovement.NormalizeWalkSpeed(JxClassicMovement.GetCurrentWalkSpeed(controller));
            if (IsClassicMovingCommand(sync.Doing))
            {
                world.MoveNpcToClassicMps(
                    controller,
                    sync.MapX,
                    sync.MapY,
                    moveSpeed,
                    768,
                    resolvedDirection,
                    isRunning);
            }
            else
            {
                float duration = JxClassicMovement.GetDuration(
                    controller.GetMapPosition(),
                    targetPosition,
                    moveSpeed);
                world.MoveNpcToMps(
                    controller,
                    sync.MapX,
                    sync.MapY,
                    duration,
                    768,
                    false);
            }
        }

        if (TryGetClassicNpcCommand(sync.Doing, out NPCCMD command))
        {
            NpcAction.DoAction(controller, command);
        }
    }

    private static bool TryGetClassicNpcCommand(byte command, out NPCCMD npcCommand)
    {
        npcCommand = (NPCCMD)command;
        return command > (byte)NPCCMD.do_none && command <= (byte)NPCCMD.do_revive;
    }

    private static bool IsClassicMovingCommand(byte command)
    {
        return command == (byte)NPCCMD.do_run || command == (byte)NPCCMD.do_walk;
    }

    private void ApplyClassicActorCommandSync(ClassicNpcCommandSync sync)
    {
        if (sync == null || sync.Id == 0 || !TryGetClassicNpcCommand(sync.Command, out NPCCMD command))
        {
            return;
        }

        if (sync.Id == PlayerId)
        {
            game.resource.settings.npcres.Controller mainPlayer = world.GetMainPlayer();
            if (mainPlayer != null)
            {
                NpcAction.DoAction(mainPlayer, command);
            }

            if (command == NPCCMD.do_death && character != null)
            {
                character.CurLife = 0;
            }
            return;
        }

        CharacterClick player = CharMgrs.FindPlayer(sync.Id);
        if (player != null && player.controller != null)
        {
            NpcAction.DoAction(player.controller, command);
            if (command == NPCCMD.do_death)
            {
                player.HPCur = 0;
            }
            return;
        }

        NpcClick npc = NpcMgrs.FindNpc(sync.Id);
        if (npc != null && npc.GetController() != null)
        {
            NpcAction.DoAction(npc.GetController(), command);
            if (command == NPCCMD.do_death)
            {
                npc.CurrentHPCur = 0;
            }
        }
    }

    private void ApplyClassicSkillCastSync(ClassicSkillCastSync sync)
    {
        if (sync == null || sync.Id == 0 || sync.SkillId <= 0)
        {
            return;
        }

        game.resource.settings.npcres.Controller caster = FindClassicController(sync.Id);
        if (caster == null)
        {
            return;
        }

        int direction = ResolveClassicSkillDirection(caster, sync);
        if (direction >= 0)
        {
            caster.SyncDirection(direction);
        }

        ApplyClassicSkillAnimation(caster, sync.SkillId, sync.SkillLevel);
    }

    private game.resource.settings.npcres.Controller FindClassicController(int id)
    {
        if (id == PlayerId)
        {
            return world != null ? world.GetMainPlayer() : null;
        }

        CharacterClick player = CharMgrs != null ? CharMgrs.FindPlayer(id) : null;
        if (player != null && player.controller != null)
        {
            return player.controller;
        }

        NpcClick npc = NpcMgrs != null ? NpcMgrs.FindNpc(id) : null;
        return npc != null ? npc.GetController() : null;
    }

    private int ResolveClassicSkillDirection(
        game.resource.settings.npcres.Controller caster,
        ClassicSkillCastSync sync)
    {
        if (caster == null || sync == null)
        {
            return -1;
        }

        if (sync.MpsX < 0 && sync.MpsY > 0)
        {
            game.resource.settings.npcres.Controller target = FindClassicController(sync.MpsY);
            return target != null
                ? JxClassicMovement.GetDirection(caster.GetMapPosition(), target.GetMapPosition())
                : -1;
        }

        if (sync.MpsX > 0 || sync.MpsY > 0)
        {
            return JxClassicMovement.GetDirection(caster.GetMapPosition(), sync.MpsY / 2, sync.MpsX);
        }

        return -1;
    }

    private static void ApplyClassicSkillAnimation(
        game.resource.settings.npcres.Controller caster,
        int skillId,
        int skillLevel)
    {
        try
        {
            game.resource.settings.skill.SkillSetting skillSetting =
                game.resource.settings.skill.SkillSetting.Get(skillId, Math.Max(1, skillLevel));
            if (skillSetting != null &&
                skillSetting.m_nCharActionId == game.resource.settings.skill.Defination.CLIENTACTION.cdo_magic)
            {
                caster.SetAction(game.resource.settings.NpcRes.Action.magic);
                return;
            }
        }
        catch (Exception exception)
        {
            Debug.LogWarning("JxClassicClient skill animation fallback. skill=" + skillId +
                             " level=" + skillLevel +
                             " error=" + exception.GetBaseException().Message);
        }

        NpcAction.DoAction(caster, NPCCMD.do_attack);
    }

    private void ApplyClassicPositionSync(ClassicNpcPositionSync sync)
    {
        if (sync == null || sync.Id == 0 || sync.MapX <= 0 || sync.MapY <= 0)
        {
            return;
        }

        int top = sync.MapY / 2;
        int left = sync.MapX;

        if (sync.Id == PlayerId)
        {
            bool ignoreRunEcho = IsClassicMovingPositionSync(sync) &&
                                 !classicLocalMovementActive &&
                                 Time.time < classicRunEchoIgnoreUntil;

            if (classicLocalMovementActive)
            {
                MapX = sync.MapX;
                MapY = sync.MapY;
                return;
            }

            if (ignoreRunEcho)
            {
                NpcAction.DoAction(world.GetMainPlayer(), NPCCMD.do_stand);
                return;
            }

            JxClassicMovement.EnsureBaseSpeed(world.GetMainPlayer());
            ApplyClassicMoveState(world.GetMainPlayer(), top, left, sync);
            int moveDirection = ResolveClassicSyncDirection(world.GetMainPlayer(), top, left, sync);

            int moveSpeed = sync.IsRunning
                ? JxClassicMovement.NormalizeRunSpeed(Math.Max(ClassicRunSpeed, JxClassicMovement.GetCurrentRunSpeed(world.GetMainPlayer())))
                : JxClassicMovement.NormalizeWalkSpeed(Math.Max(ClassicWalkSpeed, JxClassicMovement.GetCurrentWalkSpeed(world.GetMainPlayer())));
            if (IsClassicMovingPositionSync(sync))
            {
                world.MoveMainPlayerToClassicMps(sync.MapX, sync.MapY, moveSpeed, 768, moveDirection, sync.IsRunning);
            }
            else
            {
                world.MoveMainPlayerToMps(
                    sync.MapX,
                    sync.MapY,
                    ClassicNpcNormalSyncDuration,
                    384,
                    false);
            }
            MapX = sync.MapX;
            MapY = sync.MapY;
            return;
        }

        CharacterClick player = CharMgrs.FindPlayer(sync.Id);
        if (player != null)
        {
            if (player.controller == null)
            {
                return;
            }

            JxClassicMovement.EnsureBaseSpeed(player.controller);

            if (sync.IsStanding)
            {
                ApplyClassicRemotePlayerStandCorrection(player.controller, sync);
                return;
            }

            ApplyClassicMoveState(player.controller, top, left, sync);
            int moveDirection = ResolveClassicPlayerFacingDirection(player.controller, top, left, sync);
            if (moveDirection >= 0)
            {
                player.controller.SyncDirection(moveDirection);
            }

            int moveSpeed = IsClassicRunPositionSync(sync)
                ? JxClassicMovement.NormalizeRunSpeed(JxClassicMovement.GetCurrentRunSpeed(player.controller))
                : JxClassicMovement.NormalizeWalkSpeed(JxClassicMovement.GetCurrentWalkSpeed(player.controller));
            if (IsClassicMovingPositionSync(sync))
            {
                world.MoveNpcToClassicMps(
                    player.controller,
                    sync.MapX,
                    sync.MapY,
                    moveSpeed,
                    768,
                    moveDirection,
                    IsClassicRunPositionSync(sync));
            }
            else
            {
                world.MoveNpcToMps(
                    player.controller,
                    sync.MapX,
                    sync.MapY,
                    ClassicNpcNormalSyncDuration,
                    768,
                    moveDirection >= 0);
            }

            return;
        }

        NpcClick npc = NpcMgrs.FindNpc(sync.Id);
        if (npc != null)
        {
            JxClassicMovement.EnsureBaseSpeed(npc.GetController());
            ApplyClassicMoveState(npc.GetController(), top, left, sync);
            int moveDirection = ResolveClassicSyncDirection(npc.GetController(), top, left, sync);
            int moveSpeed = IsClassicRunPositionSync(sync)
                ? JxClassicMovement.NormalizeRunSpeed(JxClassicMovement.GetCurrentRunSpeed(npc.GetController()))
                : JxClassicMovement.NormalizeWalkSpeed(JxClassicMovement.GetCurrentWalkSpeed(npc.GetController()));
            if (IsClassicMovingPositionSync(sync))
            {
                world.MoveNpcToClassicMps(
                    npc.GetController(),
                    sync.MapX,
                    sync.MapY,
                    moveSpeed,
                    768,
                    moveDirection,
                    IsClassicRunPositionSync(sync));
            }
            else
            {
                world.MoveNpcToMps(
                    npc.GetController(),
                    sync.MapX,
                    sync.MapY,
                    ClassicNpcNormalSyncDuration,
                    768,
                    false);
            }
        }
    }

    private static void ApplyClassicMoveState(game.resource.settings.npcres.Controller controller, int top, int left, ClassicNpcPositionSync sync)
    {
        if (controller == null)
        {
            return;
        }

        int direction = ResolveClassicSyncDirection(controller, top, left, sync);

        JxClassicMovement.EnsureBaseSpeed(controller);
        if (direction >= 0)
        {
            controller.SyncDirection(direction);
        }

        if (TryGetClassicNpcCommand(sync.Command, out NPCCMD command))
        {
            NpcAction.DoAction(controller, command);
        }
        else if (sync.IsStanding)
        {
            NpcAction.DoAction(controller, NPCCMD.do_stand);
        }
        else if (sync.HasMoveAction)
        {
            NpcAction.DoAction(controller, sync.IsRunning ? NPCCMD.do_run : NPCCMD.do_walk);
        }
    }

    private static bool IsClassicRunPositionSync(ClassicNpcPositionSync sync)
    {
        return sync != null && (sync.IsRunning || sync.Command == (byte)NPCCMD.do_run);
    }

    private static bool IsClassicMovingPositionSync(ClassicNpcPositionSync sync)
    {
        return sync != null &&
               (sync.HasMoveAction ||
                sync.Command == (byte)NPCCMD.do_run ||
                sync.Command == (byte)NPCCMD.do_walk);
    }

    private void ApplyClassicRemotePlayerStandCorrection(
        game.resource.settings.npcres.Controller controller,
        ClassicNpcPositionSync sync)
    {
        if (controller == null || sync == null || sync.MapX <= 0 || sync.MapY <= 0 || world == null)
        {
            return;
        }

        Vector2 currentMps = world.GetNpcMpsPosition(controller);
        Vector2 targetMps = new(sync.MapX, sync.MapY);
        if (Vector2.Distance(currentMps, targetMps) < 256f)
        {
            return;
        }

        world.MoveNpcToMps(
            controller,
            sync.MapX,
            sync.MapY,
            0f,
            int.MaxValue,
            false);
    }

    private static int ResolveClassicDirection(game.resource.map.Position currentPosition, int targetTop, int targetLeft)
    {
        return JxClassicMovement.GetDirection(currentPosition, targetTop, targetLeft);
    }

    private static int ResolveClassicSyncDirection(
        game.resource.settings.npcres.Controller controller,
        int targetTop,
        int targetLeft,
        ClassicNpcPositionSync sync)
    {
        if (controller == null || sync == null)
        {
            return -1;
        }

        if (sync.HasDirection)
        {
            return sync.Direction;
        }

        return IsClassicMovingPositionSync(sync)
            ? ResolveClassicDirection(controller.GetMapPosition(), targetTop, targetLeft)
            : -1;
    }

    private int ResolveClassicPlayerFacingDirection(
        game.resource.settings.npcres.Controller controller,
        int targetTop,
        int targetLeft,
        ClassicNpcPositionSync sync)
    {
        int direction = ResolveClassicSyncDirection(controller, targetTop, targetLeft, sync);
        if (direction >= 0)
        {
            return direction;
        }

        if (controller == null || sync == null || sync.MapX <= 0 || sync.MapY <= 0)
        {
            return -1;
        }

        if (world != null)
        {
            Vector2 currentMps = world.GetNpcMpsPosition(controller);
            Vector2 targetMps = new(sync.MapX, sync.MapY);
            direction = JxClassicMovement.GetDirection(currentMps, targetMps);
            if (direction >= 0)
            {
                return direction;
            }
        }

        return ResolveClassicDirection(controller.GetMapPosition(), targetTop, targetLeft);
    }

    private void OnDestroy()
    {
        isDestroy = true;
        if (client != null)
        {
            client.Disconnect();
            client = null;
            isConnected = false;
        }

        if (ClassicClient != null)
        {
            ClassicClient.Dispose();
            ClassicClient = null;
        }

        if (reConnect != null)
        {
            Destroy(reConnect);
        }

    }

    /// <summary>
    /// Photon Listener
    /// </summary>
    private ICharClientListener iCharClientListener;
    private INpcClientListener iNpcClientListener;
    private IMainPlayerClientListener iMainPlayerClientListener;

    public void SetCharClientListener(ICharClientListener listener) => this.iCharClientListener = listener;

    public ICharClientListener CharClientListener() => iCharClientListener;

    public void SetNpcClientListener(INpcClientListener listener) => this.iNpcClientListener = listener;

    public INpcClientListener NpcClientListener() => iNpcClientListener;

    public void SetMainPlayerClientListener(IMainPlayerClientListener listener) => this.iMainPlayerClientListener = listener;

    public IMainPlayerClientListener MainPlayerClientListener() => iMainPlayerClientListener;


    /// <summary>
    /// Photon Client
    /// </summary>
    /// <returns></returns>
    public PhotonPeer Client() => this.client;
    public bool IsConnected() => this.isConnected;
    public bool IsDisConnected()
    {
        return client == null || !isConnected;
    }

    public bool TrySendOperation(OperationCode operationCode, Dictionary<byte, object> parameters)
    {
        if (!usePhotonServer && TrySendClassicOperation(operationCode, parameters))
        {
            return true;
        }

        if (client == null || !isConnected)
        {
            return false;
        }

        return client.SendOperation(
            (byte)operationCode,
            parameters ?? new Dictionary<byte, object>(),
            ExitGames.Client.Photon.SendOptions.SendReliable);
    }

    private bool TrySendClassicOperation(OperationCode operationCode, Dictionary<byte, object> parameters)
    {
        if (ClassicClient == null || !ClassicClient.IsConnected)
        {
            return false;
        }

        switch (operationCode)
        {
            case OperationCode.DoMove:
                if (parameters == null ||
                    !parameters.TryGetValue((byte)ParamterCode.MapX, out object mapXValue) ||
                    !parameters.TryGetValue((byte)ParamterCode.MapY, out object mapYValue))
                {
                    return false;
                }

                int mapX = Convert.ToInt32(mapXValue);
                int mapY = Convert.ToInt32(mapYValue);
                int mapId = 0;

                if (parameters.TryGetValue((byte)ParamterCode.MapId, out object mapIdValue))
                {
                    mapId = Convert.ToInt32(mapIdValue);
                }

                FireAndForgetClassicSend(ClassicClient.SendRunAsync(mapX, mapY, mapId));
                return true;

            case OperationCode.StopMove:
                if (parameters != null &&
                    parameters.TryGetValue((byte)ParamterCode.MapX, out object stopMapXValue) &&
                    parameters.TryGetValue((byte)ParamterCode.MapY, out object stopMapYValue))
                {
                    int stopMapX = Convert.ToInt32(stopMapXValue);
                    int stopMapY = Convert.ToInt32(stopMapYValue);
                    FireAndForgetClassicSend(ClassicClient.SendWalkAsync(stopMapX, stopMapY));
                }

                return true;

            case OperationCode.NpcSkill:
                if (parameters == null ||
                    !parameters.TryGetValue((byte)ParamterCode.SkillId, out object skillIdValue))
                {
                    return false;
                }

                int skillId = Convert.ToInt32(skillIdValue);
                int skillLevel = 1;
                if (parameters.TryGetValue((byte)ParamterCode.SkillLevel, out object skillLevelValue))
                {
                    skillLevel = Math.Max(1, Convert.ToInt32(skillLevelValue));
                }

                if (Time.time < nextClassicSkillSendTime)
                {
                    return true;
                }

                PrepareClassicRideStateForSkill(skillId, skillLevel);

                nextClassicSkillSendTime = Time.time + GetClassicSkillSendInterval(skillId, skillLevel);

                int skillMpsX = -1;
                int skillMpsY = PlayerId;
                int skillTargetId = -1;
                bool hasMapTarget = false;

                if (parameters.TryGetValue((byte)ParamterCode.MapX, out object skillMapXValue) &&
                    parameters.TryGetValue((byte)ParamterCode.MapY, out object skillMapYValue))
                {
                    skillMpsX = Convert.ToInt32(skillMapXValue);
                    skillMpsY = Convert.ToInt32(skillMapYValue);
                    if (skillMpsX < 0)
                    {
                        skillTargetId = skillMpsY;
                    }
                    else
                    {
                        hasMapTarget = true;
                    }
                }
                else if (parameters.TryGetValue((byte)ParamterCode.Id, out object targetIdValue))
                {
                    skillTargetId = Convert.ToInt32(targetIdValue);
                    skillMpsX = -1;
                    skillMpsY = skillTargetId;
                }

                FireAndForgetClassicSend(ClassicClient.SendNpcSkillAsync(skillId, skillMpsX, skillMpsY));
                PreviewClassicNpcSkill(skillId, skillLevel, skillTargetId, hasMapTarget, skillMpsX, skillMpsY);
                Debug.Log("JxClassicClient >> npc skill id=" + skillId +
                          " x=" + skillMpsX +
                          " y=" + skillMpsY);
                return true;

            case OperationCode.AutoEquip:
                if (parameters == null ||
                    !parameters.TryGetValue((byte)ParamterCode.ItemId, out object itemIdValue))
                {
                    return false;
                }

                uint itemId = Convert.ToUInt32(itemIdValue);
                if (!playerItems.TryGetValue(itemId, out ItemData item))
                {
                    Debug.LogWarning("JxClassicClient auto equip skipped. item not found id=" + itemId);
                    return false;
                }

                bool isEquip = true;
                if (parameters.TryGetValue((byte)ParamterCode.IsEquip, out object isEquipValue))
                {
                    isEquip = Convert.ToBoolean(isEquipValue);
                }

                int kind = isEquip ? 0 : 1;
                FireAndForgetClassicSend(ClassicClient.SendAutoEquipAsync(
                    itemId,
                    kind,
                    item.Local,
                    item.X,
                    item.Y));
                Debug.Log("JxClassicClient >> auto equip item id=" + itemId +
                          " kind=" + kind +
                          " local=" + item.Local +
                          " x=" + item.X +
                          " y=" + item.Y);
                return true;

            default:
                return false;
        }
    }

    public void RequestClassicRideToggle()
    {
        if (ClassicClient == null || !ClassicClient.IsConnected)
        {
            return;
        }

        FireAndForgetClassicSend(ClassicClient.SendRideAsync());
        Debug.Log("JxClassicClient >> npc ride toggle");
    }

    private void PrepareClassicRideStateForSkill(int skillId, int skillLevel)
    {
        if (PlayerMain.instance == null || skillId <= 0)
        {
            return;
        }

        try
        {
            game.resource.settings.skill.SkillSetting skillSetting =
                game.resource.settings.skill.SkillSetting.Get(skillId, Math.Max(1, skillLevel));
            if (skillSetting == null)
            {
                return;
            }

            if (skillSetting.m_nHorseLimited == 1 && PlayerMain.instance.IsUseHorse)
            {
                PlayerMain.instance.SetHorseRidingLocal(false);
                RequestClassicRideToggle();
            }
            else if (skillSetting.m_nHorseLimited == 2 && !PlayerMain.instance.IsUseHorse)
            {
                PlayerMain.instance.SetHorseRidingLocal(true);
                RequestClassicRideToggle();
            }
        }
        catch (Exception ex)
        {
            Debug.LogWarning("JxClassicClient skill horse check skipped. skill=" + skillId +
                             " level=" + skillLevel +
                             " error=" + ex.GetBaseException().Message);
        }
    }

    private static float GetClassicSkillSendInterval(int skillId, int skillLevel)
    {
        try
        {
            game.resource.settings.skill.SkillSetting skillSetting =
                game.resource.settings.skill.SkillSetting.Get(skillId, Math.Max(1, skillLevel));
            if (skillSetting != null && skillSetting.m_nMinTimePerCast > 0)
            {
                return Mathf.Clamp(
                    skillSetting.m_nMinTimePerCast / JxClassicMovement.CoreTickRate,
                    ClassicSkillMinSendInterval,
                    ClassicSkillMaxSendInterval);
            }
        }
        catch (Exception ex)
        {
            Debug.LogWarning("JxClassicClient skill cooldown fallback. skill=" + skillId +
                             " level=" + skillLevel +
                             " error=" + ex.GetBaseException().Message);
        }

        return ClassicSkillMinSendInterval;
    }

    private void PreviewClassicNpcSkill(
        int skillId,
        int skillLevel,
        int targetId,
        bool hasMapTarget,
        int mpsX,
        int mpsY)
    {
        if (world == null || world.GetMainPlayer() == null || skillId <= 0)
        {
            return;
        }

        game.resource.settings.npcres.Controller launcher = world.GetMainPlayer();
        NpcAction.DoAction(launcher, NPCCMD.do_attack);
    }

    private static void FireAndForgetClassicSend(Task task)
    {
        task.ContinueWith(
            failed => Debug.LogWarning("JxClassicClient send failed: " + failed.Exception?.GetBaseException().Message),
            TaskContinuationOptions.OnlyOnFaulted);
    }

    /// <summary>
    /// Photon Data
    /// </summary>

    [HideInInspector]
    public int PlayerId;
    [HideInInspector]
    public ushort MapId;
    [HideInInspector]
    public int MapX, MapY;
    public int ClassicWalkSpeed { get; private set; } = 5;
    public int ClassicRunSpeed { get; private set; } = 10;

    public CharacterData character = null;
    public CharacterData GetChracter() => character;

    /// <summary>
    /// Chat
    /// </summary>
    private Dictionary<PlayerChat, MessageData> Messages = new();

    public void SetMessage(MessageData data, PlayerChat playerChat)
    {
        Messages[playerChat] = data;
        MainCanvas.instance.GetMiniChat().GetComponent<Chat>().AddNewMessage(data, playerChat);
        PopUpCanvas.instance.GetOpenChat().GetComponent<OpenChat>().AddNewMessage(data, playerChat);
    }

    public Dictionary<PlayerChat, MessageData> GetMessage() => Messages;

    /// <summary>
    /// PlayerItems
    /// </summary>
    private Dictionary<uint, ItemData> playerItems = new Dictionary<uint, ItemData>();
    private Dictionary<uint, ClassicItemSync> playerItemDetails = new Dictionary<uint, ClassicItemSync>();

    public void SetPlayerItem(ItemData data)
    {
        bool containsKey = playerItems.ContainsKey(data.id);
        if (containsKey)
        {
            playerItems[data.id] = data;
            iMainPlayerClientListener?.SyncUpdateItem(data);
        }
        else
        {
            playerItems.Add(data.id, data);
            iMainPlayerClientListener?.SyncNewItem(data);
        }
    }

    public void RemovePlayerItem(uint id)
    {
        if (playerItems.Remove(id))
        {
            iMainPlayerClientListener?.SyncRemoveItem();
        }
    }

    public Dictionary<uint, ItemData> GetPlayerItems() => playerItems;

    public ClassicItemSync GetPlayerItemDetail(uint id)
    {
        return playerItemDetails.TryGetValue(id, out ClassicItemSync detail)
            ? detail
            : null;
    }

    /// <summary>
    /// PlayerSkills
    /// </summary>
    private Dictionary<ushort, PlayerSkill> playerSkills = new Dictionary<ushort, PlayerSkill>();

    public void SetPlayerSkill(PlayerSkill data)
    {
        if (data == null || data.id == 0)
        {
            return;
        }

        bool containsKey = playerSkills.ContainsKey(data.id);
        if (containsKey)
        {
            playerSkills[data.id] = data;
            iMainPlayerClientListener?.SyncUpdateSkill(data);
        }
        else
        {
            playerSkills.Add(data.id, data);
            iMainPlayerClientListener?.SyncAddSkill(data);
        }
    }

    public Dictionary<ushort, PlayerSkill> GetPlayerSkill() => playerSkills;

    /// <summary>
    /// Player Task
    /// </summary>
    public List<PlayerTask> playerTasks;
    public void SetPlayerTask(PlayerTask data)
    {
        playerTasks.Add(data);
        iMainPlayerClientListener?.SyncTask();
    }
    public List<PlayerTask> GetPlayerTasks() => playerTasks;

    internal void DoTask()
    {
        TrySendOperation(OperationCode.DoTask, new Dictionary<byte, object>()
        {
            [(byte)ParamterCode.TaskId] = 1,
            [(byte)ParamterCode.TaskValue] = 2,
        });
    }

    internal void SetMessage(PlayerChat type, MessageData messageData)
    {
        throw new NotImplementedException();
    }
}
