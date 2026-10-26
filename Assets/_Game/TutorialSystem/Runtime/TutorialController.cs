using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Events;

// chay lan luot cac TutorialStep, moi buoc keo mui ten cau vong tu pathStartPoint den target cua buoc
// khong biet gi ve game cu the: luu tien do / dieu kien tung buoc nam o script rieng cua moi game
// - co save: script save goi StartTutorial(buocDaLuu) va doc CurrentStepIndex khi luu
// - khong save: bat isStartOnAwake
public class TutorialController : MonoBehaviour {
    public event Action<int> OnStepStarted;
    public event Action<int> OnStepCompleted;
    public event Action OnTutorialFinished;

    [SerializeField] private List<TutorialStep> steps = new List<TutorialStep>();
    [SerializeField] private TutorialPathArrow pathArrow;
    [SerializeField] private Transform pathStartPoint;
    [SerializeField] private GameObject targetBeacon;
    [SerializeField] private TMP_Text messageText;
    [SerializeField] private UnityEvent<string> onMessageChanged;
    [SerializeField] private bool isStartOnAwake;
    [SerializeField] private bool isAlwaysRestart;

    private int currentStepIndex;
    private bool isRunning;
    private bool isStepStarted;
    private Transform targetPoint;

    public int CurrentStepIndex => currentStepIndex;
    public bool IsFinished => currentStepIndex >= steps.Count;
    public bool IsAlwaysRestart => isAlwaysRestart;
    public Transform PathStartPoint => pathStartPoint;

    private void Awake() {
        targetPoint = new GameObject("TutorialTargetPoint").transform;
        targetPoint.SetParent(transform);

        // gan prefab tu Project vao thi tu sinh 1 ban trong scene, prefab asset khong render duoc
        if (!pathArrow.gameObject.scene.IsValid()) pathArrow = Instantiate(pathArrow);

        for (int i = 0; i < steps.Count; i++) {
            steps[i].Initialize(this);
        }

        if (isStartOnAwake) StartTutorial(0);
    }

    private void Update() {
        if (!isRunning || IsFinished) return;

        TutorialStep step = steps[currentStepIndex];
        if (!isStepStarted) {
            if (!step.IsReady()) return;
            BeginStep(step);
        }

        if (step.IsCompleted()) {
            CompleteCurrentStep();
            return;
        }

        UpdateTarget(step);
    }

    public void StartTutorial(int stepIndex) {
        currentStepIndex = isAlwaysRestart ? 0 : Mathf.Max(0, stepIndex);
        isRunning = true;
        isStepStarted = false;
        if (IsFinished) FinishTutorial();
    }

    public void StopTutorial() {
        isRunning = false;
        HideGuide();
    }

    // cho game kieu event-driven tu bao xong buoc, khong can doi IsCompleted
    public void CompleteCurrentStep() {
        if (!isRunning || IsFinished) return;

        if (isStepStarted) steps[currentStepIndex].OnStepEnd();
        OnStepCompleted?.Invoke(currentStepIndex);

        currentStepIndex++;
        isStepStarted = false;
        if (IsFinished) FinishTutorial();
    }

    private void BeginStep(TutorialStep step) {
        isStepStarted = true;
        step.OnStepStart();
        SetMessage(step.Message);
        OnStepStarted?.Invoke(currentStepIndex);
    }

    private void UpdateTarget(TutorialStep step) {
        if (!step.TryGetTargetPosition(out Vector3 position)) {
            pathArrow.Hide();
            if (targetBeacon != null) targetBeacon.SetActive(false);
            return;
        }

        targetPoint.position = position + step.TargetOffset;
        if (!pathArrow.IsShowing) pathArrow.Show(pathStartPoint, targetPoint);

        if (targetBeacon != null) {
            targetBeacon.transform.position = targetPoint.position;
            targetBeacon.SetActive(true);
        }
    }

    private void FinishTutorial() {
        isRunning = false;
        HideGuide();
        OnTutorialFinished?.Invoke();
    }

    private void HideGuide() {
        pathArrow.Hide();
        if (targetBeacon != null) targetBeacon.SetActive(false);
        SetMessage(null);
    }

    private void SetMessage(string message) {
        bool isVisible = !string.IsNullOrEmpty(message);
        if (messageText != null) {
            messageText.text = message;
            messageText.gameObject.SetActive(isVisible);
        }

        onMessageChanged?.Invoke(message);
    }
}
