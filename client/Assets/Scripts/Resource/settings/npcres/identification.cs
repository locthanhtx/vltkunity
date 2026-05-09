
namespace game.resource.settings.npcres
{
    public class Identification
    {
        public enum Camp
        {
            Begin = 0,
            Justice,
            Evil,
            Balance,
            Free,
            UnIdentified,
        }

        public enum Series
        {
            Metal = 0,
            Wood,
            Water,
            Fire,
            Earth,
            UnIdentified,
        }

        private class NpcSeries
        {
            private static UnityEngine.Sprite[] sprites = null;

            ////////////////////////////////////////////////////////////////////////////////

            private static UnityEngine.Sprite GetSprite(Identification.Series series)
            {
                if(NpcSeries.sprites == null)
                {
                    NpcSeries.sprites = new UnityEngine.Sprite[5];
                    string[] paths = new string[5];

                    paths[(int)Identification.Series.Metal] = mapping.userInterface.NpcBobo.Series.metal;
                    paths[(int)Identification.Series.Wood] = mapping.userInterface.NpcBobo.Series.wood;
                    paths[(int)Identification.Series.Water] = mapping.userInterface.NpcBobo.Series.water;
                    paths[(int)Identification.Series.Fire] = mapping.userInterface.NpcBobo.Series.fire;
                    paths[(int)Identification.Series.Earth] = mapping.userInterface.NpcBobo.Series.earth;

                    for(int index = 0; index < paths.Length; index++)
                    {
                        NpcSeries.sprites[index] = Game.Resource(paths[index]).Get<UnityEngine.Sprite>(resource.SPR.firstFrame);
                    }
                }

                return NpcSeries.sprites[(int)series];
            }

            ////////////////////////////////////////////////////////////////////////////////

            private readonly UnityEngine.GameObject parent;
            private readonly UnityEngine.RectTransform rectTransformComponent;
            private readonly UnityEngine.SpriteRenderer spriteRendererComponent;

            ////////////////////////////////////////////////////////////////////////////////
            
            public NpcSeries(string objectName)
            {
                this.parent = new UnityEngine.GameObject(objectName);
                this.rectTransformComponent = this.parent.AddComponent<UnityEngine.RectTransform>();
                this.spriteRendererComponent = this.parent.AddComponent<UnityEngine.SpriteRenderer>();

                this.rectTransformComponent.sizeDelta = new UnityEngine.Vector2(0.18f, 0.18f);
            }

            public UnityEngine.GameObject GetAppearance() => this.parent;

            public void SetSeries(Identification.Series series)
            {
                this.spriteRendererComponent.sprite = NpcSeries.GetSprite(series);
            }

            public int GetWidth() => 18;

            public int GetHeight() => 18;

            public void SetActive(bool active) => this.parent.SetActive(active);

            public bool IsActive() => this.parent.activeSelf;

            public void SetAnchoredPosition(float xx, float yy) => this.rectTransformComponent.anchoredPosition = new UnityEngine.Vector2(xx, yy);

            public void Release() => UnityEngine.GameObject.Destroy(this.parent);
        }

        private class NpcLife
        {
            private const int lineWidth = 60;
            private const int lineHeight = 6;

            private static UnityEngine.Sprite backgroundLine = null;
            private static UnityEngine.Sprite greenLine = null;
            private static UnityEngine.Sprite yellowLine = null;
            private static UnityEngine.Sprite redLine = null;

            ////////////////////////////////////////////////////////////////////////////////

            private static UnityEngine.Texture2D GetTextureByColor(UnityEngine.Color32 color32)
            {
                UnityEngine.Texture2D texture2D = new UnityEngine.Texture2D(NpcLife.lineWidth, NpcLife.lineHeight, UnityEngine.TextureFormat.RGBA32, false);

                for (int y = 0; y < NpcLife.lineHeight; y++)
                {
                    for (int x = 0; x < NpcLife.lineWidth; x++)
                    {
                        texture2D.SetPixel(x, y, color32);
                    }
                }

                texture2D.Apply();
                return texture2D;
            }

            private static UnityEngine.Sprite GetBackgroundLineSprite()
            {
                if (NpcLife.backgroundLine != null)
                {
                    return NpcLife.backgroundLine;
                }

                return NpcLife.backgroundLine = UnityEngine.Sprite.Create(NpcLife.GetTextureByColor(new UnityEngine.Color32(255, 255, 255, 80)), new UnityEngine.Rect(0, 0, NpcLife.lineWidth, NpcLife.lineHeight), new UnityEngine.Vector2(0.5f, 0.5f));
            }

