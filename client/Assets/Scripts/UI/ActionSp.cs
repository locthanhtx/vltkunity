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
        ButtonSwitchHorse = gameObject.transform.Find("BtnSwitchHorse").gameObject;
        ButtonRun = gameObject.transform.Find("ButtonRun").gameObject;

        ButtonSit.GetComponent<Button>().onClick.AddListener(() =>
        {
            if (PlayerMain.instance != null) PlayerMain.instance.PlayerSit();
        });

        ButtonSwitchHorse.GetComponent<Button>().onClick.AddListener(() =>
        {
            SwithHorse();
        });

        ButtonRun.GetComponent<Button>().onClick.AddListener(() =>
        {
            if (PlayerMain.instance != null) PlayerMain.instance.PlayerRun();
        });

        InvokeRepeating(nameof(InitHorse), 1f, 1f);
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
