using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class DeadState {
    public static void OnEnter(Bot bot) {
        bot.OnEnterDead();
    }

    public static void OnExecute(Bot bot) {
        bot.OnExecuteDead();
    }

    public static void OnExit(Bot bot) {

    }
}