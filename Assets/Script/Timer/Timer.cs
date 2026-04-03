using UnityEngine;



namespace CookOrPanic.Timer
{

    public class Timer : MonoBehaviour
    {

        private float _currentTime;
        private float _maxTime;
        public bool _isRunning;


        public void StartTimer(float duration)
        {
            _maxTime = duration;
            //_currentTime = 0;
            _isRunning = true;
            Debug.Log($"Timer started! MaxTime: {_maxTime}, CurrentTime: {_currentTime}");
        }


        public void UpdateTimer(float deltaTime)
        {
            if (_isRunning) // Hanya cek _isRunning
            {
                _currentTime += deltaTime;

                if (_currentTime >= _maxTime)
                {
                    _currentTime = _maxTime;
                    _isRunning = false;
                }
            }
        }

        public float GetTimeLeft()
        {
            return _currentTime;

        }

        public void StopTimer()
        {
            //_currentTime = 0;
            _maxTime = 0;
            _isRunning = false;
        }

    }
}
