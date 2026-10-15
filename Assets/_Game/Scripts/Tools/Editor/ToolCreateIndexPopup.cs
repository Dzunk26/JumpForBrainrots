using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

// dung prefab CanvasIndex (Resources/UI) + IndexSlot (Prefabs/UI) bang code
// anh nen panel, nut... la placeholder lay tu built-in UI sprite, sau nay tu thay
public class ToolCreateIndexPopup : EditorWindow {
    private static string DEFAULT_LIST_BRAINROT_SO_PATH = "Assets/_Game/ScriptableObjects/BrainrotSOs/List Brainrot SO.asset";
    private static string DEFAULT_MATERIAL_FOLDER = "Assets/_Game/Materials/RarityBackground";
    private static string DEFAULT_FONT_PATH = "Assets/_Game/Fonts/Fredoka-Bold SDF.asset";
    private static string MATERIAL_PREFIX = "RarityBG_";
    private static string ITEM_PREFAB_FOLDER = "Assets/_Game/Prefabs/UI";
    private static string CANVAS_PREFAB_FOLDER = "Assets/_Game/Resources/UI";
    private static string BUILTIN_ROUND_SPRITE = "UI/Skin/UISprite.psd";

    [SerializeField] private ListBrainrotSO listBrainrotSO;
    [SerializeField] private DefaultAsset materialFolder;
    [SerializeField] private TMP_FontAsset font;

    private Sprite roundSprite;

    private void OnEnable() {
        if (listBrainrotSO == null) {
            listBrainrotSO = AssetDatabase.LoadAssetAtPath<ListBrainrotSO>(DEFAULT_LIST_BRAINROT_SO_PATH);
        }

        if (materialFolder == null) {
            materialFolder = AssetDatabase.LoadAssetAtPath<DefaultAsset>(DEFAULT_MATERIAL_FOLDER);
        }

        if (font == null) {
            font = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(DEFAULT_FONT_PATH);
        }
    }

    private void OnGUI() {
        listBrainrotSO = (ListBrainrotSO)EditorGUILayout.ObjectField("List Brainrot SO", listBrainrotSO, typeof(ListBrainrotSO), false);
        materialFolder = (DefaultAsset)EditorGUILayout.ObjectField("Rarity Material Folder", materialFolder, typeof(DefaultAsset), false);
        font = (TMP_FontAsset)EditorGUILayout.ObjectField("Font", font, typeof(TMP_FontAsset), false);

        EditorGUILayout.Space();
        EditorGUILayout.HelpBox("Tao (ghi de) " + ITEM_PREFAB_FOLDER + "/IndexSlot.prefab va " + CANVAS_PREFAB_FOLDER + "/CanvasIndex.prefab", MessageType.Info);

        if (GUILayout.Button("Create Index Popup")) {
            CreateIndexPopup();
        }
    }

    [MenuItem("Tools/UI/Index Popup Creator")]
    private static void OpenWindow() {
        GetWindow<ToolCreateIndexPopup>("Index Popup Creator");
    }

    private void CreateIndexPopup() {
        if (listBrainrotSO == null || materialFolder == null) {
            Debug.LogError("List brainrot SO or material folder is missing");
            return;
        }

        roundSprite = AssetDatabase.GetBuiltinExtraResource<Sprite>(BUILTIN_ROUND_SPRITE);
        EnsureFolder(ITEM_PREFAB_FOLDER);
        EnsureFolder(CANVAS_PREFAB_FOLDER);

        IndexSlotUI slotPrefab = CreateSlotPrefab();
        GameObject canvasPrefab = CreateCanvasPrefab(slotPrefab);

        AssetDatabase.SaveAssets();
        Selection.activeObject = canvasPrefab;
        EditorGUIUtility.PingObject(canvasPrefab);
        Debug.Log("Created index popup at " + AssetDatabase.GetAssetPath(canvasPrefab));
    }

