using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CookOrPanic.Ingredient
{
    public enum IngredientType
    {
        Telur,
        IkanGiling,
        Tepung,
        Air,
        BawangMerah,
        BawangPutih,
        Gula,
        Garam,
        PenyedapRasa,
        BackingPowder,
        Kentang, 
        Santan
    }
    public class Ingredient : MonoBehaviour
    {
        public IngredientType _ingredientType; 


        public void GetIngredientType()
        {

        }
    }
}