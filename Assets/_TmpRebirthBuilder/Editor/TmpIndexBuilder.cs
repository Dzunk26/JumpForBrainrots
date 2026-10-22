using System.Collections.Generic;
using System.IO;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;

public static class TmpIndexBuilder {
    private const string OUT_DIR = "Temp/UIPreview";
    private const string REBIRTH_PATH = "Assets/_Game/Resources/UI/Canvas-Rebirth.prefab";
    private const string INDEX_PATH = "Assets/_Game/Resources/UI/Canvas-Index.prefab";
    private const string INDEX_SLOT_PATH = "Assets/_Game/Prefabs/UI/IndexSlot.prefab";
    private const string UPGRADE_JUMP_PATH = "Assets/_Game/Resources/UI/Canvas-UpgradeJump.prefab";
    private const string INVENTORY_PATH = "Assets/_Game/Resources/UI/Canvas-Inventory.prefab";
    private const string INVENTORY_SLOT_PATH = "Assets/_Game/Prefabs/UI/InventorySlot.prefab";
    private const string UI_DIR = "Assets/_Game/Texture/UI/";
    private const string ICON_DIR = "Assets/_Game/Texture/Icon/";

    private static Sprite roundBorder, roundFill, bevel, stripes, white, checkCircle;
    private static TMP_FontAsset font;
    private static Material outlineMat;

    [MenuItem("Tools/Tmp/1. Fix Canvas Rebirth")]
    public static void FixRebirth() {
        LoadAssets();
        GameObject root = PrefabUtility.LoadPrefabContents(REBIRTH_PATH);
        Transform content = root.transform.Find("GameObject/Panel/Content");

        Image fill = content.Find("ProgressBar/FillMask/Fill").GetComponent<Image>();
        fill.sprite = white;
        fill.type = Image.Type.Filled;

        Transform rewardsRow = content.Find("RewardsRow");
        for (int i = 0; i < rewardsRow.childCount; i++) {
            Transform card = rewardsRow.GetChild(i);
            Transform oldValue = card.Find("OldValueText");
            SetRect((RectTransform)oldValue, new Vector2(0, 0.5f), new Vector2(0, 0.5f), new Vector2(188, 0), new Vector2(100, 60));
            TextMeshProUGUI oldText = oldValue.GetComponent<TextMeshProUGUI>();
            oldText.enableAutoSizing = false;
            oldText.fontSize = 38;
            Transform strike = card.Find("StrikeLine");
            SetRect((RectTransform)strike, new Vector2(0, 0.5f), new Vector2(0, 0.5f), new Vector2(188, 0), new Vector2(76, 6));
        }

        Transform headerTitle = root.transform.Find("GameObject/Panel/Header/TittleText");
        Stretch((RectTransform)headerTitle, new Vector2(150, 0), new Vector2(-130, 0));

        PrefabUtility.SaveAsPrefabAsset(root, REBIRTH_PATH);
        PrefabUtility.UnloadPrefabContents(root);
        AssetDatabase.SaveAssets();
        Render(REBIRTH_PATH, OUT_DIR + "/rebirth_after2.png", false);
    }

    [MenuItem("Tools/Tmp/2. Rebuild Canvas Index")]
    public static void RebuildIndex() {
        LoadAssets();
        Render(INDEX_PATH, OUT_DIR + "/index_before.png", true);
        BuildIndexSlot();
        BuildIndexCanvas();
        Render(INDEX_PATH, OUT_DIR + "/index_after.png", true);
    }

