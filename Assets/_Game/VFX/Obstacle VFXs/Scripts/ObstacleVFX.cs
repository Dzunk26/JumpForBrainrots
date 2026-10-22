using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObstacleVFX : MonoBehaviour {
    [SerializeField] private ParticleSystem rootParticleSystem;

    private void OnEnable() {
        // xoa hat world space con sot lai tu lan truoc khi obstacle duoc lay lai tu pool
        rootParticleSystem.Clear(true);
        rootParticleSystem.Play(true);
    }
}
