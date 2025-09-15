using UnityEngine;

public class addHP : MonoBehaviour
{
    [Header("SFX")]
    [SerializeField] private AudioClip pickupClip;
    [SerializeField] private float pickupVolume = 1f;

    // Use a trigger for simple pickups:
    // - Pickup: Collider2D set to "Is Trigger"
    // - Player: has a Rigidbody2D
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;

        var playerHealth = other.GetComponent<PlayerHealth>();
        if (playerHealth == null) return;

        // Heal by 1 (clamped in PlayerHealth)
        playerHealth.Heal(1);

        // Play sound that won't get cut off when we destroy the pickup
        if (pickupClip) AudioSource.PlayClipAtPoint(pickupClip, transform.position, pickupVolume);

        // Remove the pickup
        Destroy(gameObject);
    }

    // If you prefer collisions instead of triggers, uncomment this and
    // remove OnTriggerEnter2D. Make sure both colliders are NOT triggers,
    // and one of the two objects has a Rigidbody2D.
    /*
    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (!collision.gameObject.CompareTag("Player")) return;

        var playerHealth = collision.gameObject.GetComponent<PlayerHealth>();
        if (playerHealth == null) return;

        playerHealth.Heal(1);
        if (pickupClip) AudioSource.PlayClipAtPoint(pickupClip, transform.position, pickupVolume);
        Destroy(gameObject);
    }
    */
}
