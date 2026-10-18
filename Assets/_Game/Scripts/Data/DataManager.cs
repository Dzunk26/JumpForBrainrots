using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class DataManager : Singleton<DataManager> {
    public event Action<long> OnReturnFromAfk;
    public long OfflineSeconds { get; private set; }

    private GameData gameData;
    private List<IDataPersistence> dataPersistenceObjects;
    private DataHandler dataHandler = new DataHandler();
    private long pauseUnixTime;
    private bool isDirty;
    private float dirtyTimer;
    private float autoSaveTimer;

    private void Update() {
        if (gameData == null) return;

        if (!isDirty && HasDirtyObject()) {
            isDirty = true;
            dirtyTimer = 0f;
        }

        if (isDirty) {
            dirtyTimer += Time.unscaledDeltaTime;
            if (dirtyTimer >= Constant.SAVE_DELAY) {
                SaveGame();
                return;
            }
        }

        autoSaveTimer += Time.unscaledDeltaTime;
        if (autoSaveTimer >= Constant.AUTO_SAVE_INTERVAL) {
            SaveGame();
        }
    }

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

        OfflineSeconds = GetAfkSeconds(gameData.lastSaveUnixTime);

        foreach (IDataPersistence dataPersistenceObj in dataPersistenceObjects) {
            dataPersistenceObj.LoadData(gameData);
        }
    }

    public void SaveGame() {
        if (gameData == null) return;

        foreach (IDataPersistence dataPersistenceObj in dataPersistenceObjects) {
            dataPersistenceObj.SaveData(ref gameData);
        }

        gameData.lastSaveUnixTime = GetNowUnixTime();
        dataHandler.Save(gameData);

        isDirty = false;
        dirtyTimer = 0f;
        autoSaveTimer = 0f;
    }

    // save game khi chay tren dien thoai
    private void OnApplicationPause(bool pause) {
        if (pause) {
            SaveGame();
            pauseUnixTime = GetNowUnixTime();
            return;
        }

        if (pauseUnixTime <= 0) return;

        long afkSeconds = GetAfkSeconds(pauseUnixTime);
        pauseUnixTime = 0;

        if (afkSeconds > 0) {
            OnReturnFromAfk?.Invoke(afkSeconds);
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

    private long GetAfkSeconds(long lastSaveUnixTime) {
        if (lastSaveUnixTime <= 0) return 0;   // lan dau choi

        long seconds = GetNowUnixTime() - lastSaveUnixTime;
        return Math.Min(Math.Max(seconds, 0), Constant.AFK_MAX_SECONDS);
    }

    private long GetNowUnixTime() {
        return DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }
    private bool HasDirtyObject() {
        for (int i = 0; i < dataPersistenceObjects.Count; i++) {
            if (dataPersistenceObjects[i].IsDirty) return true;
        }
        return false;
    }
}