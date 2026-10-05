using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BrainrotSpawner : MonoBehaviour {
    [SerializeField] private GameObject brainrotPrefab;

    [SerializeField] private FloorSpawnConfigSO floorSpawnConfigSO;
    [SerializeField] private ListBrainrotSO listBrainrotSO;
    [SerializeField] private BoxCollider spawnArea;

    private List<Brainrot> spawnedBrainrots = new List<Brainrot>();
    private List<BrainrotSO> spawnableBrainrotSOs;
    private float spawnTimer;
    private float spawnTimerMax;
    private int spawnCapMax;
    private BrainrotRarity spawnBrainrotRarity;

    private void Awake() {
        OnInit();
    }

    private void Update() {
        RemoveInvalidBrainrots();

        HandleSpawn();
    }

    public void OnInit() {
        spawnTimerMax = floorSpawnConfigSO.GetSpawnTimerMax();
        spawnCapMax = floorSpawnConfigSO.GetSpawnCapMax();
        spawnBrainrotRarity = floorSpawnConfigSO.GetBrainrotRarity();
        spawnableBrainrotSOs = listBrainrotSO.GetBrainrotSOsByRarity(spawnBrainrotRarity);

        InitialSpawn();
    }

    public void OnBrainrotDespawn(Brainrot brainrot) {
        spawnedBrainrots.Remove(brainrot);
    }

    private void InitialSpawn() {
        while (spawnedBrainrots.Count < spawnCapMax) {
            Spawn(GetRandomSpawnPoint());
        }

        spawnTimer = 0;
    }

    private void HandleSpawn() {
        if (spawnedBrainrots.Count >= spawnCapMax) return;

        spawnTimer += Time.deltaTime;
        if (spawnTimer < spawnTimerMax) return;

        spawnTimer = 0f;
        Spawn(GetRandomSpawnPoint());
    }

    private void Spawn(Vector3 spawnPoint) {
        Brainrot brainrot = SimplePool.GetFromPool<Brainrot>(PoolType.Brainrot, spawnPoint, Quaternion.Euler(0f, Random.Range(0f, 360f), 0f));
        BrainrotSO brainrotSO = GetRandomBrainrotSO();
        brainrot.OnInit(brainrotSO);
        spawnedBrainrots.Add(brainrot);
    }

    private void Despawn(Brainrot brainrot) {
        SimplePool.ReturnToPool(brainrot);
        spawnedBrainrots.Remove(brainrot);
    }

    private Vector3 GetRandomSpawnPoint() {
        Bounds bounds = spawnArea.bounds;
        return new Vector3(Random.Range(bounds.min.x, bounds.max.x), bounds.min.y, Random.Range(bounds.min.z, bounds.max.z));
    }

    private void RemoveInvalidBrainrots() {
        for (int i = spawnedBrainrots.Count - 1; i >=0; i--) {
            if (!spawnedBrainrots[i].IsAlive()) {
                Despawn(spawnedBrainrots[i]);
            }
        }
    }

    private BrainrotSO GetRandomBrainrotSO() {
        int randomIndex = Random.Range(0, spawnableBrainrotSOs.Count);

        return spawnableBrainrotSOs[randomIndex];
    }
}