using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace CookOrPanic.Food
{

    using CookOrPanic.ProcessedIngredient;
    using CookOrPanic.Recipe;

    public enum FoodType
    {
        Onde,
        Pempek, 
        Kroket
    }
    public class Food : MonoBehaviour
    {
        public float _timeSpentCooking;
        public float _targetCookTime; // Waktu saat makanan mencapai status Cooked (untuk referensi jika mau buat efek khusus saat matang)
        public FoodType _foodType;
        public FoodState _foodState;
        public Recipe _recipeOrigin;
        [Header("Visual Settings")]
        [SerializeField] private Material _cookedMaterial;    // Material saat matang
        [SerializeField] private Material _overCookedMaterial; // Material saat gosong




        public void Initialze(FoodType type, FoodState state, Recipe recipe)
        {
            _foodType = type;
            _foodState = state;
            _recipeOrigin = recipe;
            _timeSpentCooking = 0f;
            

            Debug.Log($"Makanan {type} diinisialisasi dengan state: {state}"); // <--- Tambah ini
        }

        public int GetPoint()
        {
            return 0;
        }

        public void UpdateStateBasedOnTime(float duration)
        {

            // LOGIKA SATU-SATUNYA YANG MENENTUKAN STATUS
            if (_timeSpentCooking < _targetCookTime)
            {
                _foodState = FoodState.Raw;
            }
            // 2. MATANG: Waktu masak sudah lewat target, tapi belum lewat batas toleransi (2 detik)
            else if (_timeSpentCooking >= _targetCookTime && _timeSpentCooking <= _targetCookTime + 2f)
            {
                _foodState = FoodState.Cooked;
            }
            // 3. GOSONG: Waktu masak sudah lebih dari (target + 2 detik)
            else
            {
                _foodState = FoodState.OverCooked;
            }
            ApplyVisualStateToChildren();

            Debug.Log($"[Food] Status {this.name} sekarang: {_foodState} (Waktu: {duration}s)");
        }

        private void ApplyVisualStateToChildren()
        {
            // --- INI KUNCINYA UNTUK HIRARKI SEPERTI DI GAMBAR ---
            // Ambil semua renderer di objek Onde-Onde (Parent) dan semua anaknya (Mball, Sphere)
            Renderer[] renderers = GetComponentsInChildren<Renderer>();
            Material targetMat = null;

            // Pilih material mana yang akan dipakai
            if (_foodState == FoodState.Cooked) targetMat = _cookedMaterial;
            else if (_foodState == FoodState.OverCooked) targetMat = _overCookedMaterial;

            // Jika ada material target, ganti material di semua Renderer (termasuk Child)
            if (targetMat != null)
            {
                foreach (Renderer rend in renderers)
                {
                    // Membuat array baru berisi material target untuk menimpa semua slot material (biar gak belang-belang)
                    Material[] newMats = new Material[rend.sharedMaterials.Length];
                    for (int i = 0; i < newMats.Length; i++)
                    {
                        newMats[i] = targetMat;
                    }
                    rend.materials = newMats;
                }
            }
        }

        

    }
}


