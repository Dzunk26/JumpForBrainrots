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
    public ItemSO[] GetHotbarItems() => hotbarItems;
    public List<ItemSO> GetBackpackItems() => backpackItems;

    [SerializeField] private ListBrainrotSO listBrainrotSO;
    [SerializeField] private ToolSO batToolSO;

    private ItemSO[] hotbarItems = new ItemSO[Constant.HOTBAR_SIZE];
    private List<ItemSO> backpackItems = new List<ItemSO>();
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
        AddDefaultItems();
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
        AddItem(brainrotSO, out slotType, out index);
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


    // chi lay brainrot ban duoc, bo qua gay
    public List<InventoryItem> GetSellableItems() {
        List<InventoryItem> items = new List<InventoryItem>();

        for (int i = 0; i < hotbarItems.Length; i++) {
            if (!(hotbarItems[i] is BrainrotSO brainrotSO) || !brainrotSO.IsSellable()) continue;
            items.Add(new InventoryItem { slotType = SlotType.Hotbar, index = i, brainrotSO = brainrotSO });
        }

        for (int i = 0; i < backpackItems.Count; i++) {
            if (!(backpackItems[i] is BrainrotSO brainrotSO) || !brainrotSO.IsSellable()) continue;
            items.Add(new InventoryItem { slotType = SlotType.Backpack, index = i, brainrotSO = brainrotSO });
        }

        return items;
    }

    public void RemoveItems(List<InventoryItem> removeItems) {
        List<int> backpackIndexes = new List<int>();

        foreach (InventoryItem item in removeItems) {
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

        NotifyChanged();
    }

    public void MoveItem(SlotType fromType, int fromIndex, SlotType toType, int toIndex) {
        if (!HasItem(fromType, fromIndex)) return;
        if (fromType == toType && fromIndex == toIndex) return;
        if (fromType == SlotType.Backpack && toType == SlotType.Backpack) return;

        if (fromType == SlotType.Hotbar && toType == SlotType.Backpack) {
            if (toIndex < 0 || toIndex >= backpackItems.Count) {
                backpackItems.Add(hotbarItems[fromIndex]);
                hotbarItems[fromIndex] = null;
            }
            else {
                SwapBackpackAndHotbar(toIndex, fromIndex);
            }
        }
        else if (fromType == SlotType.Backpack && toType == SlotType.Hotbar) {
            SwapBackpackAndHotbar(fromIndex, toIndex);
        }
        else {
            (hotbarItems[fromIndex], hotbarItems[toIndex]) = (hotbarItems[toIndex], hotbarItems[fromIndex]);
        }

        if (selectedIndex != -1) Deselect();
        NotifyChanged();
    }

    private void ApplyLoadedData() {
        if (loadedData == null) return;

        for (int i = 0; i < hotbarItems.Length && i < loadedData.hotbarIds.Length; i++)
            hotbarItems[i] = GetItemByID(loadedData.hotbarIds[i]);
        foreach (int id in loadedData.backpackIds) {
            ItemSO so = GetItemByID(id);
            if (so != null) backpackItems.Add(so);
        }
        loadedData = null;
    }

    // gay la item mac dinh: data moi / data cu chua co gay thi them vao o hotbar trong dau tien
    private void AddDefaultItems() {
        if (batToolSO == null || HasItem(batToolSO)) return;

        AddItem(batToolSO, out _, out _);
        isDirty = true;
    }

    private bool HasItem(ItemSO itemSO) {
        return System.Array.IndexOf(hotbarItems, itemSO) >= 0 || backpackItems.Contains(itemSO);
    }

    private void AddItem(ItemSO itemSO, out SlotType slotType, out int index) {
        index = System.Array.IndexOf(hotbarItems, null);

        if (index != -1) {
            slotType = SlotType.Hotbar;
            hotbarItems[index] = itemSO;
        }
        else {
            slotType = SlotType.Backpack;
            backpackItems.Add(itemSO);
            index = backpackItems.Count - 1;
        }
    }

    private ItemSO GetItemByID(int id) {
        if (batToolSO != null && batToolSO.IsMatchID(id)) return batToolSO;

        return listBrainrotSO.GetBrainrotSOByID(id);
    }

    private bool HasItem(SlotType slotType, int index) {
        if (slotType == SlotType.Hotbar) return hotbarItems[index] != null;

        return index < backpackItems.Count;
    }

    private void Select(SlotType slotType, int index) {
        selectedSlotType = slotType;
        selectedIndex = index;
        player.EquipItem(GetSelectedItem());
    }

    private ItemSO GetSelectedItem() {
        if (selectedSlotType == SlotType.Hotbar) return hotbarItems[selectedIndex];

        return backpackItems[selectedIndex];
    }

    private void Deselect() {
        selectedIndex = -1;
        player.UnequipItem();
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
        ItemSO backpackItem = backpackItems[backpackIndex];
        ItemSO hotbarItem = hotbarItems[hotbarIndex];

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