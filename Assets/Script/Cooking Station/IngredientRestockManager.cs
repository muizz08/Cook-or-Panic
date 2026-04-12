using System.Collections;
using System.Collections.Generic;
using UnityEngine;



namespace CookOrPanic.IngredientRestockManager
{
    using CookOrPanic.Ingredient;

    [System.Serializable]
    public class IngredientGroup
    {
        public IngredientType type;
        public List<GameObject> allIngredients;

        // List untuk menyimpan posisi dan rotasi asli setiap objek di wadah
        [HideInInspector] public List<Vector3> savedPositions = new List<Vector3>();
        [HideInInspector] public List<Quaternion> savedRotations = new List<Quaternion>();
    }

    public class IngredientRestockManager : MonoBehaviour
    {
        public static IngredientRestockManager Instance;
        public List<IngredientGroup> ingredientGroups;

        void Awake()
        {
            Instance = this;
            SaveInitialTransforms();
        }

        // Mencatat posisi awal semua bahan di dalam wadah agar restok bisa rapi
        private void SaveInitialTransforms()
        {
            foreach (var group in ingredientGroups)
            {
                foreach (GameObject obj in group.allIngredients)
                {
                    group.savedPositions.Add(obj.transform.position);
                    group.savedRotations.Add(obj.transform.rotation);
                }
            }
        }

        public void HandleIngredientUsed(GameObject ingredient, IngredientType type)
        {
            IngredientGroup group = ingredientGroups.Find(g => g.type == type);
            if (group != null)
            {
                // Cari index bahan ini di dalam list grupnya
                int index = group.allIngredients.IndexOf(ingredient);

                if (index != -1)
                {
                    // Kembalikan ke posisi aslinya di dalam wadah sebelum dimatikan
                    ingredient.transform.position = group.savedPositions[index];
                    ingredient.transform.rotation = group.savedRotations[index];
                }

                ingredient.SetActive(false);
                Debug.Log($"{ingredient.name} masuk adonan dan kembali ke wadah (hidden).");
            }
        }

        public bool CanRestock(IngredientType type)
        {
            IngredientGroup group = ingredientGroups.Find(g => g.type == type);
            if (group == null) return false;

            foreach (GameObject obj in group.allIngredients)
            {
                if (obj.activeSelf) return false;
            }
            return true;
        }

        public void Restock(IngredientType type)
        {
            // Pengecekan: Hanya restok jika SEMUA bahan di wadah sudah habis (false)
            if (!CanRestock(type))
            {
                Debug.LogWarning($"Belum bisa restok {type}, masih ada bahan di meja!");
                return;
            }

            IngredientGroup group = ingredientGroups.Find(g => g.type == type);
            for (int i = 0; i < group.allIngredients.Count; i++)
            {
                GameObject obj = group.allIngredients[i];

                // Pastikan posisi benar-benar di slotnya lagi
                obj.transform.position = group.savedPositions[i];
                obj.transform.rotation = group.savedRotations[i];

                if (obj.TryGetComponent(out Rigidbody rb))
                {
                    rb.velocity = Vector3.zero; // Gunakan ini untuk Unity 2022 ke bawah
                    rb.angularVelocity = Vector3.zero;
                }

                obj.SetActive(true);
            }
            Debug.Log($"Wadah {type} telah diisi ulang sepenuhnya.");
        }

        public void ButtonRestock_Tepung()
        {
            Restock(IngredientType.TepungKetan);
        }
    }
}