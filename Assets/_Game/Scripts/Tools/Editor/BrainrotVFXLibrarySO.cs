using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

// cach dat hieu ung so voi model
// Ground: day hieu ung cham dat | Body: tam hieu ung o giua than | Top: day hieu ung o dinh dau
// Pivot...: dat goc (pivot) cua prefab o chan / dinh dau / tren dau
public enum BrainrotVFXAnchor {
    Ground,
    Body,
    Top,
    PivotGround,
    PivotTop,
    PivotAbove
}

// Width: be ngang hieu ung = duong kinh model * size | Height: chieu cao = chieu cao model * size | Inside: vua trong ca 2
public enum BrainrotVFXFit {
    Width,
    Height,
    Inside
}

// Rarity: chon bien the mau theo mau rarity | Model: theo mau model | Fixed: giu bien the dau tien
public enum BrainrotVFXColorSource {
    Rarity,
    Model,
    Fixed
}

// layer dang mesh dung material (vd shader galaxy): Disc = de tron duoi chan, Orb = qua cau lo lung tren dau
public enum BrainrotVFXMeshShape {
    None,
    Disc,
    Orb
}

[System.Serializable]
public class BrainrotVFXLayer {
    public string label = "Layer";
    public List<GameObject> variants = new List<GameObject>();
    public BrainrotVFXColorSource colorSource = BrainrotVFXColorSource.Rarity;
    [Tooltip("Ten mau uu tien (Red, Gold, Pink, Rainbow...), de trong thi theo Color Source")]
    public string colorHint = "";
    public BrainrotRarity minRarity = BrainrotRarity.Basic;
    public BrainrotVFXAnchor anchor = BrainrotVFXAnchor.Ground;
    public BrainrotVFXFit fit = BrainrotVFXFit.Width;
    public float size = 1f;
    [Tooltip("Dich len theo ti le chieu cao model")]
    public float heightOffset;
    [Tooltip("Nhan so hat phat ra")]
    public float emission = 1f;

    [Header("Mesh Layer (bo trong variants)")]
    public BrainrotVFXMeshShape meshShape = BrainrotVFXMeshShape.None;
    public Material meshMaterial;
}

[System.Serializable]
public class BrainrotVFXRarityRecipe {
    public BrainrotRarity rarity;
    public List<BrainrotVFXLayer> layers = new List<BrainrotVFXLayer>();
}

[System.Serializable]
public class BrainrotVFXThemeRecipe {
    public BrainrotVFXTheme theme;
    public List<BrainrotVFXLayer> layers = new List<BrainrotVFXLayer>();
}

[System.Serializable]
public class BrainrotVFXKeywordRecipe {
    [Tooltip("Ten brainrot (chu thuong) chua tu nay thi them cac layer ben duoi")]
    public string keyword;
    public List<BrainrotVFXLayer> layers = new List<BrainrotVFXLayer>();
}

// thu vien VFX: moi rarity 1 bo hieu ung (cang hiem cang nhieu lop), moi theme them hieu ung hop voi model,
// tu khoa ten them hieu ung rieng. chinh sua truc tiep trong Inspector
[CreateAssetMenu(menuName = "Brainrot/VFX Library")]
public class BrainrotVFXLibrarySO : ScriptableObject {
    private static string HOVL = "Assets/Hovl Studio/Magic effects pack/Prefabs/";
    private static string VEFECTS = "Assets/Vefects/Flipbook VFX Bundle Lite URP/Flipbook VFX URP/_ Lite/Prefabs/";
    private static string VEFECTS_COMBAT = "Assets/Vefects/Flipbook VFX Bundle Lite URP/Combat Flipbook VFX URP/_ Lite/Particles/";
    private static string GALAXY = "Assets/Vefects/Free Galaxy Shader URP/Galaxy/Materials/";
    private static string ETFX = "Assets/Epic Toon FX/Prefabs/";
    private static string SCIFI = "Assets/Archanor/Sci-Fi Arsenal/Sci-Fi Effects/Prefabs/";
    private static string VOXEL = "Assets/Voxel Arsenal/Prefabs/";
    private static string CFX = "Assets/JMO Assets/Cartoon FX (legacy)/CFX Prefabs/";
    private static string CFXR = "Assets/JMO Assets/Cartoon FX Remaster/CFXR Prefabs/";
    private static string FLAME = "Assets/Flame sprite effects/Prefabs/";

