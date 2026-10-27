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
    Attack,
    Knockback,
    Lying,
    GettingUp,
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

    [Header("Knockback")]
    [SerializeField] private float tumbleSpeed = 720f; // do / giay, lon vong luc bay
    [SerializeField] private float tumblePivotHeight = 2f; // lon quanh giua nguoi thay vi quanh chan
    [SerializeField] private float lyingAngle = 90f; // nam ngua: nga ra sau bao nhieu do
    [SerializeField] private float lyingHeight = 0.5f; // nua be day than, de lung cham dat chu khong lun xuong
    [SerializeField] private float lieDownDuration = 0.2f; // cham dat: tu dang lon vong xoay ve nam phang
    [SerializeField] private float getUpTurnDuration = 0.25f; // phai bang thoi gian transition sang GettingUp trong controller
    [SerializeField] private float getUpBlendDuration = 0.35f; // dung day xong blend sang idle / run lau hon transition mac dinh cho muot
    [SerializeField] private float attackLayerFadeDuration = 0.25f; // layer cam gay hien / tat dan, tay khong bi giat

    // ten state trong Base Layer de crossfade truc tiep khi dung day xong
    private static readonly int IDLE_STATE_HASH = Animator.StringToHash("Idle");
    private static readonly int RUN_STATE_HASH = Animator.StringToHash("Running");

    private Transform bat;
    private bool isHoldingBat;

    private Dictionary<SkinSO, SkinModel> dictSkinModel = new Dictionary<SkinSO, SkinModel>();
    private SkinModel currentSkin;
    private float climbSpeed;

    private Dictionary<BrainrotSO, BrainrotModel> dictBrainrotModel = new Dictionary<BrainrotSO, BrainrotModel>();
    private BrainrotModel currentModel;

    private AnimState currentAnimState;
    private Transform tf;

    private Vector3 skinRootLocalPosition;
    private Quaternion skinRootLocalRotation;
    private bool isTumbling;
    private Vector3 tumbleAxis;
    private float tumbleAngle;
    private Quaternion poseRotation = Quaternion.identity;
    private float poseHeight;
    private Quaternion blendFromRotation;
    private Quaternion blendToRotation;
    private float blendFromHeight;
    private float blendToHeight;
    private float blendDuration;
    private float blendTimer = -1f;
    private float attackLayerWeight;
    private float attackLayerTargetWeight;

    private void Awake() {
        skinRootLocalPosition = skinRoot.localPosition;
        skinRootLocalRotation = skinRoot.localRotation;
    }

    private void Start() {
        currentAnimState = AnimState.Idle;
    }

    private void Update() {
        HandleAttackLayerFade();
    }

    private void LateUpdate() {
        HandleTumble();
    }

    public void OnInit() {
        OnDropBrainrot();
        BlendSkinRootTo(Quaternion.identity, 0f, 0f);
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
        SetAttackLayerWeight(attackLayerTargetWeight);
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

    // bat / tat gay tren tay: nguoi choi theo item dang chon, bot chi lay ra luc danh
    public void SetHoldingBat(bool isHoldingBat) {
        this.isHoldingBat = isHoldingBat;
        UpdateAttackLayerWeight();
    }

    public void OnAttack() {
        animator.Play(Cache.GetAnimHash(AnimState.Attack), attackLayerIndex, 0f);
        animator.Play(Cache.GetAnimHash(AnimState.Attack), attackBodyLayerIndex, 0f);
    }

    public void OnWin() {

    }

    public void OnDead() {
        BlendSkinRootTo(Quaternion.identity, 0f, getUpTurnDuration);
        ChangeAnimState(AnimState.Death);
    }

    // bi danh: khua tay chan va lon vong theo huong bay (dau chui ve huong bay)
    public void OnKnockback(Vector3 knockbackVelocity) {
        Vector3 direction = new Vector3(knockbackVelocity.x, 0f, knockbackVelocity.z);
        if (direction.sqrMagnitude < 0.001f) direction = -TF.forward;

        tumbleAxis = skinRoot.parent.InverseTransformDirection(Vector3.Cross(Vector3.up, direction.normalized)).normalized;
        tumbleAngle = 0f;
        isTumbling = true;
        blendTimer = -1f;
        ChangeAnimState(AnimState.Knockback);
    }

    // cham dat: xoay ca nguoi nam ngua phang xuong dat, tay chan tha long
    // yawOffset: goc owner vua xoay huong nhan vat (nam huong ngau nhien), bu lai de model khong bi giat
    public void OnKnockedDown(float yawOffset) {
        poseRotation = Quaternion.Inverse(Quaternion.AngleAxis(yawOffset, Vector3.up)) * poseRotation;
        SetSkinRootPose(poseRotation, poseHeight);

        Quaternion lyingRotation = Quaternion.AngleAxis(-lyingAngle, Vector3.right);
        BlendSkinRootTo(lyingRotation, lyingHeight - tumblePivotHeight, lieDownDuration);
        ChangeAnimState(AnimState.Lying);
    }

    // xoay nguoi ve thang dung luc anim Getting Up blend vao (root cua anim dang nam nen hai cai bu nhau)
    public void OnGetUp() {
        BlendSkinRootTo(Quaternion.identity, 0f, getUpTurnDuration);
        ChangeAnimState(AnimState.GettingUp);
    }

    // dung day gan xong thi cho chuyen sang idle / run luon de blend chong len doan cuoi, khong bi khung
    public bool IsGetUpFinished(float exitNormalizedTime) {
        AnimatorStateInfo stateInfo = animator.GetCurrentAnimatorStateInfo(0);
        return stateInfo.shortNameHash == Cache.GetAnimHash(AnimState.GettingUp) && stateInfo.normalizedTime >= exitNormalizedTime;
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
        bool isFromGetUp = currentAnimState == AnimState.GettingUp && (animState == AnimState.Idle || animState == AnimState.Run);
        currentAnimState = animState;

        if (isFromGetUp) {
            animator.CrossFadeInFixedTime(animState == AnimState.Run ? RUN_STATE_HASH : IDLE_STATE_HASH, getUpBlendDuration, 0);
        }
        else {
            animator.SetTrigger(Cache.GetAnimName(currentAnimState));
        }
        UpdateAttackLayerWeight();
    }

    // khong cam gay, leo thang, chet va bi danh van thi tat layer cam gay / vung gay de khong de len anim goc
    private void UpdateAttackLayerWeight() {
        if (animator.layerCount <= attackBodyLayerIndex) return;

        bool isUseAttackLayer = isHoldingBat && !IsBodyBusyState(currentAnimState);
        attackLayerTargetWeight = isUseAttackLayer ? 1f : 0f;
        if (bat != null) {
            bat.gameObject.SetActive(isUseAttackLayer);
        }
    }

    private void HandleAttackLayerFade() {
        if (Mathf.Approximately(attackLayerWeight, attackLayerTargetWeight)) return;

        float speed = attackLayerFadeDuration > 0f ? Time.deltaTime / attackLayerFadeDuration : 1f;
        SetAttackLayerWeight(Mathf.MoveTowards(attackLayerWeight, attackLayerTargetWeight, speed));
    }

    private void SetAttackLayerWeight(float weight) {
        attackLayerWeight = weight;
        if (animator.layerCount <= attackBodyLayerIndex) return;

        animator.SetLayerWeight(attackLayerIndex, weight);
        animator.SetLayerWeight(attackBodyLayerIndex, weight);
    }

    private bool IsBodyBusyState(AnimState animState) {
        return animState == AnimState.Climbing || animState == AnimState.Death
            || animState == AnimState.Knockback || animState == AnimState.Lying || animState == AnimState.GettingUp;
    }

    private void HandleTumble() {
        if (isTumbling) {
            tumbleAngle += tumbleSpeed * Time.deltaTime;
            SetSkinRootPose(Quaternion.AngleAxis(tumbleAngle, tumbleAxis), 0f);
            return;
        }

        if (blendTimer < 0f) return;

        blendTimer += Time.deltaTime;
        float t = Mathf.Clamp01(blendTimer / blendDuration);
        SetSkinRootPose(Quaternion.Slerp(blendFromRotation, blendToRotation, t), Mathf.Lerp(blendFromHeight, blendToHeight, t));
        if (t >= 1f) {
            blendTimer = -1f;
        }
    }

    // dung lon vong, xoay / nang ha skinRoot dan ve tu the moi. duration <= 0 thi dat ngay (hoi sinh)
    private void BlendSkinRootTo(Quaternion rotation, float height, float duration) {
        isTumbling = false;

        if (duration <= 0f) {
            blendTimer = -1f;
            SetSkinRootPose(rotation, height);
            return;
        }

        blendFromRotation = poseRotation;
        blendFromHeight = poseHeight;
        blendToRotation = rotation;
        blendToHeight = height;
        blendDuration = duration;
        blendTimer = 0f;
    }

    // xoay skinRoot quanh diem giua nguoi roi nang / ha them height (trong khong gian cha cua skinRoot)
    private void SetSkinRootPose(Quaternion rotation, float height) {
        poseRotation = rotation;
        poseHeight = height;

        Vector3 pivot = skinRootLocalPosition + Vector3.up * tumblePivotHeight;
        skinRoot.localRotation = rotation * skinRootLocalRotation;
        skinRoot.localPosition = pivot + rotation * (skinRootLocalPosition - pivot) + Vector3.up * height;
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