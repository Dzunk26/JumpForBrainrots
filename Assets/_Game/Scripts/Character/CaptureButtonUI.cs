using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CaptureButtonUI : MonoBehaviour {
    [SerializeField] private Player player;
    [SerializeField] private Camera playerCamera;
    [SerializeField] private RectTransform captureButtonTF;
    [SerializeField] private TourchDownButton captureButton;

    private Brainrot targetBrainrot;

    private void OnEnable() {
        captureButton.OnTouchDown += CaptureButton_OnTouchDown;
    }

    private void Start() {
        HideCaptureButton();
    }

    private void LateUpdate() {
        UpdateCaptureButton();
    }

    private void OnDisable() {
        
    }

    private void CaptureButton_OnTouchDown(object sender, System.EventArgs e) {
        OnClickCaptureButton();
    }

    private void UpdateCaptureButton() {
        targetBrainrot = player.GetCaptureableTarget();
        if (targetBrainrot == null) {
            HideCaptureButton();
            return;
        }

        Vector3 screenPoint = Utils.WorldToScreenPoint(playerCamera, targetBrainrot.GetCaptureButtonSpawnPoint());
        if (screenPoint.z <= 0f) { // z <= 0 la sau camera
            targetBrainrot = null;
            HideCaptureButton();
            return;
        }
        captureButtonTF.position = screenPoint;
        ShowCaptureButton();
    }

    private void OnClickCaptureButton() {
        player.OnCaptureBrainrot();
    }

    private void HideCaptureButton() {
        captureButtonTF.gameObject.SetActive(false);
    }

    private void ShowCaptureButton() {
        captureButtonTF.gameObject.SetActive(true);
    }
}