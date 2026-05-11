using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using game.basemono;
using game.config;
using game.network;
using game.network.jx;
using game.network.listener;
using Photon.ShareLibrary.Constant;
using Photon.ShareLibrary.Entities;
using Photon.ShareLibrary.Handlers;
using UnityEngine;
using UnityEngine.Analytics;
using UnityEngine.Localization.Settings;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using static game.resource.mapping.settings.NpcRes.Kind;

public class LoginManger : BaseMonoBehaviour
{
    [SerializeField]
    private string classicServerHost = "157.66.80.25";
    [SerializeField]
    private int classicServerPort = 56722;
    [SerializeField]
    private uint classicServerRegionIndex = 1;
    [SerializeField]
    private uint classicEnterMapIndex = 53;
    [SerializeField]
    private bool classicForceEnterMapOnLogin = false;
    [SerializeField]
    private int classicFallbackMapX = 54784;
    [SerializeField]
    private int classicFallbackMapY = 109056;
    [SerializeField]
    private bool classicUseLocalWorldPreview = false;
    [SerializeField]
    private int classicPreviewMapId = 53;
    private const string ClassicPreviewNoMapPipelineKey = "CLASSIC_PREVIEW_NO_MAP_PIPELINE";
    private const string ClassicPreviewMiniMapWorldKey = "CLASSIC_PREVIEW_MINIMAP_WORLD";

    private JxClassicClient classicClient;
    private int selectedCharacterIndex;
    private bool isClassicLoginRunning;

    [SerializeField]
    GameObject panelUI;
    [SerializeField]
    GameObject pannelButton;
    [SerializeField]
    GameObject pannelLogin;
    [SerializeField]
    GameObject pannelRegister;

    [SerializeField]
    GameObject pannelPlay;
    [SerializeField]
    GameObject pannelCharacter;
    [SerializeField]
    GameObject pannelCreate;

    [SerializeField]
    private Toggle togglePolicy;

    class Faction
    {
        public byte Id;
        public string Name;
        public NPCSERIES Series;
    };
        private List<Faction> factions = new List<Faction>
        {
            new Faction
            {
                Id = 0,
                Name = "shaolin",
                Series = NPCSERIES.series_metal,
            },
            new Faction
            {
                Id = 1,
                Name = "tianwang",
                Series = NPCSERIES.series_metal,
            },
            new Faction
            {
                Id = 2,
                Name = "tangmen",
                Series = NPCSERIES.series_wood,
            },
            new Faction
            {
                Id = 3,
                Name = "wudu",
                Series = NPCSERIES.series_wood,
            },
            new Faction
            {
                Id = 4,
                Name = "emei",
                Series = NPCSERIES.series_water,
            },
            new Faction
            {
                Id = 5,
                Name = "cuiyan",
                Series = NPCSERIES.series_water,
            },
            new Faction
            {
                Id = 6,
                Name = "gaibang",
                Series = NPCSERIES.series_fire,
            },
            new Faction
            {
                Id = 7,
                Name = "tianren",
                Series = NPCSERIES.series_fire,
            },
            new Faction
            {
                Id = 8,
                Name = "wudang",
                Series = NPCSERIES.series_earth,
            },
            new Faction
            {
                Id = 9,
                Name = "kunlun",
                Series = NPCSERIES.series_earth,
            },
        };

    public static LoginManger instance;
    void Start()
    {
        instance = this;

        if (string.IsNullOrEmpty(PlayerPrefs.GetString(PlayerPrefsKey.USER_NAME)) ||
            string.IsNullOrEmpty(PlayerPrefs.GetString(PlayerPrefsKey.USER_PASSWORD)))
        {
            pannelPlay.SetActive(false);
            panelUI.SetActive(true);
        }
        else
        {
            PlayNowScreen();
        }
    }

    [SerializeField]
    InputField userNameInput;
    [SerializeField]
    InputField passWordInput;

