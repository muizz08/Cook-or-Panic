using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace CookOrPanic.ProcessedIngredient
{
    using CookOrPanic.Ingredient;
    public enum FoodState
    {
        Raw,
        Cooked,
        OverCooked
    }

    public class ProcessedIngredient : Ingredient
    {
 
        public FoodState _currentState;

        public IngredientType ingredientType
        {
            get { return _ingredientType; } // Mengambil nilai dari class Parent (Ingredient)
            set { _ingredientType = value; }
        }


        private void Awake()
        {
            // Memastikan setiap bahan yang baru muncul statusnya adalah Raw
            _currentState = FoodState.Raw;
        }
        public void Raw()
        {
            _currentState = FoodState.Raw;
        }

        public void Cooked()
        {
            _currentState = FoodState.Cooked;
        }

        public void OverCook()
        {
            _currentState = FoodState.OverCooked;
        }

        public FoodState GetState()
        {
            return _currentState;
        }
    }

}
