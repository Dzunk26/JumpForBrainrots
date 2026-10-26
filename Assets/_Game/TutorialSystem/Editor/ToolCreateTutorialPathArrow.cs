using UnityEditor;
using UnityEngine;

// tao material + prefab duong mui ten cau vong (dang plane) cho tutorial
// prefab da ton tai thi sua tai cho de giu nguyen tham chieu trong scene
public static class ToolCreateTutorialPathArrow {
    private static string SHADER_NAME = "_Game/VFX/Tutorial Path Arrow";
    private static string MATERIAL_PATH = "Assets/_Game/TutorialSystem/Materials/TutorialPathArrow.mat";
    private static string PREFAB_FOLDER = "Assets/_Game/TutorialSystem/Prefabs";
    private static string PREFAB_PATH = PREFAB_FOLDER + "/TutorialPathArrow.prefab";

    [MenuItem("Tools/Tutorial/Create Rainbow Path Arrow")]
    private static void Create() {
        Shader shader = Shader.Find(SHADER_NAME);
        if (shader == null) {
            Debug.LogError("Shader not found: " + SHADER_NAME);
            return;
        }

        Material material = AssetDatabase.LoadAssetAtPath<Material>(MATERIAL_PATH);
        if (material == null) {
            material = new Material(shader);
            AssetDatabase.CreateAsset(material, MATERIAL_PATH);
        }

        if (!AssetDatabase.IsValidFolder(PREFAB_FOLDER)) {
            AssetDatabase.CreateFolder("Assets/_Game/TutorialSystem", "Prefabs");
        }

        bool isExisting = AssetDatabase.LoadAssetAtPath<GameObject>(PREFAB_PATH) != null;
        GameObject root = isExisting ? PrefabUtility.LoadPrefabContents(PREFAB_PATH) : new GameObject("TutorialPathArrow");
        root.transform.rotation = Quaternion.identity;
        root.transform.localScale = Vector3.one;

        // ban cu dung LineRenderer -> bo di
        LineRenderer lineRenderer = root.GetComponent<LineRenderer>();
        if (lineRenderer != null) Object.DestroyImmediate(lineRenderer);

        MeshFilter meshFilter = GetOrAdd<MeshFilter>(root);
        MeshRenderer meshRenderer = GetOrAdd<MeshRenderer>(root);
        meshRenderer.sharedMaterial = material;
        meshRenderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
        meshRenderer.receiveShadows = false;

        TutorialPathArrow pathArrow = GetOrAdd<TutorialPathArrow>(root);
        SerializedObject serializedArrow = new SerializedObject(pathArrow);
        serializedArrow.FindProperty("meshRenderer").objectReferenceValue = meshRenderer;
        serializedArrow.FindProperty("meshFilter").objectReferenceValue = meshFilter;
        serializedArrow.ApplyModifiedPropertiesWithoutUndo();

        GameObject prefab = PrefabUtility.SaveAsPrefabAsset(root, PREFAB_PATH);
        if (isExisting) {
            PrefabUtility.UnloadPrefabContents(root);
        } else {
            Object.DestroyImmediate(root);
        }

        AssetDatabase.SaveAssets();

        Selection.activeObject = prefab;
        EditorGUIUtility.PingObject(prefab);
        Debug.Log("Created tutorial path arrow: " + PREFAB_PATH);
    }

    private static T GetOrAdd<T>(GameObject target) where T : Component {
        T component = target.GetComponent<T>();
        if (component == null) component = target.AddComponent<T>();
        return component;
    }
}
