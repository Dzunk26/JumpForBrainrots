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
}

public enum SlotInteractType {
    None,
    Place,
    Swap,
    PickUp
}

public class Player : Character {
    [SerializeField] private ListUpgradeConfigSO listUpgradeConfigSO;

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
    [SerializeField] private float climpMoveSpeedMultiplier = 0.8f;

    [SerializeField] private float statTimerMax = 3f;

    [SerializeField] private float groundStepOffset = 1.25f;

    [SerializeField] private float ladderJumpOffSpeed = 10f;
    [SerializeField] private float ladderJumpDuration = 0.3f;

    [SerializeField] private float climbInputDeadZone = 0.3f;

    private float lastJumpPressedTime = -1f;
    private float moveSpeed;
    private float fallMoveSpeed;
    private float climpMoveSpeed;
    private Vector2 inputVector;
    private bool isJumping;
    private float verticalVelocity;
    private float initialJumpVelocity;
    private PlayerState currentState;
    private RaycastHit ladderHit;
    private float maxFallSpeed;
    private float stateTimer = 0f;
    private Vector3 ladderJumpDirection;
    private float ladderJumpUpVelocity;
    private float ladderJumpTimer;
    private float climbInput;

    private void OnEnable() {
        PlayerProgress.Instance.OnLoadPlayerData += PlayerProgress_OnLoadPlayerData;
    }

    private void Update() {
        ListenInput();

        HasGroundBelow();

        UpdateStepOffset();

        HandleGravity();

        HandlePlayerState();
    }

    private void OnDisable() {
        if (!PlayerProgress.HasInstance) return;

        PlayerProgress.Instance.OnLoadPlayerData -= PlayerProgress_OnLoadPlayerData;
    }

    private void PlayerProgress_OnLoadPlayerData(object sender, EventArgs e) {
        CalculateStats();
    }

    public override void OnInit() {
        base.OnInit();
        stateTimer = 0f;
        characterVisual.OnInit();
        isJumping = false;
        ChangeState(PlayerState.Idle);
        CalculateStats();
        TeleportTo(spawnPoint);
    }

    public override void OnDespawn() {
        stateTimer = 0f;
        currentCapturedBrainrotSO = null;
        isJumping = false;
        verticalVelocity = 0f;
        TeleportTo(spawnPoint);
        FaceToBaseHouse();
        characterVisual.OnInit();
        ChangeState(PlayerState.Idle);
        base.OnDespawn();
    }

    public override void InitBaseHouse() {
        baseHouse.OnInit(this, PlayerProgress.Instance.GetBaseHouseLevel());
        baseSlotInteractor.OnInit(baseHouse);
    }

    public override void UpgradeBaseHouse() {
        if (baseHouse.IsMaxLevel()) return;

        //double cost = baseHouse.GetUpgradeCost();
        //if (!PlayerProgress.Instance.IsEnoughMoney(cost)) return;

        //PlayerProgress.Instance.SpendMoney(cost);
        baseHouse.Upgrade();
        PlayerProgress.Instance.SetBaseHouseLevel(baseHouse.GetHouseLevel());
    }

    public void Jump() {
        lastJumpPressedTime = Time.time;

        if (currentState == PlayerState.Climb) {
            StartLadderJump();
            return;
        }

        if ((currentState == PlayerState.Idle || currentState == PlayerState.Run) && IsGrounded() && !isJumping) {
            StartJump();
        }
    }

    public void TestOnUpgrade() {
        UpgradeType upgradeType = UpgradeType.JumpPower;
        PlayerProgress.Instance.IncreaseLevel(upgradeType);
        OnUpgrade(upgradeType);
    }

    public void OnUpgrade(UpgradeType upgradeType) {
        CalculateStats();
    }

    public override void Dead() {
        if (currentState == PlayerState.Dead) return;

        DropBrainrot();
        characterVisual.OnDead();
        ChangeState(PlayerState.Dead);
    }


    public override void OnCaptureBrainrot() {
        if (!CanCapture()) return;

        Brainrot brainrot = brainrotInteractor.GetSelectedTarget();
        if (brainrot == null) return;

        HoldBrainrot(brainrot.GetBrainrotSO(), brainrot.TF.position);
        brainrot.OnCaptured();
        brainrotInteractor.ClearSelectedTarget();
    }

    public Brainrot GetCaptureableTarget() {
        if (!IsOnFloor() || !CanCapture()) return null;

        return brainrotInteractor.GetSelectedTarget();
    }

    private bool CanCapture() {
        return currentCapturedBrainrotSO == null;
    }

    public override void OnInteractBaseSlot() {
        BaseSlot baseSlot = baseSlotInteractor.GetSelectedTarget();
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
        return baseSlotInteractor.GetSelectedTarget();
    }

    public SlotInteractType GetSlotInteractType(BaseSlot baseSlot) {
        if (baseSlot == null || !baseSlot.IsUnlocked) return SlotInteractType.None;

        bool isHoldingBrainrot = currentCapturedBrainrotSO != null;

        if (!baseSlot.IsActive) {
            return SlotInteractType.Place;
        }

        if (isHoldingBrainrot) {
            return SlotInteractType.Swap;
        }

        return SlotInteractType.PickUp;
    }

    public Camera GetPlayerCamera() {
        return playerCamera;
    }

