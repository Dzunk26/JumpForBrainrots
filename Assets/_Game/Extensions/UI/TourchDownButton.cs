using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class TourchDownButton : MonoBehaviour, IPointerDownHandler {
    public event EventHandler OnTouchDown;

    [SerializeField] private Button button;

    public void OnPointerDown(PointerEventData eventData) {
        if (!button.interactable) return;

        OnTouchDown?.Invoke(this, EventArgs.Empty);
    }
}