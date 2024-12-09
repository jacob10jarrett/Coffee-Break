using UnityEngine;
using System.Collections;

[ExecuteInEditMode]
[RequireComponent(typeof(Camera))]
[AddComponentMenu("Image Effects/Pixelate")]
public class Pixelate : MonoBehaviour
{
    public Shader shader;
    Material _material;

    [Range(1, 20)]
    public int basePixelSize = 1; // Base pixel size, relative to a reference resolution.
    
    public int referenceWidth = 1920; // Reference width for scaling the pixel size.
    public int referenceHeight = 1080; // Reference height for scaling the pixel size.

    public bool lockXY = true;

    void OnRenderImage(RenderTexture source, RenderTexture destination)
    {
        if (_material == null) _material = new Material(shader);

        // Calculate pixel size scaling factor based on screen size and reference resolution.
        int scaledPixelSizeX = Mathf.Max(1, (int)(basePixelSize * (float)Screen.width / referenceWidth));
        int scaledPixelSizeY = Mathf.Max(1, (int)(basePixelSize * (float)Screen.height / referenceHeight));

        if (lockXY)
        {
            // Use the same size for X and Y when lockXY is true.
            scaledPixelSizeY = scaledPixelSizeX;
        }

        // Pass scaled pixel size to the shader.
        _material.SetInt("_PixelateX", scaledPixelSizeX);
        _material.SetInt("_PixelateY", scaledPixelSizeY);

        // Render with pixelation effect.
        Graphics.Blit(source, destination, _material);
    }

    void OnDisable()
    {
        DestroyImmediate(_material);
    }
}
