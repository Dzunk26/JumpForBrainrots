using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Baseshop : MonoBehaviour {
    private void OnTriggerEnter(Collider other) {
        if (!other.CompareTag(Constant.PLAYER_TAG)) return;

        Player player = Cache.GetPlayer(other);
        if (player == null) return;

        CanvasSell canvasSell = UIManager.Instance.OpenUI<CanvasSell>();
        canvasSell.OnInit(player);
    }
}