    private static void BuildIndexCanvas() {
        Color amber = new Color(1f, 0.68f, 0.1f);

        GameObject root = PrefabUtility.LoadPrefabContents(INDEX_PATH);
        Transform panel = root.transform.Find("GameObject/Panel");

        // panel
        SetImage(panel, roundBorder, new Color(0.19f, 0.19f, 0.21f), Image.Type.Sliced);
        Image pattern = NewImage("Pattern", panel, null, new Color(0.26f, 0.26f, 0.29f), Image.Type.Simple);
        Stretch(pattern.rectTransform, new Vector2(8, 8), new Vector2(-8, -8));
        pattern.transform.SetSiblingIndex(0);

        // header
        Transform header = panel.Find("Header");
        SetRect((RectTransform)header, new Vector2(0, 1), new Vector2(1, 1), new Vector2(0, -6), new Vector2(-12, 122), new Vector2(0.5f, 1));
        SetImage(header, roundBorder, amber, Image.Type.Sliced);
        Image highlight = NewImage("Highlight", header, roundFill, new Color(1f, 0.92f, 0.55f, 0.4f), Image.Type.Sliced);
        SetRect(highlight.rectTransform, new Vector2(0, 0.5f), new Vector2(1, 1), new Vector2(0, -6), new Vector2(-14, -6));
        highlight.transform.SetSiblingIndex(0);
        Image shine = NewImage("Shine", header, stripes, new Color(1, 1, 1, 0.22f), Image.Type.Simple);
        SetRect(shine.rectTransform, new Vector2(0.5f, 0), new Vector2(0.5f, 1), new Vector2(-40, 0), new Vector2(460, -12));
        shine.transform.SetSiblingIndex(1);

        SetRect((RectTransform)header.Find("IndexIcon"), new Vector2(0, 0.5f), new Vector2(0, 0.5f), new Vector2(75, 0), new Vector2(100, 100));
        Transform title = header.Find("TitleText");
        Stretch((RectTransform)title, new Vector2(150, 0), new Vector2(-450, 0));
        StyleText(title, 68, Color.white, TextAlignmentOptions.Left);

        Image pill = NewImage("ProgressPill", header, roundFill, new Color(0, 0, 0, 0.3f), Image.Type.Sliced);
        SetRect(pill.rectTransform, new Vector2(1, 0.5f), new Vector2(1, 0.5f), new Vector2(-270, 0), new Vector2(270, 84));
        Image pillIcon = NewImage("Icon", pill.transform, checkCircle, Color.white, Image.Type.Simple);
        pillIcon.preserveAspect = true;
        SetRect(pillIcon.rectTransform, new Vector2(0, 0.5f), new Vector2(0, 0.5f), new Vector2(42, 2), new Vector2(56, 56));
        Transform progressText = header.Find("TotalProgressText");
        progressText.SetParent(pill.transform, false);
        Stretch((RectTransform)progressText, new Vector2(72, 0), new Vector2(-12, 0));
        StyleText(progressText, 46, Color.white, TextAlignmentOptions.Center);
        TextMeshProUGUI progressTmp = progressText.GetComponent<TextMeshProUGUI>();
        progressTmp.enableAutoSizing = true;
        progressTmp.fontSizeMin = 24;
        progressTmp.fontSizeMax = 46;

        Transform close = header.Find("CloseButton");
        close.SetAsLastSibling();
        SetRect((RectTransform)close, new Vector2(1, 0.5f), new Vector2(1, 0.5f), new Vector2(-64, 0), new Vector2(100, 100));
        SetImage(close, bevel, new Color(0.85f, 0.12f, 0.12f), Image.Type.Sliced);
        Transform closeText = close.Find("Text");
        SetRect((RectTransform)closeText, Vector2.zero, Vector2.one, new Vector2(0, 3), new Vector2(0, -6));
        StyleText(closeText, 66, Color.white, TextAlignmentOptions.Center);

        // scroll view
        Transform scrollView = panel.Find("ScrollView");
        SetImage(scrollView, roundFill, new Color(0, 0, 0, 0.28f), Image.Type.Sliced);
        Stretch((RectTransform)scrollView, new Vector2(22, 22), new Vector2(-22, -140));
        Transform viewport = scrollView.Find("Viewport");
        Stretch((RectTransform)viewport, new Vector2(6, 6), new Vector2(-6, -6));
        RectTransform contentRt = (RectTransform)viewport.Find("Content");
        Vector2 contentSize = contentRt.sizeDelta;
        contentRt.anchorMin = new Vector2(0, 1);
        contentRt.anchorMax = new Vector2(1, 1);
        contentRt.pivot = new Vector2(0.5f, 1);
        contentRt.anchoredPosition = Vector2.zero;
        contentRt.sizeDelta = new Vector2(0, contentSize.y);
        GridLayoutGroup grid = contentRt.GetComponent<GridLayoutGroup>();
        grid.cellSize = new Vector2(250, 250);
        grid.spacing = new Vector2(18, 18);
        grid.padding = new RectOffset(16, 16, 16, 16);
        grid.childAlignment = TextAnchor.UpperCenter;
        grid.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
        grid.constraintCount = 4;

        PrefabUtility.SaveAsPrefabAsset(root, INDEX_PATH);
        PrefabUtility.UnloadPrefabContents(root);
        AssetDatabase.SaveAssets();
    }

