using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace CookOrPanic.CanvasManager
{
    using CookOrPanic.Panel;
    public class CanvasManager : MonoBehaviour
    {
        private TMP_Text _scoreText;
        private TMP_Text _timerText;
        private Slider _angerBar;
        private Panel _panel;
      


        public void UpdateScore(int score)
        {
            _scoreText.text = "Score: " + score.ToString();
        }

        public void UpdateTimer(float time)
        {

        }

        public void UpdateAngerBar(float angerValue)
        {

        }


        public void PanelControl()
        {
            _panel._panelImage.DOKill();
            _panel._panelImage.rectTransform.DOKill();
            switch (_panel._panelType)
            {
                case Panel.PanelType.PanelPanduan:
                    _panel._panelImage.transform
                        .DOScale(Vector3.zero, 0.3f)
                        .SetEase(Ease.InBack)
                        .OnComplete(() => _panel._panelImage.gameObject.SetActive(false));
                    break;

                case Panel.PanelType.PanelResep:
                    _panel._panelImage
                        .DOFade(0f, 0.3f)
                        .SetEase(Ease.Linear)
                        .OnComplete(() => _panel._panelImage.gameObject.SetActive(false));
                    break;

                case Panel.PanelType.PanelKlikPanduan:
                    _panel._panelImage.rectTransform
                        .DOAnchorPos(new Vector2(-800, 0), 0.3f)
                        .SetEase(Ease.InCubic)
                        .OnComplete(() => _panel._panelImage.gameObject.SetActive(false));
                    break;

                case Panel.PanelType.PanelKlikResep:
                    _panel._panelImage.rectTransform
                        .DOAnchorPos(new Vector2(800, 0), 0.3f)
                        .SetEase(Ease.InCubic)
                        .OnComplete(() => _panel._panelImage.gameObject.SetActive(false));
                    break;
            }
        }

        public void ShowGameOver()
        {

        }
    }
}