    public void LoginBtn()
    {
        var userName = userNameInput.text;
        var passWord = passWordInput.text;

        if (userName == "")
        {
            ShowMessageBox(LocalizationSettings.StringDatabase.GetLocalizedString(ConfigGame.tableLanguage, "username_is_empty"), "error");
            return;
        }
        if (passWord == "")
        {
            ShowMessageBox(LocalizationSettings.StringDatabase.GetLocalizedString(ConfigGame.tableLanguage, "password_is_empty"), "error");
            return;
        }
        PlayerPrefs.SetString(PlayerPrefsKey.USER_NAME, userName);
        PlayerPrefs.SetString(PlayerPrefsKey.USER_PASSWORD, passWord);

        PlayNowScreen();
    }
    async void LoginImp()
    {
        if (isClassicLoginRunning)
        {
            return;
        }

        try
        {
            isClassicLoginRunning = true;
            string userName = PlayerPrefs.GetString(PlayerPrefsKey.USER_NAME);
            string userpassword = PlayerPrefs.GetString(PlayerPrefsKey.USER_PASSWORD);

            ShowLoading();

            classicClient?.Dispose();
            classicClient = new JxClassicClient();

            uint loginEnterMapIndex = classicForceEnterMapOnLogin ? classicEnterMapIndex : 0;
            Debug.Log("LoginManger classic login request. region=" + classicServerRegionIndex +
                      " selectedEnterMap=" + classicEnterMapIndex +
                      " sentEnterMap=" + loginEnterMapIndex);

            LoginResult result = await classicClient.LoginAsync(
                classicServerHost,
                classicServerPort,
                userName,
                userpassword,
                classicServerRegionIndex,
                loginEnterMapIndex);

            HideLoading();
            isClassicLoginRunning = false;

            if (!result.Success)
            {
                Fails(result.Message);
                return;
            }

            characterReply = result.Characters ?? new List<CharacterLogin>();
            Debug.Log("LoginManger classic login success. roleCount=" + characterReply.Count +
                      " responseRegion=" + result.ServerRegionIndex +
                      " responseEnterMap=" + result.EnterMapIndex);
            selectedCharacterIndex = 0;
            LoginResponse(1);
        }
        catch (Exception exception)
        {
            HideLoading();
            isClassicLoginRunning = false;
            Debug.LogError(exception);
            pannelPlay.SetActive(false);
            panelUI.SetActive(true);
            ShowMessageBox("Không kết nối được server JX classic: " + exception.Message, "error");
        }
    }

    [SerializeField]
    InputField usernameInput;
    [SerializeField]
    InputField passWordInput1;
    [SerializeField]
    InputField passWordInput2;

    public void RegisterImp()
    {
        var userName = usernameInput.text;
        var passWord1 = passWordInput1.text;
        var passWord2 = passWordInput2.text;

        if (userName == "")
        {
            ShowMessageBox(LocalizationSettings.StringDatabase.GetLocalizedString(ConfigGame.tableLanguage, "username_is_empty"), "error");
            return;
        }
        if (passWord1 == "")
        {
            ShowMessageBox(LocalizationSettings.StringDatabase.GetLocalizedString(ConfigGame.tableLanguage, "password_is_empty"), "error");
            return;
        }
        if (passWord1.Length < 8)
        {
            ShowMessageBox("Mật khẩu yêu cầu 8 kí tự trở lên", "error");
            return;
        }


        if (passWord2 == "")
        {
            ShowMessageBox(LocalizationSettings.StringDatabase.GetLocalizedString(ConfigGame.tableLanguage, "password_is_empty"), "error");
            return;
        }
        if (passWord2.Length < 8)
        {
            ShowMessageBox("Mật khẩu yêu cầu 8 kí tự trở lên", "error");
            return;
        }

        ShowMessageBox("Đăng ký tài khoản chưa port sang JX classic. Tạo tài khoản bằng server/tool hiện tại trước.", "error");
    }

    public void RegisterResponse()
    {
        //throw new NotImplementedException();
    }

    public void PlayNowScreen()
    {
        pannelPlay.SetActive(true);
        panelUI.SetActive(false);

        ShowWaitingForClassicLogin();

        if (!string.IsNullOrEmpty(PlayerPrefs.GetString(PlayerPrefsKey.USER_NAME)) &&
            !string.IsNullOrEmpty(PlayerPrefs.GetString(PlayerPrefsKey.USER_PASSWORD)))
        {
            LoginImp();
        }
    }

