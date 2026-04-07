using DG.Tweening;
using UnityEngine;
using UnityEngine.UIElements;
using static UnityEngine.GraphicsBuffer;

namespace CookOrPanic.UIAnimator
{
    public class UIAnimator : MonoBehaviour
    {
        public enum AnimationType
        {
            Fade,
            Scale,
            PulseFade,
            NavAnimation,
            WarningAnimation,
        }

        [SerializeField] private AnimationType currentAnimation;

        // ================= BASE CLASS =================
        private abstract class UIAnimationBase
        {
            public abstract void Show(GameObject target);
            public abstract void Hide(GameObject target);
        }

        // ================= FADE =================
        private class FadeAnimation : UIAnimationBase
        {
            public override void Show(GameObject target)
            {
                var cg = target.GetComponent<CanvasGroup>();
                if (cg == null) cg = target.AddComponent<CanvasGroup>();

                target.SetActive(true);
                cg.alpha = 0;
                cg.DOFade(1f, 0.3f);
            }

            public override void Hide(GameObject target)
            {
                var cg = target.GetComponent<CanvasGroup>();
                if (cg == null) return;

                cg.DOFade(0f, 0.2f)
                    .OnComplete(() => target.SetActive(false));
            }
        }

        // ================= SCALE =================
        private class ScaleAnimation : UIAnimationBase
        {
            public override void Show(GameObject target)
            {
                if (target == null) return;

                target.SetActive(true);

                CanvasGroup cg = target.GetComponent<CanvasGroup>();
                if (cg == null) cg = target.AddComponent<CanvasGroup>();

                cg.alpha = 0f;
                target.transform.localScale = new Vector3(0.9f, 0.9f, 0.9f);

                cg.DOKill();
                target.transform.DOKill();

                cg.DOFade(1f, 0.25f);
                target.transform.DOScale(1f, 0.25f).SetEase(Ease.OutCubic);
            }

            public override void Hide(GameObject target)
            {
                if (target == null) return;

                CanvasGroup cg = target.GetComponent<CanvasGroup>();
                if (cg == null)
                {
                    target.SetActive(false);
                    return;
                }

                cg.DOKill();
                target.transform.DOKill();

                cg.DOFade(0f, 0.2f);
                target.transform.DOScale(0.9f, 0.2f).SetEase(Ease.InCubic)
                    .OnComplete(() => target.SetActive(false));
            }
        }

        //PULSE FADE
        private class PulseFadeAnimation : UIAnimationBase
        {
            public override void Show(GameObject target)
            {
                var cg = target.GetComponent<CanvasGroup>();
                if (cg == null) cg = target.AddComponent<CanvasGroup>();

                target.SetActive(true);
                cg.DOKill();

                cg.DOFade(1f, 0.5f)
                    .From(0.2f)
                    .SetLoops(-1, LoopType.Yoyo)
                    .SetEase(Ease.InOutSine);
            }

            public override void Hide(GameObject target)
            {
                var cg = target.GetComponent<CanvasGroup>();
                if (cg == null) return;

                cg.DOKill();
                cg.alpha = 1f;
                target.SetActive(false);
            }
        }

        
        private class NavAnimation : UIAnimationBase
        {
            public override void Show(GameObject target)
            {
                RectTransform rt = target.GetComponent<RectTransform>();
                target.gameObject.SetActive(true);
                rt.DOKill();
                rt.localScale = Vector3.one;
                rt.DOScale(1.1f, 0.8f).SetEase(Ease.InOutSine).SetLoops(-1, LoopType.Yoyo);
            }

            public override void Hide(GameObject target)
            {
                target.transform.DOKill(); // Berhentikan looping Yoyo
                target.transform.localScale = Vector3.one; // Reset ke ukuran normal
                target.SetActive(false); // Matikan objek
            }
        }

        private class WarningAnimation : UIAnimationBase
        {
            public override void Show(GameObject target)
            {
                // Ambil atau tambah CanvasGroup agar bisa mengatur Alpha
                var cg = target.GetComponent<CanvasGroup>();
                if (cg == null) cg = target.AddComponent<CanvasGroup>();

                target.SetActive(true);
                cg.DOKill();
                cg.alpha = 0;

                cg.DOFade(1f, 0.5f)
                    .From(0.2f)
                    .SetLoops(-1, LoopType.Yoyo)
                    .SetEase(Ease.InOutSine);
            }

            public override void Hide(GameObject target)
            {
                var cg = target.GetComponent<CanvasGroup>();
                if (cg == null)
                {
                    target.SetActive(false);
                    return;
                }

                cg.DOKill();
                cg.DOFade(0f, 0.3f).OnComplete(() =>
                {
                    target.SetActive(false);
                });
            }
        }

        // ================= INSTANCES =================
        private static UIAnimationBase fade = new FadeAnimation();
        private static UIAnimationBase scale = new ScaleAnimation();
        private static UIAnimationBase pulse = new PulseFadeAnimation();
        private static UIAnimationBase Nav = new NavAnimation();

        private static UIAnimationBase Get(AnimationType type)
        {
            switch (type)
            {
                case AnimationType.Fade: return fade;
                case AnimationType.Scale: return scale;
                case AnimationType.PulseFade: return pulse;
                case AnimationType.NavAnimation: return Nav;
                default: return null;
            }
        }

        public static void Show(GameObject target, AnimationType type)
        {
            Get(type)?.Show(target);
        }

        public static void Hide(GameObject target, AnimationType type)
        {
            Get(type)?.Hide(target);
        }

        public void SetAnimation(AnimationType type)
        {
            currentAnimation = type;
        }
    }
}
