using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.UIElements;

public class CanvasUpgradeJump : UICanvas {
    [SerializeField] private UnityEngine.UI.Button closeButton;
    [SerializeField] private UnityEngine.UI.Button blockerButton;
    [SerializeField] private UpgradeStepUI[] upgradeStepUIs;
    [SerializeField] private ProgressBarVisual progressBarVisual;
    [SerializeField] private TextMeshProUGUI progressText;

    private Player player;

    private void OnEnable() {
        closeButton.onClick.AddListener(OnClickCloseButton);
        blockerButton.onClick.AddListener(OnClickCloseButton);
        UIManager.Instance.GetUI<CanvasGamePlay>().ShowOnly(HudIcon.Money, HudIcon.JumpPower);
    }

    private void Update() {
        UpdateVisual();
    }

    private void OnDisable() {
        closeButton.onClick.RemoveListener(OnClickCloseButton);
        blockerButton.onClick.RemoveListener(OnClickCloseButton);
        if (UIManager.Instance.IsUIOpen<CanvasGamePlay>())
            UIManager.Instance.GetUI<CanvasGamePlay>().ShowAll();
    }

    public void OnInit(Player player) {
        this.player = player;

        for (int i = 0; i < upgradeStepUIs.Length; i++) {
            upgradeStepUIs[i].OnInit(player);
        }

        UpdateVisual();
        progressBarVisual.SnapFill();
    }

    public void UpdateVisual() {
        if (player == null) return;

        int jumpLevel = PlayerProgress.Instance.GetLevel(UpgradeType.JumpPower);
        progressText.SetText(jumpLevel.ToString());
        progressBarVisual.SetFill(player.IsMaxRebirth() ? 1f : (float)jumpLevel / player.GetRebirthRequiredJumpLevel());

        for (int i = 0; i < upgradeStepUIs.Length; i++) {
            upgradeStepUIs[i].UpdateVisual();
        }
    }

    private void OnClickCloseButton() {
        Close(0f);
    }
}