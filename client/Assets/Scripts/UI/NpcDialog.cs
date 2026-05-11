using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using game.network;
using game.network.jx;
using Photon.ShareLibrary.Handlers;
using UnityEngine;
using UnityEngine.UI;

public class NpcDialog : MonoBehaviour
{
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

    private const string DefaultCloseText = "Tạm biệt";
    private const string NextText = "Tiếp";
    private const string CloseText = "Đóng";

    private int npcId = -1;
    private readonly List<DialogOption> options = new();
    private readonly List<string> talkPages = new();
    private int talkPageIndex;
    private bool isTalkMode;
    private bool sendTalkConfirmToServer;

    private sealed class DialogOption
    {
        public int Index;
        public string Text;
    }

    private void Awake()
    {
        ConfigureMoreButton(false);
    }

    public void SetNpcWithContent(Sprite sprite, int npcId, string npcName, string name, string data)
    {
        this.npcId = npcId;
        SetNpcImage(sprite);

        string safeNpcName = NormalizeDisplayText(npcName);
        textNpcName.text = safeNpcName;

        string question = NormalizeDisplayText(name);
        List<string> parsedOptions = ParseOptionList(data);
        ShowQuestion(safeNpcName, question, parsedOptions, true);
    }

    public void SetClassicDialog(ClassicScriptDialog dialog)
    {
        if (dialog == null)
        {
            return;
        }

        npcId = -1;
        SetNpcImage(dialog.Select == 1 ? LoadClassicDialogSprite(dialog.SpritePath) : null);
        textNpcName.text = dialog.IsQuestion ? "Đối thoại" : "Thông báo";

        if (dialog.IsQuestion)
        {
            ShowQuestion(
                textNpcName.text,
                dialog.Question,
                dialog.Answers,
                dialog.RequiresServerResponse);
            return;
        }

        ShowTalk(dialog.TalkPages, dialog.RequiresServerResponse);
    }

    public void ShowListAction(string data)
    {
        ShowQuestion(textNpcName.text, textNpcContent.text, ParseOptionList(data), true);
    }

    private void ShowQuestion(string npcName, string question, List<string> answers, bool sendSelectionToServer)
    {
        isTalkMode = false;
        sendTalkConfirmToServer = false;
        talkPages.Clear();
        talkPageIndex = 0;
        ConfigureMoreButton(false);

        string content = string.IsNullOrWhiteSpace(question)
            ? npcName
            : "<color=green>" + EscapeRichText(npcName) + "</color>: " + NormalizeRichText(question);
        textNpcContent.text = content;

        VerticalLayoutGroup verticalLayout = ListActions.GetComponent<VerticalLayoutGroup>();
        ResetChildren(verticalLayout);

        options.Clear();
        if (answers != null)
        {
            for (int index = 0; index < answers.Count && index < 16; index++)
            {
                string answer = NormalizeDisplayText(answers[index]);
                if (string.IsNullOrWhiteSpace(answer))
                {
                    continue;
                }

                DialogOption option = new DialogOption
                {
                    Index = index,
                    Text = answer
                };
                options.Add(option);
                AddButton(verticalLayout, answer, () => SelectOption(option, sendSelectionToServer));
            }
        }

        AddButton(verticalLayout, DefaultCloseText, CloseDialog);
    }

    private void ShowTalk(IReadOnlyList<string> pages, bool confirmToServer)
    {
        isTalkMode = true;
        sendTalkConfirmToServer = confirmToServer;
        talkPages.Clear();

        if (pages != null)
        {
            foreach (string page in pages)
            {
                string text = NormalizeRichText(page);
                if (!string.IsNullOrWhiteSpace(text))
                {
                    talkPages.Add(text);
                }
            }
        }

        if (talkPages.Count == 0)
        {
            talkPages.Add(string.Empty);
        }

        talkPageIndex = 0;
        VerticalLayoutGroup verticalLayout = ListActions.GetComponent<VerticalLayoutGroup>();
        ResetChildren(verticalLayout);
        ConfigureMoreButton(true);
        RenderTalkPage();
    }

