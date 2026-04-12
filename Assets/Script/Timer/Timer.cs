using UnityEngine;
using System;

namespace CookOrPanic.Timer
{
    public class Timer : MonoBehaviour
    {
        // Variabel Data
        private float _cookTime, _orderTime, _bookTime;
        private float _maxOrder, _maxBook;

        // Status Jalannya Timer
        private bool _isCookRunning, _isOrderRunning, _isBookRunning;

        // Event Finish
        public Action OnOrderFinished;
        public Action OnBookFinished;

        // Tambahkan ini agar script lain (seperti CookingStation) bisa ngecek status timer
        public bool IsCookRunning()
        {
            return _isCookRunning;
        }

        private void Update()
        {
            float delta = Time.deltaTime;

            if (_isCookRunning) _cookTime += delta;

            if (_isOrderRunning)
            {
                _orderTime -= delta;
                if (_orderTime <= 0)
                {
                    _orderTime = 0;
                    StopOrderTimer(); // Mematikan status running
                    OnOrderFinished?.Invoke();
                }
            }

            if (_isBookRunning)
            {
                _bookTime -= delta;
                if (_bookTime <= 0)
                {
                    _bookTime = 0;
                    StopBookTimer(); // Mematikan status running
                    OnBookFinished?.Invoke();
                }
            }
        }

        // --- 1. FUNGSI MASAK (Count Up) ---
        public void StartCookingTimer() 
        { 
            _isCookRunning = true;
        }
        public void StopCookingTimer() => _isCookRunning = false;
        public float GetCookTime() => _cookTime;

        // --- 2. FUNGSI PESANAN (Count Down) ---
        public void StartOrderTimer(float duration)
        {
            _maxOrder = duration;
            _orderTime = duration;
            _isOrderRunning = true;
        }
        public void StopOrderTimer() => _isOrderRunning = false;
        public float GetOrderRatio() => _maxOrder > 0 ? _orderTime / _maxOrder : 0;

        // --- 3. FUNGSI BUKU (Count Down) ---
        public void StartBookTimer(float duration)
        {
            _maxBook = duration;
            _bookTime = duration;
            _isBookRunning = true;
        }
        public void StopBookTimer() => _isBookRunning = false;
        public float GetBookRatio() => _maxBook > 0 ? _bookTime / _maxBook : 0;
        // Tambahkan ini di Timer.cs
        public bool IsBookRunning() => _isBookRunning;

        public float GetBookTimeRemaining() => _bookTime;



        // --- TAMBAHAN: RESET SEMUA ---
        public void ResetAllTimers()
        {
            _isCookRunning = _isOrderRunning = _isBookRunning = false;
            _cookTime = _orderTime = _bookTime = 0;
        }
    }
}