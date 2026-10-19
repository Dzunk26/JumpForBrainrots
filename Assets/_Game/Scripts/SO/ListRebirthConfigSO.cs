using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class RebirthConfig {
    public int requiredJumpLevel;
    public float moneyMultiplier;
    public float moveSpeedBonus;
}

[CreateAssetMenu()]
public class ListRebirthConfigSO : ScriptableObject {
    [SerializeField] private List<RebirthConfig> rebirthConfigs;

    // rebirthLevel = so lan da rebirth, rebirthConfigs[rebirthLevel] la moc rebirth tiep theo
    public bool IsMaxRebirth(int rebirthLevel) {
        return rebirthLevel >= rebirthConfigs.Count;
    }

    public int GetRequiredJumpLevel(int rebirthLevel) {
        return rebirthConfigs[rebirthLevel].requiredJumpLevel;
    }

    public float GetMoneyMultiplier(int rebirthLevel) {
        if (rebirthLevel <= 0) return 1f;

        return GetReachedConfig(rebirthLevel).moneyMultiplier;
    }

    public float GetMoveSpeedBonus(int rebirthLevel) {
        if (rebirthLevel <= 0) return 0f;

        return GetReachedConfig(rebirthLevel).moveSpeedBonus;
    }

    private RebirthConfig GetReachedConfig(int rebirthLevel) {
        return rebirthConfigs[Mathf.Min(rebirthLevel, rebirthConfigs.Count) - 1];
    }
}
