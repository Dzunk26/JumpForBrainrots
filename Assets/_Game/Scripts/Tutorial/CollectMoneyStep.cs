using UnityEngine;

// chi toi slot vua dat (hoac slot co brainrot dau tien), xong khi player thu tien o bat ky slot nao trong base cua minh
public class CollectMoneyStep : BrainrotTutorialStep {
    [SerializeField] private PlaceBrainrotStep placeBrainrotStep;

    private BaseSlot targetSlot;
    private bool isCollected;

    private void OnDisable() {
        BaseSlot.OnAnyIncomeCollected -= BaseSlot_OnAnyIncomeCollected;
    }

    public override void OnStepStart() {
        targetSlot = placeBrainrotStep != null ? placeBrainrotStep.PlacedSlot : null;
        isCollected = false;
        BaseSlot.OnAnyIncomeCollected += BaseSlot_OnAnyIncomeCollected;
    }

    public override void OnStepEnd() {
        BaseSlot.OnAnyIncomeCollected -= BaseSlot_OnAnyIncomeCollected;
    }

    public override bool IsCompleted() {
        return isCollected;
    }

    public override bool TryGetTargetPosition(out Vector3 position) {
        // slot bi nhac brainrot ra thi chuyen sang slot co brainrot khac
        if (targetSlot == null || !targetSlot.IsActive) targetSlot = GetFirstActiveSlot();

        position = targetSlot != null ? targetSlot.GetBaseSlotTriggerPosition() : Vector3.zero;
        return targetSlot != null;
    }

    private void BaseSlot_OnAnyIncomeCollected(BaseSlot baseSlot) {
        if (!baseSlot.IsMatchBaseHouse(Player.GetBaseHouse())) return;

        isCollected = true;
    }
}
