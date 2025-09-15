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

    // === Double Jump ===
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

    // Tint targets (OPTIONAL: drag specific sprite renderers here)
    [SerializeField] private SpriteRenderer[] tintTargets;

    // ===== GLITCH UPGRADES (unlocked after run completion) =====
    [Header("Glitch Upgrades")]
    [Tooltip("Enable after completing a run to upgrade dash into a phasing 'glitch dash'.")]
    [SerializeField] public bool glitchDashUnlocked = false;

    [Tooltip("Enable after completing a later run to upgrade double jump into a blink teleport (toward mouse). Includes glitch dash too.")]
    [SerializeField] public bool glitchDoubleJumpUnlocked = false;

    [Tooltip("Name of the layer that does NOT collide with world/enemies (configure in Physics 2D matrix).")]
    [SerializeField] private string ghostLayerName = "PlayerGhost";

    // === Glitch Blink (to mouse) ===
    [Header("Glitch Blink (Double Jump)")]
    [Tooltip("Max distance to blink toward the mouse cursor.")]
    [SerializeField] private float blinkMaxDistance = 5f;

    [Tooltip("If the mouse is closer than max distance, blink exactly to it.")]
    [SerializeField] private bool stopAtMouseIfCloser = true;

    [Tooltip("Optional extra vertical boost added to the blink target.")]
    [SerializeField] private float blinkUpBoost = 0f;

    [Tooltip("How fast the RGB glitch cycles (bigger = faster).")]
    [SerializeField] private float rgbCycleSpeed = 10f;

    // ===== AUDIO =====
    [Header("Audio")]
    [SerializeField] private AudioSource sfxSource;   // drag an AudioSource (SFX) here (2D or 3D)
    [Space(4)]
    [SerializeField] private AudioClip dashSfx;           // normal dash
    [SerializeField] private AudioClip dashGlitchSfx;     // glitch dash
    [SerializeField] private AudioClip jumpSfx;           // ground jump (optional)
    [SerializeField] private AudioClip doubleJumpSfx;     // normal air jump
    [SerializeField] private AudioClip glitchBlinkSfx;    // glitch double jump blink
    [SerializeField] private bool logSfxChoices = true;   // debug which clip is chosen

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

    // renderers under graphics for tinting/RGB
    private SpriteRenderer[] spriteRenderers;
    private Color[] originalColors;
    private MaterialPropertyBlock mpb;

    // glitch helpers
    private int originalLayer;
    private int ghostLayer = -1;
    private Coroutine rgbRoutine;

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
            var found = graphics.GetComponentsInChildren<SpriteRenderer>(true);
            if (found != null && found.Length > 0)
                spriteRenderers = found;
            else
            {
                var sr = graphics.GetComponent<SpriteRenderer>();
                if (sr) spriteRenderers = new SpriteRenderer[] { sr };
            }
        }
        else
        {
            spriteRenderers = GetComponentsInChildren<SpriteRenderer>(true);
        }

        if (spriteRenderers != null && spriteRenderers.Length > 0)
        {
            originalColors = new Color[spriteRenderers.Length];
            for (int i = 0; i < spriteRenderers.Length; i++)
                originalColors[i] = spriteRenderers[i].color;
            mpb = new MaterialPropertyBlock();
        }

        originalLayer = gameObject.layer;
        ghostLayer = LayerMask.NameToLayer(ghostLayerName);
        if (ghostLayer == -1)
            Debug.LogWarning($"[Glitch] Layer '{ghostLayerName}' not found. Create it and set collision matrix.");
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
                PlaySfx(jumpSfx);          // ground jump SFX (optional)
                DoJump();
                jumpBufferCounter = 0f;
                return;
            }

            // === Air jump (double jump) ===
            if (!IsGrounded() && airJumpsUsed < maxAirJumps)
            {
                airJumpsUsed++;

                // Glitch double jump (blink) vs normal double jump
                if (glitchDoubleJumpUnlocked)
                {
                    PlayAirJumpOrBlinkSfx(true);
                    StartCoroutine(GlitchBlinkTeleportToMouse());
                    jumpBufferCounter = 0f;
                    return;
                }

                PlayAirJumpOrBlinkSfx(false);
                DoJump();
                jumpBufferCounter = 0f;
                return;
            }
        }
        else if (ctx.canceled)
        {
            // Variable jump height cut
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

        // SFX (hard-gated)
        PlayDashSfxBasedOnState();

        // Glitch dash effects vs normal dash effects
        if (glitchDashUnlocked)
        {
            StartGlitchPhase(true);
            StartRGBEffect(dashDuration + invincibleExtraTime);
            IsInvincible = true;
        }
        else if (invincibleDuringDash)
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

        // small grace window after dash (only for non-glitch dash)
        if (invincibleDuringDash && !glitchDashUnlocked && invincibleExtraTime > 0f)
            yield return new WaitForSeconds(invincibleExtraTime);

        // stop effects
        if (glitchDashUnlocked)
        {
            StartGlitchPhase(false);
            StopRGBEffect();
            IsInvincible = false;
        }
        else if (invincibleDuringDash)
        {
            IsInvincible = false;
            ApplyDashTint(false);
        }

        // cooldown
        yield return new WaitForSeconds(dashCooldown);
        dashReady = true;
    }

    // === Glitch Blink Teleport (DOUBLE JUMP) → toward mouse ===
    private IEnumerator GlitchBlinkTeleportToMouse()
    {
        StartGlitchPhase(true);
        StartRGBEffect(0.12f); // short RGB pop

        Vector3 start = transform.position;
        Vector3 mouseWorld = GetMouseWorld();
        Vector2 toMouse = (mouseWorld - start);
        float dist = toMouse.magnitude;

        if (dist < 0.05f)
            toMouse = new Vector2((facing >= 0 ? 1f : -1f), 0f);

        float travel = stopAtMouseIfCloser ? Mathf.Min(blinkMaxDistance, dist) : blinkMaxDistance;
        Vector3 target = start + (Vector3)(toMouse.normalized * Mathf.Max(0.0f, travel));

        target.y += blinkUpBoost; // optional vertical spice

        transform.position = target;

        if (playerRigidbody.linearVelocity.y < 0f)
            playerRigidbody.linearVelocity = new Vector2(playerRigidbody.linearVelocity.x, 0f);

        yield return new WaitForSeconds(0.05f);

        StartGlitchPhase(false);
        StopRGBEffect();
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

    private Vector3 GetMouseWorld()
    {
        var cam = Camera.main;
        if (!cam) return transform.position;

        Vector3 sp;
#if ENABLE_INPUT_SYSTEM
        sp = Mouse.current != null ? (Vector3)Mouse.current.position.ReadValue()
                                   : (Vector3)Input.mousePosition;
#else
        sp = Input.mousePosition;
#endif
        var world = cam.ScreenToWorldPoint(sp);
        world.z = transform.position.z; // stay on player's z-plane
        return world;
    }

    // ----- Simple dash tint (used only for NON-glitch dash) -----
    private void ApplyDashTint(bool on)
    {
        if (spriteRenderers == null || spriteRenderers.Length == 0) return;

        for (int i = 0; i < spriteRenderers.Length; i++)
        {
            var sr = spriteRenderers[i];
            if (!sr) continue;

            if (on)
            {
                sr.color = dashTint;

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

    // ----- Glitch PHASE toggling (layer swap to ignore collisions) -----
    private void StartGlitchPhase(bool on)
    {
        if (ghostLayer == -1) return; // not configured
        if (on)
        {
            gameObject.layer = ghostLayer;
            // If you have child colliders on different objects, set their layers here too if needed.
        }
        else
        {
            gameObject.layer = originalLayer;
        }
    }

    // ----- Glitch RGB effect -----
    private void StartRGBEffect(float duration)
    {
        StopRGBEffect();
        rgbRoutine = StartCoroutine(RGBPulse(duration));
    }

    private void StopRGBEffect()
    {
        if (rgbRoutine != null) StopCoroutine(rgbRoutine);
        rgbRoutine = null;
        if (spriteRenderers == null) return;
        for (int i = 0; i < spriteRenderers.Length; i++)
        {
            if (!spriteRenderers[i]) continue;
            var restore = (originalColors != null && i < originalColors.Length) ? originalColors[i] : Color.white;
            spriteRenderers[i].color = restore;

            if (mpb == null) mpb = new MaterialPropertyBlock();
            mpb.Clear();
            spriteRenderers[i].SetPropertyBlock(mpb);
        }
    }

    private IEnumerator RGBPulse(float duration)
    {
        if (spriteRenderers == null || spriteRenderers.Length == 0) yield break;

        float t = 0f;
        while (t < duration)
        {
            t += Time.deltaTime;
            float hue = Mathf.Repeat(Time.time * rgbCycleSpeed, 1f);
            Color c = Color.HSVToRGB(hue, 1f, 1f);

            for (int i = 0; i < spriteRenderers.Length; i++)
            {
                var sr = spriteRenderers[i];
                if (!sr) continue;
                sr.color = c;

                if (mpb == null) mpb = new MaterialPropertyBlock();
                sr.GetPropertyBlock(mpb);
                if (sr.sharedMaterial && sr.sharedMaterial.HasProperty("_Color"))
                    mpb.SetColor("_Color", c);
                if (sr.sharedMaterial && sr.sharedMaterial.HasProperty("_BaseColor"))
                    mpb.SetColor("_BaseColor", c);
                sr.SetPropertyBlock(mpb);
            }

            yield return null;
        }
    }

    // ----- Audio helpers -----
    private void PlayDashSfxBasedOnState()
    {
        // If you want glitchDoubleJumpUnlocked to imply glitch dash sound, keep the OR.
        bool glitchedDash = glitchDashUnlocked || glitchDoubleJumpUnlocked;
        AudioClip clip = glitchedDash ? dashGlitchSfx : dashSfx;
        if (logSfxChoices)
            Debug.Log($"[SFX] Dash glitched={glitchedDash}, clip={(clip ? clip.name : "null")}");
        PlaySfx(clip);
    }

    private void PlayAirJumpOrBlinkSfx(bool isGlitchBlink)
    {
        AudioClip clip = isGlitchBlink ? glitchBlinkSfx : doubleJumpSfx;
        if (logSfxChoices)
            Debug.Log($"[SFX] AirJump glitched={isGlitchBlink}, clip={(clip ? clip.name : "null")}");
        PlaySfx(clip);
    }

    private void PlaySfx(AudioClip clip, float volume = 1f)
    {
        if (!clip) return;

        if (sfxSource != null)
        {
            sfxSource.PlayOneShot(clip, volume);
        }
        else
        {
            AudioSource.PlayClipAtPoint(clip, transform.position, volume);
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
