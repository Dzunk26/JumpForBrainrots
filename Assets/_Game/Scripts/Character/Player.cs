using BreakInfinity;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Playables;

public enum PlayerState {
    Idle,
    Run,
    Climb,
    Jump,
    Fall,
    LadderJump,
    Dead,
    Knockback,
    KnockedDown,
    GettingUp,
}

public enum SlotInteractType {
    None,
    Place,
    Swap,
    PickUp
}

public class Player : Character, IDataPersistence {
    public bool IsDirty => baseHouse.IsDirty;

    [Header("Drop")]
    [SerializeField] private PoolType dropVFXPoolType = PoolType.DespawnPoofVFX;

    [SerializeField] private ListUpgradeConfigSO listUpgradeConfigSO;
    [SerializeField] private ListRebirthConfigSO listRebirthConfigSO;
    [SerializeField] private ListBrainrotSO listBrainrotSO;

    [SerializeField] private Camera playerCamera;
    [SerializeField] private Transform playerVisualContainer;
    [SerializeField] private CharacterController characterController;
    [SerializeField] private BrainrotInteractor brainrotInteractor;
    [SerializeField] private SlotInteractor baseSlotInteractor;
    [SerializeField] private CameraController cameraController;

    [SerializeField] private float rotateSpeed = 15f;
    [SerializeField] private float riseGravity = -150f;
    [SerializeField] private float fallGravity = -80f;

    [SerializeField] private float ladderEnterDelta = 0.5f;

    [SerializeField] private LayerMask floorLayerMask;
    [SerializeField] private float floorCheckDistance = 0.3f;
    [SerializeField] private LayerMask groundLayerMask;
    [SerializeField] private float groundRaycastDistance = 0.2f;
    [SerializeField] private LayerMask ladderLayerMask;
    [SerializeField] private float ladderRaycastDistance = 0.4f;
    [SerializeField] private float bodyRatio = 0.6f;
    [SerializeField] private float jumpBufferTime = 0.25f;
    [SerializeField] private float baseMaxFallSpeed = 50f;
    [SerializeField] private float fallCheckDistance = 2f;

    [SerializeField] private float fallMoveSpeedMultiplier = 0.7f;
    [SerializeField] private float climbMoveSpeedMultiplier = 0.8f;

    [SerializeField] private float statTimerMax = 3f;

    [SerializeField] private float groundStepOffset = 1.25f;

    [SerializeField] private float ladderJumpOffSpeed = 10f;
    [SerializeField] private float ladderJumpDuration = 0.3f;
    [SerializeField] private float ladderJumpUpVelocity = 2.5f;

    [SerializeField] private float climbInputDeadZone = 0.3f;

    [Header("Knockback")]
    [SerializeField] private float knockbackTimerMax = 0.3f;
    [SerializeField] private float knockbackDrag = 60f;
    [SerializeField] private float knockedDownSlideDrag = 40f; // cham dat con truot them mot doan

    [Header("Attack Assist")]
    [SerializeField] private float assistRange = 7f; // bam danh thi tim muc tieu trong pham vi nay (khong lon hon tam danh + tam cong them)
    [SerializeField] private float assistAngle = 160f; // goc tim phia truoc mat
    [SerializeField] private float assistCloseRange = 4f; // gan the nay thi o sau lung cung nham
    [SerializeField] private float assistDuration = 0.45f; // quay mat trong luc lay da, truoc frame gay cham
    [SerializeField] private float assistTurnSpeed = 25f;

    private float lastJumpPressedTime = -1f;
    private float moveSpeed;
    private float fallMoveSpeed;
    private float climbMoveSpeed;
    private Vector2 inputVector;
    private float verticalVelocity;
    private float initialJumpVelocity;
    private PlayerState currentState;
    private RaycastHit ladderHit;
    private float maxFallSpeed;
    private float stateTimer = 0f;
    private Vector3 ladderJumpDirection;
    private float ladderJumpTimer;
    private float climbInput;
    private List<BaseSlotData> savedSlots;
    private Vector3 knockbackVelocity;
    private float knockbackTimer;
    private bool isHoldingBat;

