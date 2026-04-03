using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

namespace CookOrPanic.Panel
{
    using CookOrPanic.TutorialManager;
    using CookOrPanic.AudioManager;

    public class Panel : MonoBehaviour
    {
        public enum PanelType
        {
            PanelPanduan,
            PanelKlikPanduan,
            PanelResep,
            PanelKlikResep,
            PanelNavigation,
            PanelTriggerLanjut,
            PanelPotong,
        }

        public PanelType _panelType;
        public Image _panelImage;

        [Header("Navigation Settings")]
        public RectTransform _contentRect;
        public int _maxPages = 3;
        private int _currentPage = 0;

        public void Start()
        {
            if (_panelType == PanelType.PanelNavigation)
            {
                NavAnimation();
            }
        }

        public void ToggleWithPartner(Panel partnerPanel)
        {
            // PROTEKSI: Jika lupa narik referensi di Inspector, script tidak akan error/crash
            if (partnerPanel == null)
            {
                Debug.LogWarning($"Partner Panel belum diisi di Inspector objek: {gameObject.name}");
                return;
            }
            if (_panelImage == null)
            {
                Debug.LogWarning($"_panelImage belum diisi di Inspector objek: {gameObject.name}");
                return;
            }

            // Cek status aktif image
            if (_panelImage.gameObject.activeSelf)
            {
                this.HidePanel();
                partnerPanel.ShowPanel();
            }
            else
            {
                this.ShowPanel();
                partnerPanel.HidePanel();
            }
        }

        public void ShowPanel()
        {
            if (_panelImage == null) return;

            _panelImage.gameObject.SetActive(true);
            Canvas.ForceUpdateCanvases();

            // SFX: Buka Buku
            AudioManager.Instance?.PlaySFX("BookAudio");

            // Logika Tutorial
            if (_panelType == PanelType.PanelPanduan)
                TutorialManager.Instance?.OnBookOpened(PanelType.PanelPanduan);
            else if (_panelType == PanelType.PanelResep)
                TutorialManager.Instance?.OnBookOpened(PanelType.PanelResep);

            // Animasi DOTween
            _panelImage.rectTransform.DOKill();
            _panelImage.rectTransform.localScale = Vector3.zero;
            _panelImage.rectTransform.DOScale(1f, 0.4f).SetEase(Ease.OutBack);
        }

        public void HidePanel()
        {
            if (_panelImage == null) return;

            _panelImage.rectTransform.DOKill();

            Sequence seq = DOTween.Sequence();
            seq.Append(_panelImage.rectTransform.DOScale(1.1f, 0.15f).SetEase(Ease.OutQuad));
            seq.Append(_panelImage.rectTransform.DOScale(0f, 0.2f).SetEase(Ease.InBack));
            seq.OnComplete(() =>
            {
                _panelImage.gameObject.SetActive(false);
            });
        }

        public void NextPage()
        {
            if (_currentPage < _maxPages - 1)
            {
                _currentPage++;
                AudioManager.Instance?.PlaySFX("BookAudio"); // SFX Ganti Halaman
                UpdatePanelPosition();
            }
        }

        public void PreviousPage()
        {
            if (_currentPage > 0)
            {
                _currentPage--;
                AudioManager.Instance?.PlaySFX("BookAudio"); // SFX Ganti Halaman
                UpdatePanelPosition();
            }
        }

        private void UpdatePanelPosition()
        {
            if (_contentRect == null) return;

            Canvas.ForceUpdateCanvases();
            float _pageWidth = 2.57f;
            float targetX = -(_currentPage * _pageWidth);

            _contentRect.DOKill();
            _contentRect.DOAnchorPosX(targetX, 0.5f).SetEase(Ease.OutQuad);
        }

        private void NavAnimation()
        {
            if (_panelImage == null) return;
            _panelImage.gameObject.SetActive(true);
            _panelImage.rectTransform.DOKill();
            _panelImage.rectTransform.localScale = Vector3.one;
            _panelImage.rectTransform.DOScale(1.1f, 0.8f).SetEase(Ease.InOutSine).SetLoops(-1, LoopType.Yoyo);
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