using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System.Collections; // Required for IEnumerator

#if UNITY_EDITOR
using UnityEditor;
#endif

public class MenuController : MonoBehaviour
{
    public Pixelate pixelateEffect; // Reference to the Pixelate script on the camera
    public Image fadeOverlay; // Reference to a UI Image for fade-out effect
    public float transitionDuration = 1f; // Duration of the fade-out effect

    // Function called when the Play button is pressed
    public void PlayGame()
    {
        StartCoroutine(TransitionToGame());
    }

    // Function called when the Quit button is pressed
    public void QuitGame()
    {
        Debug.Log("Quit Game");
    #if UNITY_EDITOR
        EditorApplication.isPlaying = false; // Stop play mode in the editor
    #else
        Application.Quit(); // Quit the application in a build
    #endif
    }

    private IEnumerator TransitionToGame()
    {
        float elapsedTime = 0f;

        // Gradually fade to black and increase pixelation
        while (elapsedTime < transitionDuration)
        {
            elapsedTime += Time.deltaTime;
            float t = elapsedTime / transitionDuration;

            // Smoothly interpolate the base pixel size
            if (pixelateEffect != null)
            {
                pixelateEffect.basePixelSize = (int)Mathf.Lerp(8, 20, t);
            }

            // Smoothly fade the screen to black
            if (fadeOverlay != null)
            {
                Color color = fadeOverlay.color;
                color.a = Mathf.Lerp(0, 1, t); // Interpolate alpha from 0 to 1
                fadeOverlay.color = color;
            }

            yield return null;
        }

        // Ensure the pixelation effect and fade-out are fully applied
        if (pixelateEffect != null)
        {
            pixelateEffect.basePixelSize = 20;
        }

        if (fadeOverlay != null)
        {
            Color color = fadeOverlay.color;
            color.a = 1;
            fadeOverlay.color = color;
        }

        // Load the next scene
        SceneManager.LoadScene("Intro");
    }
}
