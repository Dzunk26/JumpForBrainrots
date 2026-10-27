using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;

// tao avatar mask, clip cam gay / vung gay (humanoid muscle) va 2 layer trong animator controller:
// Attack (tay phai cam gay) va AttackBody (than tren + tay trai, chi chay khi vung gay)
// clip vung gay co animation event OnAttackHit dung frame gay dap xuong
// khong dong vao ngon tay vi tay roblox la khoi vuong, gap ngon se meo mesh
public static class BatAttackAnimBuilder {
    public static string HOLD_STATE_NAME = "HoldBat";
    public static string EMPTY_STATE_NAME = "Empty";
    public static string ATTACK_STATE_NAME = "Attack";
    public static string ATTACK_LAYER_NAME = "Attack";
    public static string ATTACK_BODY_LAYER_NAME = "AttackBody";
    public static string HIT_EVENT_NAME = "OnAttackHit";

    // quat cheo kieu roblox: giu tu the -> gio gay len -> lay da cao ben phai sau vai -> quat cheo qua truoc nguoi
    // -> trung (hit) giua than -> vung theo da xuong hong trai -> thu gay -> ve tu the cam
    // tay phai: Arm Front-Back am la dua ra truoc; than: Twist duong la xoay vai phai ra sau
    // co tay: Hand Down-Up duong ngha gay ra ngoai ben phai (xa dau), Hand In-Out am la bat gay ra sau, duong la quat gay ra truoc
    // lay da va thu gay cham lai cho ro dong tac, doan quat (0.42 -> 0.48) van giu nhanh de co luc
    public static float[] ATTACK_TIMES = { 0f, 0.17f, 0.34f, 0.42f, 0.48f, 0.66f, 0.9f, 1.1f };
    public static float HIT_TIME = 0.48f;
    public static float ATTACK_DURATION = 1.1f;

    public static string[] ARM_MUSCLES = {
        "Right Shoulder Down-Up", "Right Shoulder Front-Back",
        "Right Arm Down-Up", "Right Arm Front-Back", "Right Arm Twist In-Out",
        "Right Forearm Stretch", "Right Forearm Twist In-Out",
        "Right Hand Down-Up", "Right Hand In-Out",
    };

    // tu the cam gay kieu roblox: tay phai gan thang (khuyu hoi chung), dua ra truoc hoi lech sang phai,
    // nam tay ngang nguc ngay truoc vai phai, gay dung thang len
    public static float[] HOLD_ARM = { 0f, 0f, 0.42f, -0.6f, -0.25f, 0.9f, 1f, 0f, 0f };

    // huong truc xuyen qua nam tay (root space) o pose HoldBat, do tren rig mixamo tu T-pose
    public static Vector3 HOLD_GRIP_AXIS = new Vector3(0.06f, 1f, 0.03f);

    // moi hang la 1 moc thoi gian trong ATTACK_TIMES
    public static float[][] ATTACK_ARM = {
        HOLD_ARM,
        // gio len phia tren ben phai, co tay nga gay ra ngoai ngay tu dau de dau gay khong quet qua dau
        new float[] { 0.5f, 0f, 0.55f, -0.3f, 0.3f, 0.6f, 0.6f, 0.3f, -0.25f },
        // lay da: tay mo rong sang ben phai canh dau (khong o tren dau), khuyu gap vua, co tay bat gay ra sau-ra ngoai
        new float[] { 0.8f, 0f, 0.75f, 0.3f, 0.6f, 0.5f, 1f, 0.5f, -0.35f },
        // dang quat: tay di rong qua truoc vai phai, co tay van bat ve sau (gay tre theo tay) de lay luc quat
        new float[] { 0.3f, 0f, 0.65f, -0.45f, 0.2f, 0.85f, 1f, 0.45f, -0.4f },
        // hit: co tay quat manh ra truoc (-0.4 -> 1 trong 0.06s), tay duoi thang truoc nguoi ngang nguc
        new float[] { -0.2f, 0f, 0.2f, -0.85f, -0.4f, 1f, 1f, 0.4f, 1f },
        // vung theo da: tay vat cheo xuong hong trai nhung khong ep sat nguoi, cang tay xoay nhe theo da
        new float[] { -0.4f, 0f, -0.15f, -0.9f, -0.6f, 0.95f, 0.9f, 0.2f, 0.9f },
        // co tay tha long dan truoc khi ve tu the cam
        new float[] { -0.2f, 0f, 0.1f, -0.65f, -0.3f, 0.8f, 0.95f, 0.2f, 0.4f },
        HOLD_ARM,
    };

