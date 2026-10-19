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
    public BigDouble totalMoney;
    public int baseHouseLevel;
    public int rebirthLevel;
    public string playerName;
    public int[] hotbarIds;
    public List<int> backpackIds;
    public List<BaseSlotData> baseSlots;
    public List<int> collectedIds;
    public long lastSaveUnixTime;

    public GameData() {
        jumpLevel = 0;
        totalMoney = 0;
        baseHouseLevel = 1;
        rebirthLevel = 0;
        playerName = Constant.DEFAULT_PLAYER_NAME;
        hotbarIds = Enumerable.Repeat(-1, Constant.HOTBAR_SIZE).ToArray();
        backpackIds = new List<int>();
        baseSlots = new List<BaseSlotData>();
        collectedIds = new List<int>();
    }
}