using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Tile : MonoBehaviour
{
    public SpriteRenderer spriteRenderer;
    public SpriteRenderer backgroundSpriteRenderer;
    public int tileId;
    private bool isSelected = false;
    public Vector2Int gridPosition { get; set; }

    [Header("Tile Colors")]
    public Color defaultBackgroundColor = Color.white;
    public Color selectedColor = Color.green;
    public Color highlightColor = Color.blue;

    public void SetTile(int id, Sprite sprite)
    {
        tileId = id;
        spriteRenderer.sprite = sprite;
    }

    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        // Tìm GameObject "Background" trong các con của Tile
        Transform backgroundTransform = transform.Find("Background");
        if (backgroundTransform != null)
        {
            backgroundSpriteRenderer = backgroundTransform.GetComponent<SpriteRenderer>();
            backgroundSpriteRenderer.color = defaultBackgroundColor;
        }
    }

    public void SelectTile()
    {
        isSelected = !isSelected;
        backgroundSpriteRenderer.color = isSelected ? selectedColor : defaultBackgroundColor;
    }

    public void HideTile()
    {
        this.gameObject.SetActive(false);
    }

    public void HighlightTile()
    {
        if (backgroundSpriteRenderer != null)
        {
            backgroundSpriteRenderer.color = highlightColor;
        }
    }

    public void ResetHighlight()
    {
        if (backgroundSpriteRenderer != null)
        {
            backgroundSpriteRenderer.color = defaultBackgroundColor;
        }
    }
}
