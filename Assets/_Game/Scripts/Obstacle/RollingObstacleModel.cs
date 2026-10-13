using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RollingObstacleModel : ObstacleModel {
    [SerializeField] private Transform rollTF;
    [SerializeField] private float radius = 1f;
    [SerializeField] private float rollSpeedModifier = 1f;

    private Vector3 lastPosition;

    private void LateUpdate() {
        HandleRoll();
    }

    public override void Active() {
        base.Active();
        lastPosition = TF.position; // reset de khong bi quay giat khi lay lai tu pool
        rollTF.localRotation = Quaternion.identity;
    }

    private void HandleRoll() {
        Vector3 moveDelta = TF.position - lastPosition;
        lastPosition = TF.position;

        float moveDistance = moveDelta.magnitude;
        if (moveDistance < 0.0001f) return;

        TF.rotation = Quaternion.LookRotation(moveDelta);

        float rollAngle = moveDistance / radius * Mathf.Rad2Deg * rollSpeedModifier;
        rollTF.Rotate(Vector3.right, rollAngle, Space.Self);
    }
}