            private static UnityEngine.Sprite GetGreenLineSprite()
            {
                if(NpcLife.greenLine != null)
                {
                    return NpcLife.greenLine;
                }

                return NpcLife.greenLine = UnityEngine.Sprite.Create(NpcLife.GetTextureByColor(UnityEngine.Color.green), new UnityEngine.Rect(0, 0, NpcLife.lineWidth, NpcLife.lineHeight), new UnityEngine.Vector2(0.5f, 0.5f));
            }

            private static UnityEngine.Sprite GetYellowLineSprite()
            {
                if (NpcLife.yellowLine != null)
                {
                    return NpcLife.yellowLine;
                }

                return NpcLife.yellowLine = UnityEngine.Sprite.Create(NpcLife.GetTextureByColor(UnityEngine.Color.yellow), new UnityEngine.Rect(0, 0, NpcLife.lineWidth, NpcLife.lineHeight), new UnityEngine.Vector2(0.5f, 0.5f));
            }

            private static UnityEngine.Sprite GetRedLineSprite()
            {
                if (NpcLife.redLine != null)
                {
                    return NpcLife.redLine;
                }

                return NpcLife.redLine = UnityEngine.Sprite.Create(NpcLife.GetTextureByColor(UnityEngine.Color.red), new UnityEngine.Rect(0, 0, NpcLife.lineWidth, NpcLife.lineHeight), new UnityEngine.Vector2(0.5f, 0.5f));
            }

            ////////////////////////////////////////////////////////////////////////////////

            private readonly UnityEngine.GameObject parent;
            private readonly UnityEngine.RectTransform rectTransformComponent;
            private readonly UnityEngine.GameObject backgroundObject;
            private readonly UnityEngine.SpriteRenderer backgroundSpriteRendererComponent;
            private readonly UnityEngine.GameObject currentLifeObject;
            private readonly UnityEngine.RectTransform currentLifeRectTransformComponent;
            private readonly UnityEngine.SpriteRenderer currentLifeSpriteRendererComponent;

            private int currentPercent;

            ////////////////////////////////////////////////////////////////////////////////

            public NpcLife(string objectName)
            {
                this.parent = new UnityEngine.GameObject(objectName);
                this.rectTransformComponent = this.parent.AddComponent<UnityEngine.RectTransform>();
                this.backgroundObject = new UnityEngine.GameObject("background");
                this.backgroundSpriteRendererComponent = this.backgroundObject.AddComponent<UnityEngine.SpriteRenderer>();
                this.currentLifeObject = new UnityEngine.GameObject("current");
                this.currentLifeRectTransformComponent = this.currentLifeObject.AddComponent<UnityEngine.RectTransform>();
                this.currentLifeSpriteRendererComponent = this.currentLifeObject.AddComponent<UnityEngine.SpriteRenderer>();

                this.currentPercent = 100;

                this.rectTransformComponent.sizeDelta = new UnityEngine.Vector2(NpcLife.lineWidth / 100f, NpcLife.lineHeight / 100f);
                this.backgroundObject.transform.SetParent(this.parent.transform, false);
                this.currentLifeObject.transform.SetParent(this.parent.transform, false);
                this.currentLifeRectTransformComponent.sizeDelta = this.rectTransformComponent.sizeDelta;
            }

            public UnityEngine.GameObject GetAppearance() => this.parent;

            public void SetActive(bool active)
            {
                this.parent.SetActive(active);

                if (active == true)
                {
                    if(this.backgroundSpriteRendererComponent.sprite == null)
                    {
                        this.backgroundSpriteRendererComponent.sprite = NpcLife.GetBackgroundLineSprite();
                    }

                    this.SetPercent(this.currentPercent);
                }
            }

            public bool IsActive() => this.parent.activeSelf;

            public int GetWidth() => NpcLife.lineWidth;

            public int GetHeight() => NpcLife.lineHeight;

            public void SetPercent(int percent)
            {
                this.currentPercent = percent;

                if (this.IsActive() == false)
                {
                    return;
                }

                UnityEngine.Sprite sprite = NpcLife.GetGreenLineSprite();

                if (percent <= 35)
                {
                    sprite = NpcLife.GetRedLineSprite();
                }
                else if (percent <= 65)
                {
                    sprite = NpcLife.GetYellowLineSprite();
                }

                const float originPositionX = (float)NpcLife.lineWidth / 2;
                float scale = (float)percent / 100;

                this.currentLifeRectTransformComponent.anchoredPosition = new UnityEngine.Vector3((originPositionX - (scale * NpcLife.lineWidth / 2)) / -100, 0);
                this.currentLifeRectTransformComponent.localScale = new UnityEngine.Vector3(scale, 1, 1);
                this.currentLifeSpriteRendererComponent.sprite = sprite;
            }

