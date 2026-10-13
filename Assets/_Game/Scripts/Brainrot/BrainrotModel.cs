using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum BrainrotAnimType {
    None,
    Bounce,
    Sway,
    Spin,
    Wiggle
}

public class BrainrotModel : MonoBehaviour {
    public Transform TF {
        get {
            if (tf == null) {
                tf = transform;
            }

            return tf;
        }
    }

    [SerializeField] private BrainrotAnimType defaultAnimType = BrainrotAnimType.Bounce;
    [SerializeField] private bool isRandomAnim = true;
    [SerializeField] private float animSpeed = 1.5f;
    [SerializeField] private float animSpeedRandom = 0.2f;
    [SerializeField] private float bounceHeight = 0.25f;
    [SerializeField] private float squashAmount = 0.15f;
    [SerializeField] private float swayAngle = 15f;
    [SerializeField] private float spinSpeed = 120f;
    [SerializeField] private float wiggleAngle = 8f;
    [SerializeField] private float wiggleFrequency = 4f;
    [SerializeField] private float wiggleTimerMax = 0.6f;
    [SerializeField] private float wiggleRestTimerMin = 0.8f;
    [SerializeField] private float wiggleRestTimerMax = 2.5f;
    [Header("Anim Interact")]
    [SerializeField] private float flyTimerMax = 0.3f;
    [SerializeField] private float flyArcHeight = 1.5f;
    [Header("Infomation")]
    [SerializeField] private Transform infoPoint;

    private Transform tf;
    private Vector3 baseLocalPosition;
    private Quaternion baseLocalRotation;
    private Vector3 baseLocalScale;
    private BrainrotAnimType currentAnimType = BrainrotAnimType.None;
    private float animTimer;
    private float currentAnimSpeed;
    private float wiggleTimer;
    private float wiggleRestTimer;
    private float currentWiggleRestTimerMax;
    private bool isWiggling;
    private Vector3 flyStartLocalPosition;
    private float flyTimer;
    private bool isFlying;
    private Vector3 infoLocalPosition;

    private void Update() {
        if (isFlying) {
            HandleFly();
            return;
        }

        if (currentAnimType == BrainrotAnimType.None) return;

        animTimer += Time.deltaTime * currentAnimSpeed;
        HandleAnim();
    }

    public void Active() {
        gameObject.SetActive(true);
        OnInitAnim();
    }

    public void DeActive() {
        ResetAnimTransform();
        currentAnimType = BrainrotAnimType.None;
        gameObject.SetActive(false);
        isFlying = false;
        TF.localPosition = baseLocalPosition;
    }

    public void ResetLocalPosition() {
        TF.localPosition = Vector3.zero;
    }

    public void ChangeAnimType(BrainrotAnimType animType) {
        ResetAnimTransform();
        currentAnimType = animType;
    }

    public void ResetLocalRotation() {
        TF.localRotation = Quaternion.identity;
    }

    public void FlyFrom(Vector3 worldPosition) {
        flyStartLocalPosition = TF.parent.InverseTransformPoint(worldPosition);
        flyTimer = 0f;
        isFlying = true;
        TF.localPosition = flyStartLocalPosition;
    }

    public Vector3 GetInfoLocalPosition() {
        return infoLocalPosition;
    }

    private void HandleFly() {
        flyTimer += Time.deltaTime;
        float progress = Mathf.Clamp01(flyTimer / flyTimerMax);
        float easedProgress = 1f - (1f - progress) * (1f - progress);
        float arc = Mathf.Sin(progress * Mathf.PI) * flyArcHeight;

        TF.localPosition = Vector3.Lerp(flyStartLocalPosition, baseLocalPosition, easedProgress) + Vector3.up * arc;

        if (progress >= 1f) {
            isFlying = false;
        }
    }

