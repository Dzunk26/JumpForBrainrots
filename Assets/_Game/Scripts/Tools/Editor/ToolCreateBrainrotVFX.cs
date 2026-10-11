using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[System.Serializable]
public class BrainrotVFXEntry {
    public BrainrotSO brainrotSO;
    public bool isSelected = true;
    public bool hasVFX;
    public BrainrotVFXTheme theme = BrainrotVFXTheme.Auto;
    public BrainrotVFXTheme autoTheme = BrainrotVFXTheme.Sparkle;
    public Color primaryColor = Color.white;
    public Color secondaryColor = Color.white;
    public Color autoPrimaryColor = Color.white;
    public Color autoSecondaryColor = Color.white;
    public Bounds modelBounds;

    public BrainrotVFXTheme GetResolvedTheme() {
        return theme == BrainrotVFXTheme.Auto ? autoTheme : theme;
    }

    public string GetName() {
        string brainrotName = brainrotSO.GetName();
        return string.IsNullOrEmpty(brainrotName) ? brainrotSO.name : brainrotName;
    }
}

// tao VFX cho tung brainrot bang cach ghep cac prefab trong pack VFX theo thu vien (BrainrotVFXLibrarySO)
// tu doan theme theo ten + mau texture, rarity quyet dinh bo hieu ung, tu khoa ten them hieu ung rieng
// VFX luu thanh prefab rieng roi gan vao lam con cua prefab model (bu lai scale 100 cua root model)
public class ToolCreateBrainrotVFX : EditorWindow {
    private static string DEFAULT_SO_FOLDER = "Assets/_Game/ScriptableObjects/BrainrotSOs";
    private static string PREFABS_FOLDER = "Assets/_Game/Prefabs";
    private static string VFX_PREFABS_FOLDER_NAME = "BrainrotVFX";
    private static string PREVIEW_ROOT_NAME = "Brainrot VFX Preview";
    private static string DEFAULT_LIBRARY_PATH = "Assets/_Game/VFX/Brainrot VFX Library.asset";
    private static string[] VFX_PACK_FOLDERS = new string[] { "Assets/Hovl Studio", "Assets/Epic Toon FX", "Assets/Archanor", "Assets/Voxel Arsenal" };

    [SerializeField] private DefaultAsset soFolder;
    [SerializeField] private BrainrotVFXLibrarySO library;
    [SerializeField] private float sizeMultiplier = 1f;
    [SerializeField] private float rateMultiplier = 1f;
    [SerializeField] private bool isAttachToModel = true;
    [SerializeField] private int rarityFilterIndex;
    [SerializeField] private List<BrainrotVFXEntry> entries = new List<BrainrotVFXEntry>();

    private Vector2 scrollPosition;
    private string[] rarityFilterOptions;

    private void OnEnable() {
        if (soFolder == null) {
            soFolder = AssetDatabase.LoadAssetAtPath<DefaultAsset>(DEFAULT_SO_FOLDER);
        }

        if (library == null) {
            library = AssetDatabase.LoadAssetAtPath<BrainrotVFXLibrarySO>(DEFAULT_LIBRARY_PATH);
        }

        List<string> options = new List<string> { "All" };
        options.AddRange(System.Enum.GetNames(typeof(BrainrotRarity)));
        rarityFilterOptions = options.ToArray();
    }

    private void OnGUI() {
        soFolder = (DefaultAsset)EditorGUILayout.ObjectField("SO Folder", soFolder, typeof(DefaultAsset), false);

        EditorGUILayout.BeginHorizontal();
        library = (BrainrotVFXLibrarySO)EditorGUILayout.ObjectField("VFX Library", library, typeof(BrainrotVFXLibrarySO), false);
        if (library == null && GUILayout.Button("Create Default", GUILayout.Width(110))) {
            CreateDefaultLibrary();
            GUIUtility.ExitGUI();
        }

        if (library != null && GUILayout.Button("Reset To Default", GUILayout.Width(110))) {
            ResetLibrary();
            GUIUtility.ExitGUI();
        }
        EditorGUILayout.EndHorizontal();

        if (GUILayout.Button("Upgrade VFX Pack Materials To URP")) {
            UpgradePackMaterials();
            GUIUtility.ExitGUI();
        }

        sizeMultiplier = EditorGUILayout.Slider("Size Multiplier", sizeMultiplier, 0.5f, 2f);
        rateMultiplier = EditorGUILayout.Slider("Particle Amount", rateMultiplier, 0.3f, 2f);
        isAttachToModel = EditorGUILayout.Toggle("Attach To Model Prefab", isAttachToModel);

        EditorGUILayout.Space();

        if (GUILayout.Button("Load + Analyze Brainrots", GUILayout.Height(26))) {
            LoadEntries();
            GUIUtility.ExitGUI();
        }

        if (entries.Count == 0) {
            EditorGUILayout.HelpBox("Bam Load + Analyze de doc tat ca BrainrotSO, tool se tu doan theme va mau tu model", MessageType.Info);
            return;
        }

        EditorGUILayout.Space();

        EditorGUILayout.BeginHorizontal();
        rarityFilterIndex = EditorGUILayout.Popup("Rarity Filter", rarityFilterIndex, rarityFilterOptions);
        if (GUILayout.Button("Select All", GUILayout.Width(80))) {
            SetSelectedVisible(true);
        }

        if (GUILayout.Button("Select None", GUILayout.Width(90))) {
            SetSelectedVisible(false);
        }
        EditorGUILayout.EndHorizontal();

        DrawHeader();

        scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);
        foreach (BrainrotVFXEntry entry in entries) {
            if (!IsMatchFilter(entry)) continue;

            DrawEntry(entry);
        }
        EditorGUILayout.EndScrollView();

