using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace CookOrPanic.CanvasManager
{

    using CookOrPanic.Food; // Pastikan namespace Food tersedia
    using CookOrPanic.Timer;

    public class CanvasManager : MonoBehaviour
    {
        [Header("UI Images")]
        [SerializeField] public Image _scoreDisplayImage; // Komponen Image pada UI yang akan menampilkan gambar angka
        [SerializeField] public Sprite[] _scoreSprites;   // Masukkan sprite angka 6-20 di sini (Index 0 = angka 6)

        [SerializeField]private TMP_Text _timerText;
        [SerializeField]private Slider _angerBar;
        //private Panel _panel;
        [SerializeField]private Timer _timer;

        [Header("Food Notification Settings")]
        [SerializeField] private Image _foodIconDisplay; // Image UI tempat gambar notif muncul
        [SerializeField] private GameObject _notificationPanel; // Parent object notif (opsional)

        public FoodType CurrentTargetFood { get; private set; }

        [System.Serializable]
        public struct FoodUIcon
        {
            public FoodType type; // Onde, Pempek, dll
            public Sprite icon;   // Gambar Sprite-nya
        }

        [SerializeField] private List<FoodUIcon> _foodIcons; // Daftar mapping di Inspector

        public void UpdateScoreUI(int score)
        {

            int index = score - 6;
            if (score < 6)
            {
                if (_scoreDisplayImage != null) _scoreDisplayImage.enabled = false;
                return;
            }

            if (index >= 0 && index < _scoreSprites.Length && _scoreDisplayImage != null)
            {
                _scoreDisplayImage.sprite = _scoreSprites[index];
                _scoreDisplayImage.enabled = true;
            }

            _scoreDisplayImage.transform.localScale = Vector3.zero;
            _scoreDisplayImage.transform.DOScale(Vector3.one, 0.5f).SetEase(Ease.OutBack);
        }
     
        public void UpdateUITimer(float time)
        {
            _timerText.text = time.ToString("F1") + "s";
        }

        public void UpdateAngerBar(float angerValue)
        {

        }
        public void ShowRandomOrder()
        {
            if (_foodIcons.Count == 0) return;

            // 1. Reset logika Timer internal
            if (_timer != null)
            {
                _timer.StopTimer(); // Ini akan membuat _currentTime = 0 di script Timer
            }

            // 2. PAKSA Update UI ke angka 0 secara manual
            // Tanpa ini, teks UI mungkin masih menampilkan angka terakhir dari pesanan sebelumnya
            UpdateUITimer(0f);

            // --- Sisa kode acak pesanan ---
            int randomIndex = Random.Range(0, _foodIcons.Count);
            FoodUIcon randomOrder = _foodIcons[randomIndex];
            CurrentTargetFood = randomOrder.type;

            if (_foodIconDisplay != null)
            {
                _foodIconDisplay.sprite = randomOrder.icon;
                if (_notificationPanel != null) _notificationPanel.SetActive(true);
                _foodIconDisplay.transform.localScale = Vector3.zero;
                _foodIconDisplay.transform.DOScale(Vector3.one, 0.5f).SetEase(Ease.OutBack);
            }
        }

        public void UpdateFoodIcon(FoodType type)
        {
            // Cari ikon yang sesuai dengan tipe
            FoodUIcon match = _foodIcons.Find(x => x.type == type);

            if (match.icon != null && _foodIconDisplay != null)
            {
                _foodIconDisplay.sprite = match.icon;
                if (_notificationPanel != null) _notificationPanel.SetActive(true);

                // Bonus: Animasi sedikit biar munculnya smooth pakai DOTween
                _foodIconDisplay.transform.DOPunchScale(Vector3.one * 0.2f, 0.3f);
            }
        }

        public void HideFoodIcon()
        {
            if (_notificationPanel != null) _notificationPanel.SetActive(false);
        }

        public void PanelControl()
        {
            //_panel._panelImage.DOKill();
            //_panel._panelImage.rectTransform.DOKill();
            //switch (_panel._panelType)
            //{
            //    case Panel.PanelType.PanelPanduan:
            //        _panel._panelImage.transform
            //            .DOScale(Vector3.zero, 0.3f)
            //            .SetEase(Ease.InBack)
            //            .OnComplete(() => _panel._panelImage.gameObject.SetActive(false));
            //        break;

            //    case Panel.PanelType.PanelResep:
            //        _panel._panelImage
            //            .DOFade(0f, 0.3f)
            //            .SetEase(Ease.Linear)
            //            .OnComplete(() => _panel._panelImage.gameObject.SetActive(false));
            //        break;

            //    case Panel.PanelType.PanelKlikPanduan:
            //        _panel._panelImage.rectTransform
            //            .DOAnchorPos(new Vector2(-800, 0), 0.3f)
            //            .SetEase(Ease.InCubic)
            //            .OnComplete(() => _panel._panelImage.gameObject.SetActive(false));
            //        break;

            //    case Panel.PanelType.PanelKlikResep:
            //        _panel._panelImage.rectTransform
            //            .DOAnchorPos(new Vector2(800, 0), 0.3f)
            //            .SetEase(Ease.InCubic)
            //            .OnComplete(() => _panel._panelImage.gameObject.SetActive(false));
            //        break;
            //}
        }

        public void ShowGameOver()
        {

        }
    }
}