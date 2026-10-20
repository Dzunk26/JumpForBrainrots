using BreakInfinity;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CanvasRebirth : UICanvas {
    [SerializeField] private ListRebirthConfigSO listRebirthConfigSO;
    [SerializeField] private Button closeButton;
    [SerializeField] private Button blockerButton;
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

    [SerializeField] private Color enableButtonColor = Color.white;
    [SerializeField] private Color disableButtonColor = new Color(0.45f, 0.45f, 0.45f, 1f);
    [SerializeField] private RectTransform moneyReward;
    [SerializeField] private RectTransform speedReward;
    [SerializeField] private Image moneyBurstImage;
    [SerializeField] private Image speedBurstImage;
    [SerializeField] private float drainTimeMax = 0.5f;
    [SerializeField] private float inflateTimeMax = 0.35f;
    [SerializeField] private float popTimeMax = 0.15f;
    [SerializeField] private float burstTimeMax = 0.35f;
    [SerializeField] private float countTimeMax = 0.6f;
    [SerializeField] private float inflateScale = 1.3f;
    [SerializeField] private float burstScale = 2.2f;

    private Player player;
    private bool isPlayingRebirth;

    private void OnEnable() {
        closeButton.onClick.AddListener(OnClickCloseButton);
        blockerButton.onClick.AddListener(OnClickCloseButton);
        rebirthButton.onClick.AddListener(OnClickRebirthButton);
        freeRebirthButton.onClick.AddListener(OnClickFreeRebirthButton);
    }

    private void Update() {
        if (isPlayingRebirth) return;

        UpdateVisual();
    }

    private void OnDisable() {
        closeButton.onClick.RemoveListener(OnClickCloseButton);
        blockerButton.onClick.RemoveListener(OnClickCloseButton);
        rebirthButton.onClick.RemoveListener(OnClickRebirthButton);
        freeRebirthButton.onClick.RemoveListener(OnClickFreeRebirthButton);

        StopAllCoroutines();
        ResetFeedbackVisual();
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

        BigDouble cost = player.GetRebirthCost();
        if (player.IsReachedRebirth()) {
            rebirthText.SetText(Constant.REBIRTH_TEXT);
        }
        else {
            rebirthText.SetText(Constant.MONEY_SYMBOL + Utils.SciNotToUSName(cost));
        }

        SetRebirthButtonColor(PlayerProgress.Instance.IsEnoughMoney(cost));
    }

    private void UpdateRewardVisual(int rebirthLevel) {
        float oldMultiplier = listRebirthConfigSO.GetMoneyMultiplier(rebirthLevel);
        float newMultiplier = listRebirthConfigSO.GetMoneyMultiplier(rebirthLevel + 1);
        oldMoneyText.SetText(oldMultiplier.ToString("0.##"));
        newMoneyText.SetText(newMultiplier.ToString("0.##") + Constant.MONEY_REWARD_SUFFIX);

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
        SetRebirthButtonColor(false);
    }

    private void SetRebirthButtonColor(bool canRebirth) {
        Color color = canRebirth ? enableButtonColor : disableButtonColor;
        rebirthButton.targetGraphic.color = color;
        rebirthText.color = color;
    }

    private void TryRebirth(bool isFree) {
        if (isPlayingRebirth) return;

        int oldRebirthLevel = PlayerProgress.Instance.GetRebirthLevel();
        float oldSpeed = player.GetMoveSpeed();
        float oldFill = progressFill.fillAmount;

        if (isFree) {
            player.Rebirth();
        }
        else {
            player.BuyRebirth();
        }

        if (PlayerProgress.Instance.GetRebirthLevel() == oldRebirthLevel) return; // khong du tien

        StartCoroutine(IERebirth(oldRebirthLevel, oldSpeed, oldFill));
    }

    private IEnumerator IERebirth(int oldRebirthLevel, float oldSpeed, float oldFill) {
        isPlayingRebirth = true;
        rebirthButton.interactable = false;
        freeRebirthButton.interactable = false;

        float oldMoney = listRebirthConfigSO.GetMoneyMultiplier(oldRebirthLevel);
        float newMoney = listRebirthConfigSO.GetMoneyMultiplier(oldRebirthLevel + 1);
        float newSpeed = player.GetMoveSpeed();

        // progress bar tut dan ve 0
        yield return IETween(drainTimeMax, t => progressFill.fillAmount = Mathf.Lerp(oldFill, 0f, EaseOut(t)));

        // reward phinh ra
        yield return IETween(inflateTimeMax, t => SetRewardScale(Mathf.Lerp(1f, inflateScale, t * t)));

        // no bup: reward co lai + vong burst toa ra
        StartCoroutine(IEBurst(moneyBurstImage));
        StartCoroutine(IEBurst(speedBurstImage));
        yield return IETween(popTimeMax, t => SetRewardScale(Mathf.Lerp(inflateScale, 1f, EaseOut(t))));

        // so reward chay tang dan
        yield return IETween(countTimeMax, t => {
            float value = EaseOut(t);
            oldMoneyText.SetText(Mathf.Lerp(oldMoney, newMoney, value).ToString("0.##"));
            oldSpeedText.SetText(Mathf.Lerp(oldSpeed, newSpeed, value).ToString("0.##"));
        });

        isPlayingRebirth = false;
        UpdateVisual();
    }

    private IEnumerator IEBurst(Image burstImage) {
        burstImage.gameObject.SetActive(true);
        Color color = burstImage.color;

        yield return IETween(burstTimeMax, t => {
            burstImage.rectTransform.localScale = Vector3.one * Mathf.Lerp(0.5f, burstScale, EaseOut(t));
            color.a = 1f - t;
            burstImage.color = color;
        });

        burstImage.gameObject.SetActive(false);
    }

    private IEnumerator IETween(float timeMax, System.Action<float> onUpdate) {
        for (float timer = 0f; timer < timeMax; timer += Time.unscaledDeltaTime) {
            onUpdate(timer / timeMax);
            yield return null;
        }

        onUpdate(1f);
    }

    private float EaseOut(float t) {
        return 1f - (1f - t) * (1f - t);
    }

    private void SetRewardScale(float scale) {
        moneyReward.localScale = Vector3.one * scale;
        speedReward.localScale = Vector3.one * scale;
    }

    private void ResetFeedbackVisual() {
        isPlayingRebirth = false;
        SetRewardScale(1f);
        moneyBurstImage.gameObject.SetActive(false);
        speedBurstImage.gameObject.SetActive(false);
    }

    private void OnClickRebirthButton() {
        TryRebirth(false);
    }

    // tam thoi rebirth luon, sau nay doi sang xem du so quang cao
    private void OnClickFreeRebirthButton() {
        TryRebirth(true);
    }

    private void OnClickCloseButton() {
        Close(0f);
    }
}
