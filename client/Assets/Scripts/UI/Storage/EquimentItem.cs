using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using game.network.jx;
using game.resource.settings.item;
using Photon.ShareLibrary.Constant;
using Photon.ShareLibrary.Entities;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class EquimentItem : MonoBehaviour, IPointerClickHandler
{
    [SerializeField]
    private GameObject ImageItem;
    [SerializeField]
    private GameObject ImageFrame;

    private game.resource.settings.Item item;
    private ItemData itemData;
    private int cellIndex = -1;

    private void Awake()
    {
        ConfigureInteractable();
    }

    private void Start()
    {
        ConfigureInteractable();
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (eventData != null && eventData.button != PointerEventData.InputButton.Left)
        {
            return;
        }

        OpenDetail();
    }

    private void ConfigureInteractable()
    {
        Image rootImage = GetComponent<Image>();
        if (rootImage != null)
        {
            rootImage.raycastTarget = true;
        }

        Button button = GetComponent<Button>();
        if (button == null)
        {
            button = gameObject.AddComponent<Button>();
        }

        button.interactable = true;
        if (button.targetGraphic == null && rootImage != null)
        {
            button.targetGraphic = rootImage;
        }

        ConfigureChildImage(ImageItem);
        ConfigureChildImage(ImageFrame);
    }

    private static void ConfigureChildImage(GameObject imageObject)
    {
        if (imageObject == null)
        {
            return;
        }

        Image image = imageObject.GetComponent<Image>();
        if (image == null)
        {
            return;
        }

        image.preserveAspect = true;
        image.raycastTarget = false;
    }

    private void OpenDetail()
    {
        if (item != null)
        {
            Debug.Log("EquimentItem click item id=" + item.GetDatabaseId() +
                      " cell=" + cellIndex +
                      " local=" + (itemData != null ? itemData.Local.ToString() : "<null>") +
                      " x=" + (itemData != null ? itemData.X.ToString() : "<null>") +
                      " y=" + (itemData != null ? itemData.Y.ToString() : "<null>"));
            ItemDetailPopup.Show(item, itemData, cellIndex, transform as RectTransform);
        }
        else
        {
            ItemDetailPopup.Close();
        }
    }

    public void SetItemEquiment(game.resource.settings.Item item, int cellIndex = -1, ItemData itemData = null)
    {
        this.item = item;
        this.itemData = itemData;
        this.cellIndex = cellIndex;

        // Image
        Sprite thumbnailSprite = this.item.GetThumbnailSprite();
        ImageItem.GetComponent<Image>().sprite = thumbnailSprite;
        ImageItem.SetActive(thumbnailSprite != null);

        /// KHung
        if (item.GetItemType() == Defination.Type.goldEquip)
        {
            ImageFrame.SetActive(false);
            //game.style.canvas.Base framedTypeGold = new game.style.canvas.Base("\\user.interface\\panel.equipment\\item.tab\\item.framed.type.gold.js");
            //framedTypeGold.SetCurrent(framedTypeGold.template.current + ".viewport.item." + ((System.DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() * 10) + (++index)));
            //framedTypeGold.SetParent(gameObject);
        }
        else
        {
            Sprite typeSprite = this.item.GetTypeSprite();
            ImageFrame.GetComponent<Image>().sprite = typeSprite;
            ImageFrame.SetActive(typeSprite != null);
        }
    }

    public void ClearItem()
    {
        this.item = null;
        this.itemData = null;
        this.cellIndex = -1;

        if (ImageItem != null)
        {
            ImageItem.GetComponent<Image>().sprite = null;
            ImageItem.SetActive(false);
        }

        if (ImageFrame != null)
        {
            ImageFrame.GetComponent<Image>().sprite = null;
            ImageFrame.SetActive(false);
        }
    }
}

public static class ItemDetailPopup
{
    private static GameObject current;
    private const int MagicDurability = 31;
    private const int MagicRequireStr = 32;
    private const int MagicRequireDex = 33;
    private const int MagicRequireVit = 34;
    private const int MagicRequireEng = 35;
    private const int MagicRequireLevel = 36;
    private const int MagicRequireSeries = 37;
    private const int MagicRequireSex = 38;
    private const int MagicRequireFaction = 39;
    private const int EquipMaskDetail = 11;
    private const string SeparatorText = "-----------------------------------";
    private const string MagicAttribLevelPath = "\\update10\\settings\\item\\004\\magicattriblevel_index.txt";
    private const string GoldMagicPath = "\\update10\\settings\\item\\004\\GoldMagic.txt";
    private static Dictionary<int, MagicAttribLevelInfo> magicAttribLevelByMagicId;
    private static Dictionary<int, GoldMagicInfo> goldMagicByRow;
    private static readonly Dictionary<string, string> descriptionCache = new Dictionary<string, string>();
    private static Font runtimeFont;
    private static readonly string[] EquipPartNames =
    {
        "Vũ khí",
        "Ám khí",
        "Áo",
        "Nhẫn",
        "Dây chuyền",
        "Giày",
        "Thắt lưng",
        "Nón",
        "Bao tay",
        "Ngọc bội",
        "Ngựa",
        "Mặt nạ",
        "Phi phong",
        "Ấn giám",
        "Trang sức"
    };

    public static void Show(game.resource.settings.Item item, ItemData itemData, int cellIndex, RectTransform anchor)
    {
        if (item == null)
        {
            return;
        }

        Close();

        Transform parent = ResolveParent();
        if (parent == null)
        {
            Debug.LogWarning("ItemDetailPopup cannot find canvas parent.");
            return;
        }

        current = new GameObject("ItemDetailPopup", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
        current.transform.SetParent(parent, false);

        RectTransform panelRect = current.GetComponent<RectTransform>();
        panelRect.anchorMin = new Vector2(0.5f, 0.5f);
        panelRect.anchorMax = new Vector2(0.5f, 0.5f);
        panelRect.pivot = new Vector2(0.5f, 0.5f);
        panelRect.sizeDelta = new Vector2(460f, 560f);
        panelRect.anchoredPosition = GetPanelPosition(parent as RectTransform, anchor);

        Image background = current.GetComponent<Image>();
        background.color = new Color(0.05f, 0.045f, 0.04f, 0.94f);

        VerticalLayoutGroup layout = current.AddComponent<VerticalLayoutGroup>();
        layout.padding = new RectOffset(14, 14, 12, 12);
        layout.spacing = 8f;
        layout.childControlWidth = true;
        layout.childControlHeight = false;
        layout.childForceExpandWidth = true;
        layout.childForceExpandHeight = false;

        ContentSizeFitter fitter = current.AddComponent<ContentSizeFitter>();
        fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

        ClassicItemSync itemDetail = itemData != null
            ? PhotonManager.Instance?.GetPlayerItemDetail(itemData.id)
            : null;

        AddHeader(item, itemData);
        string description = BuildDescriptionCached(item, itemData, itemDetail);
        AddScrollText(description);
        AddButtons(item, itemData, cellIndex);

        current.transform.SetAsLastSibling();
        Debug.Log("ItemDetailPopup open item id=" + item.GetDatabaseId() +
                  " rawName=" + (item.GetName() ?? "<unknown>") +
                  " title=" + BuildTitle(item, itemData) +
                  " descLines=" + CountLines(description) +
                  " detailInfoLen=" + (itemDetail != null && itemDetail.ItemInfo != null ? itemDetail.ItemInfo.Length : 0) +
                  " magicSlots=" + (itemDetail != null && itemDetail.MagicAttribs != null ? itemDetail.MagicAttribs.Count : 0));
    }

    public static void Close()
    {
        if (current != null)
        {
            Object.Destroy(current);
            current = null;
        }
    }

    private static Transform ResolveParent()
    {
        if (PopUpCanvas.instance != null)
        {
            return PopUpCanvas.instance.transform;
        }

        Canvas canvas = Object.FindObjectOfType<Canvas>();
        return canvas != null ? canvas.transform : null;
    }

    private static Vector2 GetPanelPosition(RectTransform parent, RectTransform anchor)
    {
        if (parent == null || anchor == null)
        {
            return Vector2.zero;
        }

        Vector3 worldCenter = anchor.TransformPoint(anchor.rect.center);
        Canvas canvas = parent.GetComponentInParent<Canvas>();
        Camera camera = canvas != null && canvas.renderMode != RenderMode.ScreenSpaceOverlay
            ? canvas.worldCamera
            : null;

        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            parent,
            RectTransformUtility.WorldToScreenPoint(camera, worldCenter),
            camera,
            out Vector2 localPoint);

        float x = localPoint.x + 260f;
        float y = localPoint.y;
        Rect rect = parent.rect;
        x = Mathf.Clamp(x, rect.xMin + 240f, rect.xMax - 240f);
        y = Mathf.Clamp(y, rect.yMin + 280f, rect.yMax - 280f);
        return new Vector2(x, y);
    }

