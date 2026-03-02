using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;
using static UnityEngine.GraphicsBuffer;

namespace CookOrPanic.Panel
{
    public class Panel : MonoBehaviour
    {
        public enum PanelType
        {
            PanelPanduan,
            PanelKlikPanduan,
            PanelResep,
            PanelKlikResep,
            PanelNavigation
        }

        public PanelType _panelType;
        public Image _panelImage;
       

        public void Start()
        {

            if (_panelType == PanelType.PanelNavigation)
            {
                NavAnimation();
            }
        }

        public void ToggleWithPartner(Panel partnerPanel)
        {
            if (partnerPanel == null) return;

            // Jika saya sedang AKTIF, maka saya harus MATI dan pasangan harus NYALA
            if (this._panelImage.gameObject.activeSelf)
            {
                this.HidePanel();
                partnerPanel.ShowPanel();
            }
            // Jika saya sedang MATI, maka saya harus NYALA dan pasangan harus MATI
            else
            {
                this.ShowPanel();
                partnerPanel.HidePanel();
            }
        }

        public void ShowPanel()
        {
            _panelImage.gameObject.SetActive(true);

            _panelImage.DOKill();
            _panelImage.rectTransform.DOKill();

            // Start kecil & transparan
            _panelImage.rectTransform.localScale = Vector3.zero;
            _panelImage.color = new Color(1, 1, 1, 0);

            Sequence seq = DOTween.Sequence();

            seq.Append(
                _panelImage.rectTransform
                    .DOScale(1.1f, 0.25f)
                    .SetEase(Ease.OutQuad)
            );

            seq.Append(
                _panelImage.rectTransform
                    .DOScale(1f, 0.15f)
                    .SetEase(Ease.InOutSine)
            );

            seq.Join(
                _panelImage
                    .DOFade(1f, 0.3f)
            );
        }

        public void HidePanel()
        {
            _panelImage.DOKill();
            _panelImage.rectTransform.DOKill();

            Sequence seq = DOTween.Sequence();

            seq.Append(
                _panelImage.rectTransform
                    .DOScale(1.1f, 0.15f)
                    .SetEase(Ease.OutQuad)
            );

            seq.Append(
                _panelImage.rectTransform
                    .DOScale(0f, 0.2f)
                    .SetEase(Ease.InBack)
            );

            seq.Join(
                _panelImage
                    .DOFade(0f, 0.25f)
            );

            seq.OnComplete(() =>
            {
                _panelImage.gameObject.SetActive(false);
            });

        }
        

        public void NavAnimation()
        {
            _panelImage.gameObject.SetActive(true);

            _panelImage.DOKill();
            _panelImage.rectTransform.DOKill();

            _panelImage.rectTransform.localScale = Vector3.one;

            // Loop zoom in - zoom out terus
            _panelImage.rectTransform
                .DOScale(1.1f, 0.8f)
                .SetEase(Ease.InOutSine)
                .SetLoops(-1, LoopType.Yoyo);
        }
    }
}