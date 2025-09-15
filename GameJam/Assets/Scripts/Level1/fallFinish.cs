using UnityEngine;
using System.Collections;

public class DisablePlayerCollisionOnTrigger : MonoBehaviour
{
    [SerializeField] private string playerTag = "Player";
    [SerializeField] private float disableDuration = 2f; // time in seconds

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag(playerTag))
        {
            StartCoroutine(DisableCollisionsTemporarily(other));
        }
    }

    private IEnumerator DisableCollisionsTemporarily(Collider2D playerCollider)
    {
        // Get all colliders on the player
        Collider2D[] colliders = playerCollider.GetComponentsInChildren<Collider2D>();

        // Disable them
        foreach (var col in colliders)
            col.enabled = false;

        Debug.Log("Player collisions disabled!");

        // Wait for the duration
        yield return new WaitForSeconds(disableDuration);

        // Re-enable them
        foreach (var col in colliders)
            col.enabled = true;

        Debug.Log("Player collisions re-enabled!");
    }
}
