using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class BaseBoard : MonoBehaviour {
    [SerializeField] private TextMeshPro nameText;

    public void OnInit(Character owner) {
        SetName(owner.GetCharacterName());
    }

    public void OnDespawn() {
        SetName(Constant.DEFAULT_BASE_NAME);
    }

    private void SetName(string name) {
        nameText.SetText(name + Constant.DEFAULT_BASE_NAME_SUFFIX);
    }
}