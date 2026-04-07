using UnityEngine;
using System;

namespace CookOrPanic.Timer
{
    public class Timer : MonoBehaviour
    {
        public enum TimerMode { CountUp, CountDown }

        private float _currentTime;
        private float _maxTime;
        public bool _isRunning;
        private TimerMode _currentMode;

        // Event yang terpanggil saat waktu habis (khusus CountDown)
        public Action OnTimerFinished;

        public void StartTimer(float duration, TimerMode mode = TimerMode.CountUp)
        {
            _currentMode = mode;
            _maxTime = duration;
            _isRunning = true;

            if (_currentMode == TimerMode.CountDown)
            {
                _currentTime = duration; // Mulai dari angka besar ke 0
            }
            else
            {
                _currentTime = 0; // Mulai dari 0 ke atas (untuk masak)
            }

            Debug.Log($"Timer {mode} started! Duration: {duration}");
        }

        public void TimerCooking(float deltaTime)
        {
            if (!_isRunning) return;

            if (_currentMode != TimerMode.CountUp) return;

            _currentTime += deltaTime;

            if (_currentTime >= _maxTime)
            {
                _currentTime = _maxTime;
                _isRunning = false;
            }
        }

        public void TimerBook(float deltaTime)
        {
            if (!_isRunning) return;

            if (_currentMode != TimerMode.CountDown) return;

            _currentTime -= deltaTime;

            if (_currentTime <= 0)
            {
                _currentTime = 0;
                _isRunning = false;
                OnTimerFinished?.Invoke();
                Debug.Log("Timer Buku Habis!");
            }
        }


        public float GetCurrentTime() => _currentTime;

        // Helper untuk UI agar gampang menampilkan format 00:00
        public float GetTimeNormalized() => _maxTime > 0 ? _currentTime / _maxTime : 0;

        public void StopTimer()
        {
            _isRunning = false;
        }
    }
}