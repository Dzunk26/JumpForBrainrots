using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class BackpackDropZone : MonoBehaviour, IDropHandler {
    public void OnDrop(PointerEventData eventData) {
        InventorySlotUI src = eventData.pointerDrag ? eventData.pointerDrag.GetComponent<InventorySlotUI>() : null;
        if (src == null || src.SlotType != SlotType.Hotbar) return;
        InventoryManager.Instance.MoveItem(SlotType.Hotbar, src.Index, SlotType.Backpack, -1);
    }
}