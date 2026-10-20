using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

[DefaultExecutionOrder(200)]
public class DropButtonUI : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IPointerExitHandler {
    [SerializeField] private Player player;
    [SerializeField] private Camera playerCamera;
    [SerializeField] private RectTransform dropButtonTF;
    [SerializeField] private Image holdFillImage;
    [SerializeField] private float holdTimerMax = 0.4f;
    [SerializeField] private BrainrotRarity holdRequiredRarity = BrainrotRarity.Epic;

    private float holdTimer;
    private bool isHolding;

    private void Start() {
        HideDropButton();
    }

    private void LateUpdate() {
        UpdateDropButton();
        HandleHold();
    }

    public void OnInit(Player player, Camera playerCamera) {
        this.player = player;
        this.playerCamera = playerCamera;
    }

    public void OnPointerDown(PointerEventData eventData) {
        if (!IsNeedHold()) {
            OnDrop();
            return;
        }

        isHolding = true;
        holdTimer = 0f;
    }

    public void OnPointerExit(PointerEventData eventData) {
        ResetHold();
    }

    public void OnPointerUp(PointerEventData eventData) {
        ResetHold();
    }

    private void UpdateDropButton() {
        if (!player.IsHoldingCapturedBrainrot()) {
            HideDropButton();
            return;
        }

        Vector3 screenPoint = Utils.WorldToScreenPoint(playerCamera, player.GetDropButtonSpawnPoint());
        if (screenPoint.z <= 0f) {
            HideDropButton();
            return;
        }

        if (!isHolding) {
            dropButtonTF.position = screenPoint;
        }
        ShowDropButton();
    }

    private void HandleHold() {
        if (!isHolding) return;

        holdTimer += Time.deltaTime;
        holdFillImage.fillAmount = holdTimer / holdTimerMax;
        if (holdTimer < holdTimerMax) return;

        OnDrop();
    }

    private bool IsNeedHold() {
        return player.GetCurrentHoldBrainrotSO().GetRarity() >= holdRequiredRarity;
    }

    private void OnDrop() {
        player.DropCapturedBrainrot();
        ResetHold();
    }

    private void ResetHold() {
        isHolding = false;
        holdTimer = 0f;
        holdFillImage.fillAmount = 0f;
    }

    private void HideDropButton() {
        if (!dropButtonTF.gameObject.activeSelf) return;

        ResetHold();
        dropButtonTF.gameObject.SetActive(false);
    }

    private void ShowDropButton() {
        if (dropButtonTF.gameObject.activeSelf) return;

        dropButtonTF.gameObject.SetActive(true);
    }
}