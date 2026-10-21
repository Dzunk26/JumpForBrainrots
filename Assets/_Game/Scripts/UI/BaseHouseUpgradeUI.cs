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
        upgradeButton.onClick.AddListener(OnClickUpgradeButton);
        freeUpgradeButton.onClick.AddListener(OnClickFreeUpgradeButton);
    }

    private void Start() {
        UpdateVisual();
    }

    private void OnDisable() {
        upgradeButton.onClick.RemoveListener(OnClickUpgradeButton);
        freeUpgradeButton.onClick.RemoveListener(OnClickFreeUpgradeButton);
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