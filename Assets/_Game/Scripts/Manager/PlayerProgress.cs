using BreakInfinity;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerProgress : Singleton<PlayerProgress>, IDataPersistence {
    public bool IsDirty => isDirty;
    public event EventHandler OnLoadPlayerData;

    private bool isDirty;
    private int jumpLevel;
    private int moveSpeedLevel;
    private int baseHouseLevel;
    private string playerName;
    private BigDouble totalMoney;

    public void LoadData(GameData gameData) {
        jumpLevel = gameData.jumpLevel;
        moveSpeedLevel = gameData.moveSpeedLevel;
        baseHouseLevel = gameData.baseHouseLevel;
        totalMoney = gameData.totalMoney;
        playerName = gameData.playerName;

        OnLoadPlayerData?.Invoke(this, EventArgs.Empty);
    }

    public void SaveData(ref GameData gameData) {
        gameData.jumpLevel = jumpLevel;
        gameData.moveSpeedLevel = moveSpeedLevel;
        gameData.baseHouseLevel = baseHouseLevel;
        gameData.totalMoney = totalMoney;
        gameData.playerName = playerName;
        isDirty = false;
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
        isDirty = true;
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
        isDirty = true;
    }

    public void SpendMoney(BigDouble amount) {
        totalMoney -= amount;
        isDirty = true;
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
        isDirty = true;
    }

    public string GetPlayerName() {
        return playerName;
    }
}