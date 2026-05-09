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
    private bool classicLocalMovementActive;
    private float classicRunEchoIgnoreUntil;

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
            ApplyClassicPositionSync(new ClassicNpcPositionSync
            {
                Id = sync.Id,
                MapX = sync.MapX,
                MapY = sync.MapY,
                OffsetX = sync.OffsetX,
                OffsetY = sync.OffsetY,
                IsPlayer = true
            });

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
                EnsureClassicPlayerSpawned(sync, direction, false);
                ApplyClassicPositionSync(new ClassicNpcPositionSync
                {
                    Id = sync.Id,
                    MapX = sync.MapX,
                    MapY = sync.MapY,
                    OffsetX = sync.OffsetX,
                    OffsetY = sync.OffsetY,
                    IsPlayer = true
                });
                ApplyPendingClassicPlayerSync(sync.Id);
                return;
            }

            ApplyClassicPositionSync(new ClassicNpcPositionSync
            {
                Id = sync.Id,
                MapX = sync.MapX,
                MapY = sync.MapY,
                OffsetX = sync.OffsetX,
                OffsetY = sync.OffsetY,
                IsPlayer = isPlayerNpc
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
        player.controller.SyncDirection(direction);
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
                mainPlayer.controller.SyncDirection(direction);
                JxClassicMovement.EnsureBaseSpeed(mainPlayer.controller);
            }

            return null;
        }

        CharacterClick existingPlayer = CharMgrs.FindPlayer(sync.Id);
        PlayerClick player = existingPlayer as PlayerClick;

        if (existingPlayer == null || refreshAppearance)
        {
            player = CharMgrs.SpwanPlayer(
                sync.Id,
                playerName,
                ResolveClassicPlayerSex(sync),
                direction,
                left,
                top);
            existingPlayer = player;
        }

        if (existingPlayer != null && existingPlayer.controller != null)
        {
            existingPlayer.Name = playerName;
            existingPlayer.controller.SetName(playerName);
            existingPlayer.controller.SyncDirection(direction);
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

        if (sync.Id == PlayerId)
        {
            return true;
        }

        if (!IsClassicPlayerNpcSync(sync))
        {
            return false;
        }

        if (PlayerId > 0 || character == null || string.IsNullOrEmpty(character.Name) || string.IsNullOrEmpty(sync.Name))
        {
            return false;
        }

        if (!string.Equals(sync.Name, character.Name, StringComparison.OrdinalIgnoreCase))
        {
            return false;
        }

        PlayerId = sync.Id;
        CharMgrs?.RegisterMainPlayer(sync.Id, sync.Name);
        return true;
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
            0,
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
            ClassicWalkSpeed = sync.WalkSpeed;
        }

        if (sync.RunSpeed > 0)
        {
            ClassicRunSpeed = sync.RunSpeed;
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

        game.resource.settings.npcres.Controller controller = npc.GetController();
        if (controller == null)
        {
            return;
        }

        JxClassicMovement.EnsureBaseSpeed(controller);
        controller.SyncDirection(direction);

        if (sync.MapX > 0 && sync.MapY > 0)
        {
            float duration = JxClassicMovement.GetDuration(
                controller.GetMapPosition(),
                new game.resource.map.Position(sync.MapY / 2, sync.MapX),
                sync.Doing == (byte)NPCCMD.do_run
                    ? JxClassicMovement.GetCurrentRunSpeed(controller)
                    : JxClassicMovement.GetCurrentWalkSpeed(controller));
            world.MoveNpcTo(
                controller,
                new game.resource.map.Position(sync.MapY / 2, sync.MapX),
                duration,
                768);
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
            bool ignoreRunEcho = sync.HasMoveAction &&
                                 !classicLocalMovementActive &&
                                 Time.time < classicRunEchoIgnoreUntil;

            if (classicLocalMovementActive && sync.HasMoveAction)
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

            var targetPosition = new game.resource.map.Position(top, left);
            int moveSpeed = sync.IsRunning
                ? Math.Max(ClassicRunSpeed, JxClassicMovement.GetCurrentRunSpeed(world.GetMainPlayer()))
                : Math.Max(ClassicWalkSpeed, JxClassicMovement.GetCurrentWalkSpeed(world.GetMainPlayer()));
            float moveDuration = JxClassicMovement.GetDuration(
                world.GetMainPlayer()?.GetMapPosition(),
                targetPosition,
                moveSpeed);
            world.MoveMainPlayerTo(
                targetPosition,
                sync.HasMoveAction ? moveDuration : ClassicNpcNormalSyncDuration,
                sync.HasMoveAction ? 768 : 384);
            MapX = sync.MapX;
            MapY = sync.MapY;
            return;
        }

        CharacterClick player = CharMgrs.FindPlayer(sync.Id);
        if (player != null)
        {
            JxClassicMovement.EnsureBaseSpeed(player.controller);
            ApplyClassicMoveState(player.controller, top, left, sync);
            var targetPosition = new game.resource.map.Position(top, left);
            int moveSpeed = sync.IsRunning
                ? JxClassicMovement.GetCurrentRunSpeed(player.controller)
                : JxClassicMovement.GetCurrentWalkSpeed(player.controller);
            world.MoveNpcTo(
                player.controller,
                targetPosition,
                sync.HasMoveAction ? JxClassicMovement.GetDuration(player.controller.GetMapPosition(), targetPosition, moveSpeed) : ClassicNpcNormalSyncDuration,
                768);
            return;
        }

        NpcClick npc = NpcMgrs.FindNpc(sync.Id);
        if (npc != null)
        {
            JxClassicMovement.EnsureBaseSpeed(npc.GetController());
            ApplyClassicMoveState(npc.GetController(), top, left, sync);
            var targetPosition = new game.resource.map.Position(top, left);
            int moveSpeed = sync.IsRunning
                ? JxClassicMovement.GetCurrentRunSpeed(npc.GetController())
                : JxClassicMovement.GetCurrentWalkSpeed(npc.GetController());
            world.MoveNpcTo(
                npc.GetController(),
                targetPosition,
                sync.HasMoveAction ? JxClassicMovement.GetDuration(npc.GetController().GetMapPosition(), targetPosition, moveSpeed) : ClassicNpcNormalSyncDuration,
                768);
        }
    }

    private static void ApplyClassicMoveState(game.resource.settings.npcres.Controller controller, int top, int left, ClassicNpcPositionSync sync)
    {
        if (controller == null)
        {
            return;
        }

        int direction = ResolveClassicDirection(controller.GetMapPosition(), top, left);
        JxClassicMovement.EnsureBaseSpeed(controller);
        if (direction >= 0)
        {
            controller.SyncDirection(direction);
        }

        if (sync.IsStanding)
        {
            NpcAction.DoAction(controller, NPCCMD.do_stand);
        }
        else if (sync.HasMoveAction)
        {
            NpcAction.DoAction(controller, sync.IsRunning ? NPCCMD.do_run : NPCCMD.do_walk);
        }
    }

    private static int ResolveClassicDirection(game.resource.map.Position currentPosition, int targetTop, int targetLeft)
    {
        int targetMapY = targetTop * 2;
        int currentMapY = currentPosition.top * 2;

        int direction = game.resource.settings.skill.Static.g_GetDirIndex(
            currentPosition.left,
            currentMapY,
            targetLeft,
            targetMapY);

        if (direction < 0 || direction > 63)
        {
            return -1;
        }

        return direction;
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

            default:
                return false;
        }
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

    public Dictionary<uint, ItemData> GetPlayerItems() => playerItems;

    /// <summary>
    /// PlayerSkills
    /// </summary>
    private Dictionary<ushort, PlayerSkill> playerSkills = new Dictionary<ushort, PlayerSkill>();

    public void SetPlayerSkill(PlayerSkill data)
    {
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
