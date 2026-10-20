using System.IO;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;

public static class TmpRebirthBuilder {
    private const string PREFAB_PATH = "Assets/_Game/Resources/UI/Canvas-Rebirth.prefab";
    private const string UI_DIR = "Assets/_Game/Texture/UI/";
    private const string ICON_DIR = "Assets/_Game/Texture/Icon/";

    private static Sprite roundBorder, roundFill, bevel, stripes, glow, arrowDown;
    private static TMP_FontAsset font;
    private static Material outlineMat;

    private const string OUT_DIR = "C:/Users/Admin/AppData/Local/Temp/claude/C--Users-Admin-JumpForBrainrots/1f758751-90fe-4928-bf95-8045333f3507/scratchpad";

    public static void Run() {
        string outDir = OUT_DIR;
        Render(Path.Combine(outDir, "before.png"));
        Build();
        Render(Path.Combine(outDir, "after.png"));
    }

    private static void Build() {
        roundBorder = LoadSprite(UI_DIR + "UI_RoundBorder.png");
        roundFill = LoadSprite(UI_DIR + "UI_RoundFill.png");
        bevel = LoadSprite(UI_DIR + "UI_Bevel.png");
        stripes = LoadSprite(UI_DIR + "UI_Stripes.png");
        glow = LoadSprite(UI_DIR + "UI_Glow.png");
        arrowDown = LoadSprite(UI_DIR + "UI_ArrowDown.png");
        font = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>("Assets/_Game/Fonts/Fredoka-Bold SDF.asset");
        outlineMat = AssetDatabase.LoadAssetAtPath<Material>("Assets/_Game/Fonts/Fredoka-Bold SDF Outline.mat");

        Color pink = new Color(0.86f, 0.2f, 0.75f);
        Color green = new Color(0.36f, 0.86f, 0.06f);
        Color dark = new Color(0.09f, 0.09f, 0.1f);
        Color yellow = new Color(1f, 0.92f, 0.25f);

        GameObject root = PrefabUtility.LoadPrefabContents(PREFAB_PATH);
        Transform panel = root.transform.Find("GameObject/Panel");

        // panel
        SetImage(panel, roundBorder, new Color(0.19f, 0.19f, 0.21f), Image.Type.Sliced);
        Image pattern = NewImage("Pattern", panel, null, new Color(0.26f, 0.26f, 0.29f), Image.Type.Simple);
        Stretch(pattern.rectTransform, new Vector2(8, 8), new Vector2(-8, -8));
        pattern.transform.SetSiblingIndex(0);

        // header
        Transform header = panel.Find("Header");
        RectTransform headerRt = (RectTransform)header;
        SetRect(headerRt, new Vector2(0, 1), new Vector2(1, 1), new Vector2(0, -6), new Vector2(-12, 122), new Vector2(0.5f, 1));
        SetImage(header, roundBorder, pink, Image.Type.Sliced);
        Image highlight = NewImage("Highlight", header, roundFill, new Color(1f, 0.65f, 0.95f, 0.35f), Image.Type.Sliced);
        SetRect(highlight.rectTransform, new Vector2(0, 0.5f), new Vector2(1, 1), new Vector2(0, -6), new Vector2(-14, -6));
        highlight.transform.SetSiblingIndex(0);
        Image shine = NewImage("Shine", header, stripes, new Color(1, 1, 1, 0.22f), Image.Type.Simple);
        SetRect(shine.rectTransform, new Vector2(0.5f, 0), new Vector2(0.5f, 1), new Vector2(110, 0), new Vector2(460, -12));
        shine.transform.SetSiblingIndex(1);
        SetRect((RectTransform)header.Find("RebirthIcon"), new Vector2(0, 0.5f), new Vector2(0, 0.5f), new Vector2(75, 0), new Vector2(100, 100));
        Transform headerTitle = header.Find("TittleText");
        SetRect((RectTransform)headerTitle, Vector2.zero, Vector2.one, new Vector2(145, 0), new Vector2(-295, 0));
        StyleText(headerTitle, 68, Color.white, TextAlignmentOptions.Left);
        Transform cancel = header.Find("CancelButton");
        SetRect((RectTransform)cancel, new Vector2(1, 0.5f), new Vector2(1, 0.5f), new Vector2(-64, 0), new Vector2(100, 100));
        SetImage(cancel, bevel, new Color(0.85f, 0.12f, 0.12f), Image.Type.Sliced);
        Transform cancelText = cancel.Find("Text (TMP)");
        SetRect((RectTransform)cancelText, Vector2.zero, Vector2.one, new Vector2(0, 3), new Vector2(0, -6));
        StyleText(cancelText, 66, Color.white, TextAlignmentOptions.Center);

        // content full panel de toa do tinh theo panel
        Transform content = panel.Find("Content");
        Stretch((RectTransform)content, Vector2.zero, Vector2.zero);

        Transform rewardTitle = content.Find("TittleText");
        Transform rewardsRow = rewardTitle.Find("RewardsRow");
        rewardsRow.SetParent(content, false);
        rewardTitle.name = "RewardTitleText";
        SetRect((RectTransform)rewardTitle, new Vector2(0.5f, 1), new Vector2(0.5f, 1), new Vector2(0, -170), new Vector2(1000, 60));
        StyleText(rewardTitle, 52, yellow, TextAlignmentOptions.Center);

        SetRect((RectTransform)rewardsRow, new Vector2(0.5f, 1), new Vector2(0.5f, 1), new Vector2(0, -280), new Vector2(1100, 130));
        rewardsRow.SetSiblingIndex(rewardTitle.GetSiblingIndex() + 1);
        BuildRewardCard(rewardsRow.Find("MoneyReward"), -275, new Color(0.18f, 0.55f, 0.11f), new Color(0.77f, 1f, 0.42f), yellow);
        BuildRewardCard(rewardsRow.Find("SpeedReward"), 275, new Color(0.12f, 0.39f, 0.79f), new Color(0.56f, 0.9f, 1f), yellow);

        // warning
        Transform warning = content.Find("WarningText");
        SetRect((RectTransform)warning, new Vector2(0.5f, 1), new Vector2(0.5f, 1), new Vector2(0, -375), new Vector2(900, 54));
        StyleText(warning, 34, new Color(1f, 0.54f, 0.54f), TextAlignmentOptions.Center);
        Image warningBg = NewImage("WarningBg", content, roundFill, new Color(0, 0, 0, 0.28f), Image.Type.Sliced);
        SetRect(warningBg.rectTransform, new Vector2(0.5f, 1), new Vector2(0.5f, 1), new Vector2(0, -375), new Vector2(900, 54));
        warningBg.transform.SetSiblingIndex(warning.GetSiblingIndex());

        // progress bar
        Transform bar = content.Find("ProgressBar");
        RemoveShadow(bar);
        SetRect((RectTransform)bar, new Vector2(0.5f, 1), new Vector2(0.5f, 1), new Vector2(30, -445), new Vector2(940, 64));
        SetImage(bar, roundBorder, dark, Image.Type.Sliced);
        Image fillMask = NewImage("FillMask", bar, roundFill, Color.white, Image.Type.Sliced);
        Stretch(fillMask.rectTransform, new Vector2(6, 6), new Vector2(-6, -6));
        fillMask.gameObject.AddComponent<Mask>().showMaskGraphic = false;
        fillMask.transform.SetSiblingIndex(0);
        Transform fill = bar.Find("Fill");
        fill.SetParent(fillMask.transform, false);
        Stretch((RectTransform)fill, Vector2.zero, Vector2.zero);
        Image fillImage = fill.GetComponent<Image>();
        fillImage.sprite = null;
        fillImage.color = green;
        fillImage.type = Image.Type.Filled;
        fillImage.fillMethod = Image.FillMethod.Horizontal;
        fillImage.fillOrigin = (int)Image.OriginHorizontal.Left;
        fillImage.fillAmount = 0.76f;
        Transform progressText = bar.Find("ProgressText");
        StyleText(progressText, 38, Color.white, TextAlignmentOptions.Center);
        Image jumpIcon = NewImage("JumpIcon", bar, LoadSprite(ICON_DIR + "JumpPowerIcon.png"), Color.white, Image.Type.Simple);
        jumpIcon.preserveAspect = true;
        SetRect(jumpIcon.rectTransform, new Vector2(0, 0.5f), new Vector2(0, 0.5f), new Vector2(-20, 0), new Vector2(100, 100));

        // footer
        Image footer = NewImage("Footer", content, roundBorder, dark, Image.Type.Sliced);
        SetRect(footer.rectTransform, new Vector2(0, 0), new Vector2(1, 0), new Vector2(0, 22), new Vector2(-44, 150), new Vector2(0.5f, 0));
        BuildFooterButton(content.Find("FreeRebirthButton"), footer.transform, -260, green, LoadSprite(ICON_DIR + "watch_icon.png"));
        BuildFooterButton(content.Find("RebirthButton"), footer.transform, 260, pink, LoadSprite(ICON_DIR + "RebirthIcon.png"));

        PrefabUtility.SaveAsPrefabAsset(root, PREFAB_PATH);
        PrefabUtility.UnloadPrefabContents(root);
        AssetDatabase.SaveAssets();
    }

