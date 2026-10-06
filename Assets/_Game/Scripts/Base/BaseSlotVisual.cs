using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BaseSlotVisual : MonoBehaviour {
    [SerializeField] private Transform displayPoint;

    private Dictionary<BrainrotSO, BrainrotModel> dictBrainrotModel = new Dictionary<BrainrotSO, BrainrotModel>();
    private BrainrotModel currentModel;

    public void OnInit(BrainrotSO brainrotSO) {
        if (currentModel != null) {
            currentModel.DeActive();
        }

        currentModel = GetBrainrotModel(brainrotSO);
        currentModel.Active();
    }

    public void OnClear() {
        if (currentModel == null) return;

        currentModel.DeActive();
        currentModel = null;
    }

    private BrainrotModel GetBrainrotModel(BrainrotSO brainrotSO) {
        if (!dictBrainrotModel.ContainsKey(brainrotSO)) {
            dictBrainrotModel[brainrotSO] = Instantiate(brainrotSO.GetBrainrotModel(), displayPoint);
            dictBrainrotModel[brainrotSO].ResetLocalPosition();
        }

        return dictBrainrotModel[brainrotSO];
    }
}