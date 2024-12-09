using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using UnityEngine.SceneManagement;

public class DrinkManager : MonoBehaviour
{
    [System.Serializable]
    public class Drink
    {
        public string drinkName;
        public Sprite drinkImage;
        public List<string> requiredIngredients;
        public List<Sprite> ingredientIcons;
    }

    public List<Drink> drinks;
    public Image drinkDisplay;
    public TextMeshProUGUI orderText;
    public TextMeshProUGUI feedbackText;
    public TextMeshProUGUI timerText; // Timer text at the top
    public TextMeshProUGUI[] ingredientTexts; 
    public TextMeshProUGUI[] neededIngredientTexts;
    public Image[] ingredientImages; 
    public Image[] selectedIngredientImages; 
    [SerializeField] private Sprite[] dummyIngredientIcons;
    [SerializeField] private string[] dummyIngredientNames;

    public Image progressBar;
    [SerializeField] private int drinksRequiredForCompletion = 3;
    private int drinksCompletedCount = 0;

    private float orderTimer = 8f;

    private Drink currentDrink;
    private HashSet<string> selectedIngredients;
    private Dictionary<KeyCode, string> arrowIngredients;

    // Order completion flag
    private bool orderCompleted = false;

    // Sound Effects
    [Header("Sound Effects")]
    public AudioSource audioSource;         // Assign in inspector
    public AudioClip timerStartSFX;         // Plays when timer hits 5 seconds
    public AudioClip correctSFX;            // Plays on correct ingredient
    public AudioClip wrongSFX;              // Plays on wrong ingredient
    public AudioClip orderCompleteSFX;      // Plays when order completes

    // A flag to indicate if we've already played the timerStartSFX at 5 seconds
    private bool timerSFXPlayed = false; 

    void Start()
    {
        if (drinks == null || drinks.Count == 0)
        {
            Debug.LogError("No drinks available. Please add drinks in the Inspector.");
            return;
        }

        if (ingredientTexts.Length != 4 || ingredientImages.Length != 4 || selectedIngredientImages.Length != 4 || neededIngredientTexts.Length != 4)
        {
            Debug.LogError("UI arrays must have exactly 4 elements.");
            return;
        }

        SetNewOrder();
    }

    void Update()
    {
        HandleInput();
        UpdateTimer();
    }

    void HandleInput()
    {
        if (arrowIngredients == null) return;

        if (Input.GetKeyDown(KeyCode.UpArrow) && arrowIngredients.ContainsKey(KeyCode.UpArrow))
        {
            CheckIngredient(arrowIngredients[KeyCode.UpArrow]);
        }
        else if (Input.GetKeyDown(KeyCode.DownArrow) && arrowIngredients.ContainsKey(KeyCode.DownArrow))
        {
            CheckIngredient(arrowIngredients[KeyCode.DownArrow]);
        }
        else if (Input.GetKeyDown(KeyCode.LeftArrow) && arrowIngredients.ContainsKey(KeyCode.LeftArrow))
        {
            CheckIngredient(arrowIngredients[KeyCode.LeftArrow]);
        }
        else if (Input.GetKeyDown(KeyCode.RightArrow) && arrowIngredients.ContainsKey(KeyCode.RightArrow))
        {
            CheckIngredient(arrowIngredients[KeyCode.RightArrow]);
        }
    }

    void SetNewOrder()
    {
        currentDrink = drinks[Random.Range(0, drinks.Count)];
        if (currentDrink.requiredIngredients == null || currentDrink.ingredientIcons == null ||
            currentDrink.requiredIngredients.Count != currentDrink.ingredientIcons.Count)
        {
            Debug.LogError($"Drink data for '{currentDrink.drinkName}' is incomplete or invalid.");
            return;
        }

        drinkDisplay.sprite = currentDrink.drinkImage;
        orderText.text = "Order: " + currentDrink.drinkName;
        feedbackText.text = "";

        selectedIngredients = new HashSet<string>();

        ResetSelectedIngredients();
        AssignArrowIngredients();
        DisplayNeededIngredients();

        orderTimer = 8f;
        orderCompleted = false;

        timerText.text = "";
        timerText.color = Color.white;

        timerSFXPlayed = false; // Reset this so we can play the timer sound again next round
    }

    void AssignArrowIngredients()
    {
        arrowIngredients = new Dictionary<KeyCode, string>();

        List<string> ingredientPool = new List<string>(currentDrink.requiredIngredients);
        List<Sprite> iconPool = new List<Sprite>(currentDrink.ingredientIcons);

        while (ingredientPool.Count < 4)
        {
            int dummyIndex = Random.Range(0, dummyIngredientNames.Length);
            ingredientPool.Add(dummyIngredientNames[dummyIndex]);
            iconPool.Add(dummyIngredientIcons[dummyIndex]);
        }

        // Shuffle
        for (int i = ingredientPool.Count - 1; i > 0; i--)
        {
            int randomIndex = Random.Range(0, i + 1);
            (ingredientPool[i], ingredientPool[randomIndex]) = (ingredientPool[randomIndex], ingredientPool[i]);
            (iconPool[i], iconPool[randomIndex]) = (iconPool[randomIndex], iconPool[i]);
        }

        for (int i = 0; i < 4; i++)
        {
            KeyCode arrowKey = i switch
            {
                0 => KeyCode.UpArrow,
                1 => KeyCode.DownArrow,
                2 => KeyCode.LeftArrow,
                3 => KeyCode.RightArrow,
                _ => KeyCode.None
            };

            arrowIngredients[arrowKey] = ingredientPool[i];
            ingredientTexts[i].text = ingredientPool[i];
            ingredientImages[i].sprite = iconPool[i];
            ingredientImages[i].enabled = true;
        }
    }

