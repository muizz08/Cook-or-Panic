using CookOrPanic.WinSequenceManager;
using UnityEngine;

public class WinHatTrigger : MonoBehaviour
{
    [SerializeField] private WinSequenceManager _sequenceManager;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            if (_sequenceManager != null)
            {
                _sequenceManager.OnHatPickedUp(); // Sekarang ini tidak akan error
            }
        }
    }
}