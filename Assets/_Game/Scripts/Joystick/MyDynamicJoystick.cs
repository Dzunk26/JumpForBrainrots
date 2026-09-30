using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.EnhancedTouch;

public class MyDynamicJoystick : MonoBehaviour {
    [SerializeField] private GameObject joystickVisual;
    [SerializeField] private RectTransform knob;
    [SerializeField] private Canvas canvas;
    [SerializeField] private Vector2 joystickSize = new Vector2(250, 250);
    [SerializeField] private float abc;

    private RectTransform RectTransform;

    private void Awake() {
        RectTransform = GetComponent<RectTransform>();
    }

    private void Start() {
        GameInput.Instance.OnFingerDown += GameInput_OnFingerDown;
        GameInput.Instance.OnFingerMove += GameInput_OnFingerMove;
        GameInput.Instance.OnFingerUp += GameInput_OnFingerUp;
    }

    private void GameInput_OnFingerDown(object sender, GameInput.TouchEventArgs e) {
        OnTouchFingerDown(e.touchPosition);
    }

    private void GameInput_OnFingerMove(object sender, GameInput.TouchEventArgs e) {
        OnTouchFingerMove(e.touchPosition);
    }

    private void GameInput_OnFingerUp(object sender, System.EventArgs e) {
        OnTouchFingerUp();
    }

    private Vector2 ScreenToCanvas(Vector2 screenPos) => screenPos / canvas.scaleFactor;

    private void OnTouchFingerDown(Vector2 touchPosition) {
        joystickVisual.SetActive(true);
        RectTransform.sizeDelta = joystickSize;
        RectTransform.anchoredPosition = ClampStartPosition(ScreenToCanvas(touchPosition));
    }

    private void OnTouchFingerMove(Vector2 touchPosition) {
        Vector2 offset = ScreenToCanvas(touchPosition) - RectTransform.anchoredPosition;
        knob.anchoredPosition = Vector2.ClampMagnitude(offset, joystickSize.x / 2);
    }

    private void OnTouchFingerUp() {
        knob.anchoredPosition = Vector3.zero;
        joystickVisual.SetActive(false);
    }

    private Vector2 ClampStartPosition(Vector2 startPosition) {
        if (startPosition.x < joystickSize.x / 2) {
            startPosition.x = joystickSize.x / 2;
        }
        else if (startPosition.x > Screen.width - joystickSize.x / 2) {
            startPosition.x = Screen.width - joystickSize.x / 2;
        }

        if (startPosition.y < joystickSize.y / 2) {
            startPosition.y = joystickSize.y / 2;
        }
        else if (startPosition.y > Screen.height - joystickSize.y / 2) {
            startPosition.y = Screen.height - joystickSize.y / 2;
        }

        return startPosition;
    }
}