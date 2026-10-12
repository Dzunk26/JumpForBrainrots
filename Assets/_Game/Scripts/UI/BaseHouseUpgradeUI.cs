using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BaseHouseUpgradeUI : MonoBehaviour {
    [SerializeField] private BaseHouse baseHouse;
    [SerializeField] private Button upgradeButton;
    [SerializeField] private Button freeUpgradeButton;
    [SerializeField] private int houseLevel;

    private void OnEnable() {
        baseHouse.OnHouseLevelChanged += BaseHouse_OnHouseLevelChanged;
        upgradeButton.onClick.AddListener(OnClickUpgradeButton);
        freeUpgradeButton.onClick.AddListener(OnClickFreeUpgradeButton);
    }

    private void Start() {
        UpdateVisual();
    }

    private void OnDisable() {
        baseHouse.OnHouseLevelChanged -= BaseHouse_OnHouseLevelChanged;
        upgradeButton.onClick.RemoveListener(OnClickUpgradeButton);
        freeUpgradeButton.onClick.RemoveListener(OnClickFreeUpgradeButton);
    }

    private void BaseHouse_OnHouseLevelChanged(object sender, System.EventArgs e) {
        UpdateVisual();
    }

    private void OnClickUpgradeButton() {
        baseHouse.OnClickUpgrade();
    }

    private void OnClickFreeUpgradeButton() {
        baseHouse.OnClickUpgrade();
    }

    private void UpdateVisual() {
        gameObject.SetActive(baseHouse.GetHouseLevel() == houseLevel);
    }
}