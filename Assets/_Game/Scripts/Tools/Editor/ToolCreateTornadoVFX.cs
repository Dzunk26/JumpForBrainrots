using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;

public class ToolCreateTornadoVFX : EditorWindow {
    private static string VFX_FOLDER = "Assets/_Game/VFX/Tornado";
    private static string PREFABS_FOLDER = "Assets/_Game/Prefabs/ObstacleModels";
    private static string OBSTACLE_SO_FOLDER = "Assets/_Game/ScriptableObjects/ObstacleSOs";
    private static string SWIRL_SHADER = "_Game/VFX/Tornado Swirl";
    private static string PARTICLE_SHADER = "_Game/VFX/Tornado Particle";
    private static int MESH_SEGMENTS = 48;
    private static int MESH_RINGS = 24;
    private static int HITBOX_SEGMENTS = 16;

    [SerializeField] private string tornadoName = "Tornado Blue";
    [SerializeField] private Color coreColor = new Color(1.2f, 2.6f, 4f, 1f);
    [SerializeField] private Color edgeColor = new Color(0.15f, 0.55f, 2.4f, 1f);
    [SerializeField] private float height = 3f;
    [SerializeField] private float bottomRadius = 0.25f;
    [SerializeField] private float topRadius = 1.1f;
    [SerializeField] private int hitboxSlices = 5;
    [SerializeField] private float hitboxScale = 0.8f;
    [SerializeField] private bool isCreateObstacleSO = true;
    [SerializeField] private bool isOverwrite = false;

    private void OnGUI() {
        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("Preset Blue")) {
            tornadoName = "Tornado Blue";
            coreColor = new Color(1.2f, 2.6f, 4f, 1f);
            edgeColor = new Color(0.15f, 0.55f, 2.4f, 1f);
        }

        if (GUILayout.Button("Preset Orange")) {
            tornadoName = "Tornado Orange";
            coreColor = new Color(4f, 2.2f, 0.6f, 1f);
            edgeColor = new Color(2.4f, 0.6f, 0.08f, 1f);
        }
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.Space();

        tornadoName = EditorGUILayout.TextField("Name", tornadoName);
        coreColor = EditorGUILayout.ColorField(new GUIContent("Core Color"), coreColor, true, false, true);
        edgeColor = EditorGUILayout.ColorField(new GUIContent("Edge Color"), edgeColor, true, false, true);
        height = EditorGUILayout.FloatField("Height", height);
        bottomRadius = EditorGUILayout.FloatField("Bottom Radius", bottomRadius);
        topRadius = EditorGUILayout.FloatField("Top Radius", topRadius);
        hitboxSlices = EditorGUILayout.IntSlider("Hitbox Slices", hitboxSlices, 1, 10);
        hitboxScale = EditorGUILayout.Slider("Hitbox Scale", hitboxScale, 0.3f, 1.2f);
        isCreateObstacleSO = EditorGUILayout.Toggle("Create Obstacle SO", isCreateObstacleSO);
        isOverwrite = EditorGUILayout.Toggle("Overwrite Existing", isOverwrite);

        EditorGUILayout.Space();

