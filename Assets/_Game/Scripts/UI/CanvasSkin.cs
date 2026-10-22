using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CanvasSkin : UICanvas {
    [SerializeField] private ListSkinSO listSkinSO;
    [SerializeField] private Button closeButton;
    [SerializeField] private Button blockerButton;
    [SerializeField] private SkinSlotUI slotPrefab;
    [SerializeField] private Transform slotParent;
    [SerializeField] private ScrollRect slotScrollRect;
    [SerializeField] private TextMeshProUGUI totalProgressText;

    private List<SkinSlotUI> slots = new List<SkinSlotUI>();
    private Player player;

    private void OnEnable() {
        closeButton.onClick.AddListener(OnClickCloseButton);
        blockerButton.onClick.AddListener(OnClickCloseButton);
    }

    private void OnDisable() {
        closeButton.onClick.RemoveListener(OnClickCloseButton);
        blockerButton.onClick.RemoveListener(OnClickCloseButton);
    }

    public override void SetUp() {
        base.SetUp();
        CreateSlots();
        slotScrollRect.verticalNormalizedPosition = 1f;
    }

    public void OnInit(Player player) {
        this.player = player;
        UpdateVisual();
    }

    public void UpdateVisual() {
        int ownedCount = 0;

        for (int i = 0; i < listSkinSO.GetSkinAmount(); i++) {
            SkinSO skinSO = slots[i].GetSkinSO();
            bool isOwned = SkinManager.Instance.IsOwned(skinSO);
            if (isOwned) {
                ownedCount++;
            }

            slots[i].UpdateVisual(isOwned, SkinManager.Instance.IsEquipped(skinSO));
        }

        totalProgressText.SetText("{0}/{1}", ownedCount, listSkinSO.GetSkinAmount());
    }

    // chua mo thi bam de mo khoa, da mo thi bam de mac (nhan vat ngoai map doi skin ngay)
    public void OnClickSlot(SkinSlotUI slot) {
        SkinSO skinSO = slot.GetSkinSO();
        if (SkinManager.Instance.IsEquipped(skinSO)) return;

        if (!SkinManager.Instance.IsOwned(skinSO)) {
            UnlockSkin(skinSO);
            return;
        }

        SkinManager.Instance.EquipSkin(skinSO);
        player.ChangeSkin(skinSO);
        UpdateVisual();
    }

    private void CreateSlots() {
        for (int i = 0; i < listSkinSO.GetSkinAmount(); i++) {
            if (i >= slots.Count) {
                slots.Add(Instantiate(slotPrefab, slotParent));
            }

            slots[i].gameObject.SetActive(true);
            slots[i].OnInit(this, listSkinSO.GetSkinSOByIndex(i));
        }

        for (int i = listSkinSO.GetSkinAmount(); i < slots.Count; i++) {
            slots[i].gameObject.SetActive(false);
        }
    }

    // tam thoi bam la mo khoa, sau nay thay bang xem quang cao xong moi mo
    private void UnlockSkin(SkinSO skinSO) {
        SkinManager.Instance.UnlockSkin(skinSO);
        UpdateVisual();
    }

    private void OnClickCloseButton() {
        Close(0f);
    }
}
