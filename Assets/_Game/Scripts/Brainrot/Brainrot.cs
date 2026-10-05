using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Brainrot : GameUnit {
    [SerializeField] private Button buttonPickUp;
    [SerializeField] private Collider pickUpTrigger;
    [SerializeField] private BrainrotVisual brainrotVisual;

    private BrainrotSO brainrotSO;
    private float lifeTimerMax;
    private float lifeTimer = 0f;
    private bool isAlive = false;

    //private void Awake() {
    //    buttonPickUp.onClick.AddListener(OnPickedUp);
    //}

    private void OnTriggerEnter(Collider other) {
        if (other.CompareTag(Constant.PLAYER_TAG)) {
            Player player = Cache.GetPlayer(other);

        }
    }

    public void OnInit(BrainrotSO brainrotSO) {
        this.brainrotSO = brainrotSO;
        isAlive = true;
        lifeTimerMax = brainrotSO.GetAppearTimerMax();
        lifeTimer = 0f;
        brainrotVisual.OnInit(brainrotSO);
    }

    public void OnDespawn() {
        this.brainrotSO = null;
        isAlive = false;
    }

    public bool IsAlive() {
        return isAlive;
    }

    private void OnPickedUp() {

    }

    private void HandleLifeTime() {
        lifeTimer += Time.deltaTime;
        if (lifeTimer >= lifeTimerMax) {
            OnDespawn();
        }
    }
}