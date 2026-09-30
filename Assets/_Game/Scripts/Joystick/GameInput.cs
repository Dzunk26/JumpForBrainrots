using System;
using UnityEngine;
using UnityEngine.InputSystem.EnhancedTouch;
using Etouch = UnityEngine.InputSystem.EnhancedTouch;

public class GameInput : Singleton<GameInput> {
    public event EventHandler<TouchEventArgs> OnFingerDown;
    public event EventHandler<TouchEventArgs> OnFingerMove;
    public event EventHandler OnFingerUp;

    public event EventHandler OnFirstTourch;

    public class TouchEventArgs : EventArgs {
        public Vector2 touchPosition;
    }

    [SerializeField] private Vector2 joystickSize = new Vector2(250, 250);

    private Finger movementFinger;
    private Finger cameraFinger;
    private Finger zoomFinger;

    private Vector2 startPosition;
    private Vector2 currentPosition;
    private Vector2 inputVector;
    private float maxMovement;

    private Vector2 cameraStartPosition;
    private Vector2 cameraInputVetor;
    private bool isLooking; // for input camera

    private float pinchDistance;
    private float zoomInput;
    private bool isZooming;

    public void OnInit() {
        isFirstTouch = true;

        movementFinger = null;
        startPosition = Vector2.zero;
        currentPosition = Vector2.zero;
        inputVector = Vector2.zero;
    }

    public float GetZoomInput() {
        float zoom = zoomInput;
        zoomInput = 0f;

        return zoom;
    }

    public Vector2 GetMovementVectorNormalized() {
        inputVector = currentPosition - startPosition;

        if (inputVector.sqrMagnitude > maxMovement * maxMovement) {
            inputVector = inputVector.normalized;
        }
        else {
            inputVector = inputVector / maxMovement;
            inputVector = HandleStickDeadzone(inputVector);
        }

        return inputVector;
    }

    public Vector2 GetLookVectorNormalizer() {
        return isLooking ? cameraInputVetor : Vector2.zero;
    }

    private bool isFirstTouch = true;

    private void Awake() {
        maxMovement = joystickSize.x / 2;
    }

    private void OnEnable() {
        EnhancedTouchSupport.Enable();
        Etouch.Touch.onFingerDown += Touch_onFingerDown;
        Etouch.Touch.onFingerMove += Touch_onFingerMove;
        Etouch.Touch.onFingerUp += Touch_onFingerUp;
    }

    private void OnDisable() {
        Etouch.Touch.onFingerDown -= Touch_onFingerDown;
        Etouch.Touch.onFingerMove -= Touch_onFingerMove;
        Etouch.Touch.onFingerUp -= Touch_onFingerUp;
        EnhancedTouchSupport.Disable();
    }

    private void Touch_onFingerUp(Finger lostFinger) {
        if (lostFinger == movementFinger) {
            OnFingerUp?.Invoke(this, EventArgs.Empty);
            movementFinger = null;
            startPosition = Vector2.zero;
            currentPosition = Vector2.zero;
        }
        else if (lostFinger == zoomFinger) {
            StopZoom();
        }
        else if (lostFinger == cameraFinger) {
            if (isZooming) {
                cameraFinger = zoomFinger;
                StopZoom();
            }
            else {
                cameraFinger = null;
                isLooking = false;
            }
        }
    }

    private void Touch_onFingerMove(Finger movedFinger) {
        if (movedFinger == movementFinger) {
            Etouch.Touch curentTouch = movedFinger.currentTouch;
            currentPosition = curentTouch.screenPosition;

            OnFingerMove?.Invoke(this, new TouchEventArgs {
                touchPosition = currentPosition
            });
        }
        else if (isZooming && (movedFinger == cameraFinger || movedFinger == zoomFinger)) {
            HandlePinchZoom();
        }
        else if (movedFinger == cameraFinger) {
            Vector2 pos = movedFinger.currentTouch.screenPosition;
            cameraInputVetor = pos - cameraStartPosition;
            cameraStartPosition = pos;
        }
    }

    private void Touch_onFingerDown(Finger touchedFinger) {
        //if (!GameManager.Instance.IsPlayingGame()) return;

        if (isFirstTouch) {
            OnFirstTourch?.Invoke(this, EventArgs.Empty);
            isFirstTouch = false;
        }

        float screenMidX = Screen.width * 0.5f;
        float screenMidY = Screen.height * 0.5f;
        Vector2 pos = touchedFinger.screenPosition;
        if (pos.x < screenMidX && pos.y < screenMidY) {
            if (movementFinger == null) {
                movementFinger = touchedFinger;
                startPosition = touchedFinger.screenPosition;
                currentPosition = touchedFinger.screenPosition;

                OnFingerDown?.Invoke(this, new TouchEventArgs {
                    touchPosition = startPosition
                });
            }
        }
        else {
            if (cameraFinger == null) {
                cameraFinger = touchedFinger;
                isLooking = true;
                cameraStartPosition = pos;
            }
            else if (zoomFinger == null) {
                zoomFinger = touchedFinger;
                isZooming = true;
                isLooking = false;
                pinchDistance = GetPinchDistance();
            }
        }
    }

    private void StopZoom() {
        zoomFinger = null;
        isZooming = false;

        isLooking = true;
        cameraStartPosition = cameraFinger.screenPosition;
        cameraInputVetor = Vector2.zero;
    }

    private float GetPinchDistance() {
        return Vector2.Distance(cameraFinger.screenPosition, zoomFinger.screenPosition);
    }

    private void HandlePinchZoom() {
        float currentDistance = GetPinchDistance();
        zoomInput += (currentDistance - pinchDistance) / Screen.height;
        pinchDistance = currentDistance;
    }

    private Vector2 HandleStickDeadzone(Vector2 inputVector) {
        if (inputVector.magnitude < 0.1f) {
            inputVector = Vector2.zero;
        }

        return inputVector;
    }
}