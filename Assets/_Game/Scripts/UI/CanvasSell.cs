using BreakInfinity;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CanvasSell : UICanvas {
    [SerializeField] private Button closeButton;
    [SerializeField] private Button sortValueButton;
    [SerializeField] private Button selectAllButton;
    [SerializeField] private Button sellButton;
    [SerializeField] private GameObject selectAllVisual;
    [SerializeField] private GameObject clearVisual;
    [SerializeField] private GameObject sortDescVisual;
    [SerializeField] private GameObject sortAscVisual;
    [SerializeField] private TextMeshProUGUI totalValueText;
    [SerializeField] private SellSlotUI slotPrefab;
    [SerializeField] private Transform slotParent;
    [SerializeField] private ScrollRect slotScrollRect;

    private List<SellSlotUI> slots = new List<SellSlotUI>();
    private List<InventoryItem> items = new List<InventoryItem>();
    private List<bool> isSelecteds = new List<bool>();
    private bool isSortDescending = true;

    private void OnEnable() {
        closeButton.onClick.AddListener(OnClickCloseButton);
        sortValueButton.onClick.AddListener(OnClickSortValueButton);
        selectAllButton.onClick.AddListener(OnClickSelectAllButton);
        sellButton.onClick.AddListener(OnClickSellButton);
    }

    private void OnDisable() {
        closeButton.onClick.RemoveListener(OnClickCloseButton);
        sortValueButton.onClick.RemoveListener(OnClickSortValueButton);
        selectAllButton.onClick.RemoveListener(OnClickSelectAllButton);
        sellButton.onClick.RemoveListener(OnClickSellButton);
    }

    public override void SetUp() {
        base.SetUp();
        isSortDescending = true;
        ReloadItems();
        slotScrollRect.verticalNormalizedPosition = 1f;
    }

    public void OnInit(Player player) {

    }

    public void OnSlotClick(int index) {
        isSelecteds[index] = !isSelecteds[index];
        slots[index].SetSelected(isSelecteds[index]);
        UpdateFooterVisual();
    }

    private void ReloadItems() {
        items = InventoryManager.Instance.GetAllItems();
        SortItems();

        isSelecteds.Clear();
        for (int i = 0; i < items.Count; i++) {
            isSelecteds.Add(false);
        }

        UpdateSlotsVisual();
        UpdateFooterVisual();
        UpdateSortVisual();
    }

    private void SortItems() {
        items.Sort(CompareSellPrice);
    }

    private void UpdateSortVisual() {
        sortDescVisual.SetActive(isSortDescending);
        sortAscVisual.SetActive(!isSortDescending);
    }

    private void UpdateSlotsVisual() {
        for (int i = 0; i < items.Count; i++) {
            if (i >= slots.Count) {
                slots.Add(Instantiate(slotPrefab, slotParent));
            }

            slots[i].gameObject.SetActive(true);
            slots[i].OnInit(this, i, items[i].brainrotSO, isSelecteds[i]);
        }

        for (int i = items.Count; i < slots.Count; i++) {
            slots[i].gameObject.SetActive(false);
        }
    }

    private void UpdateFooterVisual() {
        BigDouble totalValue = GetTotalValue();
        totalValueText.SetText(Constant.MONEY_SYMBOL + Utils.SciNotToUSName(totalValue));
        sellButton.interactable = totalValue > 0;

        bool isAllSelected = IsAllSelected();
        selectAllVisual.SetActive(!isAllSelected);
        clearVisual.SetActive(isAllSelected);
    }

    private BigDouble GetTotalValue() {
        BigDouble totalValue = 0;
        for (int i = 0; i < items.Count; i++) {
            if (isSelecteds[i]) totalValue += items[i].brainrotSO.GetSellPrice();
        }
        return totalValue;
    }

    private bool IsAllSelected() {
        if (items.Count == 0) return false;
        return !isSelecteds.Contains(false);
    }

    private int CompareSellPrice(InventoryItem a, InventoryItem b) {
        int result = a.brainrotSO.GetSellPrice().CompareTo(b.brainrotSO.GetSellPrice());
        return isSortDescending ? -result : result;
    }

    private void OnClickSortValueButton() {
        isSortDescending = !isSortDescending;

        HashSet<InventoryItem> selectedItems = new HashSet<InventoryItem>();
        for (int i = 0; i < items.Count; i++) {
            if (isSelecteds[i]) selectedItems.Add(items[i]);
        }

        SortItems();

        for (int i = 0; i < items.Count; i++) {
            isSelecteds[i] = selectedItems.Contains(items[i]);
        }

        UpdateSlotsVisual();
        UpdateFooterVisual();
        UpdateSortVisual();
        slotScrollRect.verticalNormalizedPosition = 1f;
    }

    private void OnClickSelectAllButton() {
        bool isSelectAll = !IsAllSelected();
        for (int i = 0; i < isSelecteds.Count; i++) {
            isSelecteds[i] = isSelectAll;
            slots[i].SetSelected(isSelectAll);
        }
        UpdateFooterVisual();
    }

    private void OnClickSellButton() {
        List<InventoryItem> sellItems = new List<InventoryItem>();
        for (int i = 0; i < items.Count; i++) {
            if (isSelecteds[i]) sellItems.Add(items[i]);
        }
        if (sellItems.Count == 0) return;

        InventoryManager.Instance.SellItems(sellItems);
        ReloadItems();
    }

    private void OnClickCloseButton() {
        Close(0f);
    }
}