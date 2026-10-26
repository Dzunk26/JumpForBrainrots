using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SkinModel : MonoBehaviour {
    public Animator Animator => animator;

    [SerializeField] private Animator animator;
    [SerializeField] private SkinAnimEvent skinAnimEvent;
    [SerializeField] private Transform batHoldPoint;

    public void OnInit(Character owner) {
        skinAnimEvent.OnInit(owner);
    }

    // diem cam gay nam duoi xuong tay phai, moi skin mot goc xoay rieng vi rig khac nhau
    public Transform GetBatHoldPoint() {
        return batHoldPoint;
    }

    public void Active() {
        gameObject.SetActive(true);
    }

    public void DeActive() {
        gameObject.SetActive(false);
    }
}