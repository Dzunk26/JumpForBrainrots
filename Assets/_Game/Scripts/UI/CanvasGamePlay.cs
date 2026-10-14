using BreakInfinity;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class CanvasGamePlay : UICanvas {
    [SerializeField] private TextMeshProUGUI moneyText;
    private string moneyNotated;

    private void Update() {
        UpdateVisual();
    }

    private void UpdateVisual() {
        BigDouble totalMoney = PlayerProgress.Instance.GetMoney();
        moneyNotated = Utils.SciNotToUSName(totalMoney);
        moneyText.SetText(Constant.MONEY_SYMBOL + moneyNotated);
    }
}