using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class IndexTabUI : MonoBehaviour {
    [SerializeField] private Button tabButton;
    [SerializeField] private Image backgroundImage;
    [SerializeField] private GameObject selectedFrame;
    [SerializeField] private TextMeshProUGUI nameText;
    [SerializeField] private TextMeshProUGUI countText;

    private CanvasIndex canvasIndex;
    private BrainrotRarity rarity;

    private void OnEnable() {
        tabButton.onClick.AddListener(OnClickTab);
    }

    private void OnDisable() {
        tabButton.onClick.RemoveListener(OnClickTab);
    }

    public void OnInit(CanvasIndex canvasIndex, IndexRarityConfig rarityConfig) {
        this.canvasIndex = canvasIndex;
        rarity = rarityConfig.rarity;

        backgroundImage.material = rarityConfig.backgroundMaterial;
        nameText.SetText(rarity.ToString());
    }

    public BrainrotRarity GetRarity() {
        return rarity;
    }

    public void SetSelected(bool isSelected) {
        selectedFrame.SetActive(isSelected);
    }

    public void SetCount(int collectedCount, int totalCount) {
        countText.SetText(collectedCount + "/" + totalCount);
    }

    private void OnClickTab() {
        canvasIndex.OnSelectRarity(rarity);
    }
}
