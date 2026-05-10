using System;
using System.Collections.Generic;
using game.network;
using game.resource.settings;
using game.resource.settings.item;
using Photon.ShareLibrary.Constant;
using Photon.ShareLibrary.Entities;
using UnityEngine;
using UnityEngine.UI;

public class Storage : MonoBehaviour
{
    // Start is called before the first frame update
    [SerializeField]
    private GameObject ListStorage;
    [SerializeField]
    private GameObject ListBag;
    [SerializeField]
    private GameObject ItemEquip;
    [SerializeField]
    private Button CloseGameObject;
    [SerializeField]
    private Text TextStorages;
    [SerializeField]
    private Text TextBags;

    private Dictionary<int, GameObject> ListBags = new();
    private Dictionary<int, GameObject> ListStorages = new();

    private void Start()
    {
        CloseGameObject.GetComponent<Button>().onClick.AddListener(() =>
        {
            gameObject.SetActive(false);
        });
    }

    public void InitStorage()
    {
        if (ListBags.Count > 0 || ListStorages.Count > 0)
        {
            return;
        }

        for (int i = 0; i < 200; i++)
        {
            GridLayoutGroup gridLayoutGroup = ListStorage.GetComponent<GridLayoutGroup>();
            GameObject StoragesChild = Instantiate(ItemEquip, Vector3.zero, Quaternion.identity);
            StoragesChild.transform.SetParent(gridLayoutGroup.transform, false);
            ListStorages.Add(i, StoragesChild);

            GridLayoutGroup gridLayoutGroup2 = ListBag.GetComponent<GridLayoutGroup>();
            GameObject BagsChild = Instantiate(ItemEquip, Vector3.zero, Quaternion.identity);
            BagsChild.transform.SetParent(gridLayoutGroup2.transform, false);
            ListBags.Add(i, BagsChild);
        }
    }

    public void SetUpPlayerItem()
    {
        List<ItemData> itemDataBags = new();
        List<ItemData> itemDataStorages = new();

        foreach (var data in PhotonManager.Instance.GetPlayerItems())
        {
            ItemData itemData = data.Value;

            if (ItemUiMapper.IsBagItem(itemData))
            {
                itemDataBags.Add(itemData);
            }
            else if (ItemUiMapper.IsStorageItem(itemData))
            {
                itemDataStorages.Add(itemData);
            }
        }

        ClearCells(ListBags);
        ClearCells(ListStorages);

        TextBags.text = itemDataBags.Count + "/" + ItemUiMapper.BagSlotCount;
        TextStorages.text = itemDataStorages.Count + "/" + ListStorages.Count;

        HashSet<int> usedBagSlots = new();
        for (int i = 0; i < itemDataBags.Count; i++)
        {
            PlaceItem(ListBags, usedBagSlots, itemDataBags[i], i);
        }

        HashSet<int> usedStorageSlots = new();
        for (int i = 0; i < itemDataStorages.Count; i++)
        {
            PlaceItem(ListStorages, usedStorageSlots, itemDataStorages[i], i);
        }

        Debug.Log("Storage SetUpPlayerItem total=" + PhotonManager.Instance.GetPlayerItems().Count +
                  " bag=" + itemDataBags.Count +
                  " storage=" + itemDataStorages.Count);
    }

    Item RestoreItemFromDatabase(ItemData itemdata)
    {
        return ItemUiMapper.RestoreItem(itemdata);
    }

    private static void ClearCells(Dictionary<int, GameObject> cells)
    {
        foreach (GameObject cell in cells.Values)
        {
            cell.GetComponent<EquimentItem>()?.ClearItem();
        }
    }

    private void PlaceItem(Dictionary<int, GameObject> cells, HashSet<int> usedSlots, ItemData itemData, int fallbackIndex)
    {
        int slot = ItemUiMapper.GetBagSlot(itemData);
        if (slot < 0 || !cells.ContainsKey(slot) || usedSlots.Contains(slot))
        {
            slot = FindFreeSlot(cells, usedSlots, fallbackIndex);
        }

        if (slot < 0 || !cells.TryGetValue(slot, out GameObject itemBag))
        {
            return;
        }

        usedSlots.Add(slot);
        Item item = RestoreItemFromDatabase(itemData);
        itemBag.GetComponent<EquimentItem>().SetItemEquiment(item, slot, itemData);
    }

