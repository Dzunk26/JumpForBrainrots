using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BrainrotVFXAnalysis {
    public Bounds bounds;
    public Color primaryColor = Color.white;
    public Color secondaryColor = Color.white;
    public Color rawPrimaryColor = Color.white;
    public bool isNeutral;
}

// phan tich model brainrot: bounds (tinh trong khong gian root, bo qua scale root) va 2 mau chu dao
// mau lay tu texture tai uv cua tung tam giac, trong so = dien tich nen phan nao to tren model thi chiem nhieu
public static class BrainrotVFXAnalyzer {
    private struct ColorSample {
        public Color color;
        public float weight;

        public ColorSample(Color color, float weight) {
            this.color = color;
            this.weight = weight;
        }
    }

    private struct ThemeKeyword {
        public string keyword;
        public BrainrotVFXTheme theme;

        public ThemeKeyword(string keyword, BrainrotVFXTheme theme) {
            this.keyword = keyword;
            this.theme = theme;
        }
    }

    private static int TEXTURE_SAMPLE_SIZE = 128;
    private static int MAX_TRIANGLE_SAMPLES = 3000;
    private static int FALLBACK_GRID_SIZE = 24;
    private static int HUE_BINS = 12;

    // xet theo thu tu, khop cai nao truoc thi lay cai do: ten rieng / dac biet dat truoc, tu chung dat sau
    // keyword >= 5 ky tu thi tim trong ca ten, ngan hon thi chi khop dau 1 tu (tranh "ice" khop nham giua tu)
    private static ThemeKeyword[] THEME_KEYWORDS = new ThemeKeyword[] {
        new ThemeKeyword("hotspot", BrainrotVFXTheme.Electric),
        new ThemeKeyword("wifi", BrainrotVFXTheme.Electric),
        new ThemeKeyword("odin", BrainrotVFXTheme.Electric),
        new ThemeKeyword("thunder", BrainrotVFXTheme.Electric),
        new ThemeKeyword("electr", BrainrotVFXTheme.Electric),
        new ThemeKeyword("volt", BrainrotVFXTheme.Electric),

        new ThemeKeyword("assassin", BrainrotVFXTheme.Shadow),
        new ThemeKeyword("ninja", BrainrotVFXTheme.Shadow),
        new ThemeKeyword("shadow", BrainrotVFXTheme.Shadow),
        new ThemeKeyword("dark", BrainrotVFXTheme.Shadow),
        new ThemeKeyword("job job", BrainrotVFXTheme.Shadow),
        new ThemeKeyword("gangster", BrainrotVFXTheme.Shadow),

        new ThemeKeyword("saturn", BrainrotVFXTheme.Cosmic),
        new ThemeKeyword("celest", BrainrotVFXTheme.Cosmic),
        new ThemeKeyword("galax", BrainrotVFXTheme.Cosmic),
        new ThemeKeyword("cosmo", BrainrotVFXTheme.Cosmic),
        new ThemeKeyword("space", BrainrotVFXTheme.Cosmic),
        new ThemeKeyword("moon", BrainrotVFXTheme.Cosmic),
        new ThemeKeyword("star", BrainrotVFXTheme.Cosmic),
        new ThemeKeyword("illuminat", BrainrotVFXTheme.Cosmic),
        new ThemeKeyword("triangol", BrainrotVFXTheme.Cosmic),
        new ThemeKeyword("combinasion", BrainrotVFXTheme.Cosmic),

        new ThemeKeyword("dragonfrut", BrainrotVFXTheme.Sweet),

        new ThemeKeyword("tralal", BrainrotVFXTheme.Water),
        new ThemeKeyword("orca", BrainrotVFXTheme.Water),
        new ThemeKeyword("dolphin", BrainrotVFXTheme.Water),
        new ThemeKeyword("shark", BrainrotVFXTheme.Water),
        new ThemeKeyword("squal", BrainrotVFXTheme.Water),
        new ThemeKeyword("octopus", BrainrotVFXTheme.Water),
        new ThemeKeyword("crab", BrainrotVFXTheme.Water),
        new ThemeKeyword("fish", BrainrotVFXTheme.Water),
        new ThemeKeyword("acqua", BrainrotVFXTheme.Water),
        new ThemeKeyword("aqua", BrainrotVFXTheme.Water),
        new ThemeKeyword("soda", BrainrotVFXTheme.Water),
        new ThemeKeyword("bubble", BrainrotVFXTheme.Water),
        new ThemeKeyword("burbal", BrainrotVFXTheme.Water),

        new ThemeKeyword("frigo", BrainrotVFXTheme.Ice),
        new ThemeKeyword("snow", BrainrotVFXTheme.Ice),
        new ThemeKeyword("frost", BrainrotVFXTheme.Ice),
        new ThemeKeyword("froz", BrainrotVFXTheme.Ice),
        new ThemeKeyword("ice", BrainrotVFXTheme.Ice),
        new ThemeKeyword("gelat", BrainrotVFXTheme.Ice),
        new ThemeKeyword("pengu", BrainrotVFXTheme.Ice),

        new ThemeKeyword("dragon", BrainrotVFXTheme.Fire),
        new ThemeKeyword("fire", BrainrotVFXTheme.Fire),
        new ThemeKeyword("flame", BrainrotVFXTheme.Fire),
        new ThemeKeyword("fuoco", BrainrotVFXTheme.Fire),
        new ThemeKeyword("chili", BrainrotVFXTheme.Fire),
        new ThemeKeyword("lava", BrainrotVFXTheme.Fire),
        new ThemeKeyword("magma", BrainrotVFXTheme.Fire),
        new ThemeKeyword("bomb", BrainrotVFXTheme.Fire),
        new ThemeKeyword("toaster", BrainrotVFXTheme.Fire),
        new ThemeKeyword("hot", BrainrotVFXTheme.Fire),
        new ThemeKeyword("burn", BrainrotVFXTheme.Fire),

        new ThemeKeyword("ballerin", BrainrotVFXTheme.Sweet),

        new ThemeKeyword("cappuccin", BrainrotVFXTheme.Steam),
        new ThemeKeyword("espresso", BrainrotVFXTheme.Steam),
        new ThemeKeyword("coffee", BrainrotVFXTheme.Steam),
        new ThemeKeyword("caff", BrainrotVFXTheme.Steam),
        new ThemeKeyword("tea", BrainrotVFXTheme.Steam),

        new ThemeKeyword("sahur", BrainrotVFXTheme.Rhythm),
        new ThemeKeyword("tung", BrainrotVFXTheme.Rhythm),
        new ThemeKeyword("lololo", BrainrotVFXTheme.Rhythm),
        new ThemeKeyword("trulala", BrainrotVFXTheme.Rhythm),
        new ThemeKeyword("lerule", BrainrotVFXTheme.Rhythm),
        new ThemeKeyword("barabum", BrainrotVFXTheme.Rhythm),
        new ThemeKeyword("tirilika", BrainrotVFXTheme.Rhythm),

        new ThemeKeyword("patapim", BrainrotVFXTheme.Nature),
        new ThemeKeyword("liril", BrainrotVFXTheme.Nature),
        new ThemeKeyword("cact", BrainrotVFXTheme.Nature),
        new ThemeKeyword("leaf", BrainrotVFXTheme.Nature),
        new ThemeKeyword("tree", BrainrotVFXTheme.Nature),
        new ThemeKeyword("flur", BrainrotVFXTheme.Nature),
        new ThemeKeyword("flor", BrainrotVFXTheme.Nature),
        new ThemeKeyword("avocad", BrainrotVFXTheme.Nature),
        new ThemeKeyword("kiwi", BrainrotVFXTheme.Nature),
        new ThemeKeyword("coco", BrainrotVFXTheme.Nature),
        new ThemeKeyword("glorbo", BrainrotVFXTheme.Nature),
        new ThemeKeyword("potato", BrainrotVFXTheme.Nature),

        new ThemeKeyword("strawberr", BrainrotVFXTheme.Sweet),
        new ThemeKeyword("banan", BrainrotVFXTheme.Sweet),
        new ThemeKeyword("cherr", BrainrotVFXTheme.Sweet),
        new ThemeKeyword("cand", BrainrotVFXTheme.Sweet),
        new ThemeKeyword("sweet", BrainrotVFXTheme.Sweet),
        new ThemeKeyword("cake", BrainrotVFXTheme.Sweet),
        new ThemeKeyword("frut", BrainrotVFXTheme.Sweet),
        new ThemeKeyword("fruit", BrainrotVFXTheme.Sweet),
        new ThemeKeyword("melon", BrainrotVFXTheme.Sweet),
        new ThemeKeyword("ananas", BrainrotVFXTheme.Sweet),
        new ThemeKeyword("flaming", BrainrotVFXTheme.Sweet),
        new ThemeKeyword("chees", BrainrotVFXTheme.Sweet),
        new ThemeKeyword("kitt", BrainrotVFXTheme.Sweet),
        new ThemeKeyword("gatto", BrainrotVFXTheme.Sweet),
        new ThemeKeyword("meow", BrainrotVFXTheme.Sweet),
        new ThemeKeyword("nyan", BrainrotVFXTheme.Sweet),
        new ThemeKeyword("cat", BrainrotVFXTheme.Sweet),
        new ThemeKeyword("love", BrainrotVFXTheme.Sweet),
        new ThemeKeyword("heart", BrainrotVFXTheme.Sweet),
        new ThemeKeyword("pink", BrainrotVFXTheme.Sweet)
    };

