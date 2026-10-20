using BreakInfinity;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerProgress : Singleton<PlayerProgress>, IDataPersistence {
    public bool IsDirty => isDirty;
    public event EventHandler OnLoadPlayerData;
    public event Action OnDataChanged;

    private bool isDirty;
    private int jumpLevel;
    private int baseHouseLevel;
    private int rebirthLevel;
    private string playerName;
    private BigDouble totalMoney;

    public void LoadData(GameData gameData) {
        jumpLevel = gameData.jumpLevel;
        baseHouseLevel = gameData.baseHouseLevel;
        rebirthLevel = gameData.rebirthLevel;
        totalMoney = gameData.totalMoney;
        playerName = gameData.playerName;

        OnLoadPlayerData?.Invoke(this, EventArgs.Empty);
        OnDataChanged?.Invoke();
    }

    public void SaveData(ref GameData gameData) {
        gameData.jumpLevel = jumpLevel;
        gameData.baseHouseLevel = baseHouseLevel;
        gameData.rebirthLevel = rebirthLevel;
        gameData.totalMoney = totalMoney;
        gameData.playerName = playerName;
        isDirty = false;
    }

    public void IncreaseLevel(UpgradeType upgradeType, int amount) {
        switch (upgradeType) {
            case UpgradeType.JumpPower:
                jumpLevel += amount;
                break;
        }
        Debug.Log(jumpLevel);
        MarkDirty();
    }

    public int GetLevel(UpgradeType upgradeType) {
        switch (upgradeType) {
            default: return 0;
            case UpgradeType.JumpPower:
                return jumpLevel;
        }
    }

    public void AddMoney(BigDouble amount) {
        totalMoney += amount;
        MarkDirty();
    }

    public void SpendMoney(BigDouble amount) {
        totalMoney -= amount;
        MarkDirty();
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
        MarkDirty();
    }

    public int GetRebirthLevel() {
        return rebirthLevel;
    }

    public void Rebirth() {
        rebirthLevel++;
        jumpLevel = 0;
        MarkDirty();
    }

    public string GetPlayerName() {
        return playerName;
    }

    private void MarkDirty() {
        isDirty = true;
        OnDataChanged?.Invoke();
    }
}