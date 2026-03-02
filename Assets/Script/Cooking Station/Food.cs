using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace CookOrPanic.Food
{

    using CookOrPanic.ProcessedIngredient;

    public enum FoodType
    {
        Onde,
        Pempek
    }
    public class Food : MonoBehaviour
    {
        private FoodType _foodType;
        private FoodState _foodState;


        public void Initialze (FoodType type, FoodState state)
        {
            _foodType = type;
            _foodState = state;
        }

        public int GetPoint()
        {
            return 0;
        }
    }
}

