using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UICanvas : MonoBehaviour {
    [SerializeField] private bool isDestroyOnClose = false;

    private void Awake() {
        // xu ly tai tho
        RectTransform rect = GetComponent<RectTransform>();
        float ratio = (float)Screen.height / Screen.width;
        if (ratio > 2.1f) {
            Vector2 leftBottom = rect.offsetMin;
            Vector2 rightTop = rect.offsetMax;

            leftBottom.y = 0f;
            rightTop.y = -100f;

            rect.offsetMin = leftBottom;
            rect.offsetMax = rightTop;
        }
    }

    // goi truoc khi canvas duoc active
    public virtual void SetUp() {

    }

    // mo canvas
    public virtual void Open() {
        gameObject.SetActive(true);
    }

    // dong canvas sau mot khoang time
    public virtual void Close(float time) {
        Invoke(nameof(CloseDirectly), time);
    }

    // tat canvas
    public void CloseDirectly() {
        if (isDestroyOnClose) {
            Destroy(gameObject);
        }
        else {
            gameObject.SetActive(false);
        }
    }
}