    public static string[] BODY_MUSCLES = {
        "Spine Front-Back", "Spine Left-Right", "Spine Twist Left-Right",
        "Chest Front-Back", "Chest Twist Left-Right",
        "UpperChest Front-Back", "UpperChest Twist Left-Right",
        "Left Arm Down-Up", "Left Arm Front-Back", "Left Forearm Stretch",
    };

    // lay da: than nghieng + xoay manh vai phai ra sau; quat: xoay nguoc sang trai, cui va nghieng theo huong gay (front-back am la cui)
    // xoay than la phan chinh keo gay cheo qua nguoi, tay chi can duoi ra truoc
    public static float[][] ATTACK_BODY = {
        new float[] { 0f, 0f, 0f, 0f, 0f, 0f, 0f, -0.3f, 0.15f, 0.9f },
        new float[] { 0.1f, 0.08f, 0.25f, 0.1f, 0.25f, 0.1f, 0.25f, -0.2f, 0f, 0.6f },
        new float[] { 0.15f, 0.08f, 0.45f, 0.15f, 0.45f, 0.15f, 0.45f, -0.1f, -0.4f, 0.3f },
        new float[] { 0f, 0.05f, 0.1f, 0f, 0.1f, 0f, 0.1f, -0.25f, -0.2f, 0.3f },
        new float[] { -0.3f, -0.15f, -0.4f, -0.3f, -0.4f, -0.25f, -0.4f, -0.45f, 0.2f, 0.2f },
        new float[] { -0.4f, -0.25f, -0.6f, -0.4f, -0.6f, -0.35f, -0.6f, -0.55f, 0.3f, 0.2f },
        new float[] { -0.2f, -0.12f, -0.3f, -0.2f, -0.3f, -0.18f, -0.3f, -0.42f, 0.22f, 0.5f },
        new float[] { 0f, 0f, 0f, 0f, 0f, 0f, 0f, -0.3f, 0.15f, 0.9f },
    };

    public static AvatarMask CreateMask(string path, params AvatarMaskBodyPart[] activeParts) {
        AvatarMask mask = AssetDatabase.LoadAssetAtPath<AvatarMask>(path);
        if (mask == null) {
            mask = new AvatarMask();
            AssetDatabase.CreateAsset(mask, path);
        }

        for (int i = 0; i < (int)AvatarMaskBodyPart.LastBodyPart; i++) {
            AvatarMaskBodyPart part = (AvatarMaskBodyPart)i;
            mask.SetHumanoidBodyPartActive(part, System.Array.IndexOf(activeParts, part) >= 0);
        }

        EditorUtility.SetDirty(mask);
        return mask;
    }

    public static AnimationClip CreateHoldClip(string path) {
        AnimationClip clip = GetOrCreateClip(path);
        clip.ClearCurves();
        SetMuscleCurves(clip, ARM_MUSCLES, new float[] { 0f, 1f }, new float[][] { HOLD_ARM, HOLD_ARM });
        SetClipSettings(clip, true, new AnimationEvent[0]);
        return clip;
    }

    public static AnimationClip CreateAttackClip(string path) {
        AnimationClip clip = GetOrCreateClip(path);
        clip.ClearCurves();
        SetMuscleCurves(clip, ARM_MUSCLES, ATTACK_TIMES, ATTACK_ARM);

        AnimationEvent hitEvent = new AnimationEvent();
        hitEvent.time = HIT_TIME;
        hitEvent.functionName = HIT_EVENT_NAME;
        SetClipSettings(clip, false, new AnimationEvent[] { hitEvent });
        return clip;
    }

