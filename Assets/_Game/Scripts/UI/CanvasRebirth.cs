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

    [Header("Header")]
    [SerializeField] private TextMeshProUGUI oldLevelText;
    [SerializeField] private TextMeshProUGUI newLevelText;
    [SerializeField] private GameObject levelArrow;

    [Header("Now")]
    [SerializeField] private TextMeshProUGUI nowMoneyText;
    [SerializeField] private TextMeshProUGUI nowSpeedText;
    [SerializeField] private TextMeshProUGUI nowJumpText;

    [Header("After Rebirth")]
    [SerializeField] private RectTransform afterBox;
    [SerializeField] private TextMeshProUGUI afterTitleText;
    [SerializeField] private TextMeshProUGUI afterMoneyText;
    [SerializeField] private TextMeshProUGUI afterSpeedText;
    [SerializeField] private GameObject afterJumpRow;
    [SerializeField] private GameObject moneyBonusChip;
    [SerializeField] private GameObject speedBonusChip;
    [SerializeField] private TextMeshProUGUI moneyBonusText;
    [SerializeField] private TextMeshProUGUI speedBonusText;
    [SerializeField] private Image afterBurstImage;
    [SerializeField] private Image[] sparkleImages;

    [Header("Goal")]
    [SerializeField] private TextMeshProUGUI goalText;
    [SerializeField] private Image progressFill;
    [SerializeField] private TextMeshProUGUI progressText;

    [Header("Footer")]
    [SerializeField] private TextMeshProUGUI rebirthText;
    [SerializeField] private GameObject rebirthIcon;
    [SerializeField] private GameObject costPill;
    [SerializeField] private TextMeshProUGUI costText;
    [SerializeField] private GameObject rebirthGlow;
    [SerializeField] private Color enableButtonColor = new Color(0.36f, 0.86f, 0.05f, 1f);
    [SerializeField] private Color disableButtonColor = new Color(0.38f, 0.38f, 0.42f, 1f);
    [SerializeField] private Color maxButtonColor = new Color(0.77f, 0.25f, 0.86f, 1f);

    [Header("Feedback")]
    [SerializeField] private float drainTimeMax = 0.5f;
    [SerializeField] private float inflateTimeMax = 0.35f;
    [SerializeField] private float popTimeMax = 0.15f;
    [SerializeField] private float burstTimeMax = 0.35f;
    [SerializeField] private float countTimeMax = 0.6f;
    [SerializeField] private float inflateScale = 1.12f;
    [SerializeField] private float burstScale = 2.2f;
    [SerializeField] private float sparkleSpeed = 3f;
    [SerializeField] private float readyPulseSpeed = 6f;
    [SerializeField] private float readyPulseScale = 0.04f;

    private Player player;
    private bool isPlayingRebirth;
    private bool isReady;

    private void OnEnable() {
        closeButton.onClick.AddListener(OnClickCloseButton);
        blockerButton.onClick.AddListener(OnClickCloseButton);
        rebirthButton.onClick.AddListener(OnClickRebirthButton);
        freeRebirthButton.onClick.AddListener(OnClickFreeRebirthButton);
    }

    private void Update() {
        UpdateSparkles();
        if (isPlayingRebirth) return;

        UpdateVisual();
        UpdateReadyPulse();
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

        int rebirthLevel = PlayerProgress.Instance.GetRebirthLevel();
        int jumpLevel = PlayerProgress.Instance.GetLevel(UpgradeType.JumpPower);

        nowMoneyText.SetText(FormatMultiplier(listRebirthConfigSO.GetMoneyMultiplier(rebirthLevel)));
        nowSpeedText.SetText(FormatValue(player.GetMoveSpeed()));
        nowJumpText.SetText(jumpLevel.ToString());

        if (player.IsMaxRebirth()) {
            UpdateMaxRebirthVisual(rebirthLevel);
            return;
        }

        int requiredJumpLevel = player.GetRebirthRequiredJumpLevel();
        isReady = player.IsReachedRebirth();

        oldLevelText.SetText(Constant.REBIRTH_LEVEL_PREFIX + rebirthLevel);
        newLevelText.SetText((rebirthLevel + 1).ToString());
        levelArrow.SetActive(true);
        newLevelText.gameObject.SetActive(true);

        afterTitleText.SetText(Constant.AFTER_REBIRTH_TITLE);
        afterJumpRow.SetActive(true);
        UpdateRewardVisual(rebirthLevel);

        progressFill.fillAmount = Mathf.Clamp01((float)jumpLevel / requiredJumpLevel);
        progressText.SetText(jumpLevel + " / " + requiredJumpLevel);
        goalText.SetText(isReady ? Constant.REBIRTH_GOAL_READY : Constant.REBIRTH_GOAL_PREFIX + requiredJumpLevel + Constant.REBIRTH_GOAL_SUFFIX);

        rebirthButton.interactable = true;
        freeRebirthButton.interactable = true;
        freeRebirthButton.gameObject.SetActive(true);
        rebirthIcon.SetActive(true);

        BigDouble cost = player.GetRebirthCost();
        costPill.SetActive(!isReady);
        if (isReady) {
            rebirthText.SetText(Constant.REBIRTH_READY_TEXT);
        }
        else {
            rebirthText.SetText(Constant.REBIRTH_TEXT);
            costText.SetText(Constant.MONEY_SYMBOL + Utils.SciNotToUSName(cost));
        }

        SetRebirthButtonColor(PlayerProgress.Instance.IsEnoughMoney(cost) ? enableButtonColor : disableButtonColor);
    }

    private void UpdateRewardVisual(int rebirthLevel) {
        float oldMoney = listRebirthConfigSO.GetMoneyMultiplier(rebirthLevel);
        float newMoney = listRebirthConfigSO.GetMoneyMultiplier(rebirthLevel + 1);
        afterMoneyText.SetText(FormatMultiplier(newMoney));
        moneyBonusText.SetText(Constant.BONUS_PREFIX + FormatValue(newMoney - oldMoney) + Constant.MULTIPLIER_PREFIX);
        moneyBonusChip.SetActive(true);

        float oldSpeed = player.GetMoveSpeed();
        float newSpeed = GetNextMoveSpeed(rebirthLevel);
        afterSpeedText.SetText(FormatValue(newSpeed));
        speedBonusText.SetText(Constant.BONUS_PREFIX + FormatValue(newSpeed - oldSpeed));
        speedBonusChip.SetActive(true);
    }

    private void UpdateMaxRebirthVisual(int rebirthLevel) {
        isReady = false;

        oldLevelText.SetText(Constant.REBIRTH_LEVEL_PREFIX + rebirthLevel);
        levelArrow.SetActive(false);
        newLevelText.gameObject.SetActive(false);

        // max thi o phai hien gia tri hien tai, an chip bonus va dong jump reset
        afterTitleText.SetText(Constant.MAX_REBIRTH_TITLE);
        afterMoneyText.SetText(nowMoneyText.text);
        afterSpeedText.SetText(nowSpeedText.text);
        afterJumpRow.SetActive(false);
        moneyBonusChip.SetActive(false);
        speedBonusChip.SetActive(false);

        progressFill.fillAmount = 1f;
        progressText.SetText(Constant.MAX_REBIRTH_TEXT);
        goalText.SetText(Constant.MAX_REBIRTH_GOAL);

        rebirthText.SetText(Constant.MAX_REBIRTH_TEXT);
        rebirthIcon.SetActive(false);
        costPill.SetActive(false);
        rebirthButton.interactable = false;
        freeRebirthButton.interactable = false;
        freeRebirthButton.gameObject.SetActive(false);
        SetRebirthButtonColor(maxButtonColor);
    }

    private void SetRebirthButtonColor(Color color) {
        rebirthButton.targetGraphic.color = color;
        rebirthGlow.SetActive(color == enableButtonColor);
    }

    private float GetNextMoveSpeed(int rebirthLevel) {
        return player.GetMoveSpeed() - listRebirthConfigSO.GetMoveSpeedBonus(rebirthLevel) + listRebirthConfigSO.GetMoveSpeedBonus(rebirthLevel + 1);
    }

    private string FormatMultiplier(float value) {
        return Constant.MULTIPLIER_PREFIX + FormatValue(value);
    }

    private string FormatValue(float value) {
        return value.ToString("0.##");
    }

    // tia lap lanh quanh o After Rebirth nhap nhay lech pha nhau
    private void UpdateSparkles() {
        for (int i = 0; i < sparkleImages.Length; i++) {
            float value = (Mathf.Sin(Time.unscaledTime * sparkleSpeed + i * 2.1f) + 1f) * 0.5f;
            Color color = sparkleImages[i].color;
            color.a = Mathf.Lerp(0.2f, 1f, value);
            sparkleImages[i].color = color;
            sparkleImages[i].rectTransform.localScale = Vector3.one * Mathf.Lerp(0.7f, 1.1f, value);
        }
    }

    // du dieu kien thi nut rebirth nay nhe
    private void UpdateReadyPulse() {
        float scale = isReady ? 1f + Mathf.Abs(Mathf.Sin(Time.unscaledTime * readyPulseSpeed)) * readyPulseScale : 1f;
        rebirthButton.transform.localScale = Vector3.one * scale;
    }

    private void TryRebirth(bool isFree) {
        if (isPlayingRebirth) return;

        int oldRebirthLevel = PlayerProgress.Instance.GetRebirthLevel();
        int oldJumpLevel = PlayerProgress.Instance.GetLevel(UpgradeType.JumpPower);
        float oldSpeed = player.GetMoveSpeed();
        float oldFill = progressFill.fillAmount;

        if (isFree) {
            player.Rebirth();
        }
        else {
            player.BuyRebirth();
        }

        if (PlayerProgress.Instance.GetRebirthLevel() == oldRebirthLevel) return; // khong du tien

        StartCoroutine(IERebirth(oldRebirthLevel, oldJumpLevel, oldSpeed, oldFill));
    }

    private IEnumerator IERebirth(int oldRebirthLevel, int oldJumpLevel, float oldSpeed, float oldFill) {
        isPlayingRebirth = true;
        rebirthButton.interactable = false;
        freeRebirthButton.interactable = false;
        rebirthButton.transform.localScale = Vector3.one;

        float oldMoney = listRebirthConfigSO.GetMoneyMultiplier(oldRebirthLevel);
        float newMoney = listRebirthConfigSO.GetMoneyMultiplier(oldRebirthLevel + 1);
        float newSpeed = player.GetMoveSpeed();

        // progress bar tut dan ve 0
        yield return IETween(drainTimeMax, t => progressFill.fillAmount = Mathf.Lerp(oldFill, 0f, EaseOut(t)));

        // o After Rebirth phinh ra
        yield return IETween(inflateTimeMax, t => afterBox.localScale = Vector3.one * Mathf.Lerp(1f, inflateScale, t * t));

        // no bup: o co lai + vong burst toa ra
        StartCoroutine(IEBurst(afterBurstImage));
        yield return IETween(popTimeMax, t => afterBox.localScale = Vector3.one * Mathf.Lerp(inflateScale, 1f, EaseOut(t)));

        // so o Now chay dan sang gia tri moi
        yield return IETween(countTimeMax, t => {
            float value = EaseOut(t);
            nowMoneyText.SetText(FormatMultiplier(Mathf.Lerp(oldMoney, newMoney, value)));
            nowSpeedText.SetText(FormatValue(Mathf.Lerp(oldSpeed, newSpeed, value)));
            nowJumpText.SetText(Mathf.RoundToInt(Mathf.Lerp(oldJumpLevel, 0f, value)).ToString());
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

    private void ResetFeedbackVisual() {
        isPlayingRebirth = false;
        afterBox.localScale = Vector3.one;
        rebirthButton.transform.localScale = Vector3.one;
        afterBurstImage.gameObject.SetActive(false);
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
