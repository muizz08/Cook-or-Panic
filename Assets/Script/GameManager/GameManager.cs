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
    public class GameManager : MonoBehaviour
    {
        [SerializeField] private LevelData _levelPlayer;
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
        }

        private void OnDisable()
        {
            _cookingStation.OnIngredientError -= HandleIngredientError;
            HeadChef.OnEmosiChanged -= HandleEmosiUpdate;
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

    }
}