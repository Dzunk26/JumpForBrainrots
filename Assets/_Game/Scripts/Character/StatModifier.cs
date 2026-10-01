using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class StatModifier {
    [SerializeField] private float jumpHeight;
    [SerializeField] private float moveSpeed;

    public StatModifier(float jumpHeight, float moveSpeed) {
        this.jumpHeight = jumpHeight;
        this.moveSpeed = moveSpeed;
    }

    public float GetJumpHeightModifier() {
        return jumpHeight;
    }

    public float GetMoveSpeedModifier() {
        return moveSpeed;
    }

    public StatModifier Multiply(int level) {
        return new StatModifier(jumpHeight * level, moveSpeed * level);
    }
}