using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterStat : MonoBehaviour {
    [SerializeField] private Stat jumpHeight;
    [SerializeField] public Stat moveSpeed;

    public void OnInit() {
        jumpHeight.RemoveAllModifiers();
        moveSpeed.RemoveAllModifiers();
    }

    public void OnUpgraded(StatModifier newModifier, StatModifier oldModifier) {
        if (newModifier != null) {
            AddModifiers(newModifier);
        }

        if (oldModifier != null) {
            RemoveModifiers(oldModifier);
        }
    }

    public float GetJumpHeight() {
        return jumpHeight.GetValue();
    }

    public float GetMoveSpeed() {
        return moveSpeed.GetValue();
    }

    private void AddModifiers(StatModifier modifier) {
        jumpHeight.AddModifier(modifier.GetJumpHeightModifier());
        moveSpeed.AddModifier(modifier.GetMoveSpeedModifier());
    }

    private void RemoveModifiers(StatModifier modifier) {
        jumpHeight.RemoveModifier(modifier.GetJumpHeightModifier());
        moveSpeed.RemoveModifier(modifier.GetMoveSpeedModifier());
    }
}