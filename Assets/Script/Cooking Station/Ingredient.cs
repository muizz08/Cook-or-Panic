using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CookOrPanic.Ingredient
{
    public enum IngredientType
    {
        Telur,
        DagingGiling,
        Terigu,
        Air,
        BawangMerah,
        BawangPutih,
        Gula,
        Garam,
        PenyedapRasa,
        BackingPowder,
        Kentang, 
        Santan,
        KacangHijau,
        TepungRoti,
        TepungBeras

    }
    public class Ingredient : MonoBehaviour
    {
        public IngredientType _ingredientType; 


        public IngredientType GetIngredientType()
        {
            return _ingredientType;
        }
    }
}