    public void SelectClassicServer()
    {
        LoginImp();
    }

    public void SelectClassicServer(string host)
    {
        if (!string.IsNullOrEmpty(host))
        {
            classicServerHost = host;
        }

        SelectClassicServer();
    }

    public void SelectClassicServerByRegion(int regionIndex)
    {
        classicServerRegionIndex = (uint)Math.Max(1, regionIndex);
        SelectClassicServer();
    }

    public void SelectClassicServer(string host, int port, int regionIndex, int enterMapIndex)
    {
        if (!string.IsNullOrEmpty(host))
        {
            classicServerHost = host;
        }

        if (port > 0)
        {
            classicServerPort = port;
        }

        classicServerRegionIndex = (uint)Math.Max(1, regionIndex);
        classicEnterMapIndex = (uint)Math.Max(0, enterMapIndex);
        SelectClassicServer();
    }
    [SerializeField]
    GameObject btnPlay;
    [SerializeField]
    GameObject btnGender;

    public void LogOut()
    {
        PlayerPrefs.DeleteAll();
        PlayerPrefs.DeleteKey(ClassicPreviewNoMapPipelineKey);
        PlayerPrefs.DeleteKey(ClassicPreviewMiniMapWorldKey);
        panelUI.SetActive(true);
        pannelButton.SetActive(true);
        pannelPlay.SetActive(false);
        pannelCharacter.SetActive(false);
        pannelCreate.SetActive(false);
    }

    public void Fails(string message)
    {
        HideLoading();
        PlayerPrefs.DeleteKey(ClassicPreviewNoMapPipelineKey);
        PlayerPrefs.DeleteKey(ClassicPreviewMiniMapWorldKey);
        pannelPlay.SetActive(false);
        panelUI.SetActive(true);
        ShowWaitingForClassicLogin();
        ShowMessageBox(message, "error");
    }

    [HideInInspector]
    public List<CharacterLogin> characterReply;

    [SerializeField]
    CharacterInf[] info;
    public void LoginResponse(uint userId)
    {
        characterReply ??= new List<CharacterLogin>();
        Debug.Log("LoginManger LoginResponse roleCount=" + characterReply.Count);
        PlayerPrefs.SetInt(PlayerPrefsKey.USER_ID, (int)userId);

        panelUI.SetActive(false);
        pannelPlay.SetActive(true);
        pannelCharacter.SetActive(characterReply.Count > 0);
        pannelCreate.SetActive(false);
        SetRightInfoPanelActive(true);

        if (characterReply.Count == 0)
        {
            pannelCreate.SetActive(true);
            btnPlay.SetActive(false);
            btnGender.SetActive(true);
            SetSelectSeriesVisible(true);
            ShowClassByAttribute();
            return;
        }

        int displayCount = Math.Min(characterReply.Count, info.Length);

        for (var i = 0; i < info.Length; i++)
        {
            info[i].Clear();
        }

        for (var i = 0; i < displayCount; i++)
        {
            info[i].CharacterName = characterReply[i].Name;
            info[i].CharacterLevel = characterReply[i].Level.ToString();
        }

        btnPlay.SetActive(true);
        btnGender.SetActive(false);
        selectedCharacterIndex = 0;
        OnSelectRole(0);
    }

    [SerializeField]
    SelectSeries man;
    [SerializeField]
    SelectSeries girl;

