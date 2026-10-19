using BreakInfinity;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CanvasRebirth : UICanvas {
    [SerializeField] private ListRebirthConfigSO listRebirthConfigSO;
    [SerializeField] private Button closeButton;
    [SerializeField] private Button rebirthButton;
    [SerializeField] private Button freeRebirthButton;
    [SerializeField] private TextMeshProUGUI rebirthText;
    [SerializeField] private TextMeshProUGUI titleText;
    [SerializeField] private TextMeshProUGUI oldMoneyText;
    [SerializeField] private TextMeshProUGUI newMoneyText;
    [SerializeField] private TextMeshProUGUI oldSpeedText;
    [SerializeField] private TextMeshProUGUI newSpeedText;
    [SerializeField] private Image progressFill;
    [SerializeField] private TextMeshProUGUI progressText;

    private Player player;

    private void OnEnable() {
        closeButton.onClick.AddListener(OnClickCloseButton);
        rebirthButton.onClick.AddListener(OnClickRebirthButton);
        freeRebirthButton.onClick.AddListener(OnClickFreeRebirthButton);
    }

    private void Update() {
        UpdateVisual();
    }

    private void OnDisable() {
        closeButton.onClick.RemoveListener(OnClickCloseButton);
        rebirthButton.onClick.RemoveListener(OnClickRebirthButton);
        freeRebirthButton.onClick.RemoveListener(OnClickFreeRebirthButton);
    }

    public void OnInit(Player player) {
        this.player = player;
        UpdateVisual();
    }

    public void UpdateVisual() {
        if (player == null) return;

        if (player.IsMaxRebirth()) {
            UpdateMaxRebirthVisual();
            return;
        }

        int rebirthLevel = PlayerProgress.Instance.GetRebirthLevel();
        int jumpLevel = PlayerProgress.Instance.GetLevel(UpgradeType.JumpPower);
        int requiredJumpLevel = player.GetRebirthRequiredJumpLevel();

        titleText.SetText(Constant.REBIRTH_TITLE_PREFIX + (rebirthLevel + 1) + Constant.REBIRTH_TITLE_SUFFIX);
        UpdateRewardVisual(rebirthLevel);

        progressFill.fillAmount = Mathf.Clamp01((float)jumpLevel / requiredJumpLevel);
        progressText.SetText(Constant.JUMP_POWER_PREFIX + jumpLevel + "/" + requiredJumpLevel);

        rebirthButton.interactable = true;
        freeRebirthButton.interactable = true;

        if (player.IsReachedRebirth()) {
            rebirthText.SetText(Constant.REBIRTH_TEXT);
        }
        else {
            BigDouble cost = player.GetRebirthCost();
            rebirthText.SetText(Constant.MONEY_SYMBOL + Utils.SciNotToUSName(cost));
        }
    }

    private void UpdateRewardVisual(int rebirthLevel) {
        float oldMultiplier = listRebirthConfigSO.GetMoneyMultiplier(rebirthLevel);
        float newMultiplier = listRebirthConfigSO.GetMoneyMultiplier(rebirthLevel + 1);
        oldMoneyText.SetText(oldMultiplier.ToString("0.##"));
        newMoneyText.SetText(newMultiplier.ToString("0.##") + Constant.MONEY_REWARD_SUFFIX);

        // speed hien tai da gom bonus cua rebirth hien tai, doi sang bonus cua moc tiep theo
        float oldSpeed = player.GetMoveSpeed();
        float newSpeed = oldSpeed - listRebirthConfigSO.GetMoveSpeedBonus(rebirthLevel) + listRebirthConfigSO.GetMoveSpeedBonus(rebirthLevel + 1);
        oldSpeedText.SetText(oldSpeed.ToString("0.##"));
        newSpeedText.SetText(newSpeed.ToString("0.##") + Constant.SPEED_REWARD_SUFFIX);
    }

    private void UpdateMaxRebirthVisual() {
        titleText.SetText(Constant.MAX_REBIRTH_TITLE);
        progressFill.fillAmount = 1f;
        progressText.SetText(Constant.MAX_REBIRTH_TEXT);
        rebirthText.SetText(Constant.MAX_REBIRTH_TEXT);
        rebirthButton.interactable = false;
        freeRebirthButton.interactable = false;
    }

    private void OnClickRebirthButton() {
        player.BuyRebirth();
        UpdateVisual();
    }

    // tam thoi rebirth luon, sau nay doi sang xem du so quang cao
    private void OnClickFreeRebirthButton() {
        player.Rebirth();
        UpdateVisual();
    }

    private void OnClickCloseButton() {
        Close(0f);
    }
}
