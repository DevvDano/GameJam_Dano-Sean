using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class EnemyFlyingShooter : MonoBehaviour
{
    [Header("Distance Control")]
    [SerializeField] float desiredDistance = 6f;     // ideal distance from player
    [SerializeField] float distanceTolerance = 2f;   // +/- band around desiredDistance

    [Header("Speeds")]
    [SerializeField] float baseApproachSpeed = 4f;   // min approach speed when just outside band
    [SerializeField] float maxApproachSpeed = 12f;   // cap when very far
    [SerializeField] float approachGain = 1.5f;      // extra speed per unit distance beyond band
    [SerializeField] float retreatSpeed = 7f;        // speed when too close
    [SerializeField] float strafeSpeed = 3f;         // sideways orbit when in band
    [SerializeField] float acceleration = 30f;       // how fast we reach targetVel
    [SerializeField] float maxTurnSpeed = 14f;       // clamp final velocity

    [Header("Hover")]
    [SerializeField] float hoverSpeed = 2.5f;        // Hz-ish
    [SerializeField] float hoverAmplitude = 0.6f;    // units

    [Header("Shooting")]
    [SerializeField] GameObject projectilePrefab;    // EnemyProjectile (has Collider2D + RB2D)
    [SerializeField] Transform firePoint;            // +X points out of barrel
    [SerializeField] float projectileSpeed = 10f;
    [SerializeField] float fireCooldown = 0.8f;
    [SerializeField] float fireRange = 10f;

    Rigidbody2D rb;
    Transform player;
    float baseY;
    float fireTimer;
    float strafeDir = 1f;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        rb.gravityScale = 0f;
        rb.freezeRotation = true;
        baseY = transform.position.y;
        strafeDir = Random.value < 0.5f ? 1f : -1f;
    }

    void Update()
    {
        if (!player) player = Targeting.FindPlayer(); // your helper
        fireTimer -= Time.deltaTime;

        // Face player (flip visuals only)
        if (player)
        {
            transform.localScale = (player.position.x >= transform.position.x)
                ? new Vector3(1, 1, 1)
                : new Vector3(-1, 1, 1);
        }
    }

    void FixedUpdate()
    {
        if (!player) return;

        Vector2 pos = rb.position;
        Vector2 toPlayer = (Vector2)player.position - pos;
        float dist = toPlayer.magnitude;
        Vector2 dirTo = dist > 0.0001f ? toPlayer / dist : Vector2.right;

        float inner = Mathf.Max(0.1f, desiredDistance - distanceTolerance);
        float outer = desiredDistance + distanceTolerance;

        // --- Choose desired horizontal velocity ---
        Vector2 targetVel = Vector2.zero;

        if (dist > outer)
        {
            // Too far → approach with distance-scaled speed
            float error = dist - outer; // how far beyond the band
            float speed = Mathf.Min(maxApproachSpeed, baseApproachSpeed + approachGain * error);
            targetVel = dirTo * speed;
        }
        else if (dist < inner)
        {
            // Too close → retreat
            targetVel = -dirTo * retreatSpeed;
        }
        else
        {
            // In comfort band → strafe (orbit)
            Vector2 tangent = new Vector2(-dirTo.y, dirTo.x); // 90° turn
            targetVel = tangent * (strafeSpeed * strafeDir);

            // occasionally flip strafe direction
            if (Random.value < 0.01f) strafeDir *= -1f;
        }

        // --- Add hover on vertical axis (velocity form) ---
        // y(t) = baseY + A*sin(w*t) → dy/dt = A*w*cos(w*t)
        float w = hoverSpeed * 2f * Mathf.PI; // angular frequency
        float hoverVy = hoverAmplitude * w * Mathf.Cos(Time.time * w);

        // Smoothly steer toward target horizontal velocity
        Vector2 current = rb.linearVelocity;
        Vector2 desired = new Vector2(targetVel.x, hoverVy);
        Vector2 newVel = Vector2.MoveTowards(current, desired, acceleration * Time.fixedDeltaTime);

        // Clamp overall speed to avoid runaway
        if (newVel.sqrMagnitude > maxTurnSpeed * maxTurnSpeed)
            newVel = newVel.normalized * maxTurnSpeed;

        rb.linearVelocity = newVel;

        // --- Shooting ---
        if (fireTimer <= 0f && dist <= fireRange && projectilePrefab && firePoint)
        {
            fireTimer = fireCooldown;

            var p = Instantiate(projectilePrefab, firePoint.position, firePoint.rotation);
            var rbp = p.GetComponent<Rigidbody2D>();
            if (rbp)
            {
                Vector2 shotDir = ((Vector2)player.position - (Vector2)firePoint.position).normalized;
                // Use linearVelocity if that's what your project uses; else velocity:
#if UNITY_2023_2_OR_NEWER
                // Some templates expose linearVelocity; if not, fall back to velocity.
                try { rbp.linearVelocity = shotDir * projectileSpeed; }
                catch { rbp.linearVelocity = shotDir * projectileSpeed; }
#else
                rbp.velocity = shotDir * projectileSpeed;
#endif
            }
        }
    }

    void OnDrawGizmosSelected()
    {
        // Distance band
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, desiredDistance);
        Gizmos.color = new Color(1f, 0.5f, 0f, 0.6f);
        Gizmos.DrawWireSphere(transform.position, Mathf.Max(0.1f, desiredDistance - distanceTolerance));
        Gizmos.color = new Color(0f, 0.6f, 1f, 0.6f);
        Gizmos.DrawWireSphere(transform.position, desiredDistance + distanceTolerance);

        // Fire range
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, fireRange);
    }
}
