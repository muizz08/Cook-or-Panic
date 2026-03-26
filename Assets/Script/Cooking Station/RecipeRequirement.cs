using System;
using UnityEngine;


namespace CookOrPanic.RecipeRequirement
{
    using CookOrPanic.ProcessedIngredient;
    using CookOrPanic.Ingredient;

    [Serializable]
    public class RecipeRequirement 
    {
        public IngredientType _ingredientType;
        public FoodState _requiredState;
        public int _requiredAmount;
    }

}

