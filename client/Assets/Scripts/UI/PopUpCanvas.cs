using game.scene.world.userInterface;
using Photon.ShareLibrary.Entities;
using UnityEngine;

public class PopUpCanvas : MonoBehaviour
{
    public static PopUpCanvas instance;
    private const string SkillPrefabPath = "WorldGameUI/Prefabs/Skill/Skill";

    private GameObject NpcDialog;

    private GameObject EnhanceDialog;

    private GameObject PointSetting;
    private GameObject SkillPannel;
    private GameObject TrainPannel;
    private GameObject GuildPannel;
    private GameObject NewsPannel;
    private GameObject PanelSafeArea;
    private GameObject PanelChatHome;
    private GameObject OpenChat;
    private GameObject SytemNotifyPannel;
    private GameObject PlayerPopUp;

    private GameObject FirstRecharge;
    private GameObject LoginReward;
    private GameObject PhucLoi;
    private GameObject Shop;
    private GameObject Storage;
    private GameObject Trade;


    private void Awake()
    {
        instance = this;
    }
    void Start()
    {
        PanelSafeArea = FindDirectChild("PanelSafeArea");

        NpcDialog = FindDirectChild("NpcDialog");
        EnhanceDialog = FindDirectChild("Enhance");
        PointSetting = FindDirectChild("PointSetting");
        EnsureSkillPanel();
        TrainPannel = FindDirectChild("Train");
        GuildPannel = FindDirectChild("Guild");
        NewsPannel = FindDirectChild("News");
        PanelChatHome = PanelSafeArea != null ? FindDirectChild(PanelSafeArea.transform, "PanelChatHome") : null;
        OpenChat = PanelChatHome != null ? FindDirectChild(PanelChatHome.transform, "OpenChat") : null;
        SytemNotifyPannel = FindDirectChild("SytemNotifyPannel");
        PlayerPopUp = FindDirectChild("PlayerPopUp");
        FirstRecharge = FindDirectChild("FirstRecharge");
        LoginReward = FindDirectChild("LoginReward");
        PhucLoi = FindDirectChild("PhucLoi");
        Shop = FindDirectChild("Shop");
        Storage = FindDirectChild("Storage");
        Trade = FindDirectChild("Trade");

        Storage?.GetComponent<Storage>()?.InitStorage();
        Trade?.GetComponent<Trade>()?.InitStorage();
    }

    //public void MoveViewport(ViewportItem viewportItem)
    //{
    //    viewportItem.current.transform.SetParent(gameObject.transform);
    //}

    public void SetUPSkillLocation(PlayerSkill skill, int location)
    {
        if (EnsureSkillPanel() != null && skill != null)
        {
            var playerSkill = SkillPannel.GetComponent<PlayerSkills>();
            playerSkill?.AddSkillToActive(skill, location);
        }
    }

    public void OpenFirstRecharge()
    {
        if (FirstRecharge != null)
        {
            FirstRecharge.SetActive(true);
        }
    }

    public void OpenLoginReward()
    {
        if (LoginReward != null)
        {
            LoginReward.SetActive(true);
        }
    }

    public void OpenPhucLoi()
    {
        if (PhucLoi != null)
        {
            PhucLoi.SetActive(true);
        }
    }

    public GameObject GetOpenChat() => this.OpenChat;

    public void OpenShop()
    {
        if (Shop != null)
        {
            Shop.SetActive(true);
        }
    }

    public void OpenPlayerPopUp()
    {
        PlayerPopUp.SetActive(true);
    }

    public void ShowMessage(string message)
    {
        SytemNotifyPannel.GetComponent<SytemNotify>().SetMessage(message);
    }

    public void OpenChatPannel()
    {
        PanelChatHome.SetActive(true);
    }

    public void OpenSkillPannel()
    {
        PlayerSkills playerSkills = EnsureSkillPanel();
        if (SkillPannel != null)
        {
            gameObject.SetActive(true);
            SkillPannel.SetActive(true);
            SkillPannel.transform.SetAsLastSibling();
            playerSkills?.RefreshSkills();
            Debug.Log("PlayerSkills panel opened. active=" + SkillPannel.activeInHierarchy + " hasPlayerSkills=" + (playerSkills != null));
        }
        else
        {
            Debug.LogWarning("PlayerSkills panel missing: cannot find or load " + SkillPrefabPath);
        }
    }