    private static void AddHeader(game.resource.settings.Item item, ItemData itemData)
    {
        GameObject header = new GameObject("Header", typeof(RectTransform));
        header.transform.SetParent(current.transform, false);

        HorizontalLayoutGroup layout = header.AddComponent<HorizontalLayoutGroup>();
        layout.spacing = 10f;
        layout.childControlWidth = false;
        layout.childControlHeight = true;
        layout.childForceExpandWidth = false;
        layout.childForceExpandHeight = false;

        Image icon = CreateImage("Icon", header.transform, new Vector2(60f, 60f));
        icon.sprite = item.GetThumbnailSprite();
        icon.preserveAspect = true;

        Text title = CreateText("Title", header.transform, 22, FontStyle.Bold, TextAnchor.MiddleLeft);
        title.text = BuildTitle(item, itemData);
        title.color = ResolveNameColor(item, itemData);
        title.horizontalOverflow = HorizontalWrapMode.Wrap;

        LayoutElement titleLayout = title.gameObject.AddComponent<LayoutElement>();
        titleLayout.preferredHeight = 60f;
        titleLayout.flexibleWidth = 1f;
    }

    private static void AddScrollText(string content)
    {
        GameObject scroll = new GameObject("Scroll", typeof(RectTransform), typeof(Image), typeof(ScrollRect));
        scroll.transform.SetParent(current.transform, false);
        scroll.GetComponent<Image>().color = new Color(0f, 0f, 0f, 0.22f);

        LayoutElement scrollLayout = scroll.AddComponent<LayoutElement>();
        scrollLayout.preferredHeight = 400f;
        scrollLayout.flexibleHeight = 1f;

        GameObject viewport = new GameObject("Viewport", typeof(RectTransform), typeof(RectMask2D));
        viewport.transform.SetParent(scroll.transform, false);
        RectTransform viewportRect = viewport.GetComponent<RectTransform>();
        viewportRect.anchorMin = Vector2.zero;
        viewportRect.anchorMax = Vector2.one;
        viewportRect.offsetMin = new Vector2(8f, 8f);
        viewportRect.offsetMax = new Vector2(-8f, -8f);

        GameObject contentObject = new GameObject("Content", typeof(RectTransform));
        contentObject.transform.SetParent(viewport.transform, false);
        RectTransform contentRect = contentObject.GetComponent<RectTransform>();
        contentRect.anchorMin = new Vector2(0f, 1f);
        contentRect.anchorMax = new Vector2(1f, 1f);
        contentRect.pivot = new Vector2(0f, 1f);
        contentRect.offsetMin = Vector2.zero;
        contentRect.offsetMax = Vector2.zero;

        VerticalLayoutGroup contentLayout = contentObject.AddComponent<VerticalLayoutGroup>();
        contentLayout.childControlWidth = true;
        contentLayout.childControlHeight = true;
        contentLayout.childForceExpandWidth = true;
        contentLayout.childForceExpandHeight = false;

        Text body = CreateText("Body", contentObject.transform, 18, FontStyle.Normal, TextAnchor.UpperLeft);
        body.text = content;
        body.color = new Color(0.92f, 0.88f, 0.78f, 1f);
        body.raycastTarget = false;
        body.horizontalOverflow = HorizontalWrapMode.Wrap;
        body.verticalOverflow = VerticalWrapMode.Overflow;

        RectTransform bodyRect = body.GetComponent<RectTransform>();
        bodyRect.anchorMin = new Vector2(0f, 1f);
        bodyRect.anchorMax = new Vector2(1f, 1f);
        bodyRect.pivot = new Vector2(0f, 1f);
        bodyRect.offsetMin = Vector2.zero;
        bodyRect.offsetMax = Vector2.zero;

        float bodyWidth = 416f;
        bodyRect.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, bodyWidth);
        bodyRect.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, Mathf.Max(24f, body.preferredHeight + 4f));

        LayoutElement bodyLayout = body.gameObject.AddComponent<LayoutElement>();
        bodyLayout.preferredHeight = Mathf.Max(24f, body.preferredHeight + 4f);

        ContentSizeFitter contentFitter = contentObject.AddComponent<ContentSizeFitter>();
        contentFitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

        ScrollRect scrollRect = scroll.GetComponent<ScrollRect>();
        scrollRect.viewport = viewportRect;
        scrollRect.content = contentRect;
        scrollRect.horizontal = false;
        scrollRect.vertical = true;
        scrollRect.movementType = ScrollRect.MovementType.Clamped;

        LayoutRebuilder.ForceRebuildLayoutImmediate(contentRect);
    }

    private static void AddButtons(game.resource.settings.Item item, ItemData itemData, int cellIndex)
    {
        GameObject row = new GameObject("Buttons", typeof(RectTransform));
        row.transform.SetParent(current.transform, false);

        HorizontalLayoutGroup layout = row.AddComponent<HorizontalLayoutGroup>();
        layout.spacing = 8f;
        layout.childControlWidth = true;
        layout.childControlHeight = true;
        layout.childForceExpandWidth = true;
        layout.childForceExpandHeight = false;

        LayoutElement rowLayout = row.AddComponent<LayoutElement>();
        rowLayout.preferredHeight = 44f;

        if (item.IsEquipment())
        {
            AddButton(row.transform, "Dung", () =>
            {
                PlayerMain.instance?.RequestEquipItemFromBag(item, itemData, cellIndex);
                Close();
            });
        }

        AddButton(row.transform, "Dong", Close);
    }

    private static string BuildDescription(game.resource.settings.Item item, ItemData itemData, ClassicItemSync itemDetail)
    {
        StringBuilder builder = new StringBuilder();

        AppendTradePrice(builder, item, itemDetail);
        AppendBindState(builder, itemData, itemDetail);
        AppendUseMap(builder, item, itemDetail);
        AppendLockState(builder, itemDetail);
        AppendEnchantInfo(builder, item, itemData);
        AppendItemInfo(builder, item, itemDetail);
        AppendStackInfo(builder, item, itemData);
        AppendSimpleTradeState(builder, item);
        AppendTaskSpecialInfo(builder, item, itemData);
        AppendSeries(builder, item);
        AppendMineQuality(builder, item);
        AppendMineGemAttributes(builder, item);
        AppendFusionAttributes(builder, item);
        AppendMineLevel(builder, item);
        AppendSeparator(builder);
        AppendBasicAttributes(builder, item, itemData);
        AppendRequiredAttributes(builder, item);
        AppendMagicAttributes(builder, item, itemData, itemDetail);
        AppendHiddenGoldAttributes(builder, item, itemData);
        AppendRongMagicAttributes(builder, item, itemData, itemDetail);
        AppendGoldForgeInfo(builder, item, itemData, itemDetail);
        AppendSupportSeries(builder, item, itemData);
        AppendExpireTime(builder, item, itemData);
        AppendGoldSet(builder, item);

        return NormalizeRichMarkup(builder.ToString()).Trim();
    }

    private static string BuildDescriptionCached(game.resource.settings.Item item, ItemData itemData, ClassicItemSync itemDetail)
    {
        string cacheKey = BuildDescriptionCacheKey(item, itemData, itemDetail);
        if (descriptionCache.TryGetValue(cacheKey, out string cachedDescription))
        {
            return cachedDescription;
        }

        string description = BuildDescription(item, itemData, itemDetail);
        descriptionCache[cacheKey] = description;
        return description;
    }

    private static string BuildDescriptionCacheKey(game.resource.settings.Item item, ItemData itemData, ClassicItemSync itemDetail)
    {
        StringBuilder key = new StringBuilder(256);
        key.Append(item.GetDatabaseId())
            .Append('|').Append(item.GetItemType())
            .Append('|').Append(item.GetGDPLS());

        if (itemData != null)
        {
            key.Append('|').Append(itemData.id)
                .Append('|').Append(itemData.Stack)
                .Append('|').Append(itemData.Durability)
                .Append('|').Append(itemData.Enchance)
                .Append('|').Append(itemData.Point)
                .Append('|').Append(itemData.RongPoint)
                .Append('|').Append(itemData.RandSeed)
                .Append('|').Append(itemData.IsWhere)
                .Append('|').Append(itemData.Year)
                .Append('|').Append(itemData.Month)
                .Append('|').Append(itemData.Day)
                .Append('|').Append(itemData.Hour)
                .Append('|').Append(itemData.Min)
                .Append('|').Append(itemData.Local)
                .Append('|').Append(itemData.X)
                .Append('|').Append(itemData.Y);
        }

        if (itemDetail != null)
        {
            key.Append('|').Append(itemDetail.IsBangRaw)
                .Append('|').Append(itemDetail.UseMap)
                .Append('|').Append(itemDetail.UseKind)
                .Append('|').Append(itemDetail.LockState)
                .Append('|').Append(itemDetail.LockTime)
                .Append('|').Append(itemDetail.TradePrice)
                .Append('|').Append(itemDetail.IsWhere);

            AppendMagicCacheKey(key, itemDetail.MagicAttribs);
            AppendIntArrayCacheKey(key, itemDetail.RongMagicLevels);
            AppendIntArrayCacheKey(key, itemDetail.JbLevels);
        }

        AppendGoldSetCacheKey(key, item);
        return key.ToString();
    }

    private static void AppendMagicCacheKey(StringBuilder key, List<KMagicAttrib> magicAttribs)
    {
        if (magicAttribs == null)
        {
            return;
        }

        key.Append("|m");
        for (int index = 0; index < magicAttribs.Count; index++)
        {
            KMagicAttrib attrib = magicAttribs[index];
            key.Append(':').Append(attrib.nAttribType);
            if (attrib.nValue == null)
            {
                key.Append(",0,0,0");
                continue;
            }

            key.Append(',').Append(attrib.nValue.Length > 0 ? attrib.nValue[0] : 0)
                .Append(',').Append(attrib.nValue.Length > 1 ? attrib.nValue[1] : 0)
                .Append(',').Append(attrib.nValue.Length > 2 ? attrib.nValue[2] : 0);
        }
    }

    private static void AppendIntArrayCacheKey(StringBuilder key, int[] values)
    {
        if (values == null)
        {
            return;
        }

        key.Append("|i");
        for (int index = 0; index < values.Length; index++)
        {
            key.Append(':').Append(values[index]);
        }
    }

    private static string BuildTitle(game.resource.settings.Item item, ItemData itemData)
    {
        string name = NormalizeDisplayText(item.GetName());
        if (string.IsNullOrEmpty(name))
        {
            name = "Item #" + item.GetDatabaseId();
        }

        if (item.IsEquipment())
        {
            name += " [cấp " + ResolveDisplayLevel(item.GetLevel()) + "]";
            int enchant = itemData != null ? itemData.Enchance : 0;
            if (enchant > 0)
            {
                name += " +" + enchant;
            }
        }
        else if (item.GetGenre() == (int)Defination.Genre.item_mine && item.GetParticular() >= 200 && item.GetParticular() <= 205)
        {
            name += " [cấp " + item.GetLevel() + "]";
        }

        return name;
    }

    private static int CountLines(string value)
    {
        if (string.IsNullOrEmpty(value))
        {
            return 0;
        }

        int count = 1;
        for (int index = 0; index < value.Length; index++)
        {
            if (value[index] == '\n')
            {
                count++;
            }
        }

        return count;
    }

    private static int ResolveDisplayLevel(int level)
    {
        if (level <= 10)
        {
            return level;
        }

        if (level < 100)
        {
            return level % 10 == 0 ? level / (level / 10) : level % 10;
        }

        if (level < 1000)
        {
            return level % 100 == 0 ? level / (level / 100) : level % 100;
        }

        return level;
    }

    private static void AppendTradePrice(StringBuilder builder, game.resource.settings.Item item, ClassicItemSync itemDetail)
    {
        if (itemDetail != null && itemDetail.TradePrice > 0)
        {
            builder.AppendLine(ColorText("Giá niêm yết: " + FormatMoney(itemDetail.TradePrice), "#ffffff"));
            return;
        }

        int price = item.GetPrice();
        if (price > 0)
        {
            builder.AppendLine(ColorText("Giá bán: " + FormatMoney(price), "#ffffff"));
        }
    }

    private static void AppendBindState(StringBuilder builder, ItemData itemData, ClassicItemSync itemDetail)
    {
        int isBang = itemDetail != null ? itemDetail.IsBangRaw : (itemData != null && itemData.IsBang ? 2 : 0);
        if (isBang > 1)
        {
            builder.AppendLine(ColorText("Đã khóa", "#00ff00"));
        }
    }

    private static void AppendUseMap(StringBuilder builder, game.resource.settings.Item item, ClassicItemSync itemDetail)
    {
        if (itemDetail == null || itemDetail.UseMap <= 0)
        {
            return;
        }

        string prefix = item.IsEquipment() ? "Khóa hồn bản đồ: " : "Chỉ dùng tại bản đồ: ";
        builder.AppendLine(prefix + ResolveMapName(itemDetail.UseMap, itemDetail.UseKind));
    }

    private static string ResolveMapName(int mapId, int useKind)
    {
        if (useKind != 0)
        {
            return "Phái " + mapId + " cấp độ";
        }

        try
        {
            game.resource.settings.MapList.MapInfo info = game.resource.settings.MapList.LoadMapInfo(mapId);
            if (info.id != 0 && !string.IsNullOrWhiteSpace(info.name))
            {
                return NormalizeDisplayText(info.name);
            }
        }
        catch
        {
            // The item tooltip should still render even if maplist is unavailable during early startup.
        }

        return "Bản đồ " + mapId;
    }

    private static void AppendLockState(StringBuilder builder, ClassicItemSync itemDetail)
    {
        if (itemDetail == null || itemDetail.LockState == 0)
        {
            return;
        }

        string line = itemDetail.LockState switch
        {
            1 => "Vật phẩm đính kèm theo nhân vật",
            2 => "Vật phẩm này đã khóa bảo hiểm vĩnh viễn",
            3 => "Vật phẩm này đã khóa bảo hiểm",
            4 => "Thời gian mở khóa: " + FormatPackedTime(itemDetail.LockTime),
            _ => "Trạng thái khóa: " + itemDetail.LockState
        };

        if (!string.IsNullOrWhiteSpace(line))
        {
            builder.AppendLine(ColorText(line, "#00ff00"));
        }
    }

    private static void AppendItemInfo(StringBuilder builder, game.resource.settings.Item item, ClassicItemSync itemDetail)
    {
        string info = ResolveItemInfo(item, itemDetail);

        if (!string.IsNullOrWhiteSpace(info) && item.IsShowValue())
        {
            info = FormatCStyleInteger(info, item.GetSeries());
        }

        info = NormalizeItemInfoText(info);
        if (!string.IsNullOrWhiteSpace(info))
        {
            builder.AppendLine(ColorText(info, "#ffffff"));
        }
    }

    private static string ResolveItemInfo(game.resource.settings.Item item, ClassicItemSync itemDetail)
    {
        string localInfo = item.GetIntro();
        if (IsUsableItemInfo(localInfo))
        {
            return localInfo;
        }

        string syncInfo = itemDetail != null ? itemDetail.ItemInfo : null;
        return IsUsableItemInfo(syncInfo) ? syncInfo : string.Empty;
    }

    private static bool IsUsableItemInfo(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return false;
        }

        string trimmed = value.Trim();
        return !string.Equals(trimmed, "null data", System.StringComparison.OrdinalIgnoreCase)
               && !string.Equals(trimmed, "暂无数据", System.StringComparison.OrdinalIgnoreCase)
               && !trimmed.EndsWith(".lua", System.StringComparison.OrdinalIgnoreCase);
    }

    private static void AppendEnchantInfo(StringBuilder builder, game.resource.settings.Item item, ItemData itemData)
    {
        if (!item.IsEquipment() || itemData == null || itemData.Enchance <= 0)
        {
            return;
        }

        builder.AppendLine(ColorText("Cường hóa: +" + itemData.Enchance, "#00bfff"));
    }

    private static void AppendStackInfo(StringBuilder builder, game.resource.settings.Item item, ItemData itemData)
    {
        int stack = itemData != null && itemData.Stack > 0 ? itemData.Stack : item.GetStackCurrently();
        if (stack <= 1)
        {
            return;
        }

        builder.AppendLine(ColorText("Số lượng: " + stack, "#ffffff"));
    }

    private static void AppendSimpleTradeState(StringBuilder builder, game.resource.settings.Item item)
    {
        if (item.IsEquipment())
        {
            return;
        }

        SimpleItemBase simple = item.GetSimpleItemBase();
        if (simple == null)
        {
            return;
        }

        bool canTrade = simple.isTrade > 0 || simple.isSell > 0;
        builder.AppendLine(ColorText(canTrade ? "Có thể giao dịch" : "Không thể giao dịch", canTrade ? "#00ff00" : "#ff0000"));
    }

    private static void AppendTaskSpecialInfo(StringBuilder builder, game.resource.settings.Item item, ItemData itemData)
    {
        if (item.GetGenre() != (int)Defination.Genre.item_task)
        {
            return;
        }

        int level = itemData != null && itemData.Level > 0 ? itemData.Level : item.GetLevel();
        if (item.GetDetail() == 371)
        {
            int count = Mathf.Clamp(level, 1, 6);
            builder.AppendLine(ColorText("Có thể luyện: " + count + " thuộc tính", "#00bfff"));
        }
        else if (item.GetDetail() == 374 || item.GetDetail() == 375)
        {
            builder.AppendLine(ColorText("Phẩm chất: " + level + " phẩm", "#00bfff"));
        }

        string name = NormalizeDisplayText(item.GetName());
        if (name.IndexOf("Huyền", System.StringComparison.OrdinalIgnoreCase) >= 0
            && name.IndexOf("hỏa", System.StringComparison.OrdinalIgnoreCase) >= 0
            && name.IndexOf("than", System.StringComparison.OrdinalIgnoreCase) >= 0)
        {
            builder.AppendLine(ColorText("Có thể tinh luyện tất cả văn cương", "#00bfff"));
        }
    }

    private static string FormatCStyleInteger(string value, int number)
    {
        if (string.IsNullOrEmpty(value))
        {
            return string.Empty;
        }

        string result = new Regex("%[0-9.\\-+ #]*[diu]").Replace(value, number.ToString(), 1);
        return result.Replace("%%", "%");
    }

    private static void AppendSeries(StringBuilder builder, game.resource.settings.Item item)
    {
        if (!ShouldShowSeries(item))
        {
            return;
        }

        string seriesName = item.GetSeries() switch
        {
            (int)Defination.Series.metal => "Kim",
            (int)Defination.Series.wood => "Mộc",
            (int)Defination.Series.water => "Thủy",
            (int)Defination.Series.fire => "Hỏa",
            (int)Defination.Series.earth => "Thổ",
            _ => string.Empty
        };

        string color = ResolveSeriesColor(item.GetSeries());

        if (!string.IsNullOrEmpty(seriesName))
        {
            builder.AppendLine("Thuộc tính ngũ hành: " + ColorText(seriesName, color));
        }
    }

    private static bool ShouldShowSeries(game.resource.settings.Item item)
    {
        int particular = item.GetParticular();
        return item.IsEquipment()
               || (item.GetGenre() == (int)Defination.Genre.item_mine
                   && (particular == 131 || particular == 132 || particular == 150 || particular == 152
                       || particular == 154 || (particular >= 200 && particular <= 205)));
    }

    private static void AppendMineQuality(StringBuilder builder, game.resource.settings.Item item)
    {
        if (item.GetGenre() != (int)Defination.Genre.item_mine)
        {
            return;
        }

        int particular = item.GetParticular();
        if (particular == 162 || particular == 163)
        {
            builder.AppendLine(ColorText("Còn dư: " + item.GetSeries(), "#ffff00"));
        }
        else if (particular == 147 || (particular >= 200 && particular <= 205))
        {
            builder.AppendLine(ColorText("Phẩm chất thuộc tính: " + item.GetLevel(), "#ffff00"));
        }
    }

    private static void AppendMineGemAttributes(StringBuilder builder, game.resource.settings.Item item)
    {
        if (item.GetGenre() != (int)Defination.Genre.item_mine)
        {
            return;
        }

        int particular = item.GetParticular();
        if (particular < 200 || particular > 205)
        {
            return;
        }

        int magicId = ResolveSimpleMagicId(item);
        if (magicId <= 0)
        {
            return;
        }

        MagicAttribLevelInfo info = GetMagicAttribLevelInfo(magicId);
        if (info == null)
        {
            return;
        }

        if (!string.IsNullOrWhiteSpace(info.Desc))
        {
            builder.AppendLine(ColorText("Thuộc tính khảm: ", "#ffff00") + ColorText(info.Desc, "#00ffff"));
        }

        if (!string.IsNullOrWhiteSpace(info.FitEquip))
        {
            builder.AppendLine(ColorText("Vị trí khảm: ", "#ffff00") + ColorText(info.FitEquip, "#00ffff"));
        }
    }

    private static void AppendFusionAttributes(StringBuilder builder, game.resource.settings.Item item)
    {
        if (item.GetGenre() != (int)Defination.Genre.item_fusion)
        {
            return;
        }

        SimpleItemBase simple = item.GetSimpleItemBase();
        if (simple == null)
        {
            return;
        }

        GoldMagicInfo info = GetGoldMagicInfo(simple.magicIndex);
        if (info != null && !string.IsNullOrWhiteSpace(info.Name))
        {
            builder.AppendLine(ColorText("Văn cương thuộc tính: ", "#ffff00") + ColorText(info.Name, "#00ffff"));
            int increase = info.Value1Min;
            string suffix = IsExclusiveFusion(item) ? "%" : " điểm";
            builder.AppendLine(ColorText("Phẩm chất: " + simple.qualityPin + " phẩm (tăng: " + increase + suffix + ")", "#00bfff"));
        }

        List<string> equipParts = ResolveFusionEquipParts(simple);
        foreach (string equipPart in equipParts)
        {
            builder.AppendLine(ColorText("Có thể văn cương bộ phận: " + equipPart, "#ffffff"));
        }
    }

    private static void AppendMineLevel(StringBuilder builder, game.resource.settings.Item item)
    {
        if (item.GetGenre() != (int)Defination.Genre.item_mine)
        {
            return;
        }

        int particular = item.GetParticular();
        if (particular == 147 || (particular >= 149 && particular <= 154) || (particular >= 200 && particular <= 205))
        {
            builder.AppendLine(ColorText("Level: " + item.GetLevel(), "#00bfff"));
        }
    }

    private static void AppendBasicAttributes(StringBuilder builder, game.resource.settings.Item item, ItemData itemData)
    {
        List<game.resource.settings.skill.SkillSettingData.KMagicAttrib> basicList = item.GetBasicAttribs();
        if (basicList == null)
        {
            return;
        }

        foreach (game.resource.settings.skill.SkillSettingData.KMagicAttrib magicEntry in basicList)
        {
            if (magicEntry == null || magicEntry.nAttribType <= 0)
            {
                continue;
            }

            if (magicEntry.nAttribType == MagicDurability)
            {
                string durability = BuildDurabilityLine(item, itemData, magicEntry);
                if (!string.IsNullOrWhiteSpace(durability))
                {
                    builder.AppendLine(durability);
                }

                continue;
            }

            string desc = NormalizeDisplayText(game.resource.settings.MagicDesc.Get(magicEntry));
            if (!ShouldAppendMagicDesc(desc))
            {
                continue;
            }

            builder.AppendLine(desc);
        }
    }

    private static string BuildDurabilityLine(game.resource.settings.Item item, ItemData itemData, game.resource.settings.skill.SkillSettingData.KMagicAttrib magicEntry)
    {
        int current = itemData != null ? itemData.Durability : 0;
        int max = 0;
        if (magicEntry.nValue != null)
        {
            for (int index = 0; index < magicEntry.nValue.Length; index++)
            {
                max = System.Math.Max(max, magicEntry.nValue[index]);
            }
        }

        if (current <= 0)
        {
            if (max <= 0)
            {
                return string.Empty;
            }

            current = max;
        }

        if (current <= 1)
        {
            return "Trang bị tổn hại";
        }

        string label = current <= 5 && item.GetDetail() != EquipMaskDetail
            ? "Độ bền (Yêu cầu sửa chữa):"
            : "Độ bền:";

        return max > 0
            ? label + current + "/" + max
            : label + current;
    }

    private static void AppendRequiredAttributes(StringBuilder builder, game.resource.settings.Item item)
    {
        List<game.resource.settings.skill.SkillSettingData.KMagicAttrib> requirements = item.GetRequiredAttribs();
        if (requirements == null)
        {
            return;
        }

        bool hasRequirement = false;
        foreach (game.resource.settings.skill.SkillSettingData.KMagicAttrib requirement in requirements)
        {
            if (requirement != null && requirement.nAttribType > 0)
            {
                hasRequirement = true;
                break;
            }
        }

        if (!hasRequirement)
        {
            return;
        }

        AppendSeparator(builder);
        foreach (game.resource.settings.skill.SkillSettingData.KMagicAttrib requirement in requirements)
        {
            if (requirement == null || requirement.nAttribType <= 0)
            {
                continue;
            }

            string desc = NormalizeDisplayText(game.resource.settings.MagicDesc.Get(requirement));
            if (!ShouldAppendMagicDesc(desc))
            {
                continue;
            }

            builder.AppendLine(ColorText(desc, IsRequirementMet(requirement) ? "#ffffff" : "#ff0000"));
        }
    }

    private static bool IsRequirementMet(game.resource.settings.skill.SkillSettingData.KMagicAttrib requirement)
    {
        if (PhotonManager.Instance == null || PhotonManager.Instance.character == null || requirement.nValue == null)
        {
            return true;
        }

        int value = requirement.nValue.Length > 0 ? requirement.nValue[0] : 0;
        CharacterData character = PhotonManager.Instance.character;
        return requirement.nAttribType switch
        {
            MagicRequireStr => character.Power >= value,
            MagicRequireDex => character.Agility >= value,
            MagicRequireVit => character.Outer >= value,
            MagicRequireEng => character.Inside >= value,
            MagicRequireLevel => character.FightLevel >= value,
            MagicRequireSeries => character.Fiveprop == value,
            MagicRequireSex => character.Sex == (value != 0),
            MagicRequireFaction => character.Sect == value || character.Sect + 1 == value,
            _ => true
        };
    }

    private static void AppendMagicAttributes(StringBuilder builder, game.resource.settings.Item item, ItemData itemData, ClassicItemSync itemDetail)
    {
        if (!item.IsEquipment())
        {
            return;
        }

        List<game.resource.settings.skill.SkillSettingData.KMagicAttrib> magicSlots = ResolveMagicSlots(item, itemDetail);
        if (magicSlots == null || magicSlots.Count == 0)
        {
            return;
        }

        bool hasMagic = false;
        for (int index = 0; index < magicSlots.Count && index < 6; index++)
        {
            if (magicSlots[index] != null && magicSlots[index].nAttribType > 0)
            {
                hasMagic = true;
                break;
            }
        }

        int point = itemData != null ? itemData.Point : 0;
        if (!hasMagic && point <= 0)
        {
            return;
        }

        AppendSeparator(builder);
        string itemColor = ResolveItemColorHex(item, itemData);
        for (int index = 0; index < 6; index++)
        {
            game.resource.settings.skill.SkillSettingData.KMagicAttrib magicEntry =
                index < magicSlots.Count ? magicSlots[index] : null;

            if (magicEntry == null || magicEntry.nAttribType <= 0)
            {
                if (IsPurpleItem(item, itemData) && index < point)
                {
                    builder.AppendLine(ColorText("Chưa khảm nạn", "#ffff00"));
                }

                continue;
            }

            string desc = NormalizeDisplayText(game.resource.settings.MagicDesc.Get(magicEntry));
            if (!ShouldAppendMagicDesc(desc))
            {
                continue;
            }

            builder.AppendLine(ColorText(desc, itemColor));
        }
    }

    private static void AppendHiddenGoldAttributes(StringBuilder builder, game.resource.settings.Item item, ItemData itemData)
    {
        if (item.GetItemType() != Defination.Type.goldEquip)
        {
            return;
        }

        GoldEquipBase goldBase = item.GetEquipmentBase() as GoldEquipBase;
        if (goldBase == null)
        {
            return;
        }

        int[] rows = { goldBase.yinMagicAttribs0, goldBase.yinMagicAttribs1 };
        bool appendedSeparator = false;
        for (int index = 0; index < rows.Length; index++)
        {
            int row = rows[index];
            if (row <= 0)
            {
                continue;
            }

            game.resource.settings.skill.SkillSettingData.KMagicAttrib attrib =
                BuildGoldMagicAttrib(row, itemData != null ? itemData.RandSeed : 0, index + 20);
            string desc = NormalizeDisplayText(game.resource.settings.MagicDesc.Get(attrib));
            if (!ShouldAppendMagicDesc(desc))
            {
                continue;
            }

            if (!appendedSeparator)
            {
                AppendSeparator(builder);
                appendedSeparator = true;
            }

            bool active = item.GetLevel() >= (index == 0 ? 5 : 10);
            builder.AppendLine(ColorText(desc, active ? "#ffd33d" : "#808080"));
        }
    }

    private static void AppendRongMagicAttributes(
        StringBuilder builder,
        game.resource.settings.Item item,
        ItemData itemData,
        ClassicItemSync itemDetail)
    {
        if (!item.IsEquipment() || itemData == null || itemData.RongPoint <= 0)
        {
            return;
        }

        int[] rows = ResolveRongMagicRows(itemData, itemDetail);

        bool appendedSeparator = false;
        AxmolRandom random = new AxmolRandom(itemData.RandSeed == 0 ? 42u : itemData.RandSeed);
        for (int index = 0; index < rows.Length && index < itemData.RongPoint; index++)
        {
            int row = rows[index];
            if (!appendedSeparator)
            {
                AppendSeparator(builder);
                appendedSeparator = true;
            }

            if (row <= 0)
            {
                builder.AppendLine(ColorText("[Chưa văn cương]", "#ffffff"));
                continue;
            }

            game.resource.settings.skill.SkillSettingData.KMagicAttrib attrib =
                BuildGoldMagicAttrib(row, ref random);
            string desc = NormalizeDisplayText(game.resource.settings.MagicDesc.Get(attrib));
            if (ShouldAppendMagicDesc(desc))
            {
                builder.AppendLine(ColorText(desc, "#ffffff"));
            }
        }

        if (itemData.IsWhere == -1)
        {
            builder.AppendLine(ColorText("Trang bị có thể tẩy luyện ra thuộc tính đặc biệt ngẫu nhiên", "#ffff00"));
        }
        else if (itemData.IsWhere == -2)
        {
            builder.AppendLine(ColorText("Thuộc tính tẩy luyện của trang bị không hiển thị", "#ffff00"));
        }
    }

    private static void AppendGoldForgeInfo(
        StringBuilder builder,
        game.resource.settings.Item item,
        ItemData itemData,
        ClassicItemSync itemDetail)
    {
        GoldEquipBase goldBase = item.GetEquipmentBase() as GoldEquipBase;
        if (goldBase == null || goldBase.rongNum <= 0)
        {
            return;
        }

        int current = 0;
        if (itemData != null)
        {
            int[] rows = ResolveRongMagicRows(itemData, itemDetail);

            for (int index = 0; index < rows.Length; index++)
            {
                if (rows[index] > 0)
                {
                    current++;
                }
            }
        }

        builder.AppendLine(ColorText("Đã luyện văn cương: " + current + "/" + goldBase.rongNum + " dòng", "#00ff00"));
        if (goldBase.wengangPin > 0)
        {
            builder.AppendLine(ColorText("Cấp văn cương tối đa: " + goldBase.wengangPin, "#00ff00"));
        }

        if (goldBase.binfujiazhi > 0)
        {
            builder.AppendLine(ColorText("Binh giáp cơ bản: " + goldBase.binfujiazhi, "#00ff00"));
        }
    }

    private static int[] ResolveRongMagicRows(ItemData itemData, ClassicItemSync itemDetail)
    {
        if (itemDetail != null && itemDetail.RongMagicLevels != null && itemDetail.RongMagicLevels.Length > 0)
        {
            return itemDetail.RongMagicLevels;
        }

        if (itemData == null)
        {
            return System.Array.Empty<int>();
        }

        return new[]
        {
            (int)itemData.Paramr1,
            (int)itemData.Paramr2,
            (int)itemData.Paramr3,
            (int)itemData.Paramr4,
            (int)itemData.Paramr5,
            (int)itemData.Paramr6
        };
    }

    private static List<game.resource.settings.skill.SkillSettingData.KMagicAttrib> ResolveMagicSlots(game.resource.settings.Item item, ClassicItemSync itemDetail)
    {
        if (itemDetail != null && itemDetail.MagicAttribs != null && itemDetail.MagicAttribs.Count > 0)
        {
            List<game.resource.settings.skill.SkillSettingData.KMagicAttrib> slots =
                new List<game.resource.settings.skill.SkillSettingData.KMagicAttrib>(itemDetail.MagicAttribs.Count);

            foreach (KMagicAttrib source in itemDetail.MagicAttribs)
            {
                int value0 = source.nValue != null && source.nValue.Length > 0 ? source.nValue[0] : 0;
                int value1 = source.nValue != null && source.nValue.Length > 1 ? source.nValue[1] : 0;
                int value2 = source.nValue != null && source.nValue.Length > 2 ? source.nValue[2] : 0;
                slots.Add(new game.resource.settings.skill.SkillSettingData.KMagicAttrib(
                    source.nAttribType,
                    value0,
                    value1,
                    value2));
            }

            return slots;
        }

        return item.GetMagicAttribs();
    }

    private static void AppendSupportSeries(StringBuilder builder, game.resource.settings.Item item, ItemData itemData)
    {
        if (!item.IsEquipment() || item.GetMagicAttribs() == null || item.GetMagicAttribs().Count < 2)
        {
            return;
        }

        if (itemData == null || itemData.Local != (byte)ItemPosition.pos_equip)
        {
            return;
        }

        string supportLine = ResolveSupportSeriesLine(item.GetSeries(), itemData.X);
        if (string.IsNullOrWhiteSpace(supportLine))
        {
            return;
        }

        AppendSeparator(builder);
        builder.AppendLine(supportLine);
    }

    private static string ResolveSupportSeriesLine(int series, int equipPart)
    {
        string supportSeries = series switch
        {
            (int)Defination.Series.metal => ColorText("(Thổ)", ResolveSeriesColor((int)Defination.Series.earth)),
            (int)Defination.Series.wood => ColorText("(Thủy)", ResolveSeriesColor((int)Defination.Series.water)),
            (int)Defination.Series.water => ColorText("(Kim)", ResolveSeriesColor((int)Defination.Series.metal)),
            (int)Defination.Series.fire => ColorText("(Mộc)", ResolveSeriesColor((int)Defination.Series.wood)),
            (int)Defination.Series.earth => ColorText("(Hỏa)", ResolveSeriesColor((int)Defination.Series.fire)),
            _ => string.Empty
        };

        if (string.IsNullOrEmpty(supportSeries))
        {
            return string.Empty;
        }

        string target = equipPart switch
        {
            3 or 0 => "Dây chuyền và Trang phục",
            4 or 7 => "Vũ khí và Nón",
            9 or 5 => "Giày và Nhẫn trên",
            2 or 8 => "Ngọc bội và Bao tay",
            1 or 6 => "Thắt lưng và Nhẫn dưới",
            _ => string.Empty
        };

        return string.IsNullOrEmpty(target)
            ? string.Empty
            : ColorText("Cần hệ", "#00ffff") + " " + supportSeries + " " + target + " " +
              ColorText("để kích hoạt thuộc tính ẩn.", "#00ffff");
    }

    private static void AppendExpireTime(StringBuilder builder, game.resource.settings.Item item, ItemData itemData)
    {
        if (itemData != null && itemData.Year > 0 && itemData.Month > 0 && itemData.Day > 0)
        {
            builder.AppendLine(ColorText(
                string.Format("Thời hạn sử dụng: {0:00}:{1:00} {2:00}-{3:00}-{4:00}",
                    itemData.Hour, itemData.Min, itemData.Day, itemData.Month, itemData.Year),
                "#ff4500"));
            return;
        }

        if (item.GetTimeUse() > 0)
        {
            builder.AppendLine(ColorText("Thời hạn sử dụng: " + item.GetTimeExpire(), "#ff4500"));
        }
    }

    private static void AppendGoldSet(StringBuilder builder, game.resource.settings.Item item)
    {
        Defination.Type itemType = item.GetItemType();
        if (itemType != Defination.Type.goldEquip && itemType != Defination.Type.platinaEquip)
        {
            return;
        }

        GoldEquipBase goldBase = item.GetEquipmentBase() as GoldEquipBase;
        if (goldBase == null || goldBase.setNum <= 0 || goldBase.idSet <= 0 || goldBase.setId <= 0)
        {
            return;
        }

        Dictionary<int, int> setColors = ResolveGoldSetColors(goldBase.set);
        List<string> setLines = new List<string>();

        for (int k = goldBase.setNum - 1; k >= 0; k--)
        {
            if (k >= 10)
            {
                continue;
            }

            int rowIndex = goldBase.idSet + 3 - goldBase.setId + k;
            GoldEquipBase setItemBase = ResolveGoldSetBase(itemType, rowIndex);
            if (setItemBase == null || string.IsNullOrWhiteSpace(setItemBase.name))
            {
                continue;
            }

            int setId = k + 1;
            string color = ResolveGoldSetColor(setColors, setId);
            setLines.Add(ColorText(NormalizeDisplayText(setItemBase.name), color));
        }

        if (setLines.Count == 0)
        {
            return;
        }

        AppendSeparator(builder);
        foreach (string setLine in setLines)
        {
            builder.AppendLine(setLine);
        }
    }

    private static GoldEquipBase ResolveGoldSetBase(Defination.Type itemType, int rowIndex)
    {
        if (rowIndex <= 0)
        {
            return null;
        }

        return itemType == Defination.Type.platinaEquip
            ? Getters.GetPlatinaEquipBase(rowIndex)
            : Getters.GetGoldEquipBase(rowIndex);
    }

    private static Dictionary<int, int> ResolveGoldSetColors(int set)
    {
        Dictionary<int, int> result = new Dictionary<int, int>();
        if (set <= 0)
        {
            return result;
        }

        Dictionary<uint, ItemData> playerItems = PhotonManager.Instance.GetPlayerItems();
        if (playerItems == null || playerItems.Count == 0)
        {
            return result;
        }

        List<ItemData> snapshot = new List<ItemData>(playerItems.Values);
        for (int index = 0; index < snapshot.Count; index++)
        {
            ItemData playerItem = snapshot[index];
            GoldEquipBase playerGoldBase = ResolvePlayerGoldBase(playerItem);
            if (playerGoldBase == null || playerGoldBase.set != set || playerGoldBase.setId <= 0)
            {
                continue;
            }

            int colorState = playerItem.Local == (byte)ItemPosition.pos_equip ? 2 : 1;
            if (!result.TryGetValue(playerGoldBase.setId, out int currentState) || colorState > currentState)
            {
                result[playerGoldBase.setId] = colorState;
            }
        }

        return result;
    }

    private static GoldEquipBase ResolvePlayerGoldBase(ItemData itemData)
    {
        if (itemData == null ||
            itemData.Equipclasscode != (byte)Defination.Genre.item_equip ||
            itemData.IdGold <= 0)
        {
            return null;
        }

        return itemData.IsPlasma
            ? Getters.GetPlatinaEquipBase(itemData.IdGold)
            : Getters.GetGoldEquipBase(itemData.IdGold);
    }

    private static string ResolveGoldSetColor(Dictionary<int, int> setColors, int setId)
    {
        int colorState = 0;
        if (setColors != null)
        {
            setColors.TryGetValue(setId, out colorState);
        }

        switch (colorState)
        {
            case 1:
                return "#00af00";

            case 2:
                return "#00ff00";

            default:
                return "#6e6e00";
        }
    }

    private static void AppendGoldSetCacheKey(StringBuilder key, game.resource.settings.Item item)
    {
        Defination.Type itemType = item.GetItemType();
        if (itemType != Defination.Type.goldEquip && itemType != Defination.Type.platinaEquip)
        {
            return;
        }

        GoldEquipBase goldBase = item.GetEquipmentBase() as GoldEquipBase;
        if (goldBase == null || goldBase.set <= 0 || goldBase.setNum <= 0)
        {
            return;
        }

        Dictionary<int, int> setColors = ResolveGoldSetColors(goldBase.set);
        key.Append("|goldset=").Append(goldBase.set);
        for (int setId = 1; setId <= goldBase.setNum && setId <= 10; setId++)
        {
            int colorState = 0;
            if (setColors != null)
            {
                setColors.TryGetValue(setId, out colorState);
            }

            key.Append(':').Append(setId).Append('=').Append(colorState);
        }
    }

    private static string FormatMoney(int value)
    {
        if (value < 10000)
        {
            return value + " lượng";
        }

        int van = value / 10000;
        int remain = value % 10000;
        return remain == 0
            ? van + " vạn lượng"
            : van + " vạn " + remain + " lượng";
    }

    private static string FormatPackedTime(int value)
    {
        if (value <= 0)
        {
            return string.Empty;
        }

        int year = value / 1000000;
        int month = (value / 10000) % 100;
        int day = (value / 100) % 100;
        int hour = value % 100;
        if (year < 1970 || month <= 0 || month > 12 || day <= 0 || day > 31 || hour < 0 || hour > 23)
        {
            return value.ToString();
        }

        return string.Format("{0:00}:00 {1:00}-{2:00}-{3:0000}", hour, day, month, year);
    }

    private static bool ShouldAppendMagicDesc(string desc)
    {
        return !string.IsNullOrWhiteSpace(desc)
               && !desc.StartsWith("<không xác định:", System.StringComparison.OrdinalIgnoreCase);
    }

    private static void AppendSeparator(StringBuilder builder)
    {
        if (builder.Length > 0)
        {
            builder.AppendLine(ColorText(SeparatorText, "#ffffff"));
        }
    }

    private static string NormalizeRichText(string value)
    {
        value = NormalizeDisplayText(value);
        return NormalizeRichMarkup(value);
    }

    private static string NormalizeItemInfoText(string value)
    {
        string result = NormalizeRichText(value);
        if (string.IsNullOrEmpty(result) || result.IndexOf('\n') >= 0)
        {
            return result;
        }

        int closeTagCount = Regex.Matches(result, "</color>", RegexOptions.IgnoreCase).Count;
        if (closeTagCount <= 1)
        {
            return result;
        }

        result = Regex.Replace(result, "</color>(?=<color=)", "</color>\n", RegexOptions.IgnoreCase);
        result = Regex.Replace(result, "</color>(?=[^\\r\\n<])", "</color>\n", RegexOptions.IgnoreCase);
        return result;
    }

    private static string NormalizeRichMarkup(string value)
    {
        if (string.IsNullOrEmpty(value))
        {
            return string.Empty;
        }

        value = value
            .Replace("\\n", "\n")
            .Replace("<enter>", "\n")
            .Replace("<br>", "\n")
            .Replace("<br/>", "\n")
            .Replace("<br />", "\n");

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
        return value;
    }

    private static string NormalizeDisplayText(string value)
    {
        return JxClassicClient.TranslateDisplayString(value ?? string.Empty).Trim();
    }

    private static string ColorText(string value, string color)
    {
        return "<color=" + color + ">" + value + "</color>";
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
            "yellow" => "#ffff00",
            "white" => "#ffffff",
            "red" => "#ff0000",
            "green" => "#00ff00",
            "blue" => "#00bfff",
            "gray" => "#808080",
            "grey" => "#808080",
            "gold" => "#ffd700",
            "purple" => "#ff00ff",
            "cyan" => "#00ffff",
            "fire" => ResolveSeriesColor((int)Defination.Series.fire),
            "water" => ResolveSeriesColor((int)Defination.Series.water),
            "metal" => ResolveSeriesColor((int)Defination.Series.metal),
            "wood" => ResolveSeriesColor((int)Defination.Series.wood),
            "earth" => ResolveSeriesColor((int)Defination.Series.earth),
            _ => "#ffffff"
        };
    }

    private static string ResolveSeriesColor(int series)
    {
        return series switch
        {
            (int)Defination.Series.metal => "#ffd700",
            (int)Defination.Series.wood => "#32cd32",
            (int)Defination.Series.water => "#1e90ff",
            (int)Defination.Series.fire => "#ff4500",
            (int)Defination.Series.earth => "#a0522d",
            _ => "#ffffff"
        };
    }

    private static string ResolveItemColorHex(game.resource.settings.Item item, ItemData itemData)
    {
        if (item.GetItemType() == Defination.Type.goldEquip)
        {
            return "#ffd33d";
        }

        if (IsPurpleItem(item, itemData))
        {
            return "#ff00ff";
        }

        return item.HaveMagicAttrib() ? "#6464ff" : "#ffffff";
    }

    private static bool IsPurpleItem(game.resource.settings.Item item, ItemData itemData)
    {
        return item.IsEquipment() && itemData != null && itemData.Point > 0 && item.GetItemType() != Defination.Type.goldEquip;
    }

    private static int ResolveSimpleMagicId(game.resource.settings.Item item)
    {
        SimpleItemBase simple = item.GetSimpleItemBase();
        if (simple != null && simple.magicId > 0)
        {
            return simple.magicId;
        }

        List<game.resource.settings.skill.SkillSettingData.KMagicAttrib> basicList = item.GetBasicAttribs();
        if (basicList == null || basicList.Count == 0 || basicList[0] == null || basicList[0].nValue == null)
        {
            return 0;
        }

        return basicList[0].nValue.Length > 0 ? basicList[0].nValue[0] : 0;
    }

    private static MagicAttribLevelInfo GetMagicAttribLevelInfo(int magicId)
    {
        EnsureMagicAttribLevelLoaded();
        return magicAttribLevelByMagicId != null && magicAttribLevelByMagicId.TryGetValue(magicId, out MagicAttribLevelInfo info)
            ? info
            : null;
    }

    private static void EnsureMagicAttribLevelLoaded()
    {
        if (magicAttribLevelByMagicId != null)
        {
            return;
        }

        magicAttribLevelByMagicId = new Dictionary<int, MagicAttribLevelInfo>();
        try
        {
            game.resource.Table table = Game.Resource(MagicAttribLevelPath).Get<game.resource.Table>();
            if (table == null || table.IsEmpty())
            {
                return;
            }

            for (int rowIndex = 1; rowIndex < table.RowCount; rowIndex++)
            {
                int magicId = table.Get<int>("MAGIC_ID", rowIndex);
                int magicType = table.Get<int>("MAGIC_TTPE", rowIndex);
                if (magicId <= 0 || magicType != 0 || magicAttribLevelByMagicId.ContainsKey(magicId))
                {
                    continue;
                }

                magicAttribLevelByMagicId[magicId] = new MagicAttribLevelInfo
                {
                    MagicId = magicId,
                    Desc = NormalizeDisplayText(table.Get<string>("DESC", rowIndex)),
                    FitEquip = NormalizeDisplayText(table.Get<string>("FIT_EQUIP", rowIndex))
                };
            }
        }
        catch (System.Exception exception)
        {
            Debug.LogWarning("ItemDetailPopup cannot load magic attrib level table: " + exception.Message);
        }
    }

    private static GoldMagicInfo GetGoldMagicInfo(int rowIndex)
    {
        EnsureGoldMagicLoaded();
        return goldMagicByRow != null && goldMagicByRow.TryGetValue(rowIndex, out GoldMagicInfo info)
            ? info
            : null;
    }

    private static void EnsureGoldMagicLoaded()
    {
        if (goldMagicByRow != null)
        {
            return;
        }

        goldMagicByRow = new Dictionary<int, GoldMagicInfo>();
        try
        {
            game.resource.Table table = Game.Resource(GoldMagicPath).Get<game.resource.Table>();
            if (table == null || table.IsEmpty())
            {
                return;
            }

            for (int rowIndex = 1; rowIndex < table.RowCount; rowIndex++)
            {
                GoldMagicInfo info = new GoldMagicInfo
                {
                    RowIndex = rowIndex,
                    Name = NormalizeDisplayText(table.Get<string>(0, rowIndex)),
                    Type = table.Get<int>(4, rowIndex, 0),
                    Value1Min = table.Get<int>(5, rowIndex, 0),
                    Value1Max = table.Get<int>(6, rowIndex, 0),
                    Value2Min = table.Get<int>(7, rowIndex, 0),
                    Value2Max = table.Get<int>(8, rowIndex, 0),
                    Value3Min = table.Get<int>(9, rowIndex, 0),
                    Value3Max = table.Get<int>(10, rowIndex, 0),
                    Intro = NormalizeDisplayText(table.Get<string>(11, rowIndex))
                };

                goldMagicByRow[rowIndex] = info;
            }
        }
        catch (System.Exception exception)
        {
            Debug.LogWarning("ItemDetailPopup cannot load gold magic table: " + exception.Message);
        }
    }

    private static game.resource.settings.skill.SkillSettingData.KMagicAttrib BuildGoldMagicAttrib(int rowIndex, uint seed, int sequenceOffset)
    {
        GoldMagicInfo info = GetGoldMagicInfo(rowIndex);
        if (info == null)
        {
            return new game.resource.settings.skill.SkillSettingData.KMagicAttrib();
        }

        AxmolRandom random = new AxmolRandom(seed == 0 ? 42u : seed);
        for (int index = 0; index < sequenceOffset; index++)
        {
            random.GetRandomNumber(0, 1);
            random.GetRandomNumber(0, 1);
            random.GetRandomNumber(0, 1);
        }

        return new game.resource.settings.skill.SkillSettingData.KMagicAttrib(
            info.Type,
            random.GetRandomNumber(info.Value1Min, info.Value1Max),
            random.GetRandomNumber(info.Value2Min, info.Value2Max),
            random.GetRandomNumber(info.Value3Min, info.Value3Max));
    }

    private static game.resource.settings.skill.SkillSettingData.KMagicAttrib BuildGoldMagicAttrib(
        int rowIndex,
        ref AxmolRandom random)
    {
        GoldMagicInfo info = GetGoldMagicInfo(rowIndex);
        if (info == null)
        {
            return new game.resource.settings.skill.SkillSettingData.KMagicAttrib();
        }

        return new game.resource.settings.skill.SkillSettingData.KMagicAttrib(
            info.Type,
            random.GetRandomNumber(info.Value1Min, info.Value1Max),
            random.GetRandomNumber(info.Value2Min, info.Value2Max),
            random.GetRandomNumber(info.Value3Min, info.Value3Max));
    }

    private static List<string> ResolveFusionEquipParts(SimpleItemBase simple)
    {
        List<string> result = new List<string>();
        List<game.resource.settings.skill.SkillSettingData.KMagicAttrib> parts = simple.GetBasicAttrib();
        if (parts == null)
        {
            return result;
        }

        foreach (game.resource.settings.skill.SkillSettingData.KMagicAttrib part in parts)
        {
            if (part == null || part.nValue == null || part.nValue.Length == 0)
            {
                continue;
            }

            int equipPart = part.nValue[0];
            if (equipPart < 0 || equipPart >= EquipPartNames.Length)
            {
                continue;
            }

            string name = EquipPartNames[equipPart];
            if (!result.Contains(name))
            {
                result.Add(name);
            }
        }

        return result;
    }

    private static bool IsExclusiveFusion(game.resource.settings.Item item)
    {
        string name = NormalizeDisplayText(item.GetName());
        return name.IndexOf("chuyên", System.StringComparison.OrdinalIgnoreCase) >= 0
               || name.IndexOf("độc", System.StringComparison.OrdinalIgnoreCase) >= 0;
    }

    private sealed class MagicAttribLevelInfo
    {
        public int MagicId;
        public string Desc;
        public string FitEquip;
    }

    private sealed class GoldMagicInfo
    {
        public int RowIndex;
        public string Name;
        public int Type;
        public int Value1Min;
        public int Value1Max;
        public int Value2Min;
        public int Value2Max;
        public int Value3Min;
        public int Value3Max;
        public string Intro;
    }

    private struct AxmolRandom
    {
        private uint seed;

        public AxmolRandom(uint seed)
        {
            this.seed = seed;
        }

        private int Random(int max)
        {
            if (max <= 0)
            {
                return 0;
            }

            unchecked
            {
                seed = (seed * 3877u) + 29573u;
            }

            return (int)(seed % (uint)max);
        }

        public int GetRandomNumber(int min, int max)
        {
            bool bothNegative = min < 0 && max < 0;
            if (bothNegative)
            {
                min = -min;
                max = -max;
            }

            int range = max - min + 1;
            if (range <= 0)
            {
                return 0;
            }

            int value = Random(range) + min;
            return bothNegative ? -value : value;
        }
    }

    private static Image CreateImage(string name, Transform parent, Vector2 size)
    {
        GameObject go = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
        go.transform.SetParent(parent, false);
        RectTransform rect = go.GetComponent<RectTransform>();
        rect.sizeDelta = size;

        LayoutElement layout = go.AddComponent<LayoutElement>();
        layout.preferredWidth = size.x;
        layout.preferredHeight = size.y;

        return go.GetComponent<Image>();
    }

    private static Text CreateText(string name, Transform parent, int fontSize, FontStyle fontStyle, TextAnchor alignment)
    {
        GameObject go = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(Text));
        go.transform.SetParent(parent, false);

        Text text = go.GetComponent<Text>();
        text.font = ResolveRuntimeFont();
        text.fontSize = fontSize;
        text.fontStyle = fontStyle;
        text.alignment = alignment;
        text.supportRichText = true;
        text.raycastTarget = false;
        text.horizontalOverflow = HorizontalWrapMode.Wrap;
        text.verticalOverflow = VerticalWrapMode.Truncate;
        return text;
    }

    private static Font ResolveRuntimeFont()
    {
        if (runtimeFont != null)
        {
            return runtimeFont;
        }

        runtimeFont = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        if (runtimeFont != null)
        {
            return runtimeFont;
        }

        runtimeFont = Resources.GetBuiltinResource<Font>("Arial.ttf");
        if (runtimeFont != null)
        {
            return runtimeFont;
        }

        try
        {
            runtimeFont = Font.CreateDynamicFontFromOSFont(
                new[] { "Segoe UI", "Arial", "Tahoma" },
                18);
        }
        catch
        {
            runtimeFont = null;
        }

        return runtimeFont;
    }

    private static void AddButton(Transform parent, string title, System.Action action)
    {
        GameObject go = new GameObject(title, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(Button));
        go.transform.SetParent(parent, false);
        go.GetComponent<Image>().color = new Color(0.23f, 0.18f, 0.12f, 1f);

        Button button = go.GetComponent<Button>();
        button.onClick.AddListener(() => action?.Invoke());

        Text label = CreateText("Text", go.transform, 14, FontStyle.Bold, TextAnchor.MiddleCenter);
        label.text = title;
        label.color = Color.white;
        RectTransform labelRect = label.GetComponent<RectTransform>();
        labelRect.anchorMin = Vector2.zero;
        labelRect.anchorMax = Vector2.one;
        labelRect.offsetMin = Vector2.zero;
        labelRect.offsetMax = Vector2.zero;
    }

    private static Color ResolveNameColor(game.resource.settings.Item item, ItemData itemData)
    {
        if (ColorUtility.TryParseHtmlString(ResolveItemColorHex(item, itemData), out Color color))
        {
            return color;
        }

        return Color.white;
    }
}
