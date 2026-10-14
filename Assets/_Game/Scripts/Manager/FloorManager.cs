using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FloorManager : Singleton<FloorManager> {
    [SerializeField] private List<BrainrotSpawner> listBrainrotSpawner;

    public BrainrotSpawner GetRandomBrainrotSpawner() {
        return listBrainrotSpawner[Random.Range(0, listBrainrotSpawner.Count)];
    }
}