using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CaptureButtonUI : MonoBehaviour {
    [SerializeField] private Player player;
    [SerializeField] private Camera playerCamera;
    [SerializeField] private RectTransform captureButtonTF;
    [SerializeField] private Button captureButton;

    private Brainrot targetBrainrot;

    private void Start() {
        captureButton.onClick.AddListener(OnClickCaptureButton);
        HideCaptureButton();
    }

    private void LateUpdate() {
        UpdateCaptureButton();
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