    private void RenderTalkPage()
    {
        string page = talkPageIndex >= 0 && talkPageIndex < talkPages.Count
            ? talkPages[talkPageIndex]
            : string.Empty;

        textNpcContent.text = page;

        VerticalLayoutGroup verticalLayout = ListActions.GetComponent<VerticalLayoutGroup>();
        ResetChildren(verticalLayout);

        bool hasNext = talkPageIndex + 1 < talkPages.Count;
        AddButton(verticalLayout, hasNext ? NextText : CloseText, AdvanceTalk);
    }

    private void AdvanceTalk()
    {
        if (!isTalkMode)
        {
            return;
        }

        if (talkPageIndex + 1 < talkPages.Count)
        {
            talkPageIndex++;
            RenderTalkPage();
            return;
        }

        if (sendTalkConfirmToServer)
        {
            SendSelection(0);
        }

        CloseDialog();
    }

    private void SelectOption(DialogOption option, bool sendSelectionToServer)
    {
        if (option == null)
        {
            return;
        }

        if (sendSelectionToServer)
        {
            SendSelection(option.Index);
        }

        CloseDialog();
    }

    private void SendSelection(int index)
    {
        Dictionary<byte, object> opParameters = new()
        {
            { (byte)ParamterCode.Id, npcId },
            { (byte)ParamterCode.Data, index }
        };

        PhotonManager.Instance.TrySendOperation(OperationCode.NpcSelect, opParameters);
    }

    private void AddButton(VerticalLayoutGroup verticalLayout, string title, Action action)
    {
        if (ButtonPrefab == null || verticalLayout == null)
        {
            return;
        }

        GameObject newChild = Instantiate(ButtonPrefab, Vector3.zero, Quaternion.identity);
        RectTransform rect = newChild.GetComponentInChildren<RectTransform>();
        if (rect != null)
        {
            rect.sizeDelta = new Vector2(120, 48);
        }

        Text text = newChild.GetComponentInChildren<Text>();
        if (text != null)
        {
            text.text = NormalizeRichText(title);
            text.supportRichText = true;
        }

        Button button = newChild.GetComponentInChildren<Button>();
        if (button != null)
        {
            button.onClick.RemoveAllListeners();
            button.onClick.AddListener(() => action?.Invoke());
        }

        newChild.transform.SetParent(verticalLayout.transform, false);
    }

    private void ResetChildren(VerticalLayoutGroup verticalLayout)
    {
        if (verticalLayout == null)
        {
            return;
        }

        foreach (Transform child in verticalLayout.transform)
        {
            Destroy(child.gameObject);
        }
    }

    private void SetNpcImage(Sprite sprite)
    {
        if (GameNpcUI == null)
        {
            return;
        }

        Image image = GameNpcUI.GetComponent<Image>();
        if (image == null)
        {
            return;
        }

        image.sprite = sprite;
        image.preserveAspect = true;
        GameNpcUI.SetActive(sprite != null);
    }

    private static Sprite LoadClassicDialogSprite(string spritePath)
    {
        if (string.IsNullOrWhiteSpace(spritePath))
        {
            return null;
        }

        try
        {
            return Game.Resource(spritePath).Get<Sprite>(game.resource.SPR.firstFrame);
        }
        catch (Exception exception)
        {
            Debug.LogWarning("NpcDialog SPR load failed: " + spritePath + " " + exception.GetBaseException().Message);
            return null;
        }
    }

    private void ConfigureMoreButton(bool talkMode)
    {
        if (MoreButton == null)
        {
            return;
        }

        MoreButton.SetActive(false);
        Button button = MoreButton.GetComponent<Button>();
        if (button == null)
        {
            button = MoreButton.AddComponent<Button>();
        }

        button.onClick.RemoveAllListeners();
        if (talkMode)
        {
            button.onClick.AddListener(AdvanceTalk);
        }
    }