            public void SetAnchoredPositionY(float yy)
            {
                this.rectTransformComponent.anchoredPosition = new UnityEngine.Vector2(0, yy);
            }

            public void Release() => UnityEngine.GameObject.Destroy(this.parent);
        }

        private class TextObject
        {
            private readonly UnityEngine.GameObject parent;
            private readonly UnityEngine.MeshRenderer meshRendererComponent;
            private readonly TMPro.TextMeshPro textMeshProComponent;
            private readonly UnityEngine.RectTransform rectTransformComponent;

            public TextObject(string objectName)
            {
                this.parent = new UnityEngine.GameObject(objectName);
                this.meshRendererComponent = this.parent.AddComponent<UnityEngine.MeshRenderer>();
                this.textMeshProComponent = this.parent.AddComponent<TMPro.TextMeshPro>();
                this.rectTransformComponent = this.parent.GetComponent<UnityEngine.RectTransform>();

                this.textMeshProComponent.fontSize = 1.2f;
                this.textMeshProComponent.fontStyle = TMPro.FontStyles.Bold;
                this.textMeshProComponent.color = UnityEngine.Color.white;
                this.textMeshProComponent.alignment = TMPro.TextAlignmentOptions.Center;
                this.textMeshProComponent.horizontalAlignment = TMPro.HorizontalAlignmentOptions.Center;

                //this.textMeshProComponent.fontMaterial.EnableKeyword(TMPro.ShaderUtilities.Keyword_Underlay);
                //this.textMeshProComponent.fontMaterial.SetColor(TMPro.ShaderUtilities.ID_UnderlayColor, UnityEngine.Color.black);
                //this.textMeshProComponent.fontMaterial.SetFloat(TMPro.ShaderUtilities.ID_UnderlayOffsetX, 0);
                //this.textMeshProComponent.fontMaterial.SetFloat(TMPro.ShaderUtilities.ID_UnderlayOffsetY, 0);
                //this.textMeshProComponent.fontMaterial.SetFloat(TMPro.ShaderUtilities.ID_UnderlayDilate, 1);
                //this.textMeshProComponent.fontMaterial.SetFloat(TMPro.ShaderUtilities.ID_UnderlaySoftness, 0);

                //this.textMeshProComponent.UpdateMeshPadding();
            }

            public UnityEngine.GameObject GetAppearance() => this.parent;

            public void SetAnchoredPositionY(float yy)
            {
                this.rectTransformComponent.anchoredPosition = new UnityEngine.Vector2(0, yy);
            }

            public void SetBorderColor(UnityEngine.Color color)
            {
                this.textMeshProComponent.fontMaterial.SetColor(TMPro.ShaderUtilities.ID_UnderlayColor, color);
            }

            public void SetTextColor(UnityEngine.Color color)
            {
                this.textMeshProComponent.color = color;
            }

            public void SetGold(byte type)
            {
                if (type == 0)
                    this.textMeshProComponent.color = UnityEngine.Color.white;
                else
                //if (type == 1)
                //    this.textMeshProComponent.color = UnityEngine.Color.green;
                //else
                if (type <= 6)
                    this.textMeshProComponent.color = new UnityEngine.Color(100/255f, 100/255f, 1f, 1f);
                else
                    this.textMeshProComponent.color = new UnityEngine.Color(234/255f, 189/255f, 11/255f, 1f);
            }

            public void SetText(string text)
            {
                this.parent.SetActive(true);
                this.rectTransformComponent.sizeDelta = new UnityEngine.Vector2(20, 10);

                this.textMeshProComponent.text = text;
                this.textMeshProComponent.ForceMeshUpdate();

                this.rectTransformComponent.sizeDelta = this.textMeshProComponent.GetRenderedValues(true);
            }

            public string GetText() => this.textMeshProComponent.text;

            public UnityEngine.Vector2 GetSize() => this.rectTransformComponent.sizeDelta;

            public void SetActive(bool active) => this.parent.SetActive(active);

            public bool IsActive() => this.parent.activeSelf;

            public void Release() => UnityEngine.GameObject.Destroy(this.parent);
        }

