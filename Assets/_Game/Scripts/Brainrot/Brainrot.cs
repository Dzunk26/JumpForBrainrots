using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Brainrot : GameUnit {
    public bool IsAlive => isAlive;

    [SerializeField] private Collider pickUpTrigger;
    [SerializeField] private BrainrotVisual brainrotVisual;
    [SerializeField] private BrainrotInfor brainrotInfor;
    [SerializeField] private Transform capturedButtonUISpawnPoint;

    private BrainrotSpawner brainrotSpawner;
    private BrainrotSO brainrotSO;
    private float lifeTimerMax;
    private float lifeTimer = 0f;
    private bool isAlive = false;

    private void Update() {
        HandleLifeTime();
    }

    public void OnInit(BrainrotSO brainrotSO, BrainrotSpawner brainrotSpawner) {
        this.brainrotSO = brainrotSO;
        this.brainrotSpawner = brainrotSpawner;
        isAlive = true;
        lifeTimerMax = brainrotSO.GetAppearTimerMax();
        lifeTimer = 0f;
        brainrotVisual.OnInit(brainrotSO);
        brainrotInfor.OnInit(brainrotSO, brainrotVisual.GetInfoLocalPosition());
    }

    public void OnDespawn() {
        this.brainrotSO = null;
        isAlive = false;
        brainrotSpawner.Despawn(this);
    }

    public void OnCaptured() {
        OnDespawn();
    }

    public BrainrotSO GetBrainrotSO() {
        return brainrotSO;
    }

    public Vector3 GetCaptureButtonSpawnPoint() {
        return capturedButtonUISpawnPoint.position;
    }

    private void HandleLifeTime() {
        lifeTimer += Time.deltaTime;
        if (lifeTimer >= lifeTimerMax) {
            OnDespawn();
            return;
        }

        brainrotInfor.SetLifeTime(lifeTimerMax - lifeTimer);
    }
}