using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum PoolType {
    Brick,
    CharacterBrick,
    Bot
}

[System.Serializable]
public class PoolConfig {
    public GameUnit prefab;
    public Transform parent;
    public int amount;
}

public class PoolControl : MonoBehaviour {
    [SerializeField] private PoolConfig[] poolConfigs;

    private void Awake() {
        for (int i = 0; i < poolConfigs.Length; i++) {
            SimplePool.PreLoad(poolConfigs[i].prefab, poolConfigs[i].amount, poolConfigs[i].parent);
        }
    }
}