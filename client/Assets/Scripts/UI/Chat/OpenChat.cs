using System.Collections.Generic;
using game.basemono;
using game.config;
using game.network;
using Photon.ShareLibrary.Constant;
using Photon.ShareLibrary.Handlers;
using UnityEngine;
using UnityEngine.Localization.Settings;
using UnityEngine.UI;

public class OpenChat : BaseMonoBehaviour
{

    [SerializeField] GameObject ChatList;
    [SerializeField] InputField ChatInput;
    [SerializeField] GameObject ChatClose;
    [SerializeField] GameObject ChatSend;
    [SerializeField] GameObject ColorPanel;
    [SerializeField] GameObject ColorButton;

    [SerializeField]
    public GameObject SystemMessage;
    [SerializeField]
    public GameObject MessageIn;
    [SerializeField]
    public GameObject MessageOut;
    [SerializeField]
    public GameObject CategoryObject;
    [SerializeField]
    public GameObject buttonPrefab;
    [SerializeField]
    public GameObject buttonCategory;
    [SerializeField]
    public GameObject pannelHome;

    private string[] listColor = { "red", "green", "yellow", "white", "orange" };
    private string chatInput = "";
    private string colorChat = "white";
    private PlayerChat playerChatSelect = PlayerChat.system;
    public ScrollRect scrollView;

    void Start()
    {
        ChatClose.GetComponent<Button>().onClick.AddListener(() =>
        {
            CloseChat();
        });

        ChatSend.GetComponent<Button>().onClick.AddListener(() =>
        {
            SendMessage();
        });

        ColorButton.GetComponent<Button>().onClick.AddListener(() =>
        {
            ColorPanel.SetActive(true);
        });

        InitColors();

        InitCategory();
    }

    public void AddNewMessage(MessageData data, PlayerChat playerChat)
    {
        if (playerChat == PlayerChat.system)
        {
            AddSystemMessage(data.Message);
        }
    }

    public void LoadMessageData()
    {
        Dictionary<PlayerChat, MessageData> chatMessages = PhotonManager.Instance.GetMessage();

        foreach (KeyValuePair<PlayerChat, MessageData> pair in chatMessages)
        {
            PlayerChat playerChat = pair.Key;
            MessageData messageData = pair.Value;

            if (playerChat == playerChatSelect)
            {
                if (playerChat == PlayerChat.system)
                {
                    AddSystemMessage(messageData.Message);
                }
            }
        }
    }

    void InitCategory()
    {
        VerticalLayoutGroup verticalLayout = CategoryObject.GetComponent<VerticalLayoutGroup>();
        CreateCatrgory("Hệ thống", PlayerChat.system, verticalLayout);
        CreateCatrgory("T.Giới", PlayerChat.world, verticalLayout);
        CreateCatrgory("Gần", PlayerChat.near, verticalLayout);
        CreateCatrgory("Bang", PlayerChat.tong, verticalLayout);
        CreateCatrgory("Mật", PlayerChat.hiden, verticalLayout);
        CreateCatrgory("Đội", PlayerChat.team, verticalLayout);
        CreateCatrgory("Môn phái", PlayerChat.menpai, verticalLayout);
    }

    void CreateCatrgory(string text, PlayerChat playerChat, VerticalLayoutGroup verticalLayout)
    {
        GameObject newChild = Instantiate(buttonCategory, Vector3.zero, Quaternion.identity);
        newChild.GetComponentInChildren<RectTransform>().sizeDelta = new Vector2(100, 70);
        newChild.GetComponentInChildren<Text>().text = text;
        newChild.GetComponentInChildren<Button>().onClick.AddListener(() => playerChatSelect = playerChat);
        newChild.transform.SetParent(verticalLayout.transform, false);
    }

    void InitColors()
    {
        foreach (string colorName in listColor)
        {
            GameObject gameObject = Instantiate(buttonPrefab, Vector3.zero, Quaternion.identity);

            Color color = ColorFromName(colorName);
            Image buttonImage = gameObject.GetComponent<Image>();
            buttonImage.color = color;

            gameObject.GetComponent<Button>().onClick.AddListener(() => OnColorButtonClicked(colorName));

            GridLayoutGroup verticalLayoutGroup = ColorPanel.GetComponent<GridLayoutGroup>();
            gameObject.transform.SetParent(verticalLayoutGroup.transform, false);
        }
    }

