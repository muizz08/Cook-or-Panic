using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;


namespace CookOrPanic.Recipe
{
    using CookOrPanic.Food;
    using CookOrPanic.RecipeRequirement;
    using CookOrPanic.ProcessedIngredient;
  

    [CreateAssetMenu(fileName = "Recipe", menuName = "Recipe")]
    public class Recipe : ScriptableObject
    {
        public FoodType _resultFoodType;
        [SerializeField] public List<RecipeRequirement> _requirements;
        [SerializeField] public List<GameObject> _resultPrefabs;
        [SerializeField] public int _cookedScore;


        public bool Validate(List<ProcessedIngredient> ingredients)
        {
            foreach (var req in _requirements)
            {
                int matchCount = ingredients.Count(input =>
                    input.GetIngredientType() == req._ingredientType &&
                    input.GetState() == req._requiredState);

                if (matchCount != req._requiredAmount) return false;
            }
            return ingredients.Count == TotalRequiredAmount();
        }

        private int TotalRequiredAmount()
        {
            int total = 0;
            foreach (var req in _requirements) total += req._requiredAmount;
            return total;
        }
    }

}
