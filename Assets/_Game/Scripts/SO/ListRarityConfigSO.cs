using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu()]
public class ListRarityConfigSO : ScriptableObject {
    [SerializeField] private List<RarityConfigSO> listRarityConfig;

    public RarityConfigSO GetRarityConfig(BrainrotRarity rarity) {
        for (int i = 0; i < listRarityConfig.Count; i++) {
            if (listRarityConfig[i].IsMatchRarity(rarity)) {
                return listRarityConfig[i];
            }
        }

        return null;
    }

    public RarityConfigSO GetRarityConfigByIndex(int index) {
        return listRarityConfig[index];
    }

    public int GetRarityAmount() {
        return listRarityConfig.Count;
    }
}