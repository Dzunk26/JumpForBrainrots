using BreakInfinity;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.UIElements;

public class CanvasRebirth : UICanvas {
    [SerializeField] private ListRebirthConfigSO listRebirthConfigSO;
    [SerializeField] private UnityEngine.UI.Button closeButton;
    [SerializeField] private UnityEngine.UI.Button blockerButton;
    [SerializeField] private UnityEngine.UI.Button rebirthButton;
    [SerializeField] private UnityEngine.UI.Button freeRebirthButton;

    [Header("Header")]
    [SerializeField] private TextMeshProUGUI nowBoxTittleText;
    [SerializeField] private TextMeshProUGUI afterBoxTittleText;

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
    [SerializeField] private UnityEngine.UI.Image afterBurstImage;

    [Header("Goal")]
    [SerializeField] private TextMeshProUGUI goalText;
    [SerializeField] private ProgressBarVisual progressBarVisual;
    [SerializeField] private TextMeshProUGUI progressText;

    [Header("Footer")]
    [SerializeField] private TextMeshProUGUI rebirthText;
    [SerializeField] private GameObject rebirthIcon;
    [SerializeField] private GameObject costPill;
    [SerializeField] private TextMeshProUGUI costText;
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
        progressBarVisual.SnapFill();
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

        nowBoxTittleText.SetText(Constant.NOW_TITTLE_TEXT + " " + Constant.REBIRTH_LEVEL_PREFIX + rebirthLevel);
        afterBoxTittleText.SetText(Constant.AFFTER_REBIRTH_TITTLE_TEXT + " " + Constant.REBIRTH_LEVEL_PREFIX + rebirthLevel);
        afterBoxTittleText.gameObject.SetActive(true);

        afterTitleText.SetText(Constant.AFTER_REBIRTH_TITLE);
        afterJumpRow.SetActive(true);
        UpdateRewardVisual(rebirthLevel);

        progressBarVisual.SetFill((float)jumpLevel / requiredJumpLevel);
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

        nowBoxTittleText.SetText(Constant.NOW_TITTLE_TEXT + " " + Constant.REBIRTH_LEVEL_PREFIX + rebirthLevel);
        afterBoxTittleText.gameObject.SetActive(false);

        afterTitleText.SetText(Constant.MAX_REBIRTH_TITLE);
        afterMoneyText.SetText(nowMoneyText.text);
        afterSpeedText.SetText(nowSpeedText.text);
        afterJumpRow.SetActive(false);
        moneyBonusChip.SetActive(false);
        speedBonusChip.SetActive(false);

        progressBarVisual.SetFill(1f);
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

    private void UpdateReadyPulse() {
        float scale = isReady ? 1f + Mathf.Abs(Mathf.Sin(Time.unscaledTime * readyPulseSpeed)) * readyPulseScale : 1f;
        rebirthButton.transform.localScale = Vector3.one * scale;
    }

    private void TryRebirth(bool isFree) {
        if (isPlayingRebirth) return;

        int oldRebirthLevel = PlayerProgress.Instance.GetRebirthLevel();
        int oldJumpLevel = PlayerProgress.Instance.GetLevel(UpgradeType.JumpPower);
        float oldSpeed = player.GetMoveSpeed();

        if (isFree) {
            player.Rebirth();
        }
        else {
            player.BuyRebirth();
        }

        if (PlayerProgress.Instance.GetRebirthLevel() == oldRebirthLevel) return; // khong du tien

        StartCoroutine(IERebirth(oldRebirthLevel, oldJumpLevel, oldSpeed));
    }

    private IEnumerator IERebirth(int oldRebirthLevel, int oldJumpLevel, float oldSpeed) {
        isPlayingRebirth = true;
        rebirthButton.interactable = false;
        freeRebirthButton.interactable = false;
        rebirthButton.transform.localScale = Vector3.one;

        int newRebirthLevel = oldRebirthLevel + 1;
        float oldMoney = listRebirthConfigSO.GetMoneyMultiplier(oldRebirthLevel);
        float newMoney = listRebirthConfigSO.GetMoneyMultiplier(newRebirthLevel);
        float newSpeed = player.GetMoveSpeed();

        bool isMax = player.IsMaxRebirth();
        float nextMoney = isMax ? newMoney : listRebirthConfigSO.GetMoneyMultiplier(newRebirthLevel + 1);
        float nextSpeed = isMax ? newSpeed : GetNextMoveSpeed(newRebirthLevel);

        progressBarVisual.SetFill(0f);
        yield return new WaitUntil(() => !progressBarVisual.IsAnimating);
        
        yield return IETween(inflateTimeMax, t => afterBox.localScale = Vector3.one * Mathf.Lerp(1f, inflateScale, t * t));

        StartCoroutine(IEBurst(afterBurstImage));
        yield return IETween(popTimeMax, t => afterBox.localScale = Vector3.one * Mathf.Lerp(inflateScale, 1f, EaseOut(t)));

        yield return IETween(countTimeMax, t => {
            float value = EaseOut(t);
            nowMoneyText.SetText(FormatMultiplier(Mathf.Lerp(oldMoney, newMoney, value)));
            nowSpeedText.SetText(FormatValue(Mathf.Lerp(oldSpeed, newSpeed, value)));
            nowJumpText.SetText(Mathf.RoundToInt(Mathf.Lerp(oldJumpLevel, 0f, value)).ToString());

            afterMoneyText.SetText(FormatMultiplier(Mathf.Lerp(newMoney, nextMoney, value)));
            afterSpeedText.SetText(FormatValue(Mathf.Lerp(newSpeed, nextSpeed, value)));
        });

        isPlayingRebirth = false;
        UpdateVisual();
    }

    private IEnumerator IEBurst(UnityEngine.UI.Image burstImage) {
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

    private void OnClickFreeRebirthButton() {
        TryRebirth(true);
    }

    private void OnClickCloseButton() {
        Close(0f);
    }
}
