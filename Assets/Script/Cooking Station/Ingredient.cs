using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CookOrPanic.Ingredient
{
    public enum IngredientType
    {
        Telur,
        AyamGiling,
        IkanGiling,
        AyamMentah,
        IkanMentah,
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
        TepungBeras,
        Wortel,
        Wijen,
        TepungKetan,
        Merica

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