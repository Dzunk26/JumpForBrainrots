using UnityEngine;

[CreateAssetMenu()]
public class SkinSO : ScriptableObject {
    [SerializeField] private int id;
    [SerializeField] private SkinModel skinModel;
    [SerializeField] private Sprite icon;

    public bool IsMatchID(int id) {
        return this.id == id;
    }

    public int GetID() {
        return id;
    }

    public SkinModel GetSkinModel() {
        return skinModel;
    }

    public Sprite GetIcon() {
        return icon;
    }
}