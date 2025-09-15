using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;

public class PlayerMovement : MonoBehaviour
{
    [Header("Refs")]
    [SerializeField] private Rigidbody2D playerRigidbody;
    [SerializeField] private Animator animator;          // assign the Animator on Graphics
    [SerializeField] private Transform graphics;         // assign the Graphics child (visuals root)

    [Header("Movement")]
    [SerializeField] private float moveSpeed = 7f;

    [Header("Jump")]
    [SerializeField] public float jumpForce = 12f;
    [SerializeField] private float coyoteTime = 0.1f;
    [SerializeField] private float jumpBufferTime = 0.1f;

    // === NEW: Double Jump ===
    [SerializeField] private int maxAirJumps = 1;   // 1 = classic double jump (one extra jump in air)
    private int airJumpsUsed = 0;

    [Header("Ground Check")]
    [SerializeField] private Transform groundCheck;
    [SerializeField] private Vector2 groundCheckSize = new Vector2(1f, 0.1f);
    [SerializeField] private LayerMask groundLayer;

    // ===== DASH =====
    [Header("Dash")]
    [SerializeField] private float dashSpeed = 18f;
    [SerializeField] private float dashDuration = 0.18f;
    [SerializeField] private float dashCooldown = 0.6f;
    [SerializeField] private bool allowAirDash = true;   // allowed, but only once per airtime

    // ===== I-FRAMES & TINT DURING DASH =====
    [Header("Dash Invincibility & Tint")]
    [SerializeField] private bool invincibleDuringDash = true;
    [SerializeField] private float invincibleExtraTime = 0.08f; // small grace after dash ends
    [SerializeField] private Color dashTint = new Color(1f, 0.25f, 0.25f, 1f);

    // If set, we’ll tint ONLY these renderers (drag your Graphics' SpriteRenderer here)
    [SerializeField] private SpriteRenderer[] tintTargets;

    private float horizontal;
    private float coyoteCounter;
    private float jumpBufferCounter;

    // dash state
    private bool isDashing;
    private bool dashReady = true;
    private int facing = 1;

    // one-air-dash lock
    private bool hasAirDashed;

    // i-frames flag (public so other scripts can respect it)
    public bool IsInvincible { get; private set; }

    // renderers under graphics for tinting
    private SpriteRenderer[] spriteRenderers;
    private Color[] originalColors;
    private MaterialPropertyBlock mpb;

    void Reset()
    {
        playerRigidbody = GetComponent<Rigidbody2D>();
        if (!animator) animator = GetComponentInChildren<Animator>();
        if (!graphics && animator) graphics = animator.transform;
    }

    void Awake()
    {
        if (!animator) animator = GetComponentInChildren<Animator>();
        if (!graphics && animator) graphics = animator.transform;

        // Prefer manual overrides if provided
        if (tintTargets != null && tintTargets.Length > 0)
        {
            spriteRenderers = tintTargets;
        }
        else if (graphics)
        {
            // Find under graphics (including itself)
            var found = graphics.GetComponentsInChildren<SpriteRenderer>(true);
            if (found != null && found.Length > 0)
                spriteRenderers = found;
            else
            {
                // Fallback: try on graphics directly
                var sr = graphics.GetComponent<SpriteRenderer>();
                if (sr) spriteRenderers = new SpriteRenderer[] { sr };
            }
        }
        else
        {
            // Last resort: search under this object
            spriteRenderers = GetComponentsInChildren<SpriteRenderer>(true);
        }

        if (spriteRenderers != null && spriteRenderers.Length > 0)
        {
            originalColors = new Color[spriteRenderers.Length];
            for (int i = 0; i < spriteRenderers.Length; i++)
                originalColors[i] = spriteRenderers[i].color;
            mpb = new MaterialPropertyBlock();
            Debug.Log($"[DashTint] Found {spriteRenderers.Length} SpriteRenderer(s) to tint.");
        }
        else
        {
            Debug.LogWarning("[DashTint] No SpriteRenderers found. Assign them in 'tintTargets' on PlayerMovement.");
        }
    }

    void Update()
    {
        bool grounded = IsGrounded();

        // reset counters on landing
        if (grounded)
        {
            hasAirDashed = false;
            airJumpsUsed = 0; // reset double-jump counter
        }

        // coyote & buffer
        if (grounded) coyoteCounter = coyoteTime; else coyoteCounter -= Time.deltaTime;

        if (!isDashing)
        {
            if (jumpBufferCounter > 0f)
            {
                // Buffered ground jump only (we don't buffer air jumps)
                if (coyoteCounter > 0f) { DoJump(); jumpBufferCounter = 0f; }
                else jumpBufferCounter -= Time.deltaTime;
            }
        }

        // facing
        if (horizontal > 0.1f) facing = 1;
        else if (horizontal < -0.1f) facing = -1;

        // flip only the graphics child (preserve original scale magnitude)
        if (graphics && !isDashing)
        {
            float xAbs = Mathf.Abs(graphics.localScale.x) > 0f ? Mathf.Abs(graphics.localScale.x) : 1f;
            if (horizontal > 0.1f)
                graphics.localScale = new Vector3(+xAbs, graphics.localScale.y, 1f);
            else if (horizontal < -0.1f)
                graphics.localScale = new Vector3(-xAbs, graphics.localScale.y, 1f);
        }

        // animator params
        if (animator)
        {
            float speed = Mathf.Abs(playerRigidbody.linearVelocity.x);
            float yvel = playerRigidbody.linearVelocity.y;

            animator.SetFloat("Speed", speed);
            animator.SetFloat("YVel", yvel);
            animator.SetBool("IsGrounded", grounded);
            animator.SetBool("IsDashing", isDashing);
        }
    }

