using BreakInfinity;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerProgress : Singleton<PlayerProgress>, IDataPersistence {
    public event EventHandler OnLoadPlayerData;

    private int jumpLevel;
    private int moveSpeedLevel;
    private int baseHouseLevel; 

    private BigDouble totalMoney;

    public void LoadData(GameData gameData) {
        jumpLevel = gameData.jumpLevel;
        moveSpeedLevel = gameData.moveSpeedLevel;
        baseHouseLevel = gameData.baseHouseLevel;
        totalMoney = gameData.totalMoney;

        OnLoadPlayerData?.Invoke(this, EventArgs.Empty);
    }

    public void SaveData(ref GameData gameData) {
        gameData.jumpLevel = jumpLevel;
        gameData.moveSpeedLevel = moveSpeedLevel;
        gameData.baseHouseLevel = baseHouseLevel;
        gameData.totalMoney = totalMoney;
    }

    public void IncreaseLevel(UpgradeType upgradeType) {
        switch (upgradeType) {
            case UpgradeType.JumpPower:
                jumpLevel += 2;
                break;
            case UpgradeType.MoveSpeed:
                moveSpeedLevel++; 
                break;
        }
        Debug.Log(jumpLevel);
    }

    public int GetLevel(UpgradeType upgradeType) {
        switch (upgradeType) {
            default: return 0;
            case UpgradeType.JumpPower:
                return jumpLevel;
            case UpgradeType.MoveSpeed:
                return moveSpeedLevel;
        }
    }

    public void AddMoney(BigDouble amount) {
        totalMoney += amount;
    }

    public void SpendMoney(BigDouble amount) {
        totalMoney -= amount;
    }

    public bool IsEnoughMoney(BigDouble spendAmount) {
        return totalMoney >= spendAmount;
    }

    public BigDouble GetMoney() {
        return totalMoney;
    }

    public int GetBaseHouseLevel() {
        return baseHouseLevel;
    }

    public void SetBaseHouseLevel(int level) {
        baseHouseLevel = level;
    }
}