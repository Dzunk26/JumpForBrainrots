using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class Cache {
    private static Dictionary<AnimState, string> dictAnimName = new Dictionary<AnimState, string>();
    private static Dictionary<Collider, Player> dictPlayer = new Dictionary<Collider, Player>();

    public static string GetAnimName(AnimState animState) {
        if (!dictAnimName.ContainsKey(animState)) {
            dictAnimName[animState] = animState.ToString();
        }

        return dictAnimName[animState];
    }

    public static Player GetPlayer(Collider collider) {
        if (!dictPlayer.ContainsKey(collider)) {
            Player player = collider.GetComponent<Player>();
            dictPlayer[collider] = player;
        }

        return dictPlayer[collider];
    }

    public static void Clear() {
        dictAnimName.Clear();
        dictPlayer.Clear();
    }
}