using UnityEngine;
using System.Collections.Generic;

[System.Serializable]
public class Drink
{
    public string drinkName;
    public Sprite drinkImage;
    public List<string> requiredIngredients; // The correct order of ingredients
}
