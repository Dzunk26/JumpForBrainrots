using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu()]
public class BaseHouseSO : ScriptableObject {
    [SerializeField] private List<double> listUpgradeCost;

    public double GetUpgradeCost(int level) {
        if (level < 0 || level >= listUpgradeCost.Count) {
            Debug.LogError("No upgrade cost for level " + level);
            return double.MaxValue;
        }

        return listUpgradeCost[level];
    }
}