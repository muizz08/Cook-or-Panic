using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;



namespace CookOrPanic.Animation
{
    public class Animation : MonoBehaviour
    {
        public enum AnimType
        {
            Navigation,
            Popup,
        }

        [Header("Animation Type")]
        public AnimType animType;

        [Header("Target (Sprite/Quad di lantai)")]
        [SerializeField] private RectTransform target;

        [Header("Canvas Group (Untuk Popup)")]
        [SerializeField] private CanvasGroup canvasGroup;

       

        public void Start()
        {
            Play();
        }
        public void Play()
        {

            switch (animType)
            {
                case AnimType.Navigation:
                    NavAnim();
                    break;

                case AnimType.Popup:
                    PopupAnim();
                    break;
            }

        }

        public void NavAnim()
        {
            float duration = 0.6f;
            float pulseScale = 1.15f;
            Ease ease = Ease.InOutSine;
            Vector3 startScale = target.localScale;

            target.DOScale(startScale * pulseScale, duration)
                .SetEase(ease)
                .SetLoops(-1, LoopType.Yoyo);
        }


        public void PopupAnim()
        {

            Sequence popupSeq = DOTween.Sequence();

            float showDuration = 0.45f;
            //float hideDuration = 0.35f;
            float startScale = 0.85f;
            float pulseScale = 1.03f;
         

            gameObject.SetActive(true);

            canvasGroup.alpha = 0f;
            target.localScale = Vector3.one * startScale;

            // Muncul (smooth, gak terlalu mantul)
            popupSeq.Append(canvasGroup.DOFade(1f, showDuration).SetEase(Ease.OutQuad));
            popupSeq.Join(target.DOScale(1f, showDuration).SetEase(Ease.OutCubic));

            // Pulse halus (bukan kedap-kedip keras)
            popupSeq.Append(target.DOScale(pulseScale, 0.12f).SetEase(Ease.OutSine));
            popupSeq.Append(target.DOScale(1f, 0.12f).SetEase(Ease.InSine));

           
        }

        public void HideAnim()
        {

            Sequence popupSeq = DOTween.Sequence();

            //float showDuration = 0.45f;
            float hideDuration = 0.35f;
            float startScale = 0.85f;
            //float pulseScale = 1.03f;

            // Hilang smooth
            popupSeq.Append(canvasGroup.DOFade(0f, hideDuration).SetEase(Ease.InQuad));
            popupSeq.Join(target.DOScale(startScale, hideDuration).SetEase(Ease.InCubic));

            popupSeq.OnComplete(() =>
            {
                gameObject.SetActive(false);
            });

        }
    }
}
