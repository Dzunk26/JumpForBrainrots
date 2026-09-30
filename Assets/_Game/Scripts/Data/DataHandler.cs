using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DataHandler {
    public GameData Load() {
        GameData loadedData = null;
        string dataToLoad = PlayerPrefs.GetString(Constant.PLAYER_DATA);
        loadedData = JsonUtility.FromJson<GameData>(dataToLoad);

        return loadedData;
    }

    public void Save(GameData data) {
        string dataToStore = JsonUtility.ToJson(data, true);
        PlayerPrefs.SetString(Constant.PLAYER_DATA, dataToStore);
    }
}