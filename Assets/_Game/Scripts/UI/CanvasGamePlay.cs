using BreakInfinity;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class CanvasGamePlay : UICanvas {
    [SerializeField] private TextMeshProUGUI moneyText;
    [SerializeField] private TextMeshProUGUI jumpPowerText;
    [SerializeField] private Button indexButton;
    [SerializeField] private Button sellButton;
    [SerializeField] private Button rebirthButton;
    [SerializeField] private Button jumpUpgradeButton;
    [SerializeField] private Button settingButton;
    [SerializeField] private DropButtonUI dropButton;
    [SerializeField] private CaptureButtonUI captureButtonUI;
    [SerializeField] private EquipButton equipButton;
    [SerializeField] private UpgradeHouseButtonUI upgradeHouseButtonUI;
    [SerializeField] private HotBarUI hotBarUI;
    [SerializeField] private TestJumpButton testJumpButton; // for testing

    private Player player;

    private void OnEnable() {
        if (!PlayerProgress.HasInstance) return;
        PlayerProgress.Instance.OnDataChanged += PlayerProgress_OnDataChanged;
        PlayerProgress_OnDataChanged();
    }

    private void Start() {
        indexButton.onClick.AddListener(OnClickIndexButton);
        rebirthButton.onClick.AddListener(OnClickRebirthButton);
        sellButton.onClick.AddListener(OnClickSellButton);
        settingButton.onClick.AddListener(OnClickSettingButton);
        jumpUpgradeButton.onClick.AddListener(OnCLickJumpUpgradeButton);
    }

    private void OnDisable() {
        if (!PlayerProgress.HasInstance) return;
        PlayerProgress.Instance.OnDataChanged -= PlayerProgress_OnDataChanged;
    }

    private void OnDestroy() {
        indexButton.onClick.RemoveListener(OnClickIndexButton);
        rebirthButton.onClick.RemoveListener(OnClickRebirthButton);
        sellButton.onClick.RemoveListener(OnClickSellButton);
        settingButton.onClick.RemoveListener(OnClickSettingButton);
        jumpUpgradeButton.onClick.RemoveListener(OnCLickJumpUpgradeButton);
    }

    public void OnInit(Player player, Camera playerCamera) {
        this.player = player;
        captureButtonUI.OnInit(player, playerCamera);
        equipButton.OnInit(player, playerCamera);
        upgradeHouseButtonUI.OnInit(player, playerCamera);
        testJumpButton.OnInit(player);
        dropButton.OnInit(player, playerCamera);
        hotBarUI.OnInit();
    }

    public void UpdateHotbar() {
        hotBarUI.UpdateVisual();
    }

    private void PlayerProgress_OnDataChanged() {
        moneyText.SetText(Constant.MONEY_SYMBOL + Utils.SciNotToUSName(PlayerProgress.Instance.GetMoney()));
        jumpPowerText.SetText(PlayerProgress.Instance.GetLevel(UpgradeType.JumpPower).ToString());
    }

    private void OnClickIndexButton() {
        UIManager.Instance.OpenUI<CanvasIndex>();
    }

    private void OnClickSellButton() {
        UIManager.Instance.OpenUI<CanvasSell>();
    }

    private void OnClickRebirthButton() {
        CanvasRebirth canvasRebirth = UIManager.Instance.OpenUI<CanvasRebirth>();
        canvasRebirth.OnInit(player);
    }

    private void OnClickSettingButton() {
        UIManager.Instance.OpenUI<CanvasSetting>();
    }

    private void OnCLickJumpUpgradeButton() {
        CanvasUpgradeJump canvasUpgradeJump = UIManager.Instance.OpenUI<CanvasUpgradeJump>();
        canvasUpgradeJump.OnInit(player);
    }
}