using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SkinModel : MonoBehaviour {
    public Animator Animator => animator;

    [SerializeField] private Animator animator;

    public void Active() {
        gameObject.SetActive(true);
    }

    public void DeActive() {
        gameObject.SetActive(false);
    }
}