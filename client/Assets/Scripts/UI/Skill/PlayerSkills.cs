using System.Collections.Generic;
using game.config;
using UnityEngine;
using UnityEngine.UI;
using Photon.ShareLibrary.Entities;

public class PlayerSkills : MonoBehaviour
{

    [SerializeField]
    public GameObject Skills;
    [SerializeField]
    public GameObject childPrefab;
    [SerializeField]
    public GameObject skillDetail;
    [SerializeField]
    public GameObject skillActive1;
    [SerializeField]
    public GameObject skillActive2;
    [SerializeField]
    public Button BtnSwitch;
    [SerializeField]
    public Button BtnClose;

    [SerializeField]
    public List<GameObject> SkillActives;
    private Dictionary<ushort, PlayerSkill> playerSkills;
    private bool IsSkillActive2 = false;
    private string lastSkillListSignature = string.Empty;
    private bool hasBuiltSkillList = false;

    void Start()
    {
        if (BtnSwitch != null)
        {
            BtnSwitch.onClick.AddListener(() => Switch());
        }

        if (BtnClose != null)
        {
            BtnClose.onClick.AddListener(() => gameObject.SetActive(false));
        }

        RefreshSkills();
    }

    public void RefreshSkills()
    {
        playerSkills = PlayerMain.instance != null
            ? PlayerMain.instance.playerSkills()
            : new Dictionary<ushort, PlayerSkill>();
        if (playerSkills == null)
        {
            playerSkills = new Dictionary<ushort, PlayerSkill>();
        }

        string skillListSignature = BuildSkillListSignature(playerSkills);
        bool shouldRebuildList = hasBuiltSkillList == false
            || skillListSignature != lastSkillListSignature
            || Skills == null
            || Skills.transform.childCount == 0;

        try
        {
            SetUpSkillActive();
        }
        catch (System.Exception exception)
        {
            Debug.LogWarning("PlayerSkills active setup failed: " + exception.Message);
        }

        if (shouldRebuildList)
        {
            try
            {
                ResetSkillList();
            }
            catch (System.Exception exception)
            {
                Debug.LogWarning("PlayerSkills reset list failed: " + exception.Message);
            }

            try
            {
                SetUpSkillList(playerSkills);
                lastSkillListSignature = skillListSignature;
                hasBuiltSkillList = true;
            }
            catch (System.Exception exception)
            {
                Debug.LogWarning("PlayerSkills list setup failed: " + exception.Message);
            }
        }
    }

    private void ResetSkillList()
    {
        if (Skills == null)
        {
            return;
        }

        foreach (Transform child in Skills.transform)
        {
            Destroy(child.gameObject);
        }
    }

    public void AddSkillToActive(PlayerSkill skill, int location)
    {
        if (skill == null || location < 0 || location >= SkillActives.Count)
        {
            return;
        }

        string locationSkill = PlayerPrefsKey.USER_SKILL_LOCATION + location;
        PlayerPrefs.SetInt(locationSkill, skill.id);
        SetUpSkillActive();
        skillDetail?.GetComponent<SkillDetail>()?.UpdateUIChange(location);
    }

    public void RemoveSkill(int location)
    {
        if (location < 0 || location >= SkillActives.Count)
        {
            return;
        }

        SkillItem skillItem = SkillActives[location]?.GetComponent<SkillItem>();
        skillItem?.RemoveSkillData();

        string locationSkill = PlayerPrefsKey.USER_SKILL_LOCATION + location;
        PlayerPrefs.SetInt(locationSkill, -1);
        skillDetail?.GetComponent<SkillDetail>()?.UpdateUIChange(-1);

    }

    void SetUpSkillActive()
    {
        if (SkillActives == null)
        {
            return;
        }

        for (int i = 0; i < SkillActives.Count; i++)
        {
            if (SkillActives[i] == null)
            {
                continue;
            }

            string locationSkill = PlayerPrefsKey.USER_SKILL_LOCATION + i;
            int targetSkillId = PlayerPrefs.GetInt(locationSkill, -1);

            if (targetSkillId > -1 && playerSkills.TryGetValue((ushort)targetSkillId, out PlayerSkill skill))
            {
                SkillItem skillItem = SkillActives[i].GetComponent<SkillItem>();
                skillItem?.SetUpSkillSetting(skill);
            }
            else
            {
                SkillItem skillItem = SkillActives[i].GetComponent<SkillItem>();
                skillItem?.RemoveSkillData();
            }
        }
    }

