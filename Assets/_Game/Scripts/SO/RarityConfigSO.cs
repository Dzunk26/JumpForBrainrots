using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu()]
public class RarityConfigSO : ScriptableObject {
    [SerializeField] private BrainrotRarity rarity;
    [SerializeField] private Material backgroundMaterial;
    [SerializeField] private Material textMaterial;

    public bool IsMatchRarity(BrainrotRarity rarity) {
        return this.rarity == rarity;
    }

    public BrainrotRarity GetBrainrotRarity() {
        return rarity;
    }

    public Material GetBackgroundMaterial() {
        return backgroundMaterial;
    }

    public Material GetTextMaterial() {
        return textMaterial;
    }
}