    private void CloseDialog()
    {
        gameObject.SetActive(false);
    }

    private static List<string> ParseOptionList(string data)
    {
        List<string> result = new List<string>();
        if (string.IsNullOrWhiteSpace(data))
        {
            return result;
        }

        string normalized = data.Replace("\r\n", "\n").Replace("\r", "\n");
        string[] parts = normalized.Contains("<perfin>", StringComparison.OrdinalIgnoreCase)
            ? Regex.Split(normalized, "<perfin>", RegexOptions.IgnoreCase)
            : normalized.Split(new[] { '|' }, StringSplitOptions.None);

        foreach (string part in parts)
        {
            string option = NormalizeDisplayText(part);
            if (!string.IsNullOrWhiteSpace(option))
            {
                result.Add(option);
            }
        }

        return result;
    }

    private static string NormalizeRichText(string value)
    {
        value = NormalizeDisplayText(value);
        if (string.IsNullOrEmpty(value))
        {
            return string.Empty;
        }

        value = value
            .Replace("\\n", "\n")
            .Replace("<enter>", "\n", StringComparison.OrdinalIgnoreCase)
            .Replace("<br>", "\n", StringComparison.OrdinalIgnoreCase)
            .Replace("<br/>", "\n", StringComparison.OrdinalIgnoreCase)
            .Replace("<br />", "\n", StringComparison.OrdinalIgnoreCase);

        value = Regex.Replace(
            value,
            "<font\\s+color=['\"]?([^'\" >]+)['\"]?\\s*>",
            match => "<color=" + ResolveRichColor(match.Groups[1].Value) + ">",
            RegexOptions.IgnoreCase);
        value = Regex.Replace(value, "</font>", "</color>", RegexOptions.IgnoreCase);
        value = Regex.Replace(
            value,
            "<color=([^>]+)>",
            match => "<color=" + ResolveRichColor(match.Groups[1].Value) + ">",
            RegexOptions.IgnoreCase);
        value = Regex.Replace(
            value,
            "<c=([^>]+)>",
            match => "<color=" + ResolveRichColor(match.Groups[1].Value) + ">",
            RegexOptions.IgnoreCase);
        value = Regex.Replace(value, "</c>", "</color>", RegexOptions.IgnoreCase);
        value = Regex.Replace(value, "<c>", "</color>", RegexOptions.IgnoreCase);
        value = Regex.Replace(value, "<color>", "</color>", RegexOptions.IgnoreCase);
        value = Regex.Replace(value, "<bclr=[^>]+>", string.Empty, RegexOptions.IgnoreCase);
        value = Regex.Replace(value, "<bclr>", string.Empty, RegexOptions.IgnoreCase);
        return value.Trim();
    }

    private static string NormalizeDisplayText(string value)
    {
        return JxClassicClient.TranslateDisplayString(value ?? string.Empty).Trim();
    }

    private static string EscapeRichText(string value)
    {
        return (value ?? string.Empty).Replace("<", "&lt;").Replace(">", "&gt;");
    }

    private static string ResolveRichColor(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return "#ffffff";
        }

        string key = value.Trim().Trim('\'', '"').ToLowerInvariant();
        if (key.StartsWith("#"))
        {
            return key;
        }

        if (key.StartsWith("0x"))
        {
            return "#" + key.Substring(2);
        }

        return key switch
        {
            "yellow" or "hyellow" or "gyellow" or "dyellow" => "#ffff00",
            "white" => "#ffffff",
            "red" => "#ff0000",
            "green" or "wood" => "#00ff00",
            "blue" or "hblue" or "dblue" or "water" => "#1e90ff",
            "cyan" => "#00ffff",
            "gray" or "grey" => "#808080",
            "gold" or "metal" => "#ffd700",
            "purple" => "#ff00ff",
            "pink" => "#ffc0cb",
            "fire" => "#ff4500",
            "earth" => "#a0522d",
            _ => "#ffffff"
        };
    }
}