    public static BrainrotVFXAnalysis Analyze(GameObject modelRoot) {
        BrainrotVFXAnalysis analysis = new BrainrotVFXAnalysis();
        List<ColorSample> samples = new List<ColorSample>();
        Dictionary<Texture, Texture2D> readableTextures = new Dictionary<Texture, Texture2D>();

        // bounds tinh theo vi tri + huong cua root nhung giu scale that, VFX dat o root se khop dung kich thuoc
        Matrix4x4 rootMatrix = Matrix4x4.TRS(modelRoot.transform.position, modelRoot.transform.rotation, Vector3.one).inverse;
        Bounds bounds = new Bounds();
        bool hasBounds = false;

        try {
            foreach (Renderer renderer in modelRoot.GetComponentsInChildren<Renderer>(true)) {
                if (renderer is ParticleSystemRenderer || renderer is TrailRenderer) continue;
                if (IsPartOfVFX(renderer.transform)) continue;

                Mesh mesh = GetMesh(renderer);
                if (mesh == null) continue;

                Matrix4x4 matrix = rootMatrix * renderer.transform.localToWorldMatrix;
                Vector3[] vertices = mesh.vertices;
                if (vertices.Length > 0) {
                    for (int i = 0; i < vertices.Length; i++) {
                        EncapsulatePoint(ref bounds, ref hasBounds, matrix.MultiplyPoint3x4(vertices[i]));
                    }
                }
                else {
                    EncapsulateMeshBounds(ref bounds, ref hasBounds, mesh.bounds, matrix);
                }

                SampleRendererColors(renderer, mesh, vertices, readableTextures, samples);
            }
        }
        finally {
            foreach (Texture2D readableTexture in readableTextures.Values) {
                if (readableTexture != null) {
                    Object.DestroyImmediate(readableTexture);
                }
            }
        }

        analysis.bounds = hasBounds ? bounds : new Bounds(Vector3.up * 0.5f, Vector3.one);
        PickColors(samples, analysis);

        return analysis;
    }

