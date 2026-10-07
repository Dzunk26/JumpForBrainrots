using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Character : MonoBehaviour {
    public Transform TF {
        get {
            if (tf == null) {
                tf = transform;
            }
            return tf;
        }
    }

    [SerializeField] protected CharacterStat characterStat;
    [SerializeField] protected CharacterVisual characterVisual;
    [SerializeField] protected BaseHouse baseHouse;
    protected BrainrotSO currentCapturedBrainrotSO;

    private Transform tf;

    public virtual void OnInit() {
        InitBaseHouse();
    }

    public virtual void OnDespawn() { }

    public virtual void OnCaptureBrainrot() { }

    public virtual void OnInteractBaseSlot() { }

    public virtual void Dead() { }

    public virtual void InitBaseHouse() { }

    public BaseHouse GetBaseHouse() {
        return baseHouse;
    }
}