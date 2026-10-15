using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BrainrotInteractor : TargetInteractor<Brainrot> {
    protected override Brainrot GetTarget(Collider collider) {
        return Cache.GetBrainrot(collider);
    }

    protected override Vector3 GetTargetPosition(Brainrot brainrot) {
        return brainrot.TF.position;
    }

    protected override bool IsValidTarget(Brainrot brainrot) {
        return brainrot.IsAlive;
    }
}