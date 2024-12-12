using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;
using System.Collections;

public class AIDialogueManager : MonoBehaviour
{
    [Header("UI References")]
    public TextMeshProUGUI npcText;      // Gita's dialogue
    public TextMeshProUGUI finnText;     // Finn's dialogue (if needed for other dialogues)
    public TextMeshProUGUI[] playerOptions; 
    public TextMeshProUGUI orderText;

    [Header("Audio")]
    public AudioSource audioSource;
    public AudioClip talkSoundEffect;   
    public AudioClip correctSFX;
    public AudioClip encourageSFX;
    public AudioClip orderCompleteSFX;
    public AudioClip navigationSoundEffect;

    [Header("Typewriter Settings")]
    public float typeSpeed = 0.05f;

    private bool isTyping = false;
    private bool optionsEnabled = false;
    private int selectedOptionIndex = 0;
    private int dialogueStep = 0;

    private bool familyMentioned;
    private int mistakesCount;

    private enum PerformanceScenario { Perfect, NearPerfect, ManyMistakes }

    private PerformanceScenario scenario;

    void Start()
    {
        // Initialize dialogue parameters from AIDialogueScript
        familyMentioned = AIDialogueScript.playerMentionedFamily;
        mistakesCount = AIDialogueScript.playerMadeMistakesCount;

        // Clear existing texts
        npcText.text = "";
        finnText.text = "";
        foreach (var opt in playerOptions) opt.gameObject.SetActive(false);

        // Determine the current performance scenario based on mistakes count
        if (mistakesCount == 0)
        {
            scenario = PerformanceScenario.Perfect;
        }
        else if (mistakesCount <= 2)
        {
            scenario = PerformanceScenario.NearPerfect;
        }
        else
        {
            scenario = PerformanceScenario.ManyMistakes;
        }

        // Begin the dialogue sequence
        StartDialogue();
    }

    void Update()
    {
        if (optionsEnabled && !isTyping)
        {
            HandleInput();
        }
    }

    void StartDialogue()
    {
        if (scenario == PerformanceScenario.Perfect || scenario == PerformanceScenario.NearPerfect)
        {
            string line = scenario == PerformanceScenario.Perfect 
                ? "That was absolutely incredible, Finn! You didn’t miss a beat."
                : "That was really impressive. You were so close to perfect!";

            if (familyMentioned)
            {
                line += scenario == PerformanceScenario.Perfect 
                    ? " I know without your family it can be hard, but you’re handling it well."
                    : " Your family would be proud of you.";
            }

            StartCoroutine(TypewriterEffect(npcText, line, () =>
            {
                dialogueStep = 1; // Proceed to player options
                HandleDialogueStep();
            }));
        }
        else
        {
            StartCoroutine(TypewriterEffect(npcText, "Haha, I noticed you stumbled along the way. Are you nervous?", () =>
            {
                dialogueStep = 1;
                HandleDialogueStep();
            }));
        }
    }

    void HandleDialogueStep()
    {
        if (scenario == PerformanceScenario.Perfect || scenario == PerformanceScenario.NearPerfect)
        {
            switch (dialogueStep)
            {
                case 1:
                    // Show response options directly after Gita's dialogue
                    DisplayFinnFirstResponseOptions();
                    break;
                case 2:
                    // After Finn chooses, Gita responds
                    ShowFinalLineForPerfectOrNearPerfect();
                    break;
            }
        }
        else // ManyMistakes
        {
            switch (dialogueStep)
            {
                case 1:
                    // After Gita speaks, display options
                    DisplayFinnFirstResponseOptions();
                    break;
                case 2:
                    // After Finn chooses, Gita responds
                    // (Handled in HandleSelection)
                    break;
                case 3:
                    ShowAdditionalDialogueAfterGitaForManyMistakes();
                    break;
                case 4:
                    // Finn says "Right behind you!"
                    ShowFinnRightBehindYou();
                    break;
            }
        }
    }

    void ShowAdditionalDialogueAfterGitaForManyMistakes()
    {
        StartCoroutine(TypewriterEffect(npcText, "Don’t worry, Finn. We all have off days. Let’s get back out there and make the next batch of orders even better! I think I'm getting the hang of this now.", () =>
        {
            dialogueStep = 4;
            HandleDialogueStep();
        }));
    }

    void ShowFinnRightBehindYou()
    {
        // Gita displays "..."
        StartCoroutine(TypewriterEffect(npcText, "...", () =>
        {
            // Finn responds with "Right behind you!"
            StartCoroutine(TypewriterEffect(finnText, "Right behind you!", () =>
            {
                // Transition back to MakingOrder scene with scenario 2
                AIDialogueScript.drinkScenario = 2; 
                Invoke(nameof(LoadMakingOrdersScene), 2f);
            }));
        }));
    }

    void ShowFinalLineForPerfectOrNearPerfect()
    {
        // Finn suggests making drinks together
        StartCoroutine(TypewriterEffect(npcText, "Let’s head back and make some drinks together!", () =>
        {
            // Transition back to MakingOrder scene with scenario 2
            AIDialogueScript.drinkScenario = 2; 
            Invoke(nameof(LoadMakingOrdersScene), 2f);
        }));
    }

    void DisplayFinnFirstResponseOptions()
    {
        if (scenario == PerformanceScenario.ManyMistakes)
        {
            // Two options:
            // "I probably just need some coffee myself."
            // "Definitely not!"
            DisplayPlayerOptions(
                "I probably just need some coffee myself.",
                "Definitely not!"
            );
        }
        else
        {
            // Perfect/Near-Perfect scenario:
            // Finn could ask if Gita feels ready or wants to try making drinks together now.
            DisplayPlayerOptions(
                "Haha, thank you! Do you feel ready to handle a few orders yourself?",
                "Thanks Gita! How 'bout we try making some drinks together now?"
            );
        }
    }

