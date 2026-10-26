using BreakInfinity;
using TMPro;
using UnityEngine;

public class BaseSlotUI : MonoBehaviour {
    public Transform TF {
        get {
            if (tf == null) {
                tf = transform;
            }

            return tf;
        }
    }

    [SerializeField] private TextMeshPro totalIncomeText;
    [SerializeField] private BaseSlot baseSlot;
    [SerializeField] private float checkTimerMax = 0.1f;
    [SerializeField] private float showDistance = 40f;
    [SerializeField] private float hideDistance = 45f;

    private Transform tf;
    private Transform cameraTF;
    private float checkTimer;
    private bool isShowing;
    private string notatedMoney;

    private void Start() {
        baseSlot.OnTotalIncomeChanged += BaseSlot_OnTotalIncomeChanged;
        baseSlot.OnBrainrotChanged += BaseSlot_OnBrainrotChanged;
        cameraTF = Camera.main.transform;

        UpdateVisual();
    }

    private void LateUpdate() {
        HandleCheckDistance();
        LookAtCamera();
    }

    private void OnDestroy() {
        baseSlot.OnTotalIncomeChanged -= BaseSlot_OnTotalIncomeChanged;
        baseSlot.OnBrainrotChanged -= BaseSlot_OnBrainrotChanged;
    }

    private void BaseSlot_OnBrainrotChanged(object sender, System.EventArgs e) {
        UpdateVisual();
    }

    private void BaseSlot_OnTotalIncomeChanged(object sender, System.EventArgs e) {
        UpdateVisual();
    }

    private void UpdateVisual() {
        UpdateTextActive();
        if (!baseSlot.IsActive || !isShowing) return;

        BigDouble totalIncome = baseSlot.GetTotalIncome();
        notatedMoney = Utils.SciNotToUSName(totalIncome);

        totalIncomeText.SetText(Constant.MONEY_SYMBOL + notatedMoney);
    }

    private void HandleCheckDistance() {
        checkTimer += Time.deltaTime;
        if (checkTimer < checkTimerMax) return;

        checkTimer = 0f;
        float sqrDistance = (cameraTF.position - TF.position).sqrMagnitude;
        if (isShowing) {
            if (sqrDistance > hideDistance * hideDistance) {
                isShowing = false;
                UpdateVisual();
            }
        }
        else {
            if (sqrDistance < showDistance * showDistance) {
                isShowing = true;
                UpdateVisual();
            }
        }
    }

    private void LookAtCamera() {
        if (!isShowing) return;

        totalIncomeText.gameObject.transform.rotation = cameraTF.rotation;
    }

    private void UpdateTextActive() {
        totalIncomeText.gameObject.SetActive(baseSlot.IsActive && isShowing);
    }
}