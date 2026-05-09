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
    private string ImageSkillPath = "SkillIcon/";

    void Start()
    {
        wordlGame = mainGameObject.GetComponent<World>();
        charManager = mainGameObject.GetComponent<CharManager>();
        npcManager = mainGameObject.GetComponent<NpcManager>();

        ButtonTargetNPC.onClick.AddListener(() => FindNpcAround());
        ButtonSwitch.onClick.AddListener(() => Swtich());
    }

    public void UpdateSkill()
    {
        playerSkills = PlayerMain.instance.playerSkills();

        for (int i = 0; i < SkillActives.Count; i++)
        {
            string locationSkill = PlayerPrefsKey.USER_SKILL_LOCATION + i;
            int targetSkillId = PlayerPrefs.GetInt(locationSkill, -1);
            Button skillUI = SkillActives[i];

            if (targetSkillId > -1 && playerSkills.TryGetValue((ushort)targetSkillId, out PlayerSkill skillData))
            {
                SkillSetting skillSetting = SkillSetting.Get(skillData.id, skillData.level);

                Sprite sprite = Resources.Load<Sprite>(ImageSkillPath + skillData.id);
                if (sprite == null)
                {
                    sprite = Game.Resource(skillSetting.m_szSkillIcon).Get<UnityEngine.Sprite>(0);
                }
                skillUI.image.sprite = sprite;
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
        string locationSkill = PlayerPrefsKey.USER_SKILL_LOCATION + skillLocationCast;
        int targetSkillId = PlayerPrefs.GetInt(locationSkill, -1);

        if (targetSkillId > -1 && playerSkills.TryGetValue((ushort)targetSkillId, out PlayerSkill skill))
        {
            if (npcManager.GetTargetID() == -1)
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
        npcManager.ChangeEmmy();
    }

    public void Swtich()
    {
        isSkillPannel2 = !isSkillPannel2;
        SkillPannel1.SetActive(!isSkillPannel2);
        SkillPannel2.SetActive(isSkillPannel2);
    }
}
