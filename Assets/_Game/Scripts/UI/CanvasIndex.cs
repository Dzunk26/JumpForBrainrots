using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CanvasIndex : UICanvas {
    [SerializeField] private ListBrainrotSO listBrainrotSO;
    [SerializeField] private ListRarityConfigSO listRarityConfig;
    [SerializeField] private RectTransform panel;
    [SerializeField] private Button closeButton;
    [SerializeField] private Button blockerButton;
    [SerializeField] private IndexSlotUI slotPrefab;
    [SerializeField] private Transform slotParent;
    [SerializeField] private ScrollRect slotScrollRect;
    [SerializeField] private TextMeshProUGUI totalProgressText;

    private List<IndexSlotUI> slots = new List<IndexSlotUI>();
    private float popUpTimer;

    private void OnEnable() {
        closeButton.onClick.AddListener(OnClickCloseButton);
        blockerButton.onClick.AddListener(OnClickCloseButton);
        UIManager.Instance.GetUI<CanvasGamePlay>().ShowOnly();
    }

    private void OnDisable() {
        closeButton.onClick.RemoveListener(OnClickCloseButton);
        blockerButton.onClick.RemoveListener(OnClickCloseButton);
        if (UIManager.Instance.IsUIOpen<CanvasGamePlay>())
            UIManager.Instance.GetUI<CanvasGamePlay>().ShowAll();
    }

    public override void SetUp() {
        base.SetUp();
        UpdateVisual();
        slotScrollRect.verticalNormalizedPosition = 1f;
    }

    public override void Open() {
        base.Open();
    }

    public void UpdateVisual() {
        int slotIndex = 0;
        int collectedCount = 0;

        for (int i = 0; i < listRarityConfig.GetRarityAmount(); i++) {
            RarityConfigSO rarityConfig = listRarityConfig.GetRarityConfigByIndex(i);
            List<BrainrotSO> brainrotSOs = listBrainrotSO.GetBrainrotSOsByRarity(rarityConfig.GetBrainrotRarity());

            for (int j = 0; j < brainrotSOs.Count; j++) {
                if (slotIndex >= slots.Count) {
                    slots.Add(Instantiate(slotPrefab, slotParent));
                }

                bool isCollected = IsBrainrotCollected(brainrotSOs[j]);
                if (isCollected) {
                    collectedCount++;
                }

                slots[slotIndex].gameObject.SetActive(true);
                slots[slotIndex].OnInit(brainrotSOs[j], rarityConfig, isCollected);
                slotIndex++;
            }
        }

        for (int i = slotIndex; i < slots.Count; i++) {
            slots[i].gameObject.SetActive(false);
        }

        totalProgressText.SetText("{0}/{1}", collectedCount, slotIndex);
    }

    private bool IsBrainrotCollected(BrainrotSO brainrotSO) {
        return IndexManager.Instance.IsCollected(brainrotSO);
    }

    private void OnClickCloseButton() {
        Close(0f);
    }
}
