using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestUpgradeButton : MonoBehaviour {
    [SerializeField] private TourchDownButton upgradeButton;
    [SerializeField] private Player player;

    private void OnEnable() {
        upgradeButton.OnTouchDown += UpgradeButton_OnTouchDown;
    }

    private void OnDisable() {
        upgradeButton.OnTouchDown -= UpgradeButton_OnTouchDown;
    }

    private void UpgradeButton_OnTouchDown(object sender, System.EventArgs e) {
        player.TestOnUpgrade(); ;
    }
}