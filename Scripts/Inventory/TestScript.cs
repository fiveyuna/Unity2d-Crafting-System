using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestScript : MonoBehaviour
{
    private InventoryManager inventoryManager;
    public ItemSO[] itemsToSpawn;

    private void Awake() {
        inventoryManager  = GameObject.Find("InventoryManager").GetComponent<InventoryManager>();
    }

    public void SpawnItem(int id) {
        bool result = inventoryManager.AddItem(itemsToSpawn[id], 1);
        if (result) {
            // Debug.Log("[Test] Item spawn");
        } else {
            // Debug.Log("[Test] Not spawn");
        }
    }

    public void MoveInForest() {
        int randId = Random.Range(0, itemsToSpawn.Length);
        bool result = inventoryManager.AddItem(itemsToSpawn[randId], 1);
        if (result) {
            Debug.Log("[Test] spawn : " + itemsToSpawn[randId].ToString());
        } else {
            Debug.Log("[Test] Not spawn");
        }
    }
}
