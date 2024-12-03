using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;
using System.Collections;

public class IntroManager : MonoBehaviour
{
    public TextMeshProUGUI gitaIntroText; // Text for Gita's dialogue
    public TextMeshProUGUI finnResponseText; // Text for Finn's dialogue
    public TextMeshProUGUI[] finnDialogueOptions; // Array for Finn's dialogue options (textboxes 0, 1, 2)
    public TextMeshProUGUI ellipsisText; // Text element for the ellipsis animation (should be in Gita's dialogue box)
    public AudioSource optionSoundEffect; // Sound effect for dialogue options appearing
    public AudioSource talkSoundEffect; // Sound effect for talking (typewriter effect)
    public AudioSource navigationSoundEffect; // Sound effect for navigating between options
    public Color highlightedColor = Color.yellow; // Highlighted color for selected option
    public Color defaultColor = Color.white; // Default color for non-selected options
    public string nextSceneName; // Name of the next scene

    [Header("Typewriter Settings")]
    public float typeSpeed = 0.05f; // Speed of the typewriter effect

    private int selectedOption = 0; // Index of the currently selected option
    private bool optionsEnabled = false; // Whether the player can interact with options
    private bool isTyping = false; // To prevent input while typewriter effect is running
    private int dialogueStep = 0; // To track the progression of the dialogue
    private bool familyThingChosen = false; // To track if "family thing" was chosen
    private Coroutine ellipsisCoroutine; // Coroutine for the ellipsis animation

    void Start()
    {
        // Start with Gita's first line
        dialogueStep = 0;

        // Initialize dialogue texts
        if (gitaIntroText != null)
        {
            gitaIntroText.text = "";
        }

        if (finnResponseText != null)
        {
            finnResponseText.text = "";
        }

        if (ellipsisText != null)
        {
            ellipsisText.text = "";
        }

        // Hide all dialogue options initially
        foreach (var option in finnDialogueOptions)
        {
            option.gameObject.SetActive(false);
        }

        // Start the first dialogue
        StartCoroutine(TypewriterEffect(gitaIntroText, "Hey, I'm Gita!", () =>
        {
            // Proceed to the next dialogue step automatically
            dialogueStep++;
            HandleDialogueStep();
        }));
    }

    void Update()
    {
        if (optionsEnabled && !isTyping)
        {
            HandleInput();
        }
    }

    void HandleDialogueStep()
    {
        switch (dialogueStep)
        {
            case 1:
                // Finn's first response options
                DisplayFinnOptions("Hey, it’s nice to meet you!", "It’s great to have another employee!", "Welcome!");
                break;

            case 2:
                // After Finn's response, Gita speaks
                StartCoroutine(TypewriterEffect(gitaIntroText, "Thanks! So, how long have you been working here?", () =>
                {
                    // Proceed to the next dialogue step automatically
                    dialogueStep++;
                    HandleDialogueStep();
                }));
                break;

            case 3:
                // Finn's second response options
                DisplayFinnOptions("Long enough to appreciate good coffee and interesting customers!", "About two years. It’s… a bit of a family thing.");
                break;

            case 4:
                // Check if "family thing" was chosen
                if (familyThingChosen)
                {
                    // Branch 2: Gita asks about family thing
                    StartCoroutine(TypewriterEffect(gitaIntroText, "Family thing?", () =>
                    {
                        // Proceed to the next dialogue step automatically
                        dialogueStep++;
                        HandleDialogueStep();
                    }));
                }
                else
                {
                    // Branch 1: Gita continues
                    StartCoroutine(TypewriterEffect(gitaIntroText, "Oh, that’s cool! So, do you have a favorite part of working here?", () =>
                    {
                        // Proceed to the next dialogue step automatically
                        dialogueStep++;
                        HandleDialogueStep();
                    }));
                }
                break;

            case 5:
                if (familyThingChosen)
                {
                    // Finn explains about his mom
                    StartCoroutine(TypewriterEffect(finnResponseText, "Yeah, my mom used to run this place. She… passed away, and I took it over. I’m trying to keep it going the way she would’ve wanted.", () =>
                    {
                        // Proceed to the next dialogue step automatically
                        dialogueStep++;
                        HandleDialogueStep();
                    }));
                }
                else
                {
                    // Finn's favorite part response options
                    DisplayFinnOptions("Honestly? The coffee. You’ll learn quickly that caffeine is a survival skill here.", "Probably the peace of it. There’s something calming about starting the day here before it gets busy.");
                }
                break;

            case 6:
                if (familyThingChosen)
                {
                    // Gita's response
                    StartCoroutine(TypewriterEffect(gitaIntroText, "Wow, that’s a lot to take on. She must’ve really loved this place.", () =>
                    {
                        // Proceed to the next dialogue step automatically
                        dialogueStep++;
                        HandleDialogueStep();
                    }));
                }
                else
                {
                    // Gita comments on coziness
                    StartCoroutine(TypewriterEffect(gitaIntroText, "That’s great. I’ve always loved how coffee shops feel… cozy, you know?", () =>
                    {
                        // Proceed to the next dialogue step automatically
                        dialogueStep++;
                        HandleDialogueStep();
                    }));
                }
                break;

            case 7:
                if (familyThingChosen)
                {
                    // Finn's response options (only 2 options)
                    DisplayFinnOptions("She did. It was her dream to have a little spot where everyone felt welcome. I just hope I’m doing it justice.", "Yeah, it’s a lot sometimes, but I think she’d be happy to see it still running.");
                }
                else
                {
                    // Finn concludes
                    StartCoroutine(TypewriterEffect(finnResponseText, "Exactly. It’s like its own little world. Let’s get you started—I’ll show you the coffee machine. It’s pretty much the heart of the shop!", () =>
                    {
                        // Proceed to the next dialogue step automatically
                        dialogueStep++;
                        HandleDialogueStep();
                    }));
                }
                break;

            case 8:
                if (familyThingChosen)
                {
                    // Finn wraps up
                    StartCoroutine(TypewriterEffect(finnResponseText, "But enough about me—let’s get you started. The coffee machine’s over here. It’s pretty much the heart of the shop!", () =>
                    {
                        // Start ellipsis animation for Gita after Finn's last dialogue
                        StartCoroutine(ShowEllipsisAndProceed());
                    }));
                }
                else
                {
                    // End of Branch 1
                    Invoke(nameof(LoadNextScene), 2f);
                }
                break;

            case 9:
                // End of Branch 2
                Invoke(nameof(LoadNextScene), 2f);
                break;

            default:
                // Should not reach here
                Debug.LogError("Unhandled dialogue step: " + dialogueStep);
                break;
        }
    }

