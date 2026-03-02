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

        public void Raw()
        {

        }

        public void Cooked()
        {

        }

        public void OverCook()
        {

        }
        public FoodState GetState()
        {
            return _currentState;
        }
    }

}
