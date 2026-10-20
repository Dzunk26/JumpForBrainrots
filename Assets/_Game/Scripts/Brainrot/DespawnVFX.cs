using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DespawnVFX : GameUnit {
    [SerializeField] private ParticleSystem rootParticle;

    private float lifeTimer;

    private void Update() {
        lifeTimer -= Time.deltaTime;
        if (lifeTimer > 0f) return;

        SimplePool.ReturnToPool(this);
    }

    public void OnInit() {
        lifeTimer = rootParticle.main.duration;
        rootParticle.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
        rootParticle.Play(true);
    }
}