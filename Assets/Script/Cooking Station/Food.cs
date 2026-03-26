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
        public float _timeSpentCooking;
        public FoodType _foodType;
        public FoodState _foodState;
        public Recipe _recipeOrigin;



        public void Initialze(FoodType type, FoodState state, Recipe recipe)
        {
            _foodType = type;
            _foodState = state;
            _recipeOrigin = recipe;
            _timeSpentCooking = 0f;
            Debug.Log($"Makanan {type} diinisialisasi dengan state: {state}"); // <--- Tambah ini
        }

        public int GetPoint()
        {
            return 0;
        }

        public void UpdateStateBasedOnTime(float duration)
        {
            // LOGIKA SATU-SATUNYA YANG MENENTUKAN STATUS
            if (duration < 6f) _foodState = FoodState.Raw;
            else if (duration >= 6f && duration <= 8f) _foodState = FoodState.Cooked;
            else _foodState = FoodState.OverCooked;

            Debug.Log($"[Food] Status {this.name} sekarang: {_foodState} (Waktu: {duration}s)");
        }

    }
}


