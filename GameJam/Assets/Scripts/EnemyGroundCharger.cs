using System.Collections;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D), typeof(Collider2D))]
public class EnemyGroundCharger : MonoBehaviour
{
    [Header("Patrol")]
    [SerializeField] float patrolSpeed = 2f;

    [Header("Ground & Walls")]
    [SerializeField] LayerMask groundMask;
    [SerializeField] float wallCheckDistance = 0.30f;   // forward ray
    [SerializeField] float ledgeCheckDistance = 0.60f;  // down ray
    [SerializeField] float skin = 0.04f;                // ray start offset outside collider
    [SerializeField] float groundedRadius = 0.08f;      // small circle at feet

    [Header("Turn Smoothing")]
    [SerializeField] float turnCheckHold = 0.10f;       // how long we must see a wall/ledge
    [SerializeField] float flipCooldown = 0.28f;        // lockout to stop ping-pong

    [Header("Aggro & Charge")]
    [SerializeField] float aggroRange = 6f;
    [SerializeField] float windupTime = 0.35f;
    [SerializeField] float chargeSpeed = 12f;
    [SerializeField] float chargeDuration = 0.35f;
    [SerializeField] float cooldown = 1.0f;

    Rigidbody2D rb;
    Collider2D col;
    Transform player;

    bool movingRight = true;
    bool charging;
    bool cooling;
    float dir = 1f;

    // anti-jitter timers
    float flipCooldownTimer;
    float turnHoldTimer;

    // safety
    bool warnedNoGroundMask;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        col = GetComponent<Collider2D>();

