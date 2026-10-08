using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

public class ToolCreatePrefabBrainrotModels : EditorWindow {
    private static string DEFAULT_MODELS_FOLDER = "Assets/_Game/Models/Brainrots";
    private static string DEFAULT_PREFABS_FOLDER = "Assets/_Game/Prefabs/BrainrotModels";

    [SerializeField] private DefaultAsset modelsFolder;
    [SerializeField] private DefaultAsset prefabsFolder;
    [SerializeField] private float modelScale = 1f;
    [SerializeField] private bool isOverwrite = false;

    private void OnEnable() {
        if (modelsFolder == null) {
            modelsFolder = AssetDatabase.LoadAssetAtPath<DefaultAsset>(DEFAULT_MODELS_FOLDER);
        }

        if (prefabsFolder == null) {
            prefabsFolder = AssetDatabase.LoadAssetAtPath<DefaultAsset>(DEFAULT_PREFABS_FOLDER);
        }
    }

    private void OnGUI() {
        modelsFolder = (DefaultAsset)EditorGUILayout.ObjectField("Models Folder", modelsFolder, typeof(DefaultAsset), false);
        prefabsFolder = (DefaultAsset)EditorGUILayout.ObjectField("Prefabs Folder", prefabsFolder, typeof(DefaultAsset), false);
        modelScale = EditorGUILayout.FloatField("Model Scale", modelScale);
        isOverwrite = EditorGUILayout.Toggle("Overwrite Existing", isOverwrite);

        EditorGUILayout.Space();

        if (GUILayout.Button("Create Prefabs")) {
            CreatePrefabs();
        }
    }

    [MenuItem("Tools/Brainrot/Brainrot Model Prefab Creator")]
    private static void OpenWindow() {
        GetWindow<ToolCreatePrefabBrainrotModels>("Brainrot Prefab Creator");
    }

    // tao prefab cho tung model trong folder, ten prefab = ten model
    private void CreatePrefabs() {
        if (modelsFolder == null || prefabsFolder == null) {
            Debug.LogError("Models folder or prefabs folder is missing");
            return;
        }

        string modelsPath = AssetDatabase.GetAssetPath(modelsFolder);
        string prefabsPath = AssetDatabase.GetAssetPath(prefabsFolder);
        string[] modelGuids = AssetDatabase.FindAssets("t:Model", new string[] { modelsPath });

        int createdCount = 0;
        int skippedCount = 0;

        try {
            for (int i = 0; i < modelGuids.Length; i++) {
                string modelPath = AssetDatabase.GUIDToAssetPath(modelGuids[i]);
                string modelName = Path.GetFileNameWithoutExtension(modelPath);
                string prefabPath = prefabsPath + "/" + modelName + ".prefab";

                EditorUtility.DisplayProgressBar("Creating Brainrot Prefabs", modelName, (float)i / modelGuids.Length);

                if (!isOverwrite && AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath) != null) {
                    skippedCount++;
                    continue;
                }

                GameObject modelAsset = AssetDatabase.LoadAssetAtPath<GameObject>(modelPath);
                CreatePrefab(modelAsset, modelName, prefabPath);
                createdCount++;
            }
        }
        finally {
            EditorUtility.ClearProgressBar();
        }

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Debug.Log("Created " + createdCount + " prefabs, skipped " + skippedCount + " prefabs");
    }

    // root gan BrainrotModel, model fbx la con de giu link voi file goc
    private void CreatePrefab(GameObject modelAsset, string modelName, string prefabPath) {
        GameObject root = new GameObject(modelName);
        root.AddComponent<BrainrotModel>();

        GameObject model = (GameObject)PrefabUtility.InstantiatePrefab(modelAsset, root.transform);
        model.transform.localPosition = Vector3.zero;
        model.transform.localRotation = Quaternion.identity;
        model.transform.localScale = Vector3.one * modelScale;

        PrefabUtility.SaveAsPrefabAsset(root, prefabPath);
        DestroyImmediate(root);
    }
}