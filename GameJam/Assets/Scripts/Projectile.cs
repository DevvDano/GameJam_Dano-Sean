using UnityEngine;

[RequireComponent(typeof(Rigidbody2D), typeof(Collider2D))]
public class Projectile : MonoBehaviour
{
    [Header("Flight")]
    [SerializeField] private float speed = 12f;
    [SerializeField] private float lifeTime = 5f;

    [Header("Damage")]
    [SerializeField] private float damage = 10f;   // overridden by Initialize
    [SerializeField] private bool destroyOnHit = true;

    [Header("Contact Behavior")]
    [Tooltip("If set (non-zero), the projectile will only despawn on contact with these layers when it doesn't find a Health script. If left 0, it despawns on ANY contact except other projectiles.")]
    [SerializeField] private LayerMask destroyOnContactLayers = 0;

    private Vector2 direction = Vector2.right;
    private Rigidbody2D rb;
    private Collider2D col;

    // cache projectile layers to avoid friendly-fire despawns if desired
    private int playerProjLayer;
    private int enemyProjLayer;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        col = GetComponent<Collider2D>();

        rb.bodyType = RigidbodyType2D.Kinematic;
        rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
        col.isTrigger = true;

        playerProjLayer = LayerMask.NameToLayer("PlayerProjectile");
        enemyProjLayer = LayerMask.NameToLayer("EnemyProjectile");
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

    // Called by the shooter after Instantiate
    public void Initialize(Vector2 dir, float dmg)
    {
        direction = dir.normalized;
        damage = dmg;
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        // 1) Try damage first
        var playerHealth = other.GetComponentInParent<PlayerHealth>();
        if (playerHealth != null)
        {
            playerHealth.TakeDamage(Mathf.RoundToInt(damage));
            if (destroyOnHit) Despawn();
            return;
        }

        var enemyHealth = other.GetComponentInParent<EnemyHealth>();
        if (enemyHealth != null)
        {
            enemyHealth.TakeDamage(Mathf.RoundToInt(damage));
            if (destroyOnHit) Despawn();
            return;
        }

        var bossHealth = other.GetComponentInParent<BossHealth>();
        if (bossHealth != null)
        {
            bossHealth.TakeDamage(Mathf.RoundToInt(damage));
            if (destroyOnHit) Despawn();
            return;
        }

        // 2) No health component found → handle generic contact
        if (!destroyOnHit) return;

        int otherLayer = other.gameObject.layer;

        // Ignore other projectiles so two bullets touching don't delete each other
        if (otherLayer == playerProjLayer || otherLayer == enemyProjLayer)
            return;

        // If a layer mask is specified, only despawn on those layers
        if (destroyOnContactLayers.value != 0)
        {
            if ((destroyOnContactLayers.value & (1 << otherLayer)) != 0)
            {
                Despawn();
            }
            // else do nothing (not a layer we care about)
        }
        else
        {
            // No mask set → despawn on ANY contact (except other projectiles)
            Despawn();
        }
    }

    void Despawn()
    {
        Destroy(gameObject);
    }
}
