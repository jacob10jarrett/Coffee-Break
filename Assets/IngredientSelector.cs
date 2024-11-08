using UnityEngine;

public class IngredientSelector : MonoBehaviour
{
    public DrinkManager drinkManager;
    public string ingredient; // This should match an ingredient name in the drink's requiredIngredients

    public void OnClick()
    {
        drinkManager.CheckIngredient(ingredient);
    }
}
