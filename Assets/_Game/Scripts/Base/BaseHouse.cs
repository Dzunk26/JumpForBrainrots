using BreakInfinity;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BaseHouse : MonoBehaviour {
    public bool IsDirty { get; private set; }
    public void MarkDirty() => IsDirty = true;
    public void ClearDirty() => IsDirty = false;
    public bool IsEmpty => owner == null;
    public Transform TF {
        get {
            if (tf == null) {
                tf = transform;
            }

            return tf;
        }
    }

    [SerializeField] private List<BaseFloor> listBaseFloor;
    [SerializeField] private BaseBoard baseBoard;
    [SerializeField] private BaseHouseSO baseHouseSO;
    [SerializeField] private Transform spawnPoint;
    [SerializeField] private float upgradeButtonRange = 4f;

    private Character owner;
    private int houseLevel;
    private Transform tf;

    private void Awake() {
        for (int i = 0; i < listBaseFloor.Count; i++) {
            listBaseFloor[i].OnInit(this);
        }
    }

    public void OnInit(Character owner, int houseLevel) {
        this.owner = owner;
        this.houseLevel = Mathf.Clamp(houseLevel, 1, GetMaxLevel());
        baseBoard.OnInit(owner);

        for (int i = 0; i < listBaseFloor.Count; i++) {
            if (i < this.houseLevel) {
                listBaseFloor[i].UnLock();
            }
            else {
                listBaseFloor[i].Lock();
            }
        }
    }

    public void OnDespawn() {
        owner = null;
        houseLevel = 0;
        baseBoard.OnDespawn();

        for (int i = 0; i < listBaseFloor.Count; i++) {
            List<BaseSlot> slots = listBaseFloor[i].GetListBaseSlot();

            for (int j = 0; j < slots.Count; j++) {
                slots[j].ClearSlot();
            }

            if (i == 0) {
                listBaseFloor[i].UnLock();
            }
            else {
                listBaseFloor[i].Lock();
            }
        }
    }

    public List<BaseSlot> GetUnlockedSlots() {
        List<BaseSlot> unlockedSlots = new List<BaseSlot>();

        for (int i = 0; i < houseLevel; i++) {
            unlockedSlots.AddRange(listBaseFloor[i].GetListBaseSlot());
        }

        return unlockedSlots;
    }

    public void Upgrade() {
        if (IsMaxLevel()) return;

        houseLevel++;
        listBaseFloor[houseLevel - 1].UnLock();
        MarkDirty();
    }

    public bool IsOwnedBy(Character character) {
        return owner == character;
    }

    public bool IsMaxLevel() {
        return houseLevel >= GetMaxLevel();
    }

    public int GetMaxLevel() {
        return listBaseFloor.Count;
    }

    public int GetHouseLevel() {
        return houseLevel;
    }

    public BigDouble GetUpgradeCost() {
        return baseHouseSO.GetUpgradeCost(houseLevel - 1);
    }

    public Vector3 GetSpawnPosition() {
        return spawnPoint.position;
    }

    public Vector3 GetSpawnForward() {
        return spawnPoint.forward;
    }

    public void OnClickUpgrade() {
        if (!CanUpgradeByPlayer()) return;

        BigDouble cost = GetUpgradeCost();
        if (!PlayerProgress.Instance.IsEnoughMoney(cost)) return;

        PlayerProgress.Instance.SpendMoney(cost);
        UpgradeByPlayer();
    }

    public void OnClickFreeUpgrade() {
        if (!CanUpgradeByPlayer()) return;

        UpgradeByPlayer();
    }

    public bool IsEnoughMoneyToUpgrade() {
        return PlayerProgress.Instance.IsEnoughMoney(GetUpgradeCost());
    }

    public bool CanShowUpgradeButton(Vector3 position) {
        if (!CanUpgradeByPlayer()) return false;

        Vector3 dirToBoard = GetUpgradeButtonSpawnPoint() - position;
        dirToBoard.y = 0f;
        return dirToBoard.sqrMagnitude <= upgradeButtonRange * upgradeButtonRange;
    }

    public Vector3 GetUpgradeButtonSpawnPoint() {
        return listBaseFloor[houseLevel].GetUpgradeButtonSpawnPoint();
    }

    private bool CanUpgradeByPlayer() {
        return IsOwnedByPlayer() && !IsMaxLevel();
    }

    private void UpgradeByPlayer() {
        Upgrade();
        PlayerProgress.Instance.SetBaseHouseLevel(houseLevel);
    }

    public bool IsOwnedByPlayer() {
        return owner is Player;
    }

    public float GetMoneyMultiplier() {
        if (owner == null) return 1f;

        return owner.GetMoneyMultiplier();
    }

    public BigDouble AddAfkIncome(long afkSeconds) {
        BigDouble total = 0;

        for (int i = 0; i < listBaseFloor.Count; i++) {
            List<BaseSlot> slots = listBaseFloor[i].GetListBaseSlot();

            for (int j = 0; j < slots.Count; j++) {
                total += slots[j].AddAfkIncome(afkSeconds);
            }
        }

        if (total > 0) MarkDirty();

        return total;
    }
    public List<BaseSlotData> GetSlotsData() {
        List<BaseSlotData> result = new List<BaseSlotData>();

        for (int i = 0; i < listBaseFloor.Count; i++) {
            List<BaseSlot> slots = listBaseFloor[i].GetListBaseSlot();

            for (int j = 0; j < slots.Count; j++) {
                if (!slots[j].IsActive) continue;

                result.Add(new BaseSlotData {
                    floorIndex = i,
                    slotIndex = j,
                    brainrotID = slots[j].GetCurrentBrainrot().GetID(),
                    pendingIncome = slots[j].GetTotalIncome(),
                });
            }
        }

        return result;
    }

    public void LoadSlots(List<BaseSlotData> slotsData, ListBrainrotSO listBrainrotSO) {
        if (slotsData == null) return;

        foreach (BaseSlotData data in slotsData) {
            if (data.floorIndex >= listBaseFloor.Count) continue;

            List<BaseSlot> slots = listBaseFloor[data.floorIndex].GetListBaseSlot();
            if (data.slotIndex >= slots.Count) continue;

            BrainrotSO brainrotSO = listBrainrotSO.GetBrainrotSOByID(data.brainrotID);
            if (brainrotSO == null) continue;

            slots[data.slotIndex].LoadBrainrot(brainrotSO, data.pendingIncome);
        }
    }

}