    private static void BuildIndexSlot() {
        GameObject root = PrefabUtility.LoadPrefabContents(INDEX_SLOT_PATH);
        Transform slot = root.transform;

        Transform rarity = slot.Find("RarityText");
        Image rarityBg = NewImage("RarityBg", slot, roundFill, new Color(0, 0, 0, 0.3f), Image.Type.Sliced);
        SetRect(rarityBg.rectTransform, new Vector2(0, 1), new Vector2(1, 1), new Vector2(0, -27), new Vector2(-28, 42));
        rarityBg.transform.SetSiblingIndex(rarity.GetSiblingIndex());
        StyleText(rarity, 30, Color.white, TextAlignmentOptions.Center);
        TextMeshProUGUI rarityTmp = rarity.GetComponent<TextMeshProUGUI>();
        rarityTmp.enableAutoSizing = true;
        rarityTmp.fontSizeMin = 16;
        rarityTmp.fontSizeMax = 30;

        Transform nameTf = slot.Find("NameText");
        Image nameBg = NewImage("NameBg", slot, roundFill, new Color(0, 0, 0, 0.4f), Image.Type.Sliced);
        SetRect(nameBg.rectTransform, new Vector2(0, 0), new Vector2(1, 0), new Vector2(0, 27), new Vector2(-28, 44));
        nameBg.transform.SetSiblingIndex(nameTf.GetSiblingIndex());
        SetRect((RectTransform)nameTf, new Vector2(0, 0), new Vector2(1, 0), new Vector2(0, 27), new Vector2(-40, 42));
        StyleText(nameTf, 26, Color.white, TextAlignmentOptions.Center);
        TextMeshProUGUI nameTmp = nameTf.GetComponent<TextMeshProUGUI>();
        nameTmp.enableAutoSizing = true;
        nameTmp.fontSizeMin = 14;
        nameTmp.fontSizeMax = 26;

        PrefabUtility.SaveAsPrefabAsset(root, INDEX_SLOT_PATH);
        PrefabUtility.UnloadPrefabContents(root);
        AssetDatabase.SaveAssets();
    }

    private static void FillIndexPreview(GameObject inst) {
        SerializedObject so = new SerializedObject(inst.GetComponent<CanvasIndex>());
        ListBrainrotSO listBrainrotSO = (ListBrainrotSO)so.FindProperty("listBrainrotSO").objectReferenceValue;
        ListRarityConfigSO listRarityConfig = (ListRarityConfigSO)so.FindProperty("listRarityConfig").objectReferenceValue;
        IndexSlotUI slotPrefab = (IndexSlotUI)so.FindProperty("slotPrefab").objectReferenceValue;
        Transform slotParent = (Transform)so.FindProperty("slotParent").objectReferenceValue;
        TextMeshProUGUI totalText = (TextMeshProUGUI)so.FindProperty("totalProgressText").objectReferenceValue;

        int count = 0;
        for (int i = 0; i < listRarityConfig.GetRarityAmount() && count < 8; i++) {
            RarityConfigSO rarityConfig = listRarityConfig.GetRarityConfigByIndex(i);
            List<BrainrotSO> brainrotSOs = listBrainrotSO.GetBrainrotSOsByRarity(rarityConfig.GetBrainrotRarity());
            for (int j = 0; j < brainrotSOs.Count && count < 8; j++) {
                IndexSlotUI slot = Object.Instantiate(slotPrefab, slotParent);
                slot.OnInit(brainrotSOs[j], rarityConfig, count % 3 != 2);
                count++;
            }
        }
        totalText.SetText("12/48");
    }

    [MenuItem("Tools/Tmp/3. Rebuild Canvas UpgradeJump")]
    public static void RebuildUpgradeJump() {
        LoadAssets();
        Render(UPGRADE_JUMP_PATH, OUT_DIR + "/jump_before.png", false);
        BuildUpgradeJump();
        Render(UPGRADE_JUMP_PATH, OUT_DIR + "/jump_after.png", false);
    }

