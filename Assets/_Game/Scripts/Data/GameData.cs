using System.Collections;
using System.Collections.Generic;
using System.Linq;
using BreakInfinity;
using UnityEngine;

[System.Serializable]
public class BaseSlotData {
    public int floorIndex;
    public int slotIndex;
    public int brainrotID;
    public BigDouble pendingIncome;
}

[System.Serializable]
public class GameData {
    public int jumpLevel;
    public int moveSpeedLevel;
    public BigDouble totalMoney;
    public int baseHouseLevel;
    public string playerName;
    public int[] hotbarIds;
    public List<int> backpackIds;
    public List<BaseSlotData> baseSlots;
    public List<int> collectedIds;
    public long lastSaveUnixTime;

    public GameData() {
        jumpLevel = 0;
        moveSpeedLevel = 0;
        totalMoney = 0;
        baseHouseLevel = 1;
        playerName = "Player";
        hotbarIds = Enumerable.Repeat(-1, Constant.HOTBAR_SIZE).ToArray();
        backpackIds = new List<int>();
        baseSlots = new List<BaseSlotData>();
        collectedIds = new List<int>();
    }
}