using UnityEngine;
using UnityEngine.UI;

public class ProgressBarVisual : MonoBehaviour {
    public bool IsAnimating => !Mathf.Approximately(currentFill, targetFill);

    [SerializeField] private Image fillImage;
    [SerializeField] private float fillSpeed = 2f;

    private float currentFill;
    private float targetFill;

    private void Update() {
        if (!IsAnimating) return;

        currentFill = Mathf.MoveTowards(currentFill, targetFill, fillSpeed * Time.unscaledDeltaTime);
        fillImage.fillAmount = currentFill;
    }

    public void SetFill(float value) {
        targetFill = Mathf.Clamp01(value);
    }

    public void SnapFill() {
        currentFill = targetFill;
        fillImage.fillAmount = currentFill;
    }
}