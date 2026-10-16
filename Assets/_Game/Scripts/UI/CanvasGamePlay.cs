using BreakInfinity;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class CanvasGamePlay : UICanvas {
    [SerializeField] private TextMeshProUGUI moneyText;
    [SerializeField] private Button indexButton;
    [SerializeField] private CaptureButtonUI captureButtonUI;
    [SerializeField] private EquipButton equipButton;
    [SerializeField] private HotBarUI hotBarUI;
    [SerializeField] private TestJumpButton testJumpButton; // for testing
    [SerializeField] private TestUpgradeButton testUpgradeButton; // for testing

    private string moneyNotated;

    private void Start() {
        indexButton.onClick.AddListener(OnPressIndexButton);
    }

    private void Update() {
        UpdateVisual();
    }

    public void OnInit(Player player, Camera playerCamera) {
        captureButtonUI.OnInit(player, playerCamera);
        equipButton.OnInit(player, playerCamera);
        testJumpButton.OnInit(player);
        testUpgradeButton.OnInit(player);
        hotBarUI.OnInit();
    }

    public void UpdateHotbar() {
        hotBarUI.UpdateVisual();
    }

    private void UpdateVisual() {
        BigDouble totalMoney = PlayerProgress.Instance.GetMoney();
        moneyNotated = Utils.SciNotToUSName(totalMoney);
        moneyText.SetText(Constant.MONEY_SYMBOL + moneyNotated);
    }

    private void OnPressIndexButton() {
        UIManager.Instance.OpenUI<CanvasIndex>();
    }
}