    protected override void OnEnable() {
        base.OnEnable();
        PlayerProgress.Instance.OnLoadPlayerData += PlayerProgress_OnLoadPlayerData;
        DataManager.Instance.OnReturnFromAfk += DataManager_OnReturnFromAfk;
    }

    private void Update() {
        ListenInput();

        UpdateStepOffset();

        HandleGravity();

        HandlePlayerState();

        HandleAttackAssist();
    }

    protected override void OnDisable() {
        base.OnDisable();
        if (PlayerProgress.HasInstance) {
            PlayerProgress.Instance.OnLoadPlayerData -= PlayerProgress_OnLoadPlayerData;
        }

        if (DataManager.HasInstance) {
            DataManager.Instance.OnReturnFromAfk -= DataManager_OnReturnFromAfk;
        }
    }

    private void PlayerProgress_OnLoadPlayerData(object sender, EventArgs e) {
        CalculateStats();
    }

    private void DataManager_OnReturnFromAfk(long afkSeconds) {
        baseHouse.AddAfkIncome(afkSeconds);
    }

    public override void OnInit() {
        characterName = PlayerProgress.Instance.GetPlayerName();
        base.OnInit();
        stateTimer = 0f;
        characterVisual.OnInit();
        ChangeSkin(SkinManager.Instance.GetEquippedSkinSO());
        ChangeState(PlayerState.Idle);
        CalculateStats();
        TeleportTo(spawnPoint);
    }

    public override void OnDespawn() {
        stateTimer = 0f;
        DropBrainrot();
        verticalVelocity = 0f;
        TeleportTo(spawnPoint);
        FaceToBaseHouse();
        characterVisual.OnInit();
        ChangeState(PlayerState.Idle);
        base.OnDespawn();
    }

    public void ChangeSkin(SkinSO skinSO) {
        characterVisual.OnChangeSkin(skinSO);
    }

    public override void StoreCapturedBrainrot() {
        if (heldBrainrotSource != HeldBrainrotSource.Captured) return;

        heldBrainrotSource = HeldBrainrotSource.Inventory;
        InventoryManager.Instance.AddAndSelectBrainrot(currentHoldBrainrotSO);
        SoundManager.Instance.PlayFx(FxID.StoreBrainrot);
        IndexManager.Instance.Collect(currentHoldBrainrotSO);
    }

    public override void InitBaseHouse() {
        baseHouse.OnInit(this, PlayerProgress.Instance.GetBaseHouseLevel());
        baseHouse.LoadSlots(savedSlots, listBrainrotSO);
        baseHouse.AddAfkIncome(DataManager.Instance.OfflineSeconds);
        savedSlots = null;
        baseSlotInteractor.OnInit(baseHouse);
    }

    public Vector3 GetDropButtonSpawnPoint() {
        return characterVisual.GetDropButtonSpawnPoint();
    }

    public void DropCapturedBrainrot() {
        if (heldBrainrotSource != HeldBrainrotSource.Captured) return;

        SimplePool.GetFromPool<DespawnVFX>(dropVFXPoolType, characterVisual.GetBrainrotHoldPosition(), Quaternion.identity).OnInit();
        DropBrainrot();
    }

    public void Jump() {
        lastJumpPressedTime = Time.time;

        if (currentState == PlayerState.Climb) {
            StartLadderJump();
            return;
        }

        if ((currentState == PlayerState.Idle || currentState == PlayerState.Run) && IsGrounded()) {
            StartJump();
        }
    }

    public void OnUpgrade(UpgradeType upgradeType) {
        CalculateStats();
    }

    public bool IsMaxUpgrade(UpgradeType upgradeType) {
        UpgradeConfigSO upgradeConfigSO = listUpgradeConfigSO.GetConfigByType(upgradeType);
        return upgradeConfigSO.IsMaxLevel(PlayerProgress.Instance.GetLevel(upgradeType));
    }

