using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using System.Collections; // Dibutuhkan untuk Coroutine

namespace CookOrPanic.ReturnToSender
{
    public class ReturnToSender : MonoBehaviour
    {
        private Rigidbody _rb;
        private XRGrabInteractable _grabInteractable;

        [SerializeField] private Transform _targetTransform;

        void Start()
        {
            _rb = GetComponent<Rigidbody>();
            _grabInteractable = GetComponent<XRGrabInteractable>();

            if (_grabInteractable != null)
            {
                _grabInteractable.selectExited.AddListener(OnReleased);
            }
        }

        private void OnReleased(SelectExitEventArgs args)
        {
            // Panggil Coroutine, bukan fungsi biasa
            StartCoroutine(TeleportProcess());
        }

        // Method publik ini juga diubah agar menjalankan Coroutine
        public void ReturnToInitialTransform()
        {
            StartCoroutine(TeleportProcess());
        }

        private IEnumerator TeleportProcess()
        {
            yield return new WaitForSeconds(0.1f);
            // 1. Langsung matikan agar tidak terbaca lagi oleh Socket Panci
            _grabInteractable.enabled = false;

            if (_rb != null)
            {
                _rb.isKinematic = true;
              
            }

            // 2. PINDAH INSTAN (Tanpa tunggu 0.5 detik di awal)
            transform.SetPositionAndRotation(
                _targetTransform.position,
                _targetTransform.rotation
            );

            // 3. JEDA SINGKAT (Hanya untuk sinkronisasi physics Unity)
            yield return new WaitForSeconds(0.1f);

            // 4. Aktifkan kembali
            _grabInteractable.enabled = true;

            if (_rb != null)
            {
                _rb.isKinematic = false;
            }

            Debug.Log(gameObject.name + " sudah kembali ke meja.");
        }

        private void OnDestroy()
        {
            if (_grabInteractable != null)
            {
                _grabInteractable.selectExited.RemoveListener(OnReleased);
            }
        }
    }
}