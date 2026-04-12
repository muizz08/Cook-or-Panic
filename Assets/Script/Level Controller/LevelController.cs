using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CookOrPanic.LevelController
{
    public class LevelController : MonoBehaviour
    {
        public static LevelController Instance;

        // Tambahkan referensi ScriptableObject untuk testing
        [SerializeField] private List<CookOrPanic.LevelData.LevelData> _levels;
        private int _levelIndex = 0;

        private void Awake()
        {
            if (Instance == null) Instance = this;
            else Destroy(gameObject);
        }

        public void LoadLevel(int index)
        {
            if (index >= 0 && index < _levels.Count)
            {
                _levelIndex = index;
                var currentData = _levels[_levelIndex];

                Debug.Log("<color=cyan>[LevelController]</color> Memuat Level: " + currentData._levelName);

                // KIRIM DATA KE UI AGAR FOTO DAN TEKS BERUBAH
                CookOrPanic.CanvasManager.CanvasManager ui = FindObjectOfType<CookOrPanic.CanvasManager.CanvasManager>();
                if (ui != null)
                {
                    ui.SetupLevelUI(currentData);
                }
            }
        }
        public void NexTLevel()
        {
            _levelIndex++;

            if (_levelIndex < _levels.Count)
            {
                Debug.Log("<color=green><b>[LevelController]</b></color> NAIK LEVEL! Sekarang di Level Index: " + _levelIndex);
                LoadLevel(_levelIndex);
            }
            else
            {
                Debug.Log("<color=yellow><b>[LevelController]</b></color> GAME SELESAI! Tidak ada level lagi di list.");
            }
        }

        // Fungsi pembantu agar Score.cs bisa tahu target skornya
        public int GetTargetScore()
        {
            if (_levels != null && _levels.Count > _levelIndex)
            {
                return _levels[_levelIndex]._targetScore;
            }
            return 20; // Fallback jika list kosong
        }
    }
}