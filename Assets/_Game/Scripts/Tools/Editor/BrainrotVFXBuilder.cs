using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;

public class BrainrotVFXRequest {
    public string vfxName;
    public string brainrotName;
    public BrainrotRarity rarity;
    public BrainrotVFXTheme theme;
    public BrainrotVFXTheme resolvedTheme;
    public Color primaryColor = Color.white;
    public Color secondaryColor = Color.white;
    public Bounds modelBounds;
    public float sizeMultiplier = 1f;
    public float rateMultiplier = 1f;
}

// ghep VFX cho 1 brainrot tu cac prefab trong pack (Epic Toon FX, Sci-Fi Arsenal, Voxel Arsenal, Cartoon FX...)
// moi layer: chon bien the mau -> copy prefab (unpack) -> bo Light / script thua -> chuyen material sang URP
// -> mo phong thu de do kich thuoc that -> scale cho vua model -> dat vao vi tri (chan, than, dinh dau...)
// layer mesh (galaxy): tao de tron duoi chan hoac qua cau tren dau dung material cua layer
public class BrainrotVFXBuilder {
    private static float MEASURE_TIME = 3f;
    private static float MEASURE_PERCENTILE = 0.03f;
    private static float MIN_SCALE = 0.02f;
    private static float MAX_SCALE = 50f;

    private BrainrotVFXRequest request;
    private Transform root;
    private float modelWidth;
    private float modelHeight;
    private Vector3 groundCenter;
    private float emissionScale;

    public static GameObject Build(BrainrotVFXRequest request, BrainrotVFXLibrarySO library) {
        BrainrotVFXBuilder builder = new BrainrotVFXBuilder(request);
        return builder.BuildVFX(library);
    }

    private BrainrotVFXBuilder(BrainrotVFXRequest request) {
        this.request = request;

        Bounds bounds = request.modelBounds;
        modelWidth = Mathf.Max(Mathf.Max(bounds.size.x, bounds.size.z), 0.1f) * request.sizeMultiplier;
        modelHeight = Mathf.Max(bounds.size.y, 0.1f);
        groundCenter = new Vector3(bounds.center.x, bounds.min.y, bounds.center.z);

        // rarity thap thi hat thua hon, rarity cao thi day hon
        emissionScale = Mathf.Lerp(0.7f, 1.1f, (float)request.rarity / (float)BrainrotRarity.Infinity) * request.rateMultiplier;
    }

    private GameObject BuildVFX(BrainrotVFXLibrarySO library) {
        GameObject vfx = new GameObject(request.vfxName);
        root = vfx.transform;

        foreach (BrainrotVFXLayer layer in library.GetLayers(request.rarity, request.resolvedTheme, request.brainrotName)) {
            if (layer.meshShape != BrainrotVFXMeshShape.None) {
                AddMeshLayer(layer);
            }
            else {
                AddLayer(layer);
            }
        }

        vfx.AddComponent<BrainrotVFX>().Setup(request.theme, request.rarity, request.primaryColor, request.secondaryColor);
        return vfx;
    }

    private void AddLayer(BrainrotVFXLayer layer) {
        GameObject prefab = PickVariant(layer);
        if (prefab == null) {
            Debug.LogWarning("VFX layer '" + layer.label + "' has no prefab, skipped (" + request.vfxName + ")");
            return;
        }

        GameObject effect = (GameObject)PrefabUtility.InstantiatePrefab(prefab);
        PrefabUtility.UnpackPrefabInstance(effect, PrefabUnpackMode.Completely, InteractionMode.AutomatedAction);
        effect.name = layer.label + " (" + prefab.name + ")";
        effect.transform.SetParent(root, false);
        effect.transform.localPosition = Vector3.zero;

        CleanEffect(effect);
        BrainrotVFXMaterialUpgrader.UpgradeEffect(effect);
        PrepareParticles(effect, layer.emission * emissionScale);

        Bounds effectBounds = MeasureEffect(effect);
        float scale = GetFitScale(layer, effectBounds);
        effect.transform.localScale *= scale;

        // effect dang o goc toa do nen scale quanh goc: bounds moi = bounds cu * scale
        Bounds scaledBounds = new Bounds(effectBounds.center * scale, effectBounds.size * scale);
        effect.transform.localPosition = GetAnchorPosition(layer, scaledBounds);
    }

