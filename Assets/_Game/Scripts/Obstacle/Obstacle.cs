using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class Obstacle : GameUnit {
    public bool IsAlive => isAlive;

    [SerializeField] private ObstacleVisual obstacleVisual;

    private ObstacleSO obstacleSO;
    private Vector3 endPoint;
    private float moveSpeed;
    private bool isAlive;

    private void Update() {
        HandleLifeTime();

        if (isAlive) {
            HandleMove();
        }
    }

    private void OnTriggerEnter(Collider other) {
        if (other.CompareTag(Constant.PLAYER_TAG)) {
            Player player = Cache.GetPlayer(other);
            player.OnDead();
        }
    }

    public void OnInit(ObstacleSO obstacleSO, Vector3 endPoint) {
        this.obstacleSO = obstacleSO;
        this.endPoint = endPoint;
        moveSpeed = obstacleSO.GetSpeed();
        isAlive = true;
        obstacleVisual.OnInit(obstacleSO);
    }

    public void OnDespawn() {
        obstacleSO = null;
        isAlive = false;
    }

    private void HandleMove() {
        TF.position = Vector3.MoveTowards(TF.position, endPoint, moveSpeed * Time.deltaTime);
    }

    private void HandleLifeTime() {
        if ((endPoint - TF.position).sqrMagnitude < 0.001f) {
            OnDespawn();
        }
    }
}