    public static BrainrotVFXTheme DetectTheme(string brainrotName, BrainrotVFXAnalysis analysis) {
        string lowerName = brainrotName.ToLowerInvariant();
        string[] words = lowerName.Split(new char[] { ' ', '_', '-' }, System.StringSplitOptions.RemoveEmptyEntries);

        foreach (ThemeKeyword themeKeyword in THEME_KEYWORDS) {
            if (IsMatchKeyword(lowerName, words, themeKeyword.keyword)) {
                return themeKeyword.theme;
            }
        }

        return DetectThemeByColor(analysis);
    }

    private static bool IsMatchKeyword(string lowerName, string[] words, string keyword) {
        if (keyword.Length >= 5) {
            return lowerName.Contains(keyword);
        }

        foreach (string word in words) {
            if (word.StartsWith(keyword)) return true;
        }

        return false;
    }

    // ten khong goi y gi thi doan theo mau chu dao cua model
    private static BrainrotVFXTheme DetectThemeByColor(BrainrotVFXAnalysis analysis) {
        Color.RGBToHSV(analysis.rawPrimaryColor, out float hue, out float saturation, out float value);

        if (analysis.isNeutral) {
            return value < 0.3f ? BrainrotVFXTheme.Shadow : BrainrotVFXTheme.Sparkle;
        }

        // nau (cam toi) thi ra hoi nuoc kieu ca phe, do / cam sang thi ra lua
        if (hue < 0.11f && hue > 0.03f && value < 0.55f) return BrainrotVFXTheme.Steam;
        if (hue < 0.11f || hue >= 0.95f) return BrainrotVFXTheme.Fire;
        if (hue < 0.18f) return BrainrotVFXTheme.Electric;
        if (hue < 0.45f) return BrainrotVFXTheme.Nature;
        if (hue < 0.68f) return BrainrotVFXTheme.Water;
        if (hue < 0.85f) return BrainrotVFXTheme.Cosmic;

        return BrainrotVFXTheme.Sweet;
    }