    public void RefreshSkillIfOpen()
    {
        if (EnsureSkillPanel() != null && SkillPannel.activeInHierarchy)
        {
            SkillPannel.GetComponent<PlayerSkills>()?.RefreshSkills();
        }
    }

    public void OpenSkillDetail(PlayerSkill skill)
    {
        if (EnsureSkillPanel() != null && skill != null)
        {
            var playerSkill = SkillPannel.GetComponent<PlayerSkills>();
            playerSkill?.OpenSkillDetail(skill);
        }

    }

    public void RemoveSkill(int location)
    {
        if (EnsureSkillPanel() != null)
        {
            var playerSkill = SkillPannel.GetComponent<PlayerSkills>();
            playerSkill?.RemoveSkill(location);
        }

    }

    private GameObject FindDirectChild(string childName)
    {
        return FindDirectChild(transform, childName);
    }

    private static GameObject FindDirectChild(Transform parent, string childName)
    {
        Transform child = parent != null ? parent.Find(childName) : null;
        return child != null ? child.gameObject : null;
    }

    private PlayerSkills EnsureSkillPanel()
    {
        if (SkillPannel != null)
        {
            PlayerSkills existing = SkillPannel.GetComponent<PlayerSkills>();
            if (existing != null)
            {
                return existing;
            }
        }

        PlayerSkills found = GetComponentInChildren<PlayerSkills>(true);
        if (found != null)
        {
            SkillPannel = found.gameObject;
            return found;
        }

        GameObject directSkill = FindDirectChild("Skill");
        if (directSkill != null)
        {
            PlayerSkills direct = directSkill.GetComponent<PlayerSkills>();
            if (direct != null)
            {
                SkillPannel = directSkill;
                return direct;
            }

            Debug.LogWarning("PopUpCanvas child 'Skill' is missing PlayerSkills. Loading " + SkillPrefabPath);
        }

        GameObject prefab = Resources.Load<GameObject>(SkillPrefabPath);
        if (prefab == null)
        {
            return null;
        }

        SkillPannel = Instantiate(prefab, transform);
        SkillPannel.name = "Skill";
        SkillPannel.SetActive(false);
        return SkillPannel.GetComponent<PlayerSkills>();
    }

    public void OpenNpcDialog(Sprite sprite, int npcId, string npcName, string name, string data)
    {
        if (NpcDialog != null)
        {
            NpcDialog.SetActive(true);
            NpcDialog.transform.SetAsLastSibling();
            NpcDialog.GetComponent<NpcDialog>().SetNpcWithContent(sprite, npcId, npcName, name, data);
        }
    }

    public void OpenNpcDialog(game.network.jx.ClassicScriptDialog dialog)
    {
        if (NpcDialog != null)
        {
            NpcDialog.SetActive(true);
            NpcDialog.transform.SetAsLastSibling();
            NpcDialog.GetComponent<NpcDialog>().SetClassicDialog(dialog);
        }
    }

    public void OpenStorage()
    {
        if (Storage != null)
        {
            Storage.SetActive(true);
            Storage.transform.SetAsLastSibling();

            Storage storage = Storage.GetComponent<Storage>();
            storage?.InitStorage();
            storage?.SetUpPlayerItem();
        }
    }

    public void RefreshStorageIfOpen()
    {
        if (Storage != null && Storage.activeInHierarchy)
        {
            Storage.GetComponent<Storage>()?.SetUpPlayerItem();
        }
    }

    public void OpenTrade()
    {
/*
        if (Trade != null)
        {
            Trade.SetActive(true);
            Trade.GetComponent<Trade>().SetUpPlayerItem();
            PlayerMain.instance.viewportItem.current.transform.SetParent(gameObject.transform);
        }
*/
    }

    public void OpenEnhanceDialog()
    {
        if (EnhanceDialog != null)
        {
            EnhanceDialog.SetActive(true);
        }
    }

    public void OpenPointSetting()
    {
        if (PointSetting != null)
        {
            PointSetting.SetActive(true);
        }
    }

    public void OpenTrainPannel()
    {
        if (TrainPannel != null)
        {
            TrainPannel.SetActive(true);
        }
    }

    public void OpenGuildPannel()
    {
        if (GuildPannel != null)
        {
            GuildPannel.SetActive(true);
        }
    }

    public void OpenNewsPannel()
    {
        if (NewsPannel != null)
        {
            NewsPannel.SetActive(true);
        }
    }
}
