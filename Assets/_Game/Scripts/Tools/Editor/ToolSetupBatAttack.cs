using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEditor;
using UnityEditor.Animations;
using UnityEditor.SceneManagement;
using UnityEngine;

// setup toan bo tinh nang cam gay danh nguoi: anim, layer Attack, prefab gay, diem cam gay tung skin,
// gan reference cho Player / Bot va tao nut Attack trong Canvas-Gameplay. chay lai nhieu lan van an toan
public static class ToolSetupBatAttack {
    private static string ANIM_FOLDER = "Assets/_Game/Animations/Character";
    private static string MASK_PATH = ANIM_FOLDER + "/RightArmMask.mask";
    private static string HOLD_CLIP_PATH = ANIM_FOLDER + "/Bat Hold.anim";
    private static string ATTACK_CLIP_PATH = ANIM_FOLDER + "/Bat Attack.anim";
    private static string BODY_MASK_PATH = ANIM_FOLDER + "/UpperBodyMask.mask";
    private static string ATTACK_BODY_CLIP_PATH = ANIM_FOLDER + "/Bat Attack Body.anim";
    private static string CONTROLLER_PATH = ANIM_FOLDER + "/Character Controller.controller";
    private static string IDLE_FBX_PATH = ANIM_FOLDER + "/Breathing Idle.fbx";
    private static string BAT_FBX_PATH = "Assets/_Game/Models/Bat/bat.fbx";
    private static string BAT_PREFAB_FOLDER = "Assets/_Game/Prefabs/Bat";
    private static string BAT_PREFAB_PATH = BAT_PREFAB_FOLDER + "/Bat.prefab";
    private static string SKIN_FOLDER = "Assets/_Game/Prefabs/Skins";
    private static string BOT_PREFAB_PATH = "Assets/_Game/Prefabs/Bot/Bot.prefab";
    private static string GAME_SCENE_PATH = "Assets/_Game/Scenes/GameScene.unity";
    private static string CANVAS_GAMEPLAY_PATH = "Assets/_Game/Resources/UI/Canvas-Gameplay.prefab";
    private static string TOOL_SO_FOLDER = "Assets/_Game/ScriptableObjects/ToolSOs";
    private static string BAT_TOOL_SO_PATH = TOOL_SO_FOLDER + "/Bat Tool SO.asset";
    private static string BAT_HOLD_POINT_NAME = "BatHoldPoint";
    private static string ATTACK_BUTTON_NAME = "Attack";
    private static float BAT_LENGTH = 3f;
    private static float ATTACK_COOLDOWN_PADDING = 0.05f;
    private static Vector2 ATTACK_BUTTON_OFFSET = new Vector2(0f, 190f);

    [MenuItem("Tools/Bat Attack/Setup Bat Attack")]
    private static void SetupBatAttack() {
        AvatarMask armMask = BatAttackAnimBuilder.CreateMask(MASK_PATH, AvatarMaskBodyPart.RightArm);
        AvatarMask bodyMask = BatAttackAnimBuilder.CreateMask(BODY_MASK_PATH, AvatarMaskBodyPart.Body, AvatarMaskBodyPart.LeftArm);
        AnimationClip holdClip = BatAttackAnimBuilder.CreateHoldClip(HOLD_CLIP_PATH);
        AnimationClip attackClip = BatAttackAnimBuilder.CreateAttackClip(ATTACK_CLIP_PATH);
        AnimationClip attackBodyClip = BatAttackAnimBuilder.CreateAttackBodyClip(ATTACK_BODY_CLIP_PATH);
        AnimatorController controller = AssetDatabase.LoadAssetAtPath<AnimatorController>(CONTROLLER_PATH);
        BatAttackAnimBuilder.SetupAttackLayers(controller, armMask, bodyMask, holdClip, attackClip, attackBodyClip);
        AssetDatabase.SaveAssets();

        Transform batPrefab = CreateBatPrefab();
        AnimationClip idleClip = LoadClip(IDLE_FBX_PATH);

        foreach (string guid in AssetDatabase.FindAssets("t:Prefab", new[] { SKIN_FOLDER })) {
            SetupSkin(AssetDatabase.GUIDToAssetPath(guid), idleClip, holdClip, false);
        }

        SetupBotPrefab(character => SetupCharacterVisual(character, batPrefab));
        SetupScenePlayer(character => SetupCharacterVisual(character, batPrefab));
        SetupAttackButton();

        AssetDatabase.SaveAssets();
        Debug.Log("[BatAttack] Setup done");
    }

