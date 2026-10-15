using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

public class ToolAssignBrainrotIcons : EditorWindow {
    private static string DEFAULT_SO_FOLDER = "Assets/_Game/ScriptableObjects/BrainrotSOs";
    private static string DEFAULT_ICON_FOLDER = "Assets/_Game/Texture/BrainrotIcons";

    [SerializeField] private DefaultAsset soFolder;
    [SerializeField] private DefaultAsset iconFolder;
    [SerializeField] private bool isOverrideExisting = true;

    private void OnEnable() {
        if (soFolder == null) {
            soFolder = AssetDatabase.LoadAssetAtPath<DefaultAsset>(DEFAULT_SO_FOLDER);
        }

        if (iconFolder == null) {
            iconFolder = AssetDatabase.LoadAssetAtPath<DefaultAsset>(DEFAULT_ICON_FOLDER);
        }
    }

    private void OnGUI() {
        soFolder = (DefaultAsset)EditorGUILayout.ObjectField("SO Folder", soFolder, typeof(DefaultAsset), false);
        iconFolder = (DefaultAsset)EditorGUILayout.ObjectField("Icon Folder", iconFolder, typeof(DefaultAsset), false);
        isOverrideExisting = EditorGUILayout.Toggle("Override Existing Icon", isOverrideExisting);

        EditorGUILayout.Space();

        if (GUILayout.Button("Assign Icons")) {
            AssignIcons();
        }
    }

    [MenuItem("Tools/Brainrot/Brainrot Icon Assigner")]
    private static void OpenWindow() {
        GetWindow<ToolAssignBrainrotIcons>("Brainrot Icon Assigner");
    }

    // ten file icon phai trung voi brainrotName trong SO
    private void AssignIcons() {
        if (soFolder == null || iconFolder == null) {
            Debug.LogError("SO folder or icon folder is missing");
            return;
        }

        Dictionary<string, Sprite> iconDictionary = GetIconDictionary(AssetDatabase.GetAssetPath(iconFolder));
        List<BrainrotSO> brainrotSOs = GetBrainrotSOs(AssetDatabase.GetAssetPath(soFolder));
        int assignedCount = 0;

        foreach (BrainrotSO brainrotSO in brainrotSOs) {
            if (!isOverrideExisting && brainrotSO.GetIndexIcon() != null) continue;

            Sprite icon;
            if (!iconDictionary.TryGetValue(brainrotSO.GetName().Trim(), out icon)) {
                Debug.LogWarning("Missing icon for " + brainrotSO.name + " (" + brainrotSO.GetName() + ")");
                continue;
            }

            SerializedObject serializedObject = new SerializedObject(brainrotSO);
            serializedObject.FindProperty("indexIcon").objectReferenceValue = icon;
            serializedObject.ApplyModifiedPropertiesWithoutUndo();

            EditorUtility.SetDirty(brainrotSO);
            Debug.Log(brainrotSO.name + " -> " + icon.name);
            assignedCount++;
        }

        AssetDatabase.SaveAssets();
        Debug.Log("Assigned icon for " + assignedCount + "/" + brainrotSOs.Count + " BrainrotSOs");
    }

    // key la ten file khong co duoi, texture can de Texture Type = Sprite
    private Dictionary<string, Sprite> GetIconDictionary(string iconPath) {
        Dictionary<string, Sprite> iconDictionary = new Dictionary<string, Sprite>();
        string[] iconGuids = AssetDatabase.FindAssets("t:Sprite", new string[] { iconPath });

        foreach (string iconGuid in iconGuids) {
            string assetPath = AssetDatabase.GUIDToAssetPath(iconGuid);
            Sprite icon = AssetDatabase.LoadAssetAtPath<Sprite>(assetPath);
            if (icon == null) continue;

            iconDictionary[Path.GetFileNameWithoutExtension(assetPath)] = icon;
        }

        return iconDictionary;
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
}
