using BreakInfinity;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public enum SlotType {
    Hotbar,
    Backpack,
}

public struct InventoryItem {
    public SlotType slotType;
    public int index;
    public BrainrotSO brainrotSO;
}

public class InventoryManager : Singleton<InventoryManager>, IDataPersistence {
    public bool IsDirty => isDirty;
    public BrainrotSO[] GetHotbarItems() => hotbarItems;
    public List<BrainrotSO> GetBackpackItems() => backpackItems;

    [SerializeField] private ListBrainrotSO listBrainrotSO;

    private BrainrotSO[] hotbarItems = new BrainrotSO[Constant.HOTBAR_SIZE];
    private List<BrainrotSO> backpackItems = new List<BrainrotSO>();
    private SlotType selectedSlotType;
    private int selectedIndex = -1;
    private Player player;
    private GameData loadedData;
    private bool isDirty;

    public void OnInit(Player player) {
        this.player = player;
        System.Array.Clear(hotbarItems, 0, hotbarItems.Length);
        backpackItems.Clear();
        selectedIndex = -1;
        ApplyLoadedData();
    }

    public void LoadData(GameData data) => loadedData = data;

    public void SaveData(ref GameData data) {
        data.hotbarIds = hotbarItems.Select(so => so != null ? so.GetID() : -1).ToArray();
        data.backpackIds = backpackItems.Select(so => so.GetID()).ToList();
        isDirty = false;
    }

    public bool IsSelected(SlotType slotType, int index) {
        return selectedIndex == index && selectedSlotType == slotType;
    }

    public void AddBrainrot(BrainrotSO brainrotSO) {
        AddBrainrot(brainrotSO, out _, out _);
    }

    public void AddBrainrot(BrainrotSO brainrotSO, out SlotType slotType, out int index) {
        index = System.Array.IndexOf(hotbarItems, null);

        if (index != -1) {
            slotType = SlotType.Hotbar;
            hotbarItems[index] = brainrotSO;
        }
        else {
            slotType = SlotType.Backpack;
            backpackItems.Add(brainrotSO);
            index = backpackItems.Count - 1;
        }

        NotifyChanged();
    }

    public void AddAndSelectBrainrot(BrainrotSO brainrotSO) {
        AddBrainrot(brainrotSO, out SlotType slotType, out int index);
        selectedSlotType = slotType;
        selectedIndex = index;
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
        NotifyChanged();
    }

    public void ReplaceSelectedItem(BrainrotSO brainrotSO) {
        if (selectedSlotType == SlotType.Hotbar) {
            hotbarItems[selectedIndex] = brainrotSO;
        }
        else {
            backpackItems[selectedIndex] = brainrotSO;
        }

        NotifyChanged();
    }


    public List<InventoryItem> GetAllItems() {
        List<InventoryItem> items = new List<InventoryItem>();

        for (int i = 0; i < hotbarItems.Length; i++) {
            if (hotbarItems[i] == null) continue;
            items.Add(new InventoryItem { slotType = SlotType.Hotbar, index = i, brainrotSO = hotbarItems[i] });
        }

        for (int i = 0; i < backpackItems.Count; i++) {
            items.Add(new InventoryItem { slotType = SlotType.Backpack, index = i, brainrotSO = backpackItems[i] });
        }

        return items;
    }

    public void SellItems(List<InventoryItem> sellItems) {
        BigDouble totalPrice = 0;
        List<int> backpackIndexes = new List<int>();

        foreach (InventoryItem item in sellItems) {
            totalPrice += item.brainrotSO.GetSellPrice();
            if (IsSelected(item.slotType, item.index)) Deselect();

            if (item.slotType == SlotType.Hotbar) {
                hotbarItems[item.index] = null;
            }
            else {
                backpackIndexes.Add(item.index);
            }
        }

        if (selectedIndex != -1 && selectedSlotType == SlotType.Backpack) {
            selectedIndex -= backpackIndexes.Count(i => i < selectedIndex);
        }

        backpackIndexes.Sort();
        for (int i = backpackIndexes.Count - 1; i >= 0; i--) {
            backpackItems.RemoveAt(backpackIndexes[i]);
        }

        PlayerProgress.Instance.AddMoney(totalPrice);
        NotifyChanged();
    }

    private void ApplyLoadedData() {
        if (loadedData == null) return;

        for (int i = 0; i < hotbarItems.Length && i < loadedData.hotbarIds.Length; i++)
            hotbarItems[i] = listBrainrotSO.GetBrainrotSOByID(loadedData.hotbarIds[i]);
        foreach (int id in loadedData.backpackIds) {
            BrainrotSO so = listBrainrotSO.GetBrainrotSOByID(id);
            if (so != null) backpackItems.Add(so);
        }
        loadedData = null;
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
        isDirty = true;
    }

    private void NotifyChanged() {
        isDirty = true;
        UpdateUI();
    }

    private void UpdateUI() {
        UIManager.Instance.GetUI<CanvasGamePlay>().UpdateHotbar();

        if (UIManager.Instance.IsUIOpen<CanvasInventory>()) {
            UIManager.Instance.GetUI<CanvasInventory>().UpdateVisual();
        }
    }
}