    [SerializeField] private List<BrainrotVFXRarityRecipe> rarityRecipes = new List<BrainrotVFXRarityRecipe>();
    [SerializeField] private List<BrainrotVFXThemeRecipe> themeRecipes = new List<BrainrotVFXThemeRecipe>();
    [SerializeField] private List<BrainrotVFXKeywordRecipe> keywordRecipes = new List<BrainrotVFXKeywordRecipe>();

    // bo cua rarity + theme (loc theo minRarity) + tu khoa dau tien khop voi ten
    public List<BrainrotVFXLayer> GetLayers(BrainrotRarity rarity, BrainrotVFXTheme theme, string brainrotName) {
        List<BrainrotVFXLayer> layers = new List<BrainrotVFXLayer>();

        foreach (BrainrotVFXRarityRecipe recipe in rarityRecipes) {
            if (recipe.rarity == rarity) {
                layers.AddRange(recipe.layers);
            }
        }

        foreach (BrainrotVFXThemeRecipe recipe in themeRecipes) {
            if (recipe.theme == theme) {
                AddLayersByRarity(layers, recipe.layers, rarity);
            }
        }

        string lowerName = brainrotName.ToLowerInvariant();
        foreach (BrainrotVFXKeywordRecipe recipe in keywordRecipes) {
            if (string.IsNullOrEmpty(recipe.keyword) || !lowerName.Contains(recipe.keyword.ToLowerInvariant())) continue;

            AddLayersByRarity(layers, recipe.layers, rarity);
            break;
        }

        return layers;
    }

    private void AddLayersByRarity(List<BrainrotVFXLayer> layers, List<BrainrotVFXLayer> recipeLayers, BrainrotRarity rarity) {
        foreach (BrainrotVFXLayer layer in recipeLayers) {
            if ((int)rarity < (int)layer.minRarity) continue;

            layers.Add(layer);
        }
    }

    // ===== thu vien mac dinh: magic, aura, galaxy, set, tia sang =====

    public void ResetToDefault() {
        rarityRecipes = new List<BrainrotVFXRarityRecipe>();
        themeRecipes = new List<BrainrotVFXThemeRecipe>();
        keywordRecipes = new List<BrainrotVFXKeywordRecipe>();

        CreateDefaultRarityRecipes();
        CreateDefaultThemeRecipes();
    }

