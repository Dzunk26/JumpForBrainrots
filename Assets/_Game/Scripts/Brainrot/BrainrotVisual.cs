using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BrainrotVisual : MonoBehaviour {
    private Dictionary<BrainrotSO, BrainrotModel> dictBrainrotModel = new Dictionary<BrainrotSO, BrainrotModel> ();
    private BrainrotModel currentModel;

    public void OnInit(BrainrotSO brainrotSO) {
        if (currentModel != null) {
            currentModel.DeActive();
        }

        currentModel = GetBrainrotModel(brainrotSO);
        currentModel.Active();
    }

    public Vector3 GetInfoLocalPosition() {
        return currentModel.GetInfoLocalPosition();
    }

    private BrainrotModel GetBrainrotModel(BrainrotSO brainrotSO) {
        if (!dictBrainrotModel.ContainsKey(brainrotSO)) {
            dictBrainrotModel[brainrotSO] = Instantiate(brainrotSO.GetBrainrotModel(), transform);
            dictBrainrotModel[brainrotSO].ResetLocalPosition();
        }

        return dictBrainrotModel[brainrotSO];
    }
}