    public static AnimationClip CreateAttackBodyClip(string path) {
        AnimationClip clip = GetOrCreateClip(path);
        clip.ClearCurves();
        SetMuscleCurves(clip, BODY_MUSCLES, ATTACK_TIMES, ATTACK_BODY);
        SetClipSettings(clip, false, new AnimationEvent[0]);
        return clip;
    }

    // tao lai 2 layer: Attack (index 1) HoldBat mac dinh, AttackBody (index 2) Empty mac dinh
    // Attack chay xong tu ve state mac dinh cua layer
    public static void SetupAttackLayers(AnimatorController controller, AvatarMask armMask, AvatarMask bodyMask, AnimationClip holdClip, AnimationClip attackClip, AnimationClip attackBodyClip) {
        for (int i = controller.layers.Length - 1; i >= 0; i--) {
            if (controller.layers[i].name == ATTACK_LAYER_NAME || controller.layers[i].name == ATTACK_BODY_LAYER_NAME) {
                controller.RemoveLayer(i);
            }
        }

        AddAttackLayer(controller, ATTACK_LAYER_NAME, armMask, HOLD_STATE_NAME, holdClip, attackClip);
        AddAttackLayer(controller, ATTACK_BODY_LAYER_NAME, bodyMask, EMPTY_STATE_NAME, null, attackBodyClip);
        EditorUtility.SetDirty(controller);
    }

    // dua animator ve dung pose tai thoi diem overlayTime (dung trong editor, khong can play mode)
    // base clip lay ca than, cac clip overlay chi de len cac muscle co curve (giong layer co mask)
    public static void SamplePose(Animator animator, AnimationClip baseClip, float baseTime, float overlayTime, params AnimationClip[] overlayClips) {
        baseClip.SampleAnimation(animator.gameObject, baseTime);
        Transform hips = animator.GetBoneTransform(HumanBodyBones.Hips);
        Vector3 hipsPosition = hips.localPosition;
        Quaternion hipsRotation = hips.localRotation;

        HumanPoseHandler poseHandler = new HumanPoseHandler(animator.avatar, animator.transform);
        HumanPose pose = new HumanPose();
        poseHandler.GetHumanPose(ref pose);

        foreach (AnimationClip clip in overlayClips) {
            foreach (EditorCurveBinding binding in AnimationUtility.GetCurveBindings(clip)) {
                int muscleIndex = GetMuscleIndex(binding.propertyName);
                if (muscleIndex < 0) continue;

                pose.muscles[muscleIndex] = AnimationUtility.GetEditorCurve(clip, binding).Evaluate(overlayTime);
            }
        }

        poseHandler.SetHumanPose(ref pose);
        poseHandler.Dispose();

        // giu hong theo base clip giong layer co mask, chi phan than tren thay doi
        hips.localPosition = hipsPosition;
        hips.localRotation = hipsRotation;
    }

    // ten curve ngon tay "RightHand.Index.1 Stretched" ung voi muscle "Right Index 1 Stretched"
    private static int GetMuscleIndex(string propertyName) {
        string muscleName = propertyName.Replace("RightHand.", "Right ").Replace("LeftHand.", "Left ").Replace(".", " ");
        return System.Array.IndexOf(HumanTrait.MuscleName, muscleName);
    }

    // dat model gay duoi batRoot: tay cam o goc toa do, than gay chi theo truc +Y cua batRoot
    // batModel la instance cua bat.fbx, la con cua batRoot
    public static void FitBatModel(Transform batRoot, Transform batModel, float batLength) {
        batModel.SetParent(batRoot, false);
        batModel.localPosition = Vector3.zero;
        batModel.localRotation = Quaternion.identity;
        batModel.localScale = Vector3.one;

        MeshFilter meshFilter = batModel.GetComponentInChildren<MeshFilter>();
        GetBatAxis(meshFilter.sharedMesh, out Vector3 axis, out Vector3 grip, out float length);
        Transform meshTF = meshFilter.transform;
        Vector3 meshAxis = batRoot.InverseTransformDirection(meshTF.TransformDirection(axis)).normalized;
        Vector3 meshGripPoint = batRoot.InverseTransformPoint(meshTF.TransformPoint(grip));
        float meshLength = batRoot.InverseTransformVector(meshTF.TransformVector(axis * length)).magnitude;
        float scale = batLength / meshLength;

        Quaternion rotation = Quaternion.FromToRotation(meshAxis, Vector3.up);
        batModel.localRotation = rotation;
        batModel.localScale = Vector3.one * scale;
        batModel.localPosition = -(rotation * (meshGripPoint * scale));
    }

