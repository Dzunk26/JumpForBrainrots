using BreakInfinity;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;

public enum BrainrotRarity {
    Basic,
    Rare,
    Epic,
    Legendary,
    Mythic,
    Secret,
    Celestial,
    Divine,
    Infinity
}

[CreateAssetMenu()]
public class BrainrotSO : ItemSO {
    [SerializeField] private int id;
    [SerializeField] private string brainrotName;
    [SerializeField] private BrainrotRarity rarity;
    [SerializeField] private BrainrotModel brainrotModel;
    [SerializeField] private float lifeTimerMax;
    [SerializeField] private float incomeAmount;
    [SerializeField] private float afkIncomeAmount;
    [SerializeField] private BigDouble sellPrice;
    [SerializeField] private Sprite indexIcon;

    public override int GetID() {
        return id;
    }

    public override Sprite GetIcon() {
        return indexIcon;
    }

    public override bool IsSellable() {
        return true;
    }

    public string GetName() {
        return brainrotName;
    }

    public bool IsMatchRarity(BrainrotRarity rarity) {
        return this.rarity == rarity;
    }

    public BrainrotRarity GetRarity() {
        return rarity;
    }

    public BrainrotModel GetBrainrotModel() {
        return brainrotModel;
    }

    public float GetAppearTimerMax() {
        return lifeTimerMax;
    }

    public float GetIncomeAmount() {
        return incomeAmount;
    }

    public float GetAfkIncomeAmount() {
        return afkIncomeAmount;
    }

    public BigDouble GetSellPrice() {
        return sellPrice;
    }

    public Sprite GetIndexIcon() {
        return indexIcon;
    }
}