    private static void SampleRendererColors(Renderer renderer, Mesh mesh, Vector3[] vertices, Dictionary<Texture, Texture2D> readableTextures, List<ColorSample> samples) {
        Material[] materials = renderer.sharedMaterials;
        if (materials.Length == 0) return;

        Vector2[] uvs = mesh.uv;
        bool hasUV = vertices.Length > 0 && uvs.Length == vertices.Length;

        for (int subMesh = 0; subMesh < mesh.subMeshCount; subMesh++) {
            Material material = materials[Mathf.Min(subMesh, materials.Length - 1)];
            if (material == null) continue;

            Color tint = GetBaseColor(material);
            Texture2D texture = GetReadableTexture(GetBaseTexture(material), readableTextures);

            // khong doc duoc mesh thi lay mau deu tren ca texture
            if (vertices.Length == 0) {
                SampleTextureGrid(texture, tint, samples);
                continue;
            }

            int[] triangles = mesh.GetTriangles(subMesh);
            int triangleCount = triangles.Length / 3;
            int step = Mathf.Max(1, triangleCount / MAX_TRIANGLE_SAMPLES);

            for (int t = 0; t < triangleCount; t += step) {
                int a = triangles[t * 3];
                int b = triangles[t * 3 + 1];
                int c = triangles[t * 3 + 2];

                float area = Vector3.Cross(vertices[b] - vertices[a], vertices[c] - vertices[a]).magnitude * 0.5f;
                if (area <= 0f) continue;

                Color color = tint;
                if (texture != null && hasUV) {
                    Vector2 uv = (uvs[a] + uvs[b] + uvs[c]) / 3f;
                    color *= texture.GetPixelBilinear(uv.x, uv.y);
                }

                samples.Add(new ColorSample(color, area * step));
            }
        }
    }

    private static void SampleTextureGrid(Texture2D texture, Color tint, List<ColorSample> samples) {
        if (texture == null) {
            samples.Add(new ColorSample(tint, 1f));
            return;
        }

        for (int y = 0; y < FALLBACK_GRID_SIZE; y++) {
            for (int x = 0; x < FALLBACK_GRID_SIZE; x++) {
                Color color = texture.GetPixelBilinear((x + 0.5f) / FALLBACK_GRID_SIZE, (y + 0.5f) / FALLBACK_GRID_SIZE) * tint;
                if (color.a < 0.5f) continue;

                samples.Add(new ColorSample(color, 1f));
            }
        }
    }