    public BigDouble GetUpgradeCost(UpgradeType upgradeType, int step) {
        UpgradeConfigSO upgradeConfigSO = listUpgradeConfigSO.GetConfigByType(upgradeType);
        int level = PlayerProgress.Instance.GetLevel(upgradeType);
        return upgradeConfigSO.GetTotalCost(level, level + upgradeConfigSO.GetUpgradeStep(level, step));
    }

    public void BuyUpgrade(UpgradeType upgradeType, int step) {
        if (IsMaxUpgrade(upgradeType)) return;

        BigDouble cost = GetUpgradeCost(upgradeType, step);
        if (!PlayerProgress.Instance.IsEnoughMoney(cost)) return;

        PlayerProgress.Instance.SpendMoney(cost);
        Upgrade(upgradeType, step);
    }

    public void Upgrade(UpgradeType upgradeType, int step) {
        UpgradeConfigSO upgradeConfigSO = listUpgradeConfigSO.GetConfigByType(upgradeType);
        int level = PlayerProgress.Instance.GetLevel(upgradeType);
        int actualStep = upgradeConfigSO.GetUpgradeStep(level, step);
        if (actualStep <= 0) return;

        PlayerProgress.Instance.IncreaseLevel(upgradeType, actualStep);
        OnUpgrade(upgradeType);
    }

    public override float GetMoneyMultiplier() {
        return listRebirthConfigSO.GetMoneyMultiplier(PlayerProgress.Instance.GetRebirthLevel());
    }

    public float GetMoveSpeed() {
        return moveSpeed;
    }

    public bool IsMaxRebirth() {
        return listRebirthConfigSO.IsMaxRebirth(PlayerProgress.Instance.GetRebirthLevel());
    }

    public int GetRebirthRequiredJumpLevel() {
        return listRebirthConfigSO.GetRequiredJumpLevel(PlayerProgress.Instance.GetRebirthLevel());
    }

    public bool IsReachedRebirth() {
        return PlayerProgress.Instance.GetLevel(UpgradeType.JumpPower) >= GetRebirthRequiredJumpLevel();
    }

    public BigDouble GetRebirthCost() {
        if (IsReachedRebirth()) return 0;

        UpgradeConfigSO jumpConfigSO = listUpgradeConfigSO.GetConfigByType(UpgradeType.JumpPower);
        return jumpConfigSO.GetTotalCost(PlayerProgress.Instance.GetLevel(UpgradeType.JumpPower), GetRebirthRequiredJumpLevel());
    }

    public void BuyRebirth() {
        if (IsMaxRebirth()) return;

        BigDouble cost = GetRebirthCost();
        if (!PlayerProgress.Instance.IsEnoughMoney(cost)) return;

        PlayerProgress.Instance.SpendMoney(cost);
        Rebirth();
    }

    public void Rebirth() {
        if (IsMaxRebirth()) return;

        PlayerProgress.Instance.Rebirth();
        CalculateStats();
    }

    public void LoadData(GameData gameData) {
        savedSlots = gameData.baseSlots;
    }

    public void SaveData(ref GameData gameData) {
        gameData.baseSlots = baseHouse.GetSlotsData();
        baseHouse.ClearDirty();
    }

    public override void Dead() {
        if (currentState == PlayerState.Dead) return;

        if (heldBrainrotSource == HeldBrainrotSource.Inventory) {
            InventoryManager.Instance.ClearSelection();
        }
        ScatterCapturedBrainrot(groundLayerMask);
        DropBrainrot();
        SoundManager.Instance.PlayFx(FxID.Dead);
        characterVisual.OnDead();
        ChangeState(PlayerState.Dead);
    }

    public override void OnHit(Vector3 knockbackVelocity, Character attacker) {
        if (currentState == PlayerState.Dead) return;

        isAttacking = false;
        ScatterCapturedBrainrot(groundLayerMask);
        this.knockbackVelocity = new Vector3(knockbackVelocity.x, 0f, knockbackVelocity.z);
        verticalVelocity = knockbackVelocity.y;
        knockbackTimer = 0f;
        characterVisual.OnKnockback(knockbackVelocity);
        ChangeState(PlayerState.Knockback);
    }
    

