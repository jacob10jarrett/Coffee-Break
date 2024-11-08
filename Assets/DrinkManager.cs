using UnityEngine;
using UnityEngine.UI; // For Image, Button, etc.
using TMPro; // For TextMeshPro elements
using System.Collections.Generic; // For List

public class DrinkManager : MonoBehaviour
{
    public List<Drink> drinks; // List of possible drinks
    public Image drinkDisplay; // Image where the drink sprite will be displayed
    public TextMeshProUGUI orderText; // Use TextMeshProUGUI for text
    public TextMeshProUGUI feedbackText; // Use TextMeshProUGUI for feedback text

    private Drink currentDrink;
    private int currentStep = 0; // Tracks the current ingredient step

    void Start()
    {
        // Set a new drink order at the beginning
        SetNewOrder();
    }

    void SetNewOrder()
    {
        // Choose a random drink order
        currentDrink = drinks[Random.Range(0, drinks.Count)];
        drinkDisplay.sprite = currentDrink.drinkImage;
        orderText.text = "Order: " + currentDrink.drinkName;
        feedbackText.text = ""; // Clear feedback text
        currentStep = 0; // Reset the step
    }

    public void CheckIngredient(string selectedIngredient)
    {
        // Check if the selected ingredient matches the required ingredient
        if (selectedIngredient == currentDrink.requiredIngredients[currentStep])
        {
            feedbackText.text = "Correct!";
            currentStep++;

            // Check if the drink is completed
            if (currentStep >= currentDrink.requiredIngredients.Count)
            {
                Debug.Log("Order Completed!");
                feedbackText.text = "Order Completed!";
                Invoke("SetNewOrder", 2f); // Set a new order after a short delay
            }
        }
        else
        {
            feedbackText.text = "Wrong ingredient! Try again.";
            currentStep = 0; // Reset if the wrong ingredient is selected
        }
    }
}
