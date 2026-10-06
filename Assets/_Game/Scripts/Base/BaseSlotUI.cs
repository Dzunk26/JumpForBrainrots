using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class BaseSlotUI : MonoBehaviour {
    [SerializeField] private TextMeshProUGUI totalIncomeText;
    [SerializeField] private BaseSlot baseSlot;

    private void Start() {
        baseSlot.OnTotalIncomeChanged += BaseSlot_OnTotalIncomeChanged;
        baseSlot.OnBrainrotChanged += BaseSlot_OnBrainrotChanged;

        UpdateVisual();
    }

    private void OnDestroy() {
        baseSlot.OnTotalIncomeChanged -= BaseSlot_OnTotalIncomeChanged;
        baseSlot.OnBrainrotChanged -= BaseSlot_OnBrainrotChanged;
    }

    private void BaseSlot_OnBrainrotChanged(object sender, System.EventArgs e) {
        UpdateVisual();
    }

    private void BaseSlot_OnTotalIncomeChanged(object sender, System.EventArgs e) {
        UpdateVisual();
    }

    private void UpdateVisual() {
        if (!baseSlot.IsActive) {
            HideText();
            return;
        }

        ShowText();
        totalIncomeText.SetText(Constant.MONEY_SYMBOL + baseSlot.GetTotalIncome());
    }

    private void ShowText() {
        totalIncomeText.gameObject.SetActive(true);
    }

    private void HideText() {
        totalIncomeText.gameObject.SetActive(false);
    }
}