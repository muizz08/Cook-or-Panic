using UnityEngine.AI;
using System;
using System.Collections;

namespace CookOrPanic.HeadChef
{
    using UnityEngine;
    using CookOrPanic.AudioManager; 

    public class HeadChef : MonoBehaviour
    {
        public static Action<float> OnEmosiChanged;
        private NavMeshAgent _agent;
        private Animator _animator;

        [Header("Patrol Settings")]
        [SerializeField] float _walkRange = 10f;
        [SerializeField] float _minDistance = 2f;
        [SerializeField] LayerMask _groundLayer;
        private Vector3 _destPoint;
        private bool _walkPointSet;

        [Header("Emosi Settings")]
        [SerializeField] private RectTransform _ikonChef;
        [SerializeField] private float _emosiSpeed = 10f;
        [SerializeField] private float _batasKiri = -2.7f; // Nilai disesuaikan dengan Inspector VR Anda
        [SerializeField] private float _batasKanan = 2.1f; // Nilai disesuaikan dengan Inspector VR Anda
        private float _isiEmosi = 0f;

        [SerializeField] private float _startDelay = 3f;
        private bool _canMove = false;

        // --- TAMBAHAN: FUNGSI UNTUK MENGAMBIL NILAI EMOSI ---
        public float GetIsiEmosi()
        {
            return _isiEmosi;
        }

        void Start()
        {
            _agent = GetComponent<NavMeshAgent>();
            _animator = GetComponent<Animator>();

            if (_agent == null)
            {
                Debug.LogError("NavMeshAgent TIDAK ditemukan di " + gameObject.name);
                enabled = false;
                return;
            }

            // ❗ STOP dulu
            _agent.isStopped = true;

            StartCoroutine(StartDelayRoutine());
        }

        void Update()
        {
            // ❗ kalau belum waktunya jalan, skip patrol
            if (_canMove)
            {
                Patrol();
                UpdateAnimation();
            }

            // Emosi tetap jalan
            HandleEmosiUI();
        }

        private IEnumerator StartDelayRoutine()
        {
            Debug.Log("Chef diam dulu...");

            yield return new WaitForSeconds(_startDelay);

            _canMove = true;
            _agent.isStopped = false;

            Debug.Log("Chef mulai bergerak!");
        }

        private void HandleEmosiUI()
        {
            if (_ikonChef == null) return;

            float targetX = Mathf.Lerp(_batasKiri, _batasKanan, _isiEmosi);

            // Gunakan localPosition agar sumbu Z tetap terjaga di World Space Canvas VR
            Vector3 targetLocalPos = new Vector3(targetX, _ikonChef.localPosition.y, _ikonChef.localPosition.z);

            _ikonChef.localPosition = Vector3.Lerp(_ikonChef.localPosition, targetLocalPos, Time.deltaTime * _emosiSpeed);
        }

        public void TambahEmosi(float jumlah)
        {
            _isiEmosi = Mathf.Clamp(_isiEmosi + jumlah, 0, 1);

            OnEmosiChanged?.Invoke(_isiEmosi);

            if (_isiEmosi >= 1f)
            {
                Debug.Log("Chef Sangat Marah! Game Over?");
            }
        }

        // --- LOGIKA PATROL ---
        void Patrol()
        {
            if (!_walkPointSet) SearchForDest();

            if (_walkPointSet) _agent.SetDestination(_destPoint);

            if (_walkPointSet && !_agent.pathPending && _agent.remainingDistance <= _agent.stoppingDistance)
            {
                _walkPointSet = false;
            }
        }

        void UpdateAnimation()
        {
            bool isWalking = _agent.velocity.magnitude > 0.1f;
            AudioManager.Instance.PlaySFX("Step");
            _animator.SetBool("isWalking", isWalking);
        }

        void SearchForDest()
        {
            for (int i = 0; i < 10; i++)
            {
                float randomZ = Random.Range(-_walkRange, _walkRange);
                float randomX = Random.Range(-_walkRange, _walkRange);

                Vector3 randomPoint = new Vector3(
                    transform.position.x + randomX,
                    transform.position.y + 5f,
                    transform.position.z + randomZ
                );

                if (Physics.Raycast(randomPoint, Vector3.down, out RaycastHit hit, 20f, _groundLayer))
                {
                    if (Vector3.Distance(transform.position, hit.point) >= _minDistance)
                    {
                        _destPoint = hit.point;
                        _walkPointSet = true;
                        return;
                    }
                }
            }
        }
    }

}
