using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class Stat {
    [SerializeField] private float baseValue;
    private List<float> modifiers = new List<float>();

    public float GetBaseValue() {
        return baseValue;
    }

    public float GetValue() {
        float finalValue = baseValue;
        modifiers.ForEach(x => finalValue += x);

        return finalValue;
    }

    public void AddModifier(float modifier) {
        modifiers.Add(modifier);
    }

    public void RemoveModifier(float modifier) {
        modifiers.Remove(modifier);
    }
    
    public void ReMoveAllModifiers() {
        modifiers.Clear();
    }
}