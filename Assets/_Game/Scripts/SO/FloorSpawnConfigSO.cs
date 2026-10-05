using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[CreateAssetMenu()]
public class FloorSpawnConfigSO : ScriptableObject {
    [SerializeField] private int floorID;

    [Header("Obstacle Config")]
    [SerializeField] private float spawnObstacleTimerMax;

    [Header("Brainrot Config")]
    [SerializeField] private int spawnCapMax = 8;
    [SerializeField] private float spawnBrainrotTimerMax = 3f;
    [SerializeField] private BrainrotRarity brainrotRarity;

    public bool MatchFloorID(int floorID) {
        return this.floorID == floorID;
    }

    public float GetObstacleSpawnTimerMax() {
        return this.spawnObstacleTimerMax;
    }

    public BrainrotRarity GetBrainrotRarity() {
        return brainrotRarity;
    }

    public int GetSpawnCapMax() {
        return spawnCapMax;
    }

    public float GetSpawnBrainrotTimerMax() {
        return spawnBrainrotTimerMax;
    }
}