    public override void OnCaptureBrainrot() {
        if (!CanEquipBrainrot()) return;

        Brainrot brainrot = brainrotInteractor.GetSelectedTarget();
        if (brainrot == null) return;

        if (heldBrainrotSource == HeldBrainrotSource.Inventory || isHoldingBat) {
            InventoryManager.Instance.ClearSelection();
        }
        SetHoldingBat(false);
        HoldBrainrot(brainrot.GetBrainrotSO(), brainrot.TF.position, HeldBrainrotSource.Captured);
        brainrot.OnCaptured();
        brainrotInteractor.ClearSelectedTarget();
    }

    public Brainrot GetCaptureableTarget() {
        if (IsKnocked() || !IsOnFloor() || !CanEquipBrainrot()) return null;

        return brainrotInteractor.GetSelectedTarget();
    }

    public override void OnInteractBaseSlot() {
        BaseSlot baseSlot = GetInteractableBaseSlot();
        SlotInteractType interactType = GetSlotInteractType(baseSlot);

        switch (interactType) {
            case SlotInteractType.Place:
                PlaceBrainrot(baseSlot);
                break;
            case SlotInteractType.Swap:
                SwapBrainrot(baseSlot);
                break;
            case SlotInteractType.PickUp:
                PickUpBrainrot(baseSlot);
                break;
        }
    }

    public BaseSlot GetInteractableBaseSlot() {
        if (IsKnocked()) return null;

        return baseSlotInteractor.GetSelectedTarget();
    }

    public SlotInteractType GetSlotInteractType(BaseSlot baseSlot) {
        if (baseSlot == null || !baseSlot.IsUnlocked) return SlotInteractType.None;

        if (!baseSlot.IsActive) {
            return currentHoldBrainrotSO != null ? SlotInteractType.Place : SlotInteractType.None;
        }

        if (currentHoldBrainrotSO != null) {
            return SlotInteractType.Swap;
        }

        return SlotInteractType.PickUp;
    }

    public Camera GetPlayerCamera() {
        return playerCamera;
    }

    public bool CanEquipBrainrot() {
        return heldBrainrotSource != HeldBrainrotSource.Captured;
    }

    public bool IsHoldingBat() {
        return isHoldingBat;
    }

    // cam item vua chon trong hotbar / backpack: brainrot va gay khong cam cung luc
    public void EquipItem(ItemSO itemSO) {
        if (itemSO is BrainrotSO brainrotSO) {
            SetHoldingBat(false);
            EquipBrainrot(brainrotSO);
        }
        else {
            UnequipBrainrot();
            SetHoldingBat(true);
        }
    }

    public void UnequipItem() {
        UnequipBrainrot();
        SetHoldingBat(false);
    }

    private void EquipBrainrot(BrainrotSO brainrotSO) {
        HoldBrainrot(brainrotSO, characterVisual.GetBrainrotHoldPosition(), HeldBrainrotSource.Inventory);
    }

    private void UnequipBrainrot() {
        if (heldBrainrotSource != HeldBrainrotSource.Inventory) return;

        DropBrainrot();
    }

    private void SetHoldingBat(bool isHoldingBat) {
        if (this.isHoldingBat == isHoldingBat) return;

        this.isHoldingBat = isHoldingBat;
        characterVisual.SetHoldingBat(isHoldingBat);
        UIManager.Instance.GetUI<CanvasGamePlay>().SetAttackButtonVisible(isHoldingBat);
    }

    private void PlaceBrainrot(BaseSlot baseSlot) {
        Vector3 fromPosition = characterVisual.GetBrainrotHoldPosition();
        baseSlot.PlaceBrainrot(currentHoldBrainrotSO, fromPosition);
        InventoryManager.Instance.RemoveSelectedItem();
        DropBrainrot();
    }

