using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;

namespace CookOrPanic.Panel
{
    using CookOrPanic.AudioManager;
    using CookOrPanic.TutorialManager;
    using CookOrPanic.UIAnimator;

    public class Panel : MonoBehaviour
    {
        public enum PanelType
        {
            PanelPanduan,
            PanelKlikPanduan,
            PanelResep,
            PanelKlikResep,
            PanelTriggerLanjut,
            PanelVideo // 🔥 tambahan

        }
        public static Action OnAnyRecipePanelOpened;
        private static Dictionary<PanelType, Panel> _panelRegistry = new Dictionary<PanelType, Panel>();

        public PanelType _panelType;
        public Image _panelImage;

        [SerializeField] private VideoPlayer _videoPlayer;

        [Header("Navigation Settings")]
        public RectTransform _contentRect;
        public int _maxPages = 3;
        private int _currentPage = 0;

        private Coroutine _autoCloseRoutine;

        [Header("Lock Settings")]
        private bool _isLocked = false;
        private int _openCount = 0; // Tambahkan ini
        private const int MAX_OPEN = 3; // Batas maksimal
        float _lastClickTime;

        [Header("Timer Trigger Settings")]

        private float _pageWidth;

        private void Start()
        {
            if (_contentRect != null && _contentRect.childCount > 0)
            {
                // Ambil lebar dari page asli, bukan viewport
                _pageWidth = ((RectTransform)_contentRect.GetChild(0)).rect.width;
            }
            if (_panelType == PanelType.PanelVideo && _videoPlayer != null)
            {
                // Matikan Play On Awake di Inspector agar tidak tabrakan dengan script
                _videoPlayer.playOnAwake = false;
                StartCoroutine(PlayVideoWithDelay(2f));
            }
        }

        public void ToggleWithPartner(Panel partnerPanel)
        {
            if (Time.time - _lastClickTime < 0.15f) return;
            _lastClickTime = Time.time;

            // Cek apakah sudah benar-benar terkunci
            if (_isLocked) return;

            if (partnerPanel == null || _panelImage == null) return;

            if (_panelImage.gameObject.activeSelf)
            {
                this.HidePanel();
                partnerPanel.ShowPanel();
            }
            else
            {
                // PROSES MEMBUKA
                this.ShowPanel();
                partnerPanel.HidePanel();

                bool isTutorial = TutorialManager.Instance != null && TutorialManager.Instance._isTutorialMode;

                if (_panelType == PanelType.PanelResep && !isTutorial)
                {
                    _openCount++; // Tambah hitungan setiap kali buka

                    // HANYA kunci jika sudah mencapai batas 3
                    if (_openCount >= MAX_OPEN)
                    {
                        _isLocked = true;
                        if (TryGetComponent(out Button btn))
                        {
                            btn.interactable = false;
                        }
                        Debug.Log("<color=red>Jatah buka habis! Tombol dikunci.</color>");
                    }
                    else
                    {
                        Debug.Log($"<color=blue>Resep dibuka ({_openCount}/{MAX_OPEN})</color>");
                    }
                }
            }
        }

        public void ShowPanel()
        {
         
            if (_panelImage == null) return;
            _currentPage = 0;

            if (_contentRect != null)
            {
                _contentRect.anchoredPosition = Vector2.zero;
            }

            // --- GANTI KODE LAMA DENGAN UIANIMATOR ---
            UIAnimator.Show(_panelImage.gameObject, UIAnimator.AnimationType.Scale);

            Canvas.ForceUpdateCanvases();
            AudioManager.Instance?.PlaySFX("BookAudio");

            // Logika Resep & Tutorial
            if (_panelType == PanelType.PanelPanduan)
            {
                TutorialManager.Instance?.OnBookOpened(PanelType.PanelPanduan);
            }
            else if (_panelType == PanelType.PanelResep)
            {
                TutorialManager.Instance?.OnBookOpened(PanelType.PanelResep);
                OnAnyRecipePanelOpened?.Invoke();

                bool isTutorial = TutorialManager.Instance != null && TutorialManager.Instance._isTutorialMode;
                if (!isTutorial)
                {
                    if (_autoCloseRoutine != null) StopCoroutine(_autoCloseRoutine);
                    _autoCloseRoutine = StartCoroutine(CloseAfterDelay(15f));
                    FindObjectOfType<CanvasManager.CanvasManager>()?.StartPanelTimer(15f);
                }
            }
        }



        private void OnVideoFinished(VideoPlayer vp)
        {
            Debug.Log("VIDEO SELESAI!");
            if (_panelType == PanelType.PanelVideo)
            {
                if (_videoPlayer != null)
                {
                    _videoPlayer.loopPointReached -= OnVideoFinished;
                    _videoPlayer.Stop();
                }
                UIAnimator.Hide(_videoPlayer.gameObject, UIAnimator.AnimationType.Scale);
             
            }
           
        }