    private void OnInitAnim() {
        baseLocalPosition = TF.localPosition;
        baseLocalRotation = TF.localRotation;
        baseLocalScale = TF.localScale;
        infoLocalPosition = TF.parent.InverseTransformPoint(infoPoint.position);

        animTimer = Random.Range(0f, 10f);
        currentAnimSpeed = animSpeed * Random.Range(1f - animSpeedRandom, 1f + animSpeedRandom);

        if (isRandomAnim) {
            currentAnimType = GetRandomDanceType();
        }
        else {
            currentAnimType = defaultAnimType;
        }
    }

    private void HandleAnim() {
        switch (currentAnimType) {
            case BrainrotAnimType.Bounce:
                HandleBounce();
                break;
            case BrainrotAnimType.Sway:
                HandleSway();
                break;
            case BrainrotAnimType.Spin:
                HandleSpin();
                break;
            case BrainrotAnimType.Wiggle:
                HandleWiggle();
                break;
        }
    }

    // nhun len xuong: cham dat thi bep, bat len thi gian, o dinh thi tron lai
    private void HandleBounce() {
        float phase = animTimer * Mathf.PI;
        float height = Mathf.Abs(Mathf.Sin(phase));
        float speed = Mathf.Abs(Mathf.Cos(phase));
        float squash = Mathf.Pow(1f - height, 6f);
        float stretchY = (speed - 2f * squash) * squashAmount;

        TF.localPosition = baseLocalPosition + Vector3.up * height * bounceHeight;
        TF.localScale = Vector3.Scale(baseLocalScale, new Vector3(1f - stretchY * 0.5f, 1f + stretchY, 1f - stretchY * 0.5f));
    }

    // lac lu trai phai, nhun len o giua moi nhip
    private void HandleSway() {
        float phase = animTimer * Mathf.PI;
        float height = Mathf.Abs(Mathf.Cos(phase));

        TF.localPosition = baseLocalPosition + Vector3.up * height * bounceHeight * 0.5f;
        TF.localRotation = Quaternion.Euler(0f, 0f, Mathf.Sin(phase) * swayAngle) * baseLocalRotation;
    }

    // vua nhun vua xoay vong
    private void HandleSpin() {
        HandleBounce();
        TF.localRotation = Quaternion.Euler(0f, animTimer * spinSpeed, 0f) * baseLocalRotation;
    }

    // lac nhe nhanh, dung khi player dang cam
    private void HandleWiggle() {
        if(!isWiggling) {
            HandleWiggleRest();
            return;
        }

        wiggleTimer += Time.deltaTime;
        if (wiggleTimer >= wiggleTimerMax) {
            StopWiggle();
            return;
        }

        // bien do manh dan len roi yeu dan de khong bi giat khi bat dau / ket thuc
        float envelope = Mathf.Sin(wiggleTimer / wiggleTimerMax * Mathf.PI);
        float phase = wiggleTimer * wiggleFrequency * 2f * Mathf.PI;
        float roll = Mathf.Sin(phase) * wiggleAngle * envelope;
        float twist = Mathf.Sin(phase * 1.37f) * wiggleAngle * 0.5f * envelope;

        TF.localRotation = Quaternion.Euler(0f, twist, roll) * baseLocalRotation;
    }

    private void HandleWiggleRest() {
        wiggleRestTimer += Time.deltaTime;
        if (wiggleRestTimer < currentWiggleRestTimerMax) return;

        StartWiggle();
    }

    private void StartWiggle() {
        isWiggling = true;
        wiggleTimer = 0f;
    }

    private void StopWiggle() {
        isWiggling = false;
        wiggleRestTimer = 0f;
        currentWiggleRestTimerMax = Random.Range(wiggleRestTimerMin, wiggleRestTimerMax);
        TF.localRotation = baseLocalRotation;
    }

    private void ResetAnimTransform() {
        if (currentAnimType == BrainrotAnimType.None) return;

        TF.localPosition = baseLocalPosition;
        TF.localRotation = baseLocalRotation;
        TF.localScale = baseLocalScale;
    }

    private BrainrotAnimType GetRandomDanceType() {
        int index = Random.Range((int)BrainrotAnimType.Bounce, (int)BrainrotAnimType.Spin + 1);
        return (BrainrotAnimType)index;
    }
}
