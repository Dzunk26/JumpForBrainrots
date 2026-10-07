using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BaseFloor : MonoBehaviour {
    public bool IsUnlocked => isUnlocked;

    [SerializeField] private List<BaseSlot> listBaseSlot;
    //[SerializeField] private GameObject lockBarrier;

    private bool isUnlocked;

    public void OnInit(BaseHouse baseHouse) {
        for (int i = 0; i < listBaseSlot.Count; i++) {
            listBaseSlot[i].SetBaseHouse(baseHouse);
        }
    }

    public void UnLock() {
        isUnlocked = true;
        //lockBarrier.SetActive(false);

        for (int i = 0; i < listBaseSlot.Count; i++) {
            listBaseSlot[i].UnLock();
        }
    }

    public void Lock() {
        isUnlocked = false;
        //lockBarrier.SetActive(true);

        for (int i = 0; i < listBaseSlot.Count; i++) {
            listBaseSlot[i].Lock();
        }
    }

    public List<BaseSlot> GetListBaseSlot() {
        return listBaseSlot;
    }
}