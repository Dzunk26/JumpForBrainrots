using UnityEngine;

// chi toi slot trong dau tien trong base, xong khi base co them 1 slot da dat brainrot
public class PlaceBrainrotStep : BrainrotTutorialStep {
    private BaseSlot targetSlot;
    private int startActiveSlotCount;

    // slot vua dat, CollectMoneyStep dung de chi dung slot do
    public BaseSlot PlacedSlot { get; private set; }

    public override void OnStepStart() {
        startActiveSlotCount = GetActiveSlotCount();
        targetSlot = GetFirstEmptySlot();
        PlacedSlot = null;
    }

    public override bool IsCompleted() {
        if (GetActiveSlotCount() <= startActiveSlotCount) return false;

        PlacedSlot = targetSlot != null && targetSlot.IsActive ? targetSlot : GetFirstActiveSlot();
        return true;
    }

    public override bool TryGetTargetPosition(out Vector3 position) {
        if (targetSlot == null || targetSlot.IsActive) targetSlot = GetFirstEmptySlot();

        position = targetSlot != null ? targetSlot.TF.position : Vector3.zero;
        return targetSlot != null;
    }
}
