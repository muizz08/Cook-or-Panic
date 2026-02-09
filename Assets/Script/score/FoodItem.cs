using System.Collections;
using System.Collections.Generic;
using UnityEngine;



namespace CookOrPanic.FoodItem 
{
   using CookOrPanic.FoodTimer;

    public class FoodItem : FoodBase
    {
        [SerializeField]private FoodTimer foodTimer;
        [SerializeField]private float COOK_TIME;

        [Header("Score Settings")]
        [SerializeField] private int cookedScore;
        [SerializeField] private int rawScore;
        [SerializeField] private int burnedScore;

        public override int GetScore()
        {
            if (isBurned)
                return burnedScore += baseScore;

            if (isCooked)
                return cookedScore;

            return rawScore += baseScore;
        }

        public void OnRemovedFromSocket()
        {
            foodTimer.StopCooking();
            ScoreFromSocket();
        }

    }
}