    private static void BuildUpgradeJump() {
        Color cyan = new Color(0.05f, 0.68f, 0.95f);
        Color yellow = new Color(1f, 0.92f, 0.25f);
        Sprite jumpIconSprite = LoadSprite(ICON_DIR + "JumpPowerIcon.png");
        Sprite glow = LoadSprite(UI_DIR + "UI_Glow.png");

        GameObject root = PrefabUtility.LoadPrefabContents(UPGRADE_JUMP_PATH);
        Transform panel = root.transform.Find("GameObject/Panel");

        // panel
        SetImage(panel, roundBorder, new Color(0.19f, 0.19f, 0.21f), Image.Type.Sliced);
        Image pattern = NewImage("Pattern", panel, null, new Color(0.26f, 0.26f, 0.29f), Image.Type.Simple);
        Stretch(pattern.rectTransform, new Vector2(8, 8), new Vector2(-8, -8));
        pattern.transform.SetSiblingIndex(0);

        // header
        Transform header = panel.Find("Header");
        SetRect((RectTransform)header, new Vector2(0, 1), new Vector2(1, 1), new Vector2(0, -6), new Vector2(-12, 122), new Vector2(0.5f, 1));
        SetImage(header, roundBorder, cyan, Image.Type.Sliced);
        Image highlight = NewImage("Highlight", header, roundFill, new Color(0.7f, 0.95f, 1f, 0.4f), Image.Type.Sliced);
        SetRect(highlight.rectTransform, new Vector2(0, 0.5f), new Vector2(1, 1), new Vector2(0, -6), new Vector2(-14, -6));
        highlight.transform.SetSiblingIndex(0);
        Image shine = NewImage("Shine", header, stripes, new Color(1, 1, 1, 0.22f), Image.Type.Simple);
        SetRect(shine.rectTransform, new Vector2(0.5f, 0), new Vector2(0.5f, 1), new Vector2(110, 0), new Vector2(460, -12));
        shine.transform.SetSiblingIndex(1);
        Image headerIcon = NewImage("Icon", header, jumpIconSprite, Color.white, Image.Type.Simple);
        headerIcon.preserveAspect = true;
        SetRect(headerIcon.rectTransform, new Vector2(0, 0.5f), new Vector2(0, 0.5f), new Vector2(75, 0), new Vector2(100, 100));
        headerIcon.transform.SetSiblingIndex(2);
        Transform title = header.Find("TittleText");
        Stretch((RectTransform)title, new Vector2(150, 0), new Vector2(-130, 0));
        StyleText(title, 68, Color.white, TextAlignmentOptions.Left);
        Transform close = header.Find("CloseButton");
        SetRect((RectTransform)close, new Vector2(1, 0.5f), new Vector2(1, 0.5f), new Vector2(-64, 0), new Vector2(100, 100));
        SetImage(close, bevel, new Color(0.85f, 0.12f, 0.12f), Image.Type.Sliced);
        Transform closeText = close.Find("Text (TMP)");
        SetRect((RectTransform)closeText, Vector2.zero, Vector2.one, new Vector2(0, 3), new Vector2(0, -6));
        StyleText(closeText, 66, Color.white, TextAlignmentOptions.Center);

        // content full panel
        Transform content = panel.Find("Content");
        Stretch((RectTransform)content, Vector2.zero, Vector2.zero);

        // progress bar
        Transform bar = content.Find("ProgressBar");
        RemoveShadow(bar);
        SetRect((RectTransform)bar, new Vector2(0.5f, 1), new Vector2(0.5f, 1), new Vector2(30, -180), new Vector2(1040, 64));
        SetImage(bar, roundBorder, new Color(0.09f, 0.09f, 0.1f), Image.Type.Sliced);
        Image fillMask = NewImage("FillMask", bar, roundFill, Color.white, Image.Type.Sliced);
        Stretch(fillMask.rectTransform, new Vector2(6, 6), new Vector2(-6, -6));
        fillMask.gameObject.AddComponent<Mask>().showMaskGraphic = false;
        fillMask.transform.SetSiblingIndex(0);
        Transform fill = bar.Find("Fill");
        fill.SetParent(fillMask.transform, false);
        Stretch((RectTransform)fill, Vector2.zero, Vector2.zero);
        Image fillImage = fill.GetComponent<Image>();
        fillImage.sprite = white;
        fillImage.color = new Color(1f, 0.82f, 0.1f);
        fillImage.type = Image.Type.Filled;
        fillImage.fillMethod = Image.FillMethod.Horizontal;
        fillImage.fillOrigin = (int)Image.OriginHorizontal.Left;
        fillImage.fillAmount = 0.6f;
        StyleText(bar.Find("ProgressText"), 40, Color.white, TextAlignmentOptions.Center);
        Image barIcon = NewImage("JumpIcon", bar, jumpIconSprite, Color.white, Image.Type.Simple);
        barIcon.preserveAspect = true;
        SetRect(barIcon.rectTransform, new Vector2(0, 0.5f), new Vector2(0, 0.5f), new Vector2(-20, 0), new Vector2(100, 100));

        // rows
        BuildUpgradeRow(content.Find("UpgradeRow_2"), -335, yellow, glow);
        BuildUpgradeRow(content.Find("UpgradeRow_10"), -553, yellow, glow);

        Transform row2 = content.Find("UpgradeRow_2");
        SetRect((RectTransform)row2.Find("Icon"), new Vector2(0, 0.5f), new Vector2(0, 0.5f), new Vector2(130, 0), new Vector2(170, 170));

        Transform jumpIcons = content.Find("UpgradeRow_10/JumpIcons");
        SetRect((RectTransform)jumpIcons, new Vector2(0, 0.5f), new Vector2(0, 0.5f), new Vector2(130, 0), new Vector2(220, 190));
        SetRect((RectTransform)jumpIcons.Find("Icon (1)"), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(25, 12), new Vector2(150, 150));
        SetRect((RectTransform)jumpIcons.Find("Icon"), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(-25, -14), new Vector2(150, 150));

        PrefabUtility.SaveAsPrefabAsset(root, UPGRADE_JUMP_PATH);
        PrefabUtility.UnloadPrefabContents(root);
        AssetDatabase.SaveAssets();
    }

