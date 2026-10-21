using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

// dung lai UI ben trong Panel cua Canvas-Rebirth theo thiet ke moi (Now -> After Rebirth)
// toa do lay theo panel 1200x700, goc tren trai
public static class CanvasRebirthBuilder {
    private const string PREFAB_PATH = "Assets/_Game/Resources/UI/Canvas-Rebirth.prefab";
    private const string UI_PATH = "Assets/_Game/Texture/UI/";
    private const string ICON_PATH = "Assets/_Game/Texture/Icon/";
    private const string FONT_PATH = "Assets/_Game/Fonts/Fredoka-Bold SDF.asset";
    private const string FONT_MATERIAL_PATH = "Assets/_Game/Fonts/Fredoka-Bold SDF Outline.mat";

    private static readonly Color DARK = Hex("111111");
    private static readonly Color YELLOW = Hex("FFE63F");
    private static readonly Color GREEN = new Color(0.36f, 0.86f, 0.05f, 1f);

    private static TMP_FontAsset font;
    private static Material fontMaterial;

    [MenuItem("Tools/Rebuild Canvas Rebirth")]
    public static void Build() {
        font = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(FONT_PATH);
        fontMaterial = AssetDatabase.LoadAssetAtPath<Material>(FONT_MATERIAL_PATH);

        GameObject root = PrefabUtility.LoadPrefabContents(PREFAB_PATH);
        try {
            BuildPanel(root);
            PrefabUtility.SaveAsPrefabAsset(root, PREFAB_PATH);
            Debug.Log("Canvas-Rebirth rebuilt");
        }
        finally {
            PrefabUtility.UnloadPrefabContents(root);
        }
    }

