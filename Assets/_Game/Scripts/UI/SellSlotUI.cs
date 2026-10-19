using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SellSlotUI : MonoBehaviour {
    [SerializeField] private Button button;
    [SerializeField] private Image brainrotIcon;
    [SerializeField] private TextMeshProUGUI incomeText;
    [SerializeField] private TextMeshProUGUI priceText;
    [SerializeField] private GameObject selectedFrame;

    private CanvasSell canvasSell;
    private int index;

    private void OnEnable() {
        button.onClick.AddListener(OnClickSlot);
    }

    private void OnDisable() {
        button.onClick.RemoveListener(OnClickSlot);
    }

    public void OnInit(CanvasSell canvasSell, int index, BrainrotSO brainrotSO, bool isSelected) {
        this.canvasSell = canvasSell;
        this.index = index;

        brainrotIcon.sprite = brainrotSO.GetIndexIcon();
        incomeText.SetText(Constant.MONEY_SYMBOL + Utils.SciNotToUSName(brainrotSO.GetIncomeAmount()) + "/s");
        priceText.SetText(Constant.MONEY_SYMBOL + Utils.SciNotToUSName(brainrotSO.GetSellPrice()));
        SetSelected(isSelected);
    }

    public void SetSelected(bool isSelected) {
        selectedFrame.SetActive(isSelected);
    }

    private void OnClickSlot() {
        canvasSell.OnSlotClick(index);
    }
}