    // tao item gay (ToolSO, id -2) va gan vao InventoryManager trong GameScene. icon tu gan sau trong asset
    [MenuItem("Tools/Bat Attack/Setup Bat Item")]
    private static void SetupBatItem() {
        if (!AssetDatabase.IsValidFolder(TOOL_SO_FOLDER)) {
            AssetDatabase.CreateFolder("Assets/_Game/ScriptableObjects", "ToolSOs");
        }

        ToolSO batToolSO = AssetDatabase.LoadAssetAtPath<ToolSO>(BAT_TOOL_SO_PATH);
        if (batToolSO == null) {
            batToolSO = ScriptableObject.CreateInstance<ToolSO>();
            AssetDatabase.CreateAsset(batToolSO, BAT_TOOL_SO_PATH);
            SerializedObject serializedTool = new SerializedObject(batToolSO);
            serializedTool.FindProperty("toolName").stringValue = "Bat";
            serializedTool.ApplyModifiedPropertiesWithoutUndo();
        }

        SetupSceneComponents<InventoryManager>(inventoryManager => {
            SerializedObject serializedInventory = new SerializedObject(inventoryManager);
            serializedInventory.FindProperty("batToolSO").objectReferenceValue = batToolSO;
            serializedInventory.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(inventoryManager);
        });

        AssetDatabase.SaveAssets();
        Selection.activeObject = batToolSO;
        Debug.Log("[BatAttack] Setup bat item done, nho gan icon cho " + BAT_TOOL_SO_PATH);
    }

    // build lai clip cam gay + 2 clip vung gay (ghi de vao asset cu, giu guid), cap nhat cooldown theo do dai clip
    // va xoay lai BatHoldPoint cho gay dung thang o pose cam moi. giu nguyen vi tri BatHoldPoint / Bat.prefab da chinh tay
    [MenuItem("Tools/Bat Attack/Rebuild Anim (Keep Bat Position)")]
    private static void RebuildAnim() {
        AnimationClip holdClip = BatAttackAnimBuilder.CreateHoldClip(HOLD_CLIP_PATH);
        BatAttackAnimBuilder.CreateAttackClip(ATTACK_CLIP_PATH);
        BatAttackAnimBuilder.CreateAttackBodyClip(ATTACK_BODY_CLIP_PATH);
        AssetDatabase.SaveAssets();

        AnimationClip idleClip = LoadClip(IDLE_FBX_PATH);
        foreach (string guid in AssetDatabase.FindAssets("t:Prefab", new[] { SKIN_FOLDER })) {
            SetupSkin(AssetDatabase.GUIDToAssetPath(guid), idleClip, holdClip, true);
        }

        SetupBotPrefab(SetAttackCooldown);
        SetupScenePlayer(SetAttackCooldown);
        AssetDatabase.SaveAssets();
        Debug.Log("[BatAttack] Rebuild anim done");
    }

    private static Transform CreateBatPrefab() {
        if (!AssetDatabase.IsValidFolder(BAT_PREFAB_FOLDER)) {
            AssetDatabase.CreateFolder("Assets/_Game/Prefabs", "Bat");
        }

        GameObject root = new GameObject("Bat");
        GameObject model = (GameObject)PrefabUtility.InstantiatePrefab(AssetDatabase.LoadAssetAtPath<GameObject>(BAT_FBX_PATH));
        BatAttackAnimBuilder.FitBatModel(root.transform, model.transform, BAT_LENGTH);

        GameObject prefab = PrefabUtility.SaveAsPrefabAsset(root, BAT_PREFAB_PATH);
        Object.DestroyImmediate(root);
        return prefab.transform;
    }

