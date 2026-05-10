using UnityEngine;
using UnityEngine.UI;

public class ActionSp : MonoBehaviour
{
    private GameObject ButtonSwitchHorse;

    [SerializeField]
    public GameObject ButtonSit;

    private GameObject ButtonRun;

    // Start is called before the first frame update
    void Start()
    {
        ButtonSwitchHorse = FindChildGameObject("BtnSwitchHorse");
        ButtonRun = FindChildGameObject("ButtonRun");

        Button sitButton = ButtonSit != null ? ButtonSit.GetComponent<Button>() : null;
        if (sitButton != null)
        {
            sitButton.onClick.AddListener(() =>
            {
                if (PlayerMain.instance != null) PlayerMain.instance.PlayerSit();
            });
        }

        Button switchHorseButton = ButtonSwitchHorse != null ? ButtonSwitchHorse.GetComponent<Button>() : null;
        if (switchHorseButton != null)
        {
            switchHorseButton.onClick.AddListener(() =>
            {
                SwithHorse();
            });
        }
        else
        {
            Debug.LogWarning("ActionSp missing BtnSwitchHorse button.");
        }

        Button runButton = ButtonRun != null ? ButtonRun.GetComponent<Button>() : null;
        if (runButton != null)
        {
            runButton.onClick.AddListener(() =>
            {
                if (PlayerMain.instance != null) PlayerMain.instance.PlayerRun();
            });
        }

        InvokeRepeating(nameof(InitHorse), 1f, 1f);
    }

    private GameObject FindChildGameObject(string childName)
    {
        Transform child = transform.Find(childName);
        return child != null ? child.gameObject : null;
    }

    /// <summary>
    /// Horse
    /// </summary>
    public void InitHorse()
    {
        if (PlayerMain.instance != null)
        {
            UpdateHosreUI(PlayerMain.instance.IsUseHorse);
            CancelInvoke();
        }
    }

    void SwithHorse()
    {
        if (PlayerMain.instance == null)
        {
            return;
        }

        bool isUseHose = PlayerMain.instance.PlayerSwitchHorse();
        UpdateHosreUI(isUseHose);
    }

    public void UpdateHosreUI(bool isUseHose)
    {
        Sprite loadedImage = Resources.Load<Sprite>(isUseHose ? "WorldGameUI/Buttons/btn_ride_up" : "WorldGameUI/Buttons/btn_ride_down");
        if (ButtonSwitchHorse != null)
        {
            Image image = ButtonSwitchHorse.GetComponent<Image>();
            if (image != null)
            {
                image.sprite = loadedImage;
            }
        }
    }

}
