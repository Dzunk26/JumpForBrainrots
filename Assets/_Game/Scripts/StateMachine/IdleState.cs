using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class IdleState {
    public static void OnEnter(Bot bot) {
        bot.OnEnterIdle();
    }

    public static void OnExecute(Bot bot) {
        bot.OnExecuteIdle();
    }

    public static void OnExit(Bot bot) {

    }
}