    private static void BuildUpgradeRow(Transform row, float y, Color amountColor, Sprite glow) {
        RemoveShadow(row);
        SetRect((RectTransform)row, new Vector2(0.5f, 1), new Vector2(0.5f, 1), new Vector2(0, y), new Vector2(1120, 200));
        SetImage(row, roundBorder, new Color(0.25f, 0.3f, 0.39f), Image.Type.Sliced);

        Image highlight = NewImage("Highlight", row, roundFill, new Color(1, 1, 1, 0.12f), Image.Type.Sliced);
        SetRect(highlight.rectTransform, new Vector2(0, 0.5f), new Vector2(1, 1), new Vector2(0, -6), new Vector2(-14, -6));
        highlight.transform.SetSiblingIndex(0);
        Image glowImage = NewImage("Glow", row, glow, new Color(0.4f, 0.9f, 1f, 0.5f), Image.Type.Simple);
        SetRect(glowImage.rectTransform, new Vector2(0, 0.5f), new Vector2(0, 0.5f), new Vector2(130, 0), new Vector2(240, 240));
        glowImage.transform.SetSiblingIndex(1);

        Transform amount = row.Find("AmountText");
        SetRect((RectTransform)amount, new Vector2(0, 0.5f), new Vector2(0, 0.5f), new Vector2(400, 0), new Vector2(260, 140));
        StyleText(amount, 96, amountColor, TextAlignmentOptions.Center);

        BuildRowButton(row.Find("FreeUpgradeButton"), 46, new Color(0.66f, 0.2f, 0.95f), LoadSprite(ICON_DIR + "watch_icon.png"));
        BuildRowButton(row.Find("BuyButton"), -46, new Color(0.36f, 0.86f, 0.06f), LoadSprite(ICON_DIR + "MoneyIcon.png"));
    }

