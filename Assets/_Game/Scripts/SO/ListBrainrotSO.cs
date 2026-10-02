using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu()]
public class ListBrainrotSO : ScriptableObject {
    [SerializeField] private List<BrainrotSO> listBrainrotSO;

    public List<BrainrotSO> GetBrainrotSOsByRarity(BrainrotRarity brainrotRarity) {
        List<BrainrotSO> brainrotSOs = new List<BrainrotSO>();

        foreach (BrainrotSO brainrotSO in listBrainrotSO) {
            if (brainrotSO.IsMatchRarity(brainrotRarity)) {
                brainrotSOs.Add(brainrotSO);
            }
        }

        return brainrotSOs;
    }

    public BrainrotSO GetBrainrotSOByID(int id) {
        BrainrotSO brainrotSO = null;

        foreach (BrainrotSO brainrotSOInList in listBrainrotSO) {
            if (brainrotSOInList.IsMatchID(id)) {
                brainrotSO = brainrotSOInList;
            }
        }

        return brainrotSO;
    }
}