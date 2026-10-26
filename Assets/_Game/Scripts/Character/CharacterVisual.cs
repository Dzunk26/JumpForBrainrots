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
    Death,
    Attack
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
    [SerializeField] private Character owner;

    [Header("Bat")]
    [SerializeField] private Transform batPrefab;
    [SerializeField] private int attackLayerIndex = 1;
    [SerializeField] private int attackBodyLayerIndex = 2;

    private Transform bat;

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
        AttachBat();

        // Animator vừa bật lại sẽ về state mặc định, nên phải set lại state và param hiện tại
        animator.SetFloat(Constant.ANIM_CLIMB_SPEED, climbSpeed);
        animator.SetTrigger(Cache.GetAnimName(currentAnimState));
        UpdateAttackLayerWeight();
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

    public void OnAttack() {
        animator.Play(Cache.GetAnimHash(AnimState.Attack), attackLayerIndex, 0f);
        animator.Play(Cache.GetAnimHash(AnimState.Attack), attackBodyLayerIndex, 0f);
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
        UpdateAttackLayerWeight();
    }

    // leo thang va chet thi tat layer cam gay / vung gay de khong de len anim goc
    private void UpdateAttackLayerWeight() {
        if (animator.layerCount <= attackBodyLayerIndex) return;

        bool isUseAttackLayer = currentAnimState != AnimState.Climbing && currentAnimState != AnimState.Death;
        animator.SetLayerWeight(attackLayerIndex, isUseAttackLayer ? 1f : 0f);
        animator.SetLayerWeight(attackBodyLayerIndex, isUseAttackLayer ? 1f : 0f);
        if (bat != null) {
            bat.gameObject.SetActive(isUseAttackLayer);
        }
    }

    private void AttachBat() {
        if (batPrefab == null) return;

        if (bat == null) {
            bat = Instantiate(batPrefab);
        }

        bat.SetParent(currentSkin.GetBatHoldPoint(), false);
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
            dictSkinModel[skinSO].OnInit(owner);
        }

        return dictSkinModel[skinSO];
    }
}