using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerProgress : Singleton<PlayerProgress>, IDataPersistence {
    private int jumpLevel;
    private int moveSpeedLevel;

    private double totalMoney;

    public void LoadData(GameData gameData) {
        jumpLevel = gameData.jumpLevel;
        moveSpeedLevel = gameData.moveSpeedLevel;
        totalMoney = gameData.totalMoney;
    }

    public void SaveData(ref GameData gameData) {
        gameData.jumpLevel = jumpLevel;
        gameData.moveSpeedLevel = moveSpeedLevel;
        gameData.totalMoney = totalMoney;
    }

    public void IncreaseLevel(UpgradeType upgradeType) {
        switch (upgradeType) {
            case UpgradeType.JumpPower:
                jumpLevel++;
                break;
            case UpgradeType.MoveSpeed:
                moveSpeedLevel++; 
                break;
        }
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

    public void AddMoney(float amount) {
        totalMoney += amount;
    }

    public void SpendMoney(float amount) {
        totalMoney -= amount;
    }

    public bool IsEnoughMoney(float spendAmount) {
        return totalMoney >= spendAmount;
    }

    public double GetMoney() {
        return totalMoney;
    }
}