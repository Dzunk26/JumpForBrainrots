using UnityEngine;

// 1 buoc tutorial, dung chung cho moi game: ke thua va override cac ham ben duoi
// gan len GameObject con cua TutorialController, thu tu buoc = thu tu trong list steps cua controller
public abstract class TutorialStep : MonoBehaviour {
    [SerializeField, TextArea] private string message;
    [SerializeField] private Vector3 targetOffset;

    protected TutorialController Controller { get; private set; }

    public string Message => message;
    public Vector3 TargetOffset => targetOffset;

    public void Initialize(TutorialController controller) {
        Controller = controller;
    }

    // game da san sang chua (vd: base cua player da init) -> chua thi controller doi, chua bat dau buoc
    public virtual bool IsReady() {
        return true;
    }

    public virtual void OnStepStart() { }

    public virtual void OnStepEnd() { }

    // controller goi moi frame, true -> sang buoc tiep theo
    public abstract bool IsCompleted();

    // diem mui ten chi toi (chua cong targetOffset), false -> tam an mui ten
    public abstract bool TryGetTargetPosition(out Vector3 position);
}
