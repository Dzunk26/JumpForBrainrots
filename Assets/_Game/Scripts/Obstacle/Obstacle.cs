using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class Obstacle : GameUnit {
    public bool IsAlive => isAlive;

    [SerializeField] private ObstacleVisual obstacleVisual;
    [SerializeField] private AudioSource loopSource;

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
        Character character = Cache.GetCharacter(other);
        if (character == null) return;

        character.Dead();
    }

    public void OnInit(ObstacleSO obstacleSO, Vector3 endPoint) {
        this.obstacleSO = obstacleSO;
        this.endPoint = endPoint;
        moveSpeed = obstacleSO.GetSpeed();
        isAlive = true;
        obstacleVisual.OnInit(obstacleSO);
        PlayLoopSound();
    }

    public void OnDespawn() {
        obstacleSO = null;
        isAlive = false;
        loopSource.Stop();
    }

    private void HandleMove() {
        TF.position = Vector3.MoveTowards(TF.position, endPoint, moveSpeed * Time.deltaTime);
    }

    private void HandleLifeTime() {
        if ((endPoint - TF.position).sqrMagnitude < 0.001f) {
            OnDespawn();
        }
    }

    private void PlayLoopSound() {
        AudioClip clip = obstacleSO.GetAudioClip();
        if (clip == null) return;

        loopSource.clip = clip;
        loopSource.time = Random.Range(0f, clip.length);
        loopSource.pitch = Random.Range(0.95f, 1.05f);
        loopSource.Play();
    }
}