using BreakInfinity;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UpgradeStepUI : MonoBehaviour {
    [SerializeField] private UpgradeType upgradeType;
    [SerializeField] private int step = 2;
    [SerializeField] private Button freeUpgradeButton;
    [SerializeField] private Button buyButton;
    [SerializeField] private TextMeshProUGUI amountText;
    [SerializeField] private TextMeshProUGUI priceText;

    private Player player;

    private void OnEnable() {
        freeUpgradeButton.onClick.AddListener(OnClickFreeUpgradeButton);
        buyButton.onClick.AddListener(OnClickBuyButton);
    }

    private void OnDisable() {
        freeUpgradeButton.onClick.RemoveListener(OnClickFreeUpgradeButton);
        buyButton.onClick.RemoveListener(OnClickBuyButton);
    }

    public void OnInit(Player player) {
        this.player = player;
        amountText.SetText(Constant.UPGRADE_STEP_PREFIX + step);
        UpdateVisual();
    }

    public void UpdateVisual() {
        if (player == null) return;

        if (player.IsMaxUpgrade(upgradeType)) {
            priceText.SetText(Constant.MAX_UPGRADE_TEXT);
            freeUpgradeButton.interactable = false;
            buyButton.interactable = false;
            return;
        }

        BigDouble cost = player.GetUpgradeCost(upgradeType, step);
        priceText.SetText(Constant.MONEY_SYMBOL + Utils.SciNotToUSName(cost));
        freeUpgradeButton.interactable = true;
        buyButton.interactable = PlayerProgress.Instance.IsEnoughMoney(cost);
    }

    private void OnClickFreeUpgradeButton() {
        player.Upgrade(upgradeType, step);
        UpdateVisual();
    }

    private void OnClickBuyButton() {
        player.BuyUpgrade(upgradeType, step);
        UpdateVisual();
    }
}