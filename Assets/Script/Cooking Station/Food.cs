using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace CookOrPanic.Food
{

    using CookOrPanic.ProcessedIngredient;
    using CookOrPanic.Recipe;

    public enum FoodType
    {
        Onde,
        Pempek
    }
    public class Food : MonoBehaviour
    {
        public FoodType _foodType;
        public FoodState _foodState;
        public Recipe _recipeOrigin;


        public void Initialze(FoodType type, FoodState state, Recipe recipe)
        {
            _foodType = type;
            _foodState = state;
            _recipeOrigin = recipe;
        }

        public int GetPoint()
        {
            return 0;
        }

    }
}