    // chia mau theo 12 khoang hue, trong so nhan them do bao hoa * do sang de mau tuoi duoc uu tien
    // mau xam / trang / den gom rieng, neu model gan nhu toan mau trung tinh thi lay mau trung tinh lam chu dao
    private static void PickColors(List<ColorSample> samples, BrainrotVFXAnalysis analysis) {
        float[] binWeights = new float[HUE_BINS];
        Color[] binColors = new Color[HUE_BINS];
        float totalWeight = 0f;
        float colorfulWeight = 0f;
        float neutralWeight = 0f;
        Color neutralColor = Color.clear;

        foreach (ColorSample sample in samples) {
            if (sample.color.a < 0.3f) continue;

            Color.RGBToHSV(sample.color, out float hue, out float saturation, out float value);
            totalWeight += sample.weight;

            if (saturation < 0.25f || value < 0.2f) {
                neutralWeight += sample.weight;
                neutralColor += sample.color * sample.weight;
                continue;
            }

            float vividWeight = sample.weight * saturation * value;
            int bin = Mathf.Min((int)(hue * HUE_BINS), HUE_BINS - 1);
            binWeights[bin] += vividWeight;
            binColors[bin] += sample.color * vividWeight;
            colorfulWeight += sample.weight;
        }

        if (totalWeight <= 0f) {
            analysis.isNeutral = true;
            return;
        }

        Color neutralAverage = neutralWeight > 0f ? neutralColor / neutralWeight : Color.white;
        neutralAverage.a = 1f;

        int primaryBin = GetBestBin(binWeights, -1);
        analysis.isNeutral = primaryBin < 0 || colorfulWeight < totalWeight * 0.2f;

        if (analysis.isNeutral) {
            analysis.rawPrimaryColor = neutralAverage;
            analysis.primaryColor = MakeVivid(neutralAverage, true);
            analysis.secondaryColor = primaryBin >= 0 ? MakeVivid(GetBinColor(binWeights, binColors, primaryBin), false) : Color.Lerp(analysis.primaryColor, Color.white, 0.5f);
            return;
        }

        analysis.rawPrimaryColor = GetBinColor(binWeights, binColors, primaryBin);
        analysis.primaryColor = MakeVivid(analysis.rawPrimaryColor, false);

        int secondaryBin = GetBestBin(binWeights, primaryBin);
        if (secondaryBin >= 0 && binWeights[secondaryBin] > binWeights[primaryBin] * 0.12f) {
            analysis.secondaryColor = MakeVivid(GetBinColor(binWeights, binColors, secondaryBin), false);
        }
        else {
            // model chi co 1 tong mau: lay mau lech hue 1 chut va sang hon lam mau phu
            Color.RGBToHSV(analysis.primaryColor, out float hue, out float saturation, out float value);
            analysis.secondaryColor = Color.HSVToRGB(Mathf.Repeat(hue + 0.08f, 1f), saturation * 0.6f, 1f);
        }
    }

    // cong them nua trong so 2 khoang ben canh de mau nam giua 2 khoang khong bi chia doi
    private static int GetBestBin(float[] binWeights, int excludeBin) {
        int bestBin = -1;
        float bestScore = 0f;

        for (int i = 0; i < HUE_BINS; i++) {
            if (excludeBin >= 0 && GetBinDistance(i, excludeBin) < 2) continue;

            float score = binWeights[i] + (binWeights[(i + HUE_BINS - 1) % HUE_BINS] + binWeights[(i + 1) % HUE_BINS]) * 0.5f;
            if (score > bestScore) {
                bestScore = score;
                bestBin = i;
            }
        }

        return bestBin;
    }

    private static int GetBinDistance(int a, int b) {
        int distance = Mathf.Abs(a - b);
        return Mathf.Min(distance, HUE_BINS - distance);
    }

    private static Color GetBinColor(float[] binWeights, Color[] binColors, int bin) {
        int previous = (bin + HUE_BINS - 1) % HUE_BINS;
        int next = (bin + 1) % HUE_BINS;
        float weight = binWeights[previous] + binWeights[bin] + binWeights[next];
        if (weight <= 0f) return Color.white;

        Color color = (binColors[previous] + binColors[bin] + binColors[next]) / weight;
        color.a = 1f;

        return color;
    }