    private void SwapBrainrot(BaseSlot baseSlot) {
        Vector3 holdPosition = characterVisual.GetBrainrotHoldPosition();
        Vector3 slotPosition = baseSlot.GetDisplayPosition();

        BrainrotSO oldBrainrotSO = baseSlot.SwapBrainrot(currentHoldBrainrotSO, holdPosition);
        InventoryManager.Instance.ReplaceSelectedItem(oldBrainrotSO);
        HoldBrainrot(oldBrainrotSO, slotPosition, HeldBrainrotSource.Inventory);
    }

    private void PickUpBrainrot(BaseSlot baseSlot) {
        BrainrotSO brainrot = baseSlot.RemoveBrainrot();
        Vector3 slotPosition = baseSlot.GetDisplayPosition();

        InventoryManager.Instance.AddAndSelectBrainrot(brainrot);
        SetHoldingBat(false);
        HoldBrainrot(brainrot, slotPosition, HeldBrainrotSource.Inventory);
    }

    public override bool IsHittable() {
        return currentState != PlayerState.Dead;
    }

    protected override void OnStartAttack() {
        attackTarget = FindAttackTarget();
    }

    // uu tien muc tieu gan va thang truoc mat; ngoai goc thi chi lay muc tieu rat gan
    private Character FindAttackTarget() {
        List<Character> characters = GetActiveCharacters();
        Character bestTarget = null;
        float bestScore = float.MaxValue;

        for (int i = 0; i < characters.Count; i++) {
            Character character = characters[i];
            if (character == this || !character.IsHittable()) continue;

            Vector3 offset = character.TF.position - TF.position;
            if (Mathf.Abs(offset.y) > hitHeightTolerance) continue;

            offset.y = 0f;
            float distance = offset.magnitude;
            if (distance > assistRange) continue;

            float angle = distance > 0.001f ? Vector3.Angle(playerVisualContainer.forward, offset) : 0f;
            if (angle > assistAngle * 0.5f && distance > assistCloseRange) continue;

            float score = distance * (1f + angle / 90f);
            if (score < bestScore) {
                bestScore = score;
                bestTarget = character;
            }
        }

        return bestTarget;
    }

    // trong luc lay da: quay mat ve muc tieu (chay sau HandlePlayerState de de len huong di chuyen)
    private void HandleAttackAssist() {
        if (attackTarget == null || !isAttacking || Time.time - lastAttackTime > assistDuration) return;
        if (IsKnocked() || currentState == PlayerState.Dead || currentState == PlayerState.Climb) return;

        Vector3 offset = attackTarget.TF.position - TF.position;
        offset.y = 0f;
        float distance = offset.magnitude;
        if (distance < 0.001f) return;

        playerVisualContainer.rotation = Quaternion.Slerp(playerVisualContainer.rotation, Quaternion.LookRotation(offset / distance), assistTurnSpeed * Time.deltaTime);
    }

    protected override bool CanAttack() {
        if (!isHoldingBat) return false;
        if (currentState == PlayerState.Dead || currentState == PlayerState.Climb || IsKnocked()) return false;

        return base.CanAttack();
    }

    private void CalculateStats() {
        characterStat.OnInit();

        foreach (UpgradeType upgradeType in Enum.GetValues(typeof(UpgradeType))) {
            UpgradeConfigSO upgradeConfigSO = listUpgradeConfigSO.GetConfigByType(upgradeType);
            if (upgradeConfigSO == null) continue;

            int level = PlayerProgress.Instance.GetLevel(upgradeType);
            characterStat.OnUpgraded(upgradeConfigSO.GetModifier(level), null);
        }

        float rebirthMoveSpeed = listRebirthConfigSO.GetMoveSpeedBonus(PlayerProgress.Instance.GetRebirthLevel());
        characterStat.OnUpgraded(new StatModifier(0f, rebirthMoveSpeed), null);

        SetupStats();
    }

