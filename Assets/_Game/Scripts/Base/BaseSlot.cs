using BreakInfinity;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BaseSlot : MonoBehaviour {
    public event EventHandler OnTotalIncomeChanged;
    public event EventHandler OnBrainrotChanged;
    public static event Action<BaseSlot> OnAnyIncomeCollected;

    public bool IsUnlocked => isUnlocked;
    public bool IsActive => currentBrainrotSO != null;
    public Transform TF {
        get {
            if (tf == null) {
                tf = transform;
            }

            return tf;
        }
    }

    [SerializeField] private BaseSlotVisual baseSlotVisual;
    [SerializeField] private Transform equipButtonSpawnPoint;
    [SerializeField] private Transform baseSlotTrigger;

    private BaseHouse baseHouse;
    private bool isUnlocked = false;
    private Transform tf;
    private BrainrotSO currentBrainrotSO;
    private float incomeTimer = 0f;
    private BigDouble totalIncome = 0f;
    private BigDouble incomeAmount;

    private void Update() {
        HandleIncome();
    }

    private void OnTriggerEnter(Collider other) {
        if (!other.CompareTag(Constant.PLAYER_TAG)) return;
        if (!baseHouse.IsOwnedBy(Cache.GetPlayer(other))) return;

        CollectIncome();
    }

    public void OnInit() {
        incomeAmount = currentBrainrotSO.GetIncomeAmount();
    }

    public void PlaceBrainrot(BrainrotSO brainrotSO, Vector3 fromPosition) {
        if (brainrotSO == null) return;

        SetBrainrot(brainrotSO);
        baseSlotVisual.FlyBrainrotFrom(fromPosition);
    }

    public BrainrotSO SwapBrainrot(BrainrotSO newBrainrot, Vector3 fromPosition) {
        BrainrotSO oldBrainrotSO = currentBrainrotSO;
        CollectIncome();
        SetBrainrot(newBrainrot);
        baseSlotVisual.FlyBrainrotFrom(fromPosition);
        return oldBrainrotSO;
    }

    public BrainrotSO RemoveBrainrot() {
        BrainrotSO removedBrainrotSO = currentBrainrotSO;
        CollectIncome();
        SetBrainrot(null);
        return removedBrainrotSO;
    }

    public void UnLock() {
        isUnlocked = true;
    }

    public void Lock() {
        isUnlocked = false;
    }

    public BigDouble GetTotalIncome() {
        return totalIncome;
    }

    public Vector3 GetEquipButtonSpawnPoint() {
        return equipButtonSpawnPoint.position;
    }

    public void SetBaseHouse(BaseHouse baseHouse) {
        this.baseHouse = baseHouse;
    }

    public bool IsMatchBaseHouse(BaseHouse baseHouse) {
        return this.baseHouse != null && this.baseHouse == baseHouse;
    }

    public Vector3 GetDisplayPosition() {
        return baseSlotVisual.GetDisplayPosition();
    }

    public BigDouble AddAfkIncome(long afkSeconds) {
        if (!IsActive || afkSeconds <= 0) return 0;

        BigDouble afkIncome = (BigDouble)currentBrainrotSO.GetAfkIncomeAmount() * (afkSeconds / Constant.INCOME_INTERVAL) * baseHouse.GetMoneyMultiplier();
        AddMoney(afkIncome);
        return afkIncome;
    }

    public BrainrotSO GetCurrentBrainrot() {
        return currentBrainrotSO;
    }

    public void LoadBrainrot(BrainrotSO brainrotSO, BigDouble pendingIncome) {
        SetBrainrot(brainrotSO);
        AddMoney(pendingIncome);
    }

    public void ClearSlot() {
        totalIncome = 0f;
        SetBrainrot(null);
        OnTotalIncomeChanged?.Invoke(this, EventArgs.Empty);
    }

    public Vector3 GetBaseSlotTriggerPosition() {
        return baseSlotTrigger.position;
    }

    private void HandleIncome() {
        if (!IsActive) return;

        incomeTimer += Time.deltaTime;
        if (incomeTimer >= Constant.INCOME_INTERVAL) {
            AddMoney(incomeAmount * baseHouse.GetMoneyMultiplier());
            incomeTimer = 0f;
        }
    }

    private void AddMoney(BigDouble amount) {
        totalIncome += amount;
        OnTotalIncomeChanged?.Invoke(this, EventArgs.Empty);
    }

    private void SetBrainrot(BrainrotSO brainrotSO) {
        currentBrainrotSO = brainrotSO;
        incomeAmount = IsActive ? currentBrainrotSO.GetIncomeAmount() : 0f;
        incomeTimer = 0f;

        if (IsActive) {
            baseSlotVisual.OnInit(currentBrainrotSO);
        }
        else {
            baseSlotVisual.OnClear();
        }

        baseHouse?.MarkDirty();
        OnBrainrotChanged?.Invoke(this, EventArgs.Empty);
    }

    private void CollectIncome() {
        if (totalIncome <= 0f) return;

        PlayerProgress.Instance.AddMoney(totalIncome);
        SoundManager.Instance.PlayFx(FxID.MoneyChange);
        if (UIManager.Instance.IsUIOpen<CanvasGamePlay>()) {
            UIManager.Instance.GetUI<CanvasGamePlay>().PlayCollectMoneyVFX(totalIncome);
        }
        totalIncome = 0f;
        OnTotalIncomeChanged?.Invoke(this, EventArgs.Empty);
        OnAnyIncomeCollected?.Invoke(this);
    }
}