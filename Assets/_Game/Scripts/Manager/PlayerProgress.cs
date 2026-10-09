using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerProgress : Singleton<PlayerProgress>, IDataPersistence {
    private int jumpLevel;
    private int moveSpeedLevel;
    private int baseHouseLevel; 

    private double totalMoney;

    public void LoadData(GameData gameData) {
        jumpLevel = gameData.jumpLevel;
        moveSpeedLevel = gameData.moveSpeedLevel;
        baseHouseLevel = gameData.baseHouseLevel;
        totalMoney = gameData.totalMoney;
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

    public void AddMoney(double amount) {
        totalMoney += amount;
    }

    public void SpendMoney(double amount) {
        totalMoney -= amount;
    }

    public bool IsEnoughMoney(double spendAmount) {
        return totalMoney >= spendAmount;
    }

    public double GetMoney() {
        return totalMoney;
    }

    public int GetBaseHouseLevel() {
        return baseHouseLevel;
    }

    public void SetBaseHouseLevel(int level) {
        baseHouseLevel = level;
    }
}