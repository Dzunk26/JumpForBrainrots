using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BaseUpgrade : MonoBehaviour {
    private void OnTriggerEnter(Collider other) {
        if (!other.CompareTag(Constant.PLAYER_TAG)) return;

        Player player = Cache.GetPlayer(other);
        if (player == null) return;

        CanvasUpgradeJump canvasUpgradeJump = UIManager.Instance.OpenUI<CanvasUpgradeJump>();
        canvasUpgradeJump.OnInit(player);
    }
}