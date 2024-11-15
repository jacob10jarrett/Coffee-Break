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
        public List<Sprite> ingredientIcons; // Add icons for each ingredient
    }

    public List<Drink> drinks;
    public Image drinkDisplay;
    public TextMeshProUGUI orderText;
    public TextMeshProUGUI feedbackText;
    public TextMeshProUGUI[] ingredientTexts; // Text for ingredient names
    public Image[] ingredientImages; // Image components for ingredient icons next to arrows

    private Drink currentDrink;
    private int currentStep = 0;

    void Start()
    {
        SetNewOrder();
    }

    void SetNewOrder()
    {
        currentDrink = drinks[Random.Range(0, drinks.Count)];
        drinkDisplay.sprite = currentDrink.drinkImage;
        orderText.text = "Order: " + currentDrink.drinkName;
        feedbackText.text = "";
        currentStep = 0;

        UpdateIngredientDisplay();
    }

    void UpdateIngredientDisplay()
    {
        for (int i = 0; i < ingredientTexts.Length; i++)
        {
            if (i < currentDrink.requiredIngredients.Count)
            {
                ingredientTexts[i].text = currentDrink.requiredIngredients[i];
                ingredientImages[i].sprite = currentDrink.ingredientIcons[i]; // Set ingredient icon
                ingredientImages[i].enabled = true; // Show the image
            }
            else
            {
                ingredientTexts[i].text = "";
                ingredientImages[i].enabled = false; // Hide the image if not used
            }
        }
    }

    public void CheckIngredient(string selectedIngredient)
    {
        if (selectedIngredient == currentDrink.requiredIngredients[currentStep])
        {
            feedbackText.text = "Correct!";
            currentStep++;

            if (currentStep >= currentDrink.requiredIngredients.Count)
            {
                Debug.Log("Order Completed!");
                feedbackText.text = "Order Completed!";
                Invoke("SetNewOrder", 2f);
            }
        }
        else
        {
            feedbackText.text = "Wrong ingredient! Try again.";
            currentStep = 0;
            UpdateIngredientDisplay();
        }
    }
}
