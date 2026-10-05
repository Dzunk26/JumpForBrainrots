using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BrainrotInteractor : TargetInteractor<Brainrot> {
    protected override Brainrot GetTarget(Collider collider) {
        return Cache.GetBrainrot(collider);
    }

    protected override Vector3 GetTargetPosition(Brainrot brainrot) {
        return brainrot.TF.position;
    }

    protected override bool IsValidTarget(Brainrot brainrot) {
        return brainrot.IsAlive;
    }

    //public Transform TF {
    //    get {
    //        if (tf == null) {
    //            tf = transform;
    //        }

    //        return tf;
    //    }
    //}

    //[SerializeField] private Camera playerCamera;
    //[SerializeField] private LayerMask brainrotLayerMask;
    //[SerializeField] private float interactRadius = 3f;
    //[SerializeField] private float minFacingDot = 0.3f;
    //[SerializeField] private float facingWeight = 1f;
    //[SerializeField] private float distanceWeight = 0.5f;
    //[SerializeField] private float scoreDelta = 0.15f; // han che button nhay qua lai giua cac brainrot
    //[SerializeField] private float scanTimerMax = 0.05f;

    //private Transform tf;
    //private Collider[] hitColliders = new Collider[25];
    //private Brainrot selectedBrainrot;
    //private float scanTimer;

    //private void Update() {
    //    scanTimer += Time.deltaTime;
    //    if (scanTimer < scanTimerMax) return;

    //    scanTimer = 0f;
    //    HandleSelectBrainrot();
    //}

    //private void OnDrawGizmos() {
    //    Gizmos.color = Color.red;
    //    Gizmos.DrawWireSphere(TF.position, interactRadius);
    //}

    //public void ClearSelectedBrainrot() {
    //    SetSelectedBrainrot(null);
    //}

    //public Brainrot GetSelectedBrainrot() {
    //    return selectedBrainrot;
    //}

    //private void OnDrawGizmos() {
    //    Gizmos.color = Color.red;
    //    Gizmos.DrawWireSphere(TF.position, interactRadius);
    //}

    //private void HandleSelectBrainrot() {
    //    Brainrot bestBrainrot = null;
    //    float bestScore = float.MinValue;

    //    int hitCount = Physics.OverlapSphereNonAlloc(TF.position, interactRadius, hitColliders, brainrotLayerMask);
    //    for (int i = 0; i < hitCount; i++) {
    //        Brainrot brainrot = Cache.GetBrainrot(hitColliders[i]);
    //        if (!CanSelect(brainrot)) continue;

    //        float score = GetScore(brainrot);
    //        if (score > bestScore) {
    //            bestScore = score;
    //            bestBrainrot = brainrot;
    //        }
    //    }

    //    if (bestBrainrot != selectedBrainrot && CanSelect(selectedBrainrot)) {
    //        if (bestScore - GetScore(selectedBrainrot) < scoreDelta) return;
    //    }

    //    SetSelectedBrainrot(bestBrainrot);
    //}

    //private bool CanSelect(Brainrot brainrot) {
    //    if (brainrot == null || !brainrot.IsAlive) return false;

    //    Vector3 dirToBrainrot = GetFlatDirection(brainrot);
    //    if (dirToBrainrot.sqrMagnitude > interactRadius * interactRadius) return false;

    //    return Vector3.Dot(GetCameraFlatForward(), dirToBrainrot.normalized) >= minFacingDot;
    //}

    //private float GetScore(Brainrot brainrot) {
    //    Vector3 dirToBrainrot = GetFlatDirection(brainrot);
    //    float facingDot = Vector3.Dot(GetCameraFlatForward(), dirToBrainrot.normalized);
    //    float distanceScore = 1f - dirToBrainrot.sqrMagnitude / (interactRadius * interactRadius);

    //    return facingDot * facingWeight + distanceScore * distanceWeight;
    //}

    //private Vector3 GetFlatDirection(Brainrot brainrot) {
    //    Vector3 dir = brainrot.TF.position - TF.position;
    //    dir.y = 0f;
    //    return dir;
    //}

    //private Vector3 GetCameraFlatForward() {
    //    Vector3 forward = playerCamera.transform.forward;
    //    forward.y = 0f;
    //    return forward.normalized;
    //}

    //private void SetSelectedBrainrot(Brainrot brainrot) {
    //    if (selectedBrainrot == brainrot) return;

    //    selectedBrainrot = brainrot;
    //}
}