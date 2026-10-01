using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public enum UpgradeType {
    JumpPower,
    MoveSpeed
}

[CreateAssetMenu()]
public class UpgradeConfigSO : ScriptableObject {
    [SerializeField] private UpgradeType upgradeType;
    [SerializeField] private int maxLevel = 500;

    [SerializeField] private StatModifier stepModifier;

    [SerializeField] private double baseCost = 10;
    [SerializeField] private float costGrowth = 1.15f; // cost = baseCost * costGrowth ^ level

    public bool MatchUpgradeType(UpgradeType upgradeType) {
        return this.upgradeType == upgradeType;
    }

    public UpgradeType GetUpgradeType() {
        return this.upgradeType;
    }

    public bool IsMaxLevel(int level) {
        return level >= maxLevel;
    }

    public StatModifier GetModifier(int level) {
        return stepModifier.Multiply(Mathf.Clamp(level, 0, maxLevel));
    }

    public double GetCost(int level) {
        return Math.Floor(baseCost * Math.Pow(costGrowth, level));
    }
}