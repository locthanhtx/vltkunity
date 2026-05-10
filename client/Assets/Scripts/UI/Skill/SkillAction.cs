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
    private const int ClassicTargetSelfParamX = -1;

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

        if (SkillActives == null)
        {
            return;
        }

        for (int i = 0; i < SkillActives.Count; i++)
        {
            string locationSkill = PlayerPrefsKey.USER_SKILL_LOCATION + i;
            int targetSkillId = PlayerPrefs.GetInt(locationSkill, -1);
            Button skillUI = SkillActives[i];
            if (skillUI == null)
            {
                continue;
            }

            if (targetSkillId > -1 && playerSkills.TryGetValue((ushort)targetSkillId, out PlayerSkill skillData))
            {
                if (skillUI.image != null)
                {
                    SkillSetting skillSetting = SkillIconLoader.TryGetSetting(skillData.id, skillData.level);
                    skillUI.image.sprite = SkillIconLoader.LoadIcon(skillSetting, skillData.id);
                }
            }
            else
            {
                Sprite loadedImage = Resources.Load<Sprite>(ImagePath);
                if (skillUI.image != null)
                {
                    skillUI.image.sprite = loadedImage;
                }
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
            SkillSetting skillSetting = SkillIconLoader.TryGetSetting(skill.id, skill.level);
            int targetId = npcManager != null ? npcManager.GetTargetID() : -1;

            Dictionary<byte, object> opParameters = new()
            {
                { (byte)ParamterCode.SkillId, skill.id},
                { (byte)ParamterCode.SkillLevel, skill.level},
            };

            bool shouldUseTarget = SkillShouldUseTarget(skillSetting);

            if (targetId > -1 && shouldUseTarget)
            {
                opParameters[(byte)ParamterCode.Id] = targetId;
                opParameters[(byte)ParamterCode.NpcType] = targetId;
            }
            else if (shouldUseTarget)
            {
                return;
            }
            else if (SkillTargetsSelf(skillSetting))
            {
                opParameters[(byte)ParamterCode.MapX] = ClassicTargetSelfParamX;
                opParameters[(byte)ParamterCode.MapY] = PhotonManager.Instance.PlayerId;
            }
            else if (TryGetMainPlayerMps(out Vector2 mpsPosition))
            {
                Vector2 targetMps = GetForwardSkillTargetMps(mpsPosition);
                opParameters[(byte)ParamterCode.MapX] = Mathf.RoundToInt(targetMps.x);
                opParameters[(byte)ParamterCode.MapY] = Mathf.RoundToInt(targetMps.y);
            }

            PhotonManager.Instance.TrySendOperation(OperationCode.NpcSkill, opParameters);
        }

    }

    private static bool SkillShouldUseTarget(SkillSetting skillSetting)
    {
        if (skillSetting == null || skillSetting.m_nId <= 0)
        {
            return true;
        }

        return skillSetting.m_bTargetEnemy != 0 || skillSetting.m_bTargetOnly != 0;
    }

    private static bool SkillTargetsSelf(SkillSetting skillSetting)
    {
        return skillSetting != null && skillSetting.m_bTargetSelf != 0;
    }

    private bool TryGetMainPlayerMps(out Vector2 mpsPosition)
    {
        if (wordlGame == null && PhotonManager.Instance != null)
        {
            wordlGame = PhotonManager.Instance.world;
        }

        if (wordlGame != null && wordlGame.GetMainPlayer() != null)
        {
            mpsPosition = wordlGame.GetMainPlayerMpsPosition();
            return true;
        }

        mpsPosition = Vector2.zero;
        return false;
    }

    private Vector2 GetForwardSkillTargetMps(Vector2 sourceMps)
    {
        int direction = wordlGame != null && wordlGame.GetMainPlayer() != null
            ? wordlGame.GetMainPlayer().GetDirection()
            : 0;

        if (direction < 0 || direction > 63)
        {
            direction = 0;
        }

        return game.network.jx.JxClassicMovement.AdvanceMpsPosition(sourceMps, direction, 160f);
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
