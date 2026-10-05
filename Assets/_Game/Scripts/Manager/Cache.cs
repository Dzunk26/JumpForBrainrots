using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class Cache {
    private static Dictionary<AnimState, string> dictAnimName = new Dictionary<AnimState, string>();
    private static Dictionary<AnimState, int> dictAnimHash = new Dictionary<AnimState, int>();
    private static Dictionary<Collider, Player> dictPlayer = new Dictionary<Collider, Player>();
    private static Dictionary<Collider, Brainrot> dictBrainrot = new Dictionary<Collider, Brainrot>();
    private static Dictionary<Collider, BaseSlot> dictBaseSlot = new Dictionary<Collider, BaseSlot>();

    public static string GetAnimName(AnimState animState) {
        if (!dictAnimName.ContainsKey(animState)) {
            dictAnimName[animState] = animState.ToString();
        }

        return dictAnimName[animState];
    }

    public static int GetAnimHash(AnimState animState) {
        if (!dictAnimHash.ContainsKey(animState)) {
            string animName = GetAnimName(animState);
            int hash = Animator.StringToHash(animName);
            dictAnimHash[animState] = hash;
        }
        return dictAnimHash[animState];
    }

    public static Player GetPlayer(Collider collider) {
        if (!dictPlayer.ContainsKey(collider)) {
            Player player = collider.GetComponent<Player>();
            dictPlayer[collider] = player;
        }

        return dictPlayer[collider];
    }

    public static Brainrot GetBrainrot(Collider collider) {
        if (!dictBrainrot.ContainsKey(collider)) {
            Brainrot brainrot = collider.GetComponent<Brainrot>();
            dictBrainrot[collider] = brainrot;
        }

        return dictBrainrot[collider];
    }

    public static BaseSlot GetBaseSlot(Collider collider) {
        if (!dictBaseSlot.ContainsKey(collider)) {
            BaseSlot baseSlot = collider.GetComponent<BaseSlot>();
            dictBaseSlot[collider] = baseSlot;
        }

        return dictBaseSlot[collider];
    }

    public static void Clear() {
        dictAnimName.Clear();
        dictAnimHash.Clear();
        dictPlayer.Clear();
        dictBrainrot.Clear();
        dictBaseSlot.Clear();
    }
}