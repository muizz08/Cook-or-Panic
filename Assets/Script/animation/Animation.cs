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
                    //PopupAnim();
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

    }
}