    void FixedUpdate()
    {
        if (isDashing) return;
        playerRigidbody.linearVelocity = new Vector2(horizontal * moveSpeed, playerRigidbody.linearVelocity.y);
    }

    // === Input System ===
    public void Move(InputAction.CallbackContext ctx)
    {
        horizontal = ctx.ReadValue<Vector2>().x;
    }

    public void Jump(InputAction.CallbackContext ctx)
    {
        if (isDashing) return;

        if (ctx.performed)
        {
            jumpBufferCounter = jumpBufferTime;

            // Ground/coyote jump
            if (coyoteCounter > 0f)
            {
                animator?.SetTrigger("Jump");
                DoJump();
                jumpBufferCounter = 0f;
                return;
            }

            // === Air jump (double jump) ===
            // Not grounded and out of coyote time → try to consume an air jump
            if (!IsGrounded() && airJumpsUsed < maxAirJumps)
            {
                airJumpsUsed++;
                // Optional: animator?.SetTrigger("DoubleJump"); // only if you add this param
                DoJump();
                jumpBufferCounter = 0f; // consume buffer so we don't double-fire
                return;
            }
        }
        else if (ctx.canceled)
        {
            // Variable jump height cut (works for both ground and air jumps)
            if (playerRigidbody.linearVelocity.y > 0f)
                playerRigidbody.linearVelocity = new Vector2(
                    playerRigidbody.linearVelocity.x,
                    playerRigidbody.linearVelocity.y * 0.6f);
        }
    }

    public void Dash(InputAction.CallbackContext ctx)
    {
        if (!ctx.performed) return;
        if (!dashReady) { Debug.Log("[Dash] cooldown"); return; }

        bool grounded = IsGrounded();

        if (!grounded)
        {
            if (!allowAirDash) { Debug.Log("[Dash] air dash not allowed"); return; }
            if (hasAirDashed) { Debug.Log("[Dash] already air-dashed"); return; }
        }

        Debug.Log("[Dash] start requested");
        StartCoroutine(DashRoutine(facing, grounded));
    }

    private IEnumerator DashRoutine(int dir, bool wasGroundedAtStart)
    {
        dashReady = false;
        isDashing = true;

        if (!wasGroundedAtStart)
            hasAirDashed = true;

        float savedGravity = playerRigidbody.gravityScale;
        playerRigidbody.gravityScale = 0f;

        // I-frames + tint start
        if (invincibleDuringDash)
        {
            IsInvincible = true;
            ApplyDashTint(true);
        }

        float elapsed = 0f;
        float yLock = 0f;

        while (elapsed < dashDuration)
        {
            playerRigidbody.linearVelocity = new Vector2(dir * dashSpeed, yLock);
            elapsed += Time.deltaTime;
            yield return null;
        }

        playerRigidbody.gravityScale = savedGravity;
        isDashing = false;

        // tiny grace window after dash
        if (invincibleDuringDash && invincibleExtraTime > 0f)
            yield return new WaitForSeconds(invincibleExtraTime);

        // I-frames + tint end
        if (invincibleDuringDash)
        {
            IsInvincible = false;
            ApplyDashTint(false);
        }

        // cooldown
        yield return new WaitForSeconds(dashCooldown);
        dashReady = true;

        Debug.Log("[Dash] end");
    }

    // === Helpers ===
    private void DoJump()
    {
        // zero-out current Y so each jump feels snappy and consistent
        playerRigidbody.linearVelocity = new Vector2(playerRigidbody.linearVelocity.x, 0f);
        playerRigidbody.linearVelocity += Vector2.up * jumpForce;
        coyoteCounter = 0f;
    }

    private bool IsGrounded()
    {
        return Physics2D.OverlapCapsule(
            groundCheck.position, groundCheckSize, CapsuleDirection2D.Horizontal, 0f, groundLayer);
    }

    private void ApplyDashTint(bool on)
    {
        if (spriteRenderers == null || spriteRenderers.Length == 0) return;

        for (int i = 0; i < spriteRenderers.Length; i++)
        {
            var sr = spriteRenderers[i];
            if (!sr) continue;

            if (on)
            {
                // 1) classic vertex color
                sr.color = dashTint;

                // 2) MPB for common shader color names
                if (mpb == null) mpb = new MaterialPropertyBlock();
                sr.GetPropertyBlock(mpb);
                if (sr.sharedMaterial && sr.sharedMaterial.HasProperty("_Color"))
                    mpb.SetColor("_Color", dashTint);
                if (sr.sharedMaterial && sr.sharedMaterial.HasProperty("_BaseColor"))
                    mpb.SetColor("_BaseColor", dashTint);
                sr.SetPropertyBlock(mpb);
            }
            else
            {
                var restore = (originalColors != null && i < originalColors.Length) ? originalColors[i] : Color.white;
                sr.color = restore;

                if (mpb == null) mpb = new MaterialPropertyBlock();
                mpb.Clear();
                sr.SetPropertyBlock(mpb);
            }
        }
    }

#if UNITY_EDITOR
    void OnDrawGizmosSelected()
    {
        if (!groundCheck) return;
        Gizmos.color = Color.green;
        Gizmos.DrawWireCube(groundCheck.position, groundCheckSize);
    }
#endif
}
