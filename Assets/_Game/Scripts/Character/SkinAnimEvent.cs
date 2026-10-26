using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// dat cung object voi Animator cua skin de nhan animation event
public class SkinAnimEvent : MonoBehaviour {
    private Character owner;

    public void OnInit(Character owner) {
        this.owner = owner;
    }

    // animation event goi dung frame gay cham muc tieu
    public void OnAttackHit() {
        if (owner == null) return;

        owner.OnAttackHit();
    }
}