    public void OnSelectRole(int idx)
    {
        characterReply ??= new List<CharacterLogin>();

        if (idx < characterReply.Count)
        {
            selectedCharacterIndex = idx;
            pannelCharacter.SetActive(true);
            pannelCreate.SetActive(false);
            SetRightInfoPanelActive(true);
            btnPlay.SetActive(true);
            btnGender.SetActive(false);

            byte series = characterReply[idx].Series;
            if (series > (byte)NPCSERIES.series_earth)
            {
                series = (byte)NPCSERIES.series_metal;
            }

            Debug.Log("LoginManger OnSelectRole idx=" + idx +
                      " name=" + characterReply[idx].Name +
                      " sex=" + characterReply[idx].Sex +
                      " series=" + series +
                      " level=" + characterReply[idx].Level);

            SelectSeries temp;
            if (characterReply[idx].Sex)
            {
                man.gameObject.SetActive(false);
                temp = girl;
            }
            else
            {
                girl.gameObject.SetActive(false);
                temp = man;
            }
            temp.gameObject.SetActive(true);
            temp.ChangeAttributeType((NPCSERIES)series);
        }
        else
        {
            pannelCharacter.SetActive(false);
            pannelCreate.SetActive(true);
            SetRightInfoPanelActive(true);
            btnPlay.SetActive(false);
            btnGender.SetActive(true);
            SetSelectSeriesVisible(true);

            ShowClassByAttribute();
        }
    }
    [SerializeField]
    private GameObject btnMetal;
    [SerializeField]
    private GameObject btnWood;
    [SerializeField]
    private GameObject btnWater;
    [SerializeField]
    private GameObject btnFire;
    [SerializeField]
    private GameObject btnEarth;

    [SerializeField]
    GameObject buttonPrefab;
    [SerializeField]
    GameObject pannelListClass;

    [SerializeField]
    Image imageLG;
    [SerializeField]
    Image imageBG;

    [SerializeField]
    Text TextInfor;

    private NPCSERIES Attribute = NPCSERIES.series_metal;

    public void ChangeAttributeType(string attributeTypeName)
    {
        Attribute = ConfigGame.parserCategoryToAttribute(attributeTypeName);

        UpdateUICategory(attibuteType: Attribute);

        man.ChangeAttributeType(Attribute);

        girl.ChangeAttributeType(Attribute);

        ShowClassByAttribute();

        TextInfor.text = LocalizationSettings.StringDatabase.GetLocalizedString("LanguageTable", attributeTypeName);
    }
    private void ShowClassByAttribute()
    {
        List<Faction> listFactionByAttribute = new();
        if (Attribute == NPCSERIES.series_metal)
        {
            listFactionByAttribute = factions.Where(p => p.Name == "tianwang" || p.Name == "shaolin").ToList();
        }
        else
        {
            listFactionByAttribute = factions.Where(p => p.Series == Attribute).ToList();
        }

        foreach (Transform child in pannelListClass.transform)
        {
            Destroy(child.gameObject);
        }
        for (int i = 0; i < listFactionByAttribute.Count; i++)
        {
            ClassUI(listFactionByAttribute[i]);
        }

        // Show defauft
        Faction factionDefauft = listFactionByAttribute.FirstOrDefault();

        ShowCharacterClass(factionDefauft.Name, factionDefauft.Id);

        UpdateUIButtonsGender();
    }
    private void ClassUI(Faction faction)
    {
        string factionClass = faction.Name;
        int factionId = faction.Id;

        string attribute = ConfigGame.parserAttributeTypeName(Attribute);

        GameObject pannelClass = (GameObject)Instantiate(buttonPrefab, new Vector3(0, 0, 0), Quaternion.Euler(0, 0, 0), pannelListClass.transform);
        pannelClass.name = factionClass;
        pannelClass.GetComponentInChildren<Button>().onClick.AddListener(() => ShowCharacterClass(factionClass, factionId));
        pannelClass.GetComponentInChildren<Button>().image.sprite = Resources.Load<Sprite>("characters/" + attribute + "/" + "BT_" + factionClass);
        pannelClass.GetComponentInChildren<Button>().transition = Selectable.Transition.None;
        
        var anim = pannelClass.GetComponentInChildren<Animator>();
        anim.runtimeAnimatorController = Resources.Load<RuntimeAnimatorController>("characters/" + attribute + "/" + "anim_" + attribute);
        anim.Rebind();
        
        pannelClass.GetComponentInChildren<Text>().text = faction.Series.ToString();
    }
    private void ShowCharacterClass(string factionClass, int factionID)
    {
        ChangeBackgroundClass(factionClass);
        man.ReShow();
        girl.ReShow();
        GenderSelect(man.isActive ? "man" : "girl");
        UpdateUIClass(factionClass);
    }
    private void ChangeBackgroundClass(string nameClass)
    {
        string attribute = ConfigGame.parserAttributeTypeName(Attribute);

        Sprite spriteLG = Resources.Load<Sprite>("characters/" + attribute + "/" + "LG_" + nameClass);
        imageLG.sprite = spriteLG;
        Sprite spriteBG = Resources.Load<Sprite>("characters/" + attribute + "/" + "BG_" + nameClass);
        imageBG.sprite = spriteBG;
    }
    [SerializeField]
    GameObject btnMan;
    [SerializeField]
    GameObject btnGirl;
    public void GenderSelect(string gender)
    {
        man.ChangeActive(gender == "man" ? true : false);
        btnMan.GetComponent<Image>().sprite = Resources.Load<Sprite>(gender == "man" ? ResourcesManager.genderManButtonSelected : ResourcesManager.genderManButtonDisabled);

        girl.ChangeActive(gender == "man" ? false : true);
        btnGirl.GetComponent<Image>().sprite = Resources.Load<Sprite>(gender == "girl" ? ResourcesManager.genderGirlButtonSelected : ResourcesManager.genderGirlButtonDisabled);
    }
    private void UpdateUIButtonsGender()
    {
        btnMan.GetComponent<Button>().interactable = true;
        btnGirl.GetComponent<Button>().interactable = true;

        if (Attribute == NPCSERIES.series_metal)
            btnGirl.GetComponent<Button>().interactable = false;
        else
        if (Attribute == NPCSERIES.series_water)
            btnMan.GetComponent<Button>().interactable = false;
    }
    private void UpdateUIClass(string nameClass)
    {
        foreach (Transform child in pannelListClass.transform)
        {
            if (child.gameObject.name == nameClass)
                child.GetComponent<ClassUI>().ShowAnim();
            else
                child.GetComponent<ClassUI>().HideAnim();
        }
    }
    private void UpdateUICategory(NPCSERIES attibuteType)
    {
        btnMetal.SetActive(false);
        btnWood.SetActive(false);
        btnWater.SetActive(false);
        btnFire.SetActive(false);
        btnEarth.SetActive(false);

        switch (attibuteType)
        {
            case NPCSERIES.series_metal:
                btnMetal.SetActive(true);
                break;
            case NPCSERIES.series_wood:
                btnWood.SetActive(true);
                break;
            case NPCSERIES.series_water:
                btnWater.SetActive(true);
                break;
            case NPCSERIES.series_fire:
                btnFire.SetActive(true);
                break;
            case NPCSERIES.series_earth:
                btnEarth.SetActive(true);
                break;
        }
    }