    // de galaxy: tru det, mat tren sat chan model | hanh tinh galaxy: qua cau lo lung tren dau
    private void AddMeshLayer(BrainrotVFXLayer layer) {
        if (layer.meshMaterial == null) {
            Debug.LogWarning("VFX mesh layer '" + layer.label + "' has no material, skipped (" + request.vfxName + ")");
            return;
        }

        bool isDisc = layer.meshShape == BrainrotVFXMeshShape.Disc;
        GameObject meshObject = GameObject.CreatePrimitive(isDisc ? PrimitiveType.Cylinder : PrimitiveType.Sphere);
        Object.DestroyImmediate(meshObject.GetComponent<Collider>());
        meshObject.name = layer.label;
        meshObject.transform.SetParent(root, false);

        MeshRenderer meshRenderer = meshObject.GetComponent<MeshRenderer>();
        meshRenderer.sharedMaterial = layer.meshMaterial;
        meshRenderer.shadowCastingMode = ShadowCastingMode.Off;
        meshRenderer.receiveShadows = false;

        float diameter = modelWidth * layer.size;
        Vector3 offset = Vector3.up * modelHeight * layer.heightOffset;

        if (isDisc) {
            // cylinder mac dinh cao 2 don vi, scale y = nua do day
            float thickness = modelHeight * 0.03f;
            meshObject.transform.localScale = new Vector3(diameter, thickness * 0.5f, diameter);
            meshObject.transform.localPosition = groundCenter + Vector3.up * (thickness * 0.5f - modelHeight * 0.02f) + offset;
        }
        else {
            meshObject.transform.localScale = Vector3.one * diameter;
            meshObject.transform.localPosition = groundCenter + Vector3.up * (modelHeight + diameter * 0.9f) + offset;
        }
    }

    // ===== chon mau =====

    private GameObject PickVariant(BrainrotVFXLayer layer) {
        List<GameObject> variants = layer.variants.FindAll(variant => variant != null);
        if (variants.Count == 0) return null;

        if (!string.IsNullOrEmpty(layer.colorHint)) {
            GameObject hintVariant = BrainrotVFXColors.FindVariantByColorWord(variants, layer.colorHint);
            if (hintVariant != null) return hintVariant;
        }

        switch (layer.colorSource) {
            case BrainrotVFXColorSource.Rarity:
                return BrainrotVFXColors.PickVariant(variants, BrainrotVFXColors.GetRarityColor(request.rarity), BrainrotVFXColors.IsRainbow(request.rarity));
            case BrainrotVFXColorSource.Model:
                return BrainrotVFXColors.PickVariant(variants, request.primaryColor, false);
            default:
                return variants[0];
        }
    }

    // ===== don dep prefab cua pack =====

    // giu lai script xoay (ETFXRotation, SciFiRotation, VoxelRotation...), bo script tu huy / light / camera shake
    // bo Light vi moi brainrot 1 den realtime se rat nang khi base co nhieu con
    private void CleanEffect(GameObject effect) {
        foreach (MonoBehaviour behaviour in effect.GetComponentsInChildren<MonoBehaviour>(true)) {
            if (behaviour == null || behaviour.GetType().Name.Contains("Rotat")) continue;

            Object.DestroyImmediate(behaviour);
        }

        foreach (Transform child in effect.GetComponentsInChildren<Transform>(true)) {
            GameObjectUtility.RemoveMonoBehavioursWithMissingScript(child.gameObject);
        }

        foreach (Light light in effect.GetComponentsInChildren<Light>(true)) {
            Object.DestroyImmediate(light);
        }

        foreach (AudioSource audioSource in effect.GetComponentsInChildren<AudioSource>(true)) {
            Object.DestroyImmediate(audioSource);
        }

        foreach (Renderer renderer in effect.GetComponentsInChildren<Renderer>(true)) {
            renderer.shadowCastingMode = ShadowCastingMode.Off;
            renderer.receiveShadows = false;
        }
    }

