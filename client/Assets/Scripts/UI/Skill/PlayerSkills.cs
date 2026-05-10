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

    void Start()
    {
        BtnSwitch.onClick.AddListener(() => Switch());
        BtnClose.onClick.AddListener(() => gameObject.SetActive(false));

        RefreshSkills();
    }

    public void RefreshSkills()
    {
        playerSkills = PlayerMain.instance != null
            ? PlayerMain.instance.playerSkills()
            : new Dictionary<ushort, PlayerSkill>();

        ResetSkillList();
        SetUpSkillActive();
        SetUpSkillList(playerSkills);
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
        string locationSkill = PlayerPrefsKey.USER_SKILL_LOCATION + location;
        PlayerPrefs.SetInt(locationSkill, skill.id);
        SetUpSkillActive();
        skillDetail.GetComponent<SkillDetail>().UpdateUIChange(location);
    }

    public void RemoveSkill(int location)
    {
        SkillItem skillItem = SkillActives[location].GetComponent<SkillItem>();
        skillItem.RemoveSkillData();

        string locationSkill = PlayerPrefsKey.USER_SKILL_LOCATION + location;
        PlayerPrefs.SetInt(locationSkill, -1);
        skillDetail.GetComponent<SkillDetail>().UpdateUIChange(-1);

    }

    void SetUpSkillActive()
    {
        for (int i = 0; i < SkillActives.Count; i++)
        {
            string locationSkill = PlayerPrefsKey.USER_SKILL_LOCATION + i;
            int targetSkillId = PlayerPrefs.GetInt(locationSkill, -1);

            if (targetSkillId > -1 && playerSkills.TryGetValue((ushort)targetSkillId, out PlayerSkill skill))
            {
                SkillItem skillItem = SkillActives[i].GetComponent<SkillItem>();
                skillItem.SetUpSkillSetting(skill);
            }
        }
    }

    void SetUpSkillList(Dictionary<ushort, PlayerSkill> playerSkills)
    {
        VerticalLayoutGroup verticalLayout = Skills.GetComponent<VerticalLayoutGroup>();

        foreach (KeyValuePair<ushort, PlayerSkill> pair in playerSkills)
        {
            GameObject newChild = Instantiate(childPrefab, Vector3.zero, Quaternion.identity);
            SkillItem skillItem = newChild.GetComponent<SkillItem>();
            skillItem.SetUpSkillSetting(pair.Value);
            newChild.transform.SetParent(verticalLayout.transform, false);
        }
    }

    public void Switch()
    {
        IsSkillActive2 = !IsSkillActive2;
        skillActive1.SetActive(!IsSkillActive2);
        skillActive2.SetActive(IsSkillActive2);
    }

    public void HideSkill()
    {
        skillDetail.SetActive(false);
    }

    public void OpenSkillDetail(PlayerSkill skill)
    {
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

        skillDetail.GetComponent<SkillDetail>().SetUpSkillSetting(skill, location, IsSkillActive2);
        skillDetail.SetActive(true);
    }
}
