using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using DG.Tweening;

namespace CookOrPanic.CookingStationWithUI
{
    using CookOrPanic.CookingStation;
    using CookOrPanic.TutorialManager;
    using CookOrPanic.ProcessedIngredient; // Pastikan namespace ini ada
    using CookOrPanic.AudioManager;      // Pastikan namespace ini ada

    public class CookingStationWithUI : CookingStation
    {
        [Header("UI Checklist & Feedback")]
        [SerializeField] private GameObject _checklistPrefab;
        [SerializeField] private Transform _checklistParent;
        [SerializeField] private ScrollRect _checklistScrollRect;
        [SerializeField] private CanvasGroup _wrongIngredientPopup;

        [Header("UI Panels")]
      
        [SerializeField] private Panel.Panel _isiPanelResep;

        private Dictionary<Ingredient.IngredientType, GameObject> _activeChecklistUI = new Dictionary<Ingredient.IngredientType, GameObject>();

        // --- OVERRIDE LOGIKA UI SAAT BAHAN BENAR ---
        protected override void HandleCorrectIngredient(ProcessedIngredient ingredient, string displayName)
        {
            // 1. Jalankan logika dasar dari Parent (tambah data ke list, dsb)
            base.HandleCorrectIngredient(ingredient, displayName);

            // 2. Jalankan logika UI khusus script ini
            HandleUIOnCorrectIngredient(ingredient.GetIngredientType(), displayName);
        }

        private void HandleUIOnCorrectIngredient(Ingredient.IngredientType type, string displayName)
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
            // 1. Play Error Sound
            if (AudioManager.Instance != null)
            {
                AudioManager.Instance.PlaySFX("ErrorBahan");
            }

            // 2. Animasi Popup "Bahan Salah"
            if (_wrongIngredientPopup != null)
            {
                _wrongIngredientPopup.DOKill();
                _wrongIngredientPopup.alpha = 0;
                _wrongIngredientPopup.gameObject.SetActive(true);

                Sequence s = DOTween.Sequence();
                s.Append(_wrongIngredientPopup.DOFade(1, 0.2f));
                s.AppendInterval(1.5f);
                s.Append(_wrongIngredientPopup.DOFade(0, 0.5f));
                s.OnComplete(() => _wrongIngredientPopup.gameObject.SetActive(false));
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
         
            if (_isiPanelResep != null) _isiPanelResep.ResetPanelLock();

            Debug.Log("<color=cyan>UI Station Cleared.</color>");
        }
    }
}