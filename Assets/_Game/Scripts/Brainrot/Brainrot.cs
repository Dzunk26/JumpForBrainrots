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
    [SerializeField] private Vector2 lifeTimeRandomRange = new Vector2(0.75f, 1.25f);

    [Header("Despawn")]
    [SerializeField] private Transform visualTF;
    [SerializeField] private PoolType despawnVFXPoolType = PoolType.DespawnPoofVFX;
    [SerializeField] private float vanishTimerMax = 0.3f;
    [SerializeField] private AnimationCurve vanishScaleCurve = new AnimationCurve(new Keyframe(0f, 1f), new Keyframe(0.3f, 1.15f), new Keyframe(1f, 0f));

    private BrainrotSpawner brainrotSpawner;
    private BrainrotSO brainrotSO;
    private float lifeTimerMax;
    private float lifeTimer = 0f;
    private bool isAlive = false;
    private Vector3 baseVisualScale;
    private float vanishTimer;
    private bool isVanishing;

    private void Awake() {
        baseVisualScale = visualTF.localScale;
    }

    private void Update() {
        if (isVanishing) {
            HandleVanish();
            return;
        }

        HandleLifeTime();
    }

    public void OnInit(BrainrotSO brainrotSO, BrainrotSpawner brainrotSpawner, float startLifeRatio = 0f) {
        this.brainrotSO = brainrotSO;
        this.brainrotSpawner = brainrotSpawner;
        isAlive = true;
        isVanishing = false;
        vanishTimer = 0f;
        pickUpTrigger.enabled = true;
        visualTF.localScale = baseVisualScale;
        lifeTimerMax = brainrotSO.GetAppearTimerMax() * Random.Range(lifeTimeRandomRange.x, lifeTimeRandomRange.y);
        lifeTimer = lifeTimerMax * startLifeRatio;
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
            StartVanish();
            return;
        }

        brainrotInfor.SetLifeTime(lifeTimerMax - lifeTimer);
    }

    private void StartVanish() {
        isAlive = false;
        isVanishing = true;
        vanishTimer = 0f;
        pickUpTrigger.enabled = false;
        SimplePool.GetFromPool<DespawnVFX>(despawnVFXPoolType, TF.position, Quaternion.identity).OnInit();
    }

    private void HandleVanish() {
        vanishTimer += Time.deltaTime;
        visualTF.localScale = baseVisualScale * vanishScaleCurve.Evaluate(vanishTimer / vanishTimerMax);
        if (vanishTimer < vanishTimerMax) return;

        OnDespawn();
    }
}