    // mau texture thuong toi va nhat, day do bao hoa + do sang len de hat sang ro tren nen
    private static Color MakeVivid(Color color, bool isNeutral) {
        Color.RGBToHSV(color, out float hue, out float saturation, out float value);

        if (isNeutral) {
            saturation = Mathf.Min(saturation, 0.15f);
            value = Mathf.Max(value, 0.85f);
        }
        else {
            saturation = Mathf.Clamp(saturation * 1.2f, 0.55f, 1f);
            value = Mathf.Max(value, 0.9f);
        }

        return Color.HSVToRGB(hue, saturation, value);
    }

    private static Color GetBaseColor(Material material) {
        if (material.HasProperty("_BaseColor")) return material.GetColor("_BaseColor");
        if (material.HasProperty("_Color")) return material.GetColor("_Color");

        return Color.white;
    }

    private static Texture GetBaseTexture(Material material) {
        if (material.HasProperty("_BaseMap")) return material.GetTexture("_BaseMap");
        if (material.HasProperty("_MainTex")) return material.GetTexture("_MainTex");

        return null;
    }

    // texture import khong bat Read/Write nen blit qua RenderTexture roi doc lai, khong dung vao import setting
    private static Texture2D GetReadableTexture(Texture texture, Dictionary<Texture, Texture2D> readableTextures) {
        if (texture == null) return null;
        if (readableTextures.TryGetValue(texture, out Texture2D cachedTexture)) return cachedTexture;

        RenderTexture renderTexture = RenderTexture.GetTemporary(TEXTURE_SAMPLE_SIZE, TEXTURE_SAMPLE_SIZE, 0, RenderTextureFormat.ARGB32, RenderTextureReadWrite.sRGB);
        RenderTexture previousActive = RenderTexture.active;

        Graphics.Blit(texture, renderTexture);
        RenderTexture.active = renderTexture;

        Texture2D readableTexture = new Texture2D(TEXTURE_SAMPLE_SIZE, TEXTURE_SAMPLE_SIZE, TextureFormat.RGBA32, false);
        readableTexture.ReadPixels(new Rect(0, 0, TEXTURE_SAMPLE_SIZE, TEXTURE_SAMPLE_SIZE), 0, 0);
        readableTexture.Apply();
        readableTexture.wrapMode = TextureWrapMode.Repeat;

        RenderTexture.active = previousActive;
        RenderTexture.ReleaseTemporary(renderTexture);

        readableTextures[texture] = readableTexture;
        return readableTexture;
    }

    private static Mesh GetMesh(Renderer renderer) {
        if (renderer is SkinnedMeshRenderer skinnedMeshRenderer) return skinnedMeshRenderer.sharedMesh;

        MeshFilter meshFilter = renderer.GetComponent<MeshFilter>();
        return meshFilter != null ? meshFilter.sharedMesh : null;
    }

    private static bool IsPartOfVFX(Transform target) {
        while (target != null) {
            if (target.GetComponent<BrainrotVFX>() != null) return true;
            target = target.parent;
        }

        return false;
    }

    private static void EncapsulateMeshBounds(ref Bounds bounds, ref bool hasBounds, Bounds meshBounds, Matrix4x4 matrix) {
        Vector3 min = meshBounds.min;
        Vector3 max = meshBounds.max;

        for (int i = 0; i < 8; i++) {
            Vector3 corner = new Vector3((i & 1) == 0 ? min.x : max.x, (i & 2) == 0 ? min.y : max.y, (i & 4) == 0 ? min.z : max.z);
            EncapsulatePoint(ref bounds, ref hasBounds, matrix.MultiplyPoint3x4(corner));
        }
    }

    private static void EncapsulatePoint(ref Bounds bounds, ref bool hasBounds, Vector3 point) {
        if (!hasBounds) {
            bounds = new Bounds(point, Vector3.zero);
            hasBounds = true;
            return;
        }

        bounds.Encapsulate(point);
    }
}