        // Encourage sensible Rigidbody2D setup
        rb.freezeRotation = true;
        rb.interpolation = RigidbodyInterpolation2D.Interpolate;
    }

    void Update()
    {
        // Always try to have a player reference
        if (!player)
        {
            player = Targeting.FindPlayer();
            if (!player)
            {
                GameObject p = GameObject.FindGameObjectWithTag("Player");
                if (p) player = p.transform;
            }
        }

        if (flipCooldownTimer > 0f) flipCooldownTimer -= Time.deltaTime;

        if (charging || cooling) return;

        // Decide to charge
        if (player && Vector2.Distance(transform.position, player.position) <= aggroRange)
        {
            StartCoroutine(ChargeRoutine());
            return;
        }

        Patrol();
    }

    void Patrol()
    {
        dir = movingRight ? 1f : -1f;

        // Don�t attempt fancy turning logic if we�re not grounded (falling / stepping down)
        bool grounded = IsGrounded();
        bool shouldTurn = false;

        if (HasValidGroundMask())
        {
            // Only look for ledges when grounded, otherwise we�ll flip mid-air
            if (grounded)
            {
                shouldTurn = IsWallAhead() || IsLedgeAhead();
            }
        }
        else
        {
            // We�ll still move, but won�t turn because no mask is set.
            if (!warnedNoGroundMask)
            {
                Debug.LogWarning($"[{name}] EnemyGroundCharger: groundMask is not set. " +
                                 "Ledge/Wall checks disabled. Assign your Ground layer.", this);
                warnedNoGroundMask = true;
            }
        }

        // Debounce + cooldown to prevent micro-flips
        if (shouldTurn) turnHoldTimer += Time.deltaTime; else turnHoldTimer = 0f;
        if (turnHoldTimer >= turnCheckHold && flipCooldownTimer <= 0f)
        {
            Flip();
            flipCooldownTimer = flipCooldown;
            turnHoldTimer = 0f;
        }

        // Move
        rb.linearVelocity = new Vector2(dir * patrolSpeed, rb.linearVelocity.y);

        // Face direction
        transform.localScale = new Vector3(Mathf.Sign(dir), 1f, 1f);
    }

    IEnumerator ChargeRoutine()
    {
        charging = true;

        // Face the player
        if (player) movingRight = player.position.x > transform.position.x;
        dir = movingRight ? 1f : -1f;
        transform.localScale = new Vector3(Mathf.Sign(dir), 1f, 1f);

        // Wind-up (stop briefly)
        rb.linearVelocity = new Vector2(0f, rb.linearVelocity.y);
        yield return new WaitForSeconds(windupTime);

        float t = 0f;
        while (t < chargeDuration)
        {
            // Abort if about to hit a wall or fall, but only if we have a valid mask
            if (HasValidGroundMask() && (IsWallAhead() || IsLedgeAhead()))
                break;

            rb.linearVelocity = new Vector2(dir * chargeSpeed, rb.linearVelocity.y);
            t += Time.deltaTime;
            yield return null;
        }

        // Stop and cool down
        rb.linearVelocity = new Vector2(0f, rb.linearVelocity.y);
        charging = false;
        cooling = true;

        // If we ended the charge at a boundary, flip once (with cooldown)
        if (HasValidGroundMask() && (IsWallAhead() || IsLedgeAhead()))
        {
            Flip();
            flipCooldownTimer = flipCooldown;
            turnHoldTimer = 0f;
        }

        yield return new WaitForSeconds(cooldown);
        cooling = false;
    }

    void Flip()
    {
        movingRight = !movingRight;
        dir = movingRight ? 1f : -1f;
        transform.localScale = new Vector3(Mathf.Sign(dir), 1f, 1f);
    }

    // ----------------- Helpers -----------------
    bool HasValidGroundMask() => groundMask.value != 0;

    Vector2 BoundsCenter() => (Vector2)col.bounds.center;
    float ExtX() => col.bounds.extents.x;
    float MinY() => col.bounds.min.y;

    Vector2 FeetPos() => new Vector2(BoundsCenter().x, MinY() + groundedRadius * 0.25f);

    bool IsGrounded()
    {
        if (!HasValidGroundMask()) return true; // don�t block movement if mask not set
        return Physics2D.OverlapCircle(FeetPos(), groundedRadius, groundMask) != null;
    }

    // Cast your collider shape slightly forward; if we hit a surface whose normal faces us, it's a wall.
    bool IsWallAhead()
    {
        if (!HasValidGroundMask()) return false;

        // How far to sweep the collider forward this frame
        float sweep = Mathf.Max(wallCheckDistance, Mathf.Abs(rb.linearVelocity.x) * Time.deltaTime + 0.02f);

        ContactFilter2D filter = new ContactFilter2D
        {
            useTriggers = false,
            useLayerMask = true,
            layerMask = groundMask
        };

        // Reuse a small buffer (alloc-free) for hits
        RaycastHit2D[] results = new RaycastHit2D[4];
        int count = rb.Cast(new Vector2(dir, 0f), filter, results, sweep);

        for (int i = 0; i < count; i++)
        {
            // If the surface normal faces against our travel dir, it's a blocking wall
            if (results[i].normal.x * dir < -0.5f)
                return true;
        }
        return false;
    }

    // (Optional but handy) Flip if we collide head-on (covers corner cases)
    void OnCollisionEnter2D(Collision2D c)
    {
        if (!HasValidGroundMask()) return;
        if (((1 << c.collider.gameObject.layer) & groundMask) == 0) return;

        // Check if any contact is a head-on hit
        foreach (var contact in c.contacts)
        {
            if (contact.normal.x * dir < -0.5f)
            {
                // Debounce with existing timers if you have them; otherwise just flip
                Flip();
                break;
            }
        }
    }


    bool IsLedgeAhead()
    {
        Vector2 front = new Vector2(BoundsCenter().x + dir * (ExtX() + skin), MinY() + skin);
        RaycastHit2D hit = Physics2D.Raycast(front, Vector2.down, ledgeCheckDistance, groundMask);
        return hit.collider == null; // no ground under front foot
    }

#if UNITY_EDITOR
    void OnDrawGizmosSelected()
    {
        if (!col) col = GetComponent<Collider2D>();
        if (!col) return;

        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, aggroRange);

        // Grounded probe
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(FeetPos(), groundedRadius);

        // Rays
        float d = movingRight ? 1f : -1f;
        Gizmos.color = Color.magenta; // wall
        Vector2 wallOrigin = (Vector2)col.bounds.center + new Vector2(d * (col.bounds.extents.x + skin * 0.5f), 0f);
        Gizmos.DrawLine(wallOrigin, wallOrigin + new Vector2(d, 0f) * wallCheckDistance);

        Gizmos.color = Color.yellow; // ledge
        Vector2 ledgeOrigin = new Vector2(col.bounds.center.x + d * (col.bounds.extents.x + skin), col.bounds.min.y + skin);
        Gizmos.DrawLine(ledgeOrigin, ledgeOrigin + Vector2.down * ledgeCheckDistance);
    }
#endif
}
