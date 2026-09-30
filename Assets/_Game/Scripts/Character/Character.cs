using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Character : MonoBehaviour {
    public Transform TF {
        get {
            if (tf == null) {
                tf = transform;
            }
            return tf;
        }
    }

    private Transform tf;

    protected virtual void OnInit() {

    }

    protected virtual void OnDespawn() {

    }

    //protected virtual void 
}