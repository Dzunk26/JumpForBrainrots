using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class InventorySlotUI : MonoBehaviour {
    [SerializeField] private Button button;
    [SerializeField] private Image iconImage;
    [SerializeField] private TextMeshProUGUI incomeText;
    [SerializeField] private GameObject selectedFrame;

    private SlotType slotType;
    private int index;
    private BrainrotSO brainrotSO;

    private void OnEnable() {
        button.onClick.AddListener(OnClickSlot);
    }

    private void OnDisable() {
        button.onClick.RemoveListener(OnClickSlot);
    }

    public void OnInit(SlotType slotType, int index, BrainrotSO brainrotSO, bool isSelected) {
        this.slotType = slotType;
        this.index = index;
        this.brainrotSO = brainrotSO;

        bool hasItem = brainrotSO != null;
        iconImage.gameObject.SetActive(hasItem);
        incomeText.gameObject.SetActive(hasItem);
        if (hasItem) {
            iconImage.sprite = brainrotSO.GetIndexIcon();
            incomeText.SetText(Constant.MONEY_SYMBOL + Utils.SciNotToUSName(brainrotSO.GetIncomeAmount()) + "/s");
        }

        SetSelected(isSelected);
    }

    public BrainrotSO GetBrainrotSO() {
        return brainrotSO;
    }

    public void SetSelected(bool isSelected) {
        selectedFrame.SetActive(isSelected);
    }

    private void OnClickSlot() {
        InventoryManager.Instance.OnSlotClick(slotType, index);
    }
}