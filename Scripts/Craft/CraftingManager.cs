using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// < Crafting System >
///  - Can craft 4 ingredient.
///  - Can add(or remove) the ingredients in batches
///     - Then, unlock unused slots
/// </summary>
public class CraftingManager : MonoBehaviour, IDataPersistence
{
    [Header("UI")]
    public CraftSlot[] craftSlots; // Ingredient slot (<- CraftSlot.cs)
    public Transform outputSlot; // Result item slot
    
    [SerializeField] private GameObject pfCreatedItem; // Basic item game object
    [SerializeField] private Button getItemButton; // Get result item
    [SerializeField] private Animator craftItemAnimator;
    [SerializeField] private GameObject slotCoverUI;

    [Header("Issue Response")]
    [HideInInspector] public ItemSO createdItem; 

    [Header("Unlock")]
    [SerializeField] private GameObject[] unlockCheckUI;
    [SerializeField] private GameObject[] unlockBackgroundUI;
    private bool[] isCraftUnlocked;

    [Header("Game")]
    [SerializeField] private LevelLoader levelLoader;
    [HideInInspector] public int ingredientsCount = 1;
    private List<RecipeSO> recipeList; // Crafting recipe list
    private InventoryManager inventoryManager;
    [HideInInspector] public static CraftingManager instance;
    [HideInInspector] public List<string> storyProgress;


    private void Awake() {
        instance = this;

        // Get Recipes from resources folder
        recipeList = Resources.LoadAll("SORecipes", typeof(RecipeSO)).Cast<RecipeSO>().ToList();

        getItemButton.gameObject.SetActive(false);

        ingredientsCount = 1;
    }

    private void Start() {
        // isCraftUnlocked==True : slot is unlocked.
        SetUnlockUI();
        OnOffSlotCovers(false);

        inventoryManager = GameObject.Find("InventoryManager").GetComponent<InventoryManager>();
    }

    // Button Onclick method
    public void TryCraft() 
    {
        // Already have output
        if (outputSlot.childCount > 0) 
        {
            GetCreatedItem(createdItem);
        }

        // Set ingredient list
        List<ItemSO> craftIngredients = new List<ItemSO>(); 
        foreach (CraftSlot slot in craftSlots) 
        {
            InventoryItem itemInSlot = slot.GetComponentInChildren<InventoryItem>();
            if (itemInSlot != null) {
                craftIngredients.Add(itemInSlot.item);
            }
        }

        // If is not empty ingredient slot
        if (craftIngredients.Count > 1) 
        {
            // Check all recipe
            foreach (RecipeSO r in recipeList) 
            { 
                // Compare list's count
                if (craftIngredients.Count == r.ingredients.Count)
                { 
                    // Compare list value
                    bool isEqual = Enumerable.SequenceEqual(craftIngredients.OrderBy(x => x.itemId), r.ingredients.OrderBy(x => x.itemId));
                    
                    // Find Collect Recipe (used equal ingredients)
                    if (isEqual) 
                    { 
                        // Craft Animation, Audio ETC
                        craftItemAnimator.Play("clock_in");
                        ParticlePlay.instance.PlayParticle(0);
                        AudioManager.Inst.PlayOneShot(SoundName.SFX_Craft);
                        
                        // Delete used ingredients
                        DeleteSlotItems();
                        
                        // Create result item
                        CraftNewItem(r.outputItem);
                        return;
                    }
                }
            }
        }

    }

// Button event
    public void Button_RefreshCraftSlot() {
        // TODO : Cope with result-false when AddItem()
        foreach (CraftSlot slot in craftSlots) {
            InventoryItem itemInSlot = slot.GetComponentInChildren<InventoryItem>();
            if (itemInSlot != null) {
                inventoryManager.AddItem(itemInSlot.item, ingredientsCount);
            }
        }
        DeleteSlotItems();

        ingredientsCount = 1;
        OnOffSlotCovers(false);
    }

    public void Button_IsAddIngredients(bool isAdd) {
        List<InventoryItem> craftIngredients = new List<InventoryItem>(); 
        List<ItemSO> craftItems = new List<ItemSO>();

        foreach (CraftSlot slot in craftSlots) {
            InventoryItem itemInSlot = slot.GetComponentInChildren<InventoryItem>();
            if (itemInSlot != null) {
                craftIngredients.Add(itemInSlot);
                craftItems.Add(itemInSlot.item);
            }
        }
        
        if (craftIngredients.Count < 2) { return; }

        if (isAdd) { // Add
            if (inventoryManager.IsAddIngredientItems(craftItems)) {
                ingredientsCount++;
                foreach (InventoryItem cItem in craftIngredients) {
                    cItem.count++;
                    cItem.RefreshCount();
                }
            } else {
                Debug.Log("[CraftM] Insufficient Ingredients.");
            }
        } else if (ingredientsCount > 1) { // Subtract
            foreach (ItemSO item in craftItems) {
                // TODO : Cope with result-false when AddItem()
                inventoryManager.AddItem(item, 1);
            }

            ingredientsCount--;
            foreach (InventoryItem item in craftIngredients) {
                item.count--;
                item.RefreshCount();
            }
        }
        
        if (ingredientsCount == 1) {
            OnOffSlotCovers(false);
        } else if (ingredientsCount == 2) {
            OnOffSlotCovers(true);
        }
        
    }

