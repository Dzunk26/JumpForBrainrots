using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum AnimState {
    Idle,
    Run,
    Climbing,
    StartJump,
    Falling,
    Interact,
    Death
}

public class CharacterVisual : MonoBehaviour {
    public Animator Animator => animator;
    public Transform TF {
        get {
            if (tf == null) {
                tf = transform;
            }

            return tf;
        }
    }

    [SerializeField] private Animator animator;

    private AnimState currentAnimState;
    private Transform tf;

    public void OnInit() {
        currentAnimState = AnimState.Idle;
    }

    public void OnIdle() {
        ChangeAnimState(AnimState.Idle);
    }

    public void OnRun() {
        ChangeAnimState(AnimState.Run);
    }

    public void OnClimbing() {
        ChangeAnimState(AnimState.Climbing);
    }

    public void SetClimbSpeed(float climbSpeed) {
        animator.SetFloat(Constant.ANIM_CLIMB_SPEED, climbSpeed);
    }

    public void OnStartJump() {
        ChangeAnimState(AnimState.StartJump);
    }

    public void OnFalling() {
        ChangeAnimState(AnimState.Falling);
    }

    public void OnInteract() {
        ChangeAnimState(AnimState.Interact);
    }

    public void OnWin() {

    }

    public void OnDead() {
        ChangeAnimState(AnimState.Death);
    }

    private void ChangeAnimState(AnimState animState) {
        if (currentAnimState == animState) return;

        animator.ResetTrigger(Cache.GetAnimName(currentAnimState));
        currentAnimState = animState;
        animator.SetTrigger(Cache.GetAnimName(currentAnimState));
        
    }
}