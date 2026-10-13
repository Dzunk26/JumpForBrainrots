using BreakInfinity;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameData {
    public int jumpLevel;
    public int moveSpeedLevel;
    public BigDouble totalMoney;
    public int baseHouseLevel;

    public GameData() {
        jumpLevel = 0;
        moveSpeedLevel = 0;
        totalMoney = 0;
        baseHouseLevel = 1;
    }
}