        public void ForceLockPanel()
        {
            _isLocked = true;
            _openCount = MAX_OPEN; // Set maksimal agar sistem menganggap jatah sudah habis

            if (TryGetComponent(out Button btn))
            {
                btn.interactable = false;
            }

            HidePanel(); // Tutup bukunya
            Debug.Log($"<color=red>Panel {gameObject.name} KUNCI MATI karena timer habis!</color>");
        }

        // Fungsi untuk membuka kunci (Dipanggil saat pesanan baru/reset)
        public void ResetPanelLock()
        {
            _isLocked = false;
            _openCount = 0; // RESET hitungan kembali ke 0
            if (TryGetComponent(out Button btn))
            {
                btn.interactable = true;
            }
            Debug.Log("<color=green>STATUS: Jatah buka resep di-reset.</color>");
        }

        private IEnumerator CloseAfterDelay(float delay)
        {
            yield return new WaitForSeconds(delay);
            HidePanel();
        }

        public void HidePanel()
        {
            if (_panelImage == null) return;

            // 🔥 STOP SEMUA TWEEN SEBELUM HIDE
            if (_contentRect != null)
            {
                _contentRect.DOKill();
            }

            if (_panelType == PanelType.PanelResep)
            {
                // Cari CanvasManager di scene
                var canvasMgr = FindObjectOfType<CanvasManager.CanvasManager>();
                if (canvasMgr != null)
                {
                    // Panggil fungsi untuk menghentikan UI Timer buku
                    // Pastikan fungsi StopTimer atau sejenisnya tersedia di CanvasManager
                    canvasMgr.StopOrderTimer();

                    // Atau jika Anda ingin spesifik menyembunyikan Panel Timer Buku saja:
                    // Kita bisa menggunakan FindObjectOfType untuk mematikan timer spesifik
                    // (Tergantung nama fungsi di CanvasManager Anda)
                }
            }

          

            // --- LOGIKA UNTUK MEMATIKAN TIMER DI CANVAS ---
            if (_panelType == PanelType.PanelResep)
            {
                // Cari script CanvasManager yang ada di scene
                var canvasMgr = FindObjectOfType<CanvasManager.CanvasManager>();
                if (canvasMgr != null)
                {
                    // Panggil fungsi yang baru kita buat di CanvasManager
                    canvasMgr.StopBookTimer();
                }
            }

            UIAnimator.Hide(_panelImage.gameObject, UIAnimator.AnimationType.Scale);

            if (_autoCloseRoutine != null) StopCoroutine(_autoCloseRoutine);
        }

        public void NextPage()
        {
            if (_currentPage < _maxPages - 1)
            {
                _currentPage++;
                AudioManager.Instance?.PlaySFX("BookAudio");

                StartCoroutine(DelayedUpdate());
            }
        }
        public void PreviousPage()
        {
            if (_currentPage > 0)
            {
                _currentPage--;
                AudioManager.Instance?.PlaySFX("BookAudio");

                StartCoroutine(DelayedUpdate());
            }
        }



        IEnumerator DelayedUpdate()
        {
            yield return null; // tunggu 1 frame
            UpdatePanelPosition();
        }

        private void UpdatePanelPosition()
        {
            if (_contentRect == null || !_contentRect.gameObject.activeInHierarchy)
                return;

            Canvas.ForceUpdateCanvases();

            float targetX = -(_currentPage * _pageWidth);

            _contentRect.DOKill();
            _contentRect.DOAnchorPosX(targetX, 0.5f).SetEase(Ease.OutQuad);
        }

        private IEnumerator PlayVideoWithDelay(float delay)
        {
            Debug.Log($"<color=cyan>Menunggu {delay} detik sebelum memutar video...</color>");

            // Pastikan event sudah terpasang
            _videoPlayer.loopPointReached -= OnVideoFinished;
            _videoPlayer.loopPointReached += OnVideoFinished;

            // Siapkan video di background (agar saat delay selesai, video langsung muncul)
            if (!_videoPlayer.isPrepared)
            {
                _videoPlayer.Prepare();
            }

            // Tunggu selama waktu yang ditentukan
            yield return new WaitForSeconds(delay);

            // Putar video
            Debug.Log("<color=green>Delay selesai, memutar video sekarang.</color>");
            _videoPlayer.Play();
        }

        private void OnTriggerEnter(Collider other)
        {
            if (_panelType == PanelType.PanelTriggerLanjut && other.CompareTag("Player"))
            {
                ShowPanel();
            }
        }
    }
}