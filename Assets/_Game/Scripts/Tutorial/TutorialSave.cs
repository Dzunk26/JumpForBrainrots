using UnityEngine;

// noi TutorialController (dung chung) voi he thong save cua game nay: load buoc da luu, luu lai khi qua buoc
[RequireComponent(typeof(TutorialController))]
public class TutorialSave : MonoBehaviour, IDataPersistence {
    public bool IsDirty => isDirty;

    [SerializeField] private TutorialController tutorialController;

    private bool isDirty;

    private void Reset() {
        tutorialController = GetComponent<TutorialController>();
    }

    private void Awake() {
        if (tutorialController == null) tutorialController = GetComponent<TutorialController>();
    }

    private void OnEnable() {
        tutorialController.OnStepCompleted += TutorialController_OnStepCompleted;
    }

    private void OnDisable() {
        tutorialController.OnStepCompleted -= TutorialController_OnStepCompleted;
    }

    public void LoadData(GameData gameData) {
        tutorialController.StartTutorial(gameData.tutorialStep);
    }

    public void SaveData(ref GameData gameData) {
        gameData.tutorialStep = tutorialController.CurrentStepIndex;
        isDirty = false;
    }

    private void TutorialController_OnStepCompleted(int stepIndex) {
        isDirty = true;
    }
}
