using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IDataPersistence {
    public bool IsDirty { get; }
    public  void LoadData(GameData gameData);
    public void SaveData(ref GameData gameData);
}
