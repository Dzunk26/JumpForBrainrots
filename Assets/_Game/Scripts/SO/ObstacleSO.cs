using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu()]
public class ObstacleSO : ScriptableObject {
    [SerializeField] private ObstacleModel obstacleModel;
    [SerializeField] private AudioClip loopClip;
    [SerializeField] private float speed = 10f;

    public ObstacleModel GetObstacleModel() {
        return obstacleModel;
    }

    public AudioClip GetAudioClip() {
        return loopClip;
    }

    public float GetSpeed() {
        return speed;
    }
}