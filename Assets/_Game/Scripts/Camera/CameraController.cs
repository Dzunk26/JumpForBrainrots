using Cinemachine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour {
    [SerializeField] private Transform followTarget;
    [SerializeField] private CinemachineVirtualCamera cinemachineVirtualCamera;

    [SerializeField] private float rotationSensity = 0.8f;
    [SerializeField] private float rotationSpeed = 15;
    [SerializeField] private float bottomClamp = -60f;
    [SerializeField] private float topClamp = 80f;

    [SerializeField] private float minCameraDistance = 3f;
    [SerializeField] private float maxCameraDistance = 15f;
    [SerializeField] private float minCameraZoom = 30f;
    [SerializeField] private float maxCameraZoom= 75f;
    [SerializeField] private float zoomFOVSensitivity = 10f;
    [SerializeField] private float zoomFOVSpeed = 5f;
    [SerializeField] private float zoomDistanceSensitivity = 10f;
    [SerializeField] private float zoomDistanceSpeed = 5f;

    private float cinemachineTargetPitch;
    private float cinemachineTargetYaw;
    private Vector2 lookInputVector;
    private float zoomInput;
    private Vector3 originPosition;
    private float targetZoom;
    private float targetDistance;
    private Cinemachine3rdPersonFollow thirdPersonFollow;

    private void Awake() {
        OnInit();
    }

    private void Update() {
        ListenInput();
    }

    private void LateUpdate() {
        //HandleCameraZoom();
        HandleCameraZoomDistance();

        HandleCameraRotate();
    }

    public void OnInit() {
        thirdPersonFollow = cinemachineVirtualCamera.GetCinemachineComponent<Cinemachine3rdPersonFollow>();
        originPosition = transform.position;
        targetZoom = cinemachineVirtualCamera.m_Lens.FieldOfView;
        targetDistance = thirdPersonFollow.CameraDistance;
    }

    private void ListenInput() {
        lookInputVector = GameInput.Instance.GetLookVectorNormalizer();
        zoomInput = GameInput.Instance.GetZoomInput();
    }

    private void HandleCameraRotate() {
        if (lookInputVector.sqrMagnitude < 0.2f) return;

        lookInputVector = lookInputVector * rotationSensity;

        float inputY = lookInputVector.y * rotationSpeed * Time.deltaTime;
        float inputX = lookInputVector.x * rotationSpeed * Time.deltaTime;

        cinemachineTargetPitch = UpdateRotation(cinemachineTargetPitch, inputY, bottomClamp, topClamp, true);
        cinemachineTargetYaw = UpdateRotation(cinemachineTargetYaw, inputX, float.MinValue, float.MaxValue, false);

        ApplyRotation(cinemachineTargetPitch, cinemachineTargetYaw);
    }

    private void ApplyRotation(float pitch, float yaw) {
        followTarget.rotation = Quaternion.Euler(pitch, yaw, followTarget.eulerAngles.z);
    }

    private float UpdateRotation(float currentRotation, float input, float min, float max, bool isXAxis) {
        currentRotation += isXAxis ? -input : input;

        return Mathf.Clamp(currentRotation, min, max);
    }

    private void HandleCameraZoomFOV() {
        targetZoom -= zoomInput * zoomFOVSensitivity;
        targetZoom = Mathf.Clamp(targetZoom, minCameraZoom, maxCameraZoom);

        cinemachineVirtualCamera.m_Lens.FieldOfView = Mathf.Lerp(cinemachineVirtualCamera.m_Lens.FieldOfView, targetZoom, Time.deltaTime * zoomFOVSpeed);
    }

    private void HandleCameraZoomDistance() {
        targetDistance -= zoomInput * zoomDistanceSensitivity;
        targetDistance = Mathf.Clamp(targetDistance, minCameraDistance, maxCameraDistance);

        thirdPersonFollow.CameraDistance = Mathf.Lerp(thirdPersonFollow.CameraDistance, targetDistance, Time.deltaTime * zoomDistanceSpeed);
    }
}