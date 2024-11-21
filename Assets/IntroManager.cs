using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

public class IntroManager : MonoBehaviour
{
    public TextMeshProUGUI introText; // Reference to the TextMeshPro UI element
    public AudioSource introSound;   // AudioSource to play the sound
    public float textDisplayDuration = 5f; // Duration to show the text before transitioning
    public string nextSceneName;     // Name of the next scene to load

    private float timer;

    void Start()
    {
        // Ensure the text starts empty and play the sound
        if (introText != null)
        {
            introText.text = "";
        }

        if (introSound != null)
        {
            introSound.Play();
        }

        // Start displaying the intro text
        StartCoroutine(DisplayIntroText());
    }

    void Update()
    {
        // Keep track of time to transition to the next scene
        timer += Time.deltaTime;
        if (timer >= textDisplayDuration)
        {
            LoadNextScene();
        }
    }

    System.Collections.IEnumerator DisplayIntroText()
    {
        // Fade in the text
        if (introText != null)
        {
            string message = "In a city that never stops, this little coffee shop feels like the one place where time slows down.";
            for (int i = 0; i <= message.Length; i++)
            {
                introText.text = message.Substring(0, i);
                yield return new WaitForSeconds(0.05f); // Typewriter effect
            }
        }
    }

    void LoadNextScene()
    {
        if (!string.IsNullOrEmpty(nextSceneName))
        {
            SceneManager.LoadScene(nextSceneName);
        }
        else
        {
            Debug.LogError("Next scene name not set in the Inspector.");
        }
    }
}
