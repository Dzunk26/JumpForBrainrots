using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEngine;

// chuyen VFX Roblox (.rbxmx) sang prefab Particle System cua Unity
public class ToolConvertRobloxVFX : EditorWindow {
    private static string OUTPUT_FOLDER = "Assets/_Game/VFX/Roblox";
    private static string PARTICLE_SHADER = "Universal Render Pipeline/Particles/Unlit";
    private static int MAX_GRADIENT_KEYS = 8;
    private static string[] TEXTURE_EXTENSIONS = { ".png", ".jpg", ".jpeg", ".tga", ".psd", ".tif", ".tiff", ".dds" };
    private static string[] UNSUPPORTED_CLASSES = { "Fire", "Smoke", "Sparkles", "Explosion", "Highlight" };

    [SerializeField] private string rbxmxPath = "";
    [SerializeField] private DefaultAsset textureFolder;
    [SerializeField] private string prefabName = "";
    [SerializeField] private float studScale = 0.28f;
    [SerializeField] private int defaultBurstCount = 10;
    [SerializeField] private bool isCenterAtOrigin = true;
    [SerializeField] private bool isOverwrite = false;

    private string report = "";
    private Vector2 reportScroll;

    // du lieu tam trong 1 lan convert
    private string outputFolder;
    private Shader particleShader;
    private Vector3 centerOffset;
    private bool isAnyLooping;
    private float maxDuration;
    private Dictionary<string, Transform> transformByReferent;
    private Dictionary<string, Material> materialCache;
    private HashSet<string> referencedAttachments;
    private List<KeyValuePair<RobloxInstance, GameObject>> pendingLines;
    private HashSet<string> missingTextures;
    private List<string> warnings;
    private int emitterCount;
    private int trailCount;
    private int beamCount;
    private int lightCount;

    private void OnGUI() {
        EditorGUILayout.BeginHorizontal();
        rbxmxPath = EditorGUILayout.TextField("Roblox File (.rbxmx)", rbxmxPath);
        if (GUILayout.Button("Browse", GUILayout.Width(60))) {
            string startFolder = File.Exists(rbxmxPath) ? Path.GetDirectoryName(rbxmxPath) : "";
            string path = EditorUtility.OpenFilePanel("Select Roblox model", startFolder, "rbxmx");
            if (!string.IsNullOrEmpty(path)) {
                rbxmxPath = path;
                prefabName = Path.GetFileNameWithoutExtension(path);
            }
        }
        EditorGUILayout.EndHorizontal();

        textureFolder = (DefaultAsset)EditorGUILayout.ObjectField("Texture Folder", textureFolder, typeof(DefaultAsset), false);
        prefabName = EditorGUILayout.TextField("Prefab Name", prefabName);
        studScale = EditorGUILayout.FloatField(new GUIContent("Stud Scale", "1 stud Roblox = bao nhieu unit Unity"), studScale);
        defaultBurstCount = EditorGUILayout.IntField(new GUIContent("Burst Count (disabled emitter)", "Emitter bi tat (thuong duoc script goi Emit) ma khong co attribute EmitCount se ban 1 burst voi so hat nay, 0 = bo qua"), defaultBurstCount);
        isCenterAtOrigin = EditorGUILayout.Toggle("Center At Origin", isCenterAtOrigin);
        isOverwrite = EditorGUILayout.Toggle("Overwrite Existing", isOverwrite);

        EditorGUILayout.HelpBox("Texture: dat ten file anh theo asset id (vd 1234567.png) hoac ten file goc trong rbxasset://, roi bo vao Texture Folder.", MessageType.Info);

        EditorGUILayout.Space();

        if (GUILayout.Button("Convert")) {
            Convert();
        }

        if (!string.IsNullOrEmpty(report)) {
            EditorGUILayout.Space();
            reportScroll = EditorGUILayout.BeginScrollView(reportScroll);
            EditorGUILayout.TextArea(report, EditorStyles.wordWrappedLabel);
            EditorGUILayout.EndScrollView();
        }
    }

    [MenuItem("Tools/VFX/Roblox VFX Converter")]
    private static void OpenWindow() {
        GetWindow<ToolConvertRobloxVFX>("Roblox VFX Converter");
    }

    private void Convert() {
        if (!File.Exists(rbxmxPath)) {
            Debug.LogError("Roblox file not found: " + rbxmxPath);
            return;
        }

        if (string.IsNullOrWhiteSpace(prefabName)) prefabName = Path.GetFileNameWithoutExtension(rbxmxPath);

        particleShader = Shader.Find(PARTICLE_SHADER);
        if (particleShader == null) {
            Debug.LogError("Shader not found: " + PARTICLE_SHADER);
            return;
        }

        outputFolder = OUTPUT_FOLDER + "/" + prefabName;
        string prefabPath = outputFolder + "/" + prefabName + ".prefab";
        if (!isOverwrite && AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath) != null) {
            Debug.LogError("Prefab already exists: " + prefabPath + ", tick Overwrite Existing to replace it");
            return;
        }

        List<RobloxInstance> roots;
        try {
            roots = RobloxModelParser.ParseFile(rbxmxPath);
        } catch (System.Exception exception) {
            Debug.LogError("Cannot read .rbxmx file (phai luu dang XML .rbxmx, khong phai .rbxm): " + exception.Message);
            return;
        }