    private static int FindFreeSlot(Dictionary<int, GameObject> cells, HashSet<int> usedSlots, int preferredIndex)
    {
        if (cells.ContainsKey(preferredIndex) && !usedSlots.Contains(preferredIndex))
        {
            return preferredIndex;
        }

        foreach (int slot in cells.Keys)
        {
            if (!usedSlots.Contains(slot))
            {
                return slot;
            }
        }

        return -1;
    }

}

public static class ItemUiMapper
{
    public const int BagColumns = 5;
    public const int BagRows = 6;
    public const int BagSlotCount = BagColumns * BagRows;

    private const byte AxmolBagPosition = 3;
    private const byte AxmolStoragePosition = 4;

    public static bool IsBagItem(ItemData item)
    {
        return item != null && item.Local == AxmolBagPosition;
    }

    public static bool IsStorageItem(ItemData item)
    {
        return item != null && item.Local == AxmolStoragePosition;
    }

    public static int GetBagSlot(ItemData item)
    {
        if (item == null)
        {
            return -1;
        }

        int x = item.X;
        int y = item.Y;
        if (x < 0 || x >= BagColumns || y < 0 || y >= BagRows)
        {
            return -1;
        }

        return (y * BagColumns) + x;
    }

    public static Item RestoreItem(ItemData itemData)
    {
        Database database = BuildDatabase(itemData);
        try
        {
            return new Item(database);
        }
        catch (Exception exception)
        {
            Debug.LogWarning("ItemUiMapper restore item fallback. id=" + itemData?.id +
                             " genre=" + itemData?.Equipclasscode +
                             " detail=" + itemData?.Detailtype +
                             " particular=" + itemData?.Particulartype +
                             " level=" + itemData?.Level +
                             " gold=" + itemData?.IdGold +
                             " error=" + exception.Message);
            database.type = Defination.Type.normalEquip;
            database.rowIndex = 0;
            return new Item(database);
        }
    }

    private static Database BuildDatabase(ItemData itemData)
    {
        if (itemData == null)
        {
            return new Database();
        }

        Database database = new()
        {
            databaseId = itemData.id,
            genre = itemData.Equipclasscode,
            detail = itemData.Detailtype,
            particular = itemData.Particulartype,
            level = Math.Max(1, (int)itemData.Level),
            series = itemData.Series,
            stack = Math.Max(0, (int)itemData.Stack),
            rowIndex = itemData.IdGold,
            type = ResolveItemType(itemData)
        };

        if (itemData.Magics != null)
        {
            int count = Math.Min(6, itemData.Magics.Count);
            for (int index = 0; index < count; index++)
            {
                KMagicAttrib magic = itemData.Magics[index];

                int value0 = magic.nValue != null && magic.nValue.Length > 0 ? magic.nValue[0] : 0;
                int value1 = magic.nValue != null && magic.nValue.Length > 1 ? magic.nValue[1] : 0;
                int value2 = magic.nValue != null && magic.nValue.Length > 2 ? magic.nValue[2] : 0;
                SetMagic(database, index, magic.nAttribType, value0, value1, value2);
            }
        }

        return database;
    }

    private static Defination.Type ResolveItemType(ItemData itemData)
    {
        if (itemData.Equipclasscode != (byte)Defination.Genre.item_equip)
        {
            return Defination.Type.normalItem;
        }

        if (itemData.IsPlasma)
        {
            return Defination.Type.platinaEquip;
        }

        return itemData.IdGold > 0
            ? Defination.Type.goldEquip
            : Defination.Type.normalEquip;
    }

    private static void SetMagic(Database database, int index, int type, int value0, int value1, int value2)
    {
        switch (index)
        {
            case 0:
                database.magic0Type = type;
                database.magic0Value0 = value0;
                database.magic0Value1 = value1;
                database.magic0Value2 = value2;
                break;

            case 1:
                database.magic1Type = type;
                database.magic1Value0 = value0;
                database.magic1Value1 = value1;
                database.magic1Value2 = value2;
                break;

            case 2:
                database.magic2Type = type;
                database.magic2Value0 = value0;
                database.magic2Value1 = value1;
                database.magic2Value2 = value2;
                break;

            case 3:
                database.magic3Type = type;
                database.magic3Value0 = value0;
                database.magic3Value1 = value1;
                database.magic3Value2 = value2;
                break;

            case 4:
                database.magic4Type = type;
                database.magic4Value0 = value0;
                database.magic4Value1 = value1;
                database.magic4Value2 = value2;
                break;

            case 5:
                database.magic5Type = type;
                database.magic5Value0 = value0;
                database.magic5Value1 = value1;
                database.magic5Value2 = value2;
                break;
        }
    }
}
