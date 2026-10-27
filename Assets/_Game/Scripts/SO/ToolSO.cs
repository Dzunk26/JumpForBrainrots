using UnityEngine;

// dung cu mac dinh cua nguoi choi (gay), khong ban duoc
// id de am (< -1) de khong trung id brainrot va o trong (-1)
[CreateAssetMenu()]
public class ToolSO : ItemSO {
    [SerializeField] private int id = -2;
    [SerializeField] private string toolName;
    [SerializeField] private Sprite icon;

    public override int GetID() {
        return id;
    }

    public override Sprite GetIcon() {
        return icon;
    }

    public override bool IsSellable() {
        return false;
    }

    public string GetName() {
        return toolName;
    }
}
