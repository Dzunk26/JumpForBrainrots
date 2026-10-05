using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu()]
public class ListObstacleSO : ScriptableObject {
    [SerializeField] private List<ObstacleSO> listObstacleSO;

    public ObstacleSO GetRandomObstacleSO() {
        int randomIndex = Random.Range(0, listObstacleSO.Count);

        return listObstacleSO[randomIndex];
    }
}