using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using System.Collections;


namespace CookOrPanic.Score
{
    using CookOrPanic.CanvasManager;
    using CookOrPanic.Food;
    using CookOrPanic.ProcessedIngredient;   
    using CookOrPanic.LevelController;
  
    public class Score : MonoBehaviour
    {
        public static Score Instance; // Sistem Singleton
        private int _currentScore;

        private void Awake()
        {
            if (Instance == null) Instance = this;
            else Destroy(gameObject);
        }

        public void AddScore(Food food)
        {
            int baseScore = food._recipeOrigin._cookedScore;
            FoodState state = food._foodState;
            AddScoreFromFood(baseScore, state);
        }

        public void AddScoreFromFood(int _cookedScore, FoodState state)
        {
            int pointToAdd = 0;

            switch (state)
            {
                case FoodState.Cooked:
                    pointToAdd = _cookedScore;
                    break;
                case FoodState.Raw:
                    pointToAdd = Mathf.Max(9, _cookedScore - 5);
                    break;
                case FoodState.OverCooked:
                    pointToAdd = Mathf.Max(5, _cookedScore - 3);
                    break;
                default:
                    pointToAdd = 0;
                    break;
            }

            // Di Score.cs -> fungsi AddScoreFromFood
            _currentScore += pointToAdd;
         

            if (LevelController.Instance != null)
            {
                // Cek apakah skor sudah mencapai atau melewati target level saat ini
                if (_currentScore >= LevelController.Instance.GetTargetScore())
                {
                    LevelController.Instance.NexTLevel();
                }
            } 
        }

        public int GetCurrentScore() => _currentScore;

        public void ResetScore()
        {
            _currentScore = 0;
            CanvasManager canvas = FindObjectOfType<CanvasManager>();
            if (canvas != null) canvas.UpdateScoreUI(0);
        }

    }
}