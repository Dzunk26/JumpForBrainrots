using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

public abstract class TargetInteractor<T> : MonoBehaviour where T : Component{
    public Transform TF {
        get {
            if (tf == null) {
                tf = transform;
            }

            return tf;
        }
    }

    [SerializeField] private Camera playerCamera;
    [FormerlySerializedAs("brainrotLayerMask")]
    [SerializeField] private LayerMask targetLayerMask;
    [SerializeField] private float interactRadius = 3f;
    [SerializeField] private float minFacingDot = 0.3f;
    [SerializeField] private float facingWeight = 1f;
    [SerializeField] private float distanceWeight = 0.5f;
    [SerializeField] private float scoreDelta = 0.15f; // han che button nhay qua lai giua cac target
    [SerializeField] private float scanTimerMax = 0.05f;

    private Transform tf;
    private Collider[] hitColliders = new Collider[25];
    private T selectedTarget;
    private float scanTimer;

    private void Update() {
        scanTimer += Time.deltaTime;
        if (scanTimer < scanTimerMax) return;

        scanTimer = 0f;
        HandleSelectTarget();
    }

    private void OnDisable() {
        ClearSelectedTarget();
    }

    public void ClearSelectedTarget() {
        SetSelectedTarget(null);
    }

    public T GetSelectedTarget() {
        return selectedTarget;
    }

    protected abstract T GetTarget(Collider collider);

    protected abstract Vector3 GetTargetPosition(T target);

    protected abstract bool IsValidTarget(T target);

    private void HandleSelectTarget() {
        T bestTarget = null;
        float bestScore = float.MinValue;

        int hitCount = Physics.OverlapSphereNonAlloc(TF.position, interactRadius, hitColliders, targetLayerMask);
        for (int i = 0; i < hitCount; i++) {
            T target = GetTarget(hitColliders[i]);
            if (!CanSelect(target)) continue;

            float score = GetScore(target);
            if (score > bestScore) {
                bestScore = score;
                bestTarget = target;
            }
        }

        if (bestTarget != selectedTarget && CanSelect(selectedTarget)) {
            if (bestScore - GetScore(selectedTarget) < scoreDelta) return;
        }

        SetSelectedTarget(bestTarget);
    }

    private bool CanSelect(T target) {
        if (target == null || !IsValidTarget(target)) return false;

        Vector3 dirToTarget = GetFlatDirection(target);
        if (dirToTarget.sqrMagnitude > interactRadius * interactRadius) return false;

        return Vector3.Dot(GetCameraFlatForward(), dirToTarget.normalized) >= minFacingDot;
    }

    private float GetScore(T target) {
        Vector3 dirToTarget = GetFlatDirection(target);
        float facingDot = Vector3.Dot(GetCameraFlatForward(), dirToTarget.normalized);
        float distanceScore = 1f - dirToTarget.sqrMagnitude / (interactRadius * interactRadius);

        return facingDot * facingWeight + distanceScore * distanceWeight;
    }

    private Vector3 GetFlatDirection(T target) {
        Vector3 dir = GetTargetPosition(target) - TF.position;
        dir.y = 0f;
        return dir;
    }

    private Vector3 GetCameraFlatForward() {
        Vector3 forward = playerCamera.transform.forward;
        forward.y = 0f;
        return forward.normalized;
    }

    private void SetSelectedTarget(T target) {
        if (selectedTarget == target) return;

        selectedTarget = target;
    }
}