    // goi sau khi animator da o pose HoldBat: tinh local transform cua diem cam gay trong khong gian xuong tay phai
    // diem cam o tam khoi nam tay, truc +Y trung truc xuyen qua nam tay (ngon cai huong len) o pose HoldBat
    public static void GetBatHoldPointPose(Animator animator, out Vector3 localPosition, out Quaternion localRotation, out Vector3 localScale) {
        Transform hand = animator.GetBoneTransform(HumanBodyBones.RightHand);
        Transform root = animator.transform;

        Vector3 gripPoint = GetHandMeshCenter(animator, hand);
        Vector3 shaftDirection = root.TransformDirection(HOLD_GRIP_AXIS).normalized;
        Quaternion worldRotation = Quaternion.LookRotation(Vector3.Cross(root.right, shaftDirection), shaftDirection);

        localPosition = hand.InverseTransformPoint(gripPoint);
        localRotation = Quaternion.Inverse(hand.rotation) * worldRotation;
        localScale = Vector3.one * (root.lossyScale.x / hand.lossyScale.x);
    }

    // tam khoi ban tay: tam hop bao (trong khong gian xuong tay) cua cac dinh mesh an theo xuong ban tay, bo ngon tay
    public static Vector3 GetHandMeshCenter(Animator animator, Transform hand) {
        Bounds localBounds = new Bounds();
        bool hasPoint = false;
        Mesh bakedMesh = new Mesh();

        foreach (SkinnedMeshRenderer skinnedMesh in animator.GetComponentsInChildren<SkinnedMeshRenderer>(true)) {
            Transform[] bones = skinnedMesh.bones;
            BoneWeight[] boneWeights = skinnedMesh.sharedMesh.boneWeights;
            if (boneWeights.Length == 0) continue;

            skinnedMesh.BakeMesh(bakedMesh, false);
            Vector3[] vertices = bakedMesh.vertices;
            Matrix4x4 toWorld = Matrix4x4.TRS(skinnedMesh.transform.position, skinnedMesh.transform.rotation, Vector3.one);

            for (int i = 0; i < vertices.Length; i++) {
                if (bones[boneWeights[i].boneIndex0] != hand || boneWeights[i].weight0 < 0.5f) continue;

                Vector3 localPoint = hand.InverseTransformPoint(toWorld.MultiplyPoint3x4(vertices[i]));
                if (!hasPoint) {
                    localBounds = new Bounds(localPoint, Vector3.zero);
                    hasPoint = true;
                }
                localBounds.Encapsulate(localPoint);
            }
        }

        Object.DestroyImmediate(bakedMesh);
        return hasPoint ? hand.TransformPoint(localBounds.center) : hand.position;
    }