    IEnumerator ShowEllipsisAndProceed()
    {
        // Start the ellipsis animation in Gita's dialogue box
        StartEllipsisAnimation();

        // Wait for a specified duration (e.g., 2 seconds)
        yield return new WaitForSeconds(2f);

        // Stop the ellipsis animation
        StopEllipsisAnimation();

        // Proceed to the next scene
        LoadNextScene();
    }

    void DisplayFinnOptions(params string[] options)
    {
        // Clear Finn's previous dialogue text
        finnResponseText.text = "";

        // Stop ellipsis animation when options are displayed
        StopEllipsisAnimation();

        // Hide all Finn's dialogue options first
        foreach (var option in finnDialogueOptions)
        {
            option.gameObject.SetActive(false);
        }

        // Determine indices for displaying options
        int[] optionIndices;
        if (options.Length == 2)
        {
            // When only two options, display them in positions 0 and 2
            optionIndices = new int[] { 0, 2 };
        }
        else
        {
            // Use positions 0, 1, 2
            optionIndices = new int[] { 0, 1, 2 };
        }

        // Display given options
        for (int i = 0; i < options.Length; i++)
        {
            if (i < optionIndices.Length && optionIndices[i] < finnDialogueOptions.Length)
            {
                int index = optionIndices[i];
                finnDialogueOptions[index].text = options[i];
                finnDialogueOptions[index].gameObject.SetActive(true);

                if (optionSoundEffect != null)
                {
                    optionSoundEffect.Play(); // Play sound effect for each dialogue option appearing
                }

                UpdateOptionStyle(index); // Set default style
            }
            else
            {
                Debug.LogWarning("Not enough Finn dialogue option UI elements assigned in the Inspector.");
            }
        }

        // Set selectedOption to the first active option's index
        selectedOption = optionIndices[0];
        UpdateOptionStyles();

        optionsEnabled = true;
    }

