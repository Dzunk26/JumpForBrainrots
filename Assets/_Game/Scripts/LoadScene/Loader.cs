using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public enum Scene {
    LoadingScene,
    GameScene
}

public class Loader : MonoBehaviour {
    public static Scene targetScene = Scene.GameScene;

    public static void Load(Scene targetScene) {
        Loader.targetScene = targetScene;

        SceneManager.LoadScene(Scene.LoadingScene.ToString());
    }

    public static AsyncOperation LoaderCallBack() {
        return SceneManager.LoadSceneAsync(targetScene.ToString(), LoadSceneMode.Additive);
    }

    public static void OnTargetSceneReady() {
        UnityEngine.SceneManagement.Scene loadingScene = SceneManager.GetSceneByName(Scene.LoadingScene.ToString());
        if (!loadingScene.isLoaded) return;

        SceneManager.SetActiveScene(SceneManager.GetSceneByName(targetScene.ToString()));
        SceneManager.UnloadSceneAsync(loadingScene);
    }
}