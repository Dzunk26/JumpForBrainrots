using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SlotInteractor : TargetInteractor<BaseSlot> {
    protected override BaseSlot GetTarget(Collider collider) {
        return Cache.GetBaseSlot(collider);
    }

    protected override Vector3 GetTargetPosition(BaseSlot target) {
        return TF.position;
    }

    protected override bool IsValidTarget(BaseSlot target) {
        return target.IsActive;
    }
}