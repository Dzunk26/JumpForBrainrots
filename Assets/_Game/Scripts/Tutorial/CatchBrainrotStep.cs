using UnityEngine;

// chi toi brainrot con song gan nhat cua spawner, xong khi player dang cam brainrot vua bat
public class CatchBrainrotStep : BrainrotTutorialStep {
    [SerializeField] private BrainrotSpawner brainrotSpawner;

    private Brainrot targetBrainrot;

    public override void OnStepStart() {
        targetBrainrot = null;
    }

    public override bool IsCompleted() {
        return Player.IsHoldingCapturedBrainrot();
    }

    public override bool TryGetTargetPosition(out Vector3 position) {
        // brainrot bi bat / bien mat thi chuyen sang con gan nhat khac
        if (brainrotSpawner != null && (targetBrainrot == null || !targetBrainrot.IsAlive)) {
            targetBrainrot = brainrotSpawner.GetClosestBrainrot(Player.TF.position);
        }

        position = targetBrainrot != null ? targetBrainrot.TF.position : Vector3.zero;
        return targetBrainrot != null;
    }
}