    private void SetupStats() {
        moveSpeed = characterStat.GetMoveSpeed();
        climbMoveSpeed = moveSpeed * climbMoveSpeedMultiplier;
        fallMoveSpeed = moveSpeed * fallMoveSpeedMultiplier;
        initialJumpVelocity = Mathf.Sqrt(-2f * riseGravity * characterStat.GetJumpHeight());
        maxFallSpeed = Mathf.Max(baseMaxFallSpeed, initialJumpVelocity);
    }

    private void StartLadderJump() {
        ladderJumpDirection = GetLadderJumpDirection();
        ladderJumpTimer = 0f;
        lastJumpPressedTime = -1f;
        verticalVelocity = ladderJumpUpVelocity;
        characterVisual.OnStartJump();
        ChangeState(PlayerState.LadderJump);
    }

    private Vector3 GetLadderJumpDirection() {
        return -GetLadderDirection();
    }

    private void HandlePlayerState() {
        switch (currentState) {
            case PlayerState.Idle:
                HandleIdle();
                break;
            case PlayerState.Run:
                HandleRun();
                break;
            case PlayerState.Climb:
                HandleClimb();
                break;
            case PlayerState.Jump:
                HandleJump();
                break;
            case PlayerState.Fall:
                HandleFall();
                break;
            case PlayerState.LadderJump:
                HandleLadderJump();
                break;
            case PlayerState.Dead:
                HandleDead();
                break;
            case PlayerState.Knockback:
                HandleKnockback();
                break;
            case PlayerState.KnockedDown:
                HandleKnockedDown();
                break;
            case PlayerState.GettingUp:
                HandleGettingUp();
                break;
        }
    }

    private void HandleRun() {
        if (TryStartClimb()) return;

        if (IsFall()) {
            ChangeState(PlayerState.Fall);
            return;
        }

        characterVisual.OnRun();
        HandleMovement();

        if (!IsMoving()) {
            ChangeState(PlayerState.Idle);
        }
    }

    private void HandleMovement() {
        if (currentState == PlayerState.Climb) {
            HandleClimbMovement();
            return;
        }

        Vector3 moveDir = GetMoveDirection();
        Vector3 nextPosition = moveDir * GetHorizontalMoveSpeed() * Time.deltaTime;

        characterController.Move(nextPosition);
        RotateVisualTowards(moveDir);
    }

    private Vector3 GetMoveDirection() {
        Vector3 camForward = playerCamera.transform.forward;
        Vector3 camRight = playerCamera.transform.right;

        camForward.y = 0f;
        camRight.y = 0f;
        camForward.Normalize();
        camRight.Normalize();

        return (camForward * inputVector.y + camRight * inputVector.x).normalized;
    }

    private void HandleClimbMovement() {
        climbInput = GetClimbInput(); ;

        characterController.Move(Vector3.up * climbInput * climbMoveSpeed * Time.deltaTime);
        characterVisual.SetClimbSpeed(climbInput);

        RotateVisualTowards(GetLadderDirection());
    }

    private void RotateVisualTowards(Vector3 direction) {
        if (direction.sqrMagnitude < 0.001f) return;

        Quaternion targetRot = Quaternion.LookRotation(direction);
        playerVisualContainer.rotation = Quaternion.Slerp(playerVisualContainer.rotation, targetRot, rotateSpeed * Time.deltaTime);
    }

    private float GetClimbInput() {
        if (!IsMoving()) return 0f;

        float input = Vector3.Dot(GetMoveDirection(), GetLadderDirection());
        if (Mathf.Abs(input) < climbInputDeadZone) return 0f;

        return Mathf.Sign(input);
    }

    private bool CanStartClimb() {
        if (!IsMoving() || !IsFacingLadder()) return false;

        return Vector3.Dot(GetMoveDirection(), -ladderHit.normal) >= ladderEnterDelta;
    }

    private void StartClimb() {
        verticalVelocity = 0f;
        ChangeState(PlayerState.Climb);
    }

    private bool TryStartClimb() {
        if (!CanStartClimb()) return false;

        StartClimb();
        return true;
    }

