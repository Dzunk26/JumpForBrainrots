using BreakInfinity;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BaseHouse : MonoBehaviour {
    public event EventHandler OnHouseLevelChanged;
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


    private Character owner;
    private int houseLevel;
    private Transform tf;

    public void OnInit(Character owner, int houseLevel) {
        this.owner = owner;
        this.houseLevel = Mathf.Clamp(houseLevel, 1, GetMaxLevel());
        baseBoard.OnInit(owner);

        for (int i = 0; i < listBaseFloor.Count; i++) {
            listBaseFloor[i].OnInit(this);

            if (i < this.houseLevel) {
                listBaseFloor[i].UnLock();
            }
            else {
                listBaseFloor[i].Lock();
            }
        }

        OnHouseLevelChanged?.Invoke(this, EventArgs.Empty);
    }

    public void OnDespawn() {
        owner = null;
        houseLevel = 0;
        baseBoard.OnDespawn();

        for (int i = 0; i < listBaseFloor.Count; i++) {
            if (i == 0) {
                listBaseFloor[i].UnLock();
            }
            else {
                listBaseFloor[i].Lock();
            }
        }

        OnHouseLevelChanged?.Invoke(this, EventArgs.Empty);
    }

    public void Upgrade() {
        if (IsMaxLevel()) return;

        houseLevel++;
        listBaseFloor[houseLevel - 1].UnLock();
        MarkDirty();
        OnHouseLevelChanged?.Invoke(this, EventArgs.Empty);
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
        if (!IsOwnedByPlayer()) return;

        owner.UpgradeBaseHouse();
    }

    public bool IsOwnedByPlayer() {
        return owner is Player;
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