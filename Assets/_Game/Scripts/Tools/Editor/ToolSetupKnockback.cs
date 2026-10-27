using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.Animations;
using UnityEditor.SceneManagement;
using UnityEngine;

// setup anim bi danh van kieu roblox: clip khua tay chan luc bay (muscle humanoid), import Getting Up thanh humanoid,
// them 3 state Knockback / Lying / GettingUp vao Base Layer. chay lai nhieu lan van an toan
public static class ToolSetupKnockback {
    private static string ANIM_FOLDER = "Assets/_Game/Animations/Character";
    private static string CONTROLLER_PATH = ANIM_FOLDER + "/Character Controller.controller";
    private static string IDLE_FBX_PATH = ANIM_FOLDER + "/Breathing Idle.fbx";
    private static string GETTING_UP_FBX_PATH = ANIM_FOLDER + "/Getting Up.fbx";
    private static string FLAIL_CLIP_PATH = ANIM_FOLDER + "/Knockback Flail.anim";
    private static string LYING_CLIP_PATH = ANIM_FOLDER + "/Knockback Lying.anim";
    private static string SKIN_FOLDER = "Assets/_Game/Prefabs/Skins";

    private static string KNOCKBACK_STATE_NAME = "Knockback";
    private static string LYING_STATE_NAME = "Lying";
    private static string GETTING_UP_STATE_NAME = "GettingUp";

    private static float KNOCKBACK_TRANSITION_DURATION = 0.1f;
    // khi cham dat: tu tu the khua tay chan tha long tay chan ra (CharacterVisual.lieDownDuration xoay nguoi nam phang cung luc)
    private static float LYING_TRANSITION_DURATION = 0.2f;
    // phai bang CharacterVisual.getUpTurnDuration: root cua Getting Up nghieng dan tu nam sang dung trong khi
    // skinRoot xoay nguoc lai tu nam phang ve thang, hai cai bu nhau nen nguoi van nam sat dat luc blend
    private static float GETTING_UP_TRANSITION_DURATION = 0.25f;

    // tay chan dang rong va vung qua lai, than uon nguoc ra sau (front-back duong la ngua ra sau)
    private static float[] FLAIL_TIMES = { 0f, 0.2f, 0.4f };
    private static string[] FLAIL_MUSCLES = {
        "Spine Front-Back", "Chest Front-Back",
        "Left Arm Down-Up", "Left Arm Front-Back", "Left Forearm Stretch",
        "Right Arm Down-Up", "Right Arm Front-Back", "Right Forearm Stretch",
        "Left Upper Leg Front-Back", "Left Upper Leg In-Out", "Left Lower Leg Stretch",
        "Right Upper Leg Front-Back", "Right Upper Leg In-Out", "Right Lower Leg Stretch",
    };
    private static float[] FLAIL_POSE_A = { 0.3f, 0.2f, 0.9f, -0.2f, 0.6f, 0.6f, 0.3f, 0.3f, 0.4f, 0.4f, 0.2f, -0.3f, 0.3f, 0.6f };
    private static float[] FLAIL_POSE_B = { 0.4f, 0.3f, 0.6f, 0.3f, 0.3f, 0.9f, -0.2f, 0.6f, -0.3f, 0.3f, 0.6f, 0.4f, 0.4f, 0.2f };

    // tu the nam lay tu frame dau Breathing Idle (tay xuoi, chan duoi thang) roi cong them do lech cho tha long, khong deu nhau
    // CharacterVisual xoay ca nguoi nam ngua xuong dat nen tay chan nam sat dat, khong chong len troi
    private static string[] LYING_OFFSET_MUSCLES = {
        "Head Turn Left-Right", "Spine Twist Left-Right",
        "Left Arm Down-Up", "Left Forearm Stretch",
        "Right Arm Down-Up", "Right Forearm Stretch",
        "Left Lower Leg Stretch",
    };
    private static float[] LYING_OFFSETS = { 0.35f, 0.05f, 0.2f, -0.15f, 0.3f, -0.3f, -0.15f };
    // ten curve ngon tay khac ten muscle, tay roblox la khoi vuong nen bo qua ngon tay
    private static string[] FINGER_KEYWORDS = { "Thumb", "Index", "Middle", "Ring", "Little" };

