using UnityEngine;
using System.Collections.Generic;
using CookOrPanic.Ingredient;

[CreateAssetMenu(fileName = "IngredientReferenceData", menuName = "ScriptableObjects/IngredientReferenceData")]
public class IngredientReferenceData : ScriptableObject
{
    [System.Serializable]
    public struct IngredientMap
    {
        public IngredientType type;
        // Sekarang menggunakan List agar 1 Enum bisa punya banyak titik target
        public List<GameObject> targetPoints;
    }

    public List<IngredientMap> ingredientMaps = new List<IngredientMap>();

    // Fungsi untuk mendapatkan target yang paling dekat dengan posisi objek saat ini
    public Transform GetNearestTarget(IngredientType type, Vector3 currentPosition)
    {
        foreach (var item in ingredientMaps)
        {
            if (item.type == type && item.targetPoints != null && item.targetPoints.Count > 0)
            {
                Transform bestTarget = null;
                float closestDistanceSqr = Mathf.Infinity;

                // Loop semua target yang terdaftar untuk enum ini
                foreach (GameObject point in item.targetPoints)
                {
                    if (point == null) continue;

                    Vector3 directionToTarget = point.transform.position - currentPosition;
                    float dSqrToTarget = directionToTarget.sqrMagnitude;
                    if (dSqrToTarget < closestDistanceSqr)
                    {
                        closestDistanceSqr = dSqrToTarget;
                        bestTarget = point.transform;
                    }
                }
                return bestTarget;
            }
        }
        return null;
    }
}