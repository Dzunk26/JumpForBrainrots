using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObstacleVisual : MonoBehaviour {
    private Dictionary<ObstacleSO, ObstacleModel> dictObstacleModel = new Dictionary<ObstacleSO, ObstacleModel>();
    private ObstacleModel currentModel;

    public void OnInit(ObstacleSO obstacleSO) {
        if (currentModel != null) {
            currentModel.DeActive();
        }

        currentModel = GetBrainrotModel(obstacleSO);
        currentModel.Active();
    }

    private ObstacleModel GetBrainrotModel(ObstacleSO obstacleSO) {
        if (!dictObstacleModel.ContainsKey(obstacleSO)) {
            dictObstacleModel[obstacleSO] = Instantiate(obstacleSO.GetObstacleModel(), transform);
            dictObstacleModel[obstacleSO].ResetLocalPosition();
        }

        return dictObstacleModel[obstacleSO];
    }
}