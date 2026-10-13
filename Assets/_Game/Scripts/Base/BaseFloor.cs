using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BaseFloor : MonoBehaviour {
    public bool IsUnlocked => isUnlocked;

    [SerializeField] private List<BaseSlot> listBaseSlot;
    [SerializeField] private List<GameObject> listFloorObject;
    [SerializeField] private List<GameObject> listBlockObject;

    private bool isUnlocked;

    public void OnInit(BaseHouse baseHouse) {
        for (int i = 0; i < listBaseSlot.Count; i++) {
            listBaseSlot[i].SetBaseHouse(baseHouse);
        }
    }

    public void UnLock() {
        isUnlocked = true;
        SetActiveFloor(true);
        SetActiveBlockBarriers(false);

        for (int i = 0; i < listBaseSlot.Count; i++) {
            listBaseSlot[i].UnLock();
        }
    }

    public void Lock() {
        isUnlocked = false;
        SetActiveFloor(false);
        SetActiveBlockBarriers(true);

        for (int i = 0; i < listBaseSlot.Count; i++) {
            listBaseSlot[i].Lock();
        }
    }

    public List<BaseSlot> GetListBaseSlot() {
        return listBaseSlot;
    }

    private void SetActiveFloor(bool isActive) {
        gameObject.SetActive(isActive);

        for (int i = 0; i < listFloorObject.Count; i++) {
            listFloorObject[i].SetActive(isActive);
        }
    }

    public void SetActiveBlockBarriers(bool isActive) {
        for (int i = 0; i < listBlockObject.Count; i++) {
            listBlockObject[i].SetActive(isActive);
        }
    }
}