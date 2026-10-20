using BreakInfinity;
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
    [SerializeField] private Button rebirthButton;
    [SerializeField] private CaptureButtonUI captureButtonUI;
    [SerializeField] private EquipButton equipButton;
    [SerializeField] private HotBarUI hotBarUI;
    [SerializeField] private TestJumpButton testJumpButton; // for testing

    private Player player;

    private void OnEnable() {
        if (!PlayerProgress.HasInstance) return;
        PlayerProgress.Instance.OnDataChanged += PlayerProgress_OnDataChanged;
        PlayerProgress_OnDataChanged();
    }

    private void Start() {
        indexButton.onClick.AddListener(OnPressIndexButton);
        rebirthButton.onClick.AddListener(OnPressRebirthButton);
    }

    private void OnDisable() {
        if (!PlayerProgress.HasInstance) return;
        PlayerProgress.Instance.OnDataChanged -= PlayerProgress_OnDataChanged;
    }

    public void OnInit(Player player, Camera playerCamera) {
        this.player = player;
        captureButtonUI.OnInit(player, playerCamera);
        equipButton.OnInit(player, playerCamera);
        testJumpButton.OnInit(player);
        hotBarUI.OnInit();
    }

    public void UpdateHotbar() {
        hotBarUI.UpdateVisual();
    }

    private void PlayerProgress_OnDataChanged() {
        moneyText.SetText(Constant.MONEY_SYMBOL + Utils.SciNotToUSName(PlayerProgress.Instance.GetMoney()));
        jumpPowerText.SetText(PlayerProgress.Instance.GetLevel(UpgradeType.JumpPower).ToString());
    }

    private void OnPressIndexButton() {
        UIManager.Instance.OpenUI<CanvasIndex>();
    }

    private void OnPressRebirthButton() {
        CanvasRebirth canvasRebirth = UIManager.Instance.OpenUI<CanvasRebirth>();
        canvasRebirth.OnInit(player);
    }
}