        ResetState();
        EnsureFolder(outputFolder);

        foreach (RobloxInstance root in roots) {
            CollectReferences(root);
        }

        centerOffset = Vector3.zero;
        if (isCenterAtOrigin) {
            foreach (RobloxInstance root in roots) {
                if (TryFindFirstPartPosition(root, out centerOffset)) break;
            }
        }

        GameObject rootObject = new GameObject(prefabName);
        foreach (RobloxInstance root in roots) {
            BuildInstance(root, rootObject.transform);
        }

        foreach (KeyValuePair<RobloxInstance, GameObject> pending in pendingLines) {
            if (pending.Key.className == "Trail") SetupTrail(pending.Key, pending.Value);
            else SetupBeam(pending.Key, pending.Value);
        }

        SetupPreviewRoot(rootObject);

        GameObject prefab = PrefabUtility.SaveAsPrefabAsset(rootObject, prefabPath);
        DestroyImmediate(rootObject);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        BuildReport(prefabPath);
        Debug.Log(report);
        if (prefab != null) EditorGUIUtility.PingObject(prefab);
    }

    private void ResetState() {
        transformByReferent = new Dictionary<string, Transform>();
        materialCache = new Dictionary<string, Material>();
        referencedAttachments = new HashSet<string>();
        pendingLines = new List<KeyValuePair<RobloxInstance, GameObject>>();
        missingTextures = new HashSet<string>();
        warnings = new List<string>();
        isAnyLooping = false;
        maxDuration = 0f;
        emitterCount = 0;
        trailCount = 0;
        beamCount = 0;
        lightCount = 0;
    }

    #region Hierarchy

    private void CollectReferences(RobloxInstance instance) {
        if (instance.className == "Trail" || instance.className == "Beam") {
            string attachment0 = instance.GetRef("Attachment0");
            string attachment1 = instance.GetRef("Attachment1");
            if (attachment0.Length > 0) referencedAttachments.Add(attachment0);
            if (attachment1.Length > 0) referencedAttachments.Add(attachment1);
        }

        if (IsScript(instance) && instance.GetString("Source").Contains(":Emit(")) {
            warnings.Add("Script '" + instance.name + "' goi :Emit(), kiem tra lai timing burst cho dung voi script");
        }

        foreach (string unsupported in UNSUPPORTED_CLASSES) {
            if (instance.className == unsupported) warnings.Add("Bo qua " + instance.className + " '" + instance.name + "' (chua ho tro)");
        }

        foreach (RobloxInstance child in instance.children) {
            CollectReferences(child);
        }
    }

    // chi tao GameObject cho node la effect hoac chua effect ben trong
    private bool ShouldBuild(RobloxInstance instance) {
        if (IsEffect(instance)) return true;
        if (referencedAttachments.Contains(instance.referent)) return true;
        foreach (RobloxInstance child in instance.children) {
            if (ShouldBuild(child)) return true;
        }
        return false;
    }

    private void BuildInstance(RobloxInstance instance, Transform parent) {
        if (!ShouldBuild(instance)) return;

        GameObject gameObject = new GameObject(instance.name);
        gameObject.transform.SetParent(parent, false);
        if (instance.referent.Length > 0) transformByReferent[instance.referent] = gameObject.transform;

        RobloxCFrame cframe;
        if (instance.TryGetCFrame("CFrame", out cframe)) {
            if (IsAttachment(instance)) {
                gameObject.transform.localPosition = cframe.GetUnityPosition(studScale);
                gameObject.transform.localRotation = cframe.GetUnityRotation();
            } else {
                // CFrame cua Part la toa do world
                gameObject.transform.SetPositionAndRotation(cframe.GetUnityPosition(studScale) - centerOffset, cframe.GetUnityRotation());
            }
        }

        switch (instance.className) {
            case "ParticleEmitter":
                SetupEmitter(instance, gameObject);
                break;
            case "Trail":
            case "Beam":
                pendingLines.Add(new KeyValuePair<RobloxInstance, GameObject>(instance, gameObject));
                break;
            case "PointLight":
            case "SpotLight":
                SetupLight(instance, gameObject);
                break;
        }

        foreach (RobloxInstance child in instance.children) {
            BuildInstance(child, gameObject.transform);
        }
    }

    private bool TryFindFirstPartPosition(RobloxInstance instance, out Vector3 position) {
        RobloxCFrame cframe;
        if (!IsAttachment(instance) && ShouldBuild(instance) && instance.TryGetCFrame("CFrame", out cframe)) {
            position = cframe.GetUnityPosition(studScale);
            return true;
        }

        foreach (RobloxInstance child in instance.children) {
            if (TryFindFirstPartPosition(child, out position)) return true;
        }
        position = Vector3.zero;
        return false;
    }

    // ParticleSystem rong o root de chon root la preview duoc ca VFX
    private void SetupPreviewRoot(GameObject rootObject) {
        ParticleSystem particleSystem = rootObject.AddComponent<ParticleSystem>();
        particleSystem.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);

        ParticleSystem.MainModule main = particleSystem.main;
        main.loop = isAnyLooping;
        main.duration = Mathf.Max(maxDuration, 0.05f);

        ParticleSystem.EmissionModule emission = particleSystem.emission;
        emission.enabled = false;
        ParticleSystem.ShapeModule shape = particleSystem.shape;
        shape.enabled = false;
        rootObject.GetComponent<ParticleSystemRenderer>().enabled = false;
    }

    #endregion

    #region ParticleEmitter

    private void SetupEmitter(RobloxInstance emitter, GameObject gameObject) {
        emitterCount++;
        ParticleSystem particleSystem = gameObject.AddComponent<ParticleSystem>();
        particleSystem.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);

        Vector2 lifetime = emitter.GetNumberRange("Lifetime", new Vector2(5f, 10f));
        Vector2 speed = emitter.GetNumberRange("Speed", new Vector2(5f, 5f)) * studScale;
        Vector2 rotation = emitter.GetNumberRange("Rotation", Vector2.zero);

        int shapeInOut = emitter.GetInt("ShapeInOut", 0);
        if (shapeInOut == 1) speed = new Vector2(-speed.y, -speed.x);
        else if (shapeInOut == 2) speed = new Vector2(-speed.y, speed.y);

        ParticleSystem.MainModule main = particleSystem.main;
        main.playOnAwake = true;
        main.startLifetime = new ParticleSystem.MinMaxCurve(lifetime.x, lifetime.y);
        main.startSpeed = new ParticleSystem.MinMaxCurve(speed.x, speed.y);
        main.startRotation = new ParticleSystem.MinMaxCurve(rotation.x * Mathf.Deg2Rad, rotation.y * Mathf.Deg2Rad);
        main.startColor = Color.white;
        main.gravityModifier = 0f;
        main.simulationSpace = emitter.GetBool("LockedToPart") ? ParticleSystemSimulationSpace.Local : ParticleSystemSimulationSpace.World;
        main.simulationSpeed = Mathf.Max(emitter.GetFloat("TimeScale", 1f), 0.01f);
        main.scalingMode = ParticleSystemScalingMode.Hierarchy;

        SetupEmitterEmission(emitter, particleSystem, lifetime);
        SetupEmitterSize(emitter, particleSystem);
        SetupEmitterShape(emitter, particleSystem);
        SetupEmitterMotion(emitter, particleSystem);
        SetupEmitterFlipbook(emitter, particleSystem);

        ParticleSystem.ColorOverLifetimeModule colorOverLifetime = particleSystem.colorOverLifetime;
        colorOverLifetime.enabled = true;
        colorOverLifetime.color = BuildGradient(emitter.GetColorSequence("Color"), emitter.GetNumberSequence("Transparency"));

        ParticleSystemRenderer renderer = gameObject.GetComponent<ParticleSystemRenderer>();
        renderer.renderMode = ParticleSystemRenderMode.Billboard;
        switch (emitter.GetInt("Orientation", 0)) {
            case 1:
                renderer.renderMode = ParticleSystemRenderMode.VerticalBillboard;
                break;
            case 2:
                renderer.renderMode = ParticleSystemRenderMode.Stretch;
                renderer.velocityScale = 0f;
                renderer.lengthScale = 1f;
                break;
            case 3:
                renderer.alignment = ParticleSystemRenderSpace.Velocity;
                break;
        }
        renderer.sortingFudge = -emitter.GetFloat("ZOffset") * 10f;
        renderer.sharedMaterial = GetMaterial(emitter);

        if (emitter.Has("Squash")) {
            List<RobloxNumberKey> squash = emitter.GetNumberSequence("Squash");
            foreach (RobloxNumberKey key in squash) {
                if (Mathf.Abs(key.value) > 0.001f) {
                    warnings.Add("Emitter '" + emitter.name + "' co Squash, Unity khong co tuong duong nen bo qua");
                    break;
                }
            }
        }
    }

    // Rate lien tuc, hoac 1 burst neu emitter bi tat / co attribute EmitCount
    private void SetupEmitterEmission(RobloxInstance emitter, ParticleSystem particleSystem, Vector2 lifetime) {
        ParticleSystem.MainModule main = particleSystem.main;
        ParticleSystem.EmissionModule emission = particleSystem.emission;

        float rate = emitter.GetFloat("Rate", 5f);
        float emitCount = emitter.GetAttributeFloat("EmitCount", -1f);
        float emitDelay = Mathf.Max(emitter.GetAttributeFloat("EmitDelay", 0f), 0f);
        bool isEnabled = emitter.GetBool("Enabled", true);

        if (emitCount > 0f || !isEnabled) {
            int count = emitCount > 0f ? Mathf.RoundToInt(emitCount) : defaultBurstCount;
            emission.rateOverTime = 0f;
            main.loop = false;
            main.duration = Mathf.Max(emitDelay + lifetime.y, 0.05f);
            main.maxParticles = Mathf.Clamp(count * 2, 10, 5000);

            if (count <= 0) {
                emission.enabled = false;
                warnings.Add("Emitter '" + emitter.name + "' bi tat va khong co EmitCount, da tat emission");
            } else {
                emission.SetBursts(new[] { new ParticleSystem.Burst(emitDelay, (short)Mathf.Min(count, short.MaxValue)) });
            }
        } else {
            emission.rateOverTime = rate;
            main.loop = true;
            main.duration = 5f;
            main.maxParticles = Mathf.Clamp(Mathf.CeilToInt(rate * lifetime.y * 1.2f), 50, 5000);
            isAnyLooping = true;
        }
        maxDuration = Mathf.Max(maxDuration, main.duration);
    }

    private void SetupEmitterSize(RobloxInstance emitter, ParticleSystem particleSystem) {
        List<RobloxNumberKey> sizeKeys = emitter.GetNumberSequence("Size");
        if (sizeKeys.Count == 0) sizeKeys.Add(new RobloxNumberKey { time = 0f, value = 1f });

        float maxSize = 0f;
        float minSize = float.MaxValue;
        bool isRandom = false;
        foreach (RobloxNumberKey key in sizeKeys) {
            maxSize = Mathf.Max(maxSize, key.value + key.envelope);
            minSize = Mathf.Min(minSize, key.value);
            if (key.envelope > 0.0001f) isRandom = true;
        }
        if (maxSize <= 0f) maxSize = 1f;

        ParticleSystem.MainModule main = particleSystem.main;
        main.startSize = maxSize * studScale;

        bool isConstant = !isRandom && Mathf.Approximately(minSize, maxSize);
        ParticleSystem.SizeOverLifetimeModule sizeOverLifetime = particleSystem.sizeOverLifetime;
        sizeOverLifetime.enabled = !isConstant;
        if (isConstant) return;

        if (isRandom) {
            AnimationCurve minCurve = BuildCurve(sizeKeys, -1f, maxSize);
            AnimationCurve maxCurve = BuildCurve(sizeKeys, 1f, maxSize);
            sizeOverLifetime.size = new ParticleSystem.MinMaxCurve(1f, minCurve, maxCurve);
        } else {
            sizeOverLifetime.size = new ParticleSystem.MinMaxCurve(1f, BuildCurve(sizeKeys, 0f, maxSize));
        }
    }

    private void SetupEmitterShape(RobloxInstance emitter, ParticleSystem particleSystem) {
        ParticleSystem.ShapeModule shape = particleSystem.shape;
        shape.enabled = true;

        Vector2 spread = emitter.GetVector2("SpreadAngle");
        float spreadMax = Mathf.Max(Mathf.Abs(spread.x), Mathf.Abs(spread.y));
        Vector3 direction = NormalIdToUnity(emitter.GetInt("EmissionDirection", 1));
        Quaternion directionRotation = Quaternion.FromToRotation(Vector3.forward, direction);
        bool isSurface = emitter.GetInt("ShapeStyle", 0) == 1;

        RobloxInstance parentPart = emitter.parent;
        bool isOnPart = parentPart != null && !IsAttachment(parentPart) && parentPart.Has("size");

        if (!isOnPart) {
            // emitter trong Attachment phat ra tu 1 diem
            if (spreadMax >= 180f) {
                shape.shapeType = ParticleSystemShapeType.Sphere;
            } else if (spreadMax > 90f) {
                shape.shapeType = ParticleSystemShapeType.Hemisphere;
            } else {
                shape.shapeType = ParticleSystemShapeType.Cone;
                shape.angle = spreadMax;
            }
            shape.radius = 0.0001f;
            shape.rotation = directionRotation.eulerAngles;
            return;
        }

        Vector3 partSize = parentPart.GetVector3("size") * studScale;
        partSize = new Vector3(Mathf.Abs(partSize.x), Mathf.Abs(partSize.y), Mathf.Abs(partSize.z));

        switch (emitter.GetInt("Shape", 0)) {
            case 1: // Sphere
                shape.shapeType = ParticleSystemShapeType.Sphere;
                shape.radius = Mathf.Min(partSize.x, Mathf.Min(partSize.y, partSize.z)) * 0.5f;
                shape.radiusThickness = isSurface ? 0f : 1f;
                break;
            case 2: // Cylinder, xap xi bang sphere bop theo chieu cao
                float cylinderRadius = Mathf.Max(partSize.x, partSize.z) * 0.5f;
                shape.shapeType = ParticleSystemShapeType.Sphere;
                shape.radius = cylinderRadius;
                shape.scale = new Vector3(1f, partSize.y / Mathf.Max(cylinderRadius * 2f, 0.0001f), 1f);
                shape.radiusThickness = isSurface ? 0f : 1f;
                break;
            case 3: // Disc
                shape.shapeType = ParticleSystemShapeType.Circle;
                shape.radius = Mathf.Max(partSize.x, partSize.z) * 0.5f;
                shape.radiusThickness = isSurface ? 0f : 1f;
                shape.rotation = new Vector3(90f, 0f, 0f);
                break;
            default: // Box, Unity box phat theo +Z nen xoay shape ve EmissionDirection
                shape.shapeType = isSurface ? ParticleSystemShapeType.BoxShell : ParticleSystemShapeType.Box;
                Vector3 localSize = Quaternion.Inverse(directionRotation) * partSize;
                shape.scale = new Vector3(Mathf.Abs(localSize.x), Mathf.Abs(localSize.y), Mathf.Abs(localSize.z));
                shape.rotation = directionRotation.eulerAngles;
                shape.randomDirectionAmount = Mathf.Clamp01(spreadMax / 180f);
                break;
        }
    }

    private void SetupEmitterMotion(RobloxInstance emitter, ParticleSystem particleSystem) {
        Vector3 acceleration = emitter.GetVector3("Acceleration");
        if (acceleration.sqrMagnitude > 0.0001f) {
            ParticleSystem.ForceOverLifetimeModule force = particleSystem.forceOverLifetime;
            force.enabled = true;
            force.space = ParticleSystemSimulationSpace.World;
            force.x = acceleration.x * studScale;
            force.y = acceleration.y * studScale;
            force.z = -acceleration.z * studScale;
        }

        // Roblox Drag: toc do giam mot nua sau moi 1/Drag giay -> he so mu = Drag * ln2
        float drag = emitter.GetFloat("Drag");
        if (drag > 0f) {
            ParticleSystem.LimitVelocityOverLifetimeModule limit = particleSystem.limitVelocityOverLifetime;
            limit.enabled = true;
            limit.limit = 100000f;
            limit.drag = drag * Mathf.Log(2f);
            limit.multiplyDragByParticleSize = false;
            limit.multiplyDragByParticleVelocity = false;
        }

        Vector2 rotSpeed = emitter.GetNumberRange("RotSpeed", Vector2.zero);
        if (rotSpeed != Vector2.zero) {
            ParticleSystem.RotationOverLifetimeModule rotationOverLifetime = particleSystem.rotationOverLifetime;
            rotationOverLifetime.enabled = true;
            rotationOverLifetime.z = new ParticleSystem.MinMaxCurve(rotSpeed.x * Mathf.Deg2Rad, rotSpeed.y * Mathf.Deg2Rad);
        }

        float velocityInheritance = emitter.GetFloat("VelocityInheritance");
        if (velocityInheritance > 0f) {
            ParticleSystem.InheritVelocityModule inheritVelocity = particleSystem.inheritVelocity;
            inheritVelocity.enabled = true;
            inheritVelocity.mode = ParticleSystemInheritVelocityMode.Initial;
            inheritVelocity.curve = velocityInheritance;
        }
    }

    private void SetupEmitterFlipbook(RobloxInstance emitter, ParticleSystem particleSystem) {
        int tiles;
        switch (emitter.GetInt("FlipbookLayout", 0)) {
            case 1: tiles = 2; break;
            case 2: tiles = 4; break;
            case 3: tiles = 8; break;
            default: return;
        }

        int frameCount = tiles * tiles;
        float lastFrame = (frameCount - 1f) / frameCount;
        Vector2 framerate = emitter.GetNumberRange("FlipbookFramerate", Vector2.one);

        ParticleSystem.TextureSheetAnimationModule sheet = particleSystem.textureSheetAnimation;
        sheet.enabled = true;
        sheet.mode = ParticleSystemAnimationMode.Grid;
        sheet.animation = ParticleSystemAnimationType.WholeSheet;
        sheet.numTilesX = tiles;
        sheet.numTilesY = tiles;

        switch (emitter.GetInt("FlipbookMode", 0)) {
            case 1: // OneShot
                sheet.timeMode = ParticleSystemAnimationTimeMode.Lifetime;
                sheet.frameOverTime = new ParticleSystem.MinMaxCurve(1f, AnimationCurve.Linear(0f, 0f, 1f, lastFrame));
                sheet.cycleCount = 1;
                break;
            case 2: // PingPong
                sheet.timeMode = ParticleSystemAnimationTimeMode.Lifetime;
                sheet.frameOverTime = new ParticleSystem.MinMaxCurve(1f, new AnimationCurve(new Keyframe(0f, 0f), new Keyframe(0.5f, lastFrame), new Keyframe(1f, 0f)));
                sheet.cycleCount = 1;
                break;
            case 3: // Random
                sheet.timeMode = ParticleSystemAnimationTimeMode.Lifetime;
                sheet.frameOverTime = 0f;
                sheet.startFrame = new ParticleSystem.MinMaxCurve(0f, lastFrame);
                return;
            default: // Loop
                sheet.timeMode = ParticleSystemAnimationTimeMode.FPS;
                sheet.fps = (framerate.x + framerate.y) * 0.5f;
                break;
        }

        if (emitter.GetBool("FlipbookStartRandom")) {
            sheet.startFrame = new ParticleSystem.MinMaxCurve(0f, lastFrame);
        }
    }

    #endregion

    #region Trail, Beam, Light

    private void SetupTrail(RobloxInstance trail, GameObject gameObject) {
        Transform attachment0;
        Transform attachment1;
        if (!TryGetAttachments(trail, out attachment0, out attachment1)) {
            DestroyImmediate(gameObject);
            return;
        }
        trailCount++;

        // trail chay tu diem giua 2 attachment, be rong = khoang cach 2 attachment
        Vector3 span = attachment1.position - attachment0.position;
        gameObject.transform.position = (attachment0.position + attachment1.position) * 0.5f;
        if (span.sqrMagnitude > 0.000001f) gameObject.transform.rotation = Quaternion.FromToRotation(Vector3.up, span.normalized);

        TrailRenderer trailRenderer = gameObject.AddComponent<TrailRenderer>();
        trailRenderer.time = Mathf.Max(trail.GetFloat("Lifetime", 2f), 0.01f);
        trailRenderer.minVertexDistance = Mathf.Max(trail.GetFloat("MinLength", 0.1f) * studScale, 0.01f);
        trailRenderer.widthMultiplier = Mathf.Max(span.magnitude, 0.01f);
        trailRenderer.widthCurve = BuildCurve(trail.GetNumberSequence("WidthScale"), 0f, 1f, 1f);
        trailRenderer.colorGradient = BuildGradient(trail.GetColorSequence("Color"), trail.GetNumberSequence("Transparency"));
        trailRenderer.alignment = trail.GetBool("FaceCamera") ? LineAlignment.View : LineAlignment.TransformZ;
        trailRenderer.textureMode = trail.GetInt("TextureMode", 0) == 1 ? LineTextureMode.Tile : LineTextureMode.Stretch;
        trailRenderer.emitting = trail.GetBool("Enabled", true);
        trailRenderer.sharedMaterial = GetMaterial(trail);
        trailRenderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
        trailRenderer.receiveShadows = false;
    }

    private void SetupBeam(RobloxInstance beam, GameObject gameObject) {
        Transform attachment0;
        Transform attachment1;
        if (!TryGetAttachments(beam, out attachment0, out attachment1)) {
            DestroyImmediate(gameObject);
            return;
        }
        beamCount++;

        gameObject.transform.SetParent(attachment0, false);
        gameObject.transform.localPosition = Vector3.zero;
        gameObject.transform.localRotation = Quaternion.identity;

        // bezier: control point di theo truc X (Axis) cua attachment
        Vector3 point0 = attachment0.position;
        Vector3 point3 = attachment1.position;
        Vector3 point1 = point0 + attachment0.right * beam.GetFloat("CurveSize0") * studScale;
        Vector3 point2 = point3 - attachment1.right * beam.GetFloat("CurveSize1") * studScale;

        int segments = Mathf.Clamp(beam.GetInt("Segments", 10), 1, 100);
        LineRenderer lineRenderer = gameObject.AddComponent<LineRenderer>();
        lineRenderer.useWorldSpace = false;
        lineRenderer.positionCount = segments + 1;
        for (int i = 0; i <= segments; i++) {
            float t = (float)i / segments;
            float u = 1f - t;
            Vector3 point = u * u * u * point0 + 3f * u * u * t * point1 + 3f * u * t * t * point2 + t * t * t * point3;
            lineRenderer.SetPosition(i, gameObject.transform.InverseTransformPoint(point));
        }

        lineRenderer.widthMultiplier = 1f;
        lineRenderer.widthCurve = AnimationCurve.Linear(0f, beam.GetFloat("Width0", 1f) * studScale, 1f, beam.GetFloat("Width1", 1f) * studScale);
        lineRenderer.colorGradient = BuildGradient(beam.GetColorSequence("Color"), beam.GetNumberSequence("Transparency"));
        lineRenderer.alignment = beam.GetBool("FaceCamera") ? LineAlignment.View : LineAlignment.TransformZ;
        lineRenderer.textureMode = beam.GetInt("TextureMode", 0) == 1 ? LineTextureMode.Tile : LineTextureMode.Stretch;
        lineRenderer.enabled = beam.GetBool("Enabled", true);
        lineRenderer.sharedMaterial = GetMaterial(beam);
        lineRenderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
        lineRenderer.receiveShadows = false;

        if (Mathf.Abs(beam.GetFloat("TextureSpeed")) > 0.0001f) {
            warnings.Add("Beam '" + beam.name + "' co TextureSpeed, can them script/shader cuon texture");
        }
    }

    private void SetupLight(RobloxInstance roblox, GameObject gameObject) {
        lightCount++;
        Light light = gameObject.AddComponent<Light>();
        light.type = roblox.className == "SpotLight" ? LightType.Spot : LightType.Point;
        light.range = roblox.GetFloat("Range", 8f) * studScale;
        light.intensity = roblox.GetFloat("Brightness", 1f);
        light.color = roblox.GetColor3("Color", Color.white);
        light.shadows = LightShadows.None;
        light.enabled = roblox.GetBool("Enabled", true);

        if (light.type == LightType.Spot) {
            light.spotAngle = roblox.GetFloat("Angle", 90f);
            gameObject.transform.localRotation = Quaternion.LookRotation(NormalIdToUnity(roblox.GetInt("Face", 5)));
        }
    }

    private bool TryGetAttachments(RobloxInstance instance, out Transform attachment0, out Transform attachment1) {
        transformByReferent.TryGetValue(instance.GetRef("Attachment0"), out attachment0);
        transformByReferent.TryGetValue(instance.GetRef("Attachment1"), out attachment1);
        if (attachment0 != null && attachment1 != null) return true;

        warnings.Add(instance.className + " '" + instance.name + "' thieu Attachment0/Attachment1 trong file, da bo qua");
        return false;
    }

    #endregion

    #region Material

    // LightEmission cao -> additive, thap -> alpha blend
    private Material GetMaterial(RobloxInstance instance) {
        string textureUrl = instance.GetContent("Texture");
        bool isAdditive = instance.GetFloat("LightEmission") >= 0.5f;
        float brightness = Mathf.Max(instance.GetFloat("Brightness", 1f), 0f);

        Texture2D texture = FindTexture(textureUrl);
        string textureKey = texture != null ? texture.name : "Default";
        string key = textureKey + (isAdditive ? "_Add" : "_Alpha") + (Mathf.Approximately(brightness, 1f) ? "" : "_B" + brightness.ToString("0.##"));

        Material material;
        if (materialCache.TryGetValue(key, out material)) return material;

        EnsureFolder(outputFolder + "/Materials");
        string path = outputFolder + "/Materials/M_" + SanitizeFileName(key) + ".mat";
        material = AssetDatabase.LoadAssetAtPath<Material>(path);
        if (material == null) {
            material = new Material(particleShader);
            AssetDatabase.CreateAsset(material, path);
        } else {
            material.shader = particleShader;
        }

        SetupMaterial(material, texture, isAdditive, brightness);
        EditorUtility.SetDirty(material);
        materialCache[key] = material;
        return material;
    }

    private void SetupMaterial(Material material, Texture2D texture, bool isAdditive, float brightness) {
        if (texture == null) texture = AssetDatabase.GetBuiltinExtraResource<Texture2D>("Default-Particle.psd");

        material.SetTexture("_BaseMap", texture);
        material.SetColor("_BaseColor", Color.white * brightness);
        material.SetFloat("_Surface", 1f);
        material.SetFloat("_Blend", isAdditive ? 2f : 0f);
        material.SetFloat("_SrcBlend", (float)UnityEngine.Rendering.BlendMode.SrcAlpha);
        material.SetFloat("_DstBlend", isAdditive ? (float)UnityEngine.Rendering.BlendMode.One : (float)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
        material.SetFloat("_SrcBlendAlpha", (float)UnityEngine.Rendering.BlendMode.One);
        material.SetFloat("_DstBlendAlpha", isAdditive ? (float)UnityEngine.Rendering.BlendMode.One : (float)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
        material.SetFloat("_ZWrite", 0f);
        material.SetFloat("_Cull", 0f);
        material.SetOverrideTag("RenderType", "Transparent");
        material.EnableKeyword("_SURFACE_TYPE_TRANSPARENT");
        material.DisableKeyword("_ALPHAPREMULTIPLY_ON");
        material.DisableKeyword("_ALPHAMODULATE_ON");
        material.renderQueue = (int)UnityEngine.Rendering.RenderQueue.Transparent;
    }

    // tim texture trong Texture Folder theo asset id hoac ten file
    private Texture2D FindTexture(string textureUrl) {
        if (string.IsNullOrEmpty(textureUrl)) return null;

        string searchKey;
        Match idMatch = Regex.Match(textureUrl, @"(\d{3,})");
        if (textureUrl.StartsWith("rbxasset://")) searchKey = Path.GetFileNameWithoutExtension(textureUrl);
        else if (idMatch.Success) searchKey = idMatch.Groups[1].Value;
        else searchKey = Path.GetFileNameWithoutExtension(textureUrl);

        if (textureFolder != null) {
            string folderPath = AssetDatabase.GetAssetPath(textureFolder);
            if (Directory.Exists(folderPath)) {
                foreach (string file in Directory.GetFiles(folderPath, "*", SearchOption.AllDirectories)) {
                    string extension = Path.GetExtension(file).ToLowerInvariant();
                    if (System.Array.IndexOf(TEXTURE_EXTENSIONS, extension) < 0) continue;
                    if (!Path.GetFileNameWithoutExtension(file).Contains(searchKey)) continue;

                    Texture2D texture = AssetDatabase.LoadAssetAtPath<Texture2D>(file.Replace('\\', '/'));
                    if (texture != null) return texture;
                }
            }
        }

        missingTextures.Add(textureUrl);
        return null;
    }

    #endregion

    #region Helpers

    private static bool IsEffect(RobloxInstance instance) {
        switch (instance.className) {
            case "ParticleEmitter":
            case "Trail":
            case "Beam":
            case "PointLight":
            case "SpotLight":
                return true;
        }
        return false;
    }

    private static bool IsAttachment(RobloxInstance instance) {
        return instance.className == "Attachment" || instance.className == "Bone";
    }

    private static bool IsScript(RobloxInstance instance) {
        return instance.className == "Script" || instance.className == "LocalScript" || instance.className == "ModuleScript";
    }

    // NormalId: Right, Top, Back, Left, Bottom, Front (da doi sang truc Unity)
    private static Vector3 NormalIdToUnity(int normalId) {
        switch (normalId) {
            case 0: return Vector3.right;
            case 2: return Vector3.back;
            case 3: return Vector3.left;
            case 4: return Vector3.down;
            case 5: return Vector3.forward;
            default: return Vector3.up;
        }
    }

    // envelopeSign: -1 = duong duoi, 1 = duong tren, 0 = gia tri goc
    private static AnimationCurve BuildCurve(List<RobloxNumberKey> keys, float envelopeSign, float divisor, float fallback = 1f) {
        AnimationCurve curve = new AnimationCurve();
        if (keys.Count == 0) {
            curve.AddKey(0f, fallback);
            curve.AddKey(1f, fallback);
            return curve;
        }

        foreach (RobloxNumberKey key in keys) {
            curve.AddKey(Mathf.Clamp01(key.time), Mathf.Max(key.value + key.envelope * envelopeSign, 0f) / divisor);
        }
        for (int i = 0; i < curve.length; i++) {
            AnimationUtility.SetKeyLeftTangentMode(curve, i, AnimationUtility.TangentMode.Linear);
            AnimationUtility.SetKeyRightTangentMode(curve, i, AnimationUtility.TangentMode.Linear);
        }
        return curve;
    }

    // Gradient Unity toi da 8 key, sequence dai hon thi lay mau deu
    private static Gradient BuildGradient(List<RobloxColorKey> colors, List<RobloxNumberKey> transparency) {
        if (colors.Count == 0) colors = new List<RobloxColorKey> { new RobloxColorKey { time = 0f, color = Color.white }, new RobloxColorKey { time = 1f, color = Color.white } };
        if (transparency.Count == 0) transparency = new List<RobloxNumberKey> { new RobloxNumberKey { time = 0f }, new RobloxNumberKey { time = 1f } };

        GradientColorKey[] colorKeys;
        if (colors.Count <= MAX_GRADIENT_KEYS) {
            colorKeys = new GradientColorKey[colors.Count];
            for (int i = 0; i < colors.Count; i++) {
                colorKeys[i] = new GradientColorKey(colors[i].color, colors[i].time);
            }
        } else {
            colorKeys = new GradientColorKey[MAX_GRADIENT_KEYS];
            for (int i = 0; i < MAX_GRADIENT_KEYS; i++) {
                float time = (float)i / (MAX_GRADIENT_KEYS - 1);
                colorKeys[i] = new GradientColorKey(EvaluateColor(colors, time), time);
            }
        }

        GradientAlphaKey[] alphaKeys;
        if (transparency.Count <= MAX_GRADIENT_KEYS) {
            alphaKeys = new GradientAlphaKey[transparency.Count];
            for (int i = 0; i < transparency.Count; i++) {
                alphaKeys[i] = new GradientAlphaKey(1f - Mathf.Clamp01(transparency[i].value), transparency[i].time);
            }
        } else {
            alphaKeys = new GradientAlphaKey[MAX_GRADIENT_KEYS];
            for (int i = 0; i < MAX_GRADIENT_KEYS; i++) {
                float time = (float)i / (MAX_GRADIENT_KEYS - 1);
                alphaKeys[i] = new GradientAlphaKey(1f - Mathf.Clamp01(EvaluateNumber(transparency, time)), time);
            }
        }

        Gradient gradient = new Gradient();
        gradient.SetKeys(colorKeys, alphaKeys);
        return gradient;
    }

    private static float EvaluateNumber(List<RobloxNumberKey> keys, float time) {
        for (int i = 1; i < keys.Count; i++) {
            if (time <= keys[i].time) {
                float t = Mathf.InverseLerp(keys[i - 1].time, keys[i].time, time);
                return Mathf.Lerp(keys[i - 1].value, keys[i].value, t);
            }
        }
        return keys[keys.Count - 1].value;
    }

    private static Color EvaluateColor(List<RobloxColorKey> keys, float time) {
        for (int i = 1; i < keys.Count; i++) {
            if (time <= keys[i].time) {
                float t = Mathf.InverseLerp(keys[i - 1].time, keys[i].time, time);
                return Color.Lerp(keys[i - 1].color, keys[i].color, t);
            }
        }
        return keys[keys.Count - 1].color;
    }

    private static string SanitizeFileName(string fileName) {
        foreach (char invalid in Path.GetInvalidFileNameChars()) {
            fileName = fileName.Replace(invalid, '_');
        }
        return fileName;
    }

    private static void EnsureFolder(string folderPath) {
        if (AssetDatabase.IsValidFolder(folderPath)) return;

        string parent = Path.GetDirectoryName(folderPath).Replace('\\', '/');
        EnsureFolder(parent);
        AssetDatabase.CreateFolder(parent, Path.GetFileName(folderPath));
    }

    private void BuildReport(string prefabPath) {
        StringBuilder builder = new StringBuilder();
        builder.AppendLine("Roblox VFX converted: " + prefabPath);
        builder.AppendLine("ParticleEmitter: " + emitterCount + ", Trail: " + trailCount + ", Beam: " + beamCount + ", Light: " + lightCount);

        if (missingTextures.Count > 0) {
            builder.AppendLine();
            builder.AppendLine("Thieu texture (dang dung Default-Particle), them file anh vao Texture Folder roi convert lai:");
            foreach (string texture in missingTextures) {
                builder.AppendLine("  " + texture);
            }
        }

        if (warnings.Count > 0) {
            builder.AppendLine();
            builder.AppendLine("Luu y:");
            foreach (string warning in warnings) {
                builder.AppendLine("  " + warning);
            }
        }
        report = builder.ToString();
    }

    #endregion
}
