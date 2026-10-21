using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class ButtonUIEffect : MonoBehaviour, IPointerDownHandler, IPointerUpHandler {
    [SerializeField] private float pressScale = 0.9f;
    [SerializeField] private float pressDuration = 0.08f;
    [SerializeField] private float releaseDuration = 0.3f;

    private RectTransform rectTransform;
    private Tween scaleTween;

    private void Awake() {
        rectTransform = GetComponent<RectTransform>();
    }

    private void OnDisable() {
        scaleTween?.Kill();
        rectTransform.localScale = Vector3.one;
    }

    public void OnPointerDown(PointerEventData eventData) {
        scaleTween?.Kill();
        scaleTween = rectTransform.DOScale(pressScale, pressDuration)
            .SetEase(Ease.OutQuad)
            .SetUpdate(true);
    }

    public void OnPointerUp(PointerEventData eventData) {
        scaleTween?.Kill();
        scaleTween = rectTransform.DOScale(1f, releaseDuration)
            .SetEase(Ease.OutBack)
            .SetUpdate(true);
    }
}