        ////////////////////////////////////////////////////////////////////////////////

        private readonly UnityEngine.GameObject parent;

        private float offsetY;

        private string mapPosValue;
        private string titleValue;
        private string tongNameValue;
        private string tongTitleValue;
        private string nameValue;
        private Identification.Camp campValue;
        private Identification.Series seriesValue;
        private int healthPercent;

        private bool mapPosOnMapActive;
        private bool titleOnMapActive;
        private bool tongOnMapActive;
        private bool nameOnMapActive;
        private bool healthOnMapActive;

        private Identification.TextObject mapPosShape;
        private Identification.TextObject titleShape;
        private Identification.TextObject tongShape;
        private Identification.TextObject nameShape;
        private Identification.NpcSeries seriesShape;
        private Identification.NpcLife healthShape;

        ////////////////////////////////////////////////////////////////////////////////

        public Identification()
        {
            this.parent = new UnityEngine.GameObject("npc.res.identify");

            this.offsetY = 0;
            
            this.mapPosValue = string.Empty;
            this.titleValue = string.Empty;
            this.tongNameValue = string.Empty;
            this.tongTitleValue = string.Empty;
            this.nameValue = string.Empty;
            this.campValue = Identification.Camp.UnIdentified;
            this.seriesValue = Identification.Series.UnIdentified;
            this.healthPercent = 0;

            this.titleOnMapActive = true;
            this.tongOnMapActive = true;
            this.nameOnMapActive = true;
            this.healthOnMapActive = true;
        }

        public void Destroy()
        {
            UnityEngine.GameObject.Destroy(this.parent);
        }

        private float UpdateLayout()
        {
            float anchoredPositionY = this.offsetY;

            if (this.healthShape != null)
            {
                this.healthShape.SetAnchoredPositionY(anchoredPositionY);
                anchoredPositionY += (this.healthShape.GetHeight() / 100f) * 2;
                anchoredPositionY += 0.02f;
            }

            if (this.nameShape != null)
            {
                this.nameShape.SetAnchoredPositionY(anchoredPositionY);

                if (this.seriesShape != null)
                {
                    this.seriesShape.SetAnchoredPosition(this.nameShape.GetSize().x / 2 + 0.15f, anchoredPositionY);
                }

                anchoredPositionY += this.nameShape.GetSize().y;
            }

            if (this.tongShape != null)
            {
                this.tongShape.SetAnchoredPositionY(anchoredPositionY);
                anchoredPositionY += this.tongShape.GetSize().y;
            }

            if (this.titleShape != null)
            {
                this.titleShape.SetAnchoredPositionY(anchoredPositionY);
                anchoredPositionY += this.titleShape.GetSize().y;
            }

            if (this.mapPosShape != null)
            {
                this.mapPosShape.SetAnchoredPositionY(anchoredPositionY);
            }

            return anchoredPositionY;
        }

        public UnityEngine.GameObject GetAppearance() => this.parent;

        public float SetNpcPate(int value)
        {
            this.offsetY = (float)value / 100;
            return this.UpdateLayout();
        }

        public void SetScenePosition(UnityEngine.Vector3 position)
        {
            this.parent.transform.position = position;
        }

        ////////////////////////////////////////////////////////////////////////////////

        public void SetCamp(Identification.Camp camp)
        {
            this.campValue = camp;

            if(this.nameShape == null
                && this.tongShape == null)
            {
                return;
            }

            UnityEngine.Color color = UnityEngine.Color.white;

            switch (this.campValue)
            {
                case Identification.Camp.UnIdentified:
                case Identification.Camp.Begin:
                    break;

                case Identification.Camp.Justice:
                    color.r = 255f / 255; color.g = 168f / 255; color.b = 94f / 255;
                    break;

                case Identification.Camp.Evil:
                    color.r = 255f / 255; color.g = 146f / 255; color.b = 255f / 255;
                    break;

                case Identification.Camp.Balance:
                    color.r = 85f / 255; color.g = 255f / 255; color.b = 145f / 255;
                    break;

                case Identification.Camp.Free:
                    color.r = 255f / 255; color.g = 0f / 255; color.b = 0f / 255;
                    break;

                default:
                    color.r = 255f / 255; color.g = 0f / 255; color.b = 255f / 255;
                    break;
            }

            if(nameShape != null)
            {
                this.nameShape.SetTextColor(color);
            }

            if(tongShape != null)
            {
                this.tongShape.SetTextColor(color);
            }
        }

