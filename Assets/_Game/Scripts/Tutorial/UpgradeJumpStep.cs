using UnityEngine;

// chi toi BaseUpgrade, xong khi player di toi va popup nang cap jump hien len
public class UpgradeJumpStep : BrainrotTutorialStep {
    [SerializeField] private Transform upgradeJumpPoint;

    public override bool IsCompleted() {
        return UIManager.Instance.IsUIOpen<CanvasUpgradeJump>();
    }

    public override bool TryGetTargetPosition(out Vector3 position) {
        position = upgradeJumpPoint != null ? upgradeJumpPoint.position : Vector3.zero;
        return upgradeJumpPoint != null;
    }
}
