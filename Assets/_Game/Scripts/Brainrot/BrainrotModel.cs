using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BrainrotModel : MonoBehaviour {
    public Transform TF {
        get {
            if (tf == null) {
                tf = transform;
            }

            return tf;
        }
    }

    private Transform tf;

    public void Active() {
        gameObject.SetActive(true);
    }

    public void DeActive() {
        gameObject.SetActive(false);
    }

    public void ResetLocalPosition() {
        TF.localPosition = Vector3.zero;
    }
}
