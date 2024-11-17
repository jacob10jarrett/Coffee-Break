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
    public TextMeshProUGUI[] ingredientTexts;
    public Image[] ingredientImages;
    public Image[] selectedIngredientImages;
    [SerializeField] private Sprite[] dummyIngredientIcons;

    private Drink currentDrink;
    private int currentStep = 0;
    private int selectedIndex = 0; // Tracks the currently selected ingredient

    void Start()
    {
        if (drinks == null || drinks.Count == 0)
        {
            Debug.LogError("No drinks available. Please add drinks in the Inspector.");
            return;
        }

        if (ingredientTexts == null || ingredientImages == null || selectedIngredientImages == null)
        {
            Debug.LogError("UI arrays are not assigned. Please assign them in the Inspector.");
            return;
        }

        if (ingredientTexts.Length != ingredientImages.Length || selectedIngredientImages.Length < ingredientTexts.Length)
        {
            Debug.LogError("Mismatch between UI arrays. Ensure they are properly configured.");
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
        if (Input.GetKeyDown(KeyCode.UpArrow))
        {
            NavigateIngredients(-1);
        }
        else if (Input.GetKeyDown(KeyCode.DownArrow))
        {
            NavigateIngredients(1);
        }
        else if (Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.Return))
        {
            ConfirmIngredient();
        }
    }

    void NavigateIngredients(int direction)
    {
        // Adjust the selected index and loop around if needed
        selectedIndex = (selectedIndex + direction + ingredientTexts.Length) % ingredientTexts.Length;

        UpdateSelectionHighlight();
    }

    void UpdateSelectionHighlight()
    {
        // Highlight the currently selected ingredient
        for (int i = 0; i < ingredientTexts.Length; i++)
        {
            if (i == selectedIndex)
            {
                ingredientTexts[i].color = Color.yellow; // Highlight
            }
            else
            {
                ingredientTexts[i].color = Color.white; // Default
            }
        }
    }

    void ConfirmIngredient()
    {
        // Get the currently selected ingredient
        string selectedIngredient = ingredientTexts[selectedIndex].text;

        // Validate the ingredient
        CheckIngredient(selectedIngredient);
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
        currentStep = 0;

        ResetSelectedIngredients();
        UpdateIngredientDisplay();
    }

    void UpdateIngredientDisplay()
    {
        List<string> ingredientPool = new List<string>(currentDrink.requiredIngredients);
        List<Sprite> iconPool = new List<Sprite>(currentDrink.ingredientIcons);

        while (ingredientPool.Count < ingredientImages.Length)
        {
            ingredientPool.Add("Dummy Ingredient");
            iconPool.Add(dummyIngredientIcons[Random.Range(0, dummyIngredientIcons.Length)]);
        }

        for (int i = ingredientPool.Count - 1; i > 0; i--)
        {
            int randomIndex = Random.Range(0, i + 1);
            (ingredientPool[i], ingredientPool[randomIndex]) = (ingredientPool[randomIndex], ingredientPool[i]);
            (iconPool[i], iconPool[randomIndex]) = (iconPool[randomIndex], iconPool[i]);
        }

        for (int i = 0; i < ingredientImages.Length; i++)
        {
            if (i < ingredientPool.Count)
            {
                ingredientTexts[i].text = ingredientPool[i];
                ingredientImages[i].sprite = iconPool[i];
                ingredientImages[i].enabled = true;
            }
            else
            {
                ingredientTexts[i].text = "";
                ingredientImages[i].enabled = false;
            }
        }

        selectedIndex = 0; // Reset to the first ingredient
        UpdateSelectionHighlight();
    }

    public void CheckIngredient(string selectedIngredient)
    {
        string correctIngredient = currentDrink.requiredIngredients[currentStep];

        if (selectedIngredient == correctIngredient)
        {
            feedbackText.text = "Correct!";
            if (currentStep < selectedIngredientImages.Length)
            {
                selectedIngredientImages[currentStep].sprite = ingredientImages[selectedIndex].sprite;
                selectedIngredientImages[currentStep].enabled = true;
            }

            currentStep++;

            if (currentStep >= currentDrink.requiredIngredients.Count)
            {
                feedbackText.text = "Order Completed!";
                Invoke("SetNewOrder", 2f);
            }
        }
        else
        {
            feedbackText.text = "Wrong ingredient! Try again.";
            ResetSelectedIngredients();
            currentStep = 0;
        }
    }

    void ResetSelectedIngredients()
    {
        foreach (var box in selectedIngredientImages)
        {
            if (box != null)
            {
                box.sprite = null;
                box.enabled = false;
            }
        }
    }
}
