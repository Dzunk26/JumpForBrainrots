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
    [SerializeField] private Transform brainrotHoldPoint;

    private Dictionary<BrainrotSO, BrainrotModel> dictBrainrotModel = new Dictionary<BrainrotSO, BrainrotModel>();
    private BrainrotModel currentModel;

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
        PlayAnim(AnimState.StartJump);
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

    public void OnCaptureBrainrot(BrainrotSO brainrotSO) {
        if (currentModel != null) {
            currentModel.DeActive();
        }

        currentModel = GetBrainrotModel(brainrotSO);
        currentModel.Active();
    }

    private void ChangeAnimState(AnimState animState) {
        if (currentAnimState == animState) return;

        animator.ResetTrigger(Cache.GetAnimName(currentAnimState));
        currentAnimState = animState;
        animator.SetTrigger(Cache.GetAnimName(currentAnimState));
    }

    private void PlayAnim(AnimState animState) {
        int hash = Cache.GetAnimHash(animState);
        animator.Play(hash, 0, 0f);
    }

    private BrainrotModel GetBrainrotModel(BrainrotSO brainrotSO) {
        if (!dictBrainrotModel.ContainsKey(brainrotSO)) {
            dictBrainrotModel[brainrotSO] = Instantiate(brainrotSO.GetBrainrotModel());
        }

        return dictBrainrotModel[brainrotSO];
    }
}