using BreakInfinity;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class ToolAssignBrainrotIncome : EditorWindow {
    private static string DEFAULT_SO_FOLDER = "Assets/_Game/ScriptableObjects/BrainrotSOs";
    private static string PREFS_KEY = "ToolAssignBrainrotIncome_Settings";

    [Serializable]
    private class IncomeRange {
        public long min;
        public long max;
    }

    [Serializable]
    private class Settings {
        public IncomeRange[] incomeRanges;
        public float afkIncomePercent = 10f;
        public bool roundAfkIncome = true;
        public float sellPriceMultiplier = 10f;
        public bool roundSellPrice = true;
    }

    [SerializeField] private DefaultAsset soFolder;

    private Settings settings;
    private Vector2 scrollPosition;

    private void OnEnable() {
        if (soFolder == null) {
            soFolder = AssetDatabase.LoadAssetAtPath<DefaultAsset>(DEFAULT_SO_FOLDER);
        }

        LoadSettings();
    }

    private void OnDisable() {
        SaveSettings();
    }

    private void OnGUI() {
        soFolder = (DefaultAsset)EditorGUILayout.ObjectField("SO Folder", soFolder, typeof(DefaultAsset), false);

        scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);
        EditorGUI.BeginChangeCheck();

        // income amount: min max cho tung rarity
        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Income Amount (min - max)", EditorStyles.boldLabel);

        BrainrotRarity[] rarities = (BrainrotRarity[])Enum.GetValues(typeof(BrainrotRarity));
        foreach (BrainrotRarity rarity in rarities) {
            IncomeRange incomeRange = settings.incomeRanges[(int)rarity];

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField(rarity.ToString(), GUILayout.Width(80));
            EditorGUILayout.LabelField("Min", GUILayout.Width(30));
            incomeRange.min = EditorGUILayout.LongField(incomeRange.min);
            EditorGUILayout.LabelField("Max", GUILayout.Width(30));
            incomeRange.max = EditorGUILayout.LongField(incomeRange.max);
            EditorGUILayout.EndHorizontal();
        }

        EditorGUILayout.Space();

        if (GUILayout.Button("Assign Income Amount")) {
            AssignIncomeAmount();
        }

        // afk income amount = income amount * %
        EditorGUILayout.Space();
        EditorGUILayout.LabelField("AFK Income Amount", EditorStyles.boldLabel);

        settings.afkIncomePercent = EditorGUILayout.FloatField("AFK Income (%)", settings.afkIncomePercent);
        settings.roundAfkIncome = EditorGUILayout.Toggle("Round AFK Income", settings.roundAfkIncome);

        EditorGUILayout.Space();

        if (GUILayout.Button("Assign AFK Income Amount")) {
            AssignAfkIncomeAmount();
        }

        // sell price = income amount * multiplier
        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Sell Price", EditorStyles.boldLabel);

        settings.sellPriceMultiplier = EditorGUILayout.FloatField("Sell Price Multiplier", settings.sellPriceMultiplier);
        settings.roundSellPrice = EditorGUILayout.Toggle("Round Sell Price", settings.roundSellPrice);

        EditorGUILayout.Space();

        if (GUILayout.Button("Assign Sell Price")) {
            AssignSellPrice();
        }

        if (EditorGUI.EndChangeCheck()) {
            SaveSettings();
        }

        EditorGUILayout.EndScrollView();
    }

    [MenuItem("Tools/Brainrot/Brainrot Income Assigner")]
    private static void OpenWindow() {
        GetWindow<ToolAssignBrainrotIncome>("Brainrot Income Assigner");
    }

    // random income trong khoang min max cua rarity, roi lam tron giu 2 chu so dau
    private void AssignIncomeAmount() {
        if (soFolder == null) {
            Debug.LogError("SO folder is missing");
            return;
        }

        List<BrainrotSO> brainrotSOs = GetBrainrotSOs(AssetDatabase.GetAssetPath(soFolder));
        int assignedCount = 0;

        foreach (BrainrotSO brainrotSO in brainrotSOs) {
            IncomeRange incomeRange = settings.incomeRanges[(int)brainrotSO.GetRarity()];

            if (incomeRange.min < 0 || incomeRange.min > incomeRange.max) {
                Debug.LogError("Invalid income range for " + brainrotSO.GetRarity() + ", skip " + brainrotSO.name);
                continue;
            }

            long randomIncome = GetRandomLong(incomeRange.min, incomeRange.max);
            long incomeAmount = RoundIncomeInRange(randomIncome, incomeRange.min, incomeRange.max);

            SerializedObject serializedObject = new SerializedObject(brainrotSO);
            serializedObject.FindProperty("incomeAmount").floatValue = incomeAmount;
            serializedObject.ApplyModifiedProperties();

            EditorUtility.SetDirty(brainrotSO);
            Debug.Log(brainrotSO.name + " (" + brainrotSO.GetRarity() + "): " + randomIncome + " -> " + incomeAmount);
            assignedCount++;
        }

        AssetDatabase.SaveAssets();
        Debug.Log("Assigned income amount for " + assignedCount + " BrainrotSOs");
    }

    private void AssignAfkIncomeAmount() {
        if (soFolder == null) {
            Debug.LogError("SO folder is missing");
            return;
        }

        List<BrainrotSO> brainrotSOs = GetBrainrotSOs(AssetDatabase.GetAssetPath(soFolder));

        foreach (BrainrotSO brainrotSO in brainrotSOs) {
            float incomeAmount = brainrotSO.GetIncomeAmount();
            float afkIncomeAmount = incomeAmount * settings.afkIncomePercent / 100f;

            if (settings.roundAfkIncome) {
                long roundedAfkIncome = RoundIncome((long)Math.Round(afkIncomeAmount, MidpointRounding.AwayFromZero));

                // khong de afk income ve 0 khi income van > 0
                if (incomeAmount > 0 && settings.afkIncomePercent > 0) {
                    roundedAfkIncome = Math.Max(1, roundedAfkIncome);
                }

                afkIncomeAmount = roundedAfkIncome;
            }

            SerializedObject serializedObject = new SerializedObject(brainrotSO);
            serializedObject.FindProperty("afkIncomeAmount").floatValue = afkIncomeAmount;
            serializedObject.ApplyModifiedProperties();

            EditorUtility.SetDirty(brainrotSO);
            Debug.Log(brainrotSO.name + " (" + brainrotSO.GetRarity() + "): income " + incomeAmount + " -> afk " + afkIncomeAmount);
        }

        AssetDatabase.SaveAssets();
        Debug.Log("Assigned AFK income amount for " + brainrotSOs.Count + " BrainrotSOs");
    }

    private void AssignSellPrice() {
        if (soFolder == null) {
            Debug.LogError("SO folder is missing");
            return;
        }

        if (settings.sellPriceMultiplier < 0) {
            Debug.LogError("Sell price multiplier must be >= 0");
            return;
        }

        List<BrainrotSO> brainrotSOs = GetBrainrotSOs(AssetDatabase.GetAssetPath(soFolder));

        foreach (BrainrotSO brainrotSO in brainrotSOs) {
            float incomeAmount = brainrotSO.GetIncomeAmount();
            BigDouble sellPrice = new BigDouble(incomeAmount) * settings.sellPriceMultiplier;

            if (settings.roundSellPrice) {
                sellPrice = RoundSellPrice(sellPrice);
            }

            // sellPrice la BigDouble nen phai set qua mantissa va exponent
            SerializedObject serializedObject = new SerializedObject(brainrotSO);
            SerializedProperty sellPriceProperty = serializedObject.FindProperty("sellPrice");
            sellPriceProperty.FindPropertyRelative("mantissa").doubleValue = sellPrice.Mantissa;
            sellPriceProperty.FindPropertyRelative("exponent").longValue = sellPrice.Exponent;
            serializedObject.ApplyModifiedProperties();

            EditorUtility.SetDirty(brainrotSO);
            Debug.Log(brainrotSO.name + " (" + brainrotSO.GetRarity() + "): income " + incomeAmount + " -> sell price " + sellPrice);
        }

        AssetDatabase.SaveAssets();
        Debug.Log("Assigned sell price for " + brainrotSOs.Count + " BrainrotSOs");
    }

    // giong RoundIncome nhung cho BigDouble: < 10 lam tron so nguyen, con lai giu 2 chu so dau
    private BigDouble RoundSellPrice(BigDouble value) {
        if (value < 10) return Math.Round(value.ToDouble(), MidpointRounding.AwayFromZero);

        return new BigDouble(Math.Round(value.Mantissa, 1, MidpointRounding.AwayFromZero), value.Exponent);
    }

    // min va max deu duoc tinh
    private long GetRandomLong(long min, long max) {
        long randomValue = min + (long)(UnityEngine.Random.value * (max - min + 1));
        return Math.Min(randomValue, max);
    }

    // buoc lam tron: 10-99 -> 1, 100-999 -> 10, 1000-9999 -> 100, ... (giu 2 chu so dau)
    private long GetRoundStep(long value) {
        int digitCount = value.ToString().Length;
        long step = 1;

        for (int i = 0; i < digitCount - 2; i++) {
            step *= 10;
        }

        return step;
    }

    private long RoundIncome(long value) {
        if (value < 10) return value;

        long step = GetRoundStep(value);
        return (long)Math.Round((double)value / step, MidpointRounding.AwayFromZero) * step;
    }

    // neu lam tron bi vuot ra ngoai khoang min max thi lam tron xuong/len theo cung buoc
    private long RoundIncomeInRange(long value, long min, long max) {
        long roundedValue = RoundIncome(value);
        if (roundedValue >= min && roundedValue <= max) return roundedValue;

        long step = GetRoundStep(value);
        long floorValue = value / step * step;
        long ceilValue = floorValue == value ? value : floorValue + step;

        if (floorValue >= min && floorValue <= max) return floorValue;
        if (ceilValue >= min && ceilValue <= max) return ceilValue;

        return roundedValue;
    }

    // FindAssets tim ca trong cac folder con (Basic, Rare, ...)
    private List<BrainrotSO> GetBrainrotSOs(string soPath) {
        List<BrainrotSO> brainrotSOs = new List<BrainrotSO>();
        string[] soGuids = AssetDatabase.FindAssets("t:BrainrotSO", new string[] { soPath });

        foreach (string soGuid in soGuids) {
            BrainrotSO brainrotSO = AssetDatabase.LoadAssetAtPath<BrainrotSO>(AssetDatabase.GUIDToAssetPath(soGuid));
            if (brainrotSO == null) continue;

            brainrotSOs.Add(brainrotSO);
        }

        return brainrotSOs;
    }

    // luu cac gia tri da nhap vao EditorPrefs de lan sau mo tool khong phai nhap lai
    private void LoadSettings() {
        settings = null;

        if (EditorPrefs.HasKey(PREFS_KEY)) {
            settings = JsonUtility.FromJson<Settings>(EditorPrefs.GetString(PREFS_KEY));
        }

        if (settings == null) {
            settings = new Settings();
        }

        int rarityCount = Enum.GetValues(typeof(BrainrotRarity)).Length;
        if (settings.incomeRanges == null || settings.incomeRanges.Length != rarityCount) {
            IncomeRange[] oldIncomeRanges = settings.incomeRanges;
            settings.incomeRanges = new IncomeRange[rarityCount];

            for (int i = 0; i < rarityCount; i++) {
                bool hasOldRange = oldIncomeRanges != null && i < oldIncomeRanges.Length && oldIncomeRanges[i] != null;
                settings.incomeRanges[i] = hasOldRange ? oldIncomeRanges[i] : new IncomeRange();
            }
        }
    }

    private void SaveSettings() {
        if (settings == null) return;

        EditorPrefs.SetString(PREFS_KEY, JsonUtility.ToJson(settings));
    }
}
