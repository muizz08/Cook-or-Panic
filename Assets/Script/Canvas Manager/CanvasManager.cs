using System.Collections.Generic;
using DG.Tweening;
using TMPro;
using UnityEngine;


namespace CookOrPanic.CanvasManager
{
    using CookOrPanic.Food;
    using CookOrPanic.Timer;
    using CookOrPanic.CookingStation;
    using CookOrPanic.LevelData;

    public class CanvasManager : MonoBehaviour
    {
        [SerializeField] protected CookingStation _cookingStation;
        // --- PERBAIKAN SCORE: Sprite & Image Dihapus ---
        [Header("Score UI")]
        [SerializeField] protected TMP_Text _scoreText; // Referensi teks skor

        [Header("UI Masak")]
        [SerializeField] private TMP_Text _timerText;
        [SerializeField] protected Timer _timerController;
        // Di dalam CanvasManager.cs

        [Header("Score UI")]
        [SerializeField] protected TMP_Text _totalScoreText; // Ini "Pintu Utama" untuk 

        private Coroutine _activeOrderCoroutine;
        public FoodType CurrentTargetFood { get; protected set; }

        [System.Serializable]
        public struct FoodUIcon
        {
            public FoodType type;
            public Sprite icon;
        }

        [SerializeField] protected List<FoodUIcon> _foodIcons;

        protected virtual void Start()
        {
            if (_timerController != null)
            {
                _timerController.OnOrderFinished += OnOrderExpired;
            }
        }

        protected virtual void Update()
        {
            if (_timerController == null) return;

            if (_timerController.IsCookRunning())
            {
                UpdateUITimerCooking(_timerController.GetCookTime());
            }
        }

        // --- PERBAIKAN FUNGSI SCORE ---
        public virtual void UpdateScoreUI(int score)
        {
            if (_scoreText == null) return;

            // Logika counting effect (angka jalan) menggunakan DOTween
            int currentDisplayedScore;
            if (!int.TryParse(_scoreText.text, out currentDisplayedScore))
            {
                currentDisplayedScore = 0;
            }

            // Animasi angka dari nilai lama ke nilai baru (score)
            DOTween.To(() => currentDisplayedScore, x => currentDisplayedScore = x, score, 0.5f)
                .OnUpdate(() => {
                    _scoreText.text = currentDisplayedScore.ToString();
                });

            // Efek hentakan kecil pada text
            _scoreText.transform.DOPunchScale(Vector3.one * 0.15f, 0.3f);
        }

        // --- SISA FUNGSI TETAP SAMA ---
        public virtual void ShowRandomOrder() { }

        public void UpdateUITimerCooking(float time)
        {
            if (_timerText != null) _timerText.text = time.ToString("F1") + "s";
        }

        public void UpdateUITimerBook(float time)
        {
            _timerController.StartBookTimer(Time.deltaTime);
        }

        public virtual void UpdateAngerBar(float angerValue) { }
        public virtual void StartPanelTimer(float duration) { }
        public virtual void StopBookTimer() { }
        public virtual void StopOrderTimer() { }

        public void StartOrderTimer(float duration)
        {
            if (_activeOrderCoroutine != null) StopCoroutine(_activeOrderCoroutine);
        }

        private void OnOrderExpired()
        {
            if (_cookingStation != null)
            {
                _cookingStation.FailOrderDueToTime();
            }
            Debug.Log("<color=red>Waktu Habis!</color>");
            ShowRandomOrder();

        }

        public virtual void SetupLevelUI(LevelData levelData) { }

        protected void UpdateAvailableRecipes(List<Recipe.Recipe> recipes)
        {
            _foodIcons.Clear();
            foreach (var r in recipes)
            {
                _foodIcons.Add(new FoodUIcon
                {
                    type = r._resultFoodType,
                    icon = r._foodIcon
                });
            }
        }

        public void PanelControl() { }
        public void ShowGameOver() { }
    }
}