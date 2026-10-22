using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SkinManager : Singleton<SkinManager>, IDataPersistence {
    public bool IsDirty => isDirty;

    [SerializeField] private ListSkinSO listSkinSO;

    private HashSet<int> ownedSkinIds = new HashSet<int>();
    private int equippedSkinId;
    private bool isDirty;

    public void LoadData(GameData gameData) {
        ownedSkinIds.Clear();

        if (gameData.ownedSkinIds != null) {
            ownedSkinIds.UnionWith(gameData.ownedSkinIds);
        }

        // skin mac dinh luon duoc so huu
        int defaultSkinId = listSkinSO.GetDefaultSkinSO().GetID();
        ownedSkinIds.Add(defaultSkinId);
        equippedSkinId = ownedSkinIds.Contains(gameData.equippedSkinId) ? gameData.equippedSkinId : defaultSkinId;
    }

    public void SaveData(ref GameData gameData) {
        gameData.ownedSkinIds = new List<int>(ownedSkinIds);
        gameData.equippedSkinId = equippedSkinId;
        isDirty = false;
    }

    public bool IsOwned(SkinSO skinSO) {
        return skinSO != null && ownedSkinIds.Contains(skinSO.GetID());
    }

    public bool IsEquipped(SkinSO skinSO) {
        return skinSO != null && skinSO.IsMatchID(equippedSkinId);
    }

    public SkinSO GetEquippedSkinSO() {
        SkinSO skinSO = listSkinSO.GetSkinSOByID(equippedSkinId);
        if (skinSO == null) {
            skinSO = listSkinSO.GetDefaultSkinSO();
        }

        return skinSO;
    }

    public bool UnlockSkin(SkinSO skinSO) {
        if (IsOwned(skinSO)) return false;

        ownedSkinIds.Add(skinSO.GetID());
        isDirty = true;
        return true;
    }

    public void EquipSkin(SkinSO skinSO) {
        if (!IsOwned(skinSO)) return;

        equippedSkinId = skinSO.GetID();
        isDirty = true;
    }
}