    private void CreateDefaultRarityRecipes() {
        AddRarity(BrainrotRarity.Basic,
            Layer("Magic Sparks", HovlSparks(), BrainrotVFXAnchor.Body, BrainrotVFXFit.Inside, 1.2f, emission: 0.7f));

        AddRarity(BrainrotRarity.Rare,
            Layer("Magic Field", Family(ETFX + "Combat/Magic/Field", "MagicField"), BrainrotVFXAnchor.Ground, BrainrotVFXFit.Width, 1.6f),
            Layer("Magic Sparks", HovlSparks(), BrainrotVFXAnchor.Body, BrainrotVFXFit.Inside, 1.3f));

        AddRarity(BrainrotRarity.Epic,
            Layer("Magic Circle", Single(HOVL + "Magic circles/Magic circle 2"), BrainrotVFXAnchor.Ground, BrainrotVFXFit.Width, 1.8f, BrainrotVFXColorSource.Fixed),
            Layer("Swirl Aura", Family(VOXEL + "Combat/Aura/SwirlAura", "SwirlAura"), BrainrotVFXAnchor.Ground, BrainrotVFXFit.Inside, 1.3f),
            Layer("Flashing Sparks", HovlFlashingSparks(), BrainrotVFXAnchor.Body, BrainrotVFXFit.Inside, 1.4f, colorHint: "Pink"));

        AddRarity(BrainrotRarity.Legendary,
            Layer("Magic Circle", Single(HOVL + "Magic circles/Magic circle"), BrainrotVFXAnchor.Ground, BrainrotVFXFit.Width, 2f, BrainrotVFXColorSource.Fixed),
            Layer("Buff Aura", Single(HOVL + "Character auras/Buff"), BrainrotVFXAnchor.Ground, BrainrotVFXFit.Inside, 1.4f, BrainrotVFXColorSource.Fixed),
            Layer("Loot Beam", Family(SCIFI + "Interactive/Loot/LootRays", "LootItem"), BrainrotVFXAnchor.PivotGround, BrainrotVFXFit.Width, 1f),
            Layer("Item Sparkle", Family(ETFX + "Interactive/Loot/ItemSparkle", "ItemSparkle"), BrainrotVFXAnchor.Body, BrainrotVFXFit.Inside, 1.5f));

        AddRarity(BrainrotRarity.Mythic,
            Layer("Hexagon Zone", Family(SCIFI + "Interactive/Zones/HexagonZone", "HexagonZone"), BrainrotVFXAnchor.Ground, BrainrotVFXFit.Width, 2.1f),
            Layer("Spiky Aura", Single(CFX + "Misc/CFX_SpikyAura_Character Looped"), BrainrotVFXAnchor.Ground, BrainrotVFXFit.Inside, 1.5f, BrainrotVFXColorSource.Fixed),
            Layer("Swirly Aura", Family(SCIFI + "Combat/Aura/Swirly", "SwirlyAura"), BrainrotVFXAnchor.Ground, BrainrotVFXFit.Inside, 1.4f),
            Layer("Energy Charge", Single(VEFECTS_COMBAT + "Lite_VFX_Radial_Charge_Skill_01_GS_Random_Rotate_Bunch_01_Loop"), BrainrotVFXAnchor.Body, BrainrotVFXFit.Inside, 1.4f, BrainrotVFXColorSource.Fixed),
            Layer("Loot Beam", Family(SCIFI + "Interactive/Loot/LootRays", "LootItem"), BrainrotVFXAnchor.PivotGround, BrainrotVFXFit.Width, 1.1f),
            Layer("Flashing Sparks", HovlFlashingSparks(), BrainrotVFXAnchor.Body, BrainrotVFXFit.Inside, 1.5f));

        AddRarity(BrainrotRarity.Secret,
            Layer("Dark Circle", Family(SCIFI + "Interactive/Zones/RoundZone", "RoundZone"), BrainrotVFXAnchor.Ground, BrainrotVFXFit.Width, 2.1f, colorHint: "Purple"),
            Layer("Debuff Aura", Single(HOVL + "Character auras/Debuff"), BrainrotVFXAnchor.Ground, BrainrotVFXFit.Inside, 1.5f, BrainrotVFXColorSource.Fixed),
            Layer("Black Flame Aura", Single(CFX + "Fire/Flames Looped/Color Variants/CFX_DoubleFlameC Black Looped"), BrainrotVFXAnchor.Ground, BrainrotVFXFit.Width, 1.4f, BrainrotVFXColorSource.Fixed),
            Layer("Plexus Aura", Single(HOVL + "Character auras/Plexus"), BrainrotVFXAnchor.Body, BrainrotVFXFit.Inside, 1.6f, BrainrotVFXColorSource.Fixed),
            Layer("Dark Lightning", Single(VEFECTS + "Lite_VFX_Lightning_08"), BrainrotVFXAnchor.Body, BrainrotVFXFit.Inside, 1.5f, BrainrotVFXColorSource.Fixed),
            Layer("God Rays", Family(VOXEL + "Environment/Godrays", "Godrays"), BrainrotVFXAnchor.PivotGround, BrainrotVFXFit.Width, 1.4f),
            Layer("Flashing Sparks", HovlFlashingSparks(), BrainrotVFXAnchor.Body, BrainrotVFXFit.Inside, 1.6f));

        AddRarity(BrainrotRarity.Celestial,
            MeshLayer("Galaxy Pedestal", LoadMaterial(GALAXY + "M_VFX_URP_Stylized_Galaxy_01"), BrainrotVFXMeshShape.Disc, 1.5f),
            Layer("Star Vortex", Family(ETFX + "Environment/Stars/StarVortex", "StarVortex"), BrainrotVFXAnchor.Ground, BrainrotVFXFit.Width, 2.4f),
            Layer("Star Aura", Single(HOVL + "Character auras/Star aura"), BrainrotVFXAnchor.Ground, BrainrotVFXFit.Inside, 1.5f, BrainrotVFXColorSource.Fixed),
            Layer("Star Fog", Family(ETFX + "Environment/Stars/StarFog", "StarFog"), BrainrotVFXAnchor.Body, BrainrotVFXFit.Inside, 1.9f),
            Layer("Fairies", Family(VOXEL + "Environment/Fairies", "Fairies"), BrainrotVFXAnchor.Body, BrainrotVFXFit.Inside, 1.9f, colorHint: "Pink"),
            MeshLayer("Galaxy Planet", LoadMaterial(GALAXY + "M_VFX_URP_Stylized_Galaxy_02"), BrainrotVFXMeshShape.Orb, 0.35f));

        AddRarity(BrainrotRarity.Divine,
            Layer("Magic Circle", Single(HOVL + "Magic circles/Magic circle"), BrainrotVFXAnchor.Ground, BrainrotVFXFit.Width, 2.3f, BrainrotVFXColorSource.Fixed),
            Layer("Holy Beam", Single(HOVL + "Environment/Teleport"), BrainrotVFXAnchor.PivotGround, BrainrotVFXFit.Width, 1.3f, BrainrotVFXColorSource.Fixed),
            Layer("Buff Aura", Single(HOVL + "Character auras/Buff"), BrainrotVFXAnchor.Ground, BrainrotVFXFit.Inside, 1.5f, BrainrotVFXColorSource.Fixed),
            Layer("Charge Aura", Family(VOXEL + "Combat/Aura/ChargeAura", "ChargeAura"), BrainrotVFXAnchor.Ground, BrainrotVFXFit.Inside, 1.4f),
            Layer("God Rays", Family(VOXEL + "Environment/Godrays", "Godrays"), BrainrotVFXAnchor.PivotGround, BrainrotVFXFit.Width, 1.6f),
            Layer("Item Sparkle", Family(ETFX + "Interactive/Loot/ItemSparkle", "ItemSparkle"), BrainrotVFXAnchor.Body, BrainrotVFXFit.Inside, 1.6f));

        AddRarity(BrainrotRarity.Infinity,
            MeshLayer("Galaxy Pedestal", LoadMaterial(GALAXY + "M_VFX_URP_Stylized_Galaxy_03"), BrainrotVFXMeshShape.Disc, 1.7f),
            Layer("Star Vortex", Family(ETFX + "Environment/Stars/StarVortex", "StarVortex"), BrainrotVFXAnchor.Ground, BrainrotVFXFit.Width, 2.6f, colorHint: "Blue"),
            Layer("Holy Beam", Single(HOVL + "Environment/Teleport"), BrainrotVFXAnchor.PivotGround, BrainrotVFXFit.Width, 1.4f, BrainrotVFXColorSource.Fixed),
            Layer("Star Aura", Single(HOVL + "Character auras/Star aura"), BrainrotVFXAnchor.Ground, BrainrotVFXFit.Inside, 1.6f, BrainrotVFXColorSource.Fixed),
            Layer("Lightning Aura", Single(HOVL + "Character auras/Lightning aura"), BrainrotVFXAnchor.Ground, BrainrotVFXFit.Inside, 1.4f, BrainrotVFXColorSource.Fixed),
            Layer("Spin Aura", Family(SCIFI + "Combat/Aura/Spin", "SpinAura"), BrainrotVFXAnchor.Ground, BrainrotVFXFit.Inside, 1.7f, colorHint: "Purple"),
            Layer("Rainbow Glow", Family(ETFX + "Interactive/Loot/ItemSkewedGlow", "ItemSkewedGlow"), BrainrotVFXAnchor.Body, BrainrotVFXFit.Inside, 2.3f),
            Layer("Rainbow Sparkle", Family(ETFX + "Interactive/Loot/ItemSparkle", "ItemSparkle"), BrainrotVFXAnchor.Body, BrainrotVFXFit.Inside, 1.8f),
            MeshLayer("Galaxy Planet", LoadMaterial(GALAXY + "M_VFX_URP_Stylized_Galaxy_01"), BrainrotVFXMeshShape.Orb, 0.4f));
    }