    private static void BuildRowButton(Transform button, float y, Color color, Sprite iconSprite) {
        RemoveShadow(button);
        SetRect((RectTransform)button, new Vector2(1, 0.5f), new Vector2(1, 0.5f), new Vector2(-210, y), new Vector2(380, 82));
        SetImage(button, bevel, color, Image.Type.Sliced);

        Button btn = button.GetComponent<Button>();
        ColorBlock colors = btn.colors;
        colors.disabledColor = new Color(0.55f, 0.55f, 0.55f, 1f);
        btn.colors = colors;

        Image icon = NewImage("Icon", button, iconSprite, Color.white, Image.Type.Simple);
        icon.preserveAspect = true;
        SetRect(icon.rectTransform, new Vector2(0, 0.5f), new Vector2(0, 0.5f), new Vector2(50, 2), new Vector2(64, 60));

        TextMeshProUGUI text = button.GetComponentInChildren<TextMeshProUGUI>();
        SetRect(text.rectTransform, Vector2.zero, Vector2.one, new Vector2(36, 3), new Vector2(-96, -6));
        StyleText(text.transform, 46, Color.white, TextAlignmentOptions.Center);
        text.enableAutoSizing = true;
        text.fontSizeMin = 24;
        text.fontSizeMax = 46;
    }

    [MenuItem("Tools/Tmp/4. Rebuild Canvas Inventory")]
    public static void RebuildInventory() {
        LoadAssets();
        RenderInventory(OUT_DIR + "/inventory_before.png");
        BuildInventorySlot();
        BuildInventoryCanvas();
        RenderInventory(OUT_DIR + "/inventory_after.png");
    }

    private static void BuildInventoryCanvas() {
        Color violet = new Color(0.5f, 0.32f, 0.95f);

        GameObject root = PrefabUtility.LoadPrefabContents(INVENTORY_PATH);
        Transform panel = root.transform.Find("Panel");
        SetRect((RectTransform)panel, new Vector2(0.5f, 1), new Vector2(0.5f, 1), new Vector2(0, -390), new Vector2(1200, 620));
        SetImage(panel, roundBorder, new Color(0.19f, 0.19f, 0.21f), Image.Type.Sliced);
        Image pattern = NewImage("Pattern", panel, null, new Color(0.26f, 0.26f, 0.29f), Image.Type.Simple);
        Stretch(pattern.rectTransform, new Vector2(8, 8), new Vector2(-8, -8));
        pattern.transform.SetSiblingIndex(0);

        // header moi
        Image header = NewImage("Header", panel, roundBorder, violet, Image.Type.Sliced);
        SetRect(header.rectTransform, new Vector2(0, 1), new Vector2(1, 1), new Vector2(0, -6), new Vector2(-12, 122), new Vector2(0.5f, 1));
        Image highlight = NewImage("Highlight", header.transform, roundFill, new Color(0.85f, 0.75f, 1f, 0.4f), Image.Type.Sliced);
        SetRect(highlight.rectTransform, new Vector2(0, 0.5f), new Vector2(1, 1), new Vector2(0, -6), new Vector2(-14, -6));
        Image shine = NewImage("Shine", header.transform, stripes, new Color(1, 1, 1, 0.22f), Image.Type.Simple);
        SetRect(shine.rectTransform, new Vector2(0.5f, 0), new Vector2(0.5f, 1), new Vector2(110, 0), new Vector2(460, -12));
        Image headerIcon = NewImage("Icon", header.transform, LoadSprite(ICON_DIR + "BackpackIcon.png"), Color.white, Image.Type.Simple);
        headerIcon.preserveAspect = true;
        SetRect(headerIcon.rectTransform, new Vector2(0, 0.5f), new Vector2(0, 0.5f), new Vector2(75, 0), new Vector2(100, 100));

        GameObject titleGo = new GameObject("TitleText", typeof(RectTransform), typeof(CanvasRenderer), typeof(TextMeshProUGUI));
        titleGo.layer = 5;
        titleGo.transform.SetParent(header.transform, false);
        titleGo.GetComponent<TextMeshProUGUI>().SetText("Backpack");
        Stretch((RectTransform)titleGo.transform, new Vector2(150, 0), new Vector2(-130, 0));
        StyleText(titleGo.transform, 68, Color.white, TextAlignmentOptions.Left);

        Transform close = panel.Find("CloseButton");
        close.SetParent(header.transform, false);
        SetRect((RectTransform)close, new Vector2(1, 0.5f), new Vector2(1, 0.5f), new Vector2(-64, 0), new Vector2(100, 100));
        SetImage(close, bevel, new Color(0.85f, 0.12f, 0.12f), Image.Type.Sliced);
        Transform closeText = close.Find("Text (TMP)");
        SetRect((RectTransform)closeText, Vector2.zero, Vector2.one, new Vector2(0, 3), new Vector2(0, -6));
        StyleText(closeText, 66, Color.white, TextAlignmentOptions.Center);

        // scroll view
        Transform scrollView = panel.Find("Scroll View");
        SetImage(scrollView, roundFill, new Color(0, 0, 0, 0.28f), Image.Type.Sliced);
        Stretch((RectTransform)scrollView, new Vector2(22, 22), new Vector2(-22, -140));
        Transform viewport = scrollView.Find("Viewport");
        Stretch((RectTransform)viewport, new Vector2(6, 6), new Vector2(-6, -6));
        SetImage(viewport, roundFill, Color.white, Image.Type.Sliced);
        RectTransform contentRt = (RectTransform)viewport.Find("Content");
        Vector2 contentSize = contentRt.sizeDelta;
        contentRt.anchorMin = new Vector2(0, 1);
        contentRt.anchorMax = new Vector2(1, 1);
        contentRt.pivot = new Vector2(0.5f, 1);
        contentRt.anchoredPosition = Vector2.zero;
        contentRt.sizeDelta = new Vector2(0, contentSize.y);
        GridLayoutGroup grid = contentRt.GetComponent<GridLayoutGroup>();
        grid.cellSize = new Vector2(196, 196);
        grid.spacing = new Vector2(16, 16);
        grid.padding = new RectOffset(16, 16, 16, 16);
        grid.childAlignment = TextAnchor.UpperCenter;
        grid.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
        grid.constraintCount = 5;

        header.transform.SetAsLastSibling();

        PrefabUtility.SaveAsPrefabAsset(root, INVENTORY_PATH);
        PrefabUtility.UnloadPrefabContents(root);
        AssetDatabase.SaveAssets();
    }

