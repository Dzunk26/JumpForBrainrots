using Cinemachine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour {
    [SerializeField] private Transform followTarget;
    [SerializeField] private CinemachineVirtualCamera cinemachineVirtualCamera;

    [SerializeField] private float rotationSpeed = 15;
    [SerializeField] private float bottomClamp = -60f;
    [SerializeField] private float topClamp = 80f;

    [SerializeField] private float minCameraZoom = 30f;
    [SerializeField] private float maxCameraZoom= 75f;
    [SerializeField] private float zoomSensitivity = 10f;
    [SerializeField] private float zoomSpeed = 5f;

    private float cinemachineTargetPitch;
    private float cinemachineTargetYaw;
    private Vector2 lookInputVector;
    private float zoomInput;
    private Vector3 originPosition;
    private float targetZoom;

    private void Awake() {
        OnInit();
    }

    private void Update() {
        ListenInput();
    }

    private void LateUpdate() {
        HandleCameraZoom();

        HandleCameraRotate();
    }

    public void OnInit() {
        originPosition = transform.position;
        targetZoom = cinemachineVirtualCamera.m_Lens.FieldOfView;
    }

    private void ListenInput() {
        lookInputVector = GameInput.Instance.GetLookVectorNormalizer();
        zoomInput = GameInput.Instance.GetZoomInput();
    }

    private void HandleCameraRotate() {
        if (lookInputVector.sqrMagnitude < 0.001f) return;

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

    private void HandleCameraZoom() {
        targetZoom -= zoomInput * zoomSensitivity;
        targetZoom = Mathf.Clamp(targetZoom, minCameraZoom, maxCameraZoom);

        cinemachineVirtualCamera.m_Lens.FieldOfView = Mathf.Lerp(cinemachineVirtualCamera.m_Lens.FieldOfView, targetZoom, Time.deltaTime * zoomSpeed);
    }
}