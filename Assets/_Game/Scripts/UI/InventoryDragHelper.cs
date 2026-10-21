using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class InventoryDragHelper : Singleton<InventoryDragHelper> {
    [SerializeField] private Image image;

    public void Show(Sprite icon, Vector2 pos) { image.sprite = icon; image.gameObject.SetActive(true); Move(pos); }
    public void Move(Vector2 pos) => image.transform.position = pos;
    public void Hide() => image.gameObject.SetActive(false);
}