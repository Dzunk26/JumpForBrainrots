using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HotBarUI : MonoBehaviour {
    [SerializeField] private InventorySlotUI slotPrefab;
    [SerializeField] private Transform slotParent;
    [SerializeField] private Button backpackButton;

    private List<InventorySlotUI> slots = new List<InventorySlotUI>();

    private void OnEnable() {
        backpackButton.onClick.AddListener(OnClickBackpackButton);
    }

    private void OnDisable() {
        backpackButton.onClick.RemoveListener(OnClickBackpackButton);
    }

    public void OnInit() {
        for (int i = 0; i < Constant.HOTBAR_SIZE; i++) {
            slots.Add(Instantiate(slotPrefab, slotParent));
        }
        UpdateVisual();
    }

    public void UpdateVisual() {
        BrainrotSO[] items = InventoryManager.Instance.GetHotbarItems();

        for (int i = 0; i < slots.Count; i++) {
            slots[i].OnInit(SlotType.Hotbar, i, items[i], InventoryManager.Instance.IsSelected(SlotType.Hotbar, i));
        }
    }

    private void OnClickBackpackButton() {
        if (UIManager.Instance.IsUIOpen<CanvasInventory>()) {
            UIManager.Instance.CloseUI<CanvasInventory>(0f);
        }
        else {
            UIManager.Instance.OpenUI<CanvasInventory>();
        }
    }
}