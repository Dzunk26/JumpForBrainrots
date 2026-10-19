using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CanvasUpgradeJump : UICanvas {
    [SerializeField] private Button closeButton;
    [SerializeField] private UpgradeStepUI[] upgradeStepUIs;
    [SerializeField] private Image progressFill;
    [SerializeField] private TextMeshProUGUI progressText;

    private Player player;

    private void OnEnable() {
        closeButton.onClick.AddListener(OnClickCloseButton);
    }

    private void Update() {
        UpdateVisual();
    }

    private void OnDisable() {
        closeButton.onClick.RemoveListener(OnClickCloseButton);
    }

    public void OnInit(Player player) {
        this.player = player;

        for (int i = 0; i < upgradeStepUIs.Length; i++) {
            upgradeStepUIs[i].OnInit(player);
        }

        UpdateVisual();
    }

    public void UpdateVisual() {
        if (player == null) return;

        int jumpLevel = PlayerProgress.Instance.GetLevel(UpgradeType.JumpPower);
        progressText.SetText(jumpLevel.ToString());
        progressFill.fillAmount = player.IsMaxRebirth() ? 1f : Mathf.Clamp01((float)jumpLevel / player.GetRebirthRequiredJumpLevel());

        for (int i = 0; i < upgradeStepUIs.Length; i++) {
            upgradeStepUIs[i].UpdateVisual();
        }
    }

    private void OnClickCloseButton() {
        Close(0f);
    }
}