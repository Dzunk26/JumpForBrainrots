using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BrainrotModel : MonoBehaviour {
    public void Active() {
        gameObject.SetActive(true);
    }

    public void DeActive() {
        gameObject.SetActive(false);
    }
}