    private static void BuildRewardCard(Transform card, float x, Color bgColor, Color newColor, Color arrowColor) {
        RemoveShadow(card);
        SetRect((RectTransform)card, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(x, 0), new Vector2(520, 130));
        SetImage(card, roundBorder, bgColor, Image.Type.Sliced);

        Image highlight = NewImage("Highlight", card, roundFill, new Color(1, 1, 1, 0.22f), Image.Type.Sliced);
        SetRect(highlight.rectTransform, new Vector2(0, 0.5f), new Vector2(1, 1), new Vector2(0, -5), new Vector2(-12, -5));
        highlight.transform.SetSiblingIndex(0);
        Image glowImage = NewImage("Glow", card, glow, new Color(1, 1, 0.4f, 0.5f), Image.Type.Simple);
        SetRect(glowImage.rectTransform, new Vector2(0, 0.5f), new Vector2(0, 0.5f), new Vector2(80, 0), new Vector2(150, 150));
        glowImage.transform.SetSiblingIndex(1);

        SetRect((RectTransform)card.Find("Icon"), new Vector2(0, 0.5f), new Vector2(0, 0.5f), new Vector2(80, 0), new Vector2(110, 110));

        Transform oldValue = card.Find("OldValueText");
        SetRect((RectTransform)oldValue, new Vector2(0, 0.5f), new Vector2(0, 0.5f), new Vector2(180, 0), new Vector2(90, 60));
        StyleText(oldValue, 40, new Color(0.85f, 0.85f, 0.85f), TextAlignmentOptions.Center);
        TextMeshProUGUI oldText = oldValue.GetComponent<TextMeshProUGUI>();
        oldText.enableAutoSizing = true;
        oldText.fontSizeMin = 20;
        oldText.fontSizeMax = 40;

        Transform strike = card.Find("StrikeLine");
        SetRect((RectTransform)strike, new Vector2(0, 0.5f), new Vector2(0, 0.5f), new Vector2(180, 0), new Vector2(90, 8));
        SetImage(strike, roundFill, new Color(0.95f, 0.15f, 0.15f), Image.Type.Sliced);
        strike.localEulerAngles = new Vector3(0, 0, 8);
        strike.SetSiblingIndex(oldValue.GetSiblingIndex() + 1);

        Transform arrowText = card.Find("ArrowText");
        int arrowIndex = arrowText.GetSiblingIndex();
        Object.DestroyImmediate(arrowText.gameObject);
        Image arrow = NewImage("Arrow", card, arrowDown, arrowColor, Image.Type.Simple);
        SetRect(arrow.rectTransform, new Vector2(0, 0.5f), new Vector2(0, 0.5f), new Vector2(250, 0), new Vector2(44, 44));
        arrow.rectTransform.localEulerAngles = new Vector3(0, 0, 90);
        arrow.transform.SetSiblingIndex(arrowIndex);

        Transform newValue = card.Find("NewValueText");
        RectTransform newRt = (RectTransform)newValue;
        newRt.anchorMin = new Vector2(0, 0.5f);
        newRt.anchorMax = new Vector2(1, 0.5f);
        newRt.pivot = new Vector2(0.5f, 0.5f);
        newRt.offsetMin = new Vector2(282, -55);
        newRt.offsetMax = new Vector2(-14, 55);
        StyleText(newValue, 46, newColor, TextAlignmentOptions.Left);
        TextMeshProUGUI newText = newValue.GetComponent<TextMeshProUGUI>();
        newText.enableAutoSizing = true;
        newText.fontSizeMin = 24;
        newText.fontSizeMax = 46;
    }

