using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BaseSlotVisual : MonoBehaviour {
    [SerializeField] private BrainrotInfor brainrotInfor;
    [SerializeField] private Transform displayPoint;

    private Dictionary<BrainrotSO, BrainrotModel> dictBrainrotModel = new Dictionary<BrainrotSO, BrainrotModel>();
    private BrainrotModel currentModel;

    public void OnInit(BrainrotSO brainrotSO) {
        if (currentModel != null) {
            currentModel.DeActive();
        }

        currentModel = GetBrainrotModel(brainrotSO);
        currentModel.ResetLocalRotation();
        currentModel.Active();
        brainrotInfor.OnInit(brainrotSO, currentModel.GetInfoLocalPosition());
    }

    public void OnClear() {
        if (currentModel == null) return;

        currentModel.DeActive();
        currentModel = null;
        brainrotInfor.OnDespawn();
    }

    public void FlyBrainrotFrom(Vector3 fromPosition) {
        if (currentModel == null) return;

        currentModel.FlyFrom(fromPosition);
    }

    public Vector3 GetDisplayPosition() {
        return displayPoint.position;
    }

    private BrainrotModel GetBrainrotModel(BrainrotSO brainrotSO) {
        if (!dictBrainrotModel.ContainsKey(brainrotSO)) {
            dictBrainrotModel[brainrotSO] = Instantiate(brainrotSO.GetBrainrotModel(), displayPoint);
            dictBrainrotModel[brainrotSO].ResetLocalPosition();
        }

        return dictBrainrotModel[brainrotSO];
    }
}