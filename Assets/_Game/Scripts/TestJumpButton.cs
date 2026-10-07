using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestJumpButton : MonoBehaviour {
    [SerializeField] private TourchDownButton JumpButton;
    [SerializeField] private Player player;

    private void OnEnable() {
        JumpButton.OnTouchDown += JumpButton_OnTouchDown;
    }

    private void OnDisable() {
        JumpButton.OnTouchDown -= JumpButton_OnTouchDown;
    }

    private void JumpButton_OnTouchDown(object sender, System.EventArgs e) {
        player.Jump();
    }
}