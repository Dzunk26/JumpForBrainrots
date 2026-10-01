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
    Dead,
}

public class Player : Character {
    [SerializeField] private ListUpgradeConfigSO listUpgradeConfigSO;

    [SerializeField] private Camera playerCamera;
    [SerializeField] private Transform playerVisualContainer;
    [SerializeField] private CharacterController characterController;
    [SerializeField] private CharacterVisual characterVisual;

    [SerializeField] private float rotateSpeed = 15f;
    [SerializeField] private float gravity = -30f;
    [SerializeField] private float minGravity = -30;

    [SerializeField] private float ladderEnterThreshold = 0.5f;

    [SerializeField] private float fallMultiplier = 2f;
    [SerializeField] private LayerMask groundLayerMask;
    [SerializeField] private float groundRaycastDistance = 0.2f;
    [SerializeField] private LayerMask ladderLayerMask;
    [SerializeField] private float ladderRaycastDistance = 0.4f;
    [SerializeField] private float nearGroundDistance = 1f;
    [SerializeField] private float bodyRatio = 0.6f;
    [SerializeField] private float jumpBufferTime = 0.25f;

    private float lastJumpPressedTime = -1f;
    private float moveSpeed;
    private Vector2 inputVector;
    private bool isJumping;
    private float verticalVelocity;
    private float initialJumpVelocity;
    private PlayerState currentState;
    private RaycastHit ladderHit;


    private void OnEnable() {
        OnInit();
    }

    private void Update() {
        ListenInput();

        HandleGravity();

        HandlePlayerState();
    }

    protected override void OnInit() {
        characterVisual.OnInit();
        isJumping = false;
        ChangeState(PlayerState.Idle);
        SetupStats();
        CalculateStats();
    }

    protected override void OnDespawn() {

    }

    public void Jump() {
        lastJumpPressedTime = Time.time;

        if ((currentState == PlayerState.Idle || currentState == PlayerState.Run) && IsGrounded() && !isJumping) {
            StartJump();
        }
    }

    public void TestOnUpgrade() {
        UpgradeType upgradeType = UpgradeType.JumpPower;
        PlayerProgress.Instance.IncreaseLevel(upgradeType);
        OnUpgrade(upgradeType);
        Debug.Log(PlayerProgress.Instance.GetLevel(upgradeType) + " " + characterStat.GetJumpHeight());
    }

    public void OnUpgrade(UpgradeType upgradeType) {
        CalculateStats();
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
        initialJumpVelocity = Mathf.Sqrt(-2f * gravity * characterStat.GetJumpHeight());
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

        if (!IsGrounded()) {
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
        Vector3 nextPosition = moveDir * moveSpeed * Time.deltaTime;

        characterController.Move(nextPosition);

        if (inputVector.sqrMagnitude > 0.001f) {
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
        float climbInput = Mathf.Abs(inputVector.y) >= Mathf.Abs(inputVector.x) ? inputVector.y : inputVector.x;

        characterController.Move(Vector3.up * climbInput * moveSpeed * Time.deltaTime);
        characterVisual.SetClimbSpeed(climbInput);

        Vector3 ladderDirection = -ladderHit.normal;
        ladderDirection.y = 0f;
        if (ladderDirection.sqrMagnitude > 0.001f) {
            Quaternion targetRot = Quaternion.LookRotation(ladderDirection);
            playerVisualContainer.rotation = Quaternion.Slerp(playerVisualContainer.rotation, targetRot, rotateSpeed * Time.deltaTime);
        }
    }

    private bool CanStartClimb() {
        if (!IsMoving() || !IsFacingLadder()) return false;

        return Vector3.Dot(GetMoveDirection(), -ladderHit.normal) >= ladderEnterThreshold;
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

        if (inputVector.x + inputVector.y < 0f && IsGrounded()) {
            if (IsMoving()) {
                ChangeState(PlayerState.Run);
            }
            else {
                ChangeState(PlayerState.Idle);
            }
        }
    }

    private void HandleIdle() {
        characterVisual.OnIdle();

        if (!IsGrounded()) {
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

    private void HandleGravity() {
        if (currentState == PlayerState.Climb) return;

        float currentGravity = gravity;

        if (verticalVelocity < 0f) {
            currentGravity *= fallMultiplier;
        }

        verticalVelocity += currentGravity * Time.deltaTime;

        if (verticalVelocity < minGravity) {
            verticalVelocity = minGravity;
        }

        characterController.Move(Vector3.up * verticalVelocity * Time.deltaTime);
    }

    private void HandleFall() {
        if (CanStartClimb()) {
            StartClimb();
            return;
        }

        HandleMovement();
        characterVisual.OnFalling();

        if (IsNearGround()) {
            if (!IsMoving()) {
                characterVisual.OnIdle();
            }
        }

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
        characterVisual.OnDead();
    }

    private bool IsGrounded() {
        float radius = characterController.radius * 0.95f;
        Vector3 center = TF.position + characterController.center;
        float halfHeight = characterController.height * 0.5f - radius;
        float offset = 0.1f;

        Vector3 point1 = center + Vector3.up * (halfHeight + offset);        
        Vector3 point2 = center + Vector3.down * halfHeight + (Vector3.up * offset);

        return Physics.CapsuleCast(point1, point2, radius, Vector3.down, groundRaycastDistance, groundLayerMask);
    }

    private bool IsFacingLadder() {
        Vector3 startPosition = TF.position + Vector3.up * characterController.height * bodyRatio;
        Vector3 direction = playerVisualContainer.forward;
        float distance = characterController.radius + ladderRaycastDistance;
        if (!Physics.Raycast(startPosition, direction, out ladderHit, distance, ladderLayerMask, QueryTriggerInteraction.Collide)) return false;

        return true;
    }

    private bool IsNearGround() {
        float radius = characterController.radius * 0.95f;
        Vector3 center = TF.position + characterController.center;
        float halfHeight = characterController.height * 0.5f - radius;
        float offset = 0.1f;

        Vector3 point1 = center + Vector3.up * (halfHeight + offset);
        Vector3 point2 = center + Vector3.down * halfHeight + (Vector3.up * offset);

        return Physics.CapsuleCast(point1, point2, radius, Vector3.down, nearGroundDistance, groundLayerMask);
    }


    private bool IsMoving() {
        return inputVector.sqrMagnitude > 0.01f;
    }

    private void ChangeState(PlayerState state) {
        if (currentState == state) return;

        currentState = state;
    }
}