    private static void BuildPanel(GameObject root) {
        CanvasRebirth canvas = root.GetComponent<CanvasRebirth>();
        Transform panel = FindDeep(root.transform, "Panel");

        for (int i = panel.childCount - 1; i >= 0; i--) {
            Object.DestroyImmediate(panel.GetChild(i).gameObject);
        }

        SerializedObject so = new SerializedObject(canvas);

        // ---------- nen + header ----------
        Image pattern = NewImage(panel, "Pattern", 0, 0, 1200, 700, UISprite("UI_RoundFill"), Hex("424249"));
        Stretch(pattern.rectTransform, 8);

        Image header = NewImage(panel, "Header", 6, 6, 1188, 122, UISprite("UI_RoundBorder"), Hex("C43FDC"));
        Image headerHighlight = NewImage(header.transform, "Highlight", 0, 0, 0, 0, UISprite("UI_RoundFill"), new Color(1f, 1f, 1f, 0.25f));
        StretchTop(headerHighlight.rectTransform, 14, 8, 28);
        AddShine(header.transform, 8);

        NewIcon(panel, "HeaderIcon", 22, -30, 150, 150, IconSprite("RebirthIcon"));
        NewText(panel, "TitleText", 184, 20, 300, 90, "Rebirth", 68, Color.white, TextAlignmentOptions.Left);

        RectTransform levelPill = NewImage(panel, "LevelPill", 470, 28, 240, 66, UISprite("UI_RoundFill"), DARK).rectTransform;
        levelPill.pivot = new Vector2(0f, 0.5f);
        levelPill.anchoredPosition = new Vector2(470f, -61f);
        AddHorizontalLayout(levelPill.gameObject, 22, 22, 14, true);
        TextMeshProUGUI oldLevelText = NewLayoutText(levelPill, "OldLevelText", "Lv 15", 40, YELLOW);
        Image levelArrow = NewArrow(levelPill, "Arrow", 30, YELLOW);
        TextMeshProUGUI newLevelText = NewLayoutText(levelPill, "NewLevelText", "16", 40, YELLOW);

        Button closeButton = NewButton(panel, "CloseButton", 1080, 16, 100, 100, new Color(0.85f, 0.12f, 0.12f, 1f));
        TextMeshProUGUI closeText = NewText(closeButton.transform, "Text", 0, 0, 0, 0, "X", 66, Color.white, TextAlignmentOptions.Center);
        Stretch(closeText.rectTransform, 0, 3, 0, -3);

        // ---------- o Now ----------
        RectTransform nowBox = NewImage(panel, "NowBox", 28, 156, 440, 256, UISprite("UI_RoundBorder"), Hex("29292E")).rectTransform;
        NewImage(nowBox, "Tag", 6, 6, 428, 54, UISprite("UI_RoundBorder"), Hex("5F5F6A"));
        NewText(nowBox, "TagText", 6, 6, 428, 54, "Now", 34, Hex("E8E8EE"), TextAlignmentOptions.Center);

        TextMeshProUGUI nowMoneyText = NewNowRow(nowBox, 0, "Money", IconSprite("MoneyIcon"), "x7.5");
        TextMeshProUGUI nowSpeedText = NewNowRow(nowBox, 1, "Speed", IconSprite("ShoeIcon"), "54");
        TextMeshProUGUI nowJumpText = NewNowRow(nowBox, 2, "Jump", IconSprite("JumpPowerIcon"), "282");

        Image arrow = NewArrow(panel, "Arrow", 80, YELLOW);
        SetRect(arrow.rectTransform, 472, 244, 80, 80);

        // ---------- o After Rebirth ----------
        Image afterGlow = NewImage(panel, "AfterGlow", 506, 92, 716, 384, UISprite("UI_Glow"), new Color(1f, 0.82f, 0.23f, 0.55f));
        afterGlow.raycastTarget = false;

        RectTransform afterBox = NewImage(panel, "AfterBox", 556, 142, 616, 284, UISprite("UI_RoundBorder"), Hex("33301F")).rectTransform;
        Image afterOutline = NewImage(afterBox, "Outline", 0, 0, 0, 0, UISprite("UI_RoundOutline"), Hex("FFD23A"));
        Stretch(afterOutline.rectTransform, -10);

        Image afterTag = NewImage(afterBox, "Tag", 6, 6, 604, 62, UISprite("UI_RoundBorder"), Hex("FFC21A"));
        AddShine(afterTag.transform, 6);
        TextMeshProUGUI afterTitleText = NewText(afterBox, "TagText", 6, 6, 604, 62, "After Rebirth", 40, Color.white, TextAlignmentOptions.Center);

        GameObject moneyChip;
        GameObject speedChip;
        TextMeshProUGUI moneyBonusText;
        TextMeshProUGUI speedBonusText;
        TextMeshProUGUI afterMoneyText = NewAfterRow(afterBox, 0, "Money", IconSprite("MoneyIcon"), "x8", YELLOW, "+0.5x", Hex("8DFF5A"), out moneyChip, out moneyBonusText);
        TextMeshProUGUI afterSpeedText = NewAfterRow(afterBox, 1, "Speed", IconSprite("ShoeIcon"), "55", YELLOW, "+1", Hex("8DFF5A"), out speedChip, out speedBonusText);
        NewAfterRow(afterBox, 2, "Jump", IconSprite("JumpPowerIcon"), "0", Hex("FF6B6B"), "reset", Hex("FF8A8A"), out _, out _);
        GameObject afterJumpRow = afterBox.Find("Row2").gameObject;

        Image afterBurst = NewImage(afterBox, "Burst", 158, -8, 300, 300, UISprite("UI_Burst"), new Color(1f, 0.92f, 0.35f, 1f));
        afterBurst.raycastTarget = false;
        afterBurst.gameObject.SetActive(false);

        Image[] sparkles = {
            NewSparkle(panel, "Sparkle0", 542, 112, 56),
            NewSparkle(panel, "Sparkle1", 1146, 120, 44),
            NewSparkle(panel, "Sparkle2", 1152, 398, 56),
        };

        // ---------- thanh muc tieu ----------
        RectTransform goal = NewImage(panel, "Goal", 28, 444, 1144, 92, UISprite("UI_RoundFill"), new Color(0f, 0f, 0f, 0.28f)).rectTransform;
        TextMeshProUGUI goalText = NewText(goal, "GoalText", 96, 2, 1000, 40, "Reach 340 Jump Power to rebirth for free", 26, Hex("E6E6EE"), TextAlignmentOptions.Left);

        RectTransform progressBar = NewImage(goal, "ProgressBar", 96, 40, 1024, 44, UISprite("UI_RoundBorder"), DARK).rectTransform;
        Image fillMask = NewImage(progressBar, "FillMask", 0, 0, 0, 0, UISprite("UI_RoundFill"), Color.white);
        Stretch(fillMask.rectTransform, 5);
        fillMask.gameObject.AddComponent<Mask>().showMaskGraphic = false;
        Image fill = NewImage(fillMask.transform, "Fill", 0, 0, 0, 0, null, GREEN);
        Stretch(fill.rectTransform, 0);
        fill.type = Image.Type.Filled;
        fill.fillMethod = Image.FillMethod.Horizontal;
        fill.fillOrigin = (int)Image.OriginHorizontal.Left;
        fill.fillAmount = 0.83f;
        TextMeshProUGUI progressText = NewText(progressBar, "ProgressText", 0, 0, 0, 0, "282 / 340", 28, Color.white, TextAlignmentOptions.Center);
        Stretch(progressText.rectTransform, 0, 2, 0, -2);

        NewIcon(goal, "JumpIcon", 18, -4, 66, 96, IconSprite("JumpPowerIcon"));

        // ---------- footer ----------
        RectTransform footer = NewImage(panel, "Footer", 22, 548, 1156, 130, UISprite("UI_RoundBorder"), Hex("17171A")).rectTransform;

        Button freeButton = NewButton(footer, "FreeRebirthButton", 22, 17, 300, 96, new Color(0.56f, 0.56f, 0.62f, 1f));
        AddShine(freeButton.transform, 6);
        RectTransform freeContent = NewRect(freeButton.transform, "Content", 0, 0, 0, 0);
        Stretch(freeContent, 0);
        AddHorizontalLayout(freeContent.gameObject, 0, 0, 10, false);
        NewLayoutIcon(freeContent, "Icon", IconSprite("watch_icon"), 48, 58);
        NewLayoutText(freeContent, "FreeRebirthText", "Free", 40, Color.white);
        Image adTag = NewImage(freeButton.transform, "AdTag", 238, 6, 50, 26, UISprite("UI_RoundFill"), DARK);
        TextMeshProUGUI adText = NewText(adTag.transform, "Text", 0, 0, 0, 0, "AD", 18, Color.white, TextAlignmentOptions.Center);
        Stretch(adText.rectTransform, 0);

        Image rebirthGlow = NewImage(footer, "RebirthGlow", 314, -15, 860, 160, UISprite("UI_Glow"), new Color(0.35f, 1f, 0.15f, 0.45f));
        rebirthGlow.raycastTarget = false;

        Button rebirthButton = NewButton(footer, "RebirthButton", 354, 17, 780, 96, GREEN);
        AddShine(rebirthButton.transform, 6);
        RectTransform rebirthContent = NewRect(rebirthButton.transform, "Content", 0, 0, 0, 0);
        Stretch(rebirthContent, 0);
        AddHorizontalLayout(rebirthContent.gameObject, 0, 0, 14, false);
        GameObject rebirthIcon = NewLayoutIcon(rebirthContent, "Icon", IconSprite("RebirthIcon"), 70, 70).gameObject;
        TextMeshProUGUI rebirthText = NewLayoutText(rebirthContent, "RebirthText", "REBIRTH", 56, Color.white);

        RectTransform costPill = NewImage(rebirthContent, "CostPill", 0, 0, 200, 62, UISprite("UI_RoundFill"), new Color(0f, 0f, 0f, 0.35f)).rectTransform;
        AddHorizontalLayout(costPill.gameObject, 8, 20, 6, false);
        NewLayoutIcon(costPill, "MoneyIcon", IconSprite("MoneyIcon"), 62, 52);
        TextMeshProUGUI costText = NewLayoutText(costPill, "CostText", "$1.2T", 40, Color.white);

        // ---------- gan field ----------
        Set(so, "closeButton", closeButton);
        Set(so, "rebirthButton", rebirthButton);
        Set(so, "freeRebirthButton", freeButton);
        Set(so, "oldLevelText", oldLevelText);
        Set(so, "newLevelText", newLevelText);
        Set(so, "levelArrow", levelArrow.gameObject);
        Set(so, "nowMoneyText", nowMoneyText);
        Set(so, "nowSpeedText", nowSpeedText);
        Set(so, "nowJumpText", nowJumpText);
        Set(so, "afterBox", afterBox);
        Set(so, "afterTitleText", afterTitleText);
        Set(so, "afterMoneyText", afterMoneyText);
        Set(so, "afterSpeedText", afterSpeedText);
        Set(so, "afterJumpRow", afterJumpRow);
        Set(so, "moneyBonusChip", moneyChip);
        Set(so, "speedBonusChip", speedChip);
        Set(so, "moneyBonusText", moneyBonusText);
        Set(so, "speedBonusText", speedBonusText);
        Set(so, "afterBurstImage", afterBurst);
        Set(so, "goalText", goalText);
        Set(so, "progressFill", fill);
        Set(so, "progressText", progressText);
        Set(so, "rebirthText", rebirthText);
        Set(so, "rebirthIcon", rebirthIcon);
        Set(so, "costPill", costPill.gameObject);
        Set(so, "costText", costText);
        Set(so, "rebirthGlow", rebirthGlow.gameObject);

        SerializedProperty sparkleProperty = so.FindProperty("sparkleImages");
        sparkleProperty.arraySize = sparkles.Length;
        for (int i = 0; i < sparkles.Length; i++) {
            sparkleProperty.GetArrayElementAtIndex(i).objectReferenceValue = sparkles[i];
        }

        so.ApplyModifiedPropertiesWithoutUndo();
    }

