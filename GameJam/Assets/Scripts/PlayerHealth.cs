using System.Collections;
using UnityEngine;

public class PlayerHealth : MonoBehaviour
{
    [Header("Health")]
    [SerializeField] int maxHP = 5;
    [SerializeField] float invulnTime = 0.75f;

    [Header("FX (optional)")]
    [SerializeField] Animator animator;
    [SerializeField] AudioSource hurtSfx;
    [SerializeField] AudioSource deathSfx;

    [Header("Visual Flash")]
    [SerializeField] SpriteRenderer spriteRenderer;  // assign main body SR
    [SerializeField] Color flashColor = Color.red;
    [SerializeField] float flashInterval = 0.1f;

    [Header("Death/Respawn")]
    [SerializeField] Transform respawnPoint;     // checkpoint transform
    [SerializeField] bool destroyOnDeath = false;
    [SerializeField] float respawnDelay = 0.5f;  // fallback if no animation event

    [Header("Weapon (destroy on death)")]
    [SerializeField] private GameObject gunRoot;     // drag WeaponPivot or Gun root
    [SerializeField] private bool autoFindGun = true;

    // Public flags/events for AI etc.
    public bool IsDead { get; private set; }
    public event System.Action OnDied;
    public event System.Action OnRespawned;

    // cached
    PlayerMovement movement;
    Rigidbody2D rb;
    Collider2D[] colliders;

    public int hp { get; private set; }
    bool invulnerable;
    bool deathAnimFinished;
    Color originalColor;

    void Awake()
    {
        hp = maxHP;

        if (!animator) animator = GetComponent<Animator>();
        if (!spriteRenderer) spriteRenderer = GetComponentInChildren<SpriteRenderer>();
        if (spriteRenderer) originalColor = spriteRenderer.color;

        movement = GetComponent<PlayerMovement>();
        rb = GetComponent<Rigidbody2D>();
        colliders = GetComponentsInChildren<Collider2D>(true);
    }

    public void TakeDamage(int dmg)
    {
        if (IsDead) return;
        if (invulnerable) return;

        // respect movement i-frames (e.g., during dash)
        if (movement && movement.IsInvincible) return;

        hp -= Mathf.Max(1, dmg);

        if (hp <= 0)
        {
            if (deathSfx) deathSfx.Play();
            if (animator) animator.SetTrigger("Die");
            StartCoroutine(DeathRoutine());
            return;
        }

        if (hurtSfx) hurtSfx.Play();
        if (animator) animator.SetTrigger("Hurt");
        StartCoroutine(Invulnerability());
    }

    IEnumerator Invulnerability()
    {
        invulnerable = true;

        float elapsed = 0f;
        while (elapsed < invulnTime)
        {
            if (spriteRenderer)
            {
                spriteRenderer.color = flashColor;
                yield return new WaitForSeconds(flashInterval);
                spriteRenderer.color = originalColor;
                yield return new WaitForSeconds(flashInterval);
            }
            else yield return null;

            elapsed += flashInterval * 2f;
        }

        if (spriteRenderer) spriteRenderer.color = originalColor;
        invulnerable = false;
    }

    private void DestroyGun()
    {
        if (gunRoot != null)
        {
            Destroy(gunRoot);
            gunRoot = null;
            return;
        }

        if (!autoFindGun) return;

        // Prefer object with GunAiming (usually WeaponPivot)
        var aim = GetComponentInChildren<GunAiming>(true);
        if (aim != null) { Destroy(aim.gameObject); return; }

        // Fallback by name
        var gunChild = transform.Find("Graphics/WeaponPivot/Gun") ?? transform.Find("Gun");
        if (gunChild != null) Destroy(gunChild.gameObject);
    }

    private IEnumerator DeathRoutine()
    {
        // mark dead + broadcast
        IsDead = true;
        OnDied?.Invoke();

        // nuke weapon
        DestroyGun();

        // stop control & physics
        if (movement)
        {
            movement.StopAllCoroutines();
            movement.enabled = false;
        }

        if (rb)
        {
            rb.linearVelocity = Vector2.zero; // <-- fixed
            rb.angularVelocity = 0f;
            rb.simulated = false;
        }

        if (colliders != null)
            foreach (var c in colliders) if (c) c.enabled = false;

        // wait for animation event or fallback timer
        deathAnimFinished = false;
        float t = 0f;
        while (!deathAnimFinished && t < respawnDelay)
        {
            t += Time.unscaledDeltaTime;
            yield return null;
        }

        if (destroyOnDeath)
        {
            Destroy(gameObject);
            yield break;
        }

        // respawn
        yield return StartCoroutine(Respawn());
    }

    // Add this as the LAST frame event on your Death clip (use a relay if Animator is on a child)
    public void OnDeathAnimationFinished()
    {
        deathAnimFinished = true;
    }

    IEnumerator Respawn()
    {
        yield return new WaitForSeconds(0.05f);

        hp = Mathf.Max(1, maxHP);

        if (respawnPoint) transform.position = respawnPoint.position;

        if (rb) rb.simulated = true;
        if (colliders != null)
            foreach (var c in colliders) if (c) c.enabled = true;

        if (movement) movement.enabled = true;

        if (spriteRenderer) spriteRenderer.color = originalColor;

        IsDead = false;
        OnRespawned?.Invoke();
    }

    public void Heal(int amount) => hp = Mathf.Min(maxHP, hp + Mathf.Abs(amount));
    public int CurrentHP => hp;
    public int MaxHP => maxHP;
}