    void HandleInput()
    {
        bool moved = false;
        int activeCount = 0;
        foreach (var opt in playerOptions)
            if (opt.gameObject.activeSelf) activeCount++;

        if (activeCount <= 1) return;

        if (Input.GetKeyDown(KeyCode.UpArrow))
        {
            selectedOptionIndex = (selectedOptionIndex - 1 + activeCount) % activeCount;
            moved = true;
        }
        else if (Input.GetKeyDown(KeyCode.DownArrow))
        {
            selectedOptionIndex = (selectedOptionIndex + 1) % activeCount;
            moved = true;
        }

        if (moved && navigationSoundEffect != null && audioSource != null)
        {
            audioSource.PlayOneShot(navigationSoundEffect);
        }

        if (moved) UpdateOptionStyles();

        if (Input.GetKeyDown(KeyCode.Return))
        {
            HandleSelection(selectedOptionIndex);
        }
    }

    void UpdateOptionStyles()
    {
        int activeIndex = 0;
        for (int i = 0; i < playerOptions.Length; i++)
        {
            if (playerOptions[i].gameObject.activeSelf)
            {
                playerOptions[i].color = (activeIndex == selectedOptionIndex) ? Color.yellow : Color.white;
                playerOptions[i].fontStyle = (activeIndex == selectedOptionIndex) ? FontStyles.Bold : FontStyles.Normal;
                activeIndex++;
            }
        }
    }

    void HandleSelection(int optionIndex)
    {
        optionsEnabled = false;
        string chosenText = "";
        int activeIndex = 0;
        for (int i = 0; i < playerOptions.Length; i++)
        {
            if (playerOptions[i].gameObject.activeSelf)
            {
                if (activeIndex == optionIndex)
                {
                    chosenText = playerOptions[i].text;
                }
                playerOptions[i].gameObject.SetActive(false);
                activeIndex++;
            }
        }

        if (scenario == PerformanceScenario.ManyMistakes)
        {
            if (chosenText.Contains("coffee myself"))
            {
                if (familyMentioned)
                {
                    StartCoroutine(TypewriterEffect(npcText, "Haha, maybe your family’s coffee-making skills are finally wearing off on you!", () =>
                    {
                        dialogueStep = 3; 
                        HandleDialogueStep();
                    }));
                }
                else
                {
                    StartCoroutine(TypewriterEffect(npcText, "That’s ironic, considering we’re in a coffee shop! (laughs)", () =>
                    {
                        dialogueStep = 3; 
                        HandleDialogueStep();
                    }));
                }
            }
            else
            {
                // Chosen "Definitely not!"
                if (familyMentioned)
                {
                    StartCoroutine(TypewriterEffect(npcText, "That’s the spirit! With your family’s experience, I wouldn’t expect anything less.", () =>
                    {
                        dialogueStep = 3; 
                        HandleDialogueStep();
                    }));
                }
                else
                {
                    StartCoroutine(TypewriterEffect(npcText, "I admire the confidence! Let’s channel that into the next batch of orders. I think I'm getting the hang of this now.", () =>
                    {
                        dialogueStep = 3; 
                        HandleDialogueStep();
                    }));
                }
            }
        }
        else
        {
            // Perfect/Near-Perfect scenario:
            string response;
            if (scenario == PerformanceScenario.Perfect)
            {
                response = "After watching you, Finn, I feel much more prepared. I’m feeling ready to give it a shot!";
            }
            else
            {
                response = "Seeing you so close to perfect really boosts my confidence, Finn. I’m feeling ready to give it a shot!";
            }

            if (familyMentioned)
            {
                response += " And I’m sure your family would be proud.";
            }

            StartCoroutine(TypewriterEffect(npcText, response, () =>
            {
                // After Gita responds, proceed to final line
                dialogueStep = 2;
                HandleDialogueStep();
            }));
        }
    }

    void DisplayPlayerOptions(params string[] options)
    {
        foreach (var opt in playerOptions) opt.gameObject.SetActive(false);

        int[] optionIndices;
        if (options.Length == 2)
        {
            optionIndices = new int[] { 0, 2 };
        }
        else
        {
            optionIndices = new int[] { 0, 1, 2 };
        }

        for (int i = 0; i < options.Length; i++)
        {
            if (i < optionIndices.Length && optionIndices[i] < playerOptions.Length)
            {
                int index = optionIndices[i];
                playerOptions[index].text = options[i];
                playerOptions[index].gameObject.SetActive(true);
            }
        }

        selectedOptionIndex = 0;
        UpdateOptionStyles();
        optionsEnabled = true;
    }

    IEnumerator TypewriterEffect(TextMeshProUGUI textMeshPro, string message, System.Action onComplete = null)
    {
        isTyping = true;
        textMeshPro.text = "";

        for (int i = 0; i <= message.Length; i++)
        {
            textMeshPro.text = message.Substring(0, i);
            if (talkSoundEffect != null && i % 2 == 0 && audioSource != null)
            {
                audioSource.PlayOneShot(talkSoundEffect);
            }
            yield return new WaitForSeconds(typeSpeed);
        }

        isTyping = false;
        onComplete?.Invoke();
    }

    void PlaySFX(AudioClip clip)
    {
        if (audioSource != null && clip != null)
        {
            audioSource.PlayOneShot(clip);
        }
    }

    void LoadMakingOrdersScene()
    {
        SceneManager.LoadScene("MakingOrder");
    }
}