    // o brainrot trong grid: nen material + rarity + icon + ten
    private IndexSlotUI CreateSlotPrefab() {
        GameObject root = CreateUIObject("IndexSlot", null);
        SetRect(root, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(260f, 260f));

        GameObject background = CreateUIObject("Background", root.transform);
        Stretch(background, 0f, 0f, 0f, 0f);
        Image backgroundImage = AddImage(background, null, Color.white, false);
        backgroundImage.material = GetRarityMaterial(BrainrotRarity.Basic);

        GameObject rarityObject = CreateUIObject("RarityText", root.transform);
        SetOffsets(rarityObject, new Vector2(0f, 1f), new Vector2(1f, 1f), new Vector2(8f, -48f), new Vector2(-8f, -6f));
        TextMeshProUGUI rarityText = AddText(rarityObject, "Basic", 30f, TextAlignmentOptions.Center);

        GameObject icon = CreateUIObject("Icon", root.transform);
        Stretch(icon, 14f, 14f, 50f, 50f);
        Image iconImage = AddImage(icon, null, Color.black, false);
        iconImage.preserveAspect = true;

        GameObject nameObject = CreateUIObject("NameText", root.transform);
        SetOffsets(nameObject, new Vector2(0f, 0f), new Vector2(1f, 0f), new Vector2(8f, 6f), new Vector2(-8f, 48f));
        TextMeshProUGUI nameText = AddText(nameObject, "Brainrot Name", 24f, TextAlignmentOptions.Center);
        nameText.enableAutoSizing = true;
        nameText.fontSizeMin = 14f;
        nameText.fontSizeMax = 26f;

        IndexSlotUI slot = root.AddComponent<IndexSlotUI>();
        SetReference(slot, "backgroundImage", backgroundImage);
        SetReference(slot, "iconImage", iconImage);
        SetReference(slot, "rarityText", rarityText);
        SetReference(slot, "nameText", nameText);

        return SavePrefab(root, ITEM_PREFAB_FOLDER + "/IndexSlot.prefab").GetComponent<IndexSlotUI>();
    }

