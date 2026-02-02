using UnityEngine;
using UnityEngine.AI;

public class HeadChef : MonoBehaviour
{
    private NavMeshAgent agent;
    private Animator animator;

    [Header("Patrol Settings")]
    [SerializeField] float walkRange = 10f;
    [SerializeField] LayerMask groundLayer;

    private Vector3 destPoint;
    private bool walkPointSet;

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        animator = GetComponent<Animator>();

        if (agent == null)
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
        if (!walkPointSet)
            SearchForDest();

        if (walkPointSet)
            agent.SetDestination(destPoint);

        // Gunakan remainingDistance (lebih akurat)
        if (!agent.pathPending && agent.remainingDistance <= agent.stoppingDistance)
            walkPointSet = false;
    }

    void UpdateAnimation()
    {
        bool isWalking =
        agent.hasPath &&
        agent.remainingDistance > agent.stoppingDistance;

        animator.SetBool("isWalking", isWalking);
    }

    void SearchForDest()
    {
        float randomZ = Random.Range(-walkRange, walkRange);
        float randomX = Random.Range(-walkRange, walkRange);

        Vector3 randomPoint = new Vector3(
            transform.position.x + randomX,
            transform.position.y + 5f,
            transform.position.z + randomZ
        );

        // Raycast ke bawah + LayerMask BENAR
        if (Physics.Raycast(randomPoint, Vector3.down, out RaycastHit hit, 10f, groundLayer))
        {
            destPoint = hit.point;
            walkPointSet = true;
        }
    }
}
