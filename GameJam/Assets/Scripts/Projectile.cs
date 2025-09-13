using UnityEngine;

[RequireComponent(typeof(Rigidbody2D), typeof(Collider2D))]
public class Projectile : MonoBehaviour
{
    [Header("Flight")]
    [SerializeField] private float speed = 12f;
    [SerializeField] private float lifeTime = 5f;

    [Header("Damage")]
    [SerializeField] private float damage = 10f;   // default, can be overridden by Initialize
    [SerializeField] private bool destroyOnHit = true;

    private Vector2 direction = Vector2.right;
    private Rigidbody2D rb;
    private Collider2D col;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        col = GetComponent<Collider2D>();

        rb.bodyType = RigidbodyType2D.Kinematic;                       // modern setup
        rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
        col.isTrigger = true;                                          // so OnTriggerEnter2D fires reliably
    }

    void OnEnable()
    {
        if (lifeTime > 0f) Invoke(nameof(Despawn), lifeTime);
    }

    void OnDisable()
    {
        CancelInvoke(nameof(Despawn));
    }

    void FixedUpdate()
    {
        rb.MovePosition(rb.position + direction.normalized * speed * Time.fixedDeltaTime);
    }

    // Called by PlayerShooting when bullet is spawned
    public void Initialize(Vector2 dir, float dmg)
    {
        direction = dir.normalized;
        damage = dmg;
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        // --- Player ---
        var playerHealth = other.GetComponentInParent<PlayerHealth>();
        if (playerHealth != null)
        {
            playerHealth.TakeDamage(Mathf.RoundToInt(damage));
            if (destroyOnHit) Despawn();
            return;
        }

        // --- Enemy ---
        var enemyHealth = other.GetComponentInParent<EnemyHealth>();
        if (enemyHealth != null)
        {
            enemyHealth.TakeDamage(Mathf.RoundToInt(damage));
            if (destroyOnHit) Despawn();
            return;
        }

        // --- Boss ---
        var bossHealth = other.GetComponentInParent<BossHealth>();
        if (bossHealth != null)
        {
            bossHealth.TakeDamage(Mathf.RoundToInt(damage));
            if (destroyOnHit) Despawn();
            return;
        }
    }

    void Despawn()
    {
        Destroy(gameObject);
    }
}
