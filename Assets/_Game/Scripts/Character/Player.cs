using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Playables;

public enum PlayerState {
    Idle,
    Run,
    Jump,
    Fall,
    Dead,
}

public class Player : Character {
    [SerializeField] private Camera playerCamera;
    [SerializeField] private Transform playerVisualContainer;
    [SerializeField] private CharacterController characterController;
    [SerializeField] private CharacterVisual characterVisual;

    [SerializeField] private float moveSpeed = 10f;
    [SerializeField] private float rotateSpeed = 15f;
    [SerializeField] private float maxJumpHeight = 2.5f;
    [SerializeField] private float maxJumpTime = 1f;

    [SerializeField] private float fallMultiplier = 2f;
    [SerializeField] private LayerMask groundLayerMask;
    [SerializeField] private float raycastDistance = 0.2f;

    private Vector2 inputVector;
    private bool isJumping;
    private float verticalVelocity;
    private float gravity;
    private float initialJumpVelocity;
    private PlayerState currentState;

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
        SetupJumpVariables();
    }

    protected override void OnDespawn() {

    }

    public void Jump() {
        if ((currentState == PlayerState.Idle || currentState == PlayerState.Run) && IsGrounded() && !isJumping) {
            StartJump();
        }
    }

    private void SetupJumpVariables() {
        float timeToApex = maxJumpTime / 2;
        gravity = (-2 * maxJumpHeight) / (timeToApex * timeToApex);
        initialJumpVelocity = (2 * maxJumpHeight) / timeToApex;
    }

    private void HandlePlayerState() {
        switch (currentState) {
            case PlayerState.Idle:
                HandleIdle();
                break;
            case PlayerState.Run:
                HandleRun();
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
        Vector3 camForward = playerCamera.transform.forward;
        Vector3 camRight = playerCamera.transform.right;

        camForward.y = 0f;
        camRight.y = 0f;
        camForward.Normalize();
        camRight.Normalize();

        Vector3 moveDir = (camForward * inputVector.y + camRight * inputVector.x).normalized;
        Vector3 nextPosition = moveDir * moveSpeed * Time.deltaTime;

        characterController.Move(nextPosition);

        if (inputVector.sqrMagnitude > 0.001f) {
            Quaternion targetRot = Quaternion.LookRotation(moveDir);
            playerVisualContainer.rotation = Quaternion.Slerp(playerVisualContainer.rotation, targetRot, rotateSpeed * Time.deltaTime);
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
    }

    private void StartJump() {
        isJumping = true;
        verticalVelocity = initialJumpVelocity;
        characterVisual.OnStartJump();
        ChangeState(PlayerState.Jump);
    }

    private void HandleJump() {
        HandleMovement();

        if (verticalVelocity <= 0f) {
            ChangeState(PlayerState.Fall);
        }
    }

    private void HandleGravity() {
        float currentGravity = gravity;

        if (verticalVelocity < 0f) {
            currentGravity *= fallMultiplier;
        }

        verticalVelocity += currentGravity * Time.deltaTime;

        if (verticalVelocity < -20f) {
            verticalVelocity = -20f;
        }

        characterController.Move(Vector3.up * verticalVelocity * Time.deltaTime);
    }

    private void HandleFall() {
        HandleMovement();
        characterVisual.OnFalling();

        if (IsGrounded()) {
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

        return Physics.CapsuleCast(point1, point2, radius, Vector3.down, raycastDistance, groundLayerMask);
    }

    private bool IsMoving() {
        return inputVector.sqrMagnitude > 0.01f;
    }

    private void ChangeState(PlayerState state) {
        if (currentState == state) return;

        currentState = state;
    }
}