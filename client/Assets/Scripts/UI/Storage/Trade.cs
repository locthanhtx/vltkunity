using System;
using System.Collections.Generic;
using game.network;
using game.resource.settings;
using game.resource.settings.item;
using Photon.ShareLibrary.Constant;
using Photon.ShareLibrary.Entities;
using UnityEngine;
using UnityEngine.UI;

public class Trade : MonoBehaviour
{
    [SerializeField]
    private GameObject ListTradeFriend;
    [SerializeField]
    private GameObject ListTrade;
    [SerializeField]
    private GameObject ListBag;
    [SerializeField]
    private GameObject ItemEquip;
    [SerializeField]
    private Button CloseGameObject;


    private Dictionary<int, GameObject> ListTradeFriends = new();
    private Dictionary<int, GameObject> ListTrades = new();
    private Dictionary<int, GameObject> ListBags = new();

    void Start()
    {
        CloseGameObject.GetComponent<Button>().onClick.AddListener(() =>
        {
            gameObject.SetActive(false);
        });
    }

    public void InitStorage()
    {
        if (ListBags.Count > 0 || ListTrades.Count > 0 || ListTradeFriends.Count > 0)
        {
            return;
        }

        for (int i = 0; i < 12; i++)
        {
            GridLayoutGroup gridLayoutGroup = ListTradeFriend.GetComponent<GridLayoutGroup>();
            GameObject TradeFriend = Instantiate(ItemEquip, Vector3.zero, Quaternion.identity);
            TradeFriend.transform.SetParent(gridLayoutGroup.transform, false);
            ListTradeFriends.Add(i, TradeFriend);

            GridLayoutGroup gridLayoutGroup1 = ListTrade.GetComponent<GridLayoutGroup>();
            GameObject Trade = Instantiate(ItemEquip, Vector3.zero, Quaternion.identity);
            Trade.transform.SetParent(gridLayoutGroup1.transform, false);
            ListTrades.Add(i, Trade);
        }

        for (int i = 0; i < 200; i++)
        {
            GridLayoutGroup gridLayoutGroup2 = ListBag.GetComponent<GridLayoutGroup>();
            GameObject Bag = Instantiate(ItemEquip, Vector3.zero, Quaternion.identity);
            Bag.transform.SetParent(gridLayoutGroup2.transform, false);
            ListBags.Add(i, Bag);
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
        }

        //TextBags.text = itemDataBags.Count + "/" + ListBags.Count;
        //TextStorages.text = itemDataStorages.Count + "/" + ListStorages.Count;

        ClearCells(ListBags);
        HashSet<int> usedBagSlots = new();
        for (int i = 0; i < itemDataBags.Count; i++)
        {
            PlaceItem(ListBags, usedBagSlots, itemDataBags[i], i);
        }
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
