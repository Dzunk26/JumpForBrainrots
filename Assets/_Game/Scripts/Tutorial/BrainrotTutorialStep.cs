using System.Collections.Generic;
using UnityEngine;

// base cho cac buoc tutorial rieng cua Jump For Brainrots: cung cap Player va cac ham tim slot trong base
public abstract class BrainrotTutorialStep : TutorialStep {
    [SerializeField] private Player player;

    protected Player Player {
        get {
            if (player == null) player = FindAnyObjectByType<Player>();
            return player;
        }
    }

    // base house cua player chi co slot sau khi GameManager goi player.OnInit
    public override bool IsReady() {
        return Player != null && Player.GetBaseHouse().GetHouseLevel() > 0;
    }

    protected int GetActiveSlotCount() {
        int count = 0;
        List<BaseSlot> slots = Player.GetBaseHouse().GetUnlockedSlots();
        for (int i = 0; i < slots.Count; i++) {
            if (slots[i].IsActive) count++;
        }

        return count;
    }

    protected BaseSlot GetFirstEmptySlot() {
        List<BaseSlot> slots = Player.GetBaseHouse().GetUnlockedSlots();
        for (int i = 0; i < slots.Count; i++) {
            if (slots[i].IsUnlocked && !slots[i].IsActive) return slots[i];
        }

        return null;
    }

    protected BaseSlot GetFirstActiveSlot() {
        List<BaseSlot> slots = Player.GetBaseHouse().GetUnlockedSlots();
        for (int i = 0; i < slots.Count; i++) {
            if (slots[i].IsActive) return slots[i];
        }

        return null;
    }
}
