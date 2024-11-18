using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

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
    public TextMeshProUGUI[] ingredientTexts; // Texts for main visible ingredients (4 ingredients)
    public TextMeshProUGUI[] neededIngredientTexts; // Texts for required ingredients only (up to 4 ingredients)
    public Image[] ingredientImages; // Icons for main visible ingredients (4 ingredients)
    public Image[] selectedIngredientImages; // UI to display selected ingredients at the top
    [SerializeField] private Sprite[] dummyIngredientIcons;
    [SerializeField] private string[] dummyIngredientNames;

    private Drink currentDrink;
    private HashSet<string> selectedIngredients; // Tracks selected ingredients
    private Dictionary<KeyCode, string> arrowIngredients; // Maps arrow keys to ingredient names

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
    }

    void HandleInput()
    {
        // Check for arrow key presses and select the corresponding ingredient
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
            Debug.LogError($"Drink data for '{currentDrink.drinkName}' is incomplete or invalid. Check required ingredients and icons.");
            return;
        }

        drinkDisplay.sprite = currentDrink.drinkImage;
        orderText.text = "Order: " + currentDrink.drinkName;
        feedbackText.text = "";
        selectedIngredients = new HashSet<string>();

        ResetSelectedIngredients();
        AssignArrowIngredients();
        DisplayNeededIngredients();
    }

    void AssignArrowIngredients()
    {
        arrowIngredients = new Dictionary<KeyCode, string>();

        // Create a pool of ingredients
        List<string> ingredientPool = new List<string>(currentDrink.requiredIngredients);
        List<Sprite> iconPool = new List<Sprite>(currentDrink.ingredientIcons);

        // Add dummy ingredients to fill up slots
        while (ingredientPool.Count < 4)
        {
            int dummyIndex = Random.Range(0, dummyIngredientNames.Length);
            ingredientPool.Add(dummyIngredientNames[dummyIndex]);
            iconPool.Add(dummyIngredientIcons[dummyIndex]);
        }

        // Shuffle the pool
        for (int i = ingredientPool.Count - 1; i > 0; i--)
        {
            int randomIndex = Random.Range(0, i + 1);
            (ingredientPool[i], ingredientPool[randomIndex]) = (ingredientPool[randomIndex], ingredientPool[i]);
            (iconPool[i], iconPool[randomIndex]) = (iconPool[randomIndex], iconPool[i]);
        }

        // Assign ingredients to arrow keys and update UI
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
        // Display only the needed ingredients on the left
        for (int i = 0; i < neededIngredientTexts.Length; i++)
        {
            if (i < currentDrink.requiredIngredients.Count)
            {
                neededIngredientTexts[i].text = currentDrink.requiredIngredients[i];
            }
            else
            {
                neededIngredientTexts[i].text = ""; // Clear unused slots
            }
        }
    }

    public void CheckIngredient(string selectedIngredient)
    {
        if (currentDrink.requiredIngredients.Contains(selectedIngredient) && !selectedIngredients.Contains(selectedIngredient))
        {
            selectedIngredients.Add(selectedIngredient);

            // Update the top UI to display the selected ingredient
            for (int i = 0; i < selectedIngredientImages.Length; i++)
            {
                if (!selectedIngredientImages[i].enabled)
                {
                    // Find the correct sprite from arrowIngredients
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

            feedbackText.text = "Correct!";

            // Check if all ingredients have been selected
            if (selectedIngredients.Count == currentDrink.requiredIngredients.Count)
            {
                feedbackText.text = "Order Completed!";
                Invoke("SetNewOrder", 2f);
            }
        }
        else
        {
            feedbackText.text = "Wrong ingredient or already selected!";
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
            neededText.text = ""; // Clear the needed ingredients display
        }
    }
}
