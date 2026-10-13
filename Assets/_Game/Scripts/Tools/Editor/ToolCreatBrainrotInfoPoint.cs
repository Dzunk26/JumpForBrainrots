using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class ToolCreatBrainrotInfoPoint : EditorWindow {
    private static string DEFAULT_SO_FOLDER = "Assets/_Game/ScriptableObjects/BrainrotSOs";
    private static string INFO_POINT_NAME = "InfoPoint";
    private static string INFO_POINT_PROPERTY = "infoPoint";

    [SerializeField] private DefaultAsset soFolder;
    [SerializeField] private float padding = 0.3f;
    [SerializeField] private bool isOverwrite = false;

    private void OnEnable() {
        if (soFolder == null) {
            soFolder = AssetDatabase.LoadAssetAtPath<DefaultAsset>(DEFAULT_SO_FOLDER);
        }
    }

    private void OnGUI() {
        soFolder = (DefaultAsset)EditorGUILayout.ObjectField("SO Folder", soFolder, typeof(DefaultAsset), false);
        padding = EditorGUILayout.FloatField("Padding", padding);
        isOverwrite = EditorGUILayout.Toggle("Overwrite Existing", isOverwrite);

        EditorGUILayout.HelpBox("Tat Overwrite de giu lai cac infoPoint da chinh tay", MessageType.Info);
        EditorGUILayout.Space();

        if (GUILayout.Button("Create Info Points", GUILayout.Height(30))) {
            CreateInfoPoints();
            GUIUtility.ExitGUI();
        }
    }

    [MenuItem("Tools/Brainrot/Brainrot Info Point Creator")]
    private static void OpenWindow() {
        GetWindow<ToolCreatBrainrotInfoPoint>("Brainrot Info Point Creator");
    }

    private void CreateInfoPoints() {
        if (soFolder == null) {
            Debug.LogError("SO folder is missing");
            return;
        }

        List<string> modelPaths = GetModelPaths(AssetDatabase.GetAssetPath(soFolder));
        int createdCount = 0;

        try {
            for (int i = 0; i < modelPaths.Count; i++) {
                EditorUtility.DisplayProgressBar("Creating Info Points", modelPaths[i], (float)i / modelPaths.Count);

                if (CreateInfoPoint(modelPaths[i])) {
                    createdCount++;
                }
            }
        }
        finally {
            EditorUtility.ClearProgressBar();
        }

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Debug.Log("Created info point for " + createdCount + " / " + modelPaths.Count + " brainrot models");
    }

    private bool CreateInfoPoint(string modelPath) {
        GameObject contents = PrefabUtility.LoadPrefabContents(modelPath);
        try {
            BrainrotModel model = contents.GetComponent<BrainrotModel>();
            if (model == null) {
                Debug.LogWarning("BrainrotModel is missing on root: " + modelPath);
                return false;
            }

            SerializedObject serializedObject = new SerializedObject(model);
            SerializedProperty infoPointProperty = serializedObject.FindProperty(INFO_POINT_PROPERTY);
            Transform infoPoint = (Transform)infoPointProperty.objectReferenceValue;
            if (infoPoint != null && !isOverwrite) return false;

            Transform rootTF = contents.transform;
            if (infoPoint == null) {
                infoPoint = GetOrCreateInfoPoint(rootTF);
            }

            // bounds cua analyzer tinh theo kich thuoc that, gan world position de unity tu bu scale 100 cua root
            Bounds bounds = BrainrotVFXAnalyzer.Analyze(contents).bounds;
            infoPoint.position = rootTF.position + rootTF.rotation * (Vector3.up * (bounds.max.y + padding));

            infoPointProperty.objectReferenceValue = infoPoint;
            serializedObject.ApplyModifiedPropertiesWithoutUndo();
            PrefabUtility.SaveAsPrefabAsset(contents, modelPath);
            return true;
        }
        finally {
            PrefabUtility.UnloadPrefabContents(contents);
        }
    }

    // dung lai object InfoPoint cu neu co de khong tao trung
    private Transform GetOrCreateInfoPoint(Transform rootTF) {
        Transform infoPoint = rootTF.Find(INFO_POINT_NAME);
        if (infoPoint != null) return infoPoint;

        infoPoint = new GameObject(INFO_POINT_NAME).transform;
        infoPoint.SetParent(rootTF, false);
        return infoPoint;
    }

    // nhieu SO co the dung chung 1 model nen loc trung theo duong dan prefab
    private List<string> GetModelPaths(string soPath) {
        List<string> modelPaths = new List<string>();
        string[] soGuids = AssetDatabase.FindAssets("t:BrainrotSO", new string[] { soPath });

        foreach (string soGuid in soGuids) {
            BrainrotSO brainrotSO = AssetDatabase.LoadAssetAtPath<BrainrotSO>(AssetDatabase.GUIDToAssetPath(soGuid));
            if (brainrotSO == null) continue;

            BrainrotModel model = brainrotSO.GetBrainrotModel();
            if (model == null) {
                Debug.LogWarning("Brainrot model is missing: " + brainrotSO.name);
                continue;
            }

            string modelPath = AssetDatabase.GetAssetPath(model);
            if (modelPaths.Contains(modelPath)) continue;

            modelPaths.Add(modelPath);
        }

        return modelPaths;
    }
}