        if (GUILayout.Button("Create Tornado")) {
            CreateTornado();
        }
    }

    [MenuItem("Tools/VFX/Tornado Creator")]
    private static void OpenWindow() {
        GetWindow<ToolCreateTornadoVFX>("Tornado Creator");
    }

    // tao mesh, material, particle, collider roi luu thanh prefab ObstacleModel
    private void CreateTornado() {
        Shader swirlShader = Shader.Find(SWIRL_SHADER);
        Shader particleShader = Shader.Find(PARTICLE_SHADER);
        if (swirlShader == null || particleShader == null) {
            Debug.LogError("Tornado shaders not found, check folder " + VFX_FOLDER);
            return;
        }

        string prefabPath = PREFABS_FOLDER + "/" + tornadoName + ".prefab";
        if (!isOverwrite && AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath) != null) {
            Debug.LogError("Prefab already exists: " + prefabPath + ", tick Overwrite Existing to replace it");
            return;
        }

        string meshesFolder = GetOrCreateFolder(VFX_FOLDER, "Meshes");
        string materialsFolder = GetOrCreateFolder(VFX_FOLDER, "Materials");

        GameObject root = new GameObject(tornadoName);
        root.AddComponent<ObstacleModel>();

        CreateHitbox(root.transform, meshesFolder);

        // 3 lop pheu long nhau, quay toc do khac nhau tao chieu sau
        Mesh outerMesh = SaveAsset(CreateFunnelMesh(tornadoName + " Outer", 1f), meshesFolder + "/" + tornadoName + " Outer.asset");
        Mesh middleMesh = SaveAsset(CreateFunnelMesh(tornadoName + " Middle", 0.75f), meshesFolder + "/" + tornadoName + " Middle.asset");
        Mesh coreMesh = SaveAsset(CreateFunnelMesh(tornadoName + " Core", 0.45f), meshesFolder + "/" + tornadoName + " Core.asset");

        Material outerMaterial = CreateSwirlMaterial(swirlShader, coreColor, edgeColor, 3f, 1.2f, 0.55f, 0.35f, 2.5f, 0.8f);
        Material middleMaterial = CreateSwirlMaterial(swirlShader, coreColor, edgeColor, 2f, 2f, 0.85f, 0.5f, 3.5f, 0.7f);
        Material coreMaterial = CreateSwirlMaterial(swirlShader, coreColor, coreColor * 0.6f, 4f, 0.8f, 1.2f, 0.7f, 2f, 0.5f);
        outerMaterial = SaveAsset(outerMaterial, materialsFolder + "/" + tornadoName + " Outer.mat");
        middleMaterial = SaveAsset(middleMaterial, materialsFolder + "/" + tornadoName + " Middle.mat");
        coreMaterial = SaveAsset(coreMaterial, materialsFolder + "/" + tornadoName + " Core.mat");

        CreateFunnelLayer("Funnel Outer", root.transform, outerMesh, outerMaterial);
        CreateFunnelLayer("Funnel Middle", root.transform, middleMesh, middleMaterial);
        CreateFunnelLayer("Funnel Core", root.transform, coreMesh, coreMaterial);

        Material dotMaterial = SaveAsset(CreateParticleMaterial(particleShader, coreColor, false, 2f), materialsFolder + "/" + tornadoName + " Dot.mat");
        Material ringMaterial = SaveAsset(CreateParticleMaterial(particleShader, edgeColor, true, 1.5f), materialsFolder + "/" + tornadoName + " Ring.mat");
        Material glowMaterial = SaveAsset(CreateParticleMaterial(particleShader, edgeColor, false, 1.5f), materialsFolder + "/" + tornadoName + " Glow.mat");

        CreateSparkles(root.transform, dotMaterial, outerMesh);
        CreateBubbles(root.transform, ringMaterial);
        CreateGroundRings(root.transform, ringMaterial);
        CreateBaseGlow(root.transform, glowMaterial, dotMaterial);

        GameObject prefab = PrefabUtility.SaveAsPrefabAsset(root, prefabPath);
        DestroyImmediate(root);

        if (isCreateObstacleSO) {
            CreateObstacleSO(prefab);
        }

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        EditorGUIUtility.PingObject(prefab);
        Debug.Log("Created tornado prefab: " + prefabPath);
    }

    private void CreateObstacleSO(GameObject prefab) {
        string soPath = OBSTACLE_SO_FOLDER + "/" + tornadoName + " SO.asset";
        ObstacleSO obstacleSO = AssetDatabase.LoadAssetAtPath<ObstacleSO>(soPath);
        if (obstacleSO == null) {
            obstacleSO = CreateInstance<ObstacleSO>();
            AssetDatabase.CreateAsset(obstacleSO, soPath);
        }

        SerializedObject serializedSO = new SerializedObject(obstacleSO);
        serializedSO.FindProperty("obstacleModel").objectReferenceValue = prefab.GetComponent<ObstacleModel>();
        serializedSO.ApplyModifiedPropertiesWithoutUndo();
        EditorUtility.SetDirty(obstacleSO);
    }

    // cat pheu thanh nhieu khoanh non cut, moi khoanh 1 mesh collider convex trigger
    // ghep lai bam theo duong cong cua pheu, trigger nao cung bao ve Obstacle tren root
    private void CreateHitbox(Transform parent, string meshesFolder) {
        GameObject hitbox = new GameObject("Hitbox");
        hitbox.transform.SetParent(parent, false);

        for (int i = 0; i < hitboxSlices; i++) {
            float bottomHeight01 = (float)i / hitboxSlices;
            float topHeight01 = (float)(i + 1) / hitboxSlices;
            string meshName = tornadoName + " Hitbox " + i;

            Mesh sliceMesh = SaveAsset(CreateSliceMesh(meshName, bottomHeight01, topHeight01), meshesFolder + "/" + meshName + ".asset");

            MeshCollider sliceCollider = hitbox.AddComponent<MeshCollider>();
            sliceCollider.sharedMesh = sliceMesh;
            sliceCollider.convex = true;
            sliceCollider.isTrigger = true;
        }
    }

    // non cut kin 2 dau, ban kinh lay tu GetFunnelRadius nhan hitboxScale
    private Mesh CreateSliceMesh(string meshName, float bottomHeight01, float topHeight01) {
        Vector3[] vertices = new Vector3[HITBOX_SEGMENTS * 2 + 2];
        int[] triangles = new int[HITBOX_SEGMENTS * 12];

        float sliceBottomRadius = GetFunnelRadius(bottomHeight01) * hitboxScale;
        float sliceTopRadius = GetFunnelRadius(topHeight01) * hitboxScale;
        int bottomCenter = HITBOX_SEGMENTS * 2;
        int topCenter = bottomCenter + 1;

        for (int i = 0; i < HITBOX_SEGMENTS; i++) {
            float angle = (float)i / HITBOX_SEGMENTS * Mathf.PI * 2f;
            Vector3 direction = new Vector3(Mathf.Cos(angle), 0f, Mathf.Sin(angle));

            vertices[i] = direction * sliceBottomRadius + Vector3.up * bottomHeight01 * height;
            vertices[i + HITBOX_SEGMENTS] = direction * sliceTopRadius + Vector3.up * topHeight01 * height;
        }

        vertices[bottomCenter] = Vector3.up * bottomHeight01 * height;
        vertices[topCenter] = Vector3.up * topHeight01 * height;

        int triangleIndex = 0;
        for (int i = 0; i < HITBOX_SEGMENTS; i++) {
            int next = (i + 1) % HITBOX_SEGMENTS;
            int bottom = i;
            int bottomNext = next;
            int top = i + HITBOX_SEGMENTS;
            int topNext = next + HITBOX_SEGMENTS;

            triangles[triangleIndex++] = bottom;
            triangles[triangleIndex++] = top;
            triangles[triangleIndex++] = bottomNext;
            triangles[triangleIndex++] = bottomNext;
            triangles[triangleIndex++] = top;
            triangles[triangleIndex++] = topNext;

            triangles[triangleIndex++] = bottomCenter;
            triangles[triangleIndex++] = bottom;
            triangles[triangleIndex++] = bottomNext;

            triangles[triangleIndex++] = topCenter;
            triangles[triangleIndex++] = topNext;
            triangles[triangleIndex++] = top;
        }

        Mesh mesh = new Mesh();
        mesh.name = meshName;
        mesh.vertices = vertices;
        mesh.triangles = triangles;
        mesh.RecalculateNormals();
        mesh.RecalculateBounds();

        return mesh;
    }

    // pheu tron xoay quanh truc Y, uv.x = goc, uv.y = chieu cao
    private Mesh CreateFunnelMesh(string meshName, float radiusScale) {
        int columns = MESH_SEGMENTS + 1;
        Vector3[] vertices = new Vector3[columns * (MESH_RINGS + 1)];
        Vector3[] normals = new Vector3[vertices.Length];
        Vector2[] uvs = new Vector2[vertices.Length];
        int[] triangles = new int[MESH_SEGMENTS * MESH_RINGS * 6];

        for (int y = 0; y <= MESH_RINGS; y++) {
            float v = (float)y / MESH_RINGS;
            float radius = GetFunnelRadius(v) * radiusScale;
            float slope = (GetFunnelRadius(v + 0.01f) - GetFunnelRadius(v - 0.01f)) * radiusScale / (0.02f * height);

            for (int x = 0; x <= MESH_SEGMENTS; x++) {
                float u = (float)x / MESH_SEGMENTS;
                float angle = u * Mathf.PI * 2f;
                Vector3 direction = new Vector3(Mathf.Cos(angle), 0f, Mathf.Sin(angle));
                int index = y * columns + x;

                vertices[index] = direction * radius + Vector3.up * v * height;
                normals[index] = (direction - Vector3.up * slope).normalized;
                uvs[index] = new Vector2(u, v);
            }
        }

        int triangleIndex = 0;
        for (int y = 0; y < MESH_RINGS; y++) {
            for (int x = 0; x < MESH_SEGMENTS; x++) {
                int bottomLeft = y * columns + x;
                int topLeft = bottomLeft + columns;

                triangles[triangleIndex++] = bottomLeft;
                triangles[triangleIndex++] = topLeft;
                triangles[triangleIndex++] = bottomLeft + 1;
                triangles[triangleIndex++] = bottomLeft + 1;
                triangles[triangleIndex++] = topLeft;
                triangles[triangleIndex++] = topLeft + 1;
            }
        }

        Mesh mesh = new Mesh();
        mesh.name = meshName;
        mesh.vertices = vertices;
        mesh.normals = normals;
        mesh.uv = uvs;
        mesh.triangles = triangles;
        mesh.RecalculateBounds();

        // noi rong bounds vi shader lam lac dinh, tranh bi cull som
        Bounds bounds = mesh.bounds;
        bounds.Expand(new Vector3(1f, 0f, 1f));
        mesh.bounds = bounds;

        return mesh;
    }

    private float GetFunnelRadius(float height01) {
        return Mathf.Lerp(bottomRadius, topRadius, Mathf.Pow(Mathf.Clamp01(height01), 1.6f));
    }

    private void CreateFunnelLayer(string layerName, Transform parent, Mesh mesh, Material material) {
        GameObject layer = new GameObject(layerName);
        layer.transform.SetParent(parent, false);
        layer.AddComponent<MeshFilter>().sharedMesh = mesh;

        MeshRenderer meshRenderer = layer.AddComponent<MeshRenderer>();
        meshRenderer.sharedMaterial = material;
        meshRenderer.shadowCastingMode = ShadowCastingMode.Off;
        meshRenderer.receiveShadows = false;
        meshRenderer.lightProbeUsage = LightProbeUsage.Off;
        meshRenderer.reflectionProbeUsage = ReflectionProbeUsage.Off;
    }

    private Material CreateSwirlMaterial(Shader shader, Color core, Color edge, float bandCount, float bandTilt, float swirlSpeed, float riseSpeed, float bandSharpness, float alpha) {
        Material material = new Material(shader);
        material.SetColor("_CoreColor", core);
        material.SetColor("_EdgeColor", edge);
        material.SetFloat("_BandCount", bandCount);
        material.SetFloat("_BandTilt", bandTilt);
        material.SetFloat("_SwirlSpeed", swirlSpeed);
        material.SetFloat("_RiseSpeed", riseSpeed);
        material.SetFloat("_BandSharpness", bandSharpness);
        material.SetFloat("_Alpha", alpha);

        return material;
    }

    private Material CreateParticleMaterial(Shader shader, Color color, bool isRing, float softness) {
        Material material = new Material(shader);
        material.SetColor("_Color", color);
        material.SetFloat("_IsRing", isRing ? 1f : 0f);
        material.SetFloat("_Softness", softness);

        return material;
    }

    // hat sang sinh tren be mat pheu, xoay quanh truc va bay len
    private void CreateSparkles(Transform parent, Material material, Mesh emitMesh) {
        ParticleSystem particle = CreateParticleSystem("Sparkles", parent, material, ParticleSystemRenderMode.Billboard);

        ParticleSystem.MainModule main = particle.main;
        main.startLifetime = new ParticleSystem.MinMaxCurve(1.2f, 2.2f);
        main.startSize = new ParticleSystem.MinMaxCurve(0.04f, 0.1f);

        ParticleSystem.EmissionModule emission = particle.emission;
        emission.rateOverTime = 30f;

        ParticleSystem.ShapeModule shape = particle.shape;
        shape.enabled = true;
        shape.shapeType = ParticleSystemShapeType.Mesh;
        shape.meshShapeType = ParticleSystemMeshShapeType.Triangle;
        shape.mesh = emitMesh;

        SetVelocity(particle, new ParticleSystem.MinMaxCurve(0.3f, 0.8f), new ParticleSystem.MinMaxCurve(1.5f, 3f));
        SetFade(particle, 1f);

        ParticleSystem.NoiseModule noise = particle.noise;
        noise.enabled = true;
        noise.strength = 0.2f;
        noise.frequency = 1f;
    }

    // bong bong vong tron nho bay quanh chan loc
    private void CreateBubbles(Transform parent, Material material) {
        ParticleSystem particle = CreateParticleSystem("Bubbles", parent, material, ParticleSystemRenderMode.Billboard);

        ParticleSystem.MainModule main = particle.main;
        main.startLifetime = new ParticleSystem.MinMaxCurve(1.5f, 2.5f);
        main.startSize = new ParticleSystem.MinMaxCurve(0.08f, 0.18f);

        ParticleSystem.EmissionModule emission = particle.emission;
        emission.rateOverTime = 4f;

        ParticleSystem.ShapeModule shape = particle.shape;
        shape.enabled = true;
        shape.shapeType = ParticleSystemShapeType.Sphere;
        shape.radius = topRadius;
        shape.radiusThickness = 0.3f;
        shape.scale = new Vector3(1f, 0.1f, 1f);
        shape.position = new Vector3(0f, 0.2f, 0f);

        SetVelocity(particle, new ParticleSystem.MinMaxCurve(0.4f, 0.9f), new ParticleSystem.MinMaxCurve(0.5f, 1f));
        SetFade(particle, 1f);

        ParticleSystem.NoiseModule noise = particle.noise;
        noise.enabled = true;
        noise.strength = 0.15f;
        noise.frequency = 0.8f;
    }

    // song vong tron loang ra duoi dat
    private void CreateGroundRings(Transform parent, Material material) {
        ParticleSystem particle = CreateParticleSystem("Ground Rings", parent, material, ParticleSystemRenderMode.HorizontalBillboard);
        particle.transform.localPosition = new Vector3(0f, 0.02f, 0f);

        ParticleSystem.MainModule main = particle.main;
        main.startLifetime = 1.4f;
        main.startSize = topRadius * 1.8f;

        ParticleSystem.EmissionModule emission = particle.emission;
        emission.rateOverTime = 1.5f;

        ParticleSystem.ShapeModule shape = particle.shape;
        shape.enabled = false;

        ParticleSystem.SizeOverLifetimeModule sizeOverLifetime = particle.sizeOverLifetime;
        sizeOverLifetime.enabled = true;
        sizeOverLifetime.size = new ParticleSystem.MinMaxCurve(1f, AnimationCurve.EaseInOut(0f, 0.35f, 1f, 1f));

        SetFade(particle, 0.8f);
    }

    // anh sang o chan loc: 1 lop phang duoi dat + 1 lop billboard sang o loi
    private void CreateBaseGlow(Transform parent, Material glowMaterial, Material coreMaterial) {
        ParticleSystem groundGlow = CreateParticleSystem("Ground Glow", parent, glowMaterial, ParticleSystemRenderMode.HorizontalBillboard);
        groundGlow.transform.localPosition = new Vector3(0f, 0.01f, 0f);

        ParticleSystem.MainModule groundMain = groundGlow.main;
        groundMain.startLifetime = 0.8f;
        groundMain.startSize = new ParticleSystem.MinMaxCurve(topRadius * 1.6f, topRadius * 2f);

        ParticleSystem.EmissionModule groundEmission = groundGlow.emission;
        groundEmission.rateOverTime = 5f;

        ParticleSystem.ShapeModule groundShape = groundGlow.shape;
        groundShape.enabled = false;

        SetFade(groundGlow, 0.35f);

        ParticleSystem coreGlow = CreateParticleSystem("Core Glow", parent, coreMaterial, ParticleSystemRenderMode.Billboard);
        coreGlow.transform.localPosition = new Vector3(0f, height * 0.08f, 0f);

        ParticleSystem.MainModule coreMain = coreGlow.main;
        coreMain.startLifetime = 0.6f;
        coreMain.startSize = new ParticleSystem.MinMaxCurve(bottomRadius * 3f, bottomRadius * 4.5f);

        ParticleSystem.EmissionModule coreEmission = coreGlow.emission;
        coreEmission.rateOverTime = 6f;

        ParticleSystem.ShapeModule coreShape = coreGlow.shape;
        coreShape.enabled = false;

        SetFade(coreGlow, 0.5f);
    }

    private ParticleSystem CreateParticleSystem(string particleName, Transform parent, Material material, ParticleSystemRenderMode renderMode) {
        GameObject particleObject = new GameObject(particleName);
        particleObject.transform.SetParent(parent, false);

        ParticleSystem particle = particleObject.AddComponent<ParticleSystem>();
        particle.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);

        ParticleSystem.MainModule main = particle.main;
        main.loop = true;
        main.prewarm = true;
        main.playOnAwake = true;
        main.startSpeed = 0f;
        main.startColor = Color.white;
        main.maxParticles = 200;
        main.simulationSpace = ParticleSystemSimulationSpace.Local;
        main.scalingMode = ParticleSystemScalingMode.Hierarchy;

        ParticleSystemRenderer particleRenderer = particleObject.GetComponent<ParticleSystemRenderer>();
        particleRenderer.renderMode = renderMode;
        particleRenderer.sharedMaterial = material;
        particleRenderer.shadowCastingMode = ShadowCastingMode.Off;
        particleRenderer.receiveShadows = false;
        particleRenderer.lightProbeUsage = LightProbeUsage.Off;
        particleRenderer.reflectionProbeUsage = ReflectionProbeUsage.Off;

        return particle;
    }

    // x, y, z (va orbital) phai cung mode, neu khong unity bao loi
    private void SetVelocity(ParticleSystem particle, ParticleSystem.MinMaxCurve upSpeed, ParticleSystem.MinMaxCurve orbitalSpeed) {
        ParticleSystem.VelocityOverLifetimeModule velocity = particle.velocityOverLifetime;
        velocity.enabled = true;
        velocity.space = ParticleSystemSimulationSpace.Local;
        velocity.x = new ParticleSystem.MinMaxCurve(0f, 0f);
        velocity.y = upSpeed;
        velocity.z = new ParticleSystem.MinMaxCurve(0f, 0f);
        velocity.orbitalX = new ParticleSystem.MinMaxCurve(0f, 0f);
        velocity.orbitalY = orbitalSpeed;
        velocity.orbitalZ = new ParticleSystem.MinMaxCurve(0f, 0f);
    }

    private void SetFade(ParticleSystem particle, float maxAlpha) {
        Gradient gradient = new Gradient();
        gradient.SetKeys(
            new GradientColorKey[] { new GradientColorKey(Color.white, 0f), new GradientColorKey(Color.white, 1f) },
            new GradientAlphaKey[] { new GradientAlphaKey(0f, 0f), new GradientAlphaKey(maxAlpha, 0.2f), new GradientAlphaKey(maxAlpha, 0.6f), new GradientAlphaKey(0f, 1f) });

        ParticleSystem.ColorOverLifetimeModule colorOverLifetime = particle.colorOverLifetime;
        colorOverLifetime.enabled = true;
        colorOverLifetime.color = new ParticleSystem.MinMaxGradient(gradient);
    }

    // neu asset da ton tai thi copy de vao, giu nguyen GUID cho cac reference cu
    private T SaveAsset<T>(T asset, string path) where T : Object {
        T existingAsset = AssetDatabase.LoadAssetAtPath<T>(path);
        if (existingAsset == null) {
            AssetDatabase.CreateAsset(asset, path);
            return asset;
        }

        EditorUtility.CopySerialized(asset, existingAsset);
        EditorUtility.SetDirty(existingAsset);
        return existingAsset;
    }

    private string GetOrCreateFolder(string parentFolder, string folderName) {
        string folderPath = parentFolder + "/" + folderName;
        if (!AssetDatabase.IsValidFolder(folderPath)) {
            AssetDatabase.CreateFolder(parentFolder, folderName);
        }

        return folderPath;
    }
}
