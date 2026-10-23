using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LoaderCallBack : MonoBehaviour {
    [SerializeField] private RectTransform fillTF;
    [SerializeField] private float fillSpeed = 2f;

    private AsyncOperation operation;
    private bool isFirstUpdate = true;
    private float progress;

    private void Update() {
        if (isFirstUpdate) {
            isFirstUpdate = false;
            operation = Loader.LoaderCallBack();
            operation.allowSceneActivation = false;
        }

        if (operation == null || operation.allowSceneActivation) return;

        float targetProgress = Mathf.Clamp01(operation.progress / 0.9f);
        progress = Mathf.MoveTowards(progress, targetProgress, fillSpeed * Time.deltaTime);

        float minFill = 0.07f;
        fillTF.anchorMax = new Vector2(Mathf.Lerp(minFill, 1f, progress), fillTF.anchorMax.y);

        if (progress >= 1f) {
            operation.allowSceneActivation = true;
        }
    }
}