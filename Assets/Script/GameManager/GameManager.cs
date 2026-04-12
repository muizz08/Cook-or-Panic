using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CookOrPanic.GameManager
{
    using CookOrPanic.LevelData;
    using CookOrPanic.Score;
    using CookOrPanic.CanvasManager;
    using CookOrPanic.Timer;
    using CookOrPanic.CookingStation;
    using CookOrPanic.HeadChef;
    using CookOrPanic.GameSceneManager;
    public class GameManager : MonoBehaviour
    {
        [Header("Level System")]
        [SerializeField] private LevelData _levelPlayer;
        [SerializeField] private List<LevelData> _allLevels; // List untuk menampung 4 level
        [SerializeField] private int _currentLevelIndex = 0; // Index level (0 = Level 1, 1 = Level 2, dst)
        [SerializeField] private Score _scoreManager;
        [SerializeField] private CanvasManager _canvasManager;
        [SerializeField] private HeadChef _headChef;
        [SerializeField] private Timer _timer;
        [SerializeField] private CookingStation _cookingStation;


        [Header("Panic Settings")]
        [SerializeField] private float _panicThreshold = 0.8f; // 0.8 berarti 80% emosi
        private bool _isPanicActive = false;

        [Header("Feedback Visual")]
        [SerializeField] private Heart.Heart _heartEffect; // Drag & drop objek Heart di Inspector
        private Coroutine _heartRoutine;



        // Tambahkan di dalam class GameManager
        private void Start()
        {
            InitializeLevel();// Cek apakah data level dan canvas manager sudah dipasang di Inspector

        }

        private void OnEnable()
        {
            if (_cookingStation == null)
            {
                Debug.LogError("COOKING STATION NULL!");
                return;
            }

            Debug.Log("SUBSCRIBE EVENT BERHASIL");

            _cookingStation.OnIngredientError += HandleIngredientError;
            HeadChef.OnEmosiChanged += HandleEmosiUpdate;
            _cookingStation.OrderTimeout += HandleOrderTimeout;
        }

        private void OnDisable()
        {
            _cookingStation.OnIngredientError -= HandleIngredientError;
            HeadChef.OnEmosiChanged -= HandleEmosiUpdate;
            _cookingStation.OrderTimeout -= HandleOrderTimeout;
        }

        private void InitializeLevel()
        {
            // Pastikan List tidak kosong dan index tersedia
            if (_allLevels != null && _allLevels.Count > _currentLevelIndex)
            {
                // Ambil data level berdasarkan index
                _levelPlayer = _allLevels[_currentLevelIndex];

                if (_canvasManager != null)
                {
                    _canvasManager.SetupLevelUI(_levelPlayer);

                    // Beri jeda sedikit lalu munculkan pesanan pertama
                    _canvasManager.Invoke("ShowRandomOrder", 0.8f);

                    Debug.Log($"<color=cyan>[GameManager]</color> Memulai: {_levelPlayer.name}");
                }
            }
            else
            {
                Debug.LogError("List Level kosong atau Index di luar jangkauan!");
            }
        }

        void HandleIngredientError(float amount)
        {
            Debug.Log("GAME MANAGER TERIMA ERROR");

            if (_heartEffect == null)
            {
                Debug.LogError("HEART EFFECT NULL!");
                return;
            }

            _headChef.TambahEmosi(amount);

            if (_heartRoutine != null)
            {
                StopCoroutine(_heartRoutine);
            }

            _heartRoutine = StartCoroutine(TriggerHeartEffect(3f));
        }

        private void HandleEmosiUpdate(float currentEmosi)
        {
            RefreshPanicState();

            // CEK GAME OVER: Jika emosi sudah 1 atau lebih (100%)
            if (currentEmosi >= 1f)
            {
                GameOverLogic();
            }
        }

        public void HandleSuccessOrder()
        {
            Debug.Log("<color=yellow>GAME MANAGER:</color> Pesanan Benar! Mengurangi Emosi.");
            if (_headChef != null)
            {
                _headChef.TambahEmosi(-0.2f);
            }
            RefreshPanicState();
        }


        private void RefreshPanicState()
        {
            if (_headChef == null || _heartEffect == null) return;

            float currentEmosi = _headChef.GetIsiEmosi();
            bool shouldPanic = currentEmosi >= _panicThreshold;

            if (shouldPanic != _isPanicActive)
            {
                _isPanicActive = shouldPanic;

                if (shouldPanic)
                {
                    Debug.Log("🔥 PANIC MODE AKTIF");

                    // Stop efek sementara
                    if (_heartRoutine != null)
                    {
                        StopCoroutine(_heartRoutine);
                        _heartRoutine = null;
                    }

                    _heartEffect.gameObject.SetActive(true);
                }
                else
                {
                    Debug.Log("😌 PANIC MODE OFF");
                    _heartEffect.gameObject.SetActive(false);
                }
            }
        }

        // Modifikasi Coroutine lama agar tidak mematikan jantung jika sedang panik
        private IEnumerator TriggerHeartEffect(float duration)
        {
            _heartEffect.gameObject.SetActive(true);

            Debug.Log("❤️ HEART ON");

            yield return new WaitForSeconds(duration);

            if (_headChef != null && _headChef.GetIsiEmosi() < _panicThreshold)
            {
                _heartEffect.gameObject.SetActive(false);
                Debug.Log("💔 HEART OFF");
            }

            _heartRoutine = null;
        }


        // Tambahkan fungsi ini di dalam class GameManager
        public void HandleOrderTimeout(float amount)
        {
            Debug.Log("<color=red>GAME MANAGER:</color> Waktu Pesanan Habis! Menambah Emosi Chef.");

            if (_headChef != null)
            {
                // Tambah emosi chef karena pemain terlalu lambat
                _headChef.TambahEmosi(amount);

                // Cek apakah masuk ke mode panik
                RefreshPanicState();
            }

            // Berikan feedback visual jantung berdebar
            if (_heartEffect != null)
            {
                if (_heartRoutine != null) StopCoroutine(_heartRoutine);
                _heartRoutine = StartCoroutine(TriggerHeartEffect(2f));
            }
        }

        private void GameOverLogic()
        {
            Debug.Log("<color=red>[GAME MANAGER] GAME OVER: Emosi Chef Meledak!</color>");

            // Hentikan semua aktivitas game agar tidak ada error saat transisi
            Time.timeScale = 1f; // Pastikan waktu normal

            // Panggil Scene Manager kamu
            if (GameSceneManager.Instance != null)
            {
                GameSceneManager.Instance.LoadGameOver();
            }
            else
            {
                // Fallback jika Singleton tidak ditemukan
                UnityEngine.SceneManagement.SceneManager.LoadScene("GameOverScene");
            }
        }
    }
}