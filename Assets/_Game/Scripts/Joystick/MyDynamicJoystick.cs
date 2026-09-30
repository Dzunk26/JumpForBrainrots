using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.EnhancedTouch;

public class MyDynamicJoystick : MonoBehaviour {
    [SerializeField] private GameObject joystickVisual;
    [SerializeField] private RectTransform knob;
    [SerializeField] private Vector2 joystickSize = new Vector2(250, 250);

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

    private void OnTouchFingerDown(Vector2 touchPosition) {
        joystickVisual.SetActive(true);
        RectTransform.sizeDelta = joystickSize;
        RectTransform.anchoredPosition = ClampStartPosition(touchPosition);
    }

    private void OnTouchFingerMove(Vector2 touchPosition) {
        Vector2 knobPosition;
        float maxMovement = joystickSize.x / 2;
        if ((touchPosition - RectTransform.anchoredPosition).sqrMagnitude > maxMovement * maxMovement) {
            knobPosition = (touchPosition - RectTransform.anchoredPosition).normalized * maxMovement;
        }
        else {
            knobPosition = touchPosition - RectTransform.anchoredPosition;
        }

        knob.anchoredPosition = knobPosition;
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