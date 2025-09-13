using UnityEngine;

[RequireComponent(typeof(Rigidbody2D), typeof(Collider2D))]
public class Projectile : MonoBehaviour
{
    [Header("Flight")]
    [SerializeField] private float speed = 12f;
    [SerializeField] private float lifeTime = 5f;

    [Header("Damage")]
    [SerializeField] private float damage = 10f;
    [SerializeField] private bool destroyOnHit = true;

    private Vector2 direction = Vector2.right;
    private Rigidbody2D rb;
    private Collider2D col;

    public float Damage => damage;
    public bool DestroyOnHit => destroyOnHit;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        col = GetComponent<Collider2D>();

        rb.bodyType = RigidbodyType2D.Kinematic;
        rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
        col.isTrigger = true;
    }

    void OnEnable()
    {
        if (lifeTime > 0f)
            Invoke(nameof(Despawn), lifeTime);
    }

    void OnDisable()
    {
        CancelInvoke(nameof(Despawn));
    }

    void FixedUpdate()
    {
        rb.MovePosition(rb.position + direction.normalized * speed * Time.fixedDeltaTime);
    }

    // 👇 THIS is what PlayerShooting_Generated is calling
    public void Initialize(Vector2 dir, float dmg)
    {
        direction = dir.normalized;
        damage = dmg;
    }

    void Despawn()
    {
        Destroy(gameObject);
    }
}
