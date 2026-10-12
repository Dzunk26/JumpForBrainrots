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
    protected Vector3 spawnPoint;

    private Transform tf;

    public virtual void OnInit() {
        spawnPoint = baseHouse.GetSpawnPosition();
        InitBaseHouse();
    }

    public virtual void OnDespawn() { }

    public virtual void OnCaptureBrainrot() { }

    public virtual void OnInteractBaseSlot() { }

    public virtual void Dead() { }

    public virtual void InitBaseHouse() { }

    public virtual void UpgradeBaseHouse() { }

    public BaseHouse GetBaseHouse() {
        return baseHouse;
    }
}