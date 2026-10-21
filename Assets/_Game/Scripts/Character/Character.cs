using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum HeldBrainrotSource {
    None,
    Captured,
    Inventory,
}

public class Character : GameUnit {
    [SerializeField] protected CharacterStat characterStat;
    [SerializeField] protected CharacterVisual characterVisual;
    [SerializeField] protected BaseHouse baseHouse;

    protected BrainrotSO currentHoldBrainrotSO;
    protected HeldBrainrotSource heldBrainrotSource;
    protected Vector3 spawnPoint;
    protected string characterName;

    public virtual void OnInit() {
        spawnPoint = baseHouse.GetSpawnPosition();
        InitBaseHouse();
    }

    public virtual void OnDespawn() { }

    public virtual void StoreCapturedBrainrot() { }

    public virtual void OnCaptureBrainrot() { }

    public virtual void OnInteractBaseSlot() { }

    public virtual void Dead() { }

    public virtual void InitBaseHouse() { }

    public virtual float GetMoneyMultiplier() {
        return 1f;
    }

    public BaseHouse GetBaseHouse() {
        return baseHouse;
    }

    public string GetCharacterName() {
        return characterName;
    }

    protected virtual void HoldBrainrot(BrainrotSO brainrotSO, Vector3 fromPosition, HeldBrainrotSource source) {
        currentHoldBrainrotSO = brainrotSO;
        heldBrainrotSource = source;
        characterVisual.OnCaptureBrainrot(brainrotSO);
        characterVisual.FlyBrainrotFrom(fromPosition);
    }

    protected virtual void DropBrainrot() {
        currentHoldBrainrotSO = null;
        heldBrainrotSource = HeldBrainrotSource.None;
        characterVisual.OnDropBrainrot();
    }
}