    // isKeepPosition: chi xoay lai BatHoldPoint, giu vi tri da chinh tay
    private static void SetupSkin(string path, AnimationClip idleClip, AnimationClip holdClip, bool isKeepPosition) {
        GameObject asset = AssetDatabase.LoadAssetAtPath<GameObject>(path);
        if (asset.GetComponent<SkinModel>() == null) return;

        // lay pose cam gay tren ban sao trong preview scene de khong lam ban prefab / scene dang mo
        UnityEngine.SceneManagement.Scene previewScene = EditorSceneManager.NewPreviewScene();
        GameObject instance = (GameObject)PrefabUtility.InstantiatePrefab(asset, previewScene);
        Animator instanceAnimator = instance.GetComponentInChildren<Animator>();
        BatAttackAnimBuilder.SamplePose(instanceAnimator, idleClip, 0f, 0f, holdClip);
        BatAttackAnimBuilder.GetBatHoldPointPose(instanceAnimator, out Vector3 localPosition, out Quaternion localRotation, out Vector3 localScale);
        string handPath = AnimationUtility.CalculateTransformPath(instanceAnimator.GetBoneTransform(HumanBodyBones.RightHand), instance.transform);
        EditorSceneManager.ClosePreviewScene(previewScene);

        GameObject contents = PrefabUtility.LoadPrefabContents(path);
        Transform hand = contents.transform.Find(handPath);
        Transform holdPoint = hand.Find(BAT_HOLD_POINT_NAME);
        if (holdPoint == null) {
            holdPoint = new GameObject(BAT_HOLD_POINT_NAME).transform;
            holdPoint.SetParent(hand, false);
        }
        if (!isKeepPosition) {
            holdPoint.localPosition = localPosition;
        }
        holdPoint.localRotation = localRotation;
        holdPoint.localScale = localScale;

        Animator animator = contents.GetComponentInChildren<Animator>();
        SkinAnimEvent skinAnimEvent = animator.GetComponent<SkinAnimEvent>();
        if (skinAnimEvent == null) {
            skinAnimEvent = animator.gameObject.AddComponent<SkinAnimEvent>();
        }

        SerializedObject serializedSkin = new SerializedObject(contents.GetComponent<SkinModel>());
        serializedSkin.FindProperty("skinAnimEvent").objectReferenceValue = skinAnimEvent;
        serializedSkin.FindProperty("batHoldPoint").objectReferenceValue = holdPoint;
        serializedSkin.ApplyModifiedPropertiesWithoutUndo();

        PrefabUtility.SaveAsPrefabAsset(contents, path);
        PrefabUtility.UnloadPrefabContents(contents);
        Debug.Log("[BatAttack] Skin " + path + " hand " + handPath);
    }

    private static void SetupBotPrefab(System.Action<Character> setup) {
        GameObject contents = PrefabUtility.LoadPrefabContents(BOT_PREFAB_PATH);
        setup(contents.GetComponent<Character>());
        PrefabUtility.SaveAsPrefabAsset(contents, BOT_PREFAB_PATH);
        PrefabUtility.UnloadPrefabContents(contents);
    }

    // scene dang mo thi chi danh dau dirty de nguoi dung tu Ctrl+S, chua mo thi mo them, luu roi dong
    private static void SetupScenePlayer(System.Action<Character> setup) {
        SetupSceneComponents<Player>(setup);
    }

