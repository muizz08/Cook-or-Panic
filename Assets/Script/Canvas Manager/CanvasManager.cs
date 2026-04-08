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
    using CookOrPanic.CookingStation;
    using CookOrPanic.Panel;

    public class CanvasManager : MonoBehaviour
    {
        [SerializeField] private Panel _panel;
        [Header("UI Images")]
        [SerializeField] public Image _scoreDisplayImage; // Komponen Image pada UI yang akan menampilkan gambar angka
        [SerializeField] public Sprite[] _scoreSprites;   // Masukkan sprite angka 6-20 di sini (Index 0 = angka 6)

        [Header("UI Masak")]
        [SerializeField]private TMP_Text _timerText;
        [SerializeField]private Slider _angerBar;
        [SerializeField]private Timer _timerCooking;

        [Header("Book Timer UI")]
        [SerializeField] private GameObject _pemberitahuanBook;
        [SerializeField] private GameObject _bookTimerPanel;
        [SerializeField] private TMP_Text _TimerBook;


        [Header("Food Notification Settings")]
        [SerializeField] private Image _foodIconDisplay; // Image UI tempat gambar notif muncul
        [SerializeField] private GameObject _notificationPanel; // Parent object notif (opsional)

        [SerializeField] private TMP_Text _TimerPesanan;
        [SerializeField] private float _lamaPesanan;

        private int _recipeOpenCount = 0; // Penghitung jumlah buka resep
        [SerializeField] private int _maxRecipeOpen = 3; // Batas maksimal


        [SerializeField] private CookingStation _cookingStation;
        private Coroutine _activeOrderCoroutine;

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
            UIAnimator.Show(_pemberitahuanBook, UIAnimator.AnimationType.Scale);
          
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
            StartOrderTimer(_lamaPesanan);
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

        // Tambahkan ini di CanvasManager.cs
        public void StopBookTimer()
        {
            // 1. Berhentikan logika internal timer
            if (_timerCooking != null)
            {
                _timerCooking.StopTimer();
            }

            // 2. Hentikan Coroutine yang sedang berjalan (PanelTimerRoutine)
            StopCoroutine("PanelTimerRoutine");

            // 3. Sembunyikan panel UI Timer-nya
            if (_bookTimerPanel != null)
            {
                UIAnimator.Hide(_bookTimerPanel, UIAnimator.AnimationType.Scale);
            }
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

            // --- LOGIKA MENGGUNAKAN VARIABEL _PANEL ---
            if (_timerCooking != null && _timerCooking.GetCurrentTime() <= 0.1f)
            {
                // Pastikan variabel _panel tidak kosong (sudah diisi di Inspector)
                if (_panel != null)
                {
                    // Panggil fungsi penguncian langsung ke variabel tersebut
                    _panel.ForceLockPanel();
                }

                Debug.Log("<color=orange>[CanvasManager]</color> Timer Habis! Buku dikunci melalui variabel _panel.");
            }
        }


        public void StartOrderTimer(float duration)
        {
            if (_activeOrderCoroutine != null) StopCoroutine(_activeOrderCoroutine);
            _activeOrderCoroutine = StartCoroutine(OrderCountdownRoutine(duration));
        }

        private IEnumerator OrderCountdownRoutine(float duration)
        {
            float remainingTime = duration;

            while (remainingTime > 0)
            {
                // Update teks UI setiap detik
                if (_TimerPesanan != null)
                {
                    _TimerPesanan.text = remainingTime.ToString("F0") + "s";

                    // Efek visual tambahan: jika waktu sisa sedikit (misal < 10s), teks jadi merah
                    _TimerPesanan.color = (remainingTime <= 10f) ? Color.red : Color.black;
                }

                yield return new WaitForSeconds(1f);
                remainingTime -= 1f;
            }

            // Memastikan teks menunjukkan angka 0 saat habis
            if (_TimerPesanan != null) _TimerPesanan.text = "0s";

            // --- WAKTU HABIS ---
            OnOrderExpired();
        }

      
        private void OnOrderExpired()
        {
            // 1. Beritahu CookingStation untuk menghancurkan bahan
            if (_cookingStation != null)
            {
                _cookingStation.FailOrderDueToTime();
            }

            // 2. Logika Emosi Head Chef (Tingkatkan Anger Bar)
            // Misal: UpdateAngerBar(0.2f); 

            Debug.Log("<color=red>Waktu Habis!</color>");

            // Opsional: Langsung ganti ke pesanan baru setelah gagal
            ShowRandomOrder();
        }

        // Panggil ini jika pemain berhasil menyelesaikan pesanan sebelum waktu habis
        public void StopOrderTimer()
        {
            if (_activeOrderCoroutine != null)
            {
                StopCoroutine(_activeOrderCoroutine);
                // Kosongkan teks saat timer berhenti (opsional)
                if (_TimerPesanan != null) _TimerPesanan.text = "";
            }
        }
        public void PanelControl()
        {
          
        }

        public void ShowGameOver()
        {

        }
    }
}