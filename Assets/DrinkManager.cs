using UnityEngine; 
using UnityEngine.SceneManagement;
using TMPro;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;

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
    public TextMeshProUGUI timerText; 
    public TextMeshProUGUI[] ingredientTexts; 
    public TextMeshProUGUI[] neededIngredientTexts;
    public Image[] ingredientImages; 
    public Image[] selectedIngredientImages; 
    [SerializeField] private Sprite[] dummyIngredientIcons;
    [SerializeField] private string[] dummyIngredientNames;

    public Image progressBar;
    private int drinksRequiredForCompletion;

    private int drinksCompletedCount = 0;
    private float orderTimer = 8f;

    private Drink currentDrink;
    private Dictionary<string, int> selectedIngredientCounts;
    private Dictionary<KeyCode, string> arrowIngredients;

    private bool orderCompleted = false;

    [Header("Sound Effects")]
    public AudioSource audioSource;
    public AudioClip timerStartSFX;
    public AudioClip correctSFX;
    public AudioClip wrongSFX;
    public AudioClip orderCompleteSFX;

    private bool timerSFXPlayed = false; 

    void Start()
    {
        // Set drinks required based on scenario
        if (AIDialogueScript.drinkScenario == 1)
        {
            drinksRequiredForCompletion = 5;
        }
        else if (AIDialogueScript.drinkScenario == 2)
        {
            drinksRequiredForCompletion = 8;
        }

        AIDialogueScript.drinksRequired = drinksRequiredForCompletion;
        AIDialogueScript.drinksServedCorrectly = 0;
        AIDialogueScript.playerMadeMistakesCount = 0; // Reset mistakes each time this scene starts

        if (drinks == null || drinks.Count == 0)
        {
            Debug.LogError("No drinks available.");
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
            Debug.LogError($"Drink data for '{currentDrink.drinkName}' is incomplete.");
            return;
        }

        drinkDisplay.sprite = currentDrink.drinkImage;
        orderText.text = "Order: " + currentDrink.drinkName;
        feedbackText.text = "";
        selectedIngredientCounts = new Dictionary<string, int>();

        ResetSelectedIngredients();
        AssignArrowIngredients();
        DisplayNeededIngredients();

        orderTimer = 8f;
        orderCompleted = false;

        timerText.text = "";
        timerText.color = Color.white;
        timerSFXPlayed = false;
    }

    void AssignArrowIngredients()
    {
        arrowIngredients = new Dictionary<KeyCode, string>();

        // Create a list of required ingredients, accounting for duplicates
        List<string> ingredientPool = new List<string>(currentDrink.requiredIngredients);
        List<Sprite> iconPool = new List<Sprite>(currentDrink.ingredientIcons);

        // Create a HashSet of required ingredient names for efficient lookup
        HashSet<string> requiredIngredientNames = new HashSet<string>(currentDrink.requiredIngredients);

        // Filter dummy ingredients to exclude any required ingredients
        List<string> filteredDummyNames = new List<string>();
        List<Sprite> filteredDummyIcons = new List<Sprite>();

        for (int i = 0; i < dummyIngredientNames.Length; i++)
        {
            string dummyName = dummyIngredientNames[i];
            if (!requiredIngredientNames.Contains(dummyName))
            {
                filteredDummyNames.Add(dummyName);
                filteredDummyIcons.Add(dummyIngredientIcons[i]);
            }
        }

        // Add dummy ingredients until we have 4 total ingredients
        while (ingredientPool.Count < 4 && filteredDummyNames.Count > 0)
        {
            int randomIndex = Random.Range(0, filteredDummyNames.Count);
            ingredientPool.Add(filteredDummyNames[randomIndex]);
            iconPool.Add(filteredDummyIcons[randomIndex]);
            filteredDummyNames.RemoveAt(randomIndex);
            filteredDummyIcons.RemoveAt(randomIndex);
        }

        // Shuffle the ingredientPool and iconPool together
        for (int i = ingredientPool.Count - 1; i > 0; i--)
        {
            int randomIndex = Random.Range(0, i + 1);
            // Swap ingredient names
            string tempName = ingredientPool[i];
            ingredientPool[i] = ingredientPool[randomIndex];
            ingredientPool[randomIndex] = tempName;

            // Swap corresponding icons
            Sprite tempIcon = iconPool[i];
            iconPool[i] = iconPool[randomIndex];
            iconPool[randomIndex] = tempIcon;
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

            if (i < ingredientPool.Count)
            {
                arrowIngredients[arrowKey] = ingredientPool[i];
                ingredientTexts[i].text = ingredientPool[i];
                ingredientImages[i].sprite = iconPool[i];
                ingredientImages[i].enabled = true;
            }
            else
            {
                // If there are fewer than 4 ingredients, disable remaining UI elements
                ingredientTexts[i].text = "";
                ingredientImages[i].sprite = null;
                ingredientImages[i].enabled = false;
            }
        }
    }

    void DisplayNeededIngredients()
    {
        // Count the required instances of each ingredient
        Dictionary<string, int> requiredIngredientCounts = new Dictionary<string, int>();
        foreach (var ingredient in currentDrink.requiredIngredients)
        {
            if (requiredIngredientCounts.ContainsKey(ingredient))
            {
                requiredIngredientCounts[ingredient]++;
            }
            else
            {
                requiredIngredientCounts[ingredient] = 1;
            }
        }

        // Display the required ingredients with their counts
        for (int i = 0; i < neededIngredientTexts.Length; i++)
        {
            if (i < requiredIngredientCounts.Count)
            {
                string ingredient = new List<string>(requiredIngredientCounts.Keys)[i];
                int count = requiredIngredientCounts[ingredient];
                neededIngredientTexts[i].text = $"{ingredient} x{count}";
            }
            else
            {
                neededIngredientTexts[i].text = "";
            }
        }
    }

    public void CheckIngredient(string selectedIngredient)
    {
        bool isRequired = currentDrink.requiredIngredients.Contains(selectedIngredient);
        bool alreadySelectedEnough = false;

        if (isRequired)
        {
            int requiredCount = currentDrink.requiredIngredients.FindAll(x => x == selectedIngredient).Count;
            if (selectedIngredientCounts.ContainsKey(selectedIngredient))
            {
                if (selectedIngredientCounts[selectedIngredient] >= requiredCount)
                {
                    alreadySelectedEnough = true;
                }
            }
        }

        bool correctChoice = isRequired && !alreadySelectedEnough;

        if (correctChoice)
        {
            if (selectedIngredientCounts.ContainsKey(selectedIngredient))
            {
                selectedIngredientCounts[selectedIngredient]++;
            }
            else
            {
                selectedIngredientCounts[selectedIngredient] = 1;
            }

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

            // Check if all required ingredients have been selected
            bool allIngredientsSelected = true;
            foreach (var req in currentDrink.requiredIngredients)
            {
                if (!selectedIngredientCounts.ContainsKey(req) || selectedIngredientCounts[req] < currentDrink.requiredIngredients.FindAll(x => x == req).Count)
                {
                    allIngredientsSelected = false;
                    break;
                }
            }

            if (allIngredientsSelected)
            {
                ShowFeedbackMessage("Order Completed!", false);
                drinksCompletedCount++;
                AIDialogueScript.drinksServedCorrectly = drinksCompletedCount;

                float progress = (float)drinksCompletedCount / AIDialogueScript.drinksRequired;
                progressBar.fillAmount = progress;

                orderCompleted = true;
                timerText.text = "";
                PlaySFX(orderCompleteSFX);

                if (drinksCompletedCount >= AIDialogueScript.drinksRequired)
                {
                    // If first scenario done (5 drinks), load AI dialogue and switch scenario
                    if (AIDialogueScript.drinkScenario == 1)
                    {
                        AIDialogueScript.drinkScenario = 2; // Next time we load this scene, 8 drinks required
                        Invoke("LoadAIDialogueScene", 2f);
                    }
                    else
                    {
                        // If second scenario (8 drinks) done, load AI Dialogue with new script
                        Invoke("LoadSecondAIDialogueScene", 2f);
                    }
                }
                else
                {
                    Invoke("SetNewOrder", 2f);
                }
            }
        }
        else
        {
            PlaySFX(wrongSFX);
            ShowFeedbackMessage("Wrong ingredient or already selected enough!");
            AIDialogueScript.playerMadeMistakesCount++; // Increment mistake count
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
        if (orderCompleted) return;

        if (orderTimer > 0f)
        {
            float previousTimer = orderTimer;
            orderTimer -= Time.deltaTime;

            if (orderTimer <= 0f)
            {
                LoadFailScene();
                return;
            }

            if (previousTimer > 5f && orderTimer <= 5f && !timerSFXPlayed)
            {
                PlaySFX(timerStartSFX);
                timerSFXPlayed = true;
            }
        }

        if (orderTimer <= 5f && orderTimer > 0f)
        {
            int timeLeft = Mathf.CeilToInt(orderTimer);
            if (timeLeft == 3) timerText.color = Color.yellow;
            else if (timeLeft == 2) timerText.color = new Color(1f, 0.5f, 0f);
            else if (timeLeft == 1) timerText.color = Color.red;
            else timerText.color = Color.white;

            timerText.text = timeLeft.ToString();
        }
        else
        {
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

    void LoadAIDialogueScene()
    {
        SceneManager.LoadScene("AI_Dialogue");
    }

    void LoadSecondAIDialogueScene()
    {
        SceneManager.LoadScene("Win");
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
