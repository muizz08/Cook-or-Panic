using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace CookOrPanic.IngredientPasser
{
    using CookOrPanic.CookingStation;
    public class IngredientPasser : MonoBehaviour
    {
        [HideInInspector] public CookingStation currentStation;

        private void OnParticleCollision(GameObject other)
        {
            if (currentStation != null)
            {
                currentStation.OnParticleCollision(other);
            }
            else
            {
                // Ini warning yang kamu dapatkan
                Debug.LogWarning($"[Passer] {gameObject.name} terkena bumbu tapi currentStation NULL.");
            }
        }

        // Tambahkan ini: Jika benda dicabut, hapus referensi station
        public void ClearStation() => currentStation = null;
    }

}
