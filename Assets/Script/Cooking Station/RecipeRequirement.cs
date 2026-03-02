using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace CookOrPanic.RecipeRequirement
{
    using CookOrPanic.ProcessedIngredient;
    using CookOrPanic.Ingredient;

    public class RecipeRequirement : MonoBehaviour
    {
        public IngredientType _ingredientType;
        public FoodState _requiredState;
        public int _requiredAmount;
    }

}

