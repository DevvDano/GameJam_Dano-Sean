using UnityEngine;

public class addHP : MonoBehaviour
{
    [SerializeField] private AudioSource pickupSfx;
    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            PlayerHealth playerHealth = collision.gameObject.GetComponent<PlayerHealth>();
            if (playerHealth != null)
            {
                // Set Hp to +1
                playerHealth.AddHealth(1);
                Destroy(gameObject);
                // Play sound effect
                if (pickupSfx != null)
                {
                    pickupSfx.Play();
                }
            }
        }
    }
}