    // prefab nay dung chung cho hotbar, chi doi style trung tinh
    private static void BuildInventorySlot() {
        GameObject root = PrefabUtility.LoadPrefabContents(INVENTORY_SLOT_PATH);
        Transform slot = root.transform;

        SetImage(slot, roundBorder, new Color(0.16f, 0.16f, 0.18f, 0.95f), Image.Type.Sliced);
        Image inner = NewImage("Inner", slot, roundFill, new Color(1, 1, 1, 0.06f), Image.Type.Sliced);
        Stretch(inner.rectTransform, new Vector2(6, 6), new Vector2(-6, -6));
        inner.transform.SetSiblingIndex(0);

        Transform frame = slot.Find("SelectedFrame");
        Stretch((RectTransform)frame, new Vector2(-4, -4), new Vector2(4, 4));
        SetImage(frame, LoadSprite(UI_DIR + "UI_RoundOutline.png"), new Color(0.4f, 1f, 0.15f), Image.Type.Sliced);
        frame.SetAsLastSibling();

        Image icon = slot.Find("Icon").GetComponent<Image>();
        Stretch(icon.rectTransform, new Vector2(8, 26), new Vector2(-8, -6));
        icon.preserveAspect = true;

        Transform income = slot.Find("IncomeText");
        SetRect((RectTransform)income, new Vector2(0, 0), new Vector2(1, 0), new Vector2(0, 20), new Vector2(-8, 36));
        StyleText(income, 28, new Color(0.33f, 1f, 0.15f), TextAlignmentOptions.Center);
        TextMeshProUGUI incomeTmp = income.GetComponent<TextMeshProUGUI>();
        incomeTmp.enableAutoSizing = true;
        incomeTmp.fontSizeMin = 14;
        incomeTmp.fontSizeMax = 28;

        PrefabUtility.SaveAsPrefabAsset(root, INVENTORY_SLOT_PATH);
        PrefabUtility.UnloadPrefabContents(root);
        AssetDatabase.SaveAssets();
    }

