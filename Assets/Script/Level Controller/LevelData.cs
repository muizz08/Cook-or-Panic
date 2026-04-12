    using System.Collections.Generic;
    using UnityEngine;
    using UnityEngine.UI; // Tambahkan ini jika perlu, tapi untuk ScriptableObject biasanya Sprite saja cukup

    namespace CookOrPanic.LevelData
    {
        using CookOrPanic.Recipe;

        [CreateAssetMenu(fileName = "NewLevelData", menuName = "CookOrPanic/Level Data")]
        public class LevelData : ScriptableObject
        {


            [Header("Level Configurations")]
            public string _levelName;              // Nama level (opsioal)
            public Sprite _levelPreviewImage;      // Foto/Gambar untuk level ini
            public List<Recipe> _availableRecipes;
            public int _targetScore = 14;
        }
    }