    // Hierarchy de scale theo transform (root model scale 100, VFX bu 1/100), prewarm de vua hien ra da day du
    private void PrepareParticles(GameObject effect, float emissionMultiplier) {
        foreach (ParticleSystem particle in effect.GetComponentsInChildren<ParticleSystem>(true)) {
            ParticleSystem.MainModule main = particle.main;
            main.scalingMode = ParticleSystemScalingMode.Hierarchy;
            if (main.loop) {
                main.prewarm = true;
            }

            ParticleSystem.EmissionModule emission = particle.emission;
            emission.rateOverTimeMultiplier *= emissionMultiplier;
            emission.rateOverDistanceMultiplier *= emissionMultiplier;
            ScaleBursts(emission, emissionMultiplier);

            if (emissionMultiplier > 1f) {
                main.maxParticles = Mathf.CeilToInt(main.maxParticles * emissionMultiplier);
            }

            ParticleSystem.LightsModule lights = particle.lights;
            lights.enabled = false;

            // va cham voi the gioi ton raycast, bo di
            ParticleSystem.CollisionModule collision = particle.collision;
            if (collision.enabled && collision.type == ParticleSystemCollisionType.World) {
                collision.enabled = false;
            }
        }
    }

    private void ScaleBursts(ParticleSystem.EmissionModule emission, float multiplier) {
        if (emission.burstCount == 0) return;

        ParticleSystem.Burst[] bursts = new ParticleSystem.Burst[emission.burstCount];
        emission.GetBursts(bursts);

        for (int i = 0; i < bursts.Length; i++) {
            bursts[i].minCount = (short)Mathf.Max(1, Mathf.RoundToInt(bursts[i].minCount * multiplier));
            bursts[i].maxCount = (short)Mathf.Max(1, Mathf.RoundToInt(bursts[i].maxCount * multiplier));
        }

        emission.SetBursts(bursts);
    }

    // ===== do kich thuoc =====

    // cho chay thu MEASURE_TIME giay roi lay vi tri + kich thuoc tung hat
    // bo 3% hat xa nhat moi phia de vai hat bay lac khong lam hieu ung bi thu nho qua muc
    private Bounds MeasureEffect(GameObject effect) {
        List<float> xs = new List<float>();
        List<float> ys = new List<float>();
        List<float> zs = new List<float>();

        ParticleSystem[] particleSystems = effect.GetComponentsInChildren<ParticleSystem>(true);
        foreach (ParticleSystem particleSystem in particleSystems) {
            particleSystem.Simulate(MEASURE_TIME, false, true, true);
            CollectParticlePoints(particleSystem, xs, ys, zs);
        }

        foreach (Renderer renderer in effect.GetComponentsInChildren<Renderer>(true)) {
            if (renderer is ParticleSystemRenderer) continue;

            Bounds rendererBounds = renderer.bounds;
            AddPoint(xs, ys, zs, rendererBounds.min, 0f);
            AddPoint(xs, ys, zs, rendererBounds.max, 0f);
        }

        foreach (ParticleSystem particleSystem in particleSystems) {
            particleSystem.Stop(false, ParticleSystemStopBehavior.StopEmittingAndClear);
        }

        if (xs.Count == 0) {
            Debug.LogWarning("Could not measure VFX '" + effect.name + "', using default size");
            return new Bounds(Vector3.up * 0.5f, Vector3.one);
        }

        Vector3 min = new Vector3(GetPercentile(xs, MEASURE_PERCENTILE), GetPercentile(ys, MEASURE_PERCENTILE), GetPercentile(zs, MEASURE_PERCENTILE));
        Vector3 max = new Vector3(GetPercentile(xs, 1f - MEASURE_PERCENTILE), GetPercentile(ys, 1f - MEASURE_PERCENTILE), GetPercentile(zs, 1f - MEASURE_PERCENTILE));

        Bounds bounds = new Bounds();
        bounds.SetMinMax(min, max);
        return bounds;
    }

