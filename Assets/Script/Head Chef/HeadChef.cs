using UnityEngine;
using UnityEngine.AI;

public class HeadChef : MonoBehaviour
{
    private NavMeshAgent _agent;
    private Animator _animator;

    [Header("Patrol Settings")]
    [SerializeField] float _walkRange = 10f;
    [SerializeField] LayerMask _groundLayer;

    private Vector3 _destPoint;
    private bool _walkPointSet;

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
    }

    void Update()
    {
        Patrol();
        UpdateAnimation();
    }

    void Patrol()
    {
        if (!_walkPointSet)
            SearchForDest();

        if (_walkPointSet)
            _agent.SetDestination(_destPoint);

        // Gunakan remainingDistance (lebih akurat)
        if (!_agent.pathPending && _agent.remainingDistance <= _agent.stoppingDistance)
            _walkPointSet = false;
    }

    void UpdateAnimation()
    {
        bool isWalking =
        _agent.hasPath &&
        _agent.remainingDistance > _agent.stoppingDistance;

        _animator.SetBool("isWalking", isWalking);
    }

    void SearchForDest()
    {
        float randomZ = Random.Range(_walkRange, _walkRange);
        float randomX = Random.Range(_walkRange, _walkRange);

        Vector3 randomPoint = new Vector3(
            transform.position.x + randomX,
            transform.position.y + 5f,
            transform.position.z + randomZ
        );

        // Raycast ke bawah + LayerMask BENAR
        if (Physics.Raycast(randomPoint, Vector3.down, out RaycastHit hit, 10f, _groundLayer))
        {
            _destPoint = hit.point;
            _walkPointSet = true;
        }
    }
}
