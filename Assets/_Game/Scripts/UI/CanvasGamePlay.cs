using BreakInfinity;
using DG.Tweening;
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
    [SerializeField] private Button skinButton;
    [SerializeField] private Button rebirthButton;
    [SerializeField] private Button settingButton;
    [SerializeField] private DropButtonUI dropButton;
    [SerializeField] private CaptureButtonUI captureButtonUI;
    [SerializeField] private EquipButton equipButton;
    [SerializeField] private UpgradeHouseButtonUI upgradeHouseButtonUI;
    [SerializeField] private HotBarUI hotBarUI;
    [SerializeField] private TestJumpButton testJumpButton; // for testing
    [SerializeField] private MoneyCollectVFX moneyCollectVFX;

    [Header("Spend Money Effect")]
    [SerializeField] private Color spendColor = new Color(1f, 0.29f, 0.29f);
    [SerializeField] private float spendDuration = 0.4f;
    [SerializeField] private float colorBackDuration = 0.2f;

    private Player player;
    private BigDouble flyingMoney; // tien dang bay ve HUD, chua hien thi
    private BigDouble spendingMoney; // tien da tru nhung HUD chua dem lui xong
    private Tween spendTween;
    private Color originalMoneyColor;
    private bool isMoneyColorCached;

    private void OnEnable() {
        if (!isMoneyColorCached) {
            originalMoneyColor = moneyText.color;
            isMoneyColorCached = true;
        }

        if (!PlayerProgress.HasInstance) return;
        PlayerProgress.Instance.OnDataChanged += PlayerProgress_OnDataChanged;
        PlayerProgress.Instance.OnMoneySpent += PlayerProgress_OnMoneySpent;
        PlayerProgress_OnDataChanged();
    }

    private void Start() {
        indexButton.onClick.AddListener(OnClickIndexButton);
        rebirthButton.onClick.AddListener(OnClickRebirthButton);
        skinButton.onClick.AddListener(OnClickSkinButton);
        settingButton.onClick.AddListener(OnClickSettingButton);
    }

    private void OnDisable() {
        flyingMoney = 0f;
        spendingMoney = 0f;
        spendTween?.Kill();
        moneyText.DOKill();
        moneyText.color = originalMoneyColor;

        if (!PlayerProgress.HasInstance) return;
        PlayerProgress.Instance.OnDataChanged -= PlayerProgress_OnDataChanged;
        PlayerProgress.Instance.OnMoneySpent -= PlayerProgress_OnMoneySpent;
    }

    private void OnDestroy() {
        indexButton.onClick.RemoveListener(OnClickIndexButton);
        rebirthButton.onClick.RemoveListener(OnClickRebirthButton);
        skinButton.onClick.RemoveListener(OnClickSkinButton);
        settingButton.onClick.RemoveListener(OnClickSettingButton);
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

    public void PlayCollectMoneyVFX(BigDouble amount) {
        if (!gameObject.activeInHierarchy || moneyCollectVFX == null) return;

        flyingMoney += amount;
        UpdateMoneyText();
        moneyCollectVFX.Play(amount, MoneyCollectVFX_OnCashArrived);
    }

    private void MoneyCollectVFX_OnCashArrived(BigDouble amount) {
        flyingMoney -= amount;
        if (flyingMoney < 0f) {
            flyingMoney = 0f;
        }
        UpdateMoneyText();
    }

    private void PlayerProgress_OnDataChanged() {
        UpdateMoneyText();
        jumpPowerText.SetText(PlayerProgress.Instance.GetLevel(UpgradeType.JumpPower).ToString());
    }

    private void PlayerProgress_OnMoneySpent(BigDouble amount) {
        spendingMoney += amount;
        BigDouble startSpendingMoney = spendingMoney;

        spendTween?.Kill();
        spendTween = DOVirtual.Float(1f, 0f, spendDuration, t => {
            spendingMoney = startSpendingMoney * t;
            UpdateMoneyText();
        }).SetEase(Ease.OutQuad).SetLink(gameObject).OnComplete(() => {
            spendingMoney = 0f;
            UpdateMoneyText();
            moneyText.DOColor(originalMoneyColor, colorBackDuration).SetLink(gameObject);
        });

        moneyText.DOKill();
        moneyText.color = spendColor;
        UpdateMoneyText();
    }

    private void UpdateMoneyText() {
        BigDouble displayMoney = PlayerProgress.Instance.GetMoney() - flyingMoney + spendingMoney;
        if (displayMoney < 0f) {
            displayMoney = 0f;
        }
        moneyText.SetText(Constant.MONEY_SYMBOL + Utils.SciNotToUSName(displayMoney));
    }

    private void OnClickIndexButton() {
        UIManager.Instance.OpenUI<CanvasIndex>();
    }

    private void OnClickRebirthButton() {
        CanvasRebirth canvasRebirth = UIManager.Instance.OpenUI<CanvasRebirth>();
        canvasRebirth.OnInit(player);
    }

    private void OnClickSettingButton() {
        UIManager.Instance.OpenUI<CanvasSetting>();
    }

    private void OnClickSkinButton() {
        CanvasSkin canvasSkin = UIManager.Instance.OpenUI<CanvasSkin>();
        canvasSkin.OnInit(player);
    }
}