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
    using CookOrPanic.UIAnimator;
    using CookOrPanic.TutorialManager;

    public class CanvasManager : MonoBehaviour
    {
        [Header("UI Images")]
        [SerializeField] public Image _scoreDisplayImage; // Komponen Image pada UI yang akan menampilkan gambar angka
        [SerializeField] public Sprite[] _scoreSprites;   // Masukkan sprite angka 6-20 di sini (Index 0 = angka 6)

        [Header("UI Masak")]
        [SerializeField]private TMP_Text _timerText;
        [SerializeField]private Slider _angerBar;
        [SerializeField]private Timer _timerCooking;

        [Header("Book Timer UI")]
        [SerializeField] private GameObject _bookTimerPanel;
        [SerializeField] private TMP_Text _TimerBook;


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


        private void Start()
        {
            // Memastikan saat game mulai, pesanan pertama langsung diacak
            ShowRandomOrder();
        }

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

                // Reset scale sebelum animasi agar konsisten
                _scoreDisplayImage.transform.localScale = Vector3.zero;
                _scoreDisplayImage.transform.DOScale(Vector3.one, 0.5f).SetEase(Ease.OutBack);
            }
        }   

        public void UpdateUITimerCooking(float time)
        {
            _timerText.text = time.ToString("F1") + "s";

        }

        public void UpdateUITimerBook(float time)
        {
            _timerCooking.TimerBook(Time.deltaTime);
        }

        public void UpdateAngerBar(float angerValue)
        {

        }
        public void ShowRandomOrder()
        {
            // 🔥 STOP kalau masih tutorial
            if (TutorialManager.Instance != null && TutorialManager.Instance._isTutorialMode)
            {
                Debug.Log("<color=yellow>[CanvasManager]</color> Skip random order (Tutorial Mode aktif)");
                return;
            }

            if (_foodIcons.Count == 0) return;

            if (_timerCooking != null)
            {
                _timerCooking.StopTimer();
            }

            UpdateUITimerCooking(0f);

            int randomIndex = Random.Range(0, _foodIcons.Count);
            FoodUIcon randomOrder = _foodIcons[randomIndex];
            CurrentTargetFood = randomOrder.type;

            Debug.Log($"<color=green>[ORDER]</color> Pesanan Baru: <b>{CurrentTargetFood}</b>");

            if (_foodIconDisplay != null)
            {
                _foodIconDisplay.sprite = randomOrder.icon;

                if (_notificationPanel != null)
                    _notificationPanel.SetActive(true);

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


        /// <summary>
        /// Memulai hitung mundur untuk Panel (misal: Resep tertutup dalam 5 detik)
        /// </summary>
        public void StartPanelTimer(float duration)
        {
            if (_timerCooking != null)
            {
                _timerCooking.StartTimer(duration, Timer.TimerMode.CountDown); // ✅ WAJIB
            }

            StopCoroutine("PanelTimerRoutine");
            StartCoroutine(PanelTimerRoutine());
        }

        private IEnumerator PanelTimerRoutine()
        {
            UIAnimator.Show(_bookTimerPanel, UIAnimator.AnimationType.Scale);

            while (_timerCooking != null && _timerCooking._isRunning)
            {
                _timerCooking.TimerBook(Time.deltaTime);

                if (_TimerBook != null)
                {
                    _TimerBook.text = _timerCooking.GetCurrentTime().ToString("F0");
                }

                yield return null;
            }

            UIAnimator.Hide(_bookTimerPanel, UIAnimator.AnimationType.Scale);
        }
        public void PanelControl()
        {
          
        }

        public void ShowGameOver()
        {

        }
    }
}