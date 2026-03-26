using System.Collections.Generic;
using UnityEngine;

namespace CookOrPanic.LevelData
{
    using CookOrPanic.Recipe;

    [CreateAssetMenu(fileName = "NewLevelData", menuName = "CookOrPanic/Level Data")]
    public class LevelData : ScriptableObject
    {
        [Header("Level Configurations")]
        public List<Recipe> _availableRecipes; // Resep yang muncul di level ini
        public float _levelTimeLimit = 120f;   // Batas waktu dalam detik
        public int _targetScore = 14;          // Skor target untuk naik level
    }
}