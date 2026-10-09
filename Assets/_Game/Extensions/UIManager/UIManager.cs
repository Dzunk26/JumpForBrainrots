using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIManager : Singleton<UIManager> {
    [SerializeField] private Transform parent;

    private Dictionary<System.Type, UICanvas> activedCanvases = new Dictionary<System.Type, UICanvas>();
    private Dictionary<System.Type, UICanvas> canvasPrefabs = new Dictionary<System.Type, UICanvas>();

    private void Awake() {
        // load UI prefabs tu resource
        UICanvas[] prefabs = Resources.LoadAll<UICanvas>("UI/");

        for (int i = 0; i < prefabs.Length; i++) {
            canvasPrefabs.Add(prefabs[i].GetType(), prefabs[i]);
        }
    }

    // mo canvas
    public T OpenUI<T>() where T : UICanvas {
        T canvas = GetUI<T>();

        canvas.SetUp();
        canvas.Open();

        return canvas;
    }


    // dong canvas sau mot khoang time
    public void CloseUI<T>(float time) where T : UICanvas {
        if (IsUIOpen<T>()) {
            activedCanvases[typeof(T)].Close(time);
        }
    }

    // dong canvas truc tiep
    public void CloseUIDirectly<T>() where T : UICanvas {
        if (IsUIOpen<T>()) {
            activedCanvases[typeof(T)].CloseDirectly();
        }
    }

    // kiem tra canvas da duoc tao hay chua
    public bool IsUILoaded<T>() where T : UICanvas {
        return activedCanvases.ContainsKey(typeof(T)) && activedCanvases[typeof(T)] != null;
    }

    // kiem tra canvas duoc active hay chua
    public bool IsUIOpen<T>() where T : UICanvas {
        return IsUILoaded<T>() && activedCanvases[typeof(T)].gameObject.activeSelf;
    }

    // lay canvas
    public T GetUI<T>() where T : UICanvas {
        if (!IsUILoaded<T>()) {
            T prefab = GetUIPrefab<T>();
            T canvas = Instantiate(prefab, parent);
            activedCanvases[typeof(T)] = canvas;
        }

        return activedCanvases[typeof(T)] as T;
    }

    private T GetUIPrefab<T>() where T : UICanvas {
        return canvasPrefabs[typeof(T)] as T;
    }

    // dong tat ca canvvas
    public void CloseAllUI() {
        foreach (KeyValuePair<System.Type, UICanvas> canvas in activedCanvases) {
            if (canvas.Value != null && canvas.Value.gameObject.activeSelf) {
                canvas.Value.Close(0f);
            }
        }
    }
}