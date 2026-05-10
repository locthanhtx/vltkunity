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
    private const float ClassicForwardCastDistanceMps = 160f;

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
                    SkillSetting skillSetting = SkillIconLoader.TryGetBaseSetting(skillData.id);
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
        if (!TryResolveHotKeySkill(skillLocationCast, out PlayerSkill skill, out int targetSkillId))
        {
            Debug.LogWarning("PanelHotKeys CastSkill skipped. slot=" + skillLocationCast +
                             " storedSkillId=" + targetSkillId);
            return;
        }

        SkillSetting skillSetting = SkillIconLoader.TryGetBaseSetting(skill.id);
        int targetId = npcManager != null ? npcManager.GetTargetID() : -1;

        Dictionary<byte, object> opParameters = new()
        {
            { (byte)ParamterCode.SkillId, skill.id},
            { (byte)ParamterCode.SkillLevel, skill.level},
        };

        string targetMode = "forward";
        bool needsTargetOnly = SkillNeedsTargetOnly(skillSetting);
        bool canUseTarget = SkillCanUseTarget(skillSetting);

        if (SkillTargetsSelf(skillSetting))
        {
            opParameters[(byte)ParamterCode.MapX] = ClassicTargetSelfParamX;
            opParameters[(byte)ParamterCode.MapY] = PhotonManager.Instance.PlayerId;
            targetMode = "self";
        }
        else if (targetId > -1 && canUseTarget)
        {
            opParameters[(byte)ParamterCode.Id] = targetId;
            opParameters[(byte)ParamterCode.NpcType] = targetId;
            targetMode = "target";
        }
        else
        {
            if (needsTargetOnly)
            {
                Debug.LogWarning("PanelHotKeys CastSkill needs target. slot=" + skillLocationCast +
                                 " skillId=" + skill.id +
                                 " storedSkillId=" + targetSkillId);
                return;
            }

            if (TryGetMainPlayerMps(out Vector2 mpsPosition))
            {
                Vector2 targetMps = GetForwardSkillTargetMps(mpsPosition);
                opParameters[(byte)ParamterCode.MapX] = Mathf.RoundToInt(targetMps.x);
                opParameters[(byte)ParamterCode.MapY] = Mathf.RoundToInt(targetMps.y);
            }
        }

        Debug.Log("PanelHotKeys CastSkill slot=" + skillLocationCast +
                  " storedSkillId=" + targetSkillId +
                  " sendSkillId=" + skill.id +
                  " level=" + skill.level +
                  " targetMode=" + targetMode +
                  " targetId=" + targetId);
        PhotonManager.Instance.TrySendOperation(OperationCode.NpcSkill, opParameters);
    }

    private bool TryResolveHotKeySkill(int skillLocationCast, out PlayerSkill skill, out int targetSkillId)
    {
        UpdateSkill();

        skill = null;
        targetSkillId = -1;

        if (skillLocationCast < 0 || SkillActives == null || skillLocationCast >= SkillActives.Count)
        {
            return false;
        }

        string locationSkill = PlayerPrefsKey.USER_SKILL_LOCATION + skillLocationCast;
        targetSkillId = PlayerPrefs.GetInt(locationSkill, -1);
        return targetSkillId > 0 &&
            playerSkills != null &&
            playerSkills.TryGetValue((ushort)targetSkillId, out skill);
    }

    private static bool SkillCanUseTarget(SkillSetting skillSetting)
    {
        if (skillSetting == null || skillSetting.m_nId <= 0)
        {
            return true;
        }

        return skillSetting.m_bTargetEnemy != 0 || skillSetting.m_bTargetOnly != 0;
    }

    private static bool SkillNeedsTargetOnly(SkillSetting skillSetting)
    {
        return skillSetting != null && skillSetting.m_bTargetOnly != 0;
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

        return game.network.jx.JxClassicMovement.AdvanceMpsPosition(sourceMps, direction, ClassicForwardCastDistanceMps);
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
