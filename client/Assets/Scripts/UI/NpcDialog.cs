using System;
using System.Collections.Generic;
using System.Drawing;
using System.Net.NetworkInformation;
using game.network;
using Photon.ShareLibrary.Handlers;
using UnityEngine;
using UnityEngine.UI;

public class NpcDialog : MonoBehaviour
{
    // Start is called before the first frame update

    [SerializeField]
    private Text textNpcName;
    [SerializeField]
    private Text textNpcContent;
    [SerializeField]
    private GameObject ListActions;
    [SerializeField]
    private GameObject GameNpcUI;
    [SerializeField]
    public GameObject ButtonPrefab;
    [SerializeField]
    private GameObject MoreButton;

    private int NpcID = -1;
    private string ButtonCanncel = "Tạm biệt";
    private string[] slicedParts;

    public void SetNpcWithContent(Sprite sprite, int npcId, string npcName, string name, string data)
    {
        string content = "<color=green>" + npcName + "</color>:" + name;
        textNpcName.text = npcName;
        textNpcContent.text = content;
        this.NpcID = npcId;

        GameNpcUI.GetComponent<Image>().sprite = sprite;
        GameNpcUI.GetComponent<Image>().preserveAspect = true;

        ShowListAction(data);
    }

    public void ShowListAction(string data)
    {
        VerticalLayoutGroup verticalLayout = ListActions.GetComponent<VerticalLayoutGroup>();
        ResetChildren(verticalLayout);

        if (!string.IsNullOrEmpty(data))
        {
            slicedParts = data.Split("<perfin>");
            MoreButton.SetActive(slicedParts.Length > 3);

            foreach (string action in slicedParts)
            {
                GameObject newChild = Instantiate(ButtonPrefab, Vector3.zero, Quaternion.identity);
                newChild.GetComponentInChildren<RectTransform>().sizeDelta = new Vector2(100, 60);
                newChild.GetComponentInChildren<Text>().text = action;
                newChild.GetComponentInChildren<Button>().onClick.AddListener(() => Action(action));
                newChild.transform.SetParent(verticalLayout.transform, false);
            }
        }

        AddButtonCanncel(verticalLayout);
    }

    void AddButtonCanncel(VerticalLayoutGroup verticalLayout)
    {
        GameObject newChild = Instantiate(ButtonPrefab, Vector3.zero, Quaternion.identity);
        newChild.GetComponentInChildren<RectTransform>().sizeDelta = new Vector2(100, 60);
        newChild.GetComponentInChildren<Text>().text = ButtonCanncel;
        newChild.GetComponentInChildren<Button>().onClick.AddListener(() => gameObject.SetActive(false));
        newChild.transform.SetParent(verticalLayout.transform, false);
    }

    void ResetChildren(VerticalLayoutGroup verticalLayout)
    {
        foreach (Transform child in verticalLayout.transform)
        {
            Destroy(child.gameObject);
        }
    }

    void Action(string action)
    {
        int index = System.Array.IndexOf(slicedParts, action);

        Dictionary<byte, object> opParameters = new()
        {
                { (byte)ParamterCode.Id, NpcID},
                { (byte)ParamterCode.Data,  index }
        };

        PhotonManager.Instance.TrySendOperation(OperationCode.NpcSelect, opParameters);
    }
}