    // root VFX dang o goc toa do the gioi nen toa do the gioi = toa do trong VFX
    private void CollectParticlePoints(ParticleSystem particleSystem, List<float> xs, List<float> ys, List<float> zs) {
        ParticleSystem.MainModule main = particleSystem.main;
        ParticleSystem.Particle[] particles = new ParticleSystem.Particle[Mathf.Max(1, particleSystem.particleCount)];
        int count = particleSystem.GetParticles(particles);

        Transform simulationTransform = particleSystem.transform;
        if (main.simulationSpace == ParticleSystemSimulationSpace.Custom && main.customSimulationSpace != null) {
            simulationTransform = main.customSimulationSpace;
        }

        Vector3 lossyScale = particleSystem.transform.lossyScale;
        float sizeScale = Mathf.Max(Mathf.Abs(lossyScale.x), Mathf.Abs(lossyScale.y), Mathf.Abs(lossyScale.z));

        for (int i = 0; i < count; i++) {
            Vector3 position = particles[i].position;
            if (main.simulationSpace != ParticleSystemSimulationSpace.World) {
                position = simulationTransform.TransformPoint(position);
            }

            float radius = particles[i].GetCurrentSize(particleSystem) * sizeScale * 0.5f;
            AddPoint(xs, ys, zs, position, radius);
        }
    }

    private void AddPoint(List<float> xs, List<float> ys, List<float> zs, Vector3 point, float radius) {
        xs.Add(point.x - radius);
        xs.Add(point.x + radius);
        ys.Add(point.y - radius);
        ys.Add(point.y + radius);
        zs.Add(point.z - radius);
        zs.Add(point.z + radius);
    }

    private float GetPercentile(List<float> values, float percentile) {
        values.Sort();
        int index = Mathf.Clamp(Mathf.RoundToInt((values.Count - 1) * percentile), 0, values.Count - 1);
        return values[index];
    }

    // ===== scale + vi tri =====

    private float GetFitScale(BrainrotVFXLayer layer, Bounds effectBounds) {
        float effectWidth = Mathf.Max(effectBounds.size.x, effectBounds.size.z);
        float effectHeight = effectBounds.size.y;
        float widthScale = effectWidth > 0.001f ? modelWidth * layer.size / effectWidth : 1f;
        float heightScale = effectHeight > 0.001f ? modelHeight * layer.size / effectHeight : widthScale;

        float scale;
        switch (layer.fit) {
            case BrainrotVFXFit.Height:
                scale = heightScale;
                break;
            case BrainrotVFXFit.Inside:
                scale = Mathf.Min(widthScale, heightScale);
                break;
            default:
                scale = widthScale;
                break;
        }

        return Mathf.Clamp(scale, MIN_SCALE, MAX_SCALE);
    }

    private Vector3 GetAnchorPosition(BrainrotVFXLayer layer, Bounds effectBounds) {
        Vector3 modelTop = groundCenter + Vector3.up * modelHeight;
        Vector3 bodyCenter = groundCenter + Vector3.up * modelHeight * 0.5f;
        Vector3 bottomCenter = new Vector3(effectBounds.center.x, effectBounds.min.y, effectBounds.center.z);
        Vector3 offset = Vector3.up * modelHeight * layer.heightOffset;

        switch (layer.anchor) {
            case BrainrotVFXAnchor.Ground:
                return groundCenter + Vector3.up * modelHeight * 0.01f - bottomCenter + offset;
            case BrainrotVFXAnchor.Body:
                return bodyCenter - effectBounds.center + offset;
            case BrainrotVFXAnchor.Top:
                return modelTop - bottomCenter + offset;
            case BrainrotVFXAnchor.PivotTop:
                return modelTop + offset;
            case BrainrotVFXAnchor.PivotAbove:
                return modelTop + Vector3.up * modelHeight * 0.6f + offset;
            default:
                return groundCenter + offset;
        }
    }
}

// chon bien the mau cua prefab dua tren ten (Blue, Red, Gold, Rainbow...)
public static class BrainrotVFXColors {
    private static string[] COLOR_WORDS = new string[] { "rainbow", "gold", "pink", "purple", "orange", "yellow", "green", "blue", "white", "black", "grey", "gray", "red" };

