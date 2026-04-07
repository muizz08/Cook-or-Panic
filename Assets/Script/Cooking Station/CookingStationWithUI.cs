using UnityEngine;
using UnityEngine.UI;

namespace CookOrPanic.CookingStationWithUI
{
    using CookOrPanic.CookingStation;
    public class CookingStationWithUI : CookingStation
    {
        // Tambahkan di bagian Header References
        [Header("UI Checklist & Feedback")]
        [SerializeField] private GameObject _checklistPrefab;
        [SerializeField] private Transform _checklistParent;
        [SerializeField] private ScrollRect _checklistScrollRect;
        [SerializeField] private CanvasGroup _wrongIngredientPopup; // Popup "Bahan Salah"

        [Header("UI Panels")]
        [SerializeField] private Panel.Panel _tombolKlikResep;
        [SerializeField] private Panel.Panel _isiPanelResep;


    }

}
