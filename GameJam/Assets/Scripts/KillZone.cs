using UnityEngine;
public class KillZone : MonoBehaviour
{
    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
            GameEvents.PlayerDied?.Invoke();
    }
}
