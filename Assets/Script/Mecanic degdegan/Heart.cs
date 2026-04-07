using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using UnityEngine.UI;


namespace CookOrPanic.Heart
{
    public class Heart : MonoBehaviour
    {


        public Image image;
        [Header("Heartbeat Settings")]
        [Range(0f, 1f)] public float minAlpha = 0.25f;
        [Range(0f, 1f)] public float maxAlpha = 0.9f;

        [Header("Timing")]
        public float beatIn = 0.08f;    // naik cepat
        public float beatOut = 0.12f;   // turun lembut
        public float restTime = 0.35f;  // jeda napas

        Sequence heartbeat;

        void Awake()
        {
            image.DOFade(0.8f, 0.2f);
        }

        void OnEnable()
        {
            Play();
        }

    
        void Play()
        {
            heartbeat?.Kill();

            heartbeat = DOTween.Sequence();

            // DUM (kuat)
            heartbeat.Append(image.DOFade(maxAlpha, beatIn).SetEase(Ease.OutQuad));
            heartbeat.Append(image.DOFade(minAlpha, beatOut).SetEase(Ease.InQuad));

            // dum (lebih kecil)
            heartbeat.Append(image.DOFade(maxAlpha * 0.65f, beatIn));
            heartbeat.Append(image.DOFade(minAlpha, beatOut));

            heartbeat.AppendInterval(restTime);


            heartbeat.SetLoops(-1);
            heartbeat.SetUpdate(true); // penting untuk UI / VR
   
        }

        void OnDisable()
        {
            heartbeat?.Kill();
        }

    }
}


