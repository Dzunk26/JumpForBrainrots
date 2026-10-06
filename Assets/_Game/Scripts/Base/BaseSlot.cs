using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BaseSlot : MonoBehaviour {
    public event EventHandler OnTotalIncomeChanged;
    public event EventHandler OnBrainrotChanged;

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

    private bool isUnlocked = false;
    private Transform tf;
    private BrainrotSO currentBrainrotSO;
    private float incomeTimer = 0f;
    private float totalIncome = 0f;
    private float incomeAmount;

    private void Awake() {
        UnLock();
    }

    private void Update() {
        HandleIncome();
    }

    private void OnTriggerEnter(Collider other) {
        if (other.CompareTag(Constant.PLAYER_TAG)) {
            CollectIncome();
        }
    }

    public void OnInit() {
        incomeAmount = currentBrainrotSO.GetIncomeAmount();
    }

    public void PlaceBrainrot(BrainrotSO brainrotSO) {
        if (brainrotSO == null) return;

        SetBrainrot(brainrotSO);
    }

    public BrainrotSO SwapBrainrot(BrainrotSO newBrainrot) {
        BrainrotSO oldBrainrotSO = currentBrainrotSO;
        CollectIncome();
        SetBrainrot(newBrainrot);
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

    public float GetTotalIncome() {
        return totalIncome;
    }

    public Vector3 GetEquipButtonSpawnPoint() {
        return equipButtonSpawnPoint.position;
    }

    private void HandleIncome() {
        if (!IsActive) return;

        incomeTimer += Time.deltaTime;
        if (incomeTimer >= Constant.INCOME_INTERVAL) {
            AddMoney(incomeAmount);
            incomeTimer = 0f;
        }
    }

    private void AddMoney(float amount) {
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

        OnBrainrotChanged?.Invoke(this, EventArgs.Empty);
    }

    private void CollectIncome() {
        if (totalIncome <= 0f) return;

        PlayerProgress.Instance.AddMoney(totalIncome);
        totalIncome = 0f;
        OnTotalIncomeChanged?.Invoke(this, EventArgs.Empty);
    }

    private void ClearBrainrot() {
        PlayerProgress.Instance.AddMoney(totalIncome);
        currentBrainrotSO = null;
        incomeAmount = 0f;
        totalIncome = 0f;
        baseSlotVisual.OnClear();
        OnBrainrotChanged?.Invoke(this, EventArgs.Empty);
    }
}