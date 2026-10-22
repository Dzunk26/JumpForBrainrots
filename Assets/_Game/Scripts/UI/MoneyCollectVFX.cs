using BreakInfinity;
using DG.Tweening;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class MoneyCollectVFX : MonoBehaviour {
    [Header("References")]
    [SerializeField] private RectTransform container;            // cac cuc tien spawn vao day (de len tren cung)
    [SerializeField] private TextMeshProUGUI moneyText;          // dich bay ve + template cho floating text
    [SerializeField] private Sprite moneySprite;

    [Header("Cash")]
    [SerializeField] private int cashCount = 8;
    [SerializeField] private Vector2 cashSize = new Vector2(120f, 98f);
    [SerializeField] private Vector2 cashScaleRange = new Vector2(0.7f, 1.5f);
    [SerializeField] private Vector2 spawnCenterOffset = Vector2.zero;
    [SerializeField] private Vector2 spawnRadius = new Vector2(380f, 220f);
    [SerializeField] private float spawnMinRadiusRatio = 0.5f;
    [SerializeField] private float maxRotation = 25f;

    [Header("Timing")]
    [SerializeField] private float spawnInterval = 0.035f;
    [SerializeField] private float popDuration = 0.35f;
    [SerializeField] private float idleDuration = 0.25f;
    [SerializeField] private float flyInterval = 0.07f;
    [SerializeField] private float flyDuration = 0.45f;
    [SerializeField] private float flyEndScaleRatio = 0.4f;
    [SerializeField] private float punchScale = 0.15f;

    [Header("Floating Text")]
    [SerializeField] private bool showFloatingText = true;
    [SerializeField] private Vector2 floatingTextOffset = new Vector2(0f, 200f);
    [SerializeField] private float floatingTextFontSize = 90f;
    [SerializeField] private Color floatingTextColor = new Color(0.55f, 1f, 0.3f);
    [SerializeField] private float floatingTextMoveY = 90f;
    [SerializeField] private float floatingTextDuration = 1.3f;

    private readonly List<RectTransform> cashPool = new List<RectTransform>();
    private readonly List<CanvasGroup> textPool = new List<CanvasGroup>();
    private readonly List<RectTransform> activeCashes = new List<RectTransform>();
    private readonly List<CanvasGroup> activeTexts = new List<CanvasGroup>();

    private void OnDisable() {
        Clear();
    }

    public void Play(BigDouble amount, Action<BigDouble> onCashArrived) {
        if (amount <= 0f || cashCount <= 0) return;

        Vector2 center = spawnCenterOffset;

        if (showFloatingText) {
            PlayFloatingText(amount, center + floatingTextOffset);
        }

        BigDouble amountPerCash = amount / cashCount;
        BigDouble remainAmount = amount;

        for (int i = 0; i < cashCount; i++) {
            bool isLast = i == cashCount - 1;
            BigDouble cashAmount = isLast ? remainAmount : amountPerCash;
            remainAmount -= amountPerCash;

            PlayCash(i, center, () => onCashArrived?.Invoke(cashAmount));
        }
    }

    public void Clear() {
        for (int i = 0; i < activeCashes.Count; i++) {
            activeCashes[i].DOKill();
            Despawn(activeCashes[i]);
        }
        activeCashes.Clear();

        for (int i = 0; i < activeTexts.Count; i++) {
            activeTexts[i].DOKill();
            activeTexts[i].transform.DOKill();
            activeTexts[i].gameObject.SetActive(false);
            textPool.Add(activeTexts[i]);
        }
        activeTexts.Clear();

        if (moneyText != null) {
            moneyText.rectTransform.DOKill(true);
        }
    }

    private void PlayCash(int index, Vector2 center, Action onArrived) {
        RectTransform cash = SpawnCash();

        float angle = (index / (float)cashCount) * Mathf.PI * 2f + UnityEngine.Random.Range(0f, 0.5f);
        float radius = UnityEngine.Random.Range(spawnMinRadiusRatio, 1f);
        Vector2 spreadPos = center + new Vector2(Mathf.Cos(angle) * spawnRadius.x, Mathf.Sin(angle) * spawnRadius.y) * radius;
        float scale = UnityEngine.Random.Range(cashScaleRange.x, cashScaleRange.y);
        float rotation = UnityEngine.Random.Range(-maxRotation, maxRotation);

        cash.anchoredPosition = center;
        cash.localScale = Vector3.zero;
        cash.localRotation = Quaternion.identity;

        float spawnDelay = index * spawnInterval;
        float flyDelay = spawnDelay + popDuration + idleDuration + index * flyInterval;

        Sequence sequence = DOTween.Sequence().SetTarget(cash).SetLink(cash.gameObject);
        sequence.Insert(spawnDelay, cash.DOAnchorPos(spreadPos, popDuration).SetEase(Ease.OutCubic));
        sequence.Insert(spawnDelay, cash.DOScale(scale, popDuration).SetEase(Ease.OutBack));
        sequence.Insert(spawnDelay, cash.DOLocalRotate(new Vector3(0f, 0f, rotation), popDuration).SetEase(Ease.OutCubic));
        sequence.InsertCallback(flyDelay, () => {
            // lay vi tri dich luc bat dau bay, phong khi HUD dich chuyen
            Vector2 targetPos = GetTargetAnchoredPosition();
            cash.DOAnchorPos(targetPos, flyDuration).SetEase(Ease.InQuad).SetTarget(cash).SetLink(cash.gameObject)
                .OnComplete(() => {
                    activeCashes.Remove(cash);
                    Despawn(cash);
                    PunchMoneyText();
                    onArrived?.Invoke();
                });
            cash.DOScale(scale * flyEndScaleRatio, flyDuration).SetEase(Ease.InQuad).SetTarget(cash);
            cash.DOLocalRotate(Vector3.zero, flyDuration).SetTarget(cash);
        });
    }

    private void PlayFloatingText(BigDouble amount, Vector2 position) {
        CanvasGroup group = SpawnText();
        RectTransform rect = (RectTransform)group.transform;
        TextMeshProUGUI text = group.GetComponent<TextMeshProUGUI>();

        text.SetText("+" + Constant.MONEY_SYMBOL + Utils.SciNotToUSName(amount));
        rect.anchoredPosition = position;
        rect.localScale = Vector3.zero;
        group.alpha = 1f;

        float popTime = floatingTextDuration * 0.15f;

        Sequence sequence = DOTween.Sequence().SetTarget(group).SetLink(group.gameObject);
        sequence.Insert(0f, rect.DOScale(1.15f, popTime).SetEase(Ease.OutBack));
        sequence.Insert(popTime, rect.DOScale(1f, popTime));
        sequence.Insert(0f, rect.DOAnchorPosY(position.y + floatingTextMoveY, floatingTextDuration).SetEase(Ease.OutCubic));
        sequence.Insert(floatingTextDuration * 0.6f, group.DOFade(0f, floatingTextDuration * 0.4f));
        sequence.OnComplete(() => {
            activeTexts.Remove(group);
            group.gameObject.SetActive(false);
            textPool.Add(group);
        });
    }

    private void PunchMoneyText() {
        RectTransform rect = moneyText.rectTransform;
        rect.DOKill(true);
        rect.DOPunchScale(Vector3.one * punchScale, 0.15f, 1, 0f);

        if (SoundManager.HasInstance) {
            SoundManager.Instance.PlayFx(FxID.MoneyChange);
        }
    }

    private Vector2 GetTargetAnchoredPosition() {
        Vector3 localPos = container.InverseTransformPoint(moneyText.rectTransform.position);
        return localPos;
    }

    private RectTransform SpawnCash() {
        RectTransform cash;
        if (cashPool.Count > 0) {
            cash = cashPool[cashPool.Count - 1];
            cashPool.RemoveAt(cashPool.Count - 1);
        }
        else {
            GameObject go = new GameObject("CashVFX", typeof(RectTransform), typeof(Image));
            cash = (RectTransform)go.transform;
            cash.SetParent(container, false);
            cash.anchorMin = cash.anchorMax = new Vector2(0.5f, 0.5f);
            cash.sizeDelta = cashSize;

            Image image = go.GetComponent<Image>();
            image.sprite = moneySprite;
            image.preserveAspect = true;
            image.raycastTarget = false;
        }

        cash.gameObject.SetActive(true);
        cash.SetAsLastSibling();
        activeCashes.Add(cash);
        return cash;
    }

    private void Despawn(RectTransform cash) {
        cash.gameObject.SetActive(false);
        cashPool.Add(cash);
    }

    private CanvasGroup SpawnText() {
        CanvasGroup group;
        if (textPool.Count > 0) {
            group = textPool[textPool.Count - 1];
            textPool.RemoveAt(textPool.Count - 1);
        }
        else {
            // clone moneyText de giu nguyen font + outline cua HUD
            TextMeshProUGUI text = Instantiate(moneyText, container);
            text.name = "FloatingMoneyText";
            text.raycastTarget = false;
            text.enableAutoSizing = false;
            text.fontSize = floatingTextFontSize;
            text.color = floatingTextColor;
            text.alignment = TextAlignmentOptions.Center;
            text.enableWordWrapping = false;

            RectTransform rect = text.rectTransform;
            rect.anchorMin = rect.anchorMax = rect.pivot = new Vector2(0.5f, 0.5f);
            rect.sizeDelta = new Vector2(800f, 200f);
            rect.localRotation = Quaternion.identity;

            group = text.gameObject.AddComponent<CanvasGroup>();
            group.blocksRaycasts = false;
            group.interactable = false;
        }

        group.gameObject.SetActive(true);
        group.transform.SetAsLastSibling();
        activeTexts.Add(group);
        return group;
    }
}