    private void PlaceBrainrot(BaseSlot baseSlot) {
        Vector3 fromPosition = characterVisual.GetBrainrotHoldPosition();
        baseSlot.PlaceBrainrot(currentCapturedBrainrotSO, fromPosition);
        DropBrainrot();
    }

    private void SwapBrainrot(BaseSlot baseSlot) {
        Vector3 holdPosition = characterVisual.GetBrainrotHoldPosition();
        Vector3 slotPosition = baseSlot.GetDisplayPosition();

        BrainrotSO oldBrainrotSO = baseSlot.SwapBrainrot(currentCapturedBrainrotSO, holdPosition);
        HoldBrainrot(oldBrainrotSO, slotPosition);
    }

    private void PickUpBrainrot(BaseSlot baseSlot) {
        Vector3 slotPosition = baseSlot.GetDisplayPosition();
        HoldBrainrot(baseSlot.RemoveBrainrot(), slotPosition);
    }

    private void CalculateStats() {
        characterStat.OnInit();

        foreach (UpgradeType upgradeType in Enum.GetValues(typeof(UpgradeType))) {
            UpgradeConfigSO upgradeConfigSO = listUpgradeConfigSO.GetConfigByType(upgradeType);
            if (upgradeConfigSO == null) continue;

            int level = PlayerProgress.Instance.GetLevel(upgradeType);
            characterStat.OnUpgraded(upgradeConfigSO.GetModifier(level), null);
        }

        SetupStats();
    }

    private void SetupStats() {
        moveSpeed = characterStat.GetMoveSpeed();
        climpMoveSpeed = moveSpeed * climpMoveSpeedMultiplier;
        fallMoveSpeed = moveSpeed * fallMoveSpeedMultiplier;
        initialJumpVelocity = Mathf.Sqrt(-2f * riseGravity * characterStat.GetJumpHeight());
        maxFallSpeed = Mathf.Max(baseMaxFallSpeed, initialJumpVelocity);
    }

    private void StartLadderJump() {
        ladderJumpDirection = GetLadderJumpDirection();
        ladderJumpTimer = 0f;
        isJumping = true;
        lastJumpPressedTime = -1f;
        verticalVelocity = ladderJumpUpVelocity;
        characterVisual.OnStartJump();
        ChangeState(PlayerState.LadderJump);
    }

    private Vector3 GetLadderJumpDirection() {
        Vector3 direction = ladderHit.normal;
        direction.y = 0f;
        if (direction.sqrMagnitude < 0.001f) {
            direction = -playerVisualContainer.forward;
        }

        return direction.normalized;
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
        }
    }

    private void HandleRun() {
        if (CanStartClimb()) {
            StartClimb();
            return;
        }

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

        if (moveDir.sqrMagnitude > 0.001f) {
            Quaternion targetRot = Quaternion.LookRotation(moveDir);
            playerVisualContainer.rotation = Quaternion.Slerp(playerVisualContainer.rotation, targetRot, rotateSpeed * Time.deltaTime);
        }
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
        float climbInput = GetClimbInput(); ;

        characterController.Move(Vector3.up * climbInput * climpMoveSpeed * Time.deltaTime);
        characterVisual.SetClimbSpeed(climbInput);

        Vector3 ladderDirection = -ladderHit.normal;
        ladderDirection.y = 0f;
        if (ladderDirection.sqrMagnitude > 0.001f) {
            Quaternion targetRot = Quaternion.LookRotation(ladderDirection);
            playerVisualContainer.rotation = Quaternion.Slerp(playerVisualContainer.rotation, targetRot, rotateSpeed * Time.deltaTime);
        }
    }

    private float GetClimbInput() {
        if (!IsMoving()) return 0f;

        Vector3 ladderDirection = -ladderHit.normal;
        ladderDirection.y = 0f;
        ladderDirection.Normalize();

        float input = Vector3.Dot(GetMoveDirection(), ladderDirection);
        if (Mathf.Abs(input) < climbInputDeadZone) return 0f;

        return Mathf.Sign(input);
    }

    private bool CanStartClimb() {
        if (!IsMoving() || !IsFacingLadder()) return false;

        return Vector3.Dot(GetMoveDirection(), -ladderHit.normal) >= ladderEnterDelta;
    }

    private void StartClimb() {
        isJumping = false;
        verticalVelocity = 0f;
        ChangeState(PlayerState.Climb);
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
        isJumping = true;
        lastJumpPressedTime = -1f;
        verticalVelocity = initialJumpVelocity;
        characterVisual.OnStartJump();
        ChangeState(PlayerState.Jump);
    }

    private void HandleJump() {
        if (CanStartClimb()) {
            StartClimb();
            return;
        }

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
        if (CanStartClimb()) {
            StartClimb();
            return;
        }

        HandleMovement();
        characterVisual.OnFalling();

        if (IsGrounded()) {
            if (Time.time - lastJumpPressedTime <= jumpBufferTime) {
                StartJump();
                return;
            }

            verticalVelocity = Constant.GROUNDED_GRAVITY;
            isJumping = false;

            if (IsMoving()) {
                ChangeState(PlayerState.Run);
            }
            else {
                ChangeState(PlayerState.Idle);
            }
        }
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
        if (!Physics.Raycast(startPosition, direction, out ladderHit, distance, ladderLayerMask, QueryTriggerInteraction.Collide)) return false;

        return true;
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
    }
}