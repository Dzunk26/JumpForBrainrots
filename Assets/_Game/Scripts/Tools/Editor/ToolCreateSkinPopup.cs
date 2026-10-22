using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

// dung prefab Canvas-Skin (Resources/UI) + SkinSlot (Prefabs/UI) bang code
// clone tu Canvas-Index de giu nguyen style popup (header, pattern, shine, nut close, popup effect)
// panel dat ben phai man hinh de van nhin thay nhan vat doi skin ngoai map
public class ToolCreateSkinPopup : EditorWindow {
    private static string INDEX_CANVAS_PATH = "Assets/_Game/Resources/UI/Canvas-Index.prefab";
    private static string SKIN_CANVAS_PATH = "Assets/_Game/Resources/UI/Canvas-Skin.prefab";
    private static string SKIN_SLOT_PATH = "Assets/_Game/Prefabs/UI/SkinSlot.prefab";
    private static string ROUND_BORDER_SPRITE_PATH = "Assets/_Game/Texture/UI/UI_RoundBorder.png";
    private static string ROUND_FILL_SPRITE_PATH = "Assets/_Game/Texture/UI/UI_RoundFill.png";
    private static string ROUND_OUTLINE_SPRITE_PATH = "Assets/_Game/Texture/UI/UI_RoundOutline.png";
    private static string CHECK_SPRITE_PATH = "Assets/_Game/Texture/UI/UI_CheckCircle.png";
    private static string DEFAULT_HEADER_ICON_PATH = "Assets/_Game/Texture/Icon/ShoeIcon.png";
    private static Vector2 PANEL_SIZE = new Vector2(820f, 720f);
    private static Vector2 PANEL_POSITION = new Vector2(-60f, -20f);
    private static Vector2 SLOT_SIZE = new Vector2(230f, 260f);
    private static float BLOCKER_ALPHA = 0.25f;

    [SerializeField] private ListSkinSO listSkinSO;
    [SerializeField] private Sprite headerIcon;
    [SerializeField] private Sprite lockSprite;

    private TMP_FontAsset font;
    private Sprite roundBorderSprite;
    private Sprite roundFillSprite;
    private Sprite roundOutlineSprite;
    private Sprite checkSprite;

    private void OnEnable() {
        if (headerIcon == null) {
            headerIcon = AssetDatabase.LoadAssetAtPath<Sprite>(DEFAULT_HEADER_ICON_PATH);
        }
    }

    private void OnGUI() {
        listSkinSO = (ListSkinSO)EditorGUILayout.ObjectField("List Skin SO", listSkinSO, typeof(ListSkinSO), false);
        headerIcon = (Sprite)EditorGUILayout.ObjectField("Header Icon", headerIcon, typeof(Sprite), false);
        lockSprite = (Sprite)EditorGUILayout.ObjectField("Lock Sprite (optional)", lockSprite, typeof(Sprite), false);

        EditorGUILayout.Space();
        EditorGUILayout.HelpBox("Clone " + INDEX_CANVAS_PATH + " roi tao (ghi de) " + SKIN_SLOT_PATH + " va " + SKIN_CANVAS_PATH + "\nKhong co Lock Sprite thi o khoa duoc ghep tu UI_RoundOutline + UI_RoundFill", MessageType.Info);

        if (GUILayout.Button("Create Skin Popup")) {
            CreateSkinPopup();
        }
    }

    [MenuItem("Tools/UI/Skin Popup Creator")]
    private static void OpenWindow() {
        GetWindow<ToolCreateSkinPopup>("Skin Popup Creator");
    }

    private void CreateSkinPopup() {
        if (listSkinSO == null) {
            Debug.LogWarning("List skin SO is missing, nho keo vao CanvasSkin sau");
        }

        roundBorderSprite = AssetDatabase.LoadAssetAtPath<Sprite>(ROUND_BORDER_SPRITE_PATH);
        roundFillSprite = AssetDatabase.LoadAssetAtPath<Sprite>(ROUND_FILL_SPRITE_PATH);
        roundOutlineSprite = AssetDatabase.LoadAssetAtPath<Sprite>(ROUND_OUTLINE_SPRITE_PATH);
        checkSprite = AssetDatabase.LoadAssetAtPath<Sprite>(CHECK_SPRITE_PATH);

        GameObject root = PrefabUtility.LoadPrefabContents(INDEX_CANVAS_PATH);
        font = FindChild(root.transform, "TitleText").GetComponent<TextMeshProUGUI>().font;

        SkinSlotUI slotPrefab = CreateSlotPrefab();
        GameObject canvasPrefab = CreateCanvasPrefab(root, slotPrefab);

        AssetDatabase.SaveAssets();
        Selection.activeObject = canvasPrefab;
        EditorGUIUtility.PingObject(canvasPrefab);
        Debug.Log("Created skin popup at " + SKIN_CANVAS_PATH);
    }

