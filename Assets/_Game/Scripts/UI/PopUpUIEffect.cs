using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

public class PopUpUIEffect : MonoBehaviour {
    [SerializeField] private Vector2 popUpOrigin = new Vector2(0.5f, 0.5f);   // (0.5, 0.5) = center, (0.5, 0) = bottom
    [SerializeField] private float popUpStartScale = 0.5f;
    [SerializeField] private float duration = 0.3f;

    private RectTransform rect;

    private void Awake() {
        rect = GetComponent<RectTransform>();
        SetPivotKeepPosition(popUpOrigin);
    }

    private void OnEnable() {
        rect.DOKill();
        rect.localScale = Vector3.one * popUpStartScale;
        rect.DOScale(1f, duration).SetEase(Ease.OutBack).SetUpdate(true);
    }

    private void OnDisable() {
        rect.DOKill();
        rect.localScale = Vector3.one;
    }

    private void SetPivotKeepPosition(Vector2 pivot) {
        Vector2 delta = pivot - rect.pivot;
        rect.pivot = pivot;
        rect.anchoredPosition += Vector2.Scale(delta, rect.rect.size);
    }
}