    private static void BuildFooterButton(Transform button, Transform footer, float x, Color color, Sprite iconSprite) {
        button.SetParent(footer, false);
        RemoveShadow(button);
        SetRect((RectTransform)button, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(x, 0), new Vector2(480, 110));
        SetImage(button, bevel, color, Image.Type.Sliced);

        Button btn = button.GetComponent<Button>();
        ColorBlock colors = btn.colors;
        colors.disabledColor = new Color(0.55f, 0.55f, 0.55f, 1f);
        btn.colors = colors;

        Image icon = NewImage("Icon", button, iconSprite, Color.white, Image.Type.Simple);
        icon.preserveAspect = true;
        SetRect(icon.rectTransform, new Vector2(0, 0.5f), new Vector2(0, 0.5f), new Vector2(70, 2), new Vector2(80, 80));

        TextMeshProUGUI text = button.GetComponentInChildren<TextMeshProUGUI>();
        SetRect(text.rectTransform, Vector2.zero, Vector2.one, new Vector2(40, 3), new Vector2(-120, -6));
        StyleText(text.transform, 56, Color.white, TextAlignmentOptions.Center);
        text.enableAutoSizing = true;
        text.fontSizeMin = 28;
        text.fontSizeMax = 56;
    }

    private static Image NewImage(string name, Transform parent, Sprite sprite, Color color, Image.Type type) {
        GameObject go = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
        go.layer = 5;
        go.transform.SetParent(parent, false);
        Image image = go.GetComponent<Image>();
        image.sprite = sprite;
        image.color = color;
        image.type = type;
        image.raycastTarget = false;
        return image;
    }