    void HandleInput()
    {
        // Navigate options with arrow keys
        bool moved = false;

        int[] activeOptionIndices = GetActiveFinnOptionIndices();

        if (Input.GetKeyDown(KeyCode.UpArrow))
        {
            int currentIndex = System.Array.IndexOf(activeOptionIndices, selectedOption);
            currentIndex = (currentIndex - 1 + activeOptionIndices.Length) % activeOptionIndices.Length;
            selectedOption = activeOptionIndices[currentIndex];
            moved = true;
        }
        else if (Input.GetKeyDown(KeyCode.DownArrow))
        {
            int currentIndex = System.Array.IndexOf(activeOptionIndices, selectedOption);
            currentIndex = (currentIndex + 1) % activeOptionIndices.Length;
            selectedOption = activeOptionIndices[currentIndex];
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

    int[] GetActiveFinnOptionIndices()
    {
        System.Collections.Generic.List<int> indices = new System.Collections.Generic.List<int>();
        for (int i = 0; i < finnDialogueOptions.Length; i++)
        {
            if (finnDialogueOptions[i].gameObject.activeSelf)
            {
                indices.Add(i);
            }
        }
        return indices.ToArray();
    }

    void UpdateOptionStyles()
    {
        for (int i = 0; i < finnDialogueOptions.Length; i++)
        {
            UpdateOptionStyle(i);
        }
    }

    void UpdateOptionStyle(int index)
    {
        if (finnDialogueOptions[index].gameObject.activeSelf)
        {
            if (index == selectedOption)
            {
                finnDialogueOptions[index].color = highlightedColor; // Highlight selected option
                finnDialogueOptions[index].fontStyle = FontStyles.Bold;
            }
            else
            {
                finnDialogueOptions[index].color = defaultColor; // Reset non-selected options
                finnDialogueOptions[index].fontStyle = FontStyles.Normal;
            }
        }
    }

    void HandleSelection(int optionIndex)
    {
        string chosenDialogue = finnDialogueOptions[optionIndex].text;

        // Disable options while typing
        optionsEnabled = false;

        // Hide Finn's dialogue options
        foreach (var option in finnDialogueOptions)
        {
            option.gameObject.SetActive(false);
        }

        // Handle different dialogue steps
        if (dialogueStep == 1)
        {
            // Finn's first response
            StartCoroutine(TypewriterEffect(finnResponseText, chosenDialogue, () =>
            {
                // Proceed to the next dialogue step automatically
                dialogueStep++;
                HandleDialogueStep();
            }));
        }
        else if (dialogueStep == 3)
        {
            // Finn's second response
            if (chosenDialogue.Contains("family thing"))
            {
                familyThingChosen = true;
            }
            else
            {
                familyThingChosen = false;
            }

            StartCoroutine(TypewriterEffect(finnResponseText, chosenDialogue, () =>
            {
                // Proceed to the next dialogue step automatically
                dialogueStep++;
                HandleDialogueStep();
            }));
        }
        else if (dialogueStep == 5)
        {
            if (familyThingChosen)
            {
                // Finn just explained about his mom, proceed to Gita's response
                dialogueStep++;
                HandleDialogueStep();
            }
            else
            {
                // Finn's favorite part response
                StartCoroutine(TypewriterEffect(finnResponseText, chosenDialogue, () =>
                {
                    // Proceed to the next dialogue step automatically
                    dialogueStep++;
                    HandleDialogueStep();
                }));
            }
        }
        else if (dialogueStep == 7 && familyThingChosen)
        {
            // Finn's response in Branch 2
            StartCoroutine(TypewriterEffect(finnResponseText, chosenDialogue, () =>
            {
                // Proceed to the next dialogue step automatically
                dialogueStep++;
                HandleDialogueStep();
            }));
        }
        else
        {
            // Should not reach here
            Debug.LogError("Unhandled selection at dialogue step: " + dialogueStep);
        }
    }

    System.Collections.IEnumerator TypewriterEffect(TextMeshProUGUI textMeshPro, string message, System.Action onComplete = null)
    {
        // Stop ellipsis animation when someone is speaking
        StopEllipsisAnimation();

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

            yield return new WaitForSeconds(typeSpeed); // Typewriter speed
        }

        isTyping = false;
        onComplete?.Invoke(); // Call the optional completion callback
    }

    void StartEllipsisAnimation()
    {
        if (ellipsisCoroutine == null)
        {
            ellipsisCoroutine = StartCoroutine(EllipsisAnimation());
        }
    }

    void StopEllipsisAnimation()
    {
        if (ellipsisCoroutine != null)
        {
            StopCoroutine(ellipsisCoroutine);
            ellipsisCoroutine = null;
            if (ellipsisText != null)
            {
                ellipsisText.text = "";
            }
        }
    }

    System.Collections.IEnumerator EllipsisAnimation()
    {
        if (ellipsisText == null)
        {
            Debug.LogError("Ellipsis Text is not assigned in the Inspector.");
            yield break;
        }

        string[] ellipsisStates = { "", ".", "..", "..." };
        int index = 0;

        while (true)
        {
            ellipsisText.text = ellipsisStates[index];
            index = (index + 1) % ellipsisStates.Length;
            yield return new WaitForSeconds(0.5f);
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

    void OnDestroy()
    {
        // Ensure the coroutine is stopped when the object is destroyed
        if (ellipsisCoroutine != null)
        {
            StopCoroutine(ellipsisCoroutine);
        }
    }
}