        EditorGUILayout.Space();

        int targetCount = GetTargetEntries().Count;
        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("Create VFX (" + targetCount + ")", GUILayout.Height(30))) {
            CreateSelectedVFX();
            GUIUtility.ExitGUI();
        }

        if (GUILayout.Button("Remove VFX From Model (" + targetCount + ")", GUILayout.Height(30))) {
            RemoveSelectedVFX();
            GUIUtility.ExitGUI();
        }
        EditorGUILayout.EndHorizontal();

        if (GUILayout.Button("Spawn Preview In Scene (" + targetCount + ")")) {
            SpawnPreview();
            GUIUtility.ExitGUI();
        }
    }

    [MenuItem("Tools/VFX/Brainrot VFX Creator")]
    private static void OpenWindow() {
        GetWindow<ToolCreateBrainrotVFX>("Brainrot VFX Creator");
    }

    private void DrawHeader() {
        EditorGUILayout.BeginHorizontal();
        GUILayout.Space(20);
        EditorGUILayout.LabelField("Brainrot", EditorStyles.miniBoldLabel, GUILayout.Width(200));
        EditorGUILayout.LabelField("Rarity", EditorStyles.miniBoldLabel, GUILayout.Width(70));
        EditorGUILayout.LabelField("Theme", EditorStyles.miniBoldLabel, GUILayout.Width(155));
        EditorGUILayout.LabelField("Colors", EditorStyles.miniBoldLabel, GUILayout.Width(100));
        EditorGUILayout.EndHorizontal();
    }

    private void DrawEntry(BrainrotVFXEntry entry) {
        BrainrotRarity rarity = entry.brainrotSO.GetRarity();

        EditorGUILayout.BeginHorizontal();
        entry.isSelected = EditorGUILayout.Toggle(entry.isSelected, GUILayout.Width(16));
        EditorGUILayout.LabelField(entry.GetName() + (entry.hasVFX ? "  [VFX]" : ""), GUILayout.Width(200));

        Color previousColor = GUI.contentColor;
        GUI.contentColor = GetRarityLabelColor(rarity);
        EditorGUILayout.LabelField(rarity.ToString(), EditorStyles.boldLabel, GUILayout.Width(70));
        GUI.contentColor = previousColor;

        entry.theme = (BrainrotVFXTheme)EditorGUILayout.EnumPopup(entry.theme, GUILayout.Width(80));
        EditorGUILayout.LabelField(entry.theme == BrainrotVFXTheme.Auto ? "-> " + entry.autoTheme : "", GUILayout.Width(70));

        entry.primaryColor = EditorGUILayout.ColorField(GUIContent.none, entry.primaryColor, true, false, false, GUILayout.Width(45));
        entry.secondaryColor = EditorGUILayout.ColorField(GUIContent.none, entry.secondaryColor, true, false, false, GUILayout.Width(45));

        if (GUILayout.Button("Reset", GUILayout.Width(45))) {
            entry.theme = BrainrotVFXTheme.Auto;
            entry.primaryColor = entry.autoPrimaryColor;
            entry.secondaryColor = entry.autoSecondaryColor;
        }

        if (GUILayout.Button("Ping", GUILayout.Width(40)) && entry.brainrotSO.GetBrainrotModel() != null) {
            EditorGUIUtility.PingObject(entry.brainrotSO.GetBrainrotModel().gameObject);
        }
        EditorGUILayout.EndHorizontal();
    }

    // ===== thu vien =====

    private void CreateDefaultLibrary() {
        library = CreateInstance<BrainrotVFXLibrarySO>();
        library.ResetToDefault();
        AssetDatabase.CreateAsset(library, DEFAULT_LIBRARY_PATH);
        AssetDatabase.SaveAssets();

        EditorGUIUtility.PingObject(library);
        Debug.Log("Created VFX library: " + DEFAULT_LIBRARY_PATH);
    }

    private void ResetLibrary() {
        if (!EditorUtility.DisplayDialog("Reset VFX Library", "Moi chinh sua trong thu vien se mat, tiep tuc?", "Reset", "Cancel")) return;

        Undo.RecordObject(library, "Reset Brainrot VFX Library");
        library.ResetToDefault();
        EditorUtility.SetDirty(library);
        AssetDatabase.SaveAssets();
    }

    // material built-in cua pack hien hong / sai mau tren URP, Create VFX da tu chuyen phan duoc dung,
    // nut nay chuyen het de xem demo scene cua pack cung dung
    private void UpgradePackMaterials() {
        List<string> folders = new List<string>();
        foreach (string folder in VFX_PACK_FOLDERS) {
            if (AssetDatabase.IsValidFolder(folder)) {
                folders.Add(folder);
            }
        }

        int upgradedCount = BrainrotVFXMaterialUpgrader.UpgradeFolders(folders.ToArray());
        Debug.Log("Upgraded " + upgradedCount + " VFX materials to URP");
    }

    // ===== load + phan tich =====

    private void LoadEntries() {
        if (soFolder == null) {
            Debug.LogError("SO folder is missing");
            return;
        }

        List<BrainrotSO> brainrotSOs = new List<BrainrotSO>();
        string[] soGuids = AssetDatabase.FindAssets("t:BrainrotSO", new string[] { AssetDatabase.GetAssetPath(soFolder) });
        foreach (string soGuid in soGuids) {
            BrainrotSO brainrotSO = AssetDatabase.LoadAssetAtPath<BrainrotSO>(AssetDatabase.GUIDToAssetPath(soGuid));
            if (brainrotSO == null) continue;

            brainrotSOs.Add(brainrotSO);
        }

        brainrotSOs.Sort(CompareBrainrotSO);
        entries.Clear();

        try {
            for (int i = 0; i < brainrotSOs.Count; i++) {
                EditorUtility.DisplayProgressBar("Analyzing Brainrots", brainrotSOs[i].name, (float)i / brainrotSOs.Count);

                BrainrotVFXEntry entry = CreateEntry(brainrotSOs[i]);
                if (entry == null) continue;

                entries.Add(entry);
            }
        }
        finally {
            EditorUtility.ClearProgressBar();
        }

        Debug.Log("Analyzed " + entries.Count + " brainrots");
    }

    // neu model da co VFX thi lay lai theme + mau da chinh tay lan truoc
    private BrainrotVFXEntry CreateEntry(BrainrotSO brainrotSO) {
        BrainrotModel model = brainrotSO.GetBrainrotModel();
        if (model == null) {
            Debug.LogWarning("Brainrot model is missing: " + brainrotSO.name);
            return null;
        }

        BrainrotVFXEntry entry = new BrainrotVFXEntry();
        entry.brainrotSO = brainrotSO;

        GameObject contents = PrefabUtility.LoadPrefabContents(AssetDatabase.GetAssetPath(model));
        try {
            BrainrotVFXAnalysis analysis = BrainrotVFXAnalyzer.Analyze(contents);
            entry.modelBounds = analysis.bounds;
            entry.autoPrimaryColor = analysis.primaryColor;
            entry.autoSecondaryColor = analysis.secondaryColor;
            entry.autoTheme = BrainrotVFXAnalyzer.DetectTheme(entry.GetName() + " " + model.name, analysis);
            entry.primaryColor = analysis.primaryColor;
            entry.secondaryColor = analysis.secondaryColor;

            BrainrotVFX existingVFX = contents.GetComponentInChildren<BrainrotVFX>(true);
            if (existingVFX == null) {
                GameObject vfxPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(GetVFXPrefabPath(model));
                existingVFX = vfxPrefab != null ? vfxPrefab.GetComponent<BrainrotVFX>() : null;
            }

            entry.hasVFX = existingVFX != null;
            if (existingVFX != null) {
                entry.theme = existingVFX.GetTheme();
                entry.primaryColor = existingVFX.GetPrimaryColor();
                entry.secondaryColor = existingVFX.GetSecondaryColor();
            }
        }
        finally {
            PrefabUtility.UnloadPrefabContents(contents);
        }

        return entry;
    }

    // ===== tao / xoa VFX =====

    private void CreateSelectedVFX() {
        List<BrainrotVFXEntry> targetEntries = GetTargetEntries();
        if (targetEntries.Count == 0) {
            Debug.LogWarning("No brainrot selected");
            return;
        }

        if (library == null) {
            Debug.LogError("VFX library is missing, press Create Default first");
            return;
        }

        string vfxFolder = GetOrCreateFolder(PREFABS_FOLDER, VFX_PREFABS_FOLDER_NAME);
        int createdCount = 0;

        try {
            for (int i = 0; i < targetEntries.Count; i++) {
                EditorUtility.DisplayProgressBar("Creating Brainrot VFX", targetEntries[i].GetName(), (float)i / targetEntries.Count);

                if (CreateVFX(targetEntries[i], vfxFolder)) {
                    createdCount++;
                }
            }
        }
        finally {
            EditorUtility.ClearProgressBar();
        }

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Debug.Log("Created VFX for " + createdCount + " brainrots");
    }

    private bool CreateVFX(BrainrotVFXEntry entry, string vfxFolder) {
        BrainrotModel model = entry.brainrotSO.GetBrainrotModel();
        if (model == null) {
            Debug.LogWarning("Brainrot model is missing: " + entry.brainrotSO.name);
            return false;
        }

        BrainrotVFXRequest request = new BrainrotVFXRequest();
        request.vfxName = model.name + " VFX";
        request.brainrotName = entry.GetName() + " " + model.name;
        request.rarity = entry.brainrotSO.GetRarity();
        request.theme = entry.theme;
        request.resolvedTheme = entry.GetResolvedTheme();
        request.primaryColor = entry.primaryColor;
        request.secondaryColor = entry.secondaryColor;
        request.modelBounds = entry.modelBounds;
        request.sizeMultiplier = sizeMultiplier;
        request.rateMultiplier = rateMultiplier;

        GameObject vfxRoot = BrainrotVFXBuilder.Build(request, library);
        GameObject vfxPrefab = PrefabUtility.SaveAsPrefabAsset(vfxRoot, vfxFolder + "/" + request.vfxName + ".prefab");
        DestroyImmediate(vfxRoot);

        if (isAttachToModel) {
            AttachToModel(AssetDatabase.GetAssetPath(model), vfxPrefab);
        }

        entry.hasVFX = true;
        return true;
    }

    // VFX dung theo kich thuoc that (root model scale 100) nen con VFX phai scale 1/100 de bu lai
    private void AttachToModel(string modelPath, GameObject vfxPrefab) {
        GameObject contents = PrefabUtility.LoadPrefabContents(modelPath);
        try {
            RemoveVFX(contents);

            GameObject vfxInstance = (GameObject)PrefabUtility.InstantiatePrefab(vfxPrefab, contents.transform);
            Vector3 rootScale = contents.transform.localScale;
            vfxInstance.transform.localPosition = Vector3.zero;
            vfxInstance.transform.localRotation = Quaternion.identity;
            vfxInstance.transform.localScale = new Vector3(GetInverseScale(rootScale.x), GetInverseScale(rootScale.y), GetInverseScale(rootScale.z));

            PrefabUtility.SaveAsPrefabAsset(contents, modelPath);
        }
        finally {
            PrefabUtility.UnloadPrefabContents(contents);
        }
    }

    // chi go VFX khoi prefab model, prefab VFX rieng van giu lai
    private void RemoveSelectedVFX() {
        List<BrainrotVFXEntry> targetEntries = GetTargetEntries();
        int removedCount = 0;

        foreach (BrainrotVFXEntry entry in targetEntries) {
            BrainrotModel model = entry.brainrotSO.GetBrainrotModel();
            if (model == null) continue;

            string modelPath = AssetDatabase.GetAssetPath(model);
            GameObject contents = PrefabUtility.LoadPrefabContents(modelPath);
            try {
                if (RemoveVFX(contents)) {
                    PrefabUtility.SaveAsPrefabAsset(contents, modelPath);
                    removedCount++;
                }
            }
            finally {
                PrefabUtility.UnloadPrefabContents(contents);
            }

            entry.hasVFX = false;
        }

        AssetDatabase.SaveAssets();
        Debug.Log("Removed VFX from " + removedCount + " brainrot models");
    }

    private bool RemoveVFX(GameObject contents) {
        BrainrotVFX[] vfxs = contents.GetComponentsInChildren<BrainrotVFX>(true);
        foreach (BrainrotVFX vfx in vfxs) {
            DestroyImmediate(vfx.gameObject);
        }

        return vfxs.Length > 0;
    }

    // ===== preview =====

    // xep model theo hang rarity (truc Z) de so sanh, bam Play de xem particle + qua cau chay
    private void SpawnPreview() {
        List<BrainrotVFXEntry> targetEntries = GetTargetEntries();
        if (targetEntries.Count == 0) return;

        GameObject oldPreview = GameObject.Find(PREVIEW_ROOT_NAME);
        if (oldPreview != null) {
            Undo.DestroyObjectImmediate(oldPreview);
        }

        GameObject previewRoot = new GameObject(PREVIEW_ROOT_NAME);
        Undo.RegisterCreatedObjectUndo(previewRoot, "Spawn Brainrot VFX Preview");

        float spacing = 1f;
        foreach (BrainrotVFXEntry entry in targetEntries) {
            spacing = Mathf.Max(spacing, Mathf.Max(entry.modelBounds.size.x, entry.modelBounds.size.z) * 2.6f * sizeMultiplier);
        }

        Dictionary<BrainrotRarity, int> columnCounts = new Dictionary<BrainrotRarity, int>();
        foreach (BrainrotVFXEntry entry in targetEntries) {
            BrainrotModel model = entry.brainrotSO.GetBrainrotModel();
            if (model == null) continue;

            BrainrotRarity rarity = entry.brainrotSO.GetRarity();
            columnCounts.TryGetValue(rarity, out int column);
            columnCounts[rarity] = column + 1;

            GameObject modelInstance = (GameObject)PrefabUtility.InstantiatePrefab(model.gameObject, previewRoot.transform);
            modelInstance.transform.localPosition = new Vector3(column * spacing, 0f, (int)rarity * spacing);

            // VFX khong gan vao model thi sinh kem de xem
            if (!isAttachToModel && modelInstance.GetComponentInChildren<BrainrotVFX>(true) == null) {
                GameObject vfxPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(GetVFXPrefabPath(model));
                if (vfxPrefab != null) {
                    GameObject vfxInstance = (GameObject)PrefabUtility.InstantiatePrefab(vfxPrefab, modelInstance.transform);
                    Vector3 rootScale = modelInstance.transform.localScale;
                    vfxInstance.transform.localScale = new Vector3(GetInverseScale(rootScale.x), GetInverseScale(rootScale.y), GetInverseScale(rootScale.z));
                }
            }
        }

        Selection.activeGameObject = previewRoot;
        SceneView.FrameLastActiveSceneView();
    }

    // ===== helper =====

    private List<BrainrotVFXEntry> GetTargetEntries() {
        List<BrainrotVFXEntry> targetEntries = new List<BrainrotVFXEntry>();
        foreach (BrainrotVFXEntry entry in entries) {
            if (entry.brainrotSO == null || !entry.isSelected || !IsMatchFilter(entry)) continue;

            targetEntries.Add(entry);
        }

        return targetEntries;
    }

    private bool IsMatchFilter(BrainrotVFXEntry entry) {
        if (entry.brainrotSO == null) return false;
        if (rarityFilterIndex == 0) return true;

        return entry.brainrotSO.GetRarity() == (BrainrotRarity)(rarityFilterIndex - 1);
    }

    private void SetSelectedVisible(bool isSelected) {
        foreach (BrainrotVFXEntry entry in entries) {
            if (!IsMatchFilter(entry)) continue;

            entry.isSelected = isSelected;
        }
    }

    private string GetVFXPrefabPath(BrainrotModel model) {
        return PREFABS_FOLDER + "/" + VFX_PREFABS_FOLDER_NAME + "/" + model.name + " VFX.prefab";
    }

    private Color GetRarityLabelColor(BrainrotRarity rarity) {
        return BrainrotVFXColors.GetRarityColor(rarity);
    }

    private string GetOrCreateFolder(string parentFolder, string folderName) {
        string folderPath = parentFolder + "/" + folderName;
        if (!AssetDatabase.IsValidFolder(folderPath)) {
            AssetDatabase.CreateFolder(parentFolder, folderName);
        }

        return folderPath;
    }

    private float GetInverseScale(float scale) {
        if (Mathf.Abs(scale) < 0.0001f) return 1f;

        return 1f / scale;
    }

    private int CompareBrainrotSO(BrainrotSO a, BrainrotSO b) {
        int rarityCompare = ((int)a.GetRarity()).CompareTo((int)b.GetRarity());
        if (rarityCompare != 0) return rarityCompare;

        return string.CompareOrdinal(a.name, b.name);
    }
}
