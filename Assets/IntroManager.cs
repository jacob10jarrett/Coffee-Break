using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;
using System.Collections;

public class IntroManager : MonoBehaviour
{
    public TextMeshProUGUI gitaIntroText; 
    public TextMeshProUGUI finnResponseText;
    public TextMeshProUGUI[] finnDialogueOptions;
    public TextMeshProUGUI ellipsisText; 
    public AudioSource optionSoundEffect;
    public AudioSource talkSoundEffect;
    public AudioSource navigationSoundEffect;
    public Color highlightedColor = Color.yellow;
    public Color defaultColor = Color.white;
    public string nextSceneName; // minigame scene name

    [Header("Typewriter Settings")]
    public float typeSpeed = 0.05f;

    private int selectedOption = 0;
    private bool optionsEnabled = false;
    private bool isTyping = false;
    private int dialogueStep = 0;
    private bool familyThingChosen = false;

    private Coroutine ellipsisCoroutine;

    void Start()
    {
        dialogueStep = 0;
        if (gitaIntroText != null) gitaIntroText.text = "";
        if (finnResponseText != null) finnResponseText.text = "";
        if (ellipsisText != null) ellipsisText.text = "";
        foreach (var option in finnDialogueOptions) option.gameObject.SetActive(false);

        StartCoroutine(TypewriterEffect(gitaIntroText, "Hi! I'm Gita—excited to be here!", () =>
        {
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
                DisplayFinnOptions("Hey, it’s nice to meet you!", "It’s great to have another employee!", "Great to have you here! You'll fit right in.");
                break;
            case 2:
                StartCoroutine(TypewriterEffect(gitaIntroText, "Thanks! So, how long have you been keeping this place running?", () =>
                {
                    dialogueStep++;
                    HandleDialogueStep();
                }));
                break;
            case 3:
                DisplayFinnOptions("Long enough to appreciate good coffee and interesting customers!", 
                                   "About two years. It’s… a bit of a family thing.");
                break;
            case 4:
                if (familyThingChosen)
                {
                    StartCoroutine(TypewriterEffect(gitaIntroText, "Family thing?", () =>
                    {
                        dialogueStep++;
                        HandleDialogueStep();
                    }));
                }
                else
                {
                    StartCoroutine(TypewriterEffect(gitaIntroText, "Oh, that’s cool! So, do you have a favorite part of working here?", () =>
                    {
                        dialogueStep++;
                        HandleDialogueStep();
                    }));
                }
                break;
            case 5:
                if (familyThingChosen)
                {
                    StartCoroutine(TypewriterEffect(finnResponseText, "Yeah, my family’s been running this place for years. I took it over recently and I’m doing my best to keep the tradition alive.", () =>
                    {
                        dialogueStep++;
                        HandleDialogueStep();
                    }));
                }
                else
                {
                    DisplayFinnOptions("Honestly? The coffee. You’ll learn quickly that caffeine is a survival skill here.", 
                                       "Probably the peace of it. There’s something calming about starting the day here before it gets busy.");
                }
                break;
            case 6:
                if (familyThingChosen)
                {
                    StartCoroutine(TypewriterEffect(gitaIntroText, "That’s really nice. It must be special to keep a family tradition going.", () =>
                    {
                        dialogueStep++;
                        HandleDialogueStep();
                    }));
                }
                else
                {
                    StartCoroutine(TypewriterEffect(gitaIntroText, "That’s great. I’ve always loved how coffee shops feel… cozy, you know?", () =>
                    {
                        dialogueStep++;
                        HandleDialogueStep();
                    }));
                }
                break;
            case 7:
                if (familyThingChosen)
                {
                    DisplayFinnOptions("Yeah, it’s a lot sometimes, but it’s worth it to see people enjoy this place.", 
                                       "It’s always been about creating a welcoming space. I just hope I’m doing it right.");
                }
                else
                {
                    StartCoroutine(TypewriterEffect(finnResponseText, "Exactly. It’s like its own little world. Let’s get started—I’ll show you the coffee machine. It’s pretty much the heart of the shop!", () =>
                    {
                        dialogueStep++;
                        HandleDialogueStep();
                    }));
                }
                break;
            case 8:
                if (familyThingChosen)
                {
                    StartCoroutine(TypewriterEffect(finnResponseText, "Alright, enough about me! Let’s get brewing. The coffee machine’s the heart of the shop—let’s keep it beating!", () =>
                    {
                        StartCoroutine(ShowEllipsisAndProceed());
                    }));
                }
                else
                {
                    SetAIDialogueScriptValuesFromIntro();
                    Invoke(nameof(LoadNextScene), 2f);
                }
                break;
            case 9:
                SetAIDialogueScriptValuesFromIntro();
                Invoke(nameof(LoadNextScene), 2f);
                break;
            default:
                Debug.LogError("Unhandled dialogue step: " + dialogueStep);
                break;
        }
    }

    void SetAIDialogueScriptValuesFromIntro()
    {
        AIDialogueScript.playerMentionedFamily = familyThingChosen; 
        AIDialogueScript.playerWasFriendly = true;      
    }

    IEnumerator ShowEllipsisAndProceed()
    {
        StartEllipsisAnimation();
        yield return new WaitForSeconds(2f);
        StopEllipsisAnimation();
        SetAIDialogueScriptValuesFromIntro();
        LoadNextScene();
    }

    void DisplayFinnOptions(params string[] options)
    {
        finnResponseText.text = "";
        StopEllipsisAnimation();

        foreach (var option in finnDialogueOptions) option.gameObject.SetActive(false);

        int[] optionIndices;
        if (options.Length == 2)
        {
            // Use [0,2] for two options as requested
            optionIndices = new int[] { 0, 2 };
        }
        else
        {
            optionIndices = new int[] { 0, 1, 2 };
        }

        for (int i = 0; i < options.Length; i++)
        {
            if (i < optionIndices.Length && optionIndices[i] < finnDialogueOptions.Length)
            {
                int index = optionIndices[i];
                finnDialogueOptions[index].text = options[i];
                finnDialogueOptions[index].gameObject.SetActive(true);

                if (optionSoundEffect != null) optionSoundEffect.Play();
                UpdateOptionStyle(index);
            }
            else
            {
                Debug.LogWarning("Not enough Finn dialogue option UI elements assigned.");
            }
        }

        selectedOption = optionIndices[0];
        UpdateOptionStyles();
        optionsEnabled = true;
    }

    void HandleInput()
    {
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

        if (moved && navigationSoundEffect != null)
        {
            navigationSoundEffect.Play();
        }
        if (moved) UpdateOptionStyles();

        if (Input.GetKeyDown(KeyCode.Return))
        {
            HandleSelection(selectedOption);
        }
    }

    int[] GetActiveFinnOptionIndices()
    {
        var indices = new System.Collections.Generic.List<int>();
        for (int i = 0; i < finnDialogueOptions.Length; i++)
        {
            if (finnDialogueOptions[i].gameObject.activeSelf) indices.Add(i);
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
            finnDialogueOptions[index].color = (index == selectedOption) ? highlightedColor : defaultColor;
            finnDialogueOptions[index].fontStyle = (index == selectedOption) ? FontStyles.Bold : FontStyles.Normal;
        }
    }

    void HandleSelection(int optionIndex)
    {
        string chosenDialogue = finnDialogueOptions[optionIndex].text;
        optionsEnabled = false;
        foreach (var option in finnDialogueOptions) option.gameObject.SetActive(false);

        if (dialogueStep == 1)
        {
            StartCoroutine(TypewriterEffect(finnResponseText, chosenDialogue, () =>
            {
                dialogueStep++;
                HandleDialogueStep();
            }));
        }
        else if (dialogueStep == 3)
        {
            if (chosenDialogue.Contains("family thing"))
            {
                familyThingChosen = true;
            }
            StartCoroutine(TypewriterEffect(finnResponseText, chosenDialogue, () =>
            {
                dialogueStep++;
                HandleDialogueStep();
            }));
        }
        else if (dialogueStep == 5)
        {
            if (familyThingChosen)
            {
                dialogueStep++;
                HandleDialogueStep();
            }
            else
            {
                StartCoroutine(TypewriterEffect(finnResponseText, chosenDialogue, () =>
                {
                    dialogueStep++;
                    HandleDialogueStep();
                }));
            }
        }
        else if (dialogueStep == 7 && familyThingChosen)
        {
            StartCoroutine(TypewriterEffect(finnResponseText, chosenDialogue, () =>
            {
                dialogueStep++;
                HandleDialogueStep();
            }));
        }
        else
        {
            Debug.LogError("Unhandled selection at dialogue step: " + dialogueStep);
        }
    }

    IEnumerator TypewriterEffect(TextMeshProUGUI textMeshPro, string message, System.Action onComplete = null)
    {
        StopEllipsisAnimation();
        isTyping = true;
        textMeshPro.text = "";

        for (int i = 0; i <= message.Length; i++)
        {
            textMeshPro.text = message.Substring(0, i);
            if (talkSoundEffect != null && i % 2 == 0) talkSoundEffect.Play();
            yield return new WaitForSeconds(typeSpeed);
        }

        isTyping = false;
        onComplete?.Invoke();
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
            if (ellipsisText != null) ellipsisText.text = "";
        }
    }

    IEnumerator EllipsisAnimation()
    {
        if (ellipsisText == null)
        {
            Debug.LogError("Ellipsis Text is not assigned.");
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
        if (ellipsisCoroutine != null) StopCoroutine(ellipsisCoroutine);
    }
}
