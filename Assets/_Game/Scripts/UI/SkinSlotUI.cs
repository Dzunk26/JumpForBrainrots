using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SkinSlotUI : MonoBehaviour {
    [SerializeField] private Button button;
    [SerializeField] private Image iconImage;
    [SerializeField] private Image frameImage;
    [SerializeField] private GameObject lockOverlay;
    [SerializeField] private GameObject lockIcon;
    [SerializeField] private GameObject equippedMark;
    [SerializeField] private GameObject equippedLabel;
    [SerializeField] private Color ownedFrameColor = new Color(0.35f, 0.92f, 1f, 1f);
    [SerializeField] private Color equippedFrameColor = new Color(1f, 0.82f, 0.1f, 1f);
    [SerializeField] private Color lockedIconColor = new Color(0.45f, 0.45f, 0.45f, 1f);

    private CanvasSkin canvasSkin;
    private SkinSO skinSO;

    private void OnEnable() {
        button.onClick.AddListener(OnClickSlot);
    }

    private void OnDisable() {
        button.onClick.RemoveListener(OnClickSlot);
    }

    public void OnInit(CanvasSkin canvasSkin, SkinSO skinSO) {
        this.canvasSkin = canvasSkin;
        this.skinSO = skinSO;

        iconImage.sprite = skinSO.GetIcon();
    }

    public SkinSO GetSkinSO() {
        return skinSO;
    }

    // chua mo: icon xam + o khoa, da mo: vien xanh, dang mac: vien vang + dau tick + chu Equipped
    public void UpdateVisual(bool isOwned, bool isEquipped) {
        iconImage.color = isOwned ? Color.white : lockedIconColor;
        lockOverlay.SetActive(!isOwned);
        lockIcon.SetActive(!isOwned);
        equippedMark.SetActive(isEquipped);
        equippedLabel.SetActive(isEquipped);

        frameImage.gameObject.SetActive(isOwned);
        frameImage.color = isEquipped ? equippedFrameColor : ownedFrameColor;
    }

    private void OnClickSlot() {
        canvasSkin.OnClickSlot(this);
    }
}
