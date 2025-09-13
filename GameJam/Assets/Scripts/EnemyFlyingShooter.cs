using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class EnemyFlyingShooter : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] float hoverSpeed = 2f;
    [SerializeField] float hoverAmplitude = 0.5f; // vertical bob
    [SerializeField] float keepDistance = 6f;      // back off if too close
    [SerializeField] float retreatSpeed = 2.5f;

    [Header("Shooting")]
    [SerializeField] GameObject projectilePrefab;   // EnemyProjectile prefab
    [SerializeField] Transform firePoint;
    [SerializeField] float projectileSpeed = 10f;
    [SerializeField] float fireCooldown = 1.4f;

    Rigidbody2D rb;
    Transform player;
    float baseY;
    float fireTimer;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        baseY = transform.position.y;
    }

    void Update()
    {
        if (player == null) player = Targeting.FindPlayer();
        fireTimer -= Time.deltaTime;

        // Hover bob
        float hoverY = baseY + Mathf.Sin(Time.time * hoverSpeed) * hoverAmplitude;

        // Maintain distance
        Vector2 pos = transform.position;
        if (player)
        {
            float dist = Vector2.Distance(pos, player.position);
            if (dist < keepDistance)
            {
                Vector2 dirAway = (pos - (Vector2)player.position).normalized;
                pos += dirAway * (retreatSpeed * Time.deltaTime);
            }

            // Face player (flip X scale)
            if (player.position.x > transform.position.x) transform.localScale = new Vector3(1, 1, 1);
            else transform.localScale = new Vector3(-1, 1, 1);

            // Shoot
            if (fireTimer <= 0f && projectilePrefab && firePoint)
            {
                fireTimer = fireCooldown;
                var p = Instantiate(projectilePrefab, firePoint.position, Quaternion.identity);
                var rbp = p.GetComponent<Rigidbody2D>();
                if (rbp)
                {
                    var dir = Targeting.DirTo(firePoint, player);
                    rbp.linearVelocity = dir * projectileSpeed; // using linearVelocity like your project
                }
            }
        }

        // Apply bobbed position
        rb.MovePosition(new Vector2(pos.x, hoverY));
    }
}
