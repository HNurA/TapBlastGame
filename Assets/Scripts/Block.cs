// Block.cs
using UnityEngine;

public class Block : MonoBehaviour
{
    public int colorID;
    private Sprite[] icons;
    private SpriteRenderer spriteRenderer;
    private BoardManager boardManager;

    private void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        boardManager = FindObjectOfType<BoardManager>();
    }

    public void SetupBlock(int newColorID, Sprite[] colorSprites)
    {
        if (colorSprites == null || colorSprites.Length < 4)
        {
            Debug.LogError("Color sprites array is null or doesn't have enough sprites!");
            return;
        }

        colorID = newColorID;
        icons = new Sprite[4];
        icons = colorSprites;
        
        if (spriteRenderer == null)
        {
            spriteRenderer = GetComponent<SpriteRenderer>();
        }
        
        if (icons[0] != null)
        {
            spriteRenderer.sprite = icons[0];
        }
        else
        {
            Debug.LogError("Default sprite (icons[0]) is null!");
        }
    }

    private void OnMouseDown()
    {
        boardManager.OnBlockClicked(this);
    }

    public void UpdateAppearance(int groupSize)
    {
        if (icons == null || spriteRenderer == null) return;

        if (groupSize >= 10 && icons[3] != null)
            spriteRenderer.sprite = icons[3];
        else if (groupSize >= 8 && icons[2] != null)
            spriteRenderer.sprite = icons[2];
        else if (groupSize >= 5 && icons[1] != null)
            spriteRenderer.sprite = icons[1];
        else if (icons[0] != null)
            spriteRenderer.sprite = icons[0];
    }
}