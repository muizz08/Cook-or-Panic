using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CookOrPanic.CookingStation
{
     using CookOrPanic.ProcessedIngredient;
     using CookOrPanic.Recipe;

    public class CookingStation : MonoBehaviour
    {
        private List<ProcessedIngredient> _currentIngredients;
        private List<Recipe> _availableRecipe;


        public void AddIngredient()
        {

        }

        public bool TryCook()
        {
            return true;
        }

        public void ClearStation()
        {

        }
    }
}