using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class EquipButton : MonoBehaviour {
    [SerializeField] private Player player;
    [SerializeField] private Camera playerCamera;
    [SerializeField] private RectTransform captureButtonTF;
    [SerializeField] private Button equipButton;
    [SerializeField] private TextMeshProUGUI equipButtonText;

    private BaseSlot targetBaseSlot;
    private SlotInteractType currentInteractType;

    private void Start() {
        equipButton.onClick.AddListener(OnClickEquipButton);
        HideEquipButton();
    }

    private void LateUpdate() {
        UpdateEquipButton();
    }

    private void UpdateEquipButton() {
        targetBaseSlot = player.GetInteractableBaseSlot();
        SlotInteractType slotInteractType = player.GetSlotInteractType(targetBaseSlot);
        if (slotInteractType == SlotInteractType.None) {
            HideEquipButton();
            return;
        }

        Vector3 screenPoint = Utils.WorldToScreenPoint(playerCamera, targetBaseSlot.GetEquipButtonSpawnPoint());
        if (screenPoint.z <= 0f) { // z <= 0 la sau camera
            targetBaseSlot = null;
            HideEquipButton();
            return;
        }

        captureButtonTF.position = screenPoint;
        UpdateEquipButtonText(slotInteractType);
        ShowEquipButton();
    }

    private void OnClickEquipButton() {
        player.OnInteractBaseSlot();
    }

    private void HideEquipButton() {
        captureButtonTF.gameObject.SetActive(false);
    }

    private void ShowEquipButton() {
        captureButtonTF.gameObject.SetActive(true);
    }
    private void UpdateEquipButtonText(SlotInteractType interactType) {
        if (currentInteractType == interactType) return;

        currentInteractType = interactType;
        equipButtonText.SetText(GetInteractText(interactType));
    }

    private string GetInteractText(SlotInteractType interactType) {
        switch (interactType) {
            case SlotInteractType.Place:
                return Constant.PLACE_TEXT;
            case SlotInteractType.Swap:
                return Constant.SWAP_TEXT;
            case SlotInteractType.PickUp:
                return Constant.PICK_UP_TEXT;
            default:
                return string.Empty;
        }
    }
}