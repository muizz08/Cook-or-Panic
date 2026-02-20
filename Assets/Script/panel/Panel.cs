using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;
using static UnityEngine.GraphicsBuffer;

public class Panel : MonoBehaviour
{
    [SerializeField] private RectTransform panelImage;
    [SerializeField] private CanvasGroup canvasGroup;

    public void ShowPanel()
    {
        Sequence popupSeq = DOTween.Sequence();

        float showDuration = 0.45f;
        //float hideDuration = 0.35f;
        float startScale = 0.85f;
        float pulseScale = 1.03f;


        gameObject.SetActive(true);

        canvasGroup.alpha = 0f;
        panelImage.localScale = Vector3.one * startScale;

        // Muncul (smooth, gak terlalu mantul)
        popupSeq.Append(canvasGroup.DOFade(1f, showDuration).SetEase(Ease.OutQuad));
        popupSeq.Join(panelImage.DOScale(1f, showDuration).SetEase(Ease.OutCubic));

        // Pulse halus (bukan kedap-kedip keras)
        popupSeq.Append(panelImage.DOScale(pulseScale, 0.12f).SetEase(Ease.OutSine));
        popupSeq.Append(panelImage.DOScale(1f, 0.12f).SetEase(Ease.InSine));
    }

    public void HidePanel()
    {
        Sequence popupSeq = DOTween.Sequence();

        //float showDuration = 0.45f;
        float hideDuration = 0.35f;
        float startScale = 0.85f;
        //float pulseScale = 1.03f;

        // Hilang smooth    
        popupSeq.Append(canvasGroup.DOFade(0f, hideDuration).SetEase(Ease.InQuad));
        popupSeq.Join(panelImage.DOScale(startScale, hideDuration).SetEase(Ease.InCubic));

        popupSeq.OnComplete(() =>
        {
            gameObject.SetActive(false);
        });
    }
}
