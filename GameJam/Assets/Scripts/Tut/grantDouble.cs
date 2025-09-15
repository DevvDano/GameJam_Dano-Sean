using UnityEngine;

public class AirJumpPowerup : MonoBehaviour
{
    [SerializeField] private string playerTag = "Player";

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag(playerTag))
        {
            PlayerMovement pm = other.GetComponent<PlayerMovement>();
            if (pm != null)
            {
                pm.SendMessage("SetMaxAirJumps", 1, SendMessageOptions.DontRequireReceiver);
            }

            // optionally destroy the powerup after use
            Destroy(gameObject);
        }
    }
}
