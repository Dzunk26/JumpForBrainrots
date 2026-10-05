using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObstaleSpawner : MonoBehaviour {
    [SerializeField] private Transform startPoint;
    [SerializeField] private Transform endPoint;

    [SerializeField] private Floor floor;

    [SerializeField] private ListObstacleSO listObstacleSO;

    private List<Obstacle> spawnedObstacles = new List<Obstacle>();
    private FloorSpawnConfigSO floorSpawnConfigSO;
    private float spawnTimerMax;
    private float spawnTimer = 0f;

    private void Start() {
        OnInit();
    }

    private void Update() {
        RemoveInvalidObstacles();

        HandleSpawn();
    }

    public void OnInit() {
        floorSpawnConfigSO = floor.GetFloorSpawnConfigSO();
        spawnTimerMax = floorSpawnConfigSO.GetObstacleSpawnTimerMax();
        spawnTimer = Random.Range(0, spawnTimerMax);
    }

    private void HandleSpawn() {
        spawnTimer += Time.deltaTime;
        if (spawnTimer < spawnTimerMax) return;

        Spawn();
        spawnTimer = 0f;
    }

    private void Spawn() {
        Obstacle obstacle = SimplePool.GetFromPool<Obstacle>(PoolType.Obstacle, startPoint.position, Quaternion.identity);
        ObstacleSO obstacleSO = listObstacleSO.GetRandomObstacleSO();
        obstacle.OnInit(obstacleSO, endPoint.position);
        spawnedObstacles.Add(obstacle);
    }

    private void Despawn(Obstacle obstacle) {
        SimplePool.ReturnToPool(obstacle);
        spawnedObstacles.Remove(obstacle);
    }

    private void RemoveInvalidObstacles() {
        for (int i = spawnedObstacles.Count - 1; i >= 0; i--) {
            if (!spawnedObstacles[i].IsAlive) {
                Despawn(spawnedObstacles[i]);
            }
        }
    }

}