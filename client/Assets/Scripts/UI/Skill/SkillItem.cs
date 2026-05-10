using game.resource.settings.skill;
using Photon.ShareLibrary.Entities;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class SkillItem : MonoBehaviour
{
    [SerializeField]
    public Image SkillIcon;
    [SerializeField]
    public Text nameSkill;
    [SerializeField]
    public Text levelSkill;
    [SerializeField]
    public Button button;
    [SerializeField]
    public Image image;
    [SerializeField]
    public Button btnUdpateSkill;

    private PlayerSkill skill;
    private string ImagePath = "WorldGameUI/Buttons/btn_fight";

    private void Start()
    {
        Button mainButton = button != null ? button : GetComponent<Button>();
        if (mainButton != null)
        {
            mainButton.onClick.AddListener(() => PopUpCanvas.instance?.OpenSkillDetail(skill));
        }

        Button imageButton = image != null ? image.GetComponent<Button>() : null;
        if (imageButton != null && imageButton != mainButton)
        {
            imageButton.onClick.AddListener(() => PopUpCanvas.instance?.OpenSkillDetail(skill));
        }

        if (btnUdpateSkill != null)
        {
            btnUdpateSkill.onClick.AddListener(() => Debug.Log("Update SKILL"));
        }
    }

    public void SetUpSkillSetting(PlayerSkill skill)
    {
        if (skill == null)
        {
            RemoveSkillData();
            return;
        }

        this.skill = skill;
        SkillSetting skillSetting = SkillIconLoader.TryGetSetting(skill.id, skill.level);
        if (nameSkill != null)
        {
            nameSkill.text = SkillIconLoader.DisplayName(skillSetting, skill.id);
        }

        if (levelSkill != null)
        {
            levelSkill.text = SkillIconLoader.LevelText(skillSetting, skill.id, skill.level);
        }

        if (SkillIcon != null)
        {
            SkillIcon.sprite = SkillIconLoader.LoadIcon(skillSetting, skill.id);
        }
    }

    public void RemoveSkillData()
    {
        this.skill = null;
        if (nameSkill != null)
        {
            nameSkill.text = "";
        }

        if (levelSkill != null)
        {
            levelSkill.text = "";
        }

        Sprite loadedImage = Resources.Load<Sprite>(ImagePath);
        if (image != null)
        {
            image.sprite = loadedImage;
        }

        if (SkillIcon != null)
        {
            SkillIcon.sprite = loadedImage;
        }
    }
}

internal static class SkillIconLoader
{
    private const string SkillIconPath = "SkillIcon/";
    private const string FallbackIconPath = "WorldGameUI/Buttons/btn_fight";
    private const string DefaultSkillSprPath = "\\spr\\Ui\\技能图标\\枪法.spr";
    private static readonly System.Collections.Generic.HashSet<string> MissingSprIconLogs = new();

    public static Sprite LoadIcon(int skillId)
    {
        SkillSetting skillSetting = TryGetSetting(skillId, 1);
        return LoadIcon(skillSetting, skillId);
    }

    public static SkillSetting TryGetSetting(int skillId, int skillLevel)
    {
        if (skillId <= 0)
        {
            return null;
        }

        try
        {
            return SkillSetting.Get(skillId, Mathf.Max(1, skillLevel));
        }
        catch (System.Exception exception)
        {
            Debug.LogWarning("Skill setting failed for skillId=" + skillId + " level=" + skillLevel + ": " + exception.GetBaseException().Message);
            return null;
        }
    }

    public static Sprite LoadIcon(SkillSetting skillSetting, int skillId)
    {
        Sprite sprite = null;

        if (skillSetting != null && string.IsNullOrEmpty(skillSetting.m_szSkillIcon) == false)
        {
            sprite = LoadSprIcon(skillSetting.m_szSkillIcon);
        }

        if (sprite == null && skillId > 0)
        {
            sprite = Resources.Load<Sprite>(SkillIconPath + skillId);
        }

        if (sprite == null)
        {
            sprite = LoadSprIcon(DefaultSkillSprPath);
        }

        return sprite != null ? sprite : Resources.Load<Sprite>(FallbackIconPath);
    }

    private static Sprite LoadSprIcon(string sprPath)
    {
        if (string.IsNullOrEmpty(sprPath))
        {
            return null;
        }

        try
        {
            Sprite sprite = Game.Resource(sprPath).Get<Sprite>(game.resource.SPR.firstFrame);
            if (sprite == null && MissingSprIconLogs.Add(sprPath))
            {
                Debug.LogWarning("Skill icon SPR missing: " + sprPath);
            }

            return sprite;
        }
        catch (System.Exception exception)
        {
            if (MissingSprIconLogs.Add(sprPath))
            {
                Debug.LogWarning("Skill icon SPR failed: " + sprPath + " " + exception.GetBaseException().Message);
            }

            return null;
        }
    }

    public static bool HasValidSetting(SkillSetting skillSetting, int skillId)
    {
        return skillSetting != null
            && skillSetting.m_nId == skillId
            && string.IsNullOrEmpty(skillSetting.m_szName) == false;
    }

    public static string DisplayName(SkillSetting skillSetting, int skillId)
    {
        return HasValidSetting(skillSetting, skillId) ? skillSetting.m_szName : "Skill " + skillId;
    }

    public static string LevelText(SkillSetting skillSetting, int skillId, int level)
    {
        int maxLevel = HasValidSetting(skillSetting, skillId) ? skillSetting.m_maxLevel : 0;
        return level + " / " + maxLevel;
    }

    public static string Description(SkillSetting skillSetting, int skillId)
    {
        if (HasValidSetting(skillSetting, skillId) == false)
        {
            return string.Empty;
        }

        string text = string.Empty;

        if (string.IsNullOrEmpty(skillSetting.m_property) == false)
        {
            text += skillSetting.m_property;
        }

        if (string.IsNullOrEmpty(skillSetting.m_szSkillDesc) == false)
        {
            if (text.Length > 0)
            {
                text += "\n";
            }

            text += skillSetting.m_szSkillDesc;
        }

        try
        {
            string description = skillSetting.GetDescription();
            if (string.IsNullOrEmpty(description) == false)
            {
                if (text.Length > 0)
                {
                    text += "\n";
                }

                text += description;
            }
        }
        catch (System.Exception exception)
        {
            Debug.LogWarning("Skill description failed for skillId=" + skillId + ": " + exception.Message);
        }

        return text;
    }
}