    private void HandleClimb() {
        if (!IsFacingLadder()) {
            float forceAmount = characterController.height * bodyRatio;
            characterController.Move(playerVisualContainer.forward * characterController.radius + Vector3.up * forceAmount);
            ChangeState(PlayerState.Fall);
            return;
        }

        HandleMovement();
        characterVisual.OnClimbing();

        if (climbInput <= 0f && IsMoving() && IsGrounded()) {
            ChangeState(PlayerState.Run);
        }
    }

    private void HandleIdle() {
        characterVisual.OnIdle();

        if (IsFall()) {
            ChangeState(PlayerState.Fall);
            return;
        }

        if (IsMoving()) {
            ChangeState(PlayerState.Run);
        }
    }

    private void ListenInput() {
        inputVector = GameInput.Instance.GetMovementVectorNormalized();
        if (inputVector.sqrMagnitude >= 0.001f) return;
        inputVector = GameInput.Instance.GetWASDMovementVectorNormalized();
    }

    private void StartJump() {
        lastJumpPressedTime = -1f;
        verticalVelocity = initialJumpVelocity;
        characterVisual.OnStartJump();
        ChangeState(PlayerState.Jump);
    }

    private void HandleJump() {
        if (TryStartClimb()) return;

        HandleMovement();

        if (verticalVelocity <= 0f) {
            ChangeState(PlayerState.Fall);
        }
    }

    private void HandleLadderJump() {
        characterController.Move(ladderJumpDirection * ladderJumpOffSpeed * Time.deltaTime);

        ladderJumpTimer += Time.deltaTime;
        if (ladderJumpTimer >= ladderJumpDuration) {
            ChangeState(PlayerState.Fall);
        }
    }

    private Vector3 GetLadderDirection() {
        Vector3 direction = -ladderHit.normal;
        direction.y = 0f;
        return direction.normalized;
    }

    private void HandleGravity() {
        if (currentState == PlayerState.Climb) return;

        float deltaTime = Time.deltaTime;
        float deltaY;

        if (IsGrounded() && verticalVelocity < 0f && currentState != PlayerState.Jump) {
            verticalVelocity = Constant.GROUNDED_GRAVITY;
            deltaY = verticalVelocity * deltaTime;
        }
        else {
            float currentGravity = verticalVelocity < 0f ? fallGravity : riseGravity;
            deltaY = verticalVelocity * deltaTime + 0.5f * currentGravity * deltaTime * deltaTime;
            verticalVelocity += currentGravity * deltaTime;
            verticalVelocity = Mathf.Max(verticalVelocity, -maxFallSpeed);
        }

        CollisionFlags collisionFlags = characterController.Move(Vector3.up * deltaY);
        if (verticalVelocity > 0f && (collisionFlags & CollisionFlags.Above) != 0) {
            verticalVelocity = 0f;
        }
    }

    private void HandleFall() {
        if (TryStartClimb()) return;

        HandleMovement();
        characterVisual.OnFalling();

        if (IsGrounded()) {
            SoundManager.Instance.PlayFx(FxID.Landing);

            if (Time.time - lastJumpPressedTime <= jumpBufferTime) {
                StartJump();
                return;
            }

            verticalVelocity = Constant.GROUNDED_GRAVITY;

            ChangeState(IsMoving() ? PlayerState.Run : PlayerState.Idle);
        }
    }

    // bay + lon vong, cham dat thi nam xuong (khong nhan input)
    private void HandleKnockback() {
        characterController.Move(knockbackVelocity * Time.deltaTime);
        knockbackVelocity = Vector3.MoveTowards(knockbackVelocity, Vector3.zero, knockbackDrag * Time.deltaTime);

        knockbackTimer += Time.deltaTime;
        if (knockbackTimer < knockbackTimerMax || !IsGrounded()) return;

        SoundManager.Instance.PlayFx(FxID.Landing);
        knockbackTimer = 0f;
        KnockDown(playerVisualContainer);
        ChangeState(PlayerState.KnockedDown);
    }

