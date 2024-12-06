using UnityEngine;
using TMPro;

public class FlashingColor : MonoBehaviour
{
    public Color defaultColor = new Color(0.71f, 0.53f, 0.55f); // #B5888D in RGB
    public Color flashColor = Color.white; // The brighter color to flash
    public float flashSpeed = 2f; // Speed of flashing

    private SpriteRenderer spriteRenderer;
    private TextMeshPro textMeshPro;
    private bool isFlashing = false;

    void Start()
    {
        // Try to get a SpriteRenderer for sprites
        spriteRenderer = GetComponent<SpriteRenderer>();

        // Try to get a TextMeshPro component for UI or 3D text
        textMeshPro = GetComponent<TextMeshPro>();

        // Set the initial color to the default color
        if (spriteRenderer != null)
        {
            spriteRenderer.color = defaultColor;
        }
        else if (textMeshPro != null)
        {
            textMeshPro.color = defaultColor;
        }
        else
        {
            Debug.LogError("No SpriteRenderer or TextMeshPro found on this GameObject!");
        }
    }

    void Update()
    {
        if (isFlashing)
        {
            // Calculate flashing effect
            float t = Mathf.PingPong(Time.time * flashSpeed, 1f);

            // Apply to SpriteRenderer or TextMeshPro
            if (spriteRenderer != null)
            {
                spriteRenderer.color = Color.Lerp(defaultColor, flashColor, t);
            }
            else if (textMeshPro != null)
            {
                textMeshPro.color = Color.Lerp(defaultColor, flashColor, t);
            }
        }
    }

    public void StartFlashing()
    {
        isFlashing = true;
    }

    public void StopFlashing()
    {
        isFlashing = false;

        // Reset to the default color
        if (spriteRenderer != null)
        {
            spriteRenderer.color = defaultColor;
        }
        else if (textMeshPro != null)
        {
            textMeshPro.color = defaultColor;
        }
    }
}
