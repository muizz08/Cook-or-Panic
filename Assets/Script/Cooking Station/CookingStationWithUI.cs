using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using DG.Tweening;

namespace CookOrPanic.CookingStationWithUI
{
    using CookOrPanic.UIAnimator;
    using CookOrPanic.CookingStation;
    using CookOrPanic.TutorialManager;
    using CookOrPanic.ProcessedIngredient; // Pastikan namespace ini ada
    using CookOrPanic.AudioManager;      // Pastikan namespace ini ada
    using CookOrPanic.Ingredient;
    using CookOrPanic.Panel;


    public class CookingStationWithUI : CookingStation
    {

        [Header("UI Panels")]
        [SerializeField] private Panel _panelRecipe;

        [Header("UI Checklist & Feedback")]
        [SerializeField] private GameObject _checklistPrefab;
        [SerializeField] private Transform _checklistParent;
        [SerializeField] private ScrollRect _checklistScrollRect;
        [SerializeField] private CanvasGroup _wrongIngredientPopup;

        [Header("UI Peringatan (DOTween)")]
        [SerializeField] private CanvasGroup _warningOnStoveGas;
        private Dictionary<IngredientType, GameObject> _activeChecklistUI = new Dictionary<IngredientType, GameObject>();

        // --- OVERRIDE LOGIKA UI SAAT BAHAN BENAR ---
        // Ganti virtual menjadi override
        protected override void HandleCorrectIngredient(ProcessedIngredient ingredient, string nameToDisplay, IngredientType pType = default)
        {
            // 1. Jalankan semua logika dasar (tambah data ke list, spawn gundukan, hancurkan objek)
            base.HandleCorrectIngredient(ingredient, nameToDisplay, pType);

            // 2. Ambil Tipe Bahan untuk keperluan Dictionary UI
            // Jika bumbu (ingredient null), gunakan pType. Jika bahan fisik, gunakan GetIngredientType()
            IngredientType type = (ingredient != null) ? ingredient.GetIngredientType() : pType;

            // 3. Jalankan logika UI Checklist
            HandleUIOnCorrectIngredient(type, nameToDisplay);
        }

        private void HandleUIOnCorrectIngredient(IngredientType type, string displayName)
        {
            bool isTutorial = TutorialManager.Instance != null && TutorialManager.Instance._isTutorialMode;

            if (!_activeChecklistUI.ContainsKey(type) && !isTutorial)
            {
                GameObject newChecklist = Instantiate(_checklistPrefab, _checklistParent);

                // Set Text
                var textMesh = newChecklist.GetComponentInChildren<TMPro.TextMeshProUGUI>();
                if (textMesh != null) textMesh.text = displayName;

                // Reset Scale & Rebuild Layout (agar posisi scroll rect benar)
                newChecklist.transform.localScale = Vector3.zero;
                Canvas.ForceUpdateCanvases();
                LayoutRebuilder.ForceRebuildLayoutImmediate(_checklistParent.GetComponent<RectTransform>());

                // Animasi Muncul
                newChecklist.transform.DOKill();
                newChecklist.transform.DOScale(Vector3.one, 0.3f).SetEase(Ease.OutBack);

                _activeChecklistUI.Add(type, newChecklist);

                // Auto-scroll ke bawah jika menggunakan ScrollRect
                if (_checklistScrollRect != null)
                {
                    _checklistScrollRect.verticalNormalizedPosition = 0;
                }
            }
        }

        // --- OVERRIDE LOGIKA UI SAAT BAHAN SALAH ---
        protected override void ShowWrongVisualFeedback()
        {
            // 2. Animasi Popup "Bahan Salah"
            if (_wrongIngredientPopup != null)
            {
                UIAnimator.Show(_wrongIngredientPopup.gameObject, UIAnimator.AnimationType.PulseFade);
                DOVirtual.DelayedCall(2.0f, () => { // 2 detik = durasi PulseFade + durasi tampil
                    UIAnimator.Hide(_wrongIngredientPopup.gameObject, UIAnimator.AnimationType.Fade);
                });
            }
        }

        protected override void ShowStoveWarning()
        {
            if (_warningOnStoveGas != null)
            {
                UIAnimator.Show(_warningOnStoveGas.gameObject, UIAnimator.AnimationType.PulseFade);
            }
        }

        // OVERRIDE untuk menyembunyikan warning
        protected override void HideStoveWarning()
        {
            if (_warningOnStoveGas != null)
            {
                UIAnimator.Hide(_warningOnStoveGas.gameObject, UIAnimator.AnimationType.Fade);
            }
        }

        // --- OVERRIDE SAAT PEMBERSIHAN STATION ---
        protected override void ClearStation()
        {
            // 1. Jalankan logika bersih-bersih data di Parent
            base.ClearStation();

            // 2. Bersihkan UI Checklist
            foreach (var ui in _activeChecklistUI.Values)
            {
                if (ui != null) Destroy(ui);
            }
            _activeChecklistUI.Clear();

            // 3. Reset Panel Lock (khusus UI)
         
            if (_panelRecipe != null) _panelRecipe.ResetPanelLock();

            Debug.Log("<color=cyan>UI Station Cleared.</color>");
        }
    }
}