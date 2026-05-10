using System.Collections;
using System.Collections.Generic;
using game.resource.settings.skill;
using Photon.ShareLibrary.Entities;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class SkillDetail : MonoBehaviour
{
    // Start is called before the first frame update

    [SerializeField]
    public Image SkillIcon;
    [SerializeField]
    public Text nameSkill;
    [SerializeField]
    public Text levelSkill;
    [SerializeField]
    public Text skillDetail;
    [SerializeField]
    public GameObject btnCancel;
    [SerializeField]
    public GameObject panelSkill1;
    [SerializeField]
    public GameObject panelSkill2;

    private PlayerSkill skill;
    private int location;
    private bool isSkillActive2;

    private void Start()
    {
        Button cancelButton = btnCancel != null ? btnCancel.GetComponent<Button>() : null;
        if (cancelButton != null)
        {
            cancelButton.onClick.AddListener(() => RemoveSkillActive());
        }
    }

    public void SetUpSkillSetting(PlayerSkill skill, int location, bool isSkillActive2)
    {
        this.skill = skill;
        this.location = location;
        this.isSkillActive2 = isSkillActive2;
        UIChange();
        InitSkillDetail();
    }

    void RemoveSkillActive()
    {
        PopUpCanvas.instance?.RemoveSkill(location);
    }

    public void UpdateUIChange(int location)
    {
        this.location = location;
        UIChange();
    }

    void UIChange()
    {
        if (location > -1)
        {
            btnCancel?.SetActive(true);
            panelSkill1?.SetActive(false);
            panelSkill2?.SetActive(false);
        }
        else
        {
            btnCancel?.SetActive(false);
            panelSkill1?.SetActive(!isSkillActive2);
            panelSkill2?.SetActive(isSkillActive2);
        }
    }

    void InitSkillDetail()
    {
        if (skill == null)
        {
            if (nameSkill != null)
            {
                nameSkill.text = string.Empty;
            }

            if (levelSkill != null)
            {
                levelSkill.text = string.Empty;
            }

            if (skillDetail != null)
            {
                skillDetail.text = string.Empty;
            }

            if (SkillIcon != null)
            {
                SkillIcon.sprite = SkillIconLoader.LoadIcon(0);
            }
            return;
        }

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

        // Skill detail
        if (skillDetail != null)
        {
            skillDetail.text = SkillIconLoader.Description(skillSetting, skill.id);
        }
    }

    public void UseSkill(int location)
    {
        if (skill != null)
        {
            PopUpCanvas.instance?.SetUPSkillLocation(skill, location);
        }
    }
}