    // SkinSlot (chi hien icon, khong co ten / gia)
    // |- Highlight, Icon, LockOverlay, Frame
    // |- EquippedLabel > EquippedText (day the)
    // \- LockIcon, EquippedMark (goc tren phai)
    private SkinSlotUI CreateSlotPrefab() {
        GameObject root = CreateUIObject("SkinSlot", null);
        SetRect(root, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, SLOT_SIZE);
        Image rootImage = AddImage(root, roundBorderSprite, new Color(0.25f, 0.3f, 0.39f, 1f), true);
        Button button = root.AddComponent<Button>();
        button.targetGraphic = rootImage;
        root.AddComponent<ButtonUIEffect>();

        GameObject highlight = CreateUIObject("Highlight", root.transform);
        SetOffsets(highlight, new Vector2(0f, 0.5f), new Vector2(1f, 1f), new Vector2(7f, 0f), new Vector2(-7f, -9f));
        AddImage(highlight, roundFillSprite, new Color(1f, 1f, 1f, 0.12f), false);

        GameObject icon = CreateUIObject("Icon", root.transform);
        Stretch(icon, 14f, 14f, 14f, 14f);
        Image iconImage = AddImage(icon, null, Color.white, false);
        iconImage.preserveAspect = true;

        GameObject lockOverlay = CreateUIObject("LockOverlay", root.transform);
        Stretch(lockOverlay, 7f, 7f, 7f, 7f);
        AddImage(lockOverlay, roundFillSprite, new Color(0f, 0f, 0f, 0.35f), false);

        GameObject frame = CreateUIObject("Frame", root.transform);
        Stretch(frame, -6f, -6f, -6f, -6f);
        Image frameImage = AddImage(frame, roundOutlineSprite, new Color(0.35f, 0.92f, 1f, 1f), false);
        frame.SetActive(false);

        GameObject equippedLabel = CreateUIObject("EquippedLabel", root.transform);
        SetRect(equippedLabel, new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0f, 12f), new Vector2(170f, 44f));
        AddImage(equippedLabel, roundFillSprite, new Color(0.36f, 0.86f, 0.06f, 1f), false);
        equippedLabel.SetActive(false);

        GameObject equippedTextObject = CreateUIObject("EquippedText", equippedLabel.transform);
        Stretch(equippedTextObject, 8f, 8f, 0f, 0f);
        TextMeshProUGUI equippedText = AddText(equippedTextObject, "Equipped", 28f, TextAlignmentOptions.Center);
        equippedText.enableAutoSizing = true;
        equippedText.fontSizeMin = 16f;
        equippedText.fontSizeMax = 28f;

        GameObject lockIcon = CreateLockIcon(root.transform);

        GameObject equippedMark = CreateUIObject("EquippedMark", root.transform);
        SetRect(equippedMark, new Vector2(1f, 1f), new Vector2(1f, 1f), new Vector2(1f, 1f), new Vector2(-10f, -10f), new Vector2(56f, 56f));
        AddImage(equippedMark, checkSprite, Color.white, false).type = Image.Type.Simple;
        equippedMark.SetActive(false);

        SkinSlotUI slot = root.AddComponent<SkinSlotUI>();
        SetReference(slot, "button", button);
        SetReference(slot, "iconImage", iconImage);
        SetReference(slot, "frameImage", frameImage);
        SetReference(slot, "lockOverlay", lockOverlay);
        SetReference(slot, "lockIcon", lockIcon);
        SetReference(slot, "equippedMark", equippedMark);
        SetReference(slot, "equippedLabel", equippedLabel);
        SetReference(slot, "equippedText", equippedText);