        public void SetMapPos(string mapPos)
        {
            this.mapPosValue = mapPos;

            if (this.mapPosValue.CompareTo(string.Empty) == 0)
            {
                if (this.mapPosShape != null)
                {
                    this.mapPosShape.Release();
                    this.mapPosShape = null;
                    this.UpdateLayout();
                }
            }
            else if (this.mapPosOnMapActive == false) { }
            else
            {
                if (this.mapPosShape == null)
                {
                    this.mapPosShape = new Identification.TextObject("npc.map.pos");
                    this.mapPosShape.GetAppearance().transform.SetParent(this.parent.transform, false);
                    //this.mapPosShape.SetTextColor(game.Style.RGBA("#ffffffff"));
                    this.mapPosShape.SetText(this.mapPosValue);
                    this.UpdateLayout();
                }
                else
                {
                    this.mapPosShape.SetText(this.mapPosValue);
                }
            }
        }

        public void SetTitle(string title)
        {
            this.titleValue = title;

            if(this.titleValue.CompareTo(string.Empty) == 0
                || this.IsDuplicateOfName(this.titleValue))
            {
                if(this.titleShape != null)
                {
                    this.titleShape.Release();
                    this.titleShape = null;
                    this.UpdateLayout();
                }
            }
            else if(this.titleOnMapActive ==  false) { }
            else
            {
                if(this.titleShape == null)
                {
                    this.titleShape = new Identification.TextObject("npc.title");
                    this.titleShape.GetAppearance().transform.SetParent(this.parent.transform, false);
                    this.titleShape.SetTextColor(new UnityEngine.Color(1, 0, 1, 1));
                    this.titleShape.SetText(this.titleValue);
                    this.UpdateLayout();
                }
                else
                {
                    this.titleShape.SetText(this.titleValue);
                }
            }
        }

        public void SetTong(string name, string title)
        {
            this.tongNameValue = name;
            this.tongTitleValue = title;
            string tongText = this.tongNameValue + this.tongTitleValue;

            if(this.tongNameValue.CompareTo(string.Empty) == 0
                || this.IsDuplicateOfName(tongText))
            {
                this.tongTitleValue = string.Empty;

                if(this.tongShape != null)
                {
                    this.tongShape.Release();
                    this.tongShape = null;
                    this.UpdateLayout();
                }
            }
            else if(this.tongOnMapActive == false) { }
            else
            {
                if(this.tongShape == null)
                {
                    this.tongShape = new Identification.TextObject("npc.tong");
                    this.tongShape.GetAppearance().transform.SetParent(this.parent.transform, false);
                    this.tongShape.SetText(tongText);
                    this.SetCamp(this.campValue);
                    this.UpdateLayout();
                }
                else
                {
                    this.tongShape.SetText(tongText);
                    this.SetCamp(this.campValue);
                }
            }
        }

        public void SetGold(byte gold) => nameShape.SetGold(gold);
        public void SetTextColor(UnityEngine.Color color) => nameShape.SetTextColor(color);

        public string GetName() => this.nameValue;

        public void SetName(string name)
        {
            this.nameValue = name;

            if(this.nameValue.CompareTo(string.Empty) == 0)
            {
                if(this.seriesShape != null)
                {
                    this.seriesShape.Release();
                    this.seriesShape = null;
                }

                if(this.nameShape != null)
                {
                    this.nameShape.Release();
                    this.nameShape = null;
                    this.UpdateLayout();
                }
            }
            else if(this.nameOnMapActive == false) { }
            else
            {
                if(this.nameShape == null)
                {
                    this.nameShape = new Identification.TextObject("npc.name");
                    this.nameShape.GetAppearance().transform.SetParent(this.parent.transform, false);
                    this.nameShape.SetText(this.nameValue);
                    this.UpdateLayout();
                }

                if(this.seriesShape == null
                    && this.seriesValue != Identification.Series.UnIdentified)
                {
                    this.seriesShape = new Identification.NpcSeries("npc.series");
                    this.seriesShape.GetAppearance().transform.SetParent(this.parent.transform, false);
                    this.seriesShape.SetSeries(this.seriesValue);
                    this.UpdateLayout();
                }

                this.nameShape.SetText(this.nameValue);
                this.SetCamp(this.campValue);

                if (this.IsDuplicateOfName(this.tongNameValue + this.tongTitleValue))
                {
                    this.SetTong(string.Empty, string.Empty);
                }

                if (this.IsDuplicateOfName(this.titleValue))
                {
                    this.SetTitle(string.Empty);
                }
            }
        }