    public void ResetInput()
    {

    }
    [SerializeField]
    InputField inputName;
    public void CreateCharacter()
    {
        var charaterName = inputName.text.Trim();
        if (string.IsNullOrEmpty(charaterName))
        {
            ShowMessageBox(LocalizationSettings.StringDatabase.GetLocalizedString(ConfigGame.tableLanguage, "name_is_empty"), "error");
            return;
        }
        ShowMessageBox("Tạo nhân vật chưa port sang JX classic. Trước mắt hãy dùng nhân vật đã có để vào game.", "error");
    }
    public void CreateCharcterSuccess(uint cid)
    {
        PlayerPrefs.SetInt(PlayerPrefsKey.CHARACTER_ID, (int)cid);
    }
    public async void PlayGame()
    {
        if (togglePolicy.isOn)
        {
            try
            {
                if (characterReply == null || characterReply.Count == 0)
                {
                    ShowMessageBox("Chưa có nhân vật để vào game.", "error");
                    return;
                }

                if (classicClient == null || !classicClient.IsConnected)
                {
                    LoginImp();
                    return;
                }

                selectedCharacterIndex = Math.Max(0, Math.Min(selectedCharacterIndex, characterReply.Count - 1));
                CharacterLogin loginCharacter = characterReply[selectedCharacterIndex];
                PlayerPrefs.SetInt(PlayerPrefsKey.CHARACTER_ID, (int)loginCharacter.Id);

                if (classicUseLocalWorldPreview)
                {
                    LoadClassicLocalWorldPreview(loginCharacter);
                    return;
                }

                ShowLoading();
                GameLoginResult result = await classicClient.SelectCharacterAsync(
                    loginCharacter,
                    PlayerPrefs.GetString(PlayerPrefsKey.USER_NAME));
                HideLoading();

                if (!result.Success)
                {
                    ShowMessageBox(result.Message, "error");
                    return;
                }

                ApplyClassicWorldState(result, loginCharacter);
                PhotonManager.Instance.AttachClassicClient(classicClient);
                classicClient = null;

                SceneManager.LoadScene(ConfigGame.worldScreen);
            }
            catch (Exception exception)
            {
                HideLoading();
                Debug.LogError(exception);
                ShowMessageBox("Không vào được game server classic: " + exception.Message, "error");
            }
        }
        else
        {
            ShowMessageBox(LocalizationSettings.StringDatabase.GetLocalizedString(ConfigGame.tableLanguage, "please_agree_policy"), "error");
        }
    }

