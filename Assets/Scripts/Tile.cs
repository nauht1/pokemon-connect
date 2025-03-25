using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Tile : MonoBehaviour
{
    public SpriteRenderer spriteRenderer;
    public int tileId;
    private bool isSelected = false;
    public Vector2Int gridPosition { get; set; }

    public void SetTile(int id, Sprite sprite)
    {
        tileId = id;
        spriteRenderer.sprite = sprite;
    }

    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    public void SelectTile()
    {
        isSelected = !isSelected;
        spriteRenderer.color = isSelected ? Color.green : Color.white;
    }

    public void HideTile()
    {
        this.gameObject.SetActive(false);
    }
}
