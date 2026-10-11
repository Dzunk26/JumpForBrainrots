using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum BrainrotVFXTheme {
    Auto,
    Sparkle,
    Fire,
    Water,
    Ice,
    Electric,
    Nature,
    Sweet,
    Cosmic,
    Steam,
    Shadow,
    Rhythm
}

// gan tren root VFX cua brainrot (con cua BrainrotModel)
// giu VFX dung yen tai vi tri goc khi model nhun / xoay / bep, phat lai particle sach khi model duoc lay tu pool
// luu theme, rarity, mau de tool doc lai khi tao lai VFX
public class BrainrotVFX : MonoBehaviour {
    public Transform TF {
        get {
            if (tf == null) {
                tf = transform;
            }

            return tf;
        }
    }

    [SerializeField] private BrainrotVFXTheme theme = BrainrotVFXTheme.Auto;
    [SerializeField] private BrainrotRarity rarity;
    [SerializeField] private Color primaryColor = Color.white;
    [SerializeField] private Color secondaryColor = Color.white;
    [SerializeField] private bool isFollowModelAnim = false;

    private Transform tf;
    private Transform anchor;
    private Vector3 baseLocalPosition;
    private Quaternion baseLocalRotation;
    private Vector3 baseLocalScale;
    private Vector3 anchorBaseLocalPosition;
    private Quaternion anchorBaseLocalRotation;
    private Vector3 anchorBaseLocalScale;
    private ParticleSystem[] particleSystems;
    private TrailRenderer[] trails;

    private void Awake() {
        baseLocalPosition = TF.localPosition;
        baseLocalRotation = TF.localRotation;
        baseLocalScale = TF.localScale;
        particleSystems = GetRootParticleSystems();
        trails = GetComponentsInChildren<TrailRenderer>(true);
    }

    // luc nay transform cua model la transform goc (BrainrotModel reset truoc khi DeActive)
    private void OnEnable() {
        anchor = TF.parent;
        if (anchor != null) {
            anchorBaseLocalPosition = anchor.localPosition;
            anchorBaseLocalRotation = anchor.localRotation;
            anchorBaseLocalScale = anchor.localScale;
        }

        HandleStabilize();
        RestartEffects();
    }

    private void LateUpdate() {
        HandleStabilize();
    }

    public void Setup(BrainrotVFXTheme theme, BrainrotRarity rarity, Color primaryColor, Color secondaryColor) {
        this.theme = theme;
        this.rarity = rarity;
        this.primaryColor = primaryColor;
        this.secondaryColor = secondaryColor;
    }

    public BrainrotVFXTheme GetTheme() {
        return theme;
    }

    public BrainrotRarity GetRarity() {
        return rarity;
    }

    public Color GetPrimaryColor() {
        return primaryColor;
    }

    public Color GetSecondaryColor() {
        return secondaryColor;
    }

    // dat lai transform nhu the model dang o transform goc, bo qua phan nhun / xoay / bep cua anim
    private void HandleStabilize() {
        if (isFollowModelAnim || anchor == null) return;

        Vector3 anchorSpacePosition = anchorBaseLocalPosition + anchorBaseLocalRotation * Vector3.Scale(anchorBaseLocalScale, baseLocalPosition);
        Quaternion anchorSpaceRotation = anchorBaseLocalRotation * baseLocalRotation;

        Transform anchorParent = anchor.parent;
        if (anchorParent != null) {
            TF.SetPositionAndRotation(anchorParent.TransformPoint(anchorSpacePosition), anchorParent.rotation * anchorSpaceRotation);
        }
        else {
            TF.SetPositionAndRotation(anchorSpacePosition, anchorSpaceRotation);
        }

        Vector3 anchorScale = anchor.localScale;
        TF.localScale = new Vector3(
            baseLocalScale.x * GetScaleRatio(anchorBaseLocalScale.x, anchorScale.x),
            baseLocalScale.y * GetScaleRatio(anchorBaseLocalScale.y, anchorScale.y),
            baseLocalScale.z * GetScaleRatio(anchorBaseLocalScale.z, anchorScale.z));
    }

    // xoa hat / vet cu (hat world space o vi tri cu cua pool) roi phat lai de prewarm chay, hien ra day du ngay
    private void RestartEffects() {
        for (int i = 0; i < trails.Length; i++) {
            trails[i].Clear();
        }

        for (int i = 0; i < particleSystems.Length; i++) {
            particleSystems[i].Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
            particleSystems[i].Play(true);
        }
    }

    // chi lay particle cap cao nhat cua moi hieu ung, Play(true) se tu chay con + sub emitter dung cach
    private ParticleSystem[] GetRootParticleSystems() {
        List<ParticleSystem> rootParticleSystems = new List<ParticleSystem>();

        foreach (ParticleSystem particleSystem in GetComponentsInChildren<ParticleSystem>(true)) {
            Transform parent = particleSystem.transform.parent;
            bool hasParticleParent = false;

            while (parent != null && parent != TF) {
                if (parent.GetComponent<ParticleSystem>() != null) {
                    hasParticleParent = true;
                    break;
                }

                parent = parent.parent;
            }

            if (!hasParticleParent) {
                rootParticleSystems.Add(particleSystem);
            }
        }

        return rootParticleSystems.ToArray();
    }

    private float GetScaleRatio(float baseScale, float currentScale) {
        if (Mathf.Abs(currentScale) < 0.0001f) return 1f;

        return baseScale / currentScale;
    }
}
