using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using System.Collections; 


namespace CookOrPanic.SeasoningPour
{
    using CookOrPanic.ProcessedIngredient;
    using CookOrPanic.Ingredient;
    using CookOrPanic.AudioManager; 
    public class SeasoningPour : ProcessedIngredient
    {
        [Header("Particle Settings")]
        [SerializeField] private ParticleSystem _particle;
        [SerializeField] private Transform _pourDirection;
        [SerializeField] private Transform _targetTransform;

        public IngredientType MyType => _ingredientType;

        private Rigidbody _rb;
        private XRGrabInteractable _grabInteractable;
        private bool _isPouring;
       

        private void Start()
        {
            if (_particle != null)
                _particle.Stop();
            PlaySeasoningSound(false);

            _rb = GetComponent<Rigidbody>();
            _grabInteractable = GetComponent<XRGrabInteractable>();

            if (_grabInteractable != null)
            {
                _grabInteractable.selectExited.AddListener(OnReleased);
            }

        }

        private void Update()
        {
            CheckTilt();
        }

        private void CheckTilt()
        {

            // Gunakan transform.up jika poros hijau adalah bagian atas botol
            float angle = Vector3.Angle(_pourDirection.forward, Vector3.down);

            if (angle < 65f)
            {
                StartPour();
            }
            else
            {
                StopPour();
            }
        }

        private void StartPour()
        {
            if (_isPouring) return;

            _isPouring = true;

            // Panggil sound hanya SATU KALI saat transisi dari diam ke menuang
            PlaySeasoningSound(true);

            if (_particle != null && !_particle.isPlaying)
                _particle.Play();
           
        }

        private void StopPour()
        {
            _isPouring = false;

            if (_particle != null && _particle.isPlaying)
                _particle.Stop();
            PlaySeasoningSound(false);
        }

        public bool IsPouring()
        {
            return _isPouring;
        }

        private void PlaySeasoningSound(bool isStarting)
        {
            string soundName = "";
            // Sesuaikan string nama sound dengan yang ada di AudioManager kamu
            switch (_ingredientType)
            {
                case IngredientType.PenyedapRasa:
                    soundName = "Penyedap";
                    break;
                case IngredientType.Merica:
                    soundName = "Merica";
                    break;
              
            }

            // Jalankan aksinya: Play atau Stop
            if (isStarting)
            {
                AudioManager.Instance.PlaySFX(soundName);
            }
            else
            {
                // Pastikan AudioManager kamu punya fungsi StopSFX
                AudioManager.Instance.StopSFX(soundName);
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