using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu()]
public class ListSkinSO : ScriptableObject {
    [SerializeField] private List<SkinSO> listSkinSO;

    public int GetSkinAmount() {
        return listSkinSO.Count;
    }

    public SkinSO GetSkinSOByIndex(int index) {
        return listSkinSO[index];
    }

    // skin dau tien trong list la skin mac dinh
    public SkinSO GetDefaultSkinSO() {
        return listSkinSO[0];
    }

    public SkinSO GetSkinSOByID(int id) {
        foreach (SkinSO skinSO in listSkinSO) {
            if (skinSO.IsMatchID(id)) return skinSO;
        }

        return null;
    }
}
