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
        [SerializeField] private Transform _doorTarget; // Target lokasi pintu

        [Header("Topi")]
        [SerializeField] private GameObject _hatAtHand;

        [Header("Victory UI")]
        [SerializeField] private GameObject _winPanel;

        [Header("Settings")]
        [SerializeField] private float _walkDuration = 3.0f; // Berapa lama chef jalan ke pintu
        [SerializeField] private float _fadeDuration = 1.0f;
        [SerializeField] private float _delayBeforeWinPanel = 1.5f; // Jeda setelah chef hilang sampai panel muncul

        private void Start()
        {
            // Pastikan panel menang mati di awal
            if (_winPanel != null) _winPanel.SetActive(false);

            TriggerWinSequence();
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
                Rigidbody rb = _hatAtHand.GetComponent<Rigidbody>();
                if (rb != null)
                {
                    rb.isKinematic = true;
                    rb.useGravity = false;
                }
            }
        }

        // --- PANGGIL INI SAAT TOPI DIAMBIL ---
        public void OnHatPickedUp()
        {
            StartCoroutine(AfterGrabRoutine());
        }

        private IEnumerator AfterGrabRoutine()
        {
            if (_headChef != null && _doorTarget != null)
            {
                // 1. Matikan Physics agar tidak bentrok dengan DOMove
                Rigidbody rbChef = _headChef.GetComponent<Rigidbody>();
                if (rbChef != null) rbChef.isKinematic = true;

                // 2. Trigger animasi jalan
                if (_chefAnimator != null)
                {
                    _chefAnimator.SetBool("IsWalking", true);
                    // Pastikan "Apply Root Motion" di Animator OFF jika menggunakan DOMove
                }

                // 3. FIX MELAYANG: Paksa target koordinat Y sama dengan posisi Chef saat ini
                Vector3 floorLevelTarget = new Vector3(
                    _doorTarget.position.x,
                    _headChef.transform.position.y,
                    _doorTarget.position.z
                );

                // 4. Hadap ke pintu (Rotasi)
                _headChef.transform.DOLookAt(floorLevelTarget, 0.5f);

                // 5. Jalan ke pintu (Posisi)
                _headChef.transform.DOMove(floorLevelTarget, _walkDuration).SetEase(Ease.Linear);
            }

            // Tunggu sampai hampir sampai di pintu
            yield return new WaitForSeconds(_walkDuration - _fadeDuration);

            // 6. Efek menghilang (Fade Out)
            Renderer[] allRenderers = _headChef.GetComponentsInChildren<Renderer>();
            foreach (Renderer r in allRenderers)
            {
                // Pastikan Material mendukung transparansi (Mode: Transparent/Fade)
                r.material.DOFade(0, _fadeDuration);
            }

            yield return new WaitForSeconds(_fadeDuration);

            if (_headChef != null) _headChef.SetActive(false);

            // 7. Munculkan Panel Winner
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