    // nam tren dat, truot not quan tinh roi dung day
    private void HandleKnockedDown() {
        characterController.Move(knockbackVelocity * Time.deltaTime);
        knockbackVelocity = Vector3.MoveTowards(knockbackVelocity, Vector3.zero, knockedDownSlideDrag * Time.deltaTime);

        knockbackTimer += Time.deltaTime;
        if (knockbackTimer < lyingTimerMax) return;

        knockbackTimer = 0f;
        characterVisual.OnGetUp();
        ChangeState(PlayerState.GettingUp);
    }

    // anim dung day gan xong thi tra lai dieu khien, idle / run tu blend chong len phan cuoi
    private void HandleGettingUp() {
        knockbackTimer += Time.deltaTime;
        if (!characterVisual.IsGetUpFinished(getUpExitNormalizedTime) && knockbackTimer < getUpTimerMax) return;

        ChangeState(IsMoving() ? PlayerState.Run : PlayerState.Idle);
    }

    public override bool IsKnocked() {
        return currentState == PlayerState.Knockback || currentState == PlayerState.KnockedDown || currentState == PlayerState.GettingUp;
    }

    private void HandleDead() {
        stateTimer += Time.deltaTime;
        if (stateTimer < statTimerMax) return;

        OnDespawn();
    }

    private void UpdateStepOffset() {
        characterController.stepOffset = IsGrounded() ? groundStepOffset : 0f;
    }

    private float GetHorizontalMoveSpeed() {
        switch (currentState) {
            case PlayerState.Jump:
                return fallMoveSpeed;
            case PlayerState.Fall:
                return fallMoveSpeed;
            default:
                return moveSpeed;
        }
    }

    private bool HasGroundBelow() {
        Vector3 origin = TF.position + Vector3.up * 0.5f;
        return Physics.Raycast(origin, Vector3.down, 0.5f + fallCheckDistance, groundLayerMask, QueryTriggerInteraction.Ignore);
    }

    private bool IsFall() {
        return !IsGrounded() && !HasGroundBelow();
    }

    private bool IsGrounded() {
        float radius = characterController.radius;
        Vector3 center = TF.position + characterController.center;
        float halfHeight = characterController.height * 0.5f - radius;
        float offset = 0.1f;

        Vector3 point1 = center + Vector3.up * (halfHeight + offset);        
        Vector3 point2 = center + Vector3.down * halfHeight + (Vector3.up * offset);

        return Physics.CapsuleCast(point1, point2, radius, Vector3.down, groundRaycastDistance, groundLayerMask);
    }

    private bool IsOnFloor() {
        if (!IsGrounded()) return false;

        Vector3 origin = TF.position + Vector3.up * 0.1f;
        return Physics.Raycast(origin, Vector3.down, 0.1f + floorCheckDistance, floorLayerMask, QueryTriggerInteraction.Ignore);
    }

    private bool IsFacingLadder() {
        Vector3 startPosition = TF.position + Vector3.up * characterController.height * bodyRatio;
        Vector3 direction = playerVisualContainer.forward;
        float distance = characterController.radius + ladderRaycastDistance;
        return Physics.Raycast(startPosition, direction, out ladderHit, distance, ladderLayerMask, QueryTriggerInteraction.Collide);
    }


    private bool IsMoving() {
        return inputVector.sqrMagnitude > 0.01f;
    }

    private void TeleportTo(Vector3 position) {
        characterController.enabled = false;
        TF.position = position;
        characterController.enabled = true;
    }

    private void FaceToBaseHouse() {
        Vector3 direction = baseHouse.GetSpawnForward();
        direction.y = 0f;
        if (direction.sqrMagnitude < 0.001f) return;

        playerVisualContainer.rotation = Quaternion.LookRotation(direction);
        cameraController.LookAtDirection(direction);
    }

    private void ChangeState(PlayerState state) {
        if (currentState == state) return;

        currentState = state;
        cameraController.SetAirborne(IsAirborneState(state));
    }

    private bool IsAirborneState(PlayerState state) {
        return state == PlayerState.Jump || state == PlayerState.Fall || state == PlayerState.LadderJump || state == PlayerState.Knockback;
    }
}