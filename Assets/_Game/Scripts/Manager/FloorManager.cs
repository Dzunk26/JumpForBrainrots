using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FloorManager : Singleton<FloorManager> {
    [SerializeField] private List<BrainrotSpawner> listBrainrotSpawner;

    public BrainrotSpawner GetRandomBrainrotSpawner() {
        return listBrainrotSpawner[Random.Range(0, listBrainrotSpawner.Count)];
    }

    public BrainrotSpawner GetClosestBrainrotSpawner(Vector3 position) {
        BrainrotSpawner closestSpawner = null;
        float closestSqrDistance = float.MaxValue;

        for (int i = 0; i < listBrainrotSpawner.Count; i++) {
            float sqrDistance = listBrainrotSpawner[i].GetSqrDistance(position);
            if (sqrDistance < closestSqrDistance) {
                closestSqrDistance = sqrDistance;
                closestSpawner = listBrainrotSpawner[i];
            }
        }

        return closestSpawner;
    }
}