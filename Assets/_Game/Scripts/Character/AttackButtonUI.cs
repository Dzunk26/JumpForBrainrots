using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AttackButtonUI : MonoBehaviour {
    [SerializeField] private TourchDownButton attackButton;
    [SerializeField] private Player player;

    private void OnEnable() {
        attackButton.OnTouchDown += AttackButton_OnTouchDown;
    }

    private void OnDisable() {
        attackButton.OnTouchDown -= AttackButton_OnTouchDown;
    }

    public void OnInit(Player player) {
        this.player = player;
    }

    private void AttackButton_OnTouchDown(object sender, System.EventArgs e) {
        player.Attack();
    }
}
