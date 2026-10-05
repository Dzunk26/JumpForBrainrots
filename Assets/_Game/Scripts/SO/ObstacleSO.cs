using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu()]
public class ObstacleSO : ScriptableObject {
    [SerializeField] private ObstacleModel obstacleModel;
    [SerializeField] private float speed = 10f;

    public ObstacleModel GetObstacleModel() {
        return obstacleModel;
    }

    public float GetSpeed() {
        return speed;
    }
}