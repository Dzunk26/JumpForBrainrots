using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class InventorySlotUI : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler, IDropHandler {
    public SlotType SlotType => slotType;
    public int Index => index;
    public bool HasItem => itemSO != null;


    [SerializeField] private Button button;
    [SerializeField] private Image iconImage;
    [SerializeField] private TextMeshProUGUI incomeText;
    [SerializeField] private GameObject selectedFrame;

    private SlotType slotType;
    private int index;
    private ItemSO itemSO;
    private bool isDragging;

    private void OnEnable() {
        button.onClick.AddListener(OnClickSlot);
    }

    private void OnDisable() {
        button.onClick.RemoveListener(OnClickSlot);
        if (isDragging) EndDrag();
    }

    public void OnInit(SlotType slotType, int index, ItemSO itemSO, bool isSelected) {
        this.slotType = slotType;
        this.index = index;
        this.itemSO = itemSO;

        // gay khong co thu nhap nen chi hien icon
        BrainrotSO brainrotSO = itemSO as BrainrotSO;
        iconImage.gameObject.SetActive(HasItem);
        incomeText.gameObject.SetActive(brainrotSO != null);
        if (HasItem) {
            iconImage.sprite = itemSO.GetIcon();
        }
        if (brainrotSO != null) {
            incomeText.SetText(Constant.MONEY_SYMBOL + Utils.SciNotToUSName(brainrotSO.GetIncomeAmount()) + "/s");
        }

        SetSelected(isSelected);
    }

    public ItemSO GetItemSO() {
        return itemSO;
    }

    public void SetSelected(bool isSelected) {
        selectedFrame.SetActive(isSelected);
    }

    public void OnBeginDrag(PointerEventData eventData) {
        if (!HasItem || !UIManager.Instance.IsUIOpen<CanvasInventory>()) {
            eventData.pointerDrag = null;
            return;
        }
        isDragging = true;
        InventoryDragHelper.Instance.Show(itemSO.GetIcon(), eventData.position);
    }

    public void OnDrag(PointerEventData eventData) {
        InventoryDragHelper.Instance.Move(eventData.position);
    }

    public void OnEndDrag(PointerEventData eventData) {
        EndDrag();
    }

    public void OnDrop(PointerEventData eventData) {
        InventorySlotUI src = eventData.pointerDrag ? eventData.pointerDrag.GetComponent<InventorySlotUI>() : null;
        if (src == null || src == this) return;
        InventoryManager.Instance.MoveItem(src.SlotType, src.Index, slotType, index);
    }

    private void EndDrag() {
        isDragging = false;
        if (InventoryDragHelper.HasInstance) InventoryDragHelper.Instance.Hide();
    }

    private void OnClickSlot() {
        InventoryManager.Instance.OnSlotClick(slotType, index);
    }
}