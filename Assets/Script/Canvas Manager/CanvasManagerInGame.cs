using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;


namespace CookOrPanic.CanvasManagerInGame
{
    using CookOrPanic.CanvasManager;
    using CookOrPanic.Panel;
    using CookOrPanic.UIAnimator;
    using CookOrPanic.TutorialManager;
    using CookOrPanic.Food;
    using CookOrPanic.LevelData;
    using CookOrPanic.AudioManager;

    public class CanvasManagerInGame : CanvasManager
    {
        [SerializeField] private Panel _panel;
        [SerializeField] private Slider _angerBar;
        [Header("Book Timer UI")]
        [SerializeField] private GameObject _pemberitahuanBook;
        [SerializeField] private GameObject _bookTimerPanel;
        [SerializeField] private TMP_Text _TimerBook;

        [Header("Food Notification Settings")]
        [SerializeField] private Image _foodIconDisplay;
        [SerializeField] private GameObject _notificationPanel;

        [SerializeField] private TMP_Text _TimerPesanan;
        [SerializeField] private float _lamaPesanan;

        [Header("Level Display Settings")]
        [SerializeField] private Image _levelPhotoDisplay;
        [SerializeField] private TMP_Text _levelNameText;


        private Coroutine _activeOrderCoroutine;
        private int _lastSecondBeeped = -1;


        protected override void Start()
        {
            base.Start();
        }

        protected override void Update()
        {
            UpdateUITimerOrder();
        }
        public override void StartPanelTimer(float duration)
        {
            _timerController.StartBookTimer(duration);
            StopCoroutine("PanelTimerRoutine");
            StartCoroutine(PanelTimerRoutine());
        }

        public override void StopBookTimer()
        {
            // 1. Berhentikan logika internal timer
            if (_timerController != null)
            {
                _timerController.StopBookTimer();
            }

            // 2. Hentikan Coroutine yang sedang berjalan (PanelTimerRoutine)
            StopCoroutine("PanelTimerRoutine");

            // 3. Sembunyikan panel UI Timer-nya
            if (_bookTimerPanel != null)
            {
                UIAnimator.Hide(_bookTimerPanel, UIAnimator.AnimationType.Scale);
            }
        }

        public override void StopOrderTimer()
        {
            if (_activeOrderCoroutine != null)
            {
                StopCoroutine(_activeOrderCoroutine);
                // Kosongkan teks saat timer berhenti (opsional)
                if (_TimerPesanan != null) _TimerPesanan.text = "";
            }
        }

        public override void UpdateScoreUI(int score)
        {
            if (_totalScoreText != null)
            {
                // Ambil angka yang sekarang tampil di text (parsing dari string ke int)
                int currentDisplayedScore;
                if (!int.TryParse(_totalScoreText.text, out currentDisplayedScore))
                {
                    currentDisplayedScore = 0;
                }

                // Efek Angka Berjalan (Counting Effect)
                DOTween.To(() => currentDisplayedScore, x => currentDisplayedScore = x, score, 0.5f)
                    .OnUpdate(() => {
                        _totalScoreText.text = currentDisplayedScore.ToString();
                    });

                // Efek Hentakan (Punch Scale) agar teks terasa lebih hidup saat bertambah
                _totalScoreText.transform.DOPunchScale(Vector3.one * 0.2f, 0.3f);
            }
        }

        public override void SetupLevelUI(LevelData levelData)
        {
            if (levelData == null) return;

            // 1. Tampilkan Foto Level
            if (_levelPhotoDisplay != null && levelData._levelPreviewImage != null)
            {
                _levelPhotoDisplay.sprite = levelData._levelPreviewImage;
                _levelPhotoDisplay.transform.localScale = Vector3.zero;
                _levelPhotoDisplay.transform.DOScale(Vector3.one, 0.5f).SetEase(Ease.OutBack);
            }

            // 2. PERBAIKAN DI SINI:
            // Gunakan levelData._levelName (sesuai variabel di ScriptableObject kamu)
            if (_levelNameText != null)
            {
                // Sebelumnya: _levelNameText.text = levelData.name;
                _levelNameText.text = levelData._levelName;
            }

            // 3. Update daftar resep
            UpdateAvailableRecipes(levelData._availableRecipes);
        }


        public override void ShowRandomOrder()
        {
            UIAnimator.Show(_pemberitahuanBook, UIAnimator.AnimationType.Scale);

            // 🔥 STOP kalau masih tutorial
            if (TutorialManager.Instance != null && TutorialManager.Instance._isTutorialMode)
            {
                Debug.Log("<color=yellow>[CanvasManager]</color> Skip random order (Tutorial Mode aktif)");
                return;
            }

            if (_foodIcons.Count == 0) return;

            if (_timerController != null)
            {
                _timerController.StartOrderTimer(_lamaPesanan);
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
        private IEnumerator PanelTimerRoutine()
        {
            if (_bookTimerPanel != null) _bookTimerPanel.SetActive(true);

            // Selama timer buku di script Timer masih jalan
            while (_timerController.IsBookRunning())
            {
                _TimerBook.text = _timerController.GetBookTimeRemaining().ToString("F0");
                yield return null;
            }

            // Saat berhenti (Waktu Habis)
            if (_bookTimerPanel != null) _bookTimerPanel.SetActive(false);

            if (_panel != null) _panel.ForceLockPanel();
        }

        private void UpdateUITimerOrder()
        {
            float remaining = _timerController.GetOrderRatio() * _lamaPesanan;
            int currentSecond = Mathf.CeilToInt(remaining); // Ambil angka detiknya saja (misal: 10, 9, 8)

            if (remaining > 0)
            {
                _TimerPesanan.text = currentSecond.ToString() + "s";

                // Atur warna
                if (remaining <= 10f)
                {
                    _TimerPesanan.color = Color.red;
                }
                else
                {
                    _TimerPesanan.color = Color.black;
                }

                // --- LOGIKA SUARA: Hanya bunyi jika detik berganti ---
                if (currentSecond != _lastSecondBeeped)
                {
                    _lastSecondBeeped = currentSecond; // Simpan detik saat ini agar tidak bunyi lagi di frame berikutnya

                    if (remaining <= 10f)
                    {
                        AudioManager.Instance.PlaySFX("TimerWarning");
                    }
                    else
                    {
                        AudioManager.Instance.PlaySFX("Timer");
                    }
                }
            }
            else
            {
                _TimerPesanan.text = "0s";
                // Reset variabel jika pesanan baru dimulai lagi nanti
                _lastSecondBeeped = -1;
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

    }


}