        GameObject prefab = PrefabUtility.SaveAsPrefabAsset(root, SKIN_SLOT_PATH);
        DestroyImmediate(root);
        return prefab.GetComponent<SkinSlotUI>();
    }

    // co lock sprite thi dung, khong thi ghep: quai (outline) + than (fill) + lo khoa
    private GameObject CreateLockIcon(Transform parent) {
        GameObject lockIcon = CreateUIObject("LockIcon", parent);
        SetRect(lockIcon, new Vector2(1f, 1f), new Vector2(1f, 1f), new Vector2(1f, 1f), new Vector2(-12f, -12f), new Vector2(52f, 60f));

        if (lockSprite != null) {
            AddImage(lockIcon, lockSprite, Color.white, false).type = Image.Type.Simple;
            return lockIcon;
        }

        GameObject shackle = CreateUIObject("Shackle", lockIcon.transform);
        SetRect(shackle, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), Vector2.zero, new Vector2(34f, 40f));
        AddImage(shackle, roundOutlineSprite, Color.white, false).pixelsPerUnitMultiplier = 2.5f;

        GameObject body = CreateUIObject("Body", lockIcon.transform);
        SetRect(body, new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), Vector2.zero, new Vector2(52f, 38f));
        AddImage(body, roundFillSprite, Color.white, false).pixelsPerUnitMultiplier = 2f;

        GameObject keyhole = CreateUIObject("Keyhole", body.transform);
        SetRect(keyhole, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(10f, 16f));
        AddImage(keyhole, roundFillSprite, new Color(0.19f, 0.19f, 0.21f, 1f), false).pixelsPerUnitMultiplier = 4f;

        return lockIcon;
    }

    // Canvas-Skin (clone Canvas-Index)
    // |- BlockerButton (mo hon de thay nhan vat)
    // \- Panel (ben phai man hinh)
    //   |- Header (SkinIcon, TitleText, ProgressPill > TotalProgressText, CloseButton)
    //   \- ScrollView > Viewport > Content (grid SkinSlot 3 cot)
    private GameObject CreateCanvasPrefab(GameObject root, SkinSlotUI slotPrefab) {
        root.name = "Canvas-Skin";
        DestroyImmediate(root.GetComponent<CanvasIndex>());

        Image blockerImage = FindChild(root.transform, "BlockerButton").GetComponent<Image>();
        Color blockerColor = blockerImage.color;
        blockerColor.a = BLOCKER_ALPHA;
        blockerImage.color = blockerColor;

        // bo layout can giua cua Canvas-Index, tu dat panel sang phai
        Transform panel = FindChild(root.transform, "Panel");
        HorizontalLayoutGroup layoutGroup = panel.parent.GetComponent<HorizontalLayoutGroup>();
        if (layoutGroup != null) {
            DestroyImmediate(layoutGroup);
        }
        SetRect(panel.gameObject, new Vector2(1f, 0.5f), new Vector2(1f, 0.5f), new Vector2(1f, 0.5f), PANEL_POSITION, PANEL_SIZE);

        GameObject title = FindChild(root.transform, "TitleText").gameObject;
        title.GetComponent<TextMeshProUGUI>().SetText("Skins");
        RectTransform titleRect = title.GetComponent<RectTransform>();
        titleRect.offsetMin = new Vector2(150f, titleRect.offsetMin.y);
        titleRect.offsetMax = new Vector2(-410f, titleRect.offsetMax.y);

        Transform iconTransform = FindChild(root.transform, "IndexIcon");
        iconTransform.name = "SkinIcon";
        if (headerIcon != null) {
            Image iconImage = iconTransform.GetComponent<Image>();
            iconImage.sprite = headerIcon;
            iconImage.preserveAspect = true;
        }

        Transform content = FindChild(root.transform, "Content");
        GridLayoutGroup gridLayout = content.GetComponent<GridLayoutGroup>();
        gridLayout.padding = new RectOffset(16, 16, 16, 16);
        gridLayout.cellSize = SLOT_SIZE;
        gridLayout.spacing = new Vector2(16f, 16f);
        gridLayout.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
        gridLayout.constraintCount = 3;
        gridLayout.childAlignment = TextAnchor.UpperCenter;

        CanvasSkin canvasSkin = root.AddComponent<CanvasSkin>();
        SetReference(canvasSkin, "listSkinSO", listSkinSO);
        SetReference(canvasSkin, "closeButton", FindChild(root.transform, "CloseButton").GetComponent<Button>());
        SetReference(canvasSkin, "blockerButton", FindChild(root.transform, "BlockerButton").GetComponent<Button>());
        SetReference(canvasSkin, "slotPrefab", slotPrefab);
        SetReference(canvasSkin, "slotParent", content);
        SetReference(canvasSkin, "slotScrollRect", FindChild(root.transform, "ScrollView").GetComponent<ScrollRect>());
        SetReference(canvasSkin, "totalProgressText", FindChild(root.transform, "TotalProgressText").GetComponent<TextMeshProUGUI>());

        GameObject prefab = PrefabUtility.SaveAsPrefabAsset(root, SKIN_CANVAS_PATH);
        PrefabUtility.UnloadPrefabContents(root);
        return prefab;
    }

    // tim con theo ten o moi cap
    private Transform FindChild(Transform parent, string childName) {
        foreach (Transform child in parent) {
            if (child.name == childName) return child;

            Transform result = FindChild(child, childName);
            if (result != null) return result;
        }

        return null;
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

    // stretch roi can le vao trong theo (left, right, top, bottom)
    private void Stretch(GameObject uiObject, float left, float right, float top, float bottom) {
        SetOffsets(uiObject, Vector2.zero, Vector2.one, new Vector2(left, bottom), new Vector2(-right, -top));
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
}
