using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

[System.Serializable]
public class IndexRarityConfig {
    public BrainrotRarity rarity;
    public Material backgroundMaterial;
}

public class CanvasIndex : UICanvas {
    [SerializeField] private ListBrainrotSO listBrainrotSO;
    [SerializeField] private List<IndexRarityConfig> rarityConfigs;
    [SerializeField] private RectTransform panel;
    [SerializeField] private Button closeButton;
    [SerializeField] private IndexTabUI tabPrefab;
    [SerializeField] private Transform tabParent;
    [SerializeField] private IndexSlotUI slotPrefab;
    [SerializeField] private Transform slotParent;
    [SerializeField] private ScrollRect slotScrollRect;
    [SerializeField] private TextMeshProUGUI rarityTitleText;
    [SerializeField] private TextMeshProUGUI rarityProgressText;
    [SerializeField] private Image rarityProgressFill;
    [SerializeField] private TextMeshProUGUI totalProgressText;
    [SerializeField] private float popUpTimerMax = 0.2f;
    [SerializeField] private float popUpStartScale = 0.8f;

    private List<IndexTabUI> tabs = new List<IndexTabUI>();
    private List<IndexSlotUI> slots = new List<IndexSlotUI>();
    private BrainrotRarity currentRarity;
    private float popUpTimer;
    private bool isInited;

    private void OnEnable() {
        closeButton.onClick.AddListener(OnClickCloseButton);
    }

    private void OnDisable() {
        closeButton.onClick.RemoveListener(OnClickCloseButton);
    }

    public override void SetUp() {
        base.SetUp();

        if (!isInited) {
            OnInit();
        }

        UpdateVisual();
    }

    public override void Open() {
        base.Open();
        StartCoroutine(IEPopUp());
    }

    // goi tu IndexTabUI khi bam vao tab
    public void OnSelectRarity(BrainrotRarity rarity) {
        if (currentRarity == rarity) return;

        currentRarity = rarity;
        UpdateVisual();
        slotScrollRect.verticalNormalizedPosition = 1f;
    }

    // goi lai khi inventory thay doi trong luc dang mo index
    public void UpdateVisual() {
        UpdateTabs();
        UpdateSlots();
    }

    // tao tab cho tung rarity, chi tao 1 lan
    private void OnInit() {
        isInited = true;

        for (int i = 0; i < rarityConfigs.Count; i++) {
            IndexTabUI tab = Instantiate(tabPrefab, tabParent);
            tab.OnInit(this, rarityConfigs[i]);
            tabs.Add(tab);
        }

        if (rarityConfigs.Count > 0) {
            currentRarity = rarityConfigs[0].rarity;
        }
    }

    // so luong da bat cua tung rarity + tong
    private void UpdateTabs() {
        int totalCollectedCount = 0;
        int totalCount = 0;

        for (int i = 0; i < tabs.Count; i++) {
            List<BrainrotSO> brainrotSOs = listBrainrotSO.GetBrainrotSOsByRarity(tabs[i].GetRarity());
            int collectedCount = GetCollectedCount(brainrotSOs);

            tabs[i].SetCount(collectedCount, brainrotSOs.Count);
            tabs[i].SetSelected(tabs[i].GetRarity() == currentRarity);

            totalCollectedCount += collectedCount;
            totalCount += brainrotSOs.Count;
        }

        totalProgressText.SetText(totalCollectedCount + "/" + totalCount);
    }

    // hien cac brainrot cua rarity dang chon, dung lai slot cu, thieu thi tao them
    private void UpdateSlots() {
        List<BrainrotSO> brainrotSOs = listBrainrotSO.GetBrainrotSOsByRarity(currentRarity);
        Material backgroundMaterial = GetBackgroundMaterial(currentRarity);
        int collectedCount = 0;

        for (int i = 0; i < brainrotSOs.Count; i++) {
            if (i >= slots.Count) {
                slots.Add(Instantiate(slotPrefab, slotParent));
            }

            bool isCollected = IsBrainrotCollected(brainrotSOs[i]);
            if (isCollected) {
                collectedCount++;
            }

            slots[i].gameObject.SetActive(true);
            slots[i].OnInit(brainrotSOs[i], backgroundMaterial, isCollected);
        }

        for (int i = brainrotSOs.Count; i < slots.Count; i++) {
            slots[i].gameObject.SetActive(false);
        }

        rarityTitleText.SetText(currentRarity.ToString());
        rarityProgressText.SetText(collectedCount + "/" + brainrotSOs.Count);
        rarityProgressFill.fillAmount = brainrotSOs.Count > 0 ? (float)collectedCount / brainrotSOs.Count : 0f;
    }

    private Material GetBackgroundMaterial(BrainrotRarity rarity) {
        for (int i = 0; i < rarityConfigs.Count; i++) {
            if (rarityConfigs[i].rarity == rarity) {
                return rarityConfigs[i].backgroundMaterial;
            }
        }

        return null;
    }

    private int GetCollectedCount(List<BrainrotSO> brainrotSOs) {
        int collectedCount = 0;

        for (int i = 0; i < brainrotSOs.Count; i++) {
            if (IsBrainrotCollected(brainrotSOs[i])) {
                collectedCount++;
            }
        }

        return collectedCount;
    }

    // TODO: chua co inventory, sau nay check brainrot da bat duoc chua o day
    private bool IsBrainrotCollected(BrainrotSO brainrotSO) {
        //return Inventory.Instance.HasBrainrot(brainrotSO);
        return false;
    }

    private void OnClickCloseButton() {
        Close(0f);
    }

    // hieu ung pop up khi mo, dung unscaled time de van chay khi pause game
    private IEnumerator IEPopUp() {
        popUpTimer = 0f;

        while (popUpTimer < popUpTimerMax) {
            popUpTimer += Time.unscaledDeltaTime;
            float t = Mathf.Clamp01(popUpTimer / popUpTimerMax);
            panel.localScale = Vector3.one * Mathf.Lerp(popUpStartScale, 1f, 1f - (1f - t) * (1f - t));
            yield return null;
        }

        panel.localScale = Vector3.one;
    }
}