    private void OnDestroy()
    {
        classicClient?.Dispose();
        classicClient = null;
    }

    private void LoadClassicLocalWorldPreview(CharacterLogin loginCharacter)
    {
        int mapId = ResolveClassicPreviewMapId();
        int mapX = classicFallbackMapX;
        int mapY = classicFallbackMapY;
        GetClassicFallbackMapPosition(mapId, out mapX, out mapY);

        CharacterData character = new CharacterData
        {
            Name = loginCharacter.Name,
            Fiveprop = loginCharacter.Series <= (byte)NPCSERIES.series_earth ? loginCharacter.Series : (byte)NPCSERIES.series_metal,
            Sex = !loginCharacter.Sex,
            FightLevel = loginCharacter.Level > 0 ? loginCharacter.Level : (byte)1,
            Sect = 0,
            Camp = 0,
            FightMode = false,
            MapId = (ushort)mapId,
            MapX = mapX,
            MapY = mapY,
            MaxLife = 1,
            CurLife = 1,
            MaxInner = 1,
            CurInner = 1,
            MaxStamina = 1,
            CurStamina = 1,
        };

        PhotonManager.Instance.PlayerId = (int)loginCharacter.Id;
        PhotonManager.Instance.MapId = (ushort)mapId;
        PhotonManager.Instance.MapX = mapX;
        PhotonManager.Instance.MapY = mapY;
        PhotonManager.Instance.character = character;
        PlayerPrefs.DeleteKey(ClassicPreviewNoMapPipelineKey);
        PlayerPrefs.DeleteKey(ClassicPreviewMiniMapWorldKey);

        Debug.Log("LoginManger classic local preview. region=" + classicServerRegionIndex +
                  " mapId=" + mapId +
                  " mapX=" + mapX +
                  " mapY=" + mapY +
                  " role=" + loginCharacter.Name +
                  " noMapPipeline=0" +
                  " miniMapWorldPreview=0");

        SceneManager.LoadScene(ConfigGame.worldScreen);
    }

