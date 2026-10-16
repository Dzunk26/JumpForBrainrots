using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BaseZone : MonoBehaviour {
    private void OnTriggerEnter(Collider other) {
        Character character = Cache.GetCharacter(other);
        if (character == null) return;

        character.StoreCapturedBrainrot();
    }
}