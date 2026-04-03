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
            // 1. Matikan Grab agar tidak nyangkut/nempel socket
            yield return new WaitForSeconds(0.1f);

            // 2. Reset Physics
            if (_rb != null)
            {
                _rb.velocity = Vector3.zero;
                _rb.angularVelocity = Vector3.zero;
                _rb.isKinematic = true;
            }
            _grabInteractable.enabled = false;


            // 3. Pindah posisi
            transform.SetPositionAndRotation(
                _targetTransform.position,
                _targetTransform.rotation
            );

            // 4. JEDA PENTING: Tunggu 0.1 detik (atau yield return null)
            // Ini memberi waktu Unity update Collider ke posisi baru
            yield return new WaitForSeconds(0.1f);

            // 5. Nyalakan kembali
            _grabInteractable.enabled = true;

            if (_rb != null)
            {
                _rb.isKinematic = false;
            }

            Debug.Log($"{gameObject.name} sudah di posisi baru dan bisa digrab lagi.");
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