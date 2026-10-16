using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class IndexSlotUI : MonoBehaviour {
    [SerializeField] private Image backgroundImage;
    [SerializeField] private Image iconImage;
    [SerializeField] private TextMeshProUGUI rarityText;
    [SerializeField] private TextMeshProUGUI nameText;
    [SerializeField] private Color collectedIconColor = Color.white;
    [SerializeField] private Color lockedIconColor = Color.black;

    private BrainrotSO brainrotSO;

    public void OnInit(BrainrotSO brainrotSO, RarityConfigSO rarityConfig, bool isCollected) {
        this.brainrotSO = brainrotSO;

        backgroundImage.material = rarityConfig.GetBackgroundMaterial();
        rarityText.SetText(rarityConfig.GetBrainrotRarity().ToString());
        iconImage.sprite = brainrotSO.GetIndexIcon();
        nameText.SetText(brainrotSO.GetName());

        SetCollected(isCollected);
    }

    public BrainrotSO GetBrainrotSO() {
        return brainrotSO;
    }

    // chua bat duoc thi icon den xi (nhan mau den, giu nguyen alpha)
    public void SetCollected(bool isCollected) {
        iconImage.color = isCollected ? collectedIconColor : lockedIconColor;
    }
}
