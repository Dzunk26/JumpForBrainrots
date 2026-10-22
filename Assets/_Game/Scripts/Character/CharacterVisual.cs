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
    [SerializeField] private Transform dropButtonSpawnPoint;
    [SerializeField] private Transform skinRoot;

    private Dictionary<SkinSO, SkinModel> dictSkinModel = new Dictionary<SkinSO, SkinModel>();
    private SkinModel currentSkin;
    private float climbSpeed;

    private Dictionary<BrainrotSO, BrainrotModel> dictBrainrotModel = new Dictionary<BrainrotSO, BrainrotModel>();
    private BrainrotModel currentModel;

    private AnimState currentAnimState;
    private Transform tf;

    private void Start() {
        currentAnimState = AnimState.Idle;
    }

    public void OnInit() {
        OnDropBrainrot();
    }

    public void OnChangeSkin(SkinSO skinSO) {
        if (currentSkin != null) {
            currentSkin.DeActive();
        }

        currentSkin = GetSkinModel(skinSO);
        currentSkin.Active();
        animator = currentSkin.Animator;

        // Animator vừa bật lại sẽ về state mặc định, nên phải set lại state và param hiện tại
        animator.SetFloat(Constant.ANIM_CLIMB_SPEED, climbSpeed);
        animator.SetTrigger(Cache.GetAnimName(currentAnimState));
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
        currentModel.ChangeAnimType(BrainrotAnimType.Wiggle);
    }

    public void OnDropBrainrot() {
        if (currentModel == null) return;

        currentModel.DeActive();
        currentModel = null;
    }

    public Vector3 GetBrainrotHoldPosition() {
        return brainrotHoldPoint.position;
    }

    public void FlyBrainrotFrom(Vector3 fromPosition) {
        if (currentModel == null) return;

        currentModel.FlyFrom(fromPosition);
    }

    public Vector3 GetDropButtonSpawnPoint() {
        return dropButtonSpawnPoint.position;
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
            dictBrainrotModel[brainrotSO] = Instantiate(brainrotSO.GetBrainrotModel(), brainrotHoldPoint);
            dictBrainrotModel[brainrotSO].ResetLocalPosition();
        }

        return dictBrainrotModel[brainrotSO];
    }

    private SkinModel GetSkinModel(SkinSO skinSO) {
        if (!dictSkinModel.ContainsKey(skinSO)) {
            dictSkinModel[skinSO] = Instantiate(skinSO.GetSkinModel(), skinRoot);
        }

        return dictSkinModel[skinSO];
    }
}