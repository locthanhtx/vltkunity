using System.Collections.Generic;
using game.scene;
using UnityEngine;
using game.resource.settings.skill;
using game.network;
using UnityEngine.UI;
using Photon.ShareLibrary.Handlers;
using game.config;
using Photon.ShareLibrary.Entities;

public class SkillAction : MonoBehaviour
{
    public GameObject mainGameObject;
    private World wordlGame;
    private CharManager charManager;
    private NpcManager npcManager;

    [SerializeField]
    public List<Button> SkillActives;
    [SerializeField]
    public Button ButtonTargetNPC;
    [SerializeField]
    private GameObject SkillPannel1;
    [SerializeField]
    private GameObject SkillPannel2;
    [SerializeField]
    private Button ButtonSwitch;

    private Dictionary<ushort, PlayerSkill> playerSkills;
    private bool isSkillPannel2 = false;
    private string ImagePath = "WorldGameUI/Buttons/btn_fight";

    void Start()
    {
        if (mainGameObject == null)
        {
            World world = UnityEngine.Object.FindFirstObjectByType<World>();
            if (world != null)
            {
                mainGameObject = world.gameObject;
            }
        }

        if (mainGameObject != null)
        {
            wordlGame = mainGameObject.GetComponent<World>();
            charManager = mainGameObject.GetComponent<CharManager>();
            npcManager = mainGameObject.GetComponent<NpcManager>();
        }

        if (ButtonTargetNPC != null)
        {
            ButtonTargetNPC.onClick.AddListener(() => FindNpcAround());
        }

        if (ButtonSwitch != null)
        {
            ButtonSwitch.onClick.AddListener(() => Swtich());
        }
    }

    public void UpdateSkill()
    {
        playerSkills = PlayerMain.instance != null
            ? PlayerMain.instance.playerSkills()
            : new Dictionary<ushort, PlayerSkill>();
        if (playerSkills == null)
        {
            playerSkills = new Dictionary<ushort, PlayerSkill>();
        }

        for (int i = 0; i < SkillActives.Count; i++)
        {
            string locationSkill = PlayerPrefsKey.USER_SKILL_LOCATION + i;
            int targetSkillId = PlayerPrefs.GetInt(locationSkill, -1);
            Button skillUI = SkillActives[i];

            if (targetSkillId > -1 && playerSkills.TryGetValue((ushort)targetSkillId, out PlayerSkill skillData))
            {
                skillUI.image.sprite = SkillIconLoader.LoadIcon(skillData.id);
            }
            else
            {
                Sprite loadedImage = Resources.Load<Sprite>(ImagePath);
                skillUI.image.sprite = loadedImage;
            }
        }
    }

    public void CastSkill(int skillLocationCast)
    {
        if (playerSkills == null)
        {
            UpdateSkill();
        }

        string locationSkill = PlayerPrefsKey.USER_SKILL_LOCATION + skillLocationCast;
        int targetSkillId = PlayerPrefs.GetInt(locationSkill, -1);

        if (targetSkillId > -1 && playerSkills != null && playerSkills.TryGetValue((ushort)targetSkillId, out PlayerSkill skill))
        {
            if (npcManager == null || npcManager.GetTargetID() == -1)
            {
                return;
            }

            Dictionary<byte, object> opParameters = new()
            {
                { (byte)ParamterCode.Id, npcManager.GetTargetID()},
                { (byte)ParamterCode.SkillId, 53},
                //{ (byte)ParamterCode.SkillId, skill.Id},
            };

            PhotonManager.Instance.TrySendOperation(OperationCode.NpcSkill, opParameters);
        }

    }

    public void FindNpcAround()
    {
        if (npcManager != null)
        {
            npcManager.ChangeEmmy();
        }
    }

    public void Swtich()
    {
        isSkillPannel2 = !isSkillPannel2;
        if (SkillPannel1 != null)
        {
            SkillPannel1.SetActive(!isSkillPannel2);
        }

        if (SkillPannel2 != null)
        {
            SkillPannel2.SetActive(isSkillPannel2);
        }
    }
}
