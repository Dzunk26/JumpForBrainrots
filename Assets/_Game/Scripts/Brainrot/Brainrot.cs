using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Brainrot : GameUnit {
    public bool IsAlive => isAlive;

    [SerializeField] private Collider pickUpTrigger;
    [SerializeField] private BrainrotVisual brainrotVisual;
    [SerializeField] private Transform capturedButtonUISpawnPoint;

    private BrainrotSO brainrotSO;
    private float lifeTimerMax;
    private float lifeTimer = 0f;
    private bool isAlive = false;

    private void Update() {
        HandleLifeTime();
    }

    public void OnInit(BrainrotSO brainrotSO) {
        this.brainrotSO = brainrotSO;
        isAlive = true;
        lifeTimerMax = brainrotSO.GetAppearTimerMax();
        lifeTimer = 0f;
        brainrotVisual.OnInit(brainrotSO);
    }

    public void OnDespawn() {
        this.brainrotSO = null;
        isAlive = false;
    }

    public void OnCaptured() {
        OnDespawn();
        SimplePool.ReturnToPool(this);
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
        }
    }
}