    // ---------- row ----------
    private static TextMeshProUGUI NewNowRow(Transform box, int index, string label, Sprite icon, string value) {
        const float rowHeight = 62f;
        float y = 68f + index * rowHeight;
        RectTransform row = NewRect(box, "Row" + index, 0, y, 440, rowHeight);

        if (index > 0) NewImage(row, "Divider", 22, 0, 396, 3, UISprite("UI_RoundFill"), new Color(1f, 1f, 1f, 0.07f));
        NewIcon(row, "Icon", 22, 5, 52, 52, icon);
        NewText(row, "Label", 88, 0, 180, rowHeight, label, 28, Hex("B9B9C4"), TextAlignmentOptions.Left);
        return NewText(row, "ValueText", 238, 0, 180, rowHeight, value, 40, Hex("E2E2EA"), TextAlignmentOptions.Right);
    }

    private static TextMeshProUGUI NewAfterRow(Transform box, int index, string label, Sprite icon, string value, Color valueColor,
        string bonus, Color bonusColor, out GameObject chip, out TextMeshProUGUI bonusText) {
        const float rowHeight = 68f;
        float y = 76f + index * rowHeight;
        RectTransform row = NewRect(box, "Row" + index, 0, y, 616, rowHeight);

        if (index > 0) NewImage(row, "Divider", 26, 0, 564, 3, UISprite("UI_RoundFill"), new Color(1f, 1f, 1f, 0.07f));
        NewIcon(row, "Icon", 26, 4, 60, 60, icon);
        NewText(row, "Label", 100, 0, 180, rowHeight, label, 30, Hex("F3E7B8"), TextAlignmentOptions.Left);
        TextMeshProUGUI valueText = NewText(row, "ValueText", 270, 0, 200, rowHeight, value, 52, valueColor, TextAlignmentOptions.Right);

        Image chipImage = NewImage(row, "BonusChip", 482, 12, 110, 44, UISprite("UI_RoundFill"), DARK);
        bonusText = NewText(chipImage.transform, "BonusText", 0, 0, 0, 0, bonus, 28, bonusColor, TextAlignmentOptions.Center);
        Stretch(bonusText.rectTransform, 4, 0, 4, 0);
        chip = chipImage.gameObject;
        return valueText;
    }