    private static void RenderInventory(string file) {
        ListBrainrotSO listBrainrotSO = LoadFirst<ListBrainrotSO>();
        ListRarityConfigSO listRarityConfig = LoadFirst<ListRarityConfigSO>();
        GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(INVENTORY_PATH);
        SerializedObject so = new SerializedObject(prefab.GetComponent<CanvasInventory>());
        string slotParentPath = AnimationUtility.CalculateTransformPath((Transform)so.FindProperty("slotParent").objectReferenceValue, prefab.transform);

        Render(INVENTORY_PATH, file, inst => {
            InventorySlotUI slotPrefab = AssetDatabase.LoadAssetAtPath<InventorySlotUI>(INVENTORY_SLOT_PATH);
            Transform slotParent = inst.transform.Find(slotParentPath);
            int count = 0;
            for (int i = 0; i < listRarityConfig.GetRarityAmount() && count < 7; i++) {
                List<BrainrotSO> brainrotSOs = listBrainrotSO.GetBrainrotSOsByRarity(listRarityConfig.GetRarityConfigByIndex(i).GetBrainrotRarity());
                for (int j = 0; j < brainrotSOs.Count && count < 7; j++) {
                    InventorySlotUI slot = Object.Instantiate(slotPrefab, slotParent);
                    slot.OnInit(SlotType.Backpack, count, brainrotSOs[j], count == 1);
                    count++;
                }
            }
        });
    }

    private static T LoadFirst<T>() where T : Object {
        string[] guids = AssetDatabase.FindAssets("t:" + typeof(T).Name);
        return AssetDatabase.LoadAssetAtPath<T>(AssetDatabase.GUIDToAssetPath(guids[0]));
    }

    private static void RemoveShadow(Transform tf) {
        Shadow shadow = tf.GetComponent<Shadow>();
        if (shadow != null) Object.DestroyImmediate(shadow, true);
    }

    private static void LoadAssets() {
        roundBorder = LoadSprite(UI_DIR + "UI_RoundBorder.png");
        roundFill = LoadSprite(UI_DIR + "UI_RoundFill.png");
        bevel = LoadSprite(UI_DIR + "UI_Bevel.png");
        stripes = LoadSprite(UI_DIR + "UI_Stripes.png");
        white = LoadSprite(UI_DIR + "UI_White.png");
        checkCircle = LoadSprite(UI_DIR + "UI_CheckCircle.png");
        font = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>("Assets/_Game/Fonts/Fredoka-Bold SDF.asset");
        outlineMat = AssetDatabase.LoadAssetAtPath<Material>("Assets/_Game/Fonts/Fredoka-Bold SDF Outline.mat");
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
        if (sprite == null) Debug.LogError("[TmpIndexBuilder] missing sprite " + path);
        return sprite;
    }

    private static void Render(string prefabPath, string file, bool isIndex) {
        Render(prefabPath, file, isIndex ? FillIndexPreview : (System.Action<GameObject>)null);
    }

    private static void Render(string prefabPath, string file, System.Action<GameObject> setup) {
        Directory.CreateDirectory(Path.GetDirectoryName(file));
        UnityEngine.SceneManagement.Scene scene = EditorSceneManager.NewPreviewScene();
        GameObject camGo = new GameObject("Cam");
        UnityEngine.SceneManagement.SceneManager.MoveGameObjectToScene(camGo, scene);
        Camera cam = camGo.AddComponent<Camera>();
        cam.scene = scene;
        cam.clearFlags = CameraClearFlags.SolidColor;
        cam.backgroundColor = new Color(0.45f, 0.7f, 0.9f);
        RenderTexture rt = new RenderTexture(1920, 1080, 24, RenderTextureFormat.ARGB32);
        cam.targetTexture = rt;

        GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
        GameObject inst = (GameObject)PrefabUtility.InstantiatePrefab(prefab, scene);
        Canvas canvas = inst.GetComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceCamera;
        canvas.worldCamera = cam;
        canvas.planeDistance = 5;
        CanvasScaler scaler = inst.GetComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        scaler.matchWidthOrHeight = 0.5f;
        if (setup != null) setup(inst);

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
        Debug.Log("[TmpIndexBuilder] rendered " + file);
    }
}
