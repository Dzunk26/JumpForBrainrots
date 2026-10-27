using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BrainrotSpawner : MonoBehaviour {
    [SerializeField] private Floor floor;
    [SerializeField] private ListBrainrotSO listBrainrotSO;
    [SerializeField] private BoxCollider spawnArea;
    [SerializeField] private float initialMaxLifeRatio = 0.75f;

    [Header("Spawn Config")]
    [SerializeField] private float minDisBetweenSpawnPoints = 10f;
    [SerializeField] private int spawnRetryLimit = 30;

    private FloorSpawnConfigSO floorSpawnConfigSO;
    private List<Brainrot> spawnedBrainrots = new List<Brainrot>();
    private List<BrainrotSO> spawnableBrainrotSOs;
    private float spawnTimer;
    private float spawnTimerMax;
    private int spawnCapMax;
    private BrainrotRarity spawnBrainrotRarity;

    private void Start() {
        OnInit();
    }

    private void Update() {
        HandleSpawn();
    }

    public void OnInit() {
        floorSpawnConfigSO = floor.GetFloorSpawnConfigSO();
        spawnTimerMax = floorSpawnConfigSO.GetSpawnBrainrotTimerMax();
        spawnCapMax = floorSpawnConfigSO.GetSpawnCapMax();
        spawnBrainrotRarity = floorSpawnConfigSO.GetBrainrotRarity();
        spawnableBrainrotSOs = listBrainrotSO.GetBrainrotSOsByRarity(spawnBrainrotRarity);

        InitialSpawn();
    }

    public void Despawn(Brainrot brainrot) {
        spawnedBrainrots.Remove(brainrot);
        SimplePool.ReturnToPool(brainrot);
    }

    // brainrot roi ra tu nguoi bi danh / chet, thuoc spawner gan nhat de bot tim duoc va het gio thi tu bien mat
    public Brainrot SpawnDroppedBrainrot(BrainrotSO brainrotSO, Vector3 fromPosition, Vector3 toPosition) {
        Brainrot brainrot = SimplePool.GetFromPool<Brainrot>(PoolType.Brainrot, fromPosition, Quaternion.Euler(0f, Random.Range(0f, 360f), 0f));
        brainrot.OnDrop(brainrotSO, this, fromPosition, toPosition);
        spawnedBrainrots.Add(brainrot);
        return brainrot;
    }

    public float GetSqrDistance(Vector3 position) {
        return spawnArea.bounds.SqrDistance(position);
    }

    public Brainrot GetRandomBrainrot() {
        if (spawnedBrainrots.Count == 0) return null;

        return spawnedBrainrots[Random.Range(0, spawnedBrainrots.Count)];
    }

    public Brainrot GetClosestBrainrot(Vector3 position) {
        Brainrot closestBrainrot = null;
        float closestSqrDistance = float.MaxValue;

        for (int i = 0; i < spawnedBrainrots.Count; i++) {
            if (!spawnedBrainrots[i].IsAlive) continue;

            float sqrDistance = (spawnedBrainrots[i].TF.position - position).sqrMagnitude;
            if (sqrDistance < closestSqrDistance) {
                closestSqrDistance = sqrDistance;
                closestBrainrot = spawnedBrainrots[i];
            }
        }

        return closestBrainrot;
    }

    private void InitialSpawn() {
        while (spawnedBrainrots.Count < spawnCapMax) {
            Spawn(GetRandomSpawnPoint(), Random.Range(0f, initialMaxLifeRatio));
        }

        spawnTimer = 0;
    }

    private void HandleSpawn() {
        if (spawnedBrainrots.Count >= spawnCapMax) return;

        spawnTimer += Time.deltaTime;
        if (spawnTimer < spawnTimerMax) return;

        spawnTimer = 0f;
        Spawn(GetRandomSpawnPoint(), 0f);
    }

    private void Spawn(Vector3 spawnPoint, float startLifeRatio) {
        Brainrot brainrot = SimplePool.GetFromPool<Brainrot>(PoolType.Brainrot, spawnPoint, Quaternion.Euler(0f, Random.Range(0f, 360f), 0f));
        BrainrotSO brainrotSO = GetRandomBrainrotSO();
        brainrot.OnInit(brainrotSO, this, startLifeRatio);
        spawnedBrainrots.Add(brainrot);
    }

    private Vector3 GetRandomSpawnPoint() {
        Bounds bounds = spawnArea.bounds;
        Vector3 bestPoint = Vector3.zero;
        float bestSqrDistance = -1f;

        for (int i = 0; i < spawnRetryLimit; i++) {
            Vector3 point = new Vector3(Random.Range(bounds.min.x, bounds.max.x), bounds.min.y, Random.Range(bounds.min.z, bounds.max.z));
            float sqrDistance = GetClosestSqrDistance(point);

            if (sqrDistance >= minDisBetweenSpawnPoints * minDisBetweenSpawnPoints) return point;

            if (sqrDistance > bestSqrDistance) {
                bestSqrDistance = sqrDistance;
                bestPoint = point;
            }
        }

        return bestPoint;
    }

    private float GetClosestSqrDistance(Vector3 point) {
        float closestSqrDistance = float.MaxValue;

        for (int i = 0; i < spawnedBrainrots.Count; i++) {
            Vector3 offset = spawnedBrainrots[i].TF.position - point;
            offset.y = 0f;

            if (offset.sqrMagnitude < closestSqrDistance) {
                closestSqrDistance = offset.sqrMagnitude;
            }
        }

        return closestSqrDistance;
    }

    private BrainrotSO GetRandomBrainrotSO() {
        int randomIndex = Random.Range(0, spawnableBrainrotSOs.Count);

        return spawnableBrainrotSOs[randomIndex];
    }

}