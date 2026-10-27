using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Brainrot : GameUnit {
    public bool IsAlive => isAlive;
    public bool IsDropping => isDropping;

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

    [Header("Drop")]
    [SerializeField] private float dropDuration = 0.55f; // bay vong cung tu tay xuong dat
    [SerializeField] private float dropArcHeight = 3f;
    [SerializeField] private float dropSpinSpeed = 540f; // xoay quanh truc dung luc bay (do / giay)
    [SerializeField] private float bounceDuration = 0.25f; // cham dat nay len nhe 1 lan
    [SerializeField] private float bounceHeight = 0.6f;
    [SerializeField] private float landSquash = 0.25f; // cham dat bep xuong roi phong lai

    private BrainrotSpawner brainrotSpawner;
    private BrainrotSO brainrotSO;
    private float lifeTimerMax;
    private float lifeTimer = 0f;
    private bool isAlive = false;
    private Vector3 baseVisualScale;
    private float vanishTimer;
    private bool isVanishing;
    private bool isDropping;
    private float dropTimer;
    private Vector3 dropFromPosition;
    private Vector3 dropToPosition;

    private void Awake() {
        baseVisualScale = visualTF.localScale;
    }

    private void Update() {
        if (isVanishing) {
            HandleVanish();
            return;
        }

        if (isDropping) {
            HandleDrop();
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
        isDropping = false;
        pickUpTrigger.enabled = true;
        visualTF.localScale = baseVisualScale;
        visualTF.localRotation = Quaternion.identity;
        lifeTimerMax = brainrotSO.GetAppearTimerMax() * Random.Range(lifeTimeRandomRange.x, lifeTimeRandomRange.y);
        lifeTimer = lifeTimerMax * startLifeRatio;
        brainrotVisual.OnInit(brainrotSO);
        brainrotInfor.OnInit(brainrotSO, brainrotVisual.GetInfoLocalPosition());
    }

    // roi ra tu tay nguoi bi danh / chet: bay vong cung, lon 1 vong, cham dat nay nhe roi moi nhat duoc
    public void OnDrop(BrainrotSO brainrotSO, BrainrotSpawner brainrotSpawner, Vector3 fromPosition, Vector3 toPosition) {
        OnInit(brainrotSO, brainrotSpawner);
        isAlive = false;
        pickUpTrigger.enabled = false;
        isDropping = true;
        dropTimer = 0f;
        dropFromPosition = fromPosition;
        dropToPosition = toPosition;
        TF.position = fromPosition;
    }

    public void OnDespawn() {
        this.brainrotSO = null;
        isAlive = false;
        brainrotSpawner.Despawn(this);
    }

    public void OnCaptured() {
        OnDespawn();
    }

    // dang roi thi tra ve cho se cham dat, de bot chay toi dung cho
    public Vector3 GetRestPosition() {
        return isDropping ? dropToPosition : TF.position;
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

    private void HandleDrop() {
        dropTimer += Time.deltaTime;
        TF.rotation = Quaternion.AngleAxis(dropSpinSpeed * Time.deltaTime, Vector3.up) * TF.rotation;

        if (dropTimer < dropDuration) {
            float t = dropTimer / dropDuration;
            TF.position = Vector3.Lerp(dropFromPosition, dropToPosition, t) + Vector3.up * (dropArcHeight * 4f * t * (1f - t));
            visualTF.localRotation = Quaternion.AngleAxis(360f * t, Vector3.right);
            return;
        }

        float bounceT = Mathf.Clamp01((dropTimer - dropDuration) / bounceDuration);
        TF.position = dropToPosition + Vector3.up * (bounceHeight * 4f * bounceT * (1f - bounceT));
        visualTF.localRotation = Quaternion.identity;
        float squash = landSquash * (1f - bounceT);
        visualTF.localScale = Vector3.Scale(baseVisualScale, new Vector3(1f + squash, 1f - squash, 1f + squash));
        if (bounceT < 1f) return;

        isDropping = false;
        isAlive = true;
        pickUpTrigger.enabled = true;
        visualTF.localScale = baseVisualScale;
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