    [MenuItem("Tools/Knockback/Setup Knockback Anim")]
    private static void SetupKnockbackAnim() {
        AnimationClip gettingUpClip = SetupGettingUpImport();
        if (gettingUpClip == null) {
            Debug.LogError("[Knockback] Khong tim thay clip trong " + GETTING_UP_FBX_PATH);
            return;
        }

        AnimationClip flailClip = CreateMuscleClip(FLAIL_CLIP_PATH, FLAIL_MUSCLES, FLAIL_TIMES, new float[][] { FLAIL_POSE_A, FLAIL_POSE_B, FLAIL_POSE_A });
        AnimationClip lyingClip = CreateLyingClip();
        if (lyingClip == null) {
            Debug.LogError("[Knockback] Khong tim thay skin prefab co SkinModel trong " + SKIN_FOLDER);
            return;
        }
        AnimatorController controller = AssetDatabase.LoadAssetAtPath<AnimatorController>(CONTROLLER_PATH);
        SetupStates(controller, flailClip, lyingClip, gettingUpClip);

        AssetDatabase.SaveAssets();
        Debug.Log("[Knockback] Setup done");
    }

    // Getting Up tai tu mixamo dang Generic: doi sang Humanoid dung chung avatar voi Breathing Idle
    // bake het root vao pose (khong dung root motion) de nhan vat nam / dung dung cho, khong troi
    private static AnimationClip SetupGettingUpImport() {
        ModelImporter idleImporter = (ModelImporter)AssetImporter.GetAtPath(IDLE_FBX_PATH);
        ModelImporter importer = (ModelImporter)AssetImporter.GetAtPath(GETTING_UP_FBX_PATH);

        importer.animationType = ModelImporterAnimationType.Human;
        importer.avatarSetup = ModelImporterAvatarSetup.CopyFromOther;
        importer.sourceAvatar = idleImporter.sourceAvatar;
        importer.importAnimation = true;
        importer.SaveAndReimport();

        ModelImporterClipAnimation[] clips = importer.defaultClipAnimations;
        foreach (ModelImporterClipAnimation clip in clips) {
            clip.name = "Getting Up";
            clip.loopTime = false;
            clip.lockRootRotation = true;
            clip.keepOriginalOrientation = true;
            clip.lockRootHeightY = true;
            clip.keepOriginalPositionY = true;
            clip.heightFromFeet = false;
            clip.lockRootPositionXZ = true;
            clip.keepOriginalPositionXZ = true;
        }
        importer.clipAnimations = clips;
        importer.SaveAndReimport();

        return LoadClip(GETTING_UP_FBX_PATH);
    }

    private static AnimationClip CreateMuscleClip(string path, string[] muscleNames, float[] times, float[][] values) {
        AnimationClip clip = BatAttackAnimBuilder.GetOrCreateClip(path);
        clip.ClearCurves();
        BatAttackAnimBuilder.SetMuscleCurves(clip, muscleNames, times, values);
        BatAttackAnimBuilder.SetClipSettings(clip, true, new AnimationEvent[0]);
        return clip;
    }

    private static AnimationClip CreateLyingClip() {
        float[] idleMuscles = SampleIdleMuscles();
        if (idleMuscles == null) return null;

        List<string> muscleNames = new List<string>();
        List<float> values = new List<float>();
        for (int i = 0; i < HumanTrait.MuscleCount; i++) {
            string muscleName = HumanTrait.MuscleName[i];
            if (FINGER_KEYWORDS.Any(keyword => muscleName.Contains(keyword))) continue;

            int offsetIndex = System.Array.IndexOf(LYING_OFFSET_MUSCLES, muscleName);
            muscleNames.Add(muscleName);
            values.Add(idleMuscles[i] + (offsetIndex >= 0 ? LYING_OFFSETS[offsetIndex] : 0f));
        }

        float[] pose = values.ToArray();
        return CreateMuscleClip(LYING_CLIP_PATH, muscleNames.ToArray(), new float[] { 0f, 1f }, new float[][] { pose, pose });
    }