    private void OnOffSlotCovers(bool isOn) {
        slotCoverUI.SetActive(isOn);        
    }

    public void Button_CompleteResearch()
    {
        bool isComplete = true;
        foreach (bool isUnlocked in isCraftUnlocked) {
            if (!isUnlocked) {
                isComplete = false;
                break;
            }
        }
        
        if (isComplete) {
            storyProgress.Add("complete");
            AudioManager.Inst.PlayOneShot(SoundName.SFX_Craft);
            levelLoader.LoadScene(SceneName.Dialogue);
        } else {
            AudioManager.Inst.PlayOneShot(SoundName.SFX_Wrong);
        }
        
    }

    private void CraftNewItem(ItemSO item) {
        // For add item when scene move
        createdItem = item;

        // Craft new item
        GameObject newItem = Instantiate(pfCreatedItem, outputSlot); // Generate item
        newItem.GetComponent<Image>().sprite = item.image; // set item image
        newItem.GetComponentInChildren<TextMeshProUGUI>().text = (ingredientsCount==1) ? "" : ingredientsCount.ToString(); // set number of item 
        
        // Set active True Get button
        getItemButton.gameObject.SetActive(true);

        // Set onclick function
        getItemButton.onClick.RemoveAllListeners();
        getItemButton.onClick.AddListener(() => GetCreatedItem(item));
    }

    private void GetCreatedItem(ItemSO item) {

        craftItemAnimator.Play("clock_out");

        // Add item to inventory
        bool result = inventoryManager.AddItem(item, ingredientsCount);
        if (!result) {
            Debug.Log("[CraftM] Full inventory.");
            return;
        }

        // Destroy result item
        Transform Created = outputSlot.GetChild(0);
        Destroy(Created.gameObject);
        getItemButton.gameObject.SetActive(false);

        // Initialize ingredient count 
        ingredientsCount = 1;
    }

    /// <summary>
    /// Delete items in craftSlot
    /// </summary>
    private void DeleteSlotItems() {
        foreach (CraftSlot slot in craftSlots) {
            if (slot.GetComponentInChildren<InventoryItem>() != null) {
                GameObject itemInSlot = slot.transform.GetChild(0).gameObject;
                Destroy(itemInSlot);
            }
        }
    }

// Unlock functions
    public void UnlockTheSlot(int partId) {
        isCraftUnlocked[partId] = true;
        SetUnlockUI();
    }

    private void SetUnlockUI() {
        for(int i = 0; i<isCraftUnlocked.Length; i++) {
            unlockCheckUI[i].SetActive(isCraftUnlocked[i]);
            unlockBackgroundUI[i].SetActive(isCraftUnlocked[i]);
        } 
    }

    // disable
    public void AddSlotItemData() {
        // add ouput slot item
        if (this.outputSlot.childCount > 0) {
            inventoryManager.AddItem(this.createdItem, this.ingredientsCount);
            Debug.Log("Test. AddSlotItemData() output item : " + this.createdItem.name + ", count : " + this.ingredientsCount);

        }

        // add input slot item
        foreach (CraftSlot slot in this.craftSlots) {
            InventoryItem itemInSlot = slot.GetComponentInChildren<InventoryItem>();
            if (itemInSlot != null) {
                inventoryManager.AddItem(itemInSlot.item, this.ingredientsCount);
                Debug.Log("Test. AddSlotItemData() item : " + itemInSlot.item.name + ", count : " + this.ingredientsCount);
            }
        }

        inventoryManager.InventoryItemListToDataList();
    }

    public void ButtonEvent_LoadNote()
    {
        PlayerPrefs.SetString("BeforeNote", "Craft");
    }

// Data Persistence
    public void LoadData(GameData data)
    {
        this.isCraftUnlocked = data.isCraftUnlocked; 
        this.storyProgress = data.storyProgress;
    }

    public void SaveData(GameData data)
    {
        data.isCraftUnlocked = this.isCraftUnlocked;
        data.storyProgress = this.storyProgress;
    }
}
