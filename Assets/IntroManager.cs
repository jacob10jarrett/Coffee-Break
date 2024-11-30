using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

public class IntroManager : MonoBehaviour
{
    public TextMeshProUGUI gitaIntroText; // Text for Gita's introduction
    public TextMeshProUGUI finnResponseText; // Text for Finn's response
    public TextMeshProUGUI[] finnResponses; // Array of TextMeshPro for Finn's dialogue options
    public AudioSource optionSoundEffect; // Sound effect for dialogue options appearing
    public AudioSource talkSoundEffect; // Sound effect for talking (typewriter effect)
    public AudioSource navigationSoundEffect; // Sound effect for navigating between options
    public Color highlightedColor = Color.yellow; // Highlighted color for selected option
    public Color defaultColor = Color.white; // Default color for non-selected options
    public string nextSceneName; // Name of the next scene

    private int selectedOption = 0; // Index of the currently selected option
    private bool optionsEnabled = false; // Whether the player can interact with options
    private bool isTyping = false; // To prevent input while typewriter effect is running

    void Start()
    {
        // Start with Gita's typewriter effect
        if (gitaIntroText != null)
        {
            StartCoroutine(TypewriterEffect(gitaIntroText, "Hey, I'm Gita!", () =>
            {
                // Show Finn's responses after Gita's typewriter effect finishes
                StartCoroutine(DisplayDialogueOptions());
            }));
        }

        // Initialize Finn's responses
        foreach (var option in finnResponses)
        {
            option.gameObject.SetActive(false); // Hide all options initially
        }

        // Ensure Finn's response text starts empty
        if (finnResponseText != null)
        {
            finnResponseText.text = "";
        }
    }

    void Update()
    {
        if (optionsEnabled && !isTyping)
        {
            HandleInput();
        }
    }

    System.Collections.IEnumerator DisplayDialogueOptions()
    {
        for (int i = 0; i < finnResponses.Length; i++)
        {
            finnResponses[i].gameObject.SetActive(true);
            if (optionSoundEffect != null)
            {
                optionSoundEffect.Play(); // Play sound effect for each dialogue option appearing
            }
            UpdateOptionStyle(i); // Set default style
            yield return new WaitForSeconds(0.5f); // Delay between options appearing
        }

        optionsEnabled = true; // Enable player input after all options are displayed
        UpdateOptionStyles(); // Highlight the default option
    }

    void HandleInput()
    {
        // Navigate options with arrow keys
        bool moved = false;

        if (Input.GetKeyDown(KeyCode.UpArrow))
        {
            selectedOption = (selectedOption - 1 + finnResponses.Length) % finnResponses.Length;
            moved = true;
        }
        else if (Input.GetKeyDown(KeyCode.DownArrow))
        {
            selectedOption = (selectedOption + 1) % finnResponses.Length;
            moved = true;
        }

        if (moved)
        {
            if (navigationSoundEffect != null)
            {
                navigationSoundEffect.Play(); // Play navigation sound effect
            }
            UpdateOptionStyles();
        }

        // Confirm selection with Enter
        if (Input.GetKeyDown(KeyCode.Return))
        {
            HandleSelection(selectedOption);
        }
    }

    void UpdateOptionStyles()
    {
        for (int i = 0; i < finnResponses.Length; i++)
        {
            UpdateOptionStyle(i);
        }
    }

    void UpdateOptionStyle(int index)
    {
        if (index == selectedOption)
        {
            finnResponses[index].color = highlightedColor; // Highlight selected option
            finnResponses[index].fontStyle = FontStyles.Bold;
        }
        else
        {
            finnResponses[index].color = defaultColor; // Reset non-selected options
            finnResponses[index].fontStyle = FontStyles.Normal;
        }
    }

    void HandleSelection(int optionIndex)
    {
        string chosenDialogue = finnResponses[optionIndex].text;

        // Disable options while typing
        optionsEnabled = false;

        // Hide dialogue options
        foreach (var option in finnResponses)
        {
            option.gameObject.SetActive(false);
        }

        // Start typewriter effect for Finn's response
        StartCoroutine(TypewriterEffect(finnResponseText, chosenDialogue, () =>
        {
            // Transition to the next scene after a delay
            Invoke(nameof(LoadNextScene), 2f);
        }));
    }

    System.Collections.IEnumerator TypewriterEffect(TextMeshProUGUI textMeshPro, string message, System.Action onComplete = null)
    {
        isTyping = true;

        textMeshPro.text = ""; // Clear the text initially

        for (int i = 0; i <= message.Length; i++)
        {
            textMeshPro.text = message.Substring(0, i);

            // Play talking sound effect if assigned
            if (talkSoundEffect != null && i % 2 == 0)
            {
                talkSoundEffect.Play();
            }

            yield return new WaitForSeconds(0.05f); // Typewriter speed
        }

        isTyping = false;
        onComplete?.Invoke(); // Call the optional completion callback
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
