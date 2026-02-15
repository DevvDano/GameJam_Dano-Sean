using UnityEngine;

public class DestroyOnPlayerHit : MonoBehaviour
{
    [SerializeField] private string playerTag = "Player"; // default tag to check

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag(playerTag))
        {
            Destroy(gameObject); // destroy this object (the one with the script)
        }
    }
}
