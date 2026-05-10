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
        button.onClick.AddListener(() => PopUpCanvas.instance.OpenSkillDetail(skill));
        image.GetComponent<Button>().onClick.AddListener(() => PopUpCanvas.instance.OpenSkillDetail(skill));
        btnUdpateSkill.onClick.AddListener(() => Debug.Log("Update SKILL"));
    }

    public void SetUpSkillSetting(PlayerSkill skill)
    {
        if (skill == null)
        {
            RemoveSkillData();
            return;
        }

        this.skill = skill;
        SkillSetting skillSetting = SkillSetting.Get(skill.id, skill.level);
        nameSkill.text = SkillIconLoader.DisplayName(skillSetting, skill.id);
        levelSkill.text = SkillIconLoader.LevelText(skillSetting, skill.id, skill.level);
        SkillIcon.sprite = SkillIconLoader.LoadIcon(skill.id);
    }

    public void RemoveSkillData()
    {
        this.skill = null;
        nameSkill.text = "";
        levelSkill.text = "";
        Sprite loadedImage = Resources.Load<Sprite>(ImagePath);
        image.sprite = loadedImage;
    }
}

internal static class SkillIconLoader
{
    private const string SkillIconPath = "SkillIcon/";
    private const string FallbackIconPath = "WorldGameUI/Buttons/btn_fight";

    public static Sprite LoadIcon(int skillId)
    {
        Sprite sprite = null;

        if (skillId > 0)
        {
            sprite = Resources.Load<Sprite>(SkillIconPath + skillId);
        }

        return sprite != null ? sprite : Resources.Load<Sprite>(FallbackIconPath);
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
