using game.scene.world.userInterface;
using Photon.ShareLibrary.Entities;
using UnityEngine;

public class PopUpCanvas : MonoBehaviour
{
    public static PopUpCanvas instance;

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
        PanelSafeArea = this.gameObject.transform.Find("PanelSafeArea").gameObject;

        NpcDialog = this.gameObject.transform.Find("NpcDialog").gameObject;
        EnhanceDialog = this.gameObject.transform.Find("Enhance").gameObject;
        PointSetting = this.gameObject.transform.Find("PointSetting").gameObject;
        SkillPannel = this.gameObject.transform.Find("Skill").gameObject;
        TrainPannel = this.gameObject.transform.Find("Train").gameObject;
        GuildPannel = this.gameObject.transform.Find("Guild").gameObject;
        NewsPannel = this.gameObject.transform.Find("News").gameObject;
        PanelChatHome = this.PanelSafeArea.transform.Find("PanelChatHome").gameObject;
        OpenChat = this.PanelChatHome.transform.Find("OpenChat").gameObject;
        SytemNotifyPannel = this.gameObject.transform.Find("SytemNotifyPannel").gameObject;
        PlayerPopUp = this.gameObject.transform.Find("PlayerPopUp").gameObject;
        FirstRecharge = this.gameObject.transform.Find("FirstRecharge").gameObject;
        LoginReward = this.gameObject.transform.Find("LoginReward").gameObject;
        PhucLoi = this.gameObject.transform.Find("PhucLoi").gameObject;
        Shop = this.gameObject.transform.Find("Shop").gameObject;
        Storage = this.gameObject.transform.Find("Storage").gameObject;
        Trade = this.gameObject.transform.Find("Trade").gameObject;

        Storage.GetComponent<Storage>().InitStorage();
        Trade.GetComponent<Trade>().InitStorage();
    }

    //public void MoveViewport(ViewportItem viewportItem)
    //{
    //    viewportItem.current.transform.SetParent(gameObject.transform);
    //}

    public void SetUPSkillLocation(PlayerSkill skill, int location)
    {
        if (SkillPannel != null && skill != null)
        {
            var playerSkill = SkillPannel.GetComponent<PlayerSkills>();
            playerSkill.AddSkillToActive(skill, location);
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
        if (SkillPannel != null)
        {
            SkillPannel.GetComponent<PlayerSkills>()?.RefreshSkills();
            SkillPannel.SetActive(true);
        }
    }

    public void OpenSkillDetail(PlayerSkill skill)
    {
        if (SkillPannel != null && skill != null)
        {
            var playerSkill = SkillPannel.GetComponent<PlayerSkills>();
            playerSkill.OpenSkillDetail(skill);
        }

    }

    public void RemoveSkill(int location)
    {
        if (SkillPannel != null)
        {
            var playerSkill = SkillPannel.GetComponent<PlayerSkills>();
            playerSkill.RemoveSkill(location);
        }

    }

    public void OpenNpcDialog(Sprite sprite, int npcId, string npcName, string name, string data)
    {
        if (NpcDialog != null)
        {
            NpcDialog.SetActive(true);
            NpcDialog.GetComponent<NpcDialog>().SetNpcWithContent(sprite, npcId, npcName, name, data);
        }
    }

    public void OpenStorage()
    {
        if (Storage != null)
        {
            Storage.SetActive(true);
            Storage.GetComponent<Storage>().InitStorage();
            Storage.GetComponent<Storage>().SetUpPlayerItem();
        }
    }

    public void RefreshStorageIfOpen()
    {
        if (Storage != null && Storage.activeInHierarchy)
        {
            Storage.GetComponent<Storage>().SetUpPlayerItem();
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
