using UnityEngine;

public class DashDurationTrigger : MonoBehaviour
{
    [SerializeField] private string playerTag = "Player";
    [SerializeField] private float newDashDuration = 0.18f; // Set in Inspector

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag(playerTag)) return;

        PlayerMovement pm = other.GetComponent<PlayerMovement>();
        if (pm != null)
        {
            pm.SetDashDuration(newDashDuration);
        }

        // Optional: destroy trigger after use
        Destroy(gameObject);
    }
}
