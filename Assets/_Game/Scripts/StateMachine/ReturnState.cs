using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class ReturnState {
    public static void OnEnter(Bot bot) {
        bot.OnEnterReturn();
    }

    public static void OnExecute(Bot bot) {
        bot.OnExecuteReturn();
    }

    public static void OnExit(Bot bot) {

    }
}