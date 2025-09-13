using UnityEngine;

public class Projectile : MonoBehaviour
{
    [SerializeField] int damage = 1;
    [SerializeField] float lifetime = 5f; // auto-cleanup if it misses

    void Awake()
    {
        Destroy(gameObject, lifetime);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        // Player
        var player = other.GetComponent<PlayerHealth>();
        if (player != null)
        {
            player.TakeDamage(damage);
            Destroy(gameObject);
            return;
        }

        //// Enemy
        //var enemy = other.GetComponent<EnemyHealth>();
        //if (enemy != null)
        //{
        //    enemy.TakeDamage(damage);
        //    Destroy(gameObject);
        //    return;
        //}

        // Boss
        var boss = other.GetComponent<BossHealth>();
        if (boss != null)
        {
            boss.TakeDamage(damage);
            Destroy(gameObject);
            return;
        }

        // If it hits anything else, destroy the object
        Destroy(gameObject);
    }
}