    // lay muscle cua frame dau Breathing Idle tren 1 skin, lam trong preview scene de khong dong vao scene dang mo
    private static float[] SampleIdleMuscles() {
        GameObject skinAsset = AssetDatabase.FindAssets("t:Prefab", new[] { SKIN_FOLDER })
            .Select(guid => AssetDatabase.LoadAssetAtPath<GameObject>(AssetDatabase.GUIDToAssetPath(guid)))
            .FirstOrDefault(asset => asset.GetComponent<SkinModel>() != null);
        if (skinAsset == null) return null;

        UnityEngine.SceneManagement.Scene previewScene = EditorSceneManager.NewPreviewScene();
        GameObject instance = (GameObject)PrefabUtility.InstantiatePrefab(skinAsset, previewScene);
        Animator animator = instance.GetComponentInChildren<Animator>();
        LoadClip(IDLE_FBX_PATH).SampleAnimation(animator.gameObject, 0f);

        HumanPoseHandler poseHandler = new HumanPoseHandler(animator.avatar, animator.transform);
        HumanPose pose = new HumanPose();
        poseHandler.GetHumanPose(ref pose);
        poseHandler.Dispose();
        EditorSceneManager.ClosePreviewScene(previewScene);
        return pose.muscles;
    }

    private static void SetupStates(AnimatorController controller, AnimationClip flailClip, AnimationClip lyingClip, AnimationClip gettingUpClip) {
        AnimatorStateMachine stateMachine = controller.layers[0].stateMachine;
        string[] stateNames = { KNOCKBACK_STATE_NAME, LYING_STATE_NAME, GETTING_UP_STATE_NAME };

        foreach (ChildAnimatorState child in stateMachine.states) {
            if (System.Array.IndexOf(stateNames, child.state.name) >= 0) {
                stateMachine.RemoveState(child.state);
            }
        }

        AddTriggerState(controller, stateMachine, KNOCKBACK_STATE_NAME, flailClip, 1f, KNOCKBACK_TRANSITION_DURATION, new Vector3(700f, 10f, 0f));
        AddTriggerState(controller, stateMachine, LYING_STATE_NAME, lyingClip, 1f, LYING_TRANSITION_DURATION, new Vector3(700f, 80f, 0f));
        AddTriggerState(controller, stateMachine, GETTING_UP_STATE_NAME, gettingUpClip, 1f, GETTING_UP_TRANSITION_DURATION, new Vector3(700f, 150f, 0f));
        EditorUtility.SetDirty(controller);
    }

    // giong cac state co san: trigger cung ten state, chuyen tu Any State
    private static void AddTriggerState(AnimatorController controller, AnimatorStateMachine stateMachine, string stateName, AnimationClip clip, float speed, float duration, Vector3 position) {
        if (!controller.parameters.Any(parameter => parameter.name == stateName)) {
            controller.AddParameter(stateName, AnimatorControllerParameterType.Trigger);
        }

        AnimatorState state = stateMachine.AddState(stateName, position);
        state.motion = clip;
        state.speed = speed;

        AnimatorStateTransition transition = stateMachine.AddAnyStateTransition(state);
        transition.AddCondition(AnimatorConditionMode.If, 0f, stateName);
        transition.hasExitTime = false;
        transition.hasFixedDuration = true;
        transition.duration = duration;
        transition.canTransitionToSelf = false;
    }

    private static AnimationClip LoadClip(string path) {
        foreach (Object asset in AssetDatabase.LoadAllAssetsAtPath(path)) {
            if (asset is AnimationClip clip && !clip.name.StartsWith("__preview__")) return clip;
        }

        return null;
    }
}