    public static Color GetRarityColor(BrainrotRarity rarity) {
        switch (rarity) {
            case BrainrotRarity.Rare:
                return new Color(0.2f, 0.55f, 1f);
            case BrainrotRarity.Epic:
                return new Color(0.65f, 0.25f, 1f);
            case BrainrotRarity.Legendary:
                return new Color(1f, 0.75f, 0.1f);
            case BrainrotRarity.Mythic:
                return new Color(1f, 0.12f, 0.2f);
            case BrainrotRarity.Secret:
                return Color.white;
            case BrainrotRarity.Celestial:
                return new Color(0.4f, 0.9f, 1f);
            case BrainrotRarity.Divine:
                return new Color(1f, 0.85f, 0.35f);
            case BrainrotRarity.Infinity:
                return new Color(1f, 0.3f, 0.9f);
            default:
                return new Color(0.9f, 0.9f, 0.9f);
        }
    }

    public static bool IsRainbow(BrainrotRarity rarity) {
        return rarity == BrainrotRarity.Infinity;
    }

    public static GameObject FindVariantByColorWord(List<GameObject> variants, string colorWord) {
        string lowerWord = colorWord.ToLowerInvariant();
        foreach (GameObject variant in variants) {
            if (GetColorWord(variant.name) == lowerWord) return variant;
        }

        return null;
    }

    // rainbow thi uu tien ban Rainbow, khong co thi lay mau gan nhat voi mau hong tim
    public static GameObject PickVariant(List<GameObject> variants, Color targetColor, bool isRainbow) {
        if (isRainbow) {
            GameObject rainbowVariant = FindVariantByColorWord(variants, "rainbow");
            if (rainbowVariant != null) return rainbowVariant;
        }

        GameObject bestVariant = variants[0];
        float bestDistance = float.MaxValue;

        foreach (GameObject variant in variants) {
            string colorWord = GetColorWord(variant.name);
            float distance = colorWord == "rainbow" ? 0.8f : GetColorDistance(targetColor, GetWordColor(colorWord));

            if (distance < bestDistance) {
                bestDistance = distance;
                bestVariant = variant;
            }
        }

        return bestVariant;
    }

    // "red" de cuoi vi hay nam trong tu khac, ten khong co mau thi coi la trang
    private static string GetColorWord(string prefabName) {
        string lowerName = prefabName.ToLowerInvariant();
        foreach (string colorWord in COLOR_WORDS) {
            if (lowerName.Contains(colorWord)) return colorWord;
        }

        return "white";
    }

    private static Color GetWordColor(string colorWord) {
        switch (colorWord) {
            case "red":
                return new Color(1f, 0.15f, 0.15f);
            case "orange":
                return new Color(1f, 0.5f, 0.1f);
            case "gold":
                return new Color(1f, 0.75f, 0.15f);
            case "yellow":
                return new Color(1f, 0.95f, 0.2f);
            case "green":
                return new Color(0.25f, 0.9f, 0.3f);
            case "blue":
                return new Color(0.2f, 0.5f, 1f);
            case "purple":
                return new Color(0.6f, 0.25f, 1f);
            case "pink":
                return new Color(1f, 0.4f, 0.8f);
            case "black":
                return new Color(0.05f, 0.05f, 0.05f);
            case "grey":
            case "gray":
                return new Color(0.5f, 0.5f, 0.5f);
            default:
                return Color.white;
        }
    }

    // lech hue chi tinh khi ca 2 mau deu tuoi, mau trung tinh (trang / den) so theo do bao hoa + do sang
    private static float GetColorDistance(Color a, Color b) {
        Color.RGBToHSV(a, out float hueA, out float saturationA, out float valueA);
        Color.RGBToHSV(b, out float hueB, out float saturationB, out float valueB);

        float hueDistance = Mathf.Abs(hueA - hueB);
        hueDistance = Mathf.Min(hueDistance, 1f - hueDistance);

        return hueDistance * 2f * Mathf.Min(saturationA, saturationB) + Mathf.Abs(saturationA - saturationB) * 0.8f + Mathf.Abs(valueA - valueB) * 0.6f;
    }
}