    void DisplayNeededIngredients()
    {
        for (int i = 0; i < neededIngredientTexts.Length; i++)
        {
            if (i < currentDrink.requiredIngredients.Count)
            {
                neededIngredientTexts[i].text = currentDrink.requiredIngredients[i];
            }
            else
            {
                neededIngredientTexts[i].text = "";
            }
        }
    }

    public void CheckIngredient(string selectedIngredient)
    {
        bool correctChoice = currentDrink.requiredIngredients.Contains(selectedIngredient) && !selectedIngredients.Contains(selectedIngredient);

        if (correctChoice)
        {
            selectedIngredients.Add(selectedIngredient);
            PlaySFX(correctSFX);

            for (int i = 0; i < selectedIngredientImages.Length; i++)
            {
                if (!selectedIngredientImages[i].enabled)
                {
                    foreach (var pair in arrowIngredients)
                    {
                        if (pair.Value == selectedIngredient)
                        {
                            int index = pair.Key switch
                            {
                                KeyCode.UpArrow => 0,
                                KeyCode.DownArrow => 1,
                                KeyCode.LeftArrow => 2,
                                KeyCode.RightArrow => 3,
                                _ => -1
                            };

                            if (index >= 0 && index < ingredientImages.Length)
                            {
                                selectedIngredientImages[i].sprite = ingredientImages[index].sprite;
                                selectedIngredientImages[i].enabled = true;
                            }
                            break;
                        }
                    }
                    break;
                }
            }

            ShowFeedbackMessage("Correct!");

            // Check if order is complete
            if (selectedIngredients.Count == currentDrink.requiredIngredients.Count)
            {
                ShowFeedbackMessage("Order Completed!", false);
                drinksCompletedCount++;

                float progress = (float)drinksCompletedCount / drinksRequiredForCompletion;
                progressBar.fillAmount = progress;

                // Once order is completed, stop the timer display
                orderCompleted = true;
                timerText.text = "";
                PlaySFX(orderCompleteSFX);

                if (drinksCompletedCount >= drinksRequiredForCompletion)
                {
                    Invoke("LoadNextScene", 2f);
                }
                else
                {
                    Invoke("SetNewOrder", 2f);
                }
            }
        }
        else
        {
            // Wrong choice
            PlaySFX(wrongSFX);
            ShowFeedbackMessage("Wrong ingredient or already selected!");
        }
    }

    void ShowFeedbackMessage(string message, bool autoClear = true)
    {
        feedbackText.text = message;
        if (autoClear && message != "Order Completed!")
        {
            Invoke("ClearFeedbackText", 1.5f);
        }
    }

    void ResetSelectedIngredients()
    {
        foreach (var box in selectedIngredientImages)
        {
            box.sprite = null;
            box.enabled = false;
        }

        foreach (var neededText in neededIngredientTexts)
        {
            neededText.text = "";
        }
    }

    void UpdateTimer()
    {
        if (orderCompleted)
        {
            // Order is completed, don't continue updating or showing the timer
            return;
        }

        if (orderTimer > 0f)
        {
            float previousTimer = orderTimer;
            orderTimer -= Time.deltaTime;

            if (orderTimer <= 0f)
            {
                LoadFailScene();
                return;
            }

            // Check if we just crossed the 5-second threshold
            if (previousTimer > 5f && orderTimer <= 5f && !timerSFXPlayed)
            {
                // Play the timer start SFX
                PlaySFX(timerStartSFX);
                timerSFXPlayed = true;
            }
        }

        // Show the timer at the top if orderTimer <= 5 and > 0
        if (orderTimer <= 5f && orderTimer > 0f)
        {
            int timeLeft = Mathf.CeilToInt(orderTimer);

            // Color coding based on timeLeft:
            // 5,4 = white
            // 3 = yellow
            // 2 = orange
            // 1 = red
            if (timeLeft == 3)
            {
                timerText.color = Color.yellow;
            }
            else if (timeLeft == 2)
            {
                timerText.color = new Color(1f, 0.5f, 0f); // Orange
            }
            else if (timeLeft == 1)
            {
                timerText.color = Color.red;
            }
            else
            {
                timerText.color = Color.white;
            }

            timerText.text = timeLeft.ToString();
        }
        else
        {
            // Above 5 seconds or below 0, no timer is shown
            timerText.text = "";
        }
    }

    void ClearFeedbackText()
    {
        if (feedbackText.text != "Order Completed!")
        {
            feedbackText.text = "";
        }
    }

    void LoadNextScene()
    {
        SceneManager.LoadScene("AI_Dialogue");
    }

    void LoadFailScene()
    {
        SceneManager.LoadScene("Fail");
    }

    void PlaySFX(AudioClip clip)
    {
        if (audioSource != null && clip != null)
        {
            audioSource.PlayOneShot(clip);
        }
    }
}
