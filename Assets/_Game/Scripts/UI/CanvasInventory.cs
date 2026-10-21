using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CanvasInventory : UICanvas {
    [SerializeField] private RectTransform panel;
    [SerializeField] private Button closeButton;
    [SerializeField] private InventorySlotUI slotPrefab;
    [SerializeField] private Transform slotParent;
    [SerializeField] private ScrollRect slotScrollRect;

    private List<InventorySlotUI> slots = new List<InventorySlotUI>();

    private void OnEnable() {
        closeButton.onClick.AddListener(OnClickCloseButton);
    }

    private void OnDisable() {
        closeButton.onClick.RemoveListener(OnClickCloseButton);
    }

    public override void SetUp() {
        base.SetUp();
        UpdateVisual();
        slotScrollRect.verticalNormalizedPosition = 1f;
    }

    public void UpdateVisual() {
        List<BrainrotSO> items = InventoryManager.Instance.GetBackpackItems();

        for (int i = 0; i < items.Count; i++) {
            if (i >= slots.Count) {
                slots.Add(Instantiate(slotPrefab, slotParent));
            }

            slots[i].gameObject.SetActive(true);
            slots[i].OnInit(SlotType.Backpack, i, items[i], InventoryManager.Instance.IsSelected(SlotType.Backpack, i));
        }

        for (int i = items.Count; i < slots.Count; i++) {
            slots[i].gameObject.SetActive(false);
        }
    }

    private void OnClickCloseButton() {
        Close(0f);
    }
}