    // CanvasIndex
    // |- Blocker
    // \- Panel
    //   |- Header (IndexIcon, TitleText, TotalProgressText, CloseButton)
    //   \- ScrollView > Viewport > Content (grid slot, tat ca rarity)
    private GameObject CreateCanvasPrefab(IndexSlotUI slotPrefab) {
        GameObject root = CreateUIObject("CanvasIndex", null);
        Canvas canvas = root.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 10;
        CanvasScaler canvasScaler = root.AddComponent<CanvasScaler>();
        canvasScaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        canvasScaler.referenceResolution = new Vector2(1920f, 1080f);
        canvasScaler.matchWidthOrHeight = 0.5f;
        root.AddComponent<GraphicRaycaster>();

        GameObject blocker = CreateUIObject("Blocker", root.transform);
        Stretch(blocker, 0f, 0f, 0f, 0f);
        AddImage(blocker, null, new Color(0f, 0f, 0f, 0.6f), true);

        GameObject panel = CreateUIObject("Panel", root.transform);
        SetRect(panel, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(1500f, 880f));
        AddImage(panel, roundSprite, new Color(0.17f, 0.2f, 0.3f, 1f), true);

        // header
        GameObject header = CreateUIObject("Header", panel.transform);
        SetOffsets(header, new Vector2(0f, 1f), new Vector2(1f, 1f), new Vector2(0f, -110f), Vector2.zero);
        AddImage(header, roundSprite, new Color(0.1f, 0.12f, 0.2f, 1f), false);

        GameObject headerIcon = CreateUIObject("IndexIcon", header.transform);
        SetRect(headerIcon, new Vector2(0f, 0.5f), new Vector2(0f, 0.5f), new Vector2(0f, 0.5f), new Vector2(25f, 0f), new Vector2(80f, 80f));
        AddImage(headerIcon, roundSprite, Color.white, false);

        GameObject title = CreateUIObject("TitleText", header.transform);
        SetAnchors(title, new Vector2(0f, 0f), new Vector2(0.5f, 1f), 120f, 0f, 0f, 0f);
        AddText(title, "Index", 64f, TextAlignmentOptions.MidlineLeft);

        GameObject totalProgress = CreateUIObject("TotalProgressText", header.transform);
        SetAnchors(totalProgress, new Vector2(0.5f, 0f), new Vector2(1f, 1f), 0f, 120f, 0f, 0f);
        TextMeshProUGUI totalProgressText = AddText(totalProgress, "0/0", 40f, TextAlignmentOptions.MidlineRight);

        GameObject closeObject = CreateUIObject("CloseButton", header.transform);
        SetRect(closeObject, new Vector2(1f, 0.5f), new Vector2(1f, 0.5f), new Vector2(1f, 0.5f), new Vector2(-15f, 0f), new Vector2(84f, 84f));
        Image closeImage = AddImage(closeObject, roundSprite, new Color(0.9f, 0.25f, 0.25f, 1f), true);
        Button closeButton = closeObject.AddComponent<Button>();
        closeButton.targetGraphic = closeImage;
        GameObject closeLabel = CreateUIObject("Text", closeObject.transform);
        Stretch(closeLabel, 0f, 0f, 0f, 0f);
        AddText(closeLabel, "X", 52f, TextAlignmentOptions.Center);

        // grid brainrot co scroll
        GameObject scrollView = CreateUIObject("ScrollView", panel.transform);
        Stretch(scrollView, 20f, 20f, 130f, 20f);
        AddImage(scrollView, roundSprite, new Color(0f, 0f, 0f, 0.25f), true);
        ScrollRect scrollRect = scrollView.AddComponent<ScrollRect>();

        GameObject viewport = CreateUIObject("Viewport", scrollView.transform);
        Stretch(viewport, 0f, 0f, 0f, 0f);
        viewport.AddComponent<RectMask2D>();

        GameObject content = CreateUIObject("Content", viewport.transform);
        SetOffsets(content, new Vector2(0f, 1f), new Vector2(1f, 1f), Vector2.zero, Vector2.zero);
        RectTransform contentRect = content.GetComponent<RectTransform>();
        contentRect.pivot = new Vector2(0.5f, 1f);
        GridLayoutGroup gridLayout = content.AddComponent<GridLayoutGroup>();
        gridLayout.padding = new RectOffset(20, 20, 20, 20);
        gridLayout.cellSize = new Vector2(260f, 260f);
        gridLayout.spacing = new Vector2(16f, 16f);
        gridLayout.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
        gridLayout.constraintCount = 5;
        gridLayout.childAlignment = TextAnchor.UpperCenter;
        ContentSizeFitter contentSizeFitter = content.AddComponent<ContentSizeFitter>();
        contentSizeFitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

        scrollRect.content = contentRect;
        scrollRect.viewport = viewport.GetComponent<RectTransform>();
        scrollRect.horizontal = false;
        scrollRect.vertical = true;
        scrollRect.movementType = ScrollRect.MovementType.Clamped;
        scrollRect.scrollSensitivity = 30f;

        CanvasIndex canvasIndex = root.AddComponent<CanvasIndex>();
        SetReference(canvasIndex, "listBrainrotSO", listBrainrotSO);
        SetReference(canvasIndex, "panel", panel.GetComponent<RectTransform>());
        SetReference(canvasIndex, "closeButton", closeButton);
        SetReference(canvasIndex, "slotPrefab", slotPrefab);
        SetReference(canvasIndex, "slotParent", content.transform);
        SetReference(canvasIndex, "slotScrollRect", scrollRect);
        SetReference(canvasIndex, "totalProgressText", totalProgressText);
        SetRarityConfigs(canvasIndex);

        return SavePrefab(root, CANVAS_PREFAB_FOLDER + "/CanvasIndex.prefab");
    }

    // chi them rarity co brainrot trong list, thu tu theo enum
    private void SetRarityConfigs(CanvasIndex canvasIndex) {
        List<BrainrotRarity> rarities = new List<BrainrotRarity>();
        foreach (BrainrotRarity rarity in System.Enum.GetValues(typeof(BrainrotRarity))) {
            if (listBrainrotSO.GetBrainrotSOsByRarity(rarity).Count == 0) continue;

            rarities.Add(rarity);
        }

        SerializedObject serializedObject = new SerializedObject(canvasIndex);
        SerializedProperty configsProperty = serializedObject.FindProperty("rarityConfigs");
        configsProperty.arraySize = rarities.Count;

        for (int i = 0; i < rarities.Count; i++) {
            SerializedProperty configProperty = configsProperty.GetArrayElementAtIndex(i);
            configProperty.FindPropertyRelative("rarity").enumValueIndex = (int)rarities[i];
            configProperty.FindPropertyRelative("backgroundMaterial").objectReferenceValue = GetRarityMaterial(rarities[i]);
        }

        serializedObject.ApplyModifiedPropertiesWithoutUndo();
    }

