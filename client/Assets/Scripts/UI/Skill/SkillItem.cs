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
        SkillSetting skillSetting = SkillIconLoader.TryGetBaseSetting(skill.id);
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
    private static readonly System.Collections.Generic.Dictionary<int, SkillSetting> BaseSettingCache = new();
    private static readonly System.Collections.Generic.Dictionary<string, Sprite> SprIconCache = new();
    private static readonly System.Collections.Generic.Dictionary<int, Sprite> ResourceIconCache = new();
    private static readonly System.Collections.Generic.HashSet<string> MissingSprIconLogs = new();
    private static readonly System.Collections.Generic.HashSet<int> MissingSkillNameLogs = new();

    public static Sprite LoadIcon(int skillId)
    {
        SkillSetting skillSetting = TryGetBaseSetting(skillId);
        return LoadIcon(skillSetting, skillId);
    }

    public static SkillSetting TryGetBaseSetting(int skillId)
    {
        if (skillId <= 0)
        {
            return null;
        }

        if (BaseSettingCache.TryGetValue(skillId, out SkillSetting cachedSetting))
        {
            return cachedSetting;
        }

        try
        {
            SkillSetting skillSetting = SkillSetting.GetBase(skillId);
            BaseSettingCache[skillId] = skillSetting;
            return skillSetting;
        }
        catch (System.Exception exception)
        {
            Debug.LogWarning("Skill base setting failed for skillId=" + skillId + ": " + exception.GetBaseException().Message);
            BaseSettingCache[skillId] = null;
            return null;
        }
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
            if (ResourceIconCache.TryGetValue(skillId, out Sprite cachedResourceIcon))
            {
                sprite = cachedResourceIcon;
            }
            else
            {
                sprite = Resources.Load<Sprite>(SkillIconPath + skillId);
                ResourceIconCache[skillId] = sprite;
            }
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
            if (SprIconCache.TryGetValue(sprPath, out Sprite cachedSprite))
            {
                return cachedSprite;
            }

            Sprite sprite = Game.Resource(sprPath).Get<Sprite>(game.resource.SPR.firstFrame);
            SprIconCache[sprPath] = sprite;
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
        string skillName = HasValidSetting(skillSetting, skillId)
            ? skillSetting.m_szName
            : LoadNameFromSkillTable(skillId);

        if (string.IsNullOrEmpty(skillName))
        {
            return "Skill " + skillId;
        }

        return skillName + " (" + skillId + ")";
    }

    private static string LoadNameFromSkillTable(int skillId)
    {
        if (skillId <= 0)
        {
            return string.Empty;
        }

        try
        {
            game.resource.Table skillsTable = game.resource.Cache.Settings.Skill.skillsTable;
            System.Collections.Generic.Dictionary<int, int> rowMapping =
                game.resource.Cache.Settings.Skill.skillsIdToRowIndexMapping;

            if (skillsTable == null || rowMapping == null || rowMapping.TryGetValue(skillId, out int rowIndex) == false)
            {
                if (MissingSkillNameLogs.Add(skillId))
                {
                    Debug.LogWarning("Skill name missing from Skill.txt: skillId=" + skillId);
                }

                return string.Empty;
            }

            string skillName = skillsTable.Get<string>("SkillName", rowIndex);
            if (string.IsNullOrEmpty(skillName))
            {
                return string.Empty;
            }

            return skillsTable.GetEncoding().byteOrderMarks == 0
                ? game.resource.formater.TCVN3.UTF8(skillName)
                : skillName;
        }
        catch (System.Exception exception)
        {
            if (MissingSkillNameLogs.Add(skillId))
            {
                Debug.LogWarning("Skill name load failed for skillId=" + skillId + ": " + exception.GetBaseException().Message);
            }

            return string.Empty;
        }
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