    // theme them hieu ung nguyen to hop voi model, rarity cang cao cang them lop (minRarity)
    private void CreateDefaultThemeRecipes() {
        AddTheme(BrainrotVFXTheme.Sparkle,
            Layer("Sparks", HovlSparks(), BrainrotVFXAnchor.Body, BrainrotVFXFit.Inside, 1.3f, BrainrotVFXColorSource.Model),
            Layer("Sparkle Area", Family(ETFX + "Interactive/Sparkle/SparkleArea", "SparkleArea"), BrainrotVFXAnchor.Body, BrainrotVFXFit.Inside, 1.5f, BrainrotVFXColorSource.Model, minRarity: BrainrotRarity.Epic));

        AddTheme(BrainrotVFXTheme.Fire,
            Layer("Radial Fire", Family(ETFX + "Environment/Fire/Cartoon/Radial", "ToonRadialFire"), BrainrotVFXAnchor.Ground, BrainrotVFXFit.Width, 1.3f, colorHint: "Red"),
            Layer("Fire Ring", Single(CFXR + "Fire/CFXR Fire Ring"), BrainrotVFXAnchor.Ground, BrainrotVFXFit.Width, 1.7f, BrainrotVFXColorSource.Fixed, minRarity: BrainrotRarity.Rare),
            Layer("Flame Circle", Single(FLAME + "Flame_circle_loop"), BrainrotVFXAnchor.Ground, BrainrotVFXFit.Width, 1.9f, BrainrotVFXColorSource.Fixed, minRarity: BrainrotRarity.Legendary));

        AddTheme(BrainrotVFXTheme.Water,
            Layer("Water Aura", Family(ETFX + "Combat/Magic/Aura Soft", "AuraSoft"), BrainrotVFXAnchor.Ground, BrainrotVFXFit.Inside, 1.3f, colorHint: "Blue"),
            Layer("Blue Sparks", HovlFlashingSparks(), BrainrotVFXAnchor.Body, BrainrotVFXFit.Inside, 1.4f, colorHint: "Blue", minRarity: BrainrotRarity.Rare),
            Layer("Water Shield", Single(HOVL + "Magic shields/Magic shield blue"), BrainrotVFXAnchor.Body, BrainrotVFXFit.Inside, 1.5f, BrainrotVFXColorSource.Fixed, minRarity: BrainrotRarity.Mythic));

        AddTheme(BrainrotVFXTheme.Ice,
            Layer("Freeze Circle", Single(HOVL + "Magic circles/Freeze circle"), BrainrotVFXAnchor.Ground, BrainrotVFXFit.Width, 1.8f, BrainrotVFXColorSource.Fixed),
            Layer("Ice Sparks", HovlFlashingSparks(), BrainrotVFXAnchor.Body, BrainrotVFXFit.Inside, 1.4f, colorHint: "White", minRarity: BrainrotRarity.Rare),
            Layer("Ice Tornado", Single(VOXEL + "Environment/Tornado/TornadoIce"), BrainrotVFXAnchor.Ground, BrainrotVFXFit.Inside, 1.4f, BrainrotVFXColorSource.Fixed, minRarity: BrainrotRarity.Mythic));

        AddTheme(BrainrotVFXTheme.Electric,
            Layer("Lightning Aura", Single(HOVL + "Character auras/Lightning aura"), BrainrotVFXAnchor.Ground, BrainrotVFXFit.Inside, 1.3f, BrainrotVFXColorSource.Fixed),
            Layer("Lightning Bolts", Single(VEFECTS_COMBAT + "Lite_VFX_Lightning_Projectile_02_Billboard_Bunch_Loop_01_CV_02"), BrainrotVFXAnchor.Body, BrainrotVFXFit.Inside, 1.3f, BrainrotVFXColorSource.Fixed, minRarity: BrainrotRarity.Rare),
            Layer("Electrified", Single(CFXR + "Electric/CFXR Electrified 2"), BrainrotVFXAnchor.Body, BrainrotVFXFit.Inside, 1.1f, BrainrotVFXColorSource.Fixed, minRarity: BrainrotRarity.Epic),
            Layer("Lightning Strike", Single(VEFECTS + "Lite_VFX_Lightning_05"), BrainrotVFXAnchor.Body, BrainrotVFXFit.Inside, 1.6f, BrainrotVFXColorSource.Fixed, minRarity: BrainrotRarity.Mythic));

        AddTheme(BrainrotVFXTheme.Nature,
            Layer("Healing Aura", Single(HOVL + "Character auras/Healing"), BrainrotVFXAnchor.Ground, BrainrotVFXFit.Inside, 1.3f, BrainrotVFXColorSource.Fixed),
            Layer("Healing Circle", Single(HOVL + "Magic circles/Healing circle"), BrainrotVFXAnchor.Ground, BrainrotVFXFit.Width, 1.8f, BrainrotVFXColorSource.Fixed, minRarity: BrainrotRarity.Rare),
            Layer("Fireflies", Family(ETFX + "Environment/Fireflies", "FireFlies"), BrainrotVFXAnchor.Body, BrainrotVFXFit.Inside, 1.6f, colorHint: "Green", minRarity: BrainrotRarity.Epic));

        AddTheme(BrainrotVFXTheme.Sweet,
            Layer("Love Aura", Single(HOVL + "Character auras/Love aura"), BrainrotVFXAnchor.Ground, BrainrotVFXFit.Inside, 1.3f, BrainrotVFXColorSource.Fixed),
            Layer("Pink Sparks", HovlFlashingSparks(), BrainrotVFXAnchor.Body, BrainrotVFXFit.Inside, 1.4f, colorHint: "Pink", minRarity: BrainrotRarity.Rare),
            Layer("Pink Zone", Family(ETFX + "Interactive/Zone/MagicZone", "MagicZone"), BrainrotVFXAnchor.Ground, BrainrotVFXFit.Width, 1.9f, colorHint: "Pink", minRarity: BrainrotRarity.Epic));

        AddTheme(BrainrotVFXTheme.Cosmic,
            Layer("Star Aura", Single(HOVL + "Character auras/Star aura"), BrainrotVFXAnchor.Ground, BrainrotVFXFit.Inside, 1.3f, BrainrotVFXColorSource.Fixed),
            Layer("Star Fog", Family(ETFX + "Environment/Stars/StarFog", "StarFog"), BrainrotVFXAnchor.Body, BrainrotVFXFit.Inside, 1.8f, BrainrotVFXColorSource.Model, minRarity: BrainrotRarity.Rare),
            MeshLayer("Galaxy Planet", LoadMaterial(GALAXY + "M_VFX_URP_Stylized_Galaxy_02"), BrainrotVFXMeshShape.Orb, 0.3f, BrainrotRarity.Epic));

        AddTheme(BrainrotVFXTheme.Steam,
            Layer("Warm Aura", Single(HOVL + "Character auras/Buff"), BrainrotVFXAnchor.Ground, BrainrotVFXFit.Inside, 1.3f, BrainrotVFXColorSource.Fixed),
            Layer("Warm Sparks", HovlFlashingSparks(), BrainrotVFXAnchor.Body, BrainrotVFXFit.Inside, 1.4f, colorHint: "Yellow", minRarity: BrainrotRarity.Rare));

        AddTheme(BrainrotVFXTheme.Shadow,
            Layer("Debuff Aura", Single(HOVL + "Character auras/Debuff"), BrainrotVFXAnchor.Ground, BrainrotVFXFit.Inside, 1.3f, BrainrotVFXColorSource.Fixed),
            Layer("Plexus", Single(HOVL + "Character auras/Plexus"), BrainrotVFXAnchor.Body, BrainrotVFXFit.Inside, 1.5f, BrainrotVFXColorSource.Fixed, minRarity: BrainrotRarity.Rare),
            Layer("Black Flame", Single(CFX + "Fire/Flames Looped/Color Variants/CFX_DoubleFlameC Black Looped"), BrainrotVFXAnchor.Ground, BrainrotVFXFit.Width, 1.3f, BrainrotVFXColorSource.Fixed, minRarity: BrainrotRarity.Epic));

        AddTheme(BrainrotVFXTheme.Rhythm,
            Layer("Beat Sparks", HovlFlashingSparks(), BrainrotVFXAnchor.Body, BrainrotVFXFit.Inside, 1.4f, BrainrotVFXColorSource.Model),
            Layer("Musical Notes", Single(ETFX + "Misc/MusicalNotes"), BrainrotVFXAnchor.PivotTop, BrainrotVFXFit.Width, 1f, BrainrotVFXColorSource.Fixed, minRarity: BrainrotRarity.Rare));
    }

