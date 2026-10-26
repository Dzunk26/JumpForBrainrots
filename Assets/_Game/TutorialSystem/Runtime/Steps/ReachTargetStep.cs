using UnityEngine;

// buoc dung chung: chi mui ten toi 1 Transform, xong khi diem start (player) di toi gan
public class ReachTargetStep : TutorialStep {
    [SerializeField] private Transform target;
    [SerializeField] private float reachRadius = 2f;

    public override bool IsCompleted() {
        if (target == null) return false;

        Vector3 offset = Controller.PathStartPoint.position - target.position;
        offset.y = 0f;
        return offset.sqrMagnitude <= reachRadius * reachRadius;
    }

    public override bool TryGetTargetPosition(out Vector3 position) {
        position = target != null ? target.position : Vector3.zero;
        return target != null;
    }
}
