using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

// dung prefab InventorySlot (Prefabs/UI) + them HotBarUI vao Canvas-Gameplay (Resources/UI) bang code
// nen o, vien chon, icon balo... la placeholder lay tu built-in UI sprite, sau nay tu thay
public class ToolCreateHotBarUI : EditorWindow {
    private static string DEFAULT_FONT_PATH = "Assets/_Game/Fonts/Fredoka-Bold SDF.asset";
    private static string DEFAULT_CANVAS_GAMEPLAY_PATH = "Assets/_Game/Resources/UI/Canvas-Gameplay.prefab";
    private static string ITEM_PREFAB_FOLDER = "Assets/_Game/Prefabs/UI";
    private static string HOTBAR_OBJECT_NAME = "HotBarUI";
    private static string BUILTIN_ROUND_SPRITE = "UI/Skin/UISprite.psd";

    private static float SLOT_SIZE = 130f;
    private static float SLOT_SPACING = 16f;
    private static float ICON_SIZE = 110f;
    private static float BACKPACK_BUTTON_SIZE = 120f;
    private static float HOTBAR_BOTTOM_OFFSET = 24f;
    private static Color SLOT_BACKGROUND_COLOR = new Color(0f, 0f, 0f, 0.25f);
    private static Color SELECTED_FRAME_COLOR = new Color(1f, 0.88f, 0.3f, 1f);
    private static Color INCOME_TEXT_COLOR = new Color(0.42f, 1f, 0.23f, 1f);

    [SerializeField] private GameObject canvasGameplayPrefab;
    [SerializeField] private TMP_FontAsset font;
    [SerializeField] private Sprite backpackIcon;

    private Sprite roundSprite;

    private void OnEnable() {
        if (canvasGameplayPrefab == null) {
            canvasGameplayPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(DEFAULT_CANVAS_GAMEPLAY_PATH);
        }

        if (font == null) {
            font = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(DEFAULT_FONT_PATH);
        }
    }

    private void OnGUI() {
        canvasGameplayPrefab = (GameObject)EditorGUILayout.ObjectField("Canvas Gameplay Prefab", canvasGameplayPrefab, typeof(GameObject), false);
        font = (TMP_FontAsset)EditorGUILayout.ObjectField("Font", font, typeof(TMP_FontAsset), false);
        backpackIcon = (Sprite)EditorGUILayout.ObjectField("Backpack Icon", backpackIcon, typeof(Sprite), false);

        EditorGUILayout.Space();
        EditorGUILayout.HelpBox("Tao (ghi de) " + ITEM_PREFAB_FOLDER + "/InventorySlot.prefab va tao lai object " + HOTBAR_OBJECT_NAME + " trong Canvas Gameplay", MessageType.Info);

        if (GUILayout.Button("Create Hot Bar")) {
            CreateHotBar();
        }
    }

    [MenuItem("Tools/UI/Hot Bar Creator")]
    private static void OpenWindow() {
        GetWindow<ToolCreateHotBarUI>("Hot Bar Creator");
    }

    private void CreateHotBar() {
        if (canvasGameplayPrefab == null || canvasGameplayPrefab.GetComponent<CanvasGamePlay>() == null) {
            Debug.LogError("Canvas gameplay prefab is missing or has no CanvasGamePlay");
            return;
        }

        roundSprite = AssetDatabase.GetBuiltinExtraResource<Sprite>(BUILTIN_ROUND_SPRITE);
        EnsureFolder(ITEM_PREFAB_FOLDER);

        InventorySlotUI slotPrefab = CreateSlotPrefab();
        AddHotBarToCanvas(slotPrefab);

        AssetDatabase.SaveAssets();
        Selection.activeObject = canvasGameplayPrefab;
        EditorGUIUtility.PingObject(canvasGameplayPrefab);
        Debug.Log("Created hot bar in " + AssetDatabase.GetAssetPath(canvasGameplayPrefab));
    }

    // InventorySlot (130x130, nen + button)
    // |- SelectedFrame (vien, mac dinh tat)
    // |- Icon
    // \- IncomeText
    private InventorySlotUI CreateSlotPrefab() {
        GameObject root = CreateUIObject("InventorySlot", null);
        SetRect(root, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(SLOT_SIZE, SLOT_SIZE));
        Image backgroundImage = AddImage(root, roundSprite, SLOT_BACKGROUND_COLOR, true);
        Button button = root.AddComponent<Button>();
        button.targetGraphic = backgroundImage;
        AddLayoutElement(root, SLOT_SIZE, SLOT_SIZE);

        GameObject selectedFrame = CreateUIObject("SelectedFrame", root.transform);
        Stretch(selectedFrame, -5f, -5f, -5f, -5f);
        Image selectedFrameImage = AddImage(selectedFrame, roundSprite, SELECTED_FRAME_COLOR, false);
        selectedFrameImage.fillCenter = false;
        selectedFrame.SetActive(false);

        GameObject icon = CreateUIObject("Icon", root.transform);
        SetRect(icon, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -6f), new Vector2(ICON_SIZE, ICON_SIZE));
        Image iconImage = AddImage(icon, null, Color.white, false);
        iconImage.preserveAspect = true;

