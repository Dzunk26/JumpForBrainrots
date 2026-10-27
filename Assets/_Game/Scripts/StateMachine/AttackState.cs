using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// cam gay duoi theo 1 nguoi (player hoac bot khac) de danh: cuop brainrot hoac troll
public static class AttackState {
    public static void OnEnter(Bot bot) {
        bot.OnEnterAttack();
    }

    public static void OnExecute(Bot bot) {
        bot.OnExecuteAttack();
    }

    public static void OnExit(Bot bot) {
        bot.OnExitAttack();
    }
}