    private List<GameObject> HovlSparks() {
        return Family(HOVL + "Sparks", "sparks", "flashing", "explode");
    }

    private List<GameObject> HovlFlashingSparks() {
        return Family(HOVL + "Sparks", "sparks flashing");
    }

    private void AddRarity(BrainrotRarity rarity, params BrainrotVFXLayer[] layers) {
        BrainrotVFXRarityRecipe recipe = new BrainrotVFXRarityRecipe();
        recipe.rarity = rarity;
        recipe.layers.AddRange(layers);
        rarityRecipes.Add(recipe);
    }

    private void AddTheme(BrainrotVFXTheme theme, params BrainrotVFXLayer[] layers) {
        BrainrotVFXThemeRecipe recipe = new BrainrotVFXThemeRecipe();
        recipe.theme = theme;
        recipe.layers.AddRange(layers);
        themeRecipes.Add(recipe);
    }

    private BrainrotVFXLayer Layer(string label, List<GameObject> variants, BrainrotVFXAnchor anchor, BrainrotVFXFit fit, float size,
        BrainrotVFXColorSource colorSource = BrainrotVFXColorSource.Rarity, string colorHint = "", BrainrotRarity minRarity = BrainrotRarity.Basic, float emission = 1f) {
        BrainrotVFXLayer layer = new BrainrotVFXLayer();
        layer.label = label;
        layer.variants = variants;
        layer.anchor = anchor;
        layer.fit = fit;
        layer.size = size;
        layer.colorSource = colorSource;
        layer.colorHint = colorHint;
        layer.minRarity = minRarity;
        layer.emission = emission;

        return layer;
    }