        GameObject incomeObject = CreateUIObject("IncomeText", root.transform);
        SetOffsets(incomeObject, new Vector2(0f, 0f), new Vector2(1f, 0f), new Vector2(4f, 6f), new Vector2(-4f, 42f));
        TextMeshProUGUI incomeText = AddText(incomeObject, "$0/s", 26f, TextAlignmentOptions.Center);
        incomeText.color = INCOME_TEXT_COLOR;
        incomeText.enableAutoSizing = true;
        incomeText.fontSizeMin = 16f;
        incomeText.fontSizeMax = 26f;

        InventorySlotUI slot = root.AddComponent<InventorySlotUI>();
        SetReference(slot, "button", button);
        SetReference(slot, "iconImage", iconImage);
        SetReference(slot, "incomeText", incomeText);
        SetReference(slot, "selectedFrame", selectedFrame);

        return SavePrefab(root, ITEM_PREFAB_FOLDER + "/InventorySlot.prefab").GetComponent<InventorySlotUI>();
    }

    // HotBarUI (bottom-center, tu co gian theo so o dang hien)
    // |- SlotParent (5 InventorySlot tao luc runtime)
    // \- BackpackButton
    private void AddHotBarToCanvas(InventorySlotUI slotPrefab) {
        string canvasPath = AssetDatabase.GetAssetPath(canvasGameplayPrefab);
        GameObject canvasRoot = PrefabUtility.LoadPrefabContents(canvasPath);

        Transform oldHotBar = canvasRoot.transform.Find(HOTBAR_OBJECT_NAME);
        if (oldHotBar != null) {
            DestroyImmediate(oldHotBar.gameObject);
        }

        GameObject hotBar = CreateUIObject(HOTBAR_OBJECT_NAME, canvasRoot.transform);
        SetRect(hotBar, new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0f, HOTBAR_BOTTOM_OFFSET), new Vector2(0f, SLOT_SIZE));
        AddHorizontalLayout(hotBar);
        ContentSizeFitter hotBarSizeFitter = hotBar.AddComponent<ContentSizeFitter>();
        hotBarSizeFitter.horizontalFit = ContentSizeFitter.FitMode.PreferredSize;

        GameObject slotParent = CreateUIObject("SlotParent", hotBar.transform);
        AddHorizontalLayout(slotParent);

        GameObject backpackObject = CreateUIObject("BackpackButton", hotBar.transform);
        Color backpackColor = backpackIcon != null ? Color.white : new Color(0.2f, 0.6f, 1f, 1f);
        Image backpackImage = AddImage(backpackObject, backpackIcon != null ? backpackIcon : roundSprite, backpackColor, true);
        backpackImage.preserveAspect = backpackIcon != null;
        Button backpackButton = backpackObject.AddComponent<Button>();
        backpackButton.targetGraphic = backpackImage;
        AddLayoutElement(backpackObject, BACKPACK_BUTTON_SIZE, BACKPACK_BUTTON_SIZE);

        HotBarUI hotBarUI = hotBar.AddComponent<HotBarUI>();
        SetReference(hotBarUI, "slotPrefab", slotPrefab);
        SetReference(hotBarUI, "slotParent", slotParent.transform);
        SetReference(hotBarUI, "backpackButton", backpackButton);

        CanvasGamePlay canvasGamePlay = canvasRoot.GetComponent<CanvasGamePlay>();
        SetReference(canvasGamePlay, "hotBarUI", hotBarUI);

        PrefabUtility.SaveAsPrefabAsset(canvasRoot, canvasPath);
        PrefabUtility.UnloadPrefabContents(canvasRoot);
    }

    private void AddHorizontalLayout(GameObject uiObject) {
        HorizontalLayoutGroup layout = uiObject.AddComponent<HorizontalLayoutGroup>();
        layout.spacing = SLOT_SPACING;
        layout.childAlignment = TextAnchor.MiddleCenter;
        layout.childControlWidth = true;
        layout.childControlHeight = true;
        layout.childForceExpandWidth = false;
        layout.childForceExpandHeight = false;
    }

    private void AddLayoutElement(GameObject uiObject, float width, float height) {
        LayoutElement layoutElement = uiObject.AddComponent<LayoutElement>();
        layoutElement.preferredWidth = width;
        layoutElement.preferredHeight = height;
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

    // stretch roi can le vao trong theo (left, right, top, bottom), so am thi tran ra ngoai
    private void Stretch(GameObject uiObject, float left, float right, float top, float bottom) {
        SetOffsets(uiObject, Vector2.zero, Vector2.one, new Vector2(left, bottom), new Vector2(-right, -top));
    }

    private Image AddImage(GameObject uiObject, Sprite sprite, Color color, bool isRaycastTarget) {
        Image image = uiObject.AddComponent<Image>();
        image.sprite = sprite;
        image.color = color;
        image.raycastTarget = isRaycastTarget;
        if (sprite != null && sprite.border != Vector4.zero) {
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
