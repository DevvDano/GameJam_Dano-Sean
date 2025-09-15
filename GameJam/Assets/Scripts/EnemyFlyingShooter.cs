using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class EnemyFlyingShooter : MonoBehaviour
{
    [Header("Distance Control")]
    [SerializeField] float desiredDistance = 6f;
    [SerializeField] float distanceTolerance = 2f;

    [Header("Speeds")]
    [SerializeField] float baseApproachSpeed = 4f;
    [SerializeField] float maxApproachSpeed = 12f;
    [SerializeField] float approachGain = 1.5f;
    [SerializeField] float retreatSpeed = 7f;
    [SerializeField] float strafeSpeed = 3f;
    [SerializeField] float acceleration = 30f;
    [SerializeField] float maxTurnSpeed = 14f;

    [Header("Hover")]
    [SerializeField] float hoverSpeed = 2.5f;
    [SerializeField] float hoverAmplitude = 0.6f;

    [Header("Shooting")]
    [SerializeField] GameObject projectilePrefab;
    [SerializeField] Transform firePoint;
    [SerializeField] float projectileSpeed = 10f;
    [SerializeField] float fireCooldown = 0.8f;
    [SerializeField] float fireRange = 10f;

    Rigidbody2D rb;
    Transform player;
    PlayerHealth playerHealth;
    bool canAct = false;

    float fireTimer;
    float strafeDir = 1f;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        rb.gravityScale = 0f;
        rb.freezeRotation = true;
        strafeDir = Random.value < 0.5f ? 1f : -1f;
    }

    void OnEnable() { TryAcquirePlayer(); }
    void OnDisable() { Unsubscribe(); }
    void OnDestroy() { Unsubscribe(); }

    void Update()
    {
        // re-acquire if lost (e.g., destroyed on death then respawned)
        if (!player || !playerHealth) TryAcquirePlayer();

        fireTimer -= Time.deltaTime;

        // face player visually
        if (player)
        {
            transform.localScale = (player.position.x >= transform.position.x)
                ? new Vector3(1, 1, 1)
                : new Vector3(-1, 1, 1);
        }
    }

    void FixedUpdate()
    {
        if (!canAct || !player)
        {
            // slow down to a hover (use velocity)
            Vector2 current = rb.linearVelocity;
            Vector2 target = Vector2.zero;
            rb.linearVelocity = Vector2.MoveTowards(current, target, acceleration * Time.fixedDeltaTime);
            return;
        }

        Vector2 pos = rb.position;
        Vector2 toPlayer = (Vector2)player.position - pos;
        float dist = toPlayer.magnitude;
        Vector2 dirTo = dist > 0.0001f ? toPlayer / dist : Vector2.right;

        float inner = Mathf.Max(0.1f, desiredDistance - distanceTolerance);
        float outer = desiredDistance + distanceTolerance;

        // decide desired horizontal velocity
        Vector2 targetVel = Vector2.zero;

        if (dist > outer)
        {
            float error = dist - outer;
            float speed = Mathf.Min(maxApproachSpeed, baseApproachSpeed + approachGain * error);
            targetVel = dirTo * speed;
        }
        else if (dist < inner)
        {
            targetVel = -dirTo * retreatSpeed;
        }
        else
        {
            Vector2 tangent = new Vector2(-dirTo.y, dirTo.x);
            targetVel = tangent * (strafeSpeed * strafeDir);
            if (Random.value < 0.01f) strafeDir *= -1f;
        }

        // add hover on vertical (velocity-form)
        float w = hoverSpeed * 2f * Mathf.PI;
        float hoverVy = hoverAmplitude * w * Mathf.Cos(Time.time * w);

        Vector2 currentVel = rb.linearVelocity;
        Vector2 desiredVel = new Vector2(targetVel.x, hoverVy);
        Vector2 newVel = Vector2.MoveTowards(currentVel, desiredVel, acceleration * Time.fixedDeltaTime);

        if (newVel.sqrMagnitude > maxTurnSpeed * maxTurnSpeed)
            newVel = newVel.normalized * maxTurnSpeed;

        rb.linearVelocity = newVel;

        // shooting
        if (fireTimer <= 0f && dist <= fireRange && projectilePrefab && firePoint)
        {
            fireTimer = fireCooldown;

            var p = Instantiate(projectilePrefab, firePoint.position, firePoint.rotation);
            var rbp = p.GetComponent<Rigidbody2D>();
            if (rbp)
            {
                Vector2 shotDir = ((Vector2)player.position - (Vector2)firePoint.position).normalized;
                rbp.linearVelocity = shotDir * projectileSpeed;
            }
        }
    }

    // -------- player acquire + event hooks ----------
    void TryAcquirePlayer()
    {
        GameObject playerGO = GameObject.FindGameObjectWithTag("Player");
        if (!playerGO)
        {
            Unsubscribe();
            player = null;
            playerHealth = null;
            canAct = false;
            return;
        }

        player = playerGO.transform;

        var ph = playerGO.GetComponent<PlayerHealth>();
        if (ph != playerHealth)
        {
            Unsubscribe();
            playerHealth = ph;
            Subscribe();
        }

        canAct = (playerHealth != null && !playerHealth.IsDead);
    }

    void Subscribe()
    {
        if (playerHealth == null) return;
        playerHealth.OnDied += HandlePlayerDied;
        playerHealth.OnRespawned += HandlePlayerRespawned;
    }

    void Unsubscribe()
    {
        if (playerHealth == null) return;
        playerHealth.OnDied -= HandlePlayerDied;
        playerHealth.OnRespawned -= HandlePlayerRespawned;
    }

    void HandlePlayerDied()
    {
        canAct = false;
        StopAllCoroutines();
    }

    void HandlePlayerRespawned()
    {
        TryAcquirePlayer();
        canAct = (player && playerHealth != null && !playerHealth.IsDead);
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, desiredDistance);
        Gizmos.color = new Color(1f, 0.5f, 0f, 0.6f);
        Gizmos.DrawWireSphere(transform.position, Mathf.Max(0.1f, desiredDistance - distanceTolerance));
        Gizmos.color = new Color(0f, 0.6f, 1f, 0.6f);
        Gizmos.DrawWireSphere(transform.position, desiredDistance + distanceTolerance);

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, fireRange);
    }
}
