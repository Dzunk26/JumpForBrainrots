using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IndexManager : Singleton<IndexManager>, IDataPersistence {
    public bool IsDirty => isDirty;

    private HashSet<int> collectedIds = new HashSet<int>();
    private bool isDirty;

    public void LoadData(GameData gameData) {
        collectedIds.Clear();

        if (gameData.collectedIds != null) {
            collectedIds.UnionWith(gameData.collectedIds);
        }

        if (gameData.hotbarIds != null) {
            foreach (int id in gameData.hotbarIds) {
                if (id >= 0) collectedIds.Add(id);
            }
        }

        if (gameData.backpackIds != null) {
            collectedIds.UnionWith(gameData.backpackIds);
        }

        if (gameData.baseSlots != null) {
            foreach (BaseSlotData slotData in gameData.baseSlots) {
                collectedIds.Add(slotData.brainrotID);
            }
        }
    }

    public void SaveData(ref GameData gameData) {
        gameData.collectedIds = new List<int>(collectedIds);
        isDirty = false;
    }

    public bool Collect(BrainrotSO brainrotSO) {
        if (brainrotSO == null) return false;
        if (!collectedIds.Add(brainrotSO.GetID())) return false;

        isDirty = true;
        return true;
    }

    public bool IsCollected(BrainrotSO brainrotSO) {
        return brainrotSO != null && collectedIds.Contains(brainrotSO.GetID());
    }
}