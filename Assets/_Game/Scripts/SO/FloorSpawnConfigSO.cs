using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[CreateAssetMenu()]
public class FloorSpawnConfigSO : ScriptableObject {
    [SerializeField] private int floorID;
    [SerializeField] private int spawnCapMax = 8;
    [SerializeField] private float spawnTimerMax = 3f;
    [SerializeField] private BrainrotRarity brainrotRarity;

    public bool MatchFloorID(int floorID) {
        return this.floorID == floorID;
    }

    public BrainrotRarity GetBrainrotRarity() {
        return brainrotRarity;
    }

    public int GetSpawnCapMax() {
        return spawnCapMax;
    }

    public float GetSpawnTimerMax() {
        return spawnTimerMax;
    }
}