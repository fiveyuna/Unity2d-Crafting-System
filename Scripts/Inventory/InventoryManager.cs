using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.SceneManagement;

public class InventoryManager : MonoBehaviour, IDataInitialization
{
    // public InventorySlot[] inventorySlots;
    [Header("InventoryManager")]
    public static InventoryManager inventoryInstance;
    [SerializeField] private GameObject pfInventoryItem;
    [SerializeField] private GameObject mainInventoryUI;
    
    // Data
    [HideInInspector] public List<InventoryData> inventoryDataList;

    [Header("Tab UI")]
    [HideInInspector] public GameObject[] slotContainers;

    [Header("Const Value")]
    private const int MAX_COUNT = 999;
    private Dictionary<string,int> TYPE_DICT = new Dictionary<string,int> {
        {"Drop", 0},
        {"Basic", 1},
        {"Main", 2},
        {"Event", 3}
    };

    private void Awake() {
        inventoryInstance = this;

       SetInventorySlotsDefault();
    }

    public void SetInventorySlotsDefault() {
        slotContainers = mainInventoryUI.GetComponent<InventoryUI>().slotContainers;
    }

    // add to inventory
    public bool AddItem(ItemSO item, int count) {
        // Check inventory type & Set slot index
        int slotIndex = TYPE_DICT[item.itemType.ToString()];
        InventorySlot[] inventorySlots = slotContainers[slotIndex].GetComponentsInChildren<InventorySlot>();

        // Add item to Inventory
        for (int i=0; i<inventorySlots.Length; i++) {
            InventorySlot slot = inventorySlots[i];
            InventoryItem itemInSlot = slot.GetComponentInChildren<InventoryItem>();

            // check child component for is inventory slot empty
            if (itemInSlot == null) {
                SpawnNewItem(item, slot, count);
                // inventoryDataList.Add(new InventoryData(i, 1, item.itemId, 1)); // @ Type(second parameter) fix for test
                return true;
            }

            // Check same slot & lower than max
            else if (itemInSlot.item == item && itemInSlot.count < MAX_COUNT && itemInSlot.item.stackable==true) {
                itemInSlot.count += count;
                itemInSlot.RefreshCount();

                // InventoryData inventoryData = inventoryDataList.Find(x => x.slotId==i && x.slotType == 1);
                // inventoryData.count = itemInSlot.count;
                return true;
            }
        }
        return false;

    }

    private void SpawnNewItem(ItemSO item, InventorySlot slot, int count) {
        GameObject newItem = Instantiate(pfInventoryItem, slot.transform);
        newItem.gameObject.name = newItem.gameObject.name.Replace("(Clone)","").Trim();
        InventoryItem inventoryItem = newItem.GetComponent<InventoryItem>();
        inventoryItem.count = count;

        inventoryItem.InitializeItem(item);
        inventoryItem.RefreshCount();
    }

    // Craft - Button_IsAddIngredients()
    public bool IsAddIngredientItems(List<ItemSO> craftItems) {
        List<InventoryItem> changeList = new List<InventoryItem>();
        Dictionary<int,bool> craftDict = new Dictionary<int, bool>();

        // Set craft dictionary
        for (int i = 0; i < craftItems.Count; i++) {
            craftDict.Add(i, false);
        }

        // Check Is sufficient ingredients
        foreach (KeyValuePair<string,int> pair in TYPE_DICT) {
            InventorySlot[] inventorySlots = slotContainers[pair.Value].GetComponentsInChildren<InventorySlot>();
            for (int i=0; i<inventorySlots.Length; i++) {
                InventoryItem itemInSlot = inventorySlots[i].GetComponentInChildren<InventoryItem>();
                if (itemInSlot != null) {
                    // Skip
                    if (!craftItems.Contains(itemInSlot.item)) {continue;} 

                    // Compare with all of ingredient
                    for (int j=0; j < craftItems.Count; j++) {
                        // Equal with inventory item && count>=1
                        // Debug.Log("Test. IsAddIngredientItems() : [" + j + "] " + craftItems[j].itemId + ", [" + i + "]itemInSlot : " + itemInSlot.item.itemId + ", slotItem count : " + itemInSlot.count);
                        if (!craftDict[j] && craftItems[j].itemId == itemInSlot.item.itemId && itemInSlot.count > 0) {
                            itemInSlot.count--;
                            changeList.Add(itemInSlot);
                            craftDict[j] = true;
                        }
                    }       
                }

                // if modify all item
                if (!craftDict.ContainsValue(false)) {
                    foreach (InventoryItem changedInvenItem in changeList) {
                        changedInvenItem.RefreshCount();
                    }
                    return true;
                }
            }
        }

        // Result : false
        foreach (InventoryItem changedInvenItem in changeList) {
            changedInvenItem.count++;
        }
        return false;
    }

    // Manage DataPersistence System
    public void LoadAndSettingInventoryData() {
        
        int i = 0;
        foreach (KeyValuePair<string,int> pair in TYPE_DICT){
            InventorySlot[] inventorySlots = slotContainers[pair.Value].GetComponentsInChildren<InventorySlot>();
            
            // Delete exist gameobject in inventory slot
            foreach (InventorySlot slot in inventorySlots) {
                InventoryItem itemInSlot = slot.GetComponentInChildren<InventoryItem>();
                if (itemInSlot != null) Destroy(itemInSlot.gameObject);
            }

            while (i < inventoryDataList.Count) {
                InventoryData inventoryData = inventoryDataList[i];
                if (inventoryData.slotType == pair.Key) {
                    InventorySlot slot = inventorySlots[inventoryData.slotId];
                    ItemSO inventoryItem = Resources.Load("SOItems/" + inventoryData.slotType + "/" + inventoryData.itemId) as ItemSO;

                    SpawnNewItem(inventoryItem, slot, inventoryData.count);
                    i++;
                } else {
                    break;
                }
            }
            
        }
    }

    public void InventoryItemListToDataList() {
        inventoryDataList = new List<InventoryData>();

        foreach (KeyValuePair<string,int> pair in TYPE_DICT){
            InventorySlot[] inventorySlots = slotContainers[pair.Value].GetComponentsInChildren<InventorySlot>();

            for (int i=0; i<inventorySlots.Length; i++) {
                InventorySlot slot = inventorySlots[i];
                InventoryItem itemInSlot = slot.GetComponentInChildren<InventoryItem>();
                if (itemInSlot != null) {
                    inventoryDataList.Add(new InventoryData(i, itemInSlot.item.itemType.ToString(), itemInSlot.item.itemId, itemInSlot.count));
                }
            }
        }
    }

    public void LoadData(GameData data)
    {
        this.inventoryDataList = data.inventoryList;

        LoadAndSettingInventoryData();
    }

    public void SaveData(GameData data)
    {
        InventoryItemListToDataList();
        data.inventoryList = this.inventoryDataList;
    }
}
