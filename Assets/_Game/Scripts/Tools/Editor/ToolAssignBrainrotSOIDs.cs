using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class ToolAssignBrainrotSOIDs : EditorWindow {
    private static string DEFAULT_SO_FOLDER = "Assets/_Game/ScriptableObjects/BrainrotSOs";

    [SerializeField] private DefaultAsset soFolder;

    private void OnEnable() {
        if (soFolder == null) {
            soFolder = AssetDatabase.LoadAssetAtPath<DefaultAsset>(DEFAULT_SO_FOLDER);
        }
    }

    private void OnGUI() {
        soFolder = (DefaultAsset)EditorGUILayout.ObjectField("SO Folder", soFolder, typeof(DefaultAsset), false);

        EditorGUILayout.Space();

        if (GUILayout.Button("Assign IDs")) {
            AssignIDs();
        }
    }

    [MenuItem("Tools/Brainrot/Brainrot SO ID Assigner")]
    private static void OpenWindow() {
        GetWindow<ToolAssignBrainrotSOIDs>("Brainrot SO ID Assigner");
    }

    // gan id tang dan tu 0, sap xep theo rarity roi theo ten file
    private void AssignIDs() {
        if (soFolder == null) {
            Debug.LogError("SO folder is missing");
            return;
        }

        List<BrainrotSO> brainrotSOs = GetBrainrotSOs(AssetDatabase.GetAssetPath(soFolder));
        brainrotSOs.Sort(CompareBrainrotSO);

        for (int i = 0; i < brainrotSOs.Count; i++) {
            SerializedObject serializedObject = new SerializedObject(brainrotSOs[i]);
            serializedObject.FindProperty("id").intValue = i;
            serializedObject.ApplyModifiedPropertiesWithoutUndo();

            EditorUtility.SetDirty(brainrotSOs[i]);
            Debug.Log(i + " - " + brainrotSOs[i].name + " (" + brainrotSOs[i].GetRarity() + ")");
        }

        AssetDatabase.SaveAssets();
        Debug.Log("Assigned ID for " + brainrotSOs.Count + " BrainrotSOs");
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

    private int CompareBrainrotSO(BrainrotSO a, BrainrotSO b) {
        int rarityCompare = ((int)a.GetRarity()).CompareTo((int)b.GetRarity());
        if (rarityCompare != 0) return rarityCompare;

        return string.CompareOrdinal(a.name, b.name);
    }
}