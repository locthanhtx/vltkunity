using game.resource.mapping;
using Photon.ShareLibrary.Constant;
using UnityEngine;
using UnityEngine.Analytics;

public class SelectSeries : MonoBehaviour
{
    // Start is called before the first frame update
    public string gender;
    public bool isActive;

    private string PathCharacter = "";
    private game.resource.SPR.FrameCount frameLength = 0;
    private SpriteRenderer spriteRenderer;
    private NPCSERIES attributeSelect;

    private int framePerSeconds = game.resource.SPR.FPS;
    private int _gender;
    private int _series;
    private bool isChangeStandBy = false;
    private float DetaCurrent = 0;
    private ushort CurrentIndex = 0;

    private RectTransform rectTransform;
    void Start()
    {
        UnityEngine.Device.Application.targetFrameRate = 60;
        attributeSelect = NPCSERIES.series_metal;

        rectTransform = gameObject.AddComponent<UnityEngine.RectTransform>();
        spriteRenderer = gameObject.AddComponent<UnityEngine.SpriteRenderer>();

        rectTransform.sizeDelta = new UnityEngine.Vector2(0, 0);

        ShowCharacter();
    }

    private void ShowCharacter()
    {
        _gender = gender == "man" ? 1 : 0;
        _series = Mathf.Clamp((int)attributeSelect, (int)NPCSERIES.series_metal, (int)NPCSERIES.series_earth);
        CurrentIndex = 0;

        if (isActive)
        {
            PathCharacter = CreatePlayer.CharacterSeries.GetPath(_series, _gender, 1);
            isChangeStandBy = true;
            framePerSeconds = 12;
        }
        else
        {
            PathCharacter = CreatePlayer.CharacterSeries.GetPath(_series, _gender, 2);
            isChangeStandBy = false;
            framePerSeconds = game.resource.SPR.FPS;
        }

        frameLength = Game.Resource(PathCharacter).Get<game.resource.SPR.FrameCount>();
    }

    public void Update()
    {
        if (frameLength == 0)
        {
            spriteRenderer.sprite = null;
            return;
        }

        if (CurrentIndex >= frameLength)
        {
            if (isChangeStandBy)
            {
                isChangeStandBy = false;
                CurrentIndex = 0;
                PathCharacter = CreatePlayer.CharacterSeries.GetPath(_series, _gender, 0);
                frameLength = Game.Resource(PathCharacter).Get<game.resource.SPR.FrameCount>();
            }
            return;
        }
        float deta = Time.timeSinceLevelLoad - DetaCurrent;
        ushort framesIndex = (ushort)((deta * this.framePerSeconds) % this.frameLength);

        if (CurrentIndex == framesIndex)
        {
            return;
        }
        CurrentIndex = framesIndex;

        game.resource.SPR.FrameInfo frameInfo = Game.Resource(PathCharacter).Get<game.resource.SPR.FrameInfo>(framesIndex);
        Sprite sprite = Game.Resource(PathCharacter).Get<UnityEngine.Sprite>(frameInfo);
        Vector2 sizeDelta = new(((float)frameInfo.width / 100), ((float)frameInfo.height / 100));

        spriteRenderer.sortingOrder = 2;
        rectTransform.sizeDelta = sizeDelta;
        spriteRenderer.sprite = sprite;

        if (framesIndex == frameLength - 1)
        {
            DetaCurrent = Time.timeSinceLevelLoad;
            if (isChangeStandBy) ChangeToStandByActive();
        }
    }

    public void ChangeToStandByActive()
    {
        framePerSeconds = game.resource.SPR.FPS;
        isChangeStandBy = false;
        CurrentIndex = 0;
        PathCharacter = CreatePlayer.CharacterSeries.GetPath(_series, _gender, 0);
        frameLength = Game.Resource(PathCharacter).Get<game.resource.SPR.FrameCount>();
    }

    public void ChangeAttributeType(NPCSERIES attributeType)
    {
        attributeSelect = attributeType;

        if (attributeSelect == NPCSERIES.series_water)
        {
            isActive = gender != "man";
        }

        if (attributeSelect == NPCSERIES.series_metal)
        {
            isActive = gender != "girl";
        }

        ShowCharacter();
    }

    public void ChangeActive(bool active)
    {
        DetaCurrent = Time.timeSinceLevelLoad;
        isActive = active;
        ShowCharacter();
    }

    public void ReShow()
    {
        DetaCurrent = Time.timeSinceLevelLoad;
        ShowCharacter();
    }

}
