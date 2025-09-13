using UnityEngine;

public class Enemy : MonoBehaviour
{
    //On collision with Projectile, destroy the enemy
    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Projectile"))
        {
            Destroy(gameObject);
            //Play sound effect
            AudioSource audioSource = GetComponent<AudioSource>();
            if (audioSource != null)
            {
                audioSource.Play();
            }
            
        }
    }
}
