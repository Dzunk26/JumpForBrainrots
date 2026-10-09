using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class CanvasGamePlay : UICanvas {
    [SerializeField] private TextMeshProUGUI moneyText;

    private void Update() {
        UpdateVisual();
    }

    private void UpdateVisual() {
        double totalMoney = PlayerProgress.Instance.GetMoney();
        moneyText.SetText(Constant.MONEY_SYMBOL + totalMoney);
    }
}