using System;
using UnityEngine;
using UnityEngine.InputSystem;
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
    private bool isFirstTouch = true;
    private PlayerInputActions inputActions;

    private void Awake() {
        maxMovement = joystickSize.x / 2;
        inputActions = new PlayerInputActions();
        inputActions.Player.Enable();
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

    private void OnDestroy() {
        if (inputActions == null) return;

        inputActions.Player.Disable();
        inputActions.Dispose();
    }

    public void OnInit() {
        isFirstTouch = true;

        movementFinger = null;
        startPosition = Vector2.zero;
        currentPosition = Vector2.zero;
        inputVector = Vector2.zero;
        ResetCameraTouch();
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

    public Vector2 GetWASDMovementVectorNormalized() {
        Vector2 inputVector = inputActions.Player.Movement.ReadValue<Vector2>();

        inputVector = inputVector.normalized;

        return inputVector;
    }

    public Vector2 GetLookVectorNormalizer() {
        Vector2 lookVector = isLooking ? cameraInputVetor : Vector2.zero;
        cameraInputVetor = Vector2.zero;

        return lookVector;
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
                cameraInputVetor = Vector2.zero;
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

        if (movementFinger != null && !movementFinger.isActive) ResetMovementTouch();
        if (zoomFinger != null && !zoomFinger.isActive) { zoomFinger = null; isZooming = false; }
        if (cameraFinger != null && !cameraFinger.isActive) ResetCameraTouch();

        if (isFirstTouch) {
            OnFirstTourch?.Invoke(this, EventArgs.Empty);
            isFirstTouch = false;
        }

        float screenMidX = Screen.width * Constant.JOYSTICK_X_RATIO;
        float screenMidY = Screen.height * Constant.JOYSTICK_Y_RATIO;
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
                cameraInputVetor = Vector2.zero;
            }
            else if (zoomFinger == null && touchedFinger != cameraFinger) {
                zoomFinger = touchedFinger;
                isZooming = true;
                isLooking = false;
                pinchDistance = GetPinchDistance();
            }
        }
    }

    private void ResetCameraTouch() {
        cameraFinger = null;
        zoomFinger = null;
        isLooking = false;
        isZooming = false;
        cameraInputVetor = Vector2.zero;
        zoomInput = 0f;
    }

    private void ResetMovementTouch() {
        if (movementFinger != null) {
            OnFingerUp?.Invoke(this, EventArgs.Empty);
        }
        movementFinger = null;
        startPosition = Vector2.zero;
        currentPosition = Vector2.zero;
        inputVector = Vector2.zero;
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