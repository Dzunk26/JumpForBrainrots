using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class DataManager : Singleton<DataManager> {
    private GameData gameData;
    private List<IDataPersistence> dataPersistenceObjects;
    private DataHandler dataHandler = new DataHandler();

    public void OnInit() {
        dataPersistenceObjects = FindAllDataPersistenceObjects();
    }

    public void NewGame() {
        gameData = new GameData();
    }

    public void LoadGame() {
        gameData = dataHandler.Load();

        if (gameData == null) {
            Debug.Log("No data was found. Initializing data to defaults");
            NewGame();
        }

        foreach (IDataPersistence dataPersistenceObj in dataPersistenceObjects) {
            dataPersistenceObj.LoadData(gameData);
        }
    }

    public void SaveGame() {
        foreach (IDataPersistence dataPersistenceObj in dataPersistenceObjects) {
            dataPersistenceObj.SaveData(ref gameData);
        }

        dataHandler.Save(gameData);
    }

    // save game khi chay tren dien thoai
    private void OnApplicationPause(bool pause) {
        if (pause) {
            SaveGame();
        }
    }

    // save game khi chay tren may tinh
    private void OnApplicationQuit() {
        SaveGame();
    }

    private List<IDataPersistence> FindAllDataPersistenceObjects() {
        IEnumerable<IDataPersistence> dataPersistences = FindObjectsOfType<MonoBehaviour>().OfType<IDataPersistence>();

        return new List<IDataPersistence>(dataPersistences);
    }
}