using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CookOrPanic.LevelData
{
    using CookOrPanic.Recipe;
    public class LevelData : ScriptableObject
    {
        private List<Recipe> _availableController;
        private float _levelTimeLimit;
        private int _targerScore;

    }
}