    // truc dai nhat cua mesh, dau nho hon la tay cam; diem cam cach dau tay cam 12% chieu dai
    public static void GetBatAxis(Mesh mesh, out Vector3 axis, out Vector3 gripPoint, out float length) {
        Bounds bounds = mesh.bounds;
        Vector3 size = bounds.size;
        int axisIndex = size.x >= size.y && size.x >= size.z ? 0 : (size.y >= size.z ? 1 : 2);
        axis = Vector3.zero;
        axis[axisIndex] = 1f;
        length = size[axisIndex];

        float minValue = bounds.min[axisIndex];
        float maxValue = bounds.max[axisIndex];
        float minEndRadius = 0f;
        float maxEndRadius = 0f;
        int minCount = 0;
        int maxCount = 0;
        Vector3[] vertices = mesh.vertices;

        for (int i = 0; i < vertices.Length; i++) {
            Vector3 radial = vertices[i] - bounds.center;
            radial[axisIndex] = 0f;
            float value = vertices[i][axisIndex];

            if (value < minValue + length * 0.1f) {
                minEndRadius += radial.magnitude;
                minCount++;
            }
            else if (value > maxValue - length * 0.1f) {
                maxEndRadius += radial.magnitude;
                maxCount++;
            }
        }

        minEndRadius /= Mathf.Max(1, minCount);
        maxEndRadius /= Mathf.Max(1, maxCount);

        bool isHandleAtMin = minEndRadius <= maxEndRadius;
        if (!isHandleAtMin) axis = -axis;

        gripPoint = bounds.center;
        gripPoint[axisIndex] = isHandleAtMin ? minValue + length * 0.12f : maxValue - length * 0.12f;
    }

    public static AnimationClip GetOrCreateClip(string path) {
        AnimationClip clip = AssetDatabase.LoadAssetAtPath<AnimationClip>(path);
        if (clip == null) {
            clip = new AnimationClip();
            AssetDatabase.CreateAsset(clip, path);
        }

        return clip;
    }

    private static void AddAttackLayer(AnimatorController controller, string layerName, AvatarMask mask, string defaultStateName, AnimationClip defaultClip, AnimationClip attackClip) {
        controller.AddLayer(layerName);
        AnimatorControllerLayer[] layers = controller.layers;
        int layerIndex = layers.Length - 1;
        layers[layerIndex].avatarMask = mask;
        layers[layerIndex].defaultWeight = 1f;
        layers[layerIndex].blendingMode = AnimatorLayerBlendingMode.Override;
        controller.layers = layers;

        AnimatorStateMachine stateMachine = controller.layers[layerIndex].stateMachine;
        AnimatorState defaultState = stateMachine.AddState(defaultStateName, new Vector3(300f, 0f, 0f));
        defaultState.motion = defaultClip;
        AnimatorState attackState = stateMachine.AddState(ATTACK_STATE_NAME, new Vector3(300f, 100f, 0f));
        attackState.motion = attackClip;
        stateMachine.defaultState = defaultState;

        AnimatorStateTransition backTransition = attackState.AddTransition(defaultState);
        backTransition.hasExitTime = true;
        backTransition.exitTime = 1f;
        backTransition.duration = 0.1f;
        backTransition.hasFixedDuration = true;
    }

    public static void SetClipSettings(AnimationClip clip, bool isLoop, AnimationEvent[] events) {
        AnimationClipSettings settings = AnimationUtility.GetAnimationClipSettings(clip);
        settings.loopTime = isLoop;
        AnimationUtility.SetAnimationClipSettings(clip, settings);
        AnimationUtility.SetAnimationEvents(clip, events);
        EditorUtility.SetDirty(clip);
    }

    // values[i][j]: gia tri muscle j tai moc thoi gian i
    public static void SetMuscleCurves(AnimationClip clip, string[] muscleNames, float[] times, float[][] values) {
        for (int j = 0; j < muscleNames.Length; j++) {
            AnimationCurve curve = new AnimationCurve();
            for (int i = 0; i < times.Length; i++) {
                curve.AddKey(times[i], values[i][j]);
            }

            for (int i = 0; i < curve.length; i++) {
                AnimationUtility.SetKeyLeftTangentMode(curve, i, AnimationUtility.TangentMode.ClampedAuto);
                AnimationUtility.SetKeyRightTangentMode(curve, i, AnimationUtility.TangentMode.ClampedAuto);
            }

            SetCurve(clip, muscleNames[j], curve);
        }
    }

    private static void SetCurve(AnimationClip clip, string propertyName, AnimationCurve curve) {
        EditorCurveBinding binding = EditorCurveBinding.FloatCurve("", typeof(Animator), propertyName);
        AnimationUtility.SetEditorCurve(clip, binding, curve);
    }
}
