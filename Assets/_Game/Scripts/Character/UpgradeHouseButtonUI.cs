using BreakInfinity;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UpgradeHouseButtonUI : MonoBehaviour {
    [SerializeField] private Player player;
    [SerializeField] private Camera playerCamera;
    [SerializeField] private RectTransform buttonContainerTF;
    [SerializeField] private TextMeshProUGUI tittleText;
    [SerializeField] private Button upgradeButton;
    [SerializeField] private Button freeUpgradeButton;
    [SerializeField] private TextMeshProUGUI upgradeCostText;

    private BaseHouse targetBaseHouse;
    private BigDouble currentCost = -1;
    private int currentTargetLevel = -1;

    private void OnEnable() {
        upgradeButton.onClick.AddListener(OnClickUpgradeButton);
        freeUpgradeButton.onClick.AddListener(OnClickFreeUpgradeButton);
    }

    private void Start() {
        HideUpgradeButton();
    }

    private void LateUpdate() {
        UpdateUpgradeButton();
    }

    private void OnDisable() {
        upgradeButton.onClick.RemoveListener(OnClickUpgradeButton);
        freeUpgradeButton.onClick.RemoveListener(OnClickFreeUpgradeButton);
    }

    public void OnInit(Player player, Camera playerCamera) {
        this.player = player;
        this.playerCamera = playerCamera;
    }

    private void UpdateUpgradeButton() {
        targetBaseHouse = player.GetBaseHouse();
        if (targetBaseHouse == null || !targetBaseHouse.CanShowUpgradeButton(player.TF.position)) {
            HideUpgradeButton();
            return;
        }

        Vector3 screenPoint = Utils.WorldToScreenPoint(playerCamera, targetBaseHouse.GetUpgradeButtonSpawnPoint());
        if (screenPoint.z <= 0f) {
            HideUpgradeButton();
            return;
        }

        buttonContainerTF.position = screenPoint;
        UpdateTittleText(targetBaseHouse.GetHouseLevel() + 1);
        UpdateCostText(targetBaseHouse.GetUpgradeCost());
        upgradeButton.interactable = targetBaseHouse.IsEnoughMoneyToUpgrade();
        ShowUpgradeButton();
    }

    private void OnClickUpgradeButton() {
        if (targetBaseHouse == null) return;

        targetBaseHouse.OnClickUpgrade();
    }

    private void OnClickFreeUpgradeButton() {
        if (targetBaseHouse == null) return;

        targetBaseHouse.OnClickFreeUpgrade();
    }

    private void UpdateCostText(BigDouble cost) {
        if (currentCost == cost) return;

        currentCost = cost;
        upgradeCostText.SetText(Constant.MONEY_SYMBOL + Utils.SciNotToUSName(cost));
    }

    private void HideUpgradeButton() {
        buttonContainerTF.gameObject.SetActive(false);
    }

    private void ShowUpgradeButton() {
        buttonContainerTF.gameObject.SetActive(true);
    }

    private void UpdateTittleText(int targetLevel) {
        if (currentTargetLevel == targetLevel) return;

        currentTargetLevel = targetLevel;
        tittleText.SetText(targetLevel + Constant.FLOOR_TITLE_SUFFIX);
    }
}