    private static void SetImage(Transform tf, Sprite sprite, Color color, Image.Type type) {
        Image image = tf.GetComponent<Image>();
        image.sprite = sprite;
        image.color = color;
        image.type = type;
    }

    private static void StyleText(Transform tf, float size, Color color, TextAlignmentOptions alignment) {
        TextMeshProUGUI text = tf.GetComponent<TextMeshProUGUI>();
        text.font = font;
        text.fontSharedMaterial = outlineMat;
        text.fontSize = size;
        text.color = color;
        text.alignment = alignment;
        text.enableWordWrapping = false;
        text.raycastTarget = false;
    }

    private static void RemoveShadow(Transform tf) {
        Shadow shadow = tf.GetComponent<Shadow>();
        if (shadow != null) Object.DestroyImmediate(shadow, true);
    }

    private static void SetRect(RectTransform rt, Vector2 aMin, Vector2 aMax, Vector2 pos, Vector2 size) {
        SetRect(rt, aMin, aMax, pos, size, new Vector2(0.5f, 0.5f));
    }

    private static void SetRect(RectTransform rt, Vector2 aMin, Vector2 aMax, Vector2 pos, Vector2 size, Vector2 pivot) {
        rt.anchorMin = aMin;
        rt.anchorMax = aMax;
        rt.pivot = pivot;
        rt.anchoredPosition = pos;
        rt.sizeDelta = size;
    }

    private static void Stretch(RectTransform rt, Vector2 offsetMin, Vector2 offsetMax) {
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.pivot = new Vector2(0.5f, 0.5f);
        rt.offsetMin = offsetMin;
        rt.offsetMax = offsetMax;
    }

    private static Sprite LoadSprite(string path) {
        Sprite sprite = AssetDatabase.LoadAssetAtPath<Sprite>(path);
        if (sprite == null) Debug.LogError("[RebirthBuilder] missing sprite " + path);
        return sprite;
    }

    private static void Render(string file) {
        UnityEngine.SceneManagement.Scene scene = EditorSceneManager.NewPreviewScene();
        GameObject camGo = new GameObject("Cam");
        UnityEngine.SceneManagement.SceneManager.MoveGameObjectToScene(camGo, scene);
        Camera cam = camGo.AddComponent<Camera>();
        cam.scene = scene;
        cam.clearFlags = CameraClearFlags.SolidColor;
        cam.backgroundColor = new Color(0.45f, 0.7f, 0.9f);
        RenderTexture rt = new RenderTexture(1920, 1080, 24, RenderTextureFormat.ARGB32);
        cam.targetTexture = rt;

        GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(PREFAB_PATH);
        GameObject inst = (GameObject)PrefabUtility.InstantiatePrefab(prefab, scene);
        Canvas canvas = inst.GetComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceCamera;
        canvas.worldCamera = cam;
        canvas.planeDistance = 5;
        CanvasScaler scaler = inst.GetComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        scaler.matchWidthOrHeight = 0.5f;

        Canvas.ForceUpdateCanvases();
        cam.Render();
        Canvas.ForceUpdateCanvases();
        cam.Render();

        RenderTexture.active = rt;
        Texture2D tex = new Texture2D(1920, 1080, TextureFormat.RGB24, false);
        tex.ReadPixels(new Rect(0, 0, 1920, 1080), 0, 0);
        tex.Apply();
        RenderTexture.active = null;
        File.WriteAllBytes(file, tex.EncodeToPNG());
        EditorSceneManager.ClosePreviewScene(scene);
        rt.Release();
        Debug.Log("[RebirthBuilder] rendered " + file);
    }
}
