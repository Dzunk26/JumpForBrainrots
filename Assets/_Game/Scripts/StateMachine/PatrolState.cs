using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class PatrolState {
    public static void OnEnter(Bot bot) {
        bot.OnEnterPatrol();
    }

    public static void OnExecute(Bot bot) {
        bot.OnExecutePatrol();
    }

    public static void OnExit(Bot bot) {

    }
}