    // ten material: RarityBG_<Rarity>.mat
    private Material GetRarityMaterial(BrainrotRarity rarity) {
        string materialPath = AssetDatabase.GetAssetPath(materialFolder) + "/" + MATERIAL_PREFIX + rarity + ".mat";
        Material material = AssetDatabase.LoadAssetAtPath<Material>(materialPath);
        if (material == null) {
            Debug.LogWarning("Missing rarity material at " + materialPath);
        }

        return material;
    }

    private GameObject CreateUIObject(string objectName, Transform parent) {
        GameObject uiObject = new GameObject(objectName, typeof(RectTransform));
        uiObject.layer = LayerMask.NameToLayer("UI");
        if (parent != null) {
            uiObject.transform.SetParent(parent, false);
        }

        return uiObject;
    }

    private void SetRect(GameObject uiObject, Vector2 anchorMin, Vector2 anchorMax, Vector2 pivot, Vector2 anchoredPosition, Vector2 sizeDelta) {
        RectTransform rect = uiObject.GetComponent<RectTransform>();
        rect.anchorMin = anchorMin;
        rect.anchorMax = anchorMax;
        rect.pivot = pivot;
        rect.anchoredPosition = anchoredPosition;
        rect.sizeDelta = sizeDelta;
    }

    private void SetOffsets(GameObject uiObject, Vector2 anchorMin, Vector2 anchorMax, Vector2 offsetMin, Vector2 offsetMax) {
        RectTransform rect = uiObject.GetComponent<RectTransform>();
        rect.anchorMin = anchorMin;
        rect.anchorMax = anchorMax;
        rect.offsetMin = offsetMin;
        rect.offsetMax = offsetMax;
    }

    // dat anchor roi can le vao trong theo (left, right, top, bottom)
    private void SetAnchors(GameObject uiObject, Vector2 anchorMin, Vector2 anchorMax, float left, float right, float top, float bottom) {
        SetOffsets(uiObject, anchorMin, anchorMax, new Vector2(left, bottom), new Vector2(-right, -top));
    }

    private void Stretch(GameObject uiObject, float left, float right, float top, float bottom) {
        SetAnchors(uiObject, Vector2.zero, Vector2.one, left, right, top, bottom);
    }

    private Image AddImage(GameObject uiObject, Sprite sprite, Color color, bool isRaycastTarget) {
        Image image = uiObject.AddComponent<Image>();
        image.sprite = sprite;
        image.color = color;
        image.raycastTarget = isRaycastTarget;
        if (sprite != null) {
            image.type = Image.Type.Sliced;
        }

        return image;
    }

    private TextMeshProUGUI AddText(GameObject uiObject, string text, float fontSize, TextAlignmentOptions alignment) {
        TextMeshProUGUI textMesh = uiObject.AddComponent<TextMeshProUGUI>();
        if (font != null) {
            textMesh.font = font;
        }

        textMesh.text = text;
        textMesh.fontSize = fontSize;
        textMesh.alignment = alignment;
        textMesh.color = Color.white;
        textMesh.enableWordWrapping = false;
        textMesh.overflowMode = TextOverflowModes.Ellipsis;
        textMesh.raycastTarget = false;

        return textMesh;
    }

    private void SetReference(Object target, string propertyName, Object value) {
        SerializedObject serializedObject = new SerializedObject(target);
        SerializedProperty property = serializedObject.FindProperty(propertyName);
        if (property == null) {
            Debug.LogError("Missing property " + propertyName + " on " + target.GetType().Name);
            return;
        }

        property.objectReferenceValue = value;
        serializedObject.ApplyModifiedPropertiesWithoutUndo();
    }

    private GameObject SavePrefab(GameObject root, string path) {
        GameObject prefab = PrefabUtility.SaveAsPrefabAsset(root, path);
        DestroyImmediate(root);
        return prefab;
    }

    // tao folder theo tung cap neu chua co
    private void EnsureFolder(string folderPath) {
        if (AssetDatabase.IsValidFolder(folderPath)) return;

        string[] parts = folderPath.Split('/');
        string currentPath = parts[0];

        for (int i = 1; i < parts.Length; i++) {
            string nextPath = currentPath + "/" + parts[i];
            if (!AssetDatabase.IsValidFolder(nextPath)) {
                AssetDatabase.CreateFolder(currentPath, parts[i]);
            }

            currentPath = nextPath;
        }
    }
}