    private void ApplyClassicWorldState(GameLoginResult result, CharacterLogin loginCharacter)
    {
        int mapId = result.MapId;
        if (mapId <= 0)
        {
            mapId = ResolveClassicPreviewMapId();
            Debug.LogWarning("LoginManger classic world missing server map id; using fallback mapId=" + mapId);
        }

        mapId = Math.Max(1, Math.Min(ushort.MaxValue, mapId));

        int mapX = result.MapX;
        int mapY = result.MapY;
        if (mapX <= 0 || mapY <= 0)
        {
            GetClassicFallbackMapPosition(mapId, out mapX, out mapY);
        }

        CharacterData character = result.Character ?? new CharacterData();
        character.Name = string.IsNullOrEmpty(character.Name) ? loginCharacter.Name : character.Name;
        character.Fiveprop = character.Fiveprop <= (byte)NPCSERIES.series_earth ? character.Fiveprop : loginCharacter.Series;
        character.Sex = !loginCharacter.Sex;
        character.FightLevel = character.FightLevel > 0 ? character.FightLevel : loginCharacter.Level;
        character.MapId = (ushort)mapId;
        character.MapX = mapX;
        character.MapY = mapY;
        character.MaxLife = Math.Max(1, character.MaxLife);
        character.CurLife = Math.Max(1, character.CurLife);
        character.MaxInner = Math.Max(1, character.MaxInner);
        character.CurInner = Math.Max(1, character.CurInner);
        character.MaxStamina = Math.Max(1, character.MaxStamina);
        character.CurStamina = Math.Max(1, character.CurStamina);

        PhotonManager.Instance.PlayerId = result.PlayerId > 0 ? result.PlayerId : (int)loginCharacter.Id;
        PhotonManager.Instance.MapId = (ushort)mapId;
        PhotonManager.Instance.MapX = mapX;
        PhotonManager.Instance.MapY = mapY;
        PhotonManager.Instance.character = character;
        PlayerPrefs.DeleteKey(ClassicPreviewNoMapPipelineKey);
        PlayerPrefs.DeleteKey(ClassicPreviewMiniMapWorldKey);

        Debug.Log("LoginManger classic world fallback/apply. playerId=" + PhotonManager.Instance.PlayerId +
                  " mapId=" + PhotonManager.Instance.MapId +
                  " mapX=" + PhotonManager.Instance.MapX +
                  " mapY=" + PhotonManager.Instance.MapY +
                  " character=" + character.Name);
    }

    private int ResolveClassicPreviewMapId()
    {
        int[] candidates = new[]
        {
            (int)classicEnterMapIndex,
            53,
            classicPreviewMapId
        };

        foreach (int candidate in candidates.Distinct())
        {
            game.resource.settings.MapList.MapInfo mapInfo = TryLoadMapInfo(candidate);
            if (mapInfo.id > 0)
            {
                Debug.Log("LoginManger classic preview map resolved. candidate=" + candidate + " name=" + mapInfo.name);
                return candidate;
            }
        }

        int requestedMapId = (int)classicEnterMapIndex > 0 ? (int)classicEnterMapIndex : classicPreviewMapId;
        Debug.LogWarning("LoginManger classic preview map unresolved; keeping requested raw id " + requestedMapId);
        return Math.Max(1, requestedMapId);
    }

    private void GetClassicFallbackMapPosition(int mapId, out int mapX, out int mapY)
    {
        mapX = classicFallbackMapX;
        mapY = classicFallbackMapY;

        try
        {
            game.resource.settings.MapList.MapInfo mapInfo = TryLoadMapInfo(mapId);
            if (mapInfo.id == 0)
            {
                return;
            }

            game.resource.map.Location location = new game.resource.map.Location(mapInfo);
            game.resource.map.Position middle = location.Middle();
            if (middle.left > 0 && middle.top > 0)
            {
                mapX = middle.left;
                mapY = middle.top * 2;
            }
        }
        catch (Exception exception)
        {
            Debug.LogWarning("LoginManger fallback map middle failed. mapId=" + mapId + " error=" + exception.Message);
        }
    }

    private game.resource.settings.MapList.MapInfo TryLoadMapInfo(int mapId)
    {
        if (mapId <= 0)
        {
            return default;
        }

        try
        {
            return game.resource.settings.MapList.LoadMapInfo(mapId);
        }
        catch (Exception exception)
        {
            Debug.LogWarning("LoginManger TryLoadMapInfo failed. mapId=" + mapId + " error=" + exception.Message);
            return default;
        }
    }

    private void ShowWaitingForClassicLogin()
    {
        pannelCharacter.SetActive(false);
        pannelCreate.SetActive(false);
        SetRightInfoPanelActive(false);
        SetSelectSeriesVisible(false);
    }

    private void SetRightInfoPanelActive(bool active)
    {
        Transform panelInfo = btnPlay != null ? btnPlay.transform.parent : null;
        if (panelInfo != null)
        {
            panelInfo.gameObject.SetActive(active);
        }
    }

    private void SetSelectSeriesVisible(bool visible)
    {
        if (man != null)
        {
            man.gameObject.SetActive(visible);
        }

        if (girl != null)
        {
            girl.gameObject.SetActive(visible);
        }
    }
}