    private BrainrotVFXLayer MeshLayer(string label, Material material, BrainrotVFXMeshShape meshShape, float size, BrainrotRarity minRarity = BrainrotRarity.Basic) {
        BrainrotVFXLayer layer = new BrainrotVFXLayer();
        layer.label = label;
        layer.meshShape = meshShape;
        layer.meshMaterial = material;
        layer.size = size;
        layer.minRarity = minRarity;
        layer.colorSource = BrainrotVFXColorSource.Fixed;

        return layer;
    }

    private Material LoadMaterial(string pathWithoutExtension) {
        Material material = AssetDatabase.LoadAssetAtPath<Material>(pathWithoutExtension + ".mat");
        if (material == null) {
            Debug.LogWarning("VFX material not found: " + pathWithoutExtension);
        }

        return material;
    }

    private List<GameObject> Single(string pathWithoutExtension) {
        List<GameObject> variants = new List<GameObject>();
        GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(pathWithoutExtension + ".prefab");
        if (prefab == null) {
            Debug.LogWarning("VFX prefab not found: " + pathWithoutExtension);
            return variants;
        }

        variants.Add(prefab);
        return variants;
    }

    // cac bien the mau trong 1 folder (khong tim folder con), ten chua token, bo ban "Modular" va cac tu trong excludes
    private List<GameObject> Family(string folder, string token, params string[] excludes) {
        List<GameObject> variants = new List<GameObject>();
        if (!AssetDatabase.IsValidFolder(folder)) {
            Debug.LogWarning("VFX folder not found: " + folder);
            return variants;
        }

        string lowerToken = token.ToLowerInvariant();
        foreach (string guid in AssetDatabase.FindAssets("t:Prefab", new string[] { folder })) {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            if (Path.GetDirectoryName(path).Replace('\\', '/') != folder) continue;

            string prefabName = Path.GetFileNameWithoutExtension(path).ToLowerInvariant();
            if (!prefabName.Contains(lowerToken) || prefabName.Contains("modular") || IsContainAny(prefabName, excludes)) continue;

            variants.Add(AssetDatabase.LoadAssetAtPath<GameObject>(path));
        }

        variants.Sort((a, b) => string.CompareOrdinal(a.name, b.name));
        return variants;
    }

    private bool IsContainAny(string text, string[] words) {
        foreach (string word in words) {
            if (text.Contains(word)) return true;
        }

        return false;
    }
}
