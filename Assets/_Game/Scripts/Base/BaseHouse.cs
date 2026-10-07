using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BaseHouse : MonoBehaviour {
    public event EventHandler OnHouseLevelChanged;

    [SerializeField] private List<BaseFloor> listBaseFloor;
    [SerializeField] private BaseHouseSO baseHouseSO;

    private Character owner;
    private int houseLevel;

    public void OnInit(Character owner, int houseLevel) {
        this.owner = owner;
        this.houseLevel = Mathf.Clamp(houseLevel, 0, GetMaxLevel());

        for (int i = 0; i < listBaseFloor.Count; i++) {
            listBaseFloor[i].OnInit(this);

            if (i <= this.houseLevel) {
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
        listBaseFloor[houseLevel].UnLock();
        OnHouseLevelChanged?.Invoke(this, EventArgs.Empty);
    }

    public bool IsOwnedBy(Character character) {
        return owner == character;
    }

    public bool IsMaxLevel() {
        return houseLevel >= GetMaxLevel();
    }

    public int GetMaxLevel() {
        return listBaseFloor.Count - 1;
    }

    public int GetHouseLevel() {
        return houseLevel;
    }

    public double GetUpgradeCost() {
        return baseHouseSO.GetUpgradeCost(houseLevel);
    }
}