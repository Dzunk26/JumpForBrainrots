using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class BrainrotDataEntry {
    public string brainrotName;
    public string modelName;
    public BrainrotRarity rarity;

    public BrainrotDataEntry(string brainrotName, string modelName, BrainrotRarity rarity) {
        this.brainrotName = brainrotName;
        this.modelName = modelName;
        this.rarity = rarity;
    }
}

public class ToolMakeBrainrotSOs : EditorWindow {
    private static string DEFAULT_PREFABS_FOLDER = "Assets/_Game/Prefabs/BrainrotModels";
    private static string DEFAULT_SO_FOLDER = "Assets/_Game/ScriptableObjects/BrainrotSOs";

    // danh sach lay tu file Jump_for_Brainrots_Index.xlsx
    // modelName = ten prefab trong folder BrainrotModels (mot so ten khac voi file excel)
    private static BrainrotDataEntry[] BRAINROT_DATA_ENTRIES = new BrainrotDataEntry[] {
        new BrainrotDataEntry("Brr Brr Patapim", "Brr Brr Patapim", BrainrotRarity.Basic),
        new BrainrotDataEntry("Tung Tung Tung Sahur", "Tung Sahur", BrainrotRarity.Basic),
        new BrainrotDataEntry("Boneca Ambalabu", "Boneca Ambalabu", BrainrotRarity.Basic),
        new BrainrotDataEntry("Lirilli Larila", "Lirili Larila", BrainrotRarity.Basic),
        new BrainrotDataEntry("Tralalero Tralala", "Tralalero Tralala", BrainrotRarity.Basic),
        new BrainrotDataEntry("Cappuccino Assassino", "Cappuccino Assassino", BrainrotRarity.Basic),
        new BrainrotDataEntry("Bombombini Gusini", "Bombombini Gusini", BrainrotRarity.Rare),
        new BrainrotDataEntry("Trippi Troppi", "Trippi Troppi", BrainrotRarity.Rare),
        new BrainrotDataEntry("Svinina Bombardino", "Svinina Bombardino", BrainrotRarity.Rare),
        new BrainrotDataEntry("Bombardiro Crocodilo", "Bombardiro Crocodilo", BrainrotRarity.Rare),
        new BrainrotDataEntry("Cocofanto Elefanto", "Cocofanto Elefanto", BrainrotRarity.Rare),
        new BrainrotDataEntry("Frigo Camelo", "Frigo Camelo", BrainrotRarity.Epic),
        new BrainrotDataEntry("Orcalero Orcala", "Orcalero Orcala", BrainrotRarity.Epic),
        new BrainrotDataEntry("Espresso Signora", "Espresso Signora", BrainrotRarity.Epic),
        new BrainrotDataEntry("Burbaloni Loliloli", "Burbaloni Luliloli", BrainrotRarity.Epic),
        new BrainrotDataEntry("Gangster Footera", "Gangster Footera", BrainrotRarity.Epic),
        new BrainrotDataEntry("Tim Cheese", "Tim Cheese", BrainrotRarity.Epic),
        new BrainrotDataEntry("Chimpanzini Bananini", "Chimpanzini Bananini", BrainrotRarity.Legendary),
        new BrainrotDataEntry("Ballerina Cappuccina", "Ballerina Cappuccina", BrainrotRarity.Legendary),
        new BrainrotDataEntry("Girafa Celestre", "Girafa Celeste", BrainrotRarity.Legendary),
        new BrainrotDataEntry("Brri Brri Bicus Dicus Bombicus", "Brri Brri Bicus Dicus", BrainrotRarity.Legendary),
        new BrainrotDataEntry("Mateo", "Matteo", BrainrotRarity.Mythic),
        new BrainrotDataEntry("Odin Din Din Dun", "Odin Din Din Dun", BrainrotRarity.Mythic),
        new BrainrotDataEntry("Gorillo Watermelondrillo", "Gorillo Watermelondrillo", BrainrotRarity.Mythic),
        new BrainrotDataEntry("Torrtuginni Dragonfrutini", "Torrtuginni Dragonfrutini", BrainrotRarity.Mythic),
        new BrainrotDataEntry("Blueberrinni Octopusini", "Blueberrinni Octopusini", BrainrotRarity.Mythic),
        new BrainrotDataEntry("Swag Soda", "Swag Soda", BrainrotRarity.Secret),
        new BrainrotDataEntry("Chef Crabracadabra", "Chef Crabracadabra", BrainrotRarity.Secret),
        new BrainrotDataEntry("Pot Hotspot", "Pot Hotspot", BrainrotRarity.Secret),
        new BrainrotDataEntry("Tralalerito", "Los Tralaleritos", BrainrotRarity.Secret),
    };

