using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Playables;

public class Player : Character {
    [SerializeField] private CharacterController characterController;
    [SerializeField] private CharacterVisual characterVisual;

    [SerializeField] private float moveSpeed;
    [SerializeField] private float rotateSpeed;

    [SerializeField] private float fallMultiplier = 2f;
    [SerializeField] private LayerMask groundLayerMask;
    [SerializeField] private float raycastDistance = 0.2f;

    private Vector2 inputVector;
    private bool isJumping;
    private float verticalVelocity;
    private float gravity;
    private float initialJumpVelocity;

    private void HandleMovement() {
        ListenInput();


    }

    private void ListenInput() {
        inputVector = GameInput.Instance.GetMovementVectorNormalized();
    }

    private void StartJump() {
        isJumping = true;
        verticalVelocity = initialJumpVelocity;
        characterVisual.OnStartJump();
        //ChangeState(PlayerState.Jump);
    }

    private void HandleJump() {
        HandleMovement();

        if (verticalVelocity <= 0f) {
            //ChangeState(PlayerState.Fall);
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
}