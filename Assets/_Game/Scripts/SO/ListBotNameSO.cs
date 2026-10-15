using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu()]
public class ListBotNameSO : ScriptableObject {
    [SerializeField] private List<string> listBotName;

    public List<string> GetListBotName() {
        return listBotName;
    }
}