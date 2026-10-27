using UnityEngine;

// item nam trong hotbar / backpack: brainrot hoac dung cu (gay...)
// id dung de luu data, -1 la o trong nen id item khong duoc la -1
public abstract class ItemSO : ScriptableObject {
    public abstract int GetID();

    public abstract Sprite GetIcon();

    public abstract bool IsSellable();

    public bool IsMatchID(int id) {
        return GetID() == id;
    }
}