    // ---------- helper tao object ----------
    private static RectTransform NewRect(Transform parent, string name, float x, float y, float w, float h) {
        GameObject go = new GameObject(name, typeof(RectTransform));
        go.layer = LayerMask.NameToLayer("UI");
        RectTransform rect = go.GetComponent<RectTransform>();
        rect.SetParent(parent, false);
        SetRect(rect, x, y, w, h);
        return rect;
    }

    // neo goc tren trai cua parent, pivot o giua de scale tu tam
    private static void SetRect(RectTransform rect, float x, float y, float w, float h) {
        rect.anchorMin = new Vector2(0f, 1f);
        rect.anchorMax = new Vector2(0f, 1f);
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.sizeDelta = new Vector2(w, h);
        rect.anchoredPosition = new Vector2(x + w * 0.5f, -(y + h * 0.5f));
    }

    private static void Stretch(RectTransform rect, float inset) {
        Stretch(rect, inset, inset, inset, inset);
    }

    private static void Stretch(RectTransform rect, float left, float top, float right, float bottom) {
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.offsetMin = new Vector2(left, bottom);
        rect.offsetMax = new Vector2(-right, -top);
    }

    private static void StretchTop(RectTransform rect, float side, float top, float height) {
        rect.anchorMin = new Vector2(0f, 1f);
        rect.anchorMax = new Vector2(1f, 1f);
        rect.pivot = new Vector2(0.5f, 1f);
        rect.offsetMin = new Vector2(side, -top - height);
        rect.offsetMax = new Vector2(-side, -top);
    }

