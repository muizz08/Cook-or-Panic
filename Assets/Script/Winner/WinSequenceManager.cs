using System.Collections;
using UnityEngine;
using DG.Tweening;

namespace CookOrPanic.WinSequenceManager
{
    public class WinSequenceManager : MonoBehaviour
    {
        [Header("Chef & Animasi")]
        [SerializeField] private GameObject _headChef;
        [SerializeField] private Animator _chefAnimator;
        [SerializeField] private Transform _doorTarget;

        [Header("Topi")]
        [SerializeField] private GameObject _hatAtHand;
        [SerializeField] private Transform _rightHand;

        [Header("Victory UI")]
        [SerializeField] private GameObject _winPanel;

        [Header("Settings")]
        [SerializeField] private float _walkDuration = 3.0f;
        [SerializeField] private float _fadeDuration = 1.0f;
        [SerializeField] private float _delayBeforeWinPanel = 1.5f;
        [SerializeField] private float _idleDuration = 1.5f;

        private bool _isFollowingHand = false;

        private void Start()
        {
            if (_winPanel != null) _winPanel.SetActive(false);

            if (_hatAtHand != null)
                _hatAtHand.SetActive(false);

            TriggerWinSequence();
        }

        private void LateUpdate()
        {
            // 🔥 FOLLOW TANPA SCRIPT TAMBAHAN
            if (_isFollowingHand && _hatAtHand != null && _rightHand != null)
            {
                _hatAtHand.transform.position = _rightHand.position;
                _hatAtHand.transform.rotation = _rightHand.rotation;
            }
        }

        public void TriggerWinSequence()
        {
            StartCoroutine(WinRoutine());
        }

        private IEnumerator WinRoutine()
        {
            yield return new WaitForSeconds(5f);

            if (_chefAnimator != null)
                _chefAnimator.SetTrigger("GiveHat");

            yield return new WaitForSeconds(0.5f);

            if (_hatAtHand != null)
            {
                _hatAtHand.SetActive(true);

                // 🔥 Aktifkan follow manual
                _isFollowingHand = true;

                var grab = _hatAtHand.GetComponent<UnityEngine.XR.Interaction.Toolkit.XRGrabInteractable>();
                if (grab != null)
                {
                    grab.throwOnDetach = false;
                }
            }
        }

        // 🔥 DIPANGGIL SAAT TOPI DIAMBIL
        public void OnHatPickedUp()
        {
            if (_hatAtHand == null) return;

            // 🔥 MATIKAN FOLLOW (INI KUNCI)
            _isFollowingHand = false;

            // 🔥 biar tidak nembus tangan
            _hatAtHand.transform.position += Vector3.up * 0.1f;

            StartCoroutine(AfterGrabRoutine());
        }

        private IEnumerator AfterGrabRoutine()
        {
            if (_chefAnimator != null)
            {
                _chefAnimator.SetTrigger("Idle");
                _chefAnimator.SetBool("isWalking", false);
            }

            yield return new WaitForSeconds(_idleDuration);

            if (_chefAnimator != null)
            {
                _chefAnimator.SetBool("isWalking", true);
            }

            if (_headChef != null && _doorTarget != null)
            {
                Rigidbody rbChef = _headChef.GetComponent<Rigidbody>();
                if (rbChef != null) rbChef.isKinematic = true;

                Vector3 target = new Vector3(
                    _doorTarget.position.x,
                    _headChef.transform.position.y,
                    _doorTarget.position.z
                );

                _headChef.transform.DOLookAt(target, 0.5f);
                _headChef.transform.DOMove(target, _walkDuration).SetEase(Ease.Linear);
            }

            yield return new WaitForSeconds(_walkDuration - _fadeDuration);

            Renderer[] renderers = _headChef.GetComponentsInChildren<Renderer>();
            foreach (Renderer r in renderers)
            {
                r.material.DOFade(0, _fadeDuration);
            }

            yield return new WaitForSeconds(_fadeDuration);

            if (_headChef != null)
                _headChef.SetActive(false);

            yield return new WaitForSeconds(_delayBeforeWinPanel);

            if (_winPanel != null)
            {
                _winPanel.SetActive(true);
                _winPanel.transform.localScale = Vector3.zero;
                _winPanel.transform.DOScale(Vector3.one, 0.6f).SetEase(Ease.OutBack);
            }
        }
    }
}