        private bool IsDuplicateOfName(string text)
        {
            return string.IsNullOrEmpty(this.nameValue) == false
                && string.IsNullOrEmpty(text) == false
                && string.Compare(this.NormalizeLabelText(this.nameValue), this.NormalizeLabelText(text), System.StringComparison.OrdinalIgnoreCase) == 0;
        }

        private string NormalizeLabelText(string text)
        {
            return string.IsNullOrEmpty(text)
                ? string.Empty
                : text.Replace(" ", string.Empty).Trim();
        }

        public void SetSeries(Identification.Series series)
        {
            this.seriesValue = series;

            if (this.seriesValue == Identification.Series.UnIdentified)
            {
                if(this.seriesShape != null)
                {
                    this.seriesShape.Release();
                    this.seriesShape = null;
                }
            }
            else if (this.nameOnMapActive == false) { }
            else if (this.nameShape != null)
            {
                if(this.seriesShape == null)
                {
                    this.seriesShape = new Identification.NpcSeries("npc.series");
                    this.seriesShape.GetAppearance().transform.SetParent(this.parent.transform, false);
                    this.UpdateLayout();
                }

                this.seriesShape.SetSeries(this.seriesValue);
            }
        }

        public void SetHealthPercent(int percent)
        {
            this.healthPercent = percent;

            if(this.healthPercent == 0)
            {
                if(this.healthShape != null)
                {
                    this.healthShape.Release();
                    this.healthShape = null;
                }
            }
            else if (this.healthOnMapActive == false) { }
            else
            {
                if(this.healthShape == null)
                {
                    this.healthShape = new Identification.NpcLife("npc.health");
                    this.healthShape.GetAppearance().transform.SetParent(this.parent.transform, false);
                    this.UpdateLayout();
                }

                this.healthShape.SetPercent(this.healthPercent);
            }
        }

        ////////////////////////////////////////////////////////////////////////////////

        public void SetMapPosOnMapActive(bool active)
        {
            this.mapPosOnMapActive = active;

            if (this.mapPosOnMapActive == true
                && this.mapPosValue.CompareTo(string.Empty) != 0
                && this.mapPosShape == null)
            {
                this.SetMapPos(this.mapPosValue);
            }
            else if (this.mapPosOnMapActive == false
                && this.mapPosShape != null)
            {
                this.mapPosShape.Release();
                this.mapPosShape = null;
                this.UpdateLayout();
            }
        }

        public void SetTitleOnMapActive(bool active)
        {
            this.titleOnMapActive = active;

            if(this.titleOnMapActive == true 
                && this.titleValue.CompareTo(string.Empty) != 0 
                && this.titleShape == null)
            {
                this.SetTitle(this.titleValue);
            }
            else if (this.titleOnMapActive == false
                && this.titleShape != null)
            {
                this.titleShape.Release();
                this.titleShape = null;
                this.UpdateLayout();
            }
        }

        public void SetTongOnMapActive(bool active)
        {
            this.tongOnMapActive = active;

            if(this.tongOnMapActive == true
                && this.tongNameValue.CompareTo(string.Empty) != 0
                && this.tongShape == null)
            {
                this.SetTong(this.tongNameValue, this.tongTitleValue);
            }
            else if (this.tongOnMapActive == false
                && this.tongShape != null)
            {
                this.tongShape.Release();
                this.tongShape = null;
                this.UpdateLayout();
            }
        }

        public void SetNameOnMapActive(bool active)
        {
            this.nameOnMapActive = active;

            if(this.nameOnMapActive == true
                && this.nameValue.CompareTo(string.Empty) != 0
                && this.nameShape == null)
            {
                this.SetName(this.nameValue);
            }
            else if (this.nameOnMapActive == false)
            {
                if (this.seriesShape != null)
                {
                    this.seriesShape.Release();
                    this.seriesShape = null;
                }

                if (this.nameShape != null)
                {
                    this.nameShape.Release();
                    this.nameShape = null;
                    this.UpdateLayout();
                }
            }
        }

        public void SetHealthOnMapActive(bool active)
        {
            this.healthOnMapActive = active;

            if(this.healthOnMapActive == true
                && this.healthPercent > 0
                && this.healthShape == null)
            {
                this.SetHealthPercent(this.healthPercent);
            }
            else if (healthOnMapActive == false
                && this.healthShape != null)
            {
                this.healthShape.Release();
                this.healthShape = null;
                this.UpdateLayout();
            }
        }
    }
}