    private void OnColorButtonClicked(string colorName)
    {
        colorChat = colorName;
        ColorPanel.SetActive(false);
    }

    void Update()
    {
        chatInput = ChatInput.text;
    }

    public void CloseChat()
    {
        pannelHome.SetActive(false);
    }

    void InitMessage()
    {
        AddSystemMessage("Chúc mừng <color=yellow>BồMinhKhang</color> nhận bí kíp <color=red>Cực Tam Hoa Cửu Đỉnh</color>");
        AddSystemMessage("Chúc mừng <color=yellow>BồMinhKhang</color> nhận bí kíp <color=red>Cực Tam Hoa Cửu Đỉnh</color>");
        AddSystemMessage("Chúc mừng <color=yellow>BồMinhKhang</color> nhận bí kíp <color=red>Cực Tam Hoa Cửu Đỉnh</color>");
        AddSystemMessage("Chúc mừng <color=yellow>BồMinhKhang</color> nhận bí kíp <color=red>Cực Tam Hoa Cửu Đỉnh</color>");


        AddMessageIn("Hoả Long", "Đi dánh boss ko");
        AddMessageIn("Phiyen", "Đang làm nhiệm vụ");
        AddMessageIn("Bin Bao", "Đang phải làm viêc");
    }

    void SendMessage()
    {
        if (string.IsNullOrEmpty(chatInput))
        {
            ShowMessageBox(LocalizationSettings.StringDatabase.GetLocalizedString(ConfigGame.tableLanguage, "name_is_empty"), "error");
            return;
        }

        string message = "<color=" + colorChat + ">" + chatInput + "</color>";

        Dictionary<byte, object> opParameters = new()
                    {
                        {(byte) ParamterCode.Message,message},
                        {(byte) ParamterCode.ActionId,playerChatSelect},
                    };
        PhotonManager.Instance.TrySendOperation(OperationCode.DoChat, opParameters);

        AddMessageOut(name, message);
        ScrollToBottom();
        chatInput = "";
        ChatInput.text = "";
    }

    public void ScrollToBottom()
    {
        if (scrollView != null && scrollView.content != null)
        {
            Canvas.ForceUpdateCanvases(); // Ensure layout calculations are up-to-date
            scrollView.verticalNormalizedPosition = 0f; // Scroll to the bottom
        }
    }

    void AddSystemMessage(string message)
    {
        GameObject gameObject = Instantiate(SystemMessage, Vector3.zero, Quaternion.identity);
        gameObject.GetComponent<SystemMessage>().UpdateMessage(message);
        gameObject.transform.SetParent(ChatList.transform, false);
    }

    void AddMessageOut(string name, string message)
    {
        GameObject gameObject = Instantiate(MessageOut, Vector3.zero, Quaternion.identity);
        gameObject.GetComponent<MessageOut>().UpdateMessage(name, message);
        gameObject.transform.SetParent(ChatList.transform, false);

    }

    void AddMessageIn(string name, string message)
    {
        GameObject gameObject = Instantiate(MessageIn, Vector3.zero, Quaternion.identity);
        gameObject.GetComponent<MessageIN>().UpdateMessage(name, message);
        gameObject.transform.SetParent(ChatList.transform, false);
    }

    private Color ColorFromName(string colorName)
    {
        switch (colorName)
        {
            case "red": return Color.red;
            case "green": return Color.green;
            case "blue": return Color.blue;
            case "yellow": return Color.yellow;
            case "white": return Color.white;
            case "black": return Color.black;
            case "orange": return new Color(1f, 0.5f, 0f); // Orange color
            case "pink": return new Color(1f, 0.75f, 0.8f); // Pink color
            case "purple": return new Color(0.5f, 0f, 0.5f); // Purple color
            case "gray": return Color.gray;
            default: return Color.white;
        }
    }
}
