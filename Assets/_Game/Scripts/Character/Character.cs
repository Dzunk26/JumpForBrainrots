using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Character : GameUnit {
    [SerializeField] protected CharacterStat characterStat;
    [SerializeField] protected CharacterVisual characterVisual;
    [SerializeField] protected BaseHouse baseHouse;

    protected BrainrotSO currentCapturedBrainrotSO;
    protected Vector3 spawnPoint;
    protected string characterName;

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

    public string GetCharacterName() {
        return characterName;
    }

    protected virtual void HoldBrainrot(BrainrotSO brainrotSO, Vector3 fromPosition) {
        currentCapturedBrainrotSO = brainrotSO;
        characterVisual.OnCaptureBrainrot(brainrotSO);
        characterVisual.FlyBrainrotFrom(fromPosition);
    }

    protected virtual void DropBrainrot() {
        currentCapturedBrainrotSO = null;
        characterVisual.OnDropBrainrot();
    }
}