    private static void SetupSceneComponents<T>(System.Action<T> setup) where T : Component {
        UnityEngine.SceneManagement.Scene scene = UnityEngine.SceneManagement.SceneManager.GetSceneByPath(GAME_SCENE_PATH);
        bool isOpenedByTool = !scene.isLoaded;
        if (isOpenedByTool) {
            scene = EditorSceneManager.OpenScene(GAME_SCENE_PATH, OpenSceneMode.Additive);
        }

        foreach (GameObject root in scene.GetRootGameObjects()) {
            foreach (T component in root.GetComponentsInChildren<T>(true)) {
                setup(component);
            }
        }

        EditorSceneManager.MarkSceneDirty(scene);
        if (isOpenedByTool) {
            EditorSceneManager.SaveScene(scene);
            EditorSceneManager.CloseScene(scene, true);
        }
        else {
            Debug.LogWarning("[BatAttack] GameScene dang mo: nho Ctrl+S de luu thay doi cua " + typeof(T).Name);
        }
    }

    private static void SetupCharacterVisual(Character character, Transform batPrefab) {
        SetAttackCooldown(character);
        CharacterVisual characterVisual = (CharacterVisual)new SerializedObject(character).FindProperty("characterVisual").objectReferenceValue;

        SerializedObject serializedVisual = new SerializedObject(characterVisual);
        serializedVisual.FindProperty("owner").objectReferenceValue = character;
        serializedVisual.FindProperty("batPrefab").objectReferenceValue = batPrefab;
        serializedVisual.ApplyModifiedPropertiesWithoutUndo();
        EditorUtility.SetDirty(characterVisual);
    }

    // cooldown khong ngan hon clip vung gay de khong cat ngang anim
    private static void SetAttackCooldown(Character character) {
        SerializedObject serializedCharacter = new SerializedObject(character);
        serializedCharacter.FindProperty("attackTimerMax").floatValue = BatAttackAnimBuilder.ATTACK_DURATION + ATTACK_COOLDOWN_PADDING;
        serializedCharacter.ApplyModifiedPropertiesWithoutUndo();
        EditorUtility.SetDirty(character);
    }

    // nhan ban nut Jump thanh nut Attack dat phia tren
    private static void SetupAttackButton() {
        GameObject contents = PrefabUtility.LoadPrefabContents(CANVAS_GAMEPLAY_PATH);
        TestJumpButton jumpButton = contents.GetComponentInChildren<TestJumpButton>(true);
        Transform parent = jumpButton.transform.parent;

        Transform oldButton = parent.Find(ATTACK_BUTTON_NAME);
        if (oldButton != null) {
            Object.DestroyImmediate(oldButton.gameObject);
        }

        GameObject attackGO = Object.Instantiate(jumpButton.gameObject, parent);
        attackGO.name = ATTACK_BUTTON_NAME;
        Object.DestroyImmediate(attackGO.GetComponent<TestJumpButton>());

        RectTransform attackRect = attackGO.GetComponent<RectTransform>();
        attackRect.anchoredPosition = jumpButton.GetComponent<RectTransform>().anchoredPosition + ATTACK_BUTTON_OFFSET;

        foreach (TMP_Text text in attackGO.GetComponentsInChildren<TMP_Text>(true)) {
            text.text = "Hit";
        }

        AttackButtonUI attackButtonUI = attackGO.AddComponent<AttackButtonUI>();
        SerializedObject serializedButton = new SerializedObject(attackButtonUI);
        serializedButton.FindProperty("attackButton").objectReferenceValue = attackGO.GetComponentInChildren<TourchDownButton>(true);
        serializedButton.ApplyModifiedPropertiesWithoutUndo();

        SerializedObject serializedCanvas = new SerializedObject(contents.GetComponentInChildren<CanvasGamePlay>(true));
        serializedCanvas.FindProperty("attackButtonUI").objectReferenceValue = attackButtonUI;
        serializedCanvas.ApplyModifiedPropertiesWithoutUndo();

        PrefabUtility.SaveAsPrefabAsset(contents, CANVAS_GAMEPLAY_PATH);
        PrefabUtility.UnloadPrefabContents(contents);
    }

    private static AnimationClip LoadClip(string path) {
        foreach (Object asset in AssetDatabase.LoadAllAssetsAtPath(path)) {
            if (asset is AnimationClip clip && !clip.name.StartsWith("__preview__")) return clip;
        }

        return null;
    }
}
