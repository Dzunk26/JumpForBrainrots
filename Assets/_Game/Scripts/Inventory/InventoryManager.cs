using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum SlotType {
    Hotbar,
    Backpack,
}

public class InventoryManager : Singleton<InventoryManager> {
    public BrainrotSO[] GetHotbarItems() => hotbarItems;
    public List<BrainrotSO> GetBackpackItems() => backpackItems;

    private BrainrotSO[] hotbarItems = new BrainrotSO[Constant.HOTBAR_SIZE];
    private List<BrainrotSO> backpackItems = new List<BrainrotSO>();
    private SlotType selectedSlotType;
    private int selectedIndex = -1;
    private Player player;

    public void OnInit(Player player) {
        this.player = player;
        System.Array.Clear(hotbarItems, 0, hotbarItems.Length);
        backpackItems.Clear();
        selectedIndex = -1;
    }

    public bool IsSelected(SlotType slotType, int index) {
        return selectedIndex == index && selectedSlotType == slotType;
    }

    public void AddBrainrot(BrainrotSO brainrotSO) {
        int emptyIndex = System.Array.IndexOf(hotbarItems, null);
        if (emptyIndex != -1) {
            hotbarItems[emptyIndex] = brainrotSO;
        }
        else {
            backpackItems.Add(brainrotSO);
        }
        UpdateUI();
    }

    public void OnSlotClick(SlotType slotType, int index) {
        if (!player.CanEquipBrainrot()) return;

        if (IsSelected(slotType, index)) {
            Deselect();
        }
        else if (IsCrossSlotClick(slotType)) {
            SwapSelectedWith(slotType, index);
        }
        else if (HasItem(slotType, index)) {
            Select(slotType, index);
        }

        UpdateUI();
    }


    public void ClearSelection() {
        selectedIndex = -1;
        UpdateUI();
    }

    public void RemoveSelectedItem() {
        if (selectedSlotType == SlotType.Hotbar) {
            hotbarItems[selectedIndex] = null;
        }
        else {
            backpackItems.RemoveAt(selectedIndex);
        }
        selectedIndex = -1;
        UpdateUI();
    }

    public void ReplaceSelectedItem(BrainrotSO brainrotSO) {
        if (selectedSlotType == SlotType.Hotbar) {
            hotbarItems[selectedIndex] = brainrotSO;
        }
        else {
            backpackItems[selectedIndex] = brainrotSO;
        }

        UpdateUI();
    }

    private bool HasItem(SlotType slotType, int index) {
        if (slotType == SlotType.Hotbar) return hotbarItems[index] != null;

        return index < backpackItems.Count;
    }

    private void Select(SlotType slotType, int index) {
        selectedSlotType = slotType;
        selectedIndex = index;
        player.EquipBrainrot(GetSelectedItem());
    }

    private BrainrotSO GetSelectedItem() {
        if (selectedSlotType == SlotType.Hotbar) return hotbarItems[selectedIndex];

        return backpackItems[selectedIndex];
    }

    private void Deselect() {
        selectedIndex = -1;
        player.UnequipBrainrot();
    }

    private bool IsCrossSlotClick(SlotType clickedSlotType) {
        return selectedIndex != -1 && selectedSlotType != clickedSlotType;
    }

    private void SwapSelectedWith(SlotType slotType, int index) {
        if (slotType == SlotType.Hotbar) {
            SwapBackpackAndHotbar(selectedIndex, index);
        }
        else {
            SwapBackpackAndHotbar(index, selectedIndex);
        }

        selectedSlotType = slotType;
        selectedIndex = index;
    }

    private void SwapBackpackAndHotbar(int backpackIndex, int hotbarIndex) {
        BrainrotSO backpackItem = backpackItems[backpackIndex];
        BrainrotSO hotbarItem = hotbarItems[hotbarIndex];

        if (hotbarItem == null) {
            backpackItems.RemoveAt(backpackIndex);
        }
        else {
            backpackItems[backpackIndex] = hotbarItem;
        }
        hotbarItems[hotbarIndex] = backpackItem;
    }

    private void UpdateUI() {
        UIManager.Instance.GetUI<CanvasGamePlay>().UpdateHotbar();

        if (UIManager.Instance.IsUIOpen<CanvasInventory>()) {
            UIManager.Instance.GetUI<CanvasInventory>().UpdateVisual();
        }
    }
}