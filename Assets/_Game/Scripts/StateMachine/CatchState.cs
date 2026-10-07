using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class CatchState {
    public static void OnEnter(Bot bot) {
        bot.OnEnterCatch();
    }

    public static void OnExecute(Bot bot) {
        bot.OnExecuteCatch();
    }

    public static void OnExit(Bot bot) {

    }
}