    [SerializeField] private DefaultAsset prefabsFolder;
    [SerializeField] private DefaultAsset soFolder;
    [SerializeField] private bool isOverwrite = false;

    private void OnEnable() {
        if (prefabsFolder == null) {
            prefabsFolder = AssetDatabase.LoadAssetAtPath<DefaultAsset>(DEFAULT_PREFABS_FOLDER);
        }

        if (soFolder == null) {
            soFolder = AssetDatabase.LoadAssetAtPath<DefaultAsset>(DEFAULT_SO_FOLDER);
        }
    }

    private void OnGUI() {
        prefabsFolder = (DefaultAsset)EditorGUILayout.ObjectField("Prefabs Folder", prefabsFolder, typeof(DefaultAsset), false);
        soFolder = (DefaultAsset)EditorGUILayout.ObjectField("SO Folder", soFolder, typeof(DefaultAsset), false);
        isOverwrite = EditorGUILayout.Toggle("Overwrite Existing", isOverwrite);

        EditorGUILayout.Space();

        if (GUILayout.Button("Create Brainrot SOs")) {
            CreateBrainrotSOs();
        }
    }

    [MenuItem("Tools/Brainrot/Brainrot SO Creator")]
    private static void OpenWindow() {
        GetWindow<ToolMakeBrainrotSOs>("Brainrot SO Creator");
    }

    // tao SO cho cac brainrot trong danh sach, ten SO = ten brainrot + " SO"
    private void CreateBrainrotSOs() {
        if (prefabsFolder == null || soFolder == null) {
            Debug.LogError("Prefabs folder or SO folder is missing");
            return;
        }

        string prefabsPath = AssetDatabase.GetAssetPath(prefabsFolder);
        string soPath = AssetDatabase.GetAssetPath(soFolder);

        int createdCount = 0;
        int skippedCount = 0;
        int missingCount = 0;

        try {
            for (int i = 0; i < BRAINROT_DATA_ENTRIES.Length; i++) {
                BrainrotDataEntry entry = BRAINROT_DATA_ENTRIES[i];
                string prefabPath = prefabsPath + "/" + entry.modelName + ".prefab";
                string assetPath = soPath + "/" + entry.brainrotName + " SO.asset";

                EditorUtility.DisplayProgressBar("Creating Brainrot SOs", entry.brainrotName, (float)i / BRAINROT_DATA_ENTRIES.Length);

                GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
                if (prefab == null) {
                    Debug.LogError("Missing prefab: " + prefabPath);
                    missingCount++;
                    continue;
                }

                BrainrotModel brainrotModel = prefab.GetComponent<BrainrotModel>();
                if (brainrotModel == null) {
                    Debug.LogError("Prefab has no BrainrotModel: " + prefabPath);
                    missingCount++;
                    continue;
                }

                BrainrotSO brainrotSO = AssetDatabase.LoadAssetAtPath<BrainrotSO>(assetPath);
                if (brainrotSO != null && !isOverwrite) {
                    skippedCount++;
                    continue;
                }

                // da co SO thi chi ghi de data de giu nguyen guid
                if (brainrotSO == null) {
                    brainrotSO = CreateInstance<BrainrotSO>();
                    AssetDatabase.CreateAsset(brainrotSO, assetPath);
                }

                SetBrainrotSOData(brainrotSO, entry, brainrotModel);
                createdCount++;
            }
        }
        finally {
            EditorUtility.ClearProgressBar();
        }

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Debug.Log("Created " + createdCount + " SOs, skipped " + skippedCount + " SOs, missing " + missingCount + " prefabs");
    }

    // field cua BrainrotSO la private nen gan qua SerializedObject
    private void SetBrainrotSOData(BrainrotSO brainrotSO, BrainrotDataEntry entry, BrainrotModel brainrotModel) {
        SerializedObject serializedObject = new SerializedObject(brainrotSO);
        serializedObject.FindProperty("brainrotName").stringValue = entry.brainrotName;
        serializedObject.FindProperty("rarity").enumValueIndex = (int)entry.rarity;
        serializedObject.FindProperty("brainrotModel").objectReferenceValue = brainrotModel;
        serializedObject.ApplyModifiedPropertiesWithoutUndo();

        EditorUtility.SetDirty(brainrotSO);
    }
}
