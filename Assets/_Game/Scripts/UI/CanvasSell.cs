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
    [SerializeField] private GameObject sortValueActiveImage;
    [SerializeField] private GameObject selectAllActiveImage;
    [SerializeField] private TextMeshProUGUI totalValueText;
    [SerializeField] private SellSlotUI slotPrefab;
    [SerializeField] private Transform slotParent;
    [SerializeField] private ScrollRect slotScrollRect;

    private List<SellSlotUI> slots = new List<SellSlotUI>();
    private List<InventoryItem> items = new List<InventoryItem>();
    private List<bool> isSelecteds = new List<bool>();
    private bool isSortByValue;

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

        if (isSortByValue) {
            items.Sort(CompareSellPrice);
        }

        isSelecteds.Clear();
        for (int i = 0; i < items.Count; i++) {
            isSelecteds.Add(false);
        }

        UpdateSlotsVisual();
        UpdateFooterVisual();
        sortValueActiveImage.SetActive(isSortByValue);
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
        selectAllActiveImage.SetActive(IsAllSelected());
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
        return b.brainrotSO.GetSellPrice().CompareTo(a.brainrotSO.GetSellPrice());
    }

    private void OnClickSortValueButton() {
        isSortByValue = !isSortByValue;
        ReloadItems();
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