using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu()]
public class ListUpgradeConfigSO : ScriptableObject {
    [SerializeField] private List<UpgradeConfigSO> listUpgradeConfigSO;

    public UpgradeConfigSO GetConfigByType(UpgradeType upgradeType) {
        for (int i = 0; i < listUpgradeConfigSO.Count; i++) {
            if (listUpgradeConfigSO[i].MatchUpgradeType(upgradeType)) {
                return listUpgradeConfigSO[i];
            }
        }

        return null;
    }
}