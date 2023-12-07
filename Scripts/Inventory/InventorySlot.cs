using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class InventorySlot : MonoBehaviour, IDropHandler
{
    public void OnDrop(PointerEventData eventData)
    {
        InventoryItem dragItem = eventData.pointerDrag.GetComponent<InventoryItem>();
        InventoryItem parentItem = transform.GetComponentInChildren<InventoryItem>();

        if (transform.childCount == 0) {
            dragItem.parentAfterDrag = transform;
            
            dragItem.isChild = false;
        } else if (dragItem.item == parentItem.item) {
            parentItem.count++;
            parentItem.RefreshCount();
            Destroy(dragItem.gameObject);
        }
    }
}