    void SetUpSkillList(Dictionary<ushort, PlayerSkill> playerSkills)
    {
        if (Skills == null || childPrefab == null || playerSkills == null)
        {
            return;
        }

        VerticalLayoutGroup verticalLayout = Skills.GetComponent<VerticalLayoutGroup>();
        Transform parent = verticalLayout != null ? verticalLayout.transform : Skills.transform;
        int createdCount = 0;

        foreach (KeyValuePair<ushort, PlayerSkill> pair in playerSkills)
        {
            if (ShouldHideSkill(pair.Value))
            {
                continue;
            }

            GameObject newChild = null;
            try
            {
                newChild = Instantiate(childPrefab, Vector3.zero, Quaternion.identity);
                newChild.transform.SetParent(parent, false);
                SkillItem skillItem = newChild.GetComponent<SkillItem>();
                skillItem?.SetUpSkillSetting(pair.Value);
                createdCount++;
            }
            catch (System.Exception exception)
            {
                Debug.LogWarning("PlayerSkills item failed for skillId=" + pair.Key + ": " + exception.Message);
                if (newChild != null)
                {
                    Destroy(newChild);
                }
            }
        }

        Debug.Log("PlayerSkills list refreshed. skills=" + playerSkills.Count + " items=" + createdCount);
    }

    private static string BuildSkillListSignature(Dictionary<ushort, PlayerSkill> playerSkills)
    {
        if (playerSkills == null || playerSkills.Count <= 0)
        {
            return string.Empty;
        }

        List<ushort> skillIds = new List<ushort>(playerSkills.Keys);
        skillIds.Sort();

        System.Text.StringBuilder result = new System.Text.StringBuilder(skillIds.Count * 8);
        foreach (ushort skillId in skillIds)
        {
            if (playerSkills.TryGetValue(skillId, out PlayerSkill skill) == false || ShouldHideSkill(skill))
            {
                continue;
            }

            result.Append(skill.id);
            result.Append(':');
            result.Append(skill.level);
            result.Append(';');
        }

        return result.ToString();
    }

    private static bool ShouldHideSkill(PlayerSkill skill)
    {
        if (skill == null || skill.id == 0)
        {
            return true;
        }

        return skill.id == 1 || skill.id == 2 || skill.id == 53;
    }

    public void Switch()
    {
        IsSkillActive2 = !IsSkillActive2;
        if (skillActive1 != null)
        {
            skillActive1.SetActive(!IsSkillActive2);
        }

        if (skillActive2 != null)
        {
            skillActive2.SetActive(IsSkillActive2);
        }
    }

    public void HideSkill()
    {
        skillDetail?.SetActive(false);
    }

    public void OpenSkillDetail(PlayerSkill skill)
    {
        if (skill == null)
        {
            return;
        }

        if (playerSkills == null)
        {
            playerSkills = PlayerMain.instance != null
                ? PlayerMain.instance.playerSkills()
                : new Dictionary<ushort, PlayerSkill>();
            if (playerSkills == null)
            {
                playerSkills = new Dictionary<ushort, PlayerSkill>();
            }
        }

        int location = -1;

        for (int i = 0; i < SkillActives.Count; i++)
        {
            string locationSkill = PlayerPrefsKey.USER_SKILL_LOCATION + i;
            int targetSkillId = PlayerPrefs.GetInt(locationSkill, -1);

            if (targetSkillId > -1 && playerSkills.TryGetValue((ushort)targetSkillId, out PlayerSkill skillData))
            {
                if (skillData.id == skill.id)
                {
                    location = i;
                    break;
                }
            }
        }

        skillDetail?.GetComponent<SkillDetail>()?.SetUpSkillSetting(skill, location, IsSkillActive2);
        skillDetail?.SetActive(true);
    }
}
