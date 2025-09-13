using UnityEngine;

public class EnemyProjectile : MonoBehaviour
{
    [SerializeField] int damage = 1;
    [SerializeField] float lifetime = 5f;

    void Awake()
    {
        Destroy(gameObject, lifetime);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            PlayerHealth player = other.GetComponent<PlayerHealth>();
            if (player != null)
            {
                player.TakeDamage(damage);
            }
            Destroy(gameObject);
        }
        else if (!other.isTrigger)
        {
            // hit wall/ground → destroy
            Destroy(gameObject);
        }
    }
}
