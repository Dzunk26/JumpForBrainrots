using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class KnockbackState {
    public static void OnEnter(Bot bot) {
        bot.OnEnterKnockback();
    }

    public static void OnExecute(Bot bot) {
        bot.OnExecuteKnockback();
    }

    public static void OnExit(Bot bot) {

    }
}
