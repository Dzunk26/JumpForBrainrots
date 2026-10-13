using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class BrainrotInfor : MonoBehaviour {
    public Transform TF {
        get {
            if (tf == null) {
                tf = transform;
            }

            return tf;
        }
    }

    [SerializeField] private GameObject infoRoot;
    [SerializeField] private GameObject lifeTimeRoot;     
    [SerializeField] private TextMeshPro lifeTimeText;
    [SerializeField] private TextMeshPro rarityText;
    [SerializeField] private TextMeshPro nameText;
    [SerializeField] private TextMeshPro incomeText;
    [SerializeField] private float showDistance = 40f;
    [SerializeField] private float hideDistance = 45f;
    [SerializeField] private float checkTimerMax = 0.2f;
    [SerializeField] private float lifeTimeShowMax = 30f;

    private Transform tf;
    private Transform cameraTF;
    private float checkTimer;
    private bool isShowing;
    private int currentLifeSecond;

    private void LateUpdate() {
        HandleCheckDistance();
        LookAtCamera();
    }

    public void OnInit(BrainrotSO brainrotSO, Vector3 infoLocalPosition) {
        gameObject.SetActive(true);
        cameraTF = Camera.main.transform;
        TF.localPosition = infoLocalPosition;
        rarityText.SetText(brainrotSO.GetRarity().ToString());
        incomeText.SetText(brainrotSO.GetIncomeAmount() + "/s");
        nameText.SetText(brainrotSO.GetName());
        isShowing = false;
        infoRoot.SetActive(false);
        lifeTimeRoot.SetActive(false);
        checkTimer = checkTimerMax;
        currentLifeSecond = -1;
    }

    public void OnDespawn() {
        gameObject.SetActive(false);
    }

    public void SetLifeTime(float lifeTimeRemain) {
        if (lifeTimeRemain > lifeTimeShowMax) return;

        int lifeSecond = Mathf.CeilToInt(lifeTimeRemain);
        if (lifeSecond == currentLifeSecond) return;

        if (currentLifeSecond < 0) {
            lifeTimeRoot.SetActive(true);
        }

        currentLifeSecond = lifeSecond;
        lifeTimeText.SetText("{0}", lifeSecond);
    }

    private void HandleCheckDistance() {
        checkTimer += Time.deltaTime;
        if (checkTimer < checkTimerMax) return;

        checkTimer = 0f;
        float sqrDistance = (cameraTF.position - TF.position).sqrMagnitude;
        if (isShowing) {
            if (sqrDistance > hideDistance * hideDistance) {
                isShowing = false;
                infoRoot.SetActive(false);
            }
        }
        else {
            if (sqrDistance < showDistance * showDistance) {
                isShowing = true;
                infoRoot.SetActive(true);
            }
        }
    }

    private void LookAtCamera() {
        if (!isShowing) return;

        infoRoot.transform.rotation = cameraTF.rotation;
    }
}