using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class IndexSlotUI : MonoBehaviour {
    [SerializeField] private Image backgroundImage;
    [SerializeField] private Image iconImage;
    [SerializeField] private TextMeshProUGUI nameText;
    [SerializeField] private Color collectedIconColor = Color.white;
    [SerializeField] private Color lockedIconColor = Color.black;

    private BrainrotSO brainrotSO;

    public void OnInit(BrainrotSO brainrotSO, Material backgroundMaterial, bool isCollected) {
        this.brainrotSO = brainrotSO;

        backgroundImage.material = backgroundMaterial;
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
