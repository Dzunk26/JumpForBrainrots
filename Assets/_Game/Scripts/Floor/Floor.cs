using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Floor : MonoBehaviour {
    [SerializeField] private FloorSpawnConfigSO floorSpawnConfigSO;

    public FloorSpawnConfigSO GetFloorSpawnConfigSO() {
        return floorSpawnConfigSO;
    }
}