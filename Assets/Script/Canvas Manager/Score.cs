using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CookOrPanic.Score
{
    using CookOrPanic.CanvasManager;
    using CookOrPanic.Food;
    using CookOrPanic.ProcessedIngredient;
    public class Score : MonoBehaviour
    {
        private int _currentScore;


        public void AddScore(Food food)
        {
            int baseScore = food._recipeOrigin._cookedScore;
            FoodState state = food._foodState;

            AddScoreFromFood(baseScore, state);
        }

        // Method tetap sesuai permintaan Anda, ditambahkan parameter untuk kalkulasi
        public void AddScoreFromFood(int _cookedScore, FoodState state)
        {
            int pointToAdd = 0;

            switch (state)
            {
                case FoodState.Cooked:
                    pointToAdd = _cookedScore;
                    break;
                case FoodState.Raw:
                    pointToAdd = Mathf.Max(0, _cookedScore - 5);
                    break;
                case FoodState.OverCooked:
                    pointToAdd = Mathf.Max(0, _cookedScore - 3);
                    break;
                default:
                    pointToAdd = 0;
                    break;
            }

            _currentScore += pointToAdd;

        }

        public void ResetScore()
        {

        }
    }

}