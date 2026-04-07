using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;
using System;

namespace CookOrPanic.Panel
{
    using CookOrPanic.TutorialManager;
    using CookOrPanic.AudioManager;
    using CookOrPanic.UIAnimator;

    public class Panel : MonoBehaviour
    {
        public enum PanelType
        {
            PanelPanduan,
            PanelKlikPanduan,
            PanelResep,
            PanelKlikResep,
            PanelTriggerLanjut
         
        }
        public static Action OnAnyRecipePanelOpened;
        private static Dictionary<PanelType, Panel> _panelRegistry = new Dictionary<PanelType, Panel>();

        public PanelType _panelType;
        public Image _panelImage;

        [Header("Navigation Settings")]
        public RectTransform _contentRect;
        public int _maxPages = 3;
        private int _currentPage = 0;

        private Coroutine _autoCloseRoutine;

        [Header("Lock Settings")]
        private bool _isLocked = false;
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
        }

        public void ToggleWithPartner(Panel partnerPanel)
        {
            Debug.Log($"Klik: {gameObject.name} | Type: {_panelType} | IsLocked: {_isLocked}");

            if (Time.time - _lastClickTime < 0.15f) return;
            _lastClickTime = Time.time;

            if (_isLocked)
            {
                Debug.Log($"<color=orange>{gameObject.name} LOCKED</color>");
                return;
            }
            // 1. HARD LOCK CHECK
            // Jika panel ini sudah dikunci, jangan biarkan masuk ke logika apa pun
            if (_isLocked)
            {
                Debug.Log($"<color=orange>Panel {gameObject.name} ditolak karena sedang LOCKED!</color>");
                return;
            }

            if (partnerPanel == null || _panelImage == null) return;

            // 2. LOGIKA TOGGLE
            if (_panelImage.gameObject.activeSelf)
            {
                // Jika sedang terbuka, kita tutup
                this.HidePanel();
                partnerPanel.ShowPanel();
            }
            else
            {
                // 3. PROSES MEMBUKA (Ini yang kita kunci)
                this.ShowPanel();
                partnerPanel.HidePanel();

                // Kunci hanya jika BUKAN tutorial
                bool isTutorial = TutorialManager.Instance != null && TutorialManager.Instance._isTutorialMode;
                    
                if (_panelType == PanelType.PanelResep && !isTutorial)
                {
                    _isLocked = true; // Kunci variabel

                    // OPSIONAL: Matikan komponen Button agar secara fisik tidak bisa diklik di UI
                    if (TryGetComponent(out Button btn))
                    {
                        btn.interactable = false;
                    }

                    Debug.Log("<color=red>STATUS: Tombol Resep dikunci!</color>");
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

        // Fungsi untuk membuka kunci (Dipanggil saat pesanan baru/reset)
        public void ResetPanelLock()
        {
            _isLocked = false;
            if (TryGetComponent(out Button btn))
            {
                btn.interactable = true;
            }
            Debug.Log("<color=green>STATUS: Tombol Resep dibuka kembali.</color>");
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

       
        private void OnTriggerEnter(Collider other)
        {
            if (_panelType == PanelType.PanelTriggerLanjut && other.CompareTag("Player"))
            {
                ShowPanel();
            }
        }
    }
}