    private static Image NewImage(Transform parent, string name, float x, float y, float w, float h, Sprite sprite, Color color) {
        RectTransform rect = NewRect(parent, name, x, y, w, h);
        rect.gameObject.AddComponent<CanvasRenderer>();
        Image image = rect.gameObject.AddComponent<Image>();
        image.sprite = sprite;
        image.color = color;
        image.raycastTarget = false;
        if (sprite != null && sprite.border != Vector4.zero) image.type = Image.Type.Sliced;
        return image;
    }

    private static Image NewIcon(Transform parent, string name, float x, float y, float w, float h, Sprite sprite) {
        Image image = NewImage(parent, name, x, y, w, h, sprite, Color.white);
        image.preserveAspect = true;
        return image;
    }

    // UI_ArrowDown xoay 90 do de chi sang phai
    private static Image NewArrow(Transform parent, string name, float size, Color color) {
        Image image = NewIcon(parent, name, 0, 0, size, size, UISprite("UI_ArrowDown"));
        image.color = color;
        image.rectTransform.localEulerAngles = new Vector3(0f, 0f, 90f);
        LayoutElement layout = image.gameObject.AddComponent<LayoutElement>();
        layout.preferredWidth = size;
        layout.preferredHeight = size;
        return image;
    }

    private static Image NewSparkle(Transform parent, string name, float x, float y, float size) {
        Image image = NewImage(parent, name, x, y, size, size, UISprite("UI_Glow"), Hex("FFF6A8"));
        return image;
    }

    private static Button NewButton(Transform parent, string name, float x, float y, float w, float h, Color color) {
        Image image = NewImage(parent, name, x, y, w, h, UISprite("UI_Bevel"), color);
        image.raycastTarget = true;
        Button button = image.gameObject.AddComponent<Button>();
        button.targetGraphic = image;
        return button;
    }

    // soc sang cheo giong nut Sell
    private static void AddShine(Transform parent, float inset) {
        Image shine = NewImage(parent, "Shine", 0, 0, 0, 0, UISprite("UI_Stripes"), new Color(1f, 1f, 1f, 0.2f));
        shine.type = Image.Type.Tiled;
        Stretch(shine.rectTransform, inset);
    }

    private static TextMeshProUGUI NewText(Transform parent, string name, float x, float y, float w, float h, string text, float size, Color color, TextAlignmentOptions alignment) {
        RectTransform rect = NewRect(parent, name, x, y, w, h);
        rect.gameObject.AddComponent<CanvasRenderer>();
        TextMeshProUGUI tmp = rect.gameObject.AddComponent<TextMeshProUGUI>();
        tmp.font = font;
        tmp.fontSharedMaterial = fontMaterial;
        tmp.text = text;
        tmp.fontSize = size;
        tmp.color = color;
        tmp.alignment = alignment;
        tmp.enableWordWrapping = false;
        tmp.overflowMode = TextOverflowModes.Overflow;
        tmp.raycastTarget = false;
        return tmp;
    }

    private static TextMeshProUGUI NewLayoutText(Transform parent, string name, string text, float size, Color color) {
        TextMeshProUGUI tmp = NewText(parent, name, 0, 0, 100, size * 1.2f, text, size, color, TextAlignmentOptions.Center);
        return tmp;
    }

    private static Image NewLayoutIcon(Transform parent, string name, Sprite sprite, float w, float h) {
        Image image = NewIcon(parent, name, 0, 0, w, h, sprite);
        LayoutElement layout = image.gameObject.AddComponent<LayoutElement>();
        layout.preferredWidth = w;
        layout.preferredHeight = h;
        return image;
    }

    // canh hang ngang o giua, con tu lay preferred width; fitWidth thi tu co gian theo noi dung
    private static void AddHorizontalLayout(GameObject go, int left, int right, float spacing, bool fitWidth) {
        HorizontalLayoutGroup layout = go.AddComponent<HorizontalLayoutGroup>();
        layout.padding = new RectOffset(left, right, 0, 0);
        layout.spacing = spacing;
        layout.childAlignment = TextAnchor.MiddleCenter;
        layout.childControlWidth = true;
        layout.childControlHeight = false;
        layout.childForceExpandWidth = false;
        layout.childForceExpandHeight = false;

        if (fitWidth) {
            ContentSizeFitter fitter = go.AddComponent<ContentSizeFitter>();
            fitter.horizontalFit = ContentSizeFitter.FitMode.PreferredSize;
        }
    }

    // ---------- helper asset ----------
    private static Sprite UISprite(string name) {
        return AssetDatabase.LoadAssetAtPath<Sprite>(UI_PATH + name + ".png");
    }

    private static Sprite IconSprite(string name) {
        return AssetDatabase.LoadAssetAtPath<Sprite>(ICON_PATH + name + ".png");
    }

    private static Color Hex(string hex) {
        ColorUtility.TryParseHtmlString("#" + hex, out Color color);
        return color;
    }

    private static void Set(SerializedObject so, string field, Object value) {
        SerializedProperty property = so.FindProperty(field);
        if (property == null) {
            Debug.LogError("CanvasRebirthBuilder: khong tim thay field " + field);
            return;
        }
        property.objectReferenceValue = value;
    }

    private static Transform FindDeep(Transform parent, string name) {
        if (parent.name == name) return parent;
        foreach (Transform child in parent) {
            Transform